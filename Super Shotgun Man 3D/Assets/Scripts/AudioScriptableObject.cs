using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Audio Container", menuName = "Unity Assets/Audio Container", order = 0)]
public class AudioScriptableObject : ScriptableObject
{
    [System.Serializable]
    public struct MusicObject{
        public AudioClip song;
        public AudioClip loop_clip;
    }

    [SerializeField]
    public AudioClip[] VOICE_ARRAY, SFX_ARRAY;

    [SerializeField]
    public MusicObject[] MUSIC_ARRAY;
}
