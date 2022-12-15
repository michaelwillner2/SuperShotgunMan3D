using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewmodelAudioHelper : MonoBehaviour
{
    PlayerMovement movement;
    // Start is called before the first frame update
    void Start()
    {
        movement = transform.root.GetComponent<PlayerMovement>();
    }

    public void PlayFireSound() { movement.PlayFireSound(); }
    public void PlayFatFireSound() { movement.PlayFatFireSound(); }
    public void PlayReloadSound() { movement.PlayReloadSound(); }
}
