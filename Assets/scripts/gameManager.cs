using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Référence à l'instance du GameManager
    public int objectifPoints = 10; // Nombre de point pour finir la partie
    public int nbBoulesRougesDepart = 20; // Nombre de boules rouges a spawner au début d'une partie.
    public static bool partieEnCours = true;  // Est que la partie est en cours (static)
    [SerializeField] gestionnaireReseau gestionnaireReseau; // Reférence au gestionnaire réseau
    public static string nomJoueurLocal; // Le nom du joueur local
    public static Dictionary<joueurReseau, int> joueursPointagesData = new Dictionary<joueurReseau, int>();
    //Dictionnaire pour mémoriser chaque JoueurReseau et son pointage. Au moment de la création d'un joueur (fonction Spawned() du joueur)
    // il ajoutera lui même sa référence au dictionnaire du GameManager.
    // Liste static vide de type JoueurReseau qui servira à garder en mémoire tous les
    // joueurs connectés. Sera utilisé entre 2 parties pour gérer la reprise.
    //public static List<joueurReseau> lstJoueurReseau = new List<joueurReseau>();

    [Header("Éléments UI")]
    public GameObject refCanvasDepart; // Référence au canvas de départ
    public GameObject refCanvasJeu; // Référence au canvas de jeu
    public TextMeshProUGUI refTxtNomJoueur; // Référence à la zone texte contenant le nom du joueur (dans CanvasDepart)
    public TextMeshProUGUI refTxtPointage; // Référence à la zone d'affichage de tous les pointages (dans CanvasJeu)
    public GameObject refPanelGagnant; // Référence au panel affichant le texte du gagnant.
    public TextMeshProUGUI refTxtGagnant; // Référence à la zone de texte pour afficher le nom du gagnant.
    public GameObject refPanelAttente; // Référence au panel affichant le d'attente entre deux partie.
    public GameObject txtAttenteAutreJoueur; // Texte sous forme de bandeau rouge pour indiquer au joueur qu'il est en attente. À défénir dans l'inspecteur de Unity.



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
        gestionnaireReseau.spheresDejaSpawn = false;
    }

    /* Fonction permettant d'afficher ou de masquer le texte d'attente d'un autre joueur (bandeau rouge) */
    public void AfficheAttenteAutreJoueur(bool etat)
    {
        txtAttenteAutreJoueur.SetActive(etat);
    }

    /* Fonction appelée lors qu'il est temps d'instancier de nouvelles boules rouges en début de partie
   On appelle simplement une autre fonction CreationBoulleRouge dans le script gestionnaireReseau.
   */
    public void NouvellesBoulesRouges()
    {
        gestionnaireReseau.CreationBouleRouge();
    }

    /* Fonction qui déclenchera une nouvelle partie si toutes les conditions sont réunis.
   1. Désactivation des panneaux de fin de partie
   2. On met la variable partieEnCours à true;
   3. Variable unSeulJoueur : pour gérer le cas où un seul joueur serait resté connecté.
   4. Appel de la fonction Recommence pour chaque JoueurReseau. Si le joueur est seul, cette fonction
   renverra true, sinon false;
   5. S'il y a plus d'un joueur, on appelle la fonction NouvellesBoulesRouges pour spawner des boules
   */
    public void DebutNouvellePartie()
    {
        //.1
        refPanelAttente.SetActive(false);
        refPanelGagnant.SetActive(false);
        //2.
        partieEnCours = true;
        //3.
        bool unSeulJoueur = false;
        //4.
        foreach (joueurReseau leJoueur in joueursPointagesData.Keys)
        {
            unSeulJoueur = leJoueur.Recommence();
        }
        if (!unSeulJoueur) NouvellesBoulesRouges();
    }

    /* Fonction du GameManager qui recoit le nombre de boules rouges à créer. Appelle ensuite
   une fonction du même nom dans le GestionnaireRéseau.
   */
    public void AjoutBoulesRouges(int combien)
    {
        gestionnaireReseau.AjoutBoulesRouges(combien);
    }
}

