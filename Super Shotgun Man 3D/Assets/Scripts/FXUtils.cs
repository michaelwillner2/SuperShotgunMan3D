using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FXUtils
{
    private static FXScriptableObject FX_Prefabs = Resources.Load<FXScriptableObject>("FX Container");

    public static void InstanceFXObject(int o, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        Object.Instantiate(FX_Prefabs.FX_ARRAY[o], position, rotation, parent);
    }
}
