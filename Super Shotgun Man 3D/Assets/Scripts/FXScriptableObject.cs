using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FX Container", menuName = "Unity Assets/FX Container", order = 0)]
public class FXScriptableObject : ScriptableObject
{
    [SerializeField]
    public Object[] FX_ARRAY;
}
