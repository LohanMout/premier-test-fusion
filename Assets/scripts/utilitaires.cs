using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* 
 * Classe comprenant des fonctions statiques g�n�rales utilis�es
 par plusieurs scripts
*/

public class utilitaires
{
    // Fonction statique qui retourne un vector3 al�atoire
    public static Vector3 GetPositionSpawnAleatoire()
    {
        return new Vector3(Random.Range(-20, 20), 4, Random.Range(-20, 20));
    }

    public static void SetRenderLayerInChildren(Transform transform, int numLayer)
    {
        foreach (Transform trans in transform.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = numLayer;
        }
    }

}
