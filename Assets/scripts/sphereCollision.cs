using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion; // Ne pas oublier le namespace Fusion. Permet d'utiliser les commandes de Fusion

/*Script appliqué sur le boules rouge et qui détecte les collisions avec les joueurReseau uniquement. Notez qu'on s'assure ici
que seul le serveur exécute le code lorsqu'il y a une collision. Dans la simulation locale, il ne se passera rien. Ceci permet d'éviter
des problèmes, par exemple si deux joueurs touchent à une boule rouge presque en même temps. Dans ce cas, c'est le serveur qui tranchera
et déterminera celui qui lui a touché en premier.

1. Utilisation de la fonction OnTriggerEnter comme à l'habitude.
2. On vérifie qu'on est sur le serveur et que l'objet touché contient le component JoueurReseau(script). Si la condition est vraie,
la variable joueurReseau contiendra la référence au script JoueurReseau du joueur qui a touché à la boule.
3. On augmente le pointage du joueur qui a touché une boule
4. On Despawn l'objet touché (la boule rouge). Seul le serveur exécute cette commande, mais l'objet disparaitra sur tous les clients.
*/
public class SphereCollision : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other) //1.
    {
        if (Runner.IsServer && other.gameObject.TryGetComponent(out joueurReseau JoueurReseau)) //2.
        {
            JoueurReseau.nbBoulesRouges++; //3.
            Runner.Despawn(Object);//4.
        }
    }
}
