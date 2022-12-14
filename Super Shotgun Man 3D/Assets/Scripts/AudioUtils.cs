using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioUtils
{
    private static AudioScriptableObject sound_table;
    
    private static IEnumerator PlayOnce(AudioSource sound)
    {
        yield return new WaitUntil(() => sound.isPlaying);
        yield return new WaitUntil(() => !sound.isPlaying);
        MonoBehaviour.Destroy(sound.gameObject);
    }

    private static IEnumerator MusicLoop(AudioSource song, AudioClip new_loop)
    {
        yield return new WaitUntil(() => song.isPlaying);
        yield return new WaitUntil(() => song.time >= song.clip.length);
        song.clip = new_loop;
        song.Stop();
        song.Play();
    }

    //instance something from the SFX table at a point
    public static void InstanceSound(int index, Vector3 position, MonoBehaviour mono, Transform parent = null, bool falloff = false, float volume = 1.0f, float pitch = 1.0f)
    {
        if(index < 0 || index >= sound_table.SFX_ARRAY.Length)
        {
            index = 0;
            Debug.LogWarning("Index provided to InstanceSound of AudioUtils is out of range, setting it to 0!");
        }
        Object instantiation_object;
        if (falloff)
            instantiation_object = Resources.Load<Object>("AudioObjectNormal");
        else
            instantiation_object = Resources.Load<Object>("AudioObjectNOFALLOFF");

        GameObject sfx_object = (GameObject)MonoBehaviour.Instantiate(instantiation_object);
        sfx_object.transform.position = position;
        if (parent != null) sfx_object.transform.parent = parent;
        sfx_object.GetComponent<AudioSource>().clip = sound_table.SFX_ARRAY[index];
        sfx_object.GetComponent<AudioSource>().volume = volume;
        sfx_object.GetComponent<AudioSource>().pitch = pitch;
        mono.StartCoroutine(PlayOnce(sfx_object.GetComponent<AudioSource>()));
    }

    //instance something from the Voice table at a point
    public static void InstanceVoice(int index, Vector3 position, MonoBehaviour mono, Transform parent = null, bool falloff = false, float volume = 1.0f, float pitch = 1.0f)
    {
        if (index < 0 || index >= sound_table.SFX_ARRAY.Length)
        {
            index = 0;
            Debug.LogWarning("Index provided to InstanceVoice of AudioUtils is out of range, setting it to 0!");
        }
        Object instantiation_object;
        if (falloff)
            instantiation_object = Resources.Load<Object>("AudioObjectNormal");
        else
            instantiation_object = Resources.Load<Object>("AudioObjectNOFALLOFF");

        GameObject sfx_object = (GameObject)MonoBehaviour.Instantiate(instantiation_object);
        sfx_object.transform.position = position;
        if (parent != null) sfx_object.transform.parent = parent;
        sfx_object.GetComponent<AudioSource>().clip = sound_table.VOICE_ARRAY[index];
        sfx_object.GetComponent<AudioSource>().volume = volume;
        sfx_object.GetComponent<AudioSource>().pitch = pitch;
        mono.StartCoroutine(PlayOnce(sfx_object.GetComponent<AudioSource>()));
    }

    //instance something from the Music table and handle special looping
    public static void PlayMusic(int index, MonoBehaviour mono, float volume = 1.0f)
    {
        if (index < 0 || index >= sound_table.SFX_ARRAY.Length)
        {
            index = 0;
            Debug.LogWarning("Index provided to PlayMusic of AudioUtils is out of range, setting it to 0!");
        }
        GameObject music_object = (GameObject)MonoBehaviour.Instantiate(Resources.Load<Object>("AudioObjectNOFALLOFF"));
        music_object.GetComponent<AudioSource>().clip = sound_table.MUSIC_ARRAY[index].song;
        music_object.GetComponent<AudioSource>().loop = true;
        if(sound_table.MUSIC_ARRAY[index].loop_clip != null)
        {
            mono.StartCoroutine(MusicLoop(music_object.GetComponent<AudioSource>(), sound_table.MUSIC_ARRAY[index].loop_clip));
        }
    }
}
