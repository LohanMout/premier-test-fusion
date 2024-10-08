using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

/*
* Script qui ex�cute les d�placements du joueur et ainsi que l'ajustement de direction
* D�rive de NetworkBehaviour. Utilisation de la fonction r�seau FixedUpdateNetwork()
* Variables :
* - camLocale : contient la r�f�rence � la cam�ra du joueur actuel
* - NetworkCharacterController : pour m�moriser le component NetworkCharacterController
* du joueur
*/

public class gestionnaireMouvementPersonnage : NetworkBehaviour
{
    Camera camLocale;
    NetworkCharacterController networkCharacterController;

    // variable pour garder une r�f�rence au script gestionnairePointsDeVie;
    GestionnairePointsDeVie gestionnairePointsDeVie;
    // variable pour savoir si un Respawn du joueur est demand�
    bool respawnDemande = false;

    /*
        * Avant le Start(), on m�morise la r�f�rence au component networkCharacterController du joueur
        * On garde en m�moire la cam�ra du joueur courant (GetComponentInChildren)
        */
    void Awake()
    {
        networkCharacterController = GetComponent<NetworkCharacterController>();
        camLocale = GetComponentInChildren<Camera>();
        gestionnairePointsDeVie = GetComponent<GestionnairePointsDeVie>();
    }

    /* Fonction publique appel�e sur serveur uniquement, par la coroutine RessurectionServeur_CO() du
     * script GestionnairePointsDeVie. L'origine de la s�quence d'�v�nements d�but dans le script
     * GestionnairesArmes lorsq'un joueur es touch� par un tir :
     * - GestionnairesArmes : Appelle la fonction PersoEstTouche() du script GestionnairePointDeVie;
     * Notez que cet appel est fait uniquement sur l'h�te (le serveur) et ne s'ex�cute pas sur les clients
     * - GestionnairesPointsDeVie : Appel la coroutine RessurectionServeur_CO() dans son propre script
     * - Coroutine RessurectionServeur_CO() : Appel la fonction DemandeRespawn 
     *   du script GestionnaireMouvementPersonnage
     */
    public void DemandeRespawn()
    {
        respawnDemande = true;
    }

    /*
     * Fonction r�cursive r�seau pour la simulation. � utiliser pour ce qui doit �tre synchronis� entre
     * les diff�rents clients.
     * 1.R�cup�ration des Inputs m�moris�s dans le script GestionnaireReseau (input.set). Ces donn�es enregistr�es
     * sous forme de structure de donn�es (struc) doivent �tre r�cup�r�es sous la m�me forme.
     * 2.Ajustement de la direction du joueur � partir � partir des donn�es de Input enregistr�s dans les script
     * GestionnaireR�seau et GestionnaireInputs.
     * 3. Correction du vecteur de rotation pour garder seulement la rotation Y pour le personnage (la capsule)
     * 4.Calcul du vecteur de direction du d�placement en utilisant les donn�es de Input enregistr�s.
     * Avec cette formule,il y a un d�placement lat�ral (strafe) li�  � l'axe horizontal (mouvementInput.x)
     * Le vecteur est normalis� pour �tre ramen� � une longueur de 1.
     * Appel de la fonction Move() du networkCharacterController (fonction pr�existante)
     * 5.Si les donn�es enregistr�es indiquent un saut, on appelle la fonction Jump() du script
     * networkCharacterController (fonction pr�existante)
     */
    public override void FixedUpdateNetwork()
    {
        //Si on est sur le serveur et qu'un respawn a �t� demand�, on appele la fonction Respawn()
        if (Object.HasStateAuthority && respawnDemande)
        {
            Respawn();
            return;
        }
        // Si le joueur est mort, on sort du script imm�diatement
        if (gestionnairePointsDeVie.estMort)
            return;

        // 1.
        GetInput(out donneesInputReseau donneesInputReseau);
        if (GameManager.partieEnCours)
        {
            //2.
            transform.forward = donneesInputReseau.vecteurDevant;

            //3.
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
            transform.rotation = rotation;

            //4.
            Vector3 directionMouvement = transform.forward * donneesInputReseau.mouvementInput.y + transform.right * donneesInputReseau.mouvementInput.x;
            directionMouvement.Normalize();
            networkCharacterController.Move(directionMouvement);

            //5.saut, important de le faire apr�s le d�placement
            if (donneesInputReseau.saute) networkCharacterController.Jump();
        }
    }

    /* Fonction qui appelle la fonction TeleportToPosition du script networkCharacterControllerPrototypeV2
    * 1. T�l�porte � un point al�atoire et modifie la variable respawnDemande � false
    * 2. Appelle la fonction Respawn() du script gestionnairePointsDeVie
    */
    void Respawn()
    {
        //1.
        ActivationCharacterController(true);
        networkCharacterController.Teleport(utilitaires.GetPositionSpawnAleatoire());
        respawnDemande = false;
        //2.
        gestionnairePointsDeVie.Respawn();
    }

    /* Fonction publique qui active ou d�sactive le script networkCharacterControllerPrototypeV2
     */
    public void ActivationCharacterController(bool estActif)
    {
        networkCharacterController.enabled = estActif;
    }
}

