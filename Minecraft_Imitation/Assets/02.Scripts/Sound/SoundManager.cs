using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SFXSound
{
    public SFXSound(AudioClip audioClip)
    {
        if (audioClip == null)
            return;
        this.audioClip = audioClip;
        length = audioClip.length;
    }
    
    public AudioClip audioClip;
    public float length;
}

public class SoundManager : MonoBehaviour
{
    // 싱글턴
    public static SoundManager instance;

    // 효과음 AudioSource Prefab
    public SFXAudioSource prefab_SFXAudioSource;
    // 배경음악 AudioSource
    private AudioSource bgmAudioSource;

    // 효과음 AudioSource 오브젝트 리스트
    private Queue<SFXAudioSource> sfxAudioSources = new Queue<SFXAudioSource>();
    // 풀링을 위한 비활성화 효과음 AudioSource 오브젝트 리스트
    private Queue<SFXAudioSource> inactiveSFXAudioSources = new Queue<SFXAudioSource>();

    // 효과음 볼륨
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

    // 효과음 볼륨 변경
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

    // 효과음 audioSource 활성화
    public SFXAudioSource ActiveSFXSound(SFXSound sound, SFXAudioSource audioSource, Transform parent, Block block = null)
    {
        SFXAudioSource source = null;
        if (audioSource != null) // 효과음 중복 막기
        {
            source = audioSource;
        }
        else
        {
            // 풀에 오브젝트가 없으면 생성
            if(inactiveSFXAudioSources.Count == 0)
            {
                source = Instantiate(prefab_SFXAudioSource, transform);
                sfxAudioSources.Enqueue(source);
            }
            else // 풀에 오브젝트가 있으면 액티브
            {
                source = inactiveSFXAudioSources.Dequeue();
                source.gameObject.SetActive(true);
            }
        }

        source.transform.SetParent(parent);
        source.transform.localPosition = Vector3.zero;
        source.ActiveSound(sound , block);

        return source;
    }

    //  풀링을 위한 효과음 오브젝트 비활성화
    public void InactiveSFXSound(SFXAudioSource sfxAudioSource)
    {
        sfxAudioSource.transform.SetParent(transform);
        sfxAudioSource.gameObject.SetActive(false);
        inactiveSFXAudioSources.Enqueue(sfxAudioSource);
    }

}
