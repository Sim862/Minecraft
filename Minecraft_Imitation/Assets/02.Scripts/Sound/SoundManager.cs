using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SFXSound
{
    public SFXSound()
    {
        length = audioClip.length;
    }
    
    public AudioClip audioClip;
    public float length;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public SFXAudioSource prefab_SFXAudioSource;

    private AudioSource bgmAudioSource;
    private Queue<SFXAudioSource> sfxAudioSources = new Queue<SFXAudioSource>();
    private Queue<SFXAudioSource> inactiveSFXAudioSources = new Queue<SFXAudioSource>();

    private float sfxVolume = 0.5f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        bgmAudioSource = GetComponent<AudioSource>();
    }

    public void ChangeSFXVolum(float voluem)
    {
        if (voluem > 1) voluem = 1;
        else if (voluem < 0) voluem = 0;
        sfxVolume = voluem;

        foreach (SFXAudioSource item in sfxAudioSources)
        {
            item.SetVolume(sfxVolume);
        }
    }

    public SFXAudioSource ActiveSFXSound(SFXSound sound, SFXAudioSource audioSource, Vector3 position, Block block = null)
    {
        SFXAudioSource source = null;
        if (audioSource != null)
        {
            source = audioSource;
        }
        else
        {
            if(inactiveSFXAudioSources.Count == 0)
            {
                source = Instantiate(prefab_SFXAudioSource, transform);
                sfxAudioSources.Enqueue(source);
            }
            else
            {
                source = inactiveSFXAudioSources.Dequeue();
                source.gameObject.SetActive(true);
            }
        }

        source.transform.position = position;
        source.ActiveSound(sound , block);

        return source;
    }

    public void InactiveSFXSound(SFXAudioSource sfxAudioSource)
    {
        inactiveSFXAudioSources.Enqueue(sfxAudioSource);
    }

}
