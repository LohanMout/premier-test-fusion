using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion; // namespace pour utiliser les classes de Fusion
/* 
 * 1.Les objets r�seau ne doivent pas d�river de MonoBehavior, mais bien de NetworkBehavior
 * Importation de l'interface IPlayerLeft
 * 2.Variable pour m�moriser l'instance du joueur
 * 3.Fonction Spawned() : Semblable au Start(), mais pour les objets r�seaux
 * Sera ex�cut� lorsque le personnage sera cr�� (spawn)
 * Test si le personnage cr�� est le personnage contr�l� par l'utilisateur local.
 * HasInputAuthority permet de v�rifier cela.
 * Retourne true si on est sur le client qui a g�n�r� la cr�ation du joueur
 * Retourne false pour les autres clients
 * 4. Lorsqu'un joueur se d�connecte du r�seau, on �limine (Despawn) son joueur.
 */
public class joueurReseau : NetworkBehaviour, IPlayerLeft, IDespawned //1.
{
    //Variable qui sera automatiquement synchronis�e par le serveur sur tous les clients
    [Networked] public Color maCouleur { get; set; }
    // Variable pour le pointage (nombre de boules rouge) du joueur qui sera automatiquement synchronis� par le serveur sur tous les clients
    // Lorsqu'un chanegement est d�tect�, la fonction OnChangementPointage sera automatiquement appel�e pour faire
    // une mise � jour de l'affichage du texte.
    [Networked, OnChangedRender(nameof(OnChangementPointage))] public int nbBoulesRouges { get; set; }
    //Variable r�seau (Networked) contenant le nom du joueur (sera synchronis�e)
    [Networked] public string monNom { get; set; }
    //Lorsqu'un joueur est pr�t pour une nouvelle partie (il appuy� sur R), on met cette variable � true
    // ce qui d�clenchera l'appel de la fonction OnPretAReprendre sur tous les clients connect�s.
    [Networked, OnChangedRender(nameof(OnPretAReprendre))] public bool pretNouvellePartie { get; set; }
    public static int nbClientsPret; // Pour compteur le nombre de joueurs qui sont pr�ts � reprendre

    // R�f�rence au script GestionnaireInput (pour savoir si touche R a �t� enfonc�e)
    gestionnaireInputs gestionnaireInputs;


    // Variable pour m�moriser la zone de texte au dessus de la t�te du joueur et qui afficher le pointage
    // Cette variable doit �tre d�finie dans l'inspecteur de Unity
    public TextMeshProUGUI affichagePointageJoueur;
    public static joueurReseau Local; //.2
    public Transform modeleJoueur;

    /*
     * Au d�part, on change la couleur du joueur. La variable maCouleur sera d�finie
     * par le serveur dans le script GestionnaireReseau.La fonction Start() sera appel�e apr�s la fonction Spawned().
     */
    private void Start()
    {
        GetComponentInChildren<MeshRenderer>().material.color = maCouleur;
        gestionnaireInputs = GetComponent<gestionnaireInputs>(); //On r�cup�re le component GestionnaireInput

    }

    /* Le joueur v�rifie si la partie est termin�e et qu'il n'est pas d�j� pr�t � reprendre. Si c'est le cas:
   - On va chercher les dernier input et on v�rifie si pretARejouer = true. Ce sera le cas si le joueur � appuy�
   sur la touche R.
   - Si le joueur est pr�t � rejouer :
   - On met la variable r�seau PretNouvelle partie � true. La variable est synchronis�e sur tous les clients
   qui appeleront la fonction OnPretAReprendre du JoueurReseau. On remet les variables pretARecommencer du
   gestionneInput � false et pretArejouer du donneesInputReseau � false �galement.
   */
    public override void FixedUpdateNetwork()
    {
        if (!GameManager.partieEnCours && !pretNouvellePartie)
        {
            GetInput(out donneesInputReseau donneesInputReseau);
            if (donneesInputReseau.pretARejouer)
            {
                pretNouvellePartie = true;
                gestionnaireInputs.pretARecommencer = false;
                donneesInputReseau.pretARejouer = false;
            }
        }
    }

    public override void Spawned() //3.
    {
        // � sa cr�ation, le joueur ajoute sa r�f�rence (son script JoueurReseau) et son pointage (var nbBoulesRouges) au dictionnaire
        // du GameManager.
        GameManager.joueursPointagesData.Add(this, nbBoulesRouges);

        if (Object.HasInputAuthority)
        {
            Local = this;
            Debug.Log("Un joueur local a �t� cr��");
            /*� la cr�ation du joueur et s'il est le joueur local (HasInputAuthority), ont doit d�f�nir son nom en allant
          chercher la variable nomJoueurLocal du GameManager.
          Pour que le nom soit synchronis� sur tous les clients, appelle d'une fonction RPC (RemoteProcedureCall) qui permet
          de dire � tous les clients d'ex�cuter la fonction  "RPC_ChangementdeNom"
          */
            monNom = GameManager.nomJoueurLocal;
            RPC_ChangementdeNom(monNom);

            //Si c'est le joueur du client, on appel la fonction pour le rendre invisible
            utilitaires.SetRenderLayerInChildren(modeleJoueur, LayerMask.NameToLayer("joueurLocal"));

            //On d�sactive la mainCamera. Assurez-vous que la cam�ra de d�part poss�de bien le tag MainCamera
            Camera.main.gameObject.SetActive(false);
        }
        else
        {
            //Si le joueur cr�� est contr�l� par un autre joueur, on d�sactive le component cam�ra de cet objet
            Camera camLocale = GetComponentInChildren<Camera>();
            camLocale.enabled = false;

            // On d�sactive aussi le component AudioListener
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;
            Debug.Log("Un joueur r�seau a �t� cr��");
        }
        // on affiche le nom du joueur cr�� et son pointage
        affichagePointageJoueur.text = $"{monNom}:{nbBoulesRouges.ToString()}";

        /* Au moment o� un joueur est cr�e, on v�rifie s'il est seul. Si c'est le cas, on appelle la fonction
        AfficheAttenteAutreJoueur du GameManager. S'il y a plus d'un joueur, on appelle la fonction qui permet
        de cr�er les boules rouges. Notez bien que la fonction NouvellesBoulesRouges sera appel�e uniquement
        par le serveur (Runner.IsServer)
        */
        if (Runner.SessionInfo.PlayerCount == 1)
        {
            GameManager.instance.AfficheAttenteAutreJoueur(true);
        }
        else if (Runner.SessionInfo.PlayerCount > 1)
        {
            GameManager.instance.AfficheAttenteAutreJoueur(false);
            if (Runner.IsServer) GameManager.instance.NouvellesBoulesRouges();
        }
    }

    public void PlayerLeft(PlayerRef player) //.4
    {
        if (player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }

    /* Fonction appel�e automatiquement lorsqu'un changement est d�tect� dans la variable nbBoulesRouges du joueur (variable Networked)
    Mise � jour du pointage du joueur qui sera �gal au nombre de boules rouges ramass�es
*/
    public void OnChangementPointage()
    {
        affichagePointageJoueur.text = $"{monNom}:{nbBoulesRouges.ToString()}";

        // On v�rifie si le nombre de boules rouge == l'objectif de points � atteindre
        // Si oui, on appelle la fonction FinPartie en passant le nom du joueur gagnant.
        // Cette fonction sera appel�e dans le script du gagnant, sur tous les clients connect�s
        if (nbBoulesRouges >= GameManager.instance.objectifPoints)
        {
            GameManager.instance.FinPartie(monNom);
        }
    }

    /* Fonction RPC (RemoteProcedureCall) d�clench� par un joueur local qui permet la mise � jour du nom du joueur
    sur tous les autres clients. La source (l'�metteur) est le joueur local (RpcSources.InputAuthority). La cible est tous les joueurs
    connect�s (RpcTargets.All). Le param�tre re�u contient le nom du joueur � d�f�nir.
    Pour bien comprendre : Mathieu se connecte au serveur en inscrivant son nom. Il envoir un message � tous les autres clients. Sur
    chaque client, le joueur contr�l� par Mathieu ex�cutera cette fonction ce qui permettra une mise � jour du nom.
    1. On d�finit la variable nomNom
    2. On affiche le nom et le poitage au dessus de la t�te du joueur.
    */
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_ChangementdeNom(string leNom, RpcInfo infos = default)
    {
        //1.
        monNom = leNom;
        //2.
        affichagePointageJoueur.text = $"{monNom}:{nbBoulesRouges.ToString()}";
    }

    /* Fonction appel�e par le GameManager lorsque tous les joueurs sont pr�ts et qu'il faut relancer
  une nouvelle partie.
  1. R�initialisation des diff�rentes variables
  2. On s'assure que le joueur n'est pas seul. S'il l'est, on affiche le paneau d'attente d'un autre joueur
  et on renvoie true au GameMananger pour ne pas que des boules soit cr��es.
  3. Si on se rend ici, c'est que le joueur n'est pas seul. On retoure alors false au GameManager.
 */
    public bool Recommence()
    {
        //1.
        GetComponent<gestionnaireInputs>().pretARecommencer = false;
        nbBoulesRouges = 0;
        pretNouvellePartie = false;
        nbClientsPret = 0;
        //2.
        if (Runner.SessionInfo.PlayerCount == 1)
        {
            GameManager.instance.AfficheAttenteAutreJoueur(true);
            return true;
        }
        //3.
        return false;
    }

    /* Fonction appel�e automatiquement sur tous les clients lorsque la variable pretNouvellePartie est modif�e.
   - Le reste du code s'ex�cute seulement sur le serveur (if(Runner.IsServer))
   - On ajoute 1 � la variable nbClientsPret;
   - On r�cup�re le nombre de joueurs connect�s;
   - Si tout le monde est pr�te, on appelle la fonction RPC_OnNouvellePartie() qui est un Remote Procedure Call
   */
    public void OnPretAReprendre()
    {
        if (Runner.IsServer)
        {
            nbClientsPret++;
            int nbJoueursTotal = Runner.SessionInfo.PlayerCount;
            if (nbClientsPret >= nbJoueursTotal)
            {
                RPC_OnNouvellePartie();
            }
        }
    }

    /* Fonction RPC (remote procedure call) qui sera ex�cut� par tous les clients (RpcTargets.All))
   Tous les joueurs connect�s ex�cuteront ainsi la fonction du GameManager DebutNouvellePartie
   */
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_OnNouvellePartie(RpcInfo infos = default)
    {
        GameManager.instance.DebutNouvellePartie();
    }

    // Fonction ex�cut�e lorsqu'un JoueurReseau est despawned. Soit lorsqu'il quitte volontairement ou
    // encore quand la connection au serveur est interrompue pour une autre raison.
    // Quand cela se produit, on s'assure de mettre � jour notre dictionnaire JoueursPointagesData en
    // supprimant la r�f�rence au joueur d�connect�.
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GameManager.joueursPointagesData.Remove(this);
    }
}

