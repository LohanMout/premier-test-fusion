using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System;

public class gestionnaireReseau : MonoBehaviour, INetworkRunnerCallbacks
{
    //Contient une référence au component NetworkRunner
    NetworkRunner _runner;
    // pour mémoriser le component GestionnaireMouvementPersonnage du joueur
    gestionnaireInputs gestionnaireInputs;
    //Index de la scène du jeu
    public int IndexSceneJeu;
    // Contient la référence au script JoueurReseau du Prefab
    public joueurReseau joueurPrefab;
    // Tableau de couleurs à définir dans l'inspecteur
    public Color[] couleurJoueurs;
    // Pour compteur le nombre de joueurs connectés
    public int nbJoueurs = 0;
    public SphereCollision sphereCollision; 
    // référence au prefab de la boule rouge
    public bool spheresDejaSpawn; 
    // Permet de savoir les boules ont déjà été créées.

    void Start()
    {
        // Création d'une partie dès le départ
        //CreationPartie(GameMode.AutoHostOrClient);
    }

    /* Fonction exécuté sur le serveur seulement qui spawn le nombre de boules rouges déterminés au lancement
   d'une nouvelle partie.
   */


    public void CreationBouleRouge()
    {
        if (_runner.IsServer && !spheresDejaSpawn)
        {
            GameManager.partieEnCours = true;
            for (int i = 0; i < GameManager.instance.nbBoulesRougesDepart; i++)
            {
                _runner.Spawn(sphereCollision, utilitaires.GetPositionSpawnAleatoire(), Quaternion.identity);
            }
            spheresDejaSpawn = true;
        }
    }

    // Fonction asynchrone pour démarrer Fusion et créer une partie
    public async void CreationPartie(GameMode mode)
    {
        /*  1.Mémorisation du component NetworkRunner . On garde en mémoire
            la référence à ce component dans la variable _runner.
            2.Indique au NetworkRunner qu'il doit fournir les entrées (inputs) au 
            simulateur (Fusion)
        */
        _runner = gameObject.GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        /*Méthode du NetworkRunner qui permet d'initialiser une partie
         * GameMode : reçu en argument. Valeur possible : Client, Host, Server,
           AutoHostOrClient, etc.)
         * SessionName : Nom de la chambre (room) pour cette partie
         * Scene : la scène qui doit être utilisée pour la simulation
         * SceneManager : référence au component script
          NetworkSceneManagerDefault qui est ajouté au même moment
         */
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "Chambre test",
            Scene = SceneRef.FromIndex(IndexSceneJeu),
            PlayerCount = 10, //ici, on limite à 10 joueurs
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
       
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }
    /* Fonction du Runner pour définir les inputs du client dans la simulation
     * 1. On récupère le component GestionnaireInputs du joueur local
     * 2. On définit (set) le paramètre input en lui donnant la structure de données (struc) qu'on récupère
     * en appelant la fonction GestInputReseau du script GestionnaireInputs. Les valeurs seront mémorisées
     * et nous pourrons les utilisées pour le déplacement du joueur dans un autre script. Ouf...*/

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //1.
        if (gestionnaireInputs == null && joueurReseau.Local != null)
        {

            gestionnaireInputs = joueurReseau.Local.GetComponent<gestionnaireInputs>();
        }

        //2.
        if (gestionnaireInputs != null)
        {
            input.Set(gestionnaireInputs.GetInputReseau());
        }

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
       
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (_runner.IsServer)
        {
            Debug.Log("Un joueur s'est connecté comme serveur. Spawn d'un joueur");
            /*On garde la référence au nouveau joueur créé par le serveur. La variable locale
             créée est de type JoueurReseau (nom du script qui contient la fonction Spawned()*/
            joueurReseau leNouveuJoueur = _runner.Spawn(joueurPrefab, utilitaires.GetPositionSpawnAleatoire(),Quaternion.identity, player);
            /*On change la variable maCouleur du nouveauJoueur et on augmente le nombre de joueurs connectés
            Comme j'ai seulement 10 couleurs de définies, je m'assure de ne pas dépasser la longueur de mon
            tableau*/
            Debug.Log(nbJoueurs);
            leNouveuJoueur.maCouleur = couleurJoueurs[nbJoueurs];
            nbJoueurs++;
            if (nbJoueurs >= 10) nbJoueurs = 0;
        }
        else
        {
            Debug.Log("Un joueur s'est connecté comme client. Spawn d'un joueur");
        }
}

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
       
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        /*
         * Fonction appelée lorsqu'une connexion réseau est refusée ou lorsqu'un client perd
        * la connexion suite à une erreur réseau. Le paramètre ShutdownReason est une énumération (enum)
        * contenant différentes causes possibles.
         */
        if (shutdownReason == ShutdownReason.GameIsFull)
        {
            Debug.Log("Le maximum de joueur est atteint. Réessayer plus tard.");
        }

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
}
