using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FXUtils
{
    private static FXScriptableObject FX_Prefabs = Resources.Load<FXScriptableObject>("FX Container");

    public static void InstanceFXObject(int o, Vector3 position, Quaternion rotation, Transform parent = null, bool alt_mode = false)
    {
        GameObject instance = (GameObject)Object.Instantiate(FX_Prefabs.FX_ARRAY[o], position, rotation, parent);

        //behaviors for alternate modes of effects
        if (alt_mode)
        {
            switch (o){
                case 2:
                    instance.GetComponent<ExplosionBehavior>().damaging = true;
                    break;
            }
        }
    }
}
