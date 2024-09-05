using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Référence à l'instance du GameManager
    public int objectifPoints = 10; // Nombre de point pour finir la partie
    public static bool partieEnCours = true;  // Est que la partie est en cours (static)
    [SerializeField] gestionnaireReseau gestionnaireReseau; // Reférence au gestionnaire réseau
    public static string nomJoueurLocal; // Le nom du joueur local
    public static Dictionary<joueurReseau, int> joueursPointagesData = new Dictionary<joueurReseau, int>();
    //Dictionnaire pour mémoriser chaque JoueurReseau et son pointage. Au moment de la création d'un joueur (fonction Spawned() du joueur)
    // il ajoutera lui même sa référence au dictionnaire du GameManager.
    // Liste static vide de type JoueurReseau qui servira à garder en mémoire tous les
    // joueurs connectés. Sera utilisé entre 2 parties pour gérer la reprise.
    public static List<joueurReseau> lstJoueurReseau = new List<joueurReseau>();

    [Header("Éléments UI")]
    public GameObject refCanvasDepart; // Référence au canvas de départ
    public GameObject refCanvasJeu; // Référence au canvas de jeu
    public TextMeshProUGUI refTxtNomJoueur; // Référence à la zone texte contenant le nom du joueur (dans CanvasDepart)
    public TextMeshProUGUI refTxtPointage; // Référence à la zone d'affichage de tous les pointages (dans CanvasJeu)
    public GameObject refPanelGagnant; // Référence au panel affichant le texte du gagnant.
    public TextMeshProUGUI refTxtGagnant; // Référence à la zone de texte pour afficher le nom du gagnant.
    public GameObject refPanelAttente; // Référence au panel affichant le d'attente entre deux partie.



    // Au départ, on définit la variable "instance" qui permettra au autre script de communiquer avec l'instance du GameManager.
    void Awake()
    {
        instance = this;
    }

    /* Affichage du pointage des différents joueurs connectés à la partie.
   1. Si la partie est en cours...
   2. Création d'une variable locale de type string "lesPointages"
   3. Boucle qui passera tous les éléments du dictionnaire contenant la référence à chaque joueur et à son pointage.
   On va chercher le nom du joueur ainsi que son pointage et on l'ajoute à la variable locale "lesPointages". À la fin
   la chaine de caractère contientra tous les noms et tous les pointages.
   4. Affichage des noms et des pointages (var lesPointages ) dans la zone de texte située en haut de l'écran.
   */
    void Update()
    {
        if (partieEnCours)
        {
            string lesPointages = "";
            foreach (joueurReseau joueurReseau in joueursPointagesData.Keys)
            {
                lesPointages += $"{joueurReseau.monNom} : {joueurReseau.nbBoulesRouges}   ";
            }
            refTxtPointage.text = lesPointages;
        }
    }

    /* Fonction appelée par le bouton pour commencer une partie
    1. Récupération du nom du joueur (string)
    2. Appel de la fonction CreationPartie pour établir la connexion au serveur (dans script gestionnaireReseau)
    3. Désactivation du canvas de départ et activation du canvas de jeu
    */
    public void OnRejoindrePartie()
    {
        //.1
        nomJoueurLocal = refTxtNomJoueur.text;
        //.2
        gestionnaireReseau.CreationPartie(GameMode.AutoHostOrClient);
        //.3
        refCanvasDepart.SetActive(false);
        refCanvasJeu.SetActive(true);
    }

    public void FinPartie(string nomGagnant)
    {
        partieEnCours = false;
        refPanelGagnant.SetActive(true);
        refTxtGagnant.text = nomGagnant;
        foreach (joueurReseau leJoueur in joueursPointagesData.Keys)
        {
            lstJoueurReseau.Add(leJoueur);
        }

    }

    /* Fonction appelée par le GestionnaireMouvementPersonnage qui vérifie si la touche "R" a été
   enfoncée pour reprendre une nouvelle partie. Cette fonction sera exécuté seulement sur le
   serveur.
   1. On retire de la liste lstJoueurReseau la référence au joueur qui est prêt à reprendre.
   2. Si la liste lstJoueurReseau est rendu vide (== 0), c'est que tous les joueurs sont prêt
   a reprendre. Si c'est le cas, on appelle la fonction Recommence présente dans le script
   JoueurReseau. Tous les joueurs exécuteront cette fonction.
   */
    public void JoueurPretReprise(joueurReseau joueurReseau)
    {
        lstJoueurReseau.Remove(joueurReseau);

        if (lstJoueurReseau.Count == 0)
        {
            foreach (joueurReseau leJoueur in joueursPointagesData.Keys)
            {
                leJoueur.Recommence();
            }
        }
    }
}

