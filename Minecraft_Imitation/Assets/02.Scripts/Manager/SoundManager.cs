using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Audio;
using static BlockData;

[System.Serializable]
public class Sound
{
    public enum AudioClipName
    {
        None,
        DirtBroken,
        DirtBreak,

        Landing,
        // 플레이어
        Player_Hurt,
        Player_Attack,

        // 돼지
        Pig_Idle,
        Pig_Death,

        // 거미
        Spider_Idle,
        Spider_Death,
    }
    public AudioClipName audioClipName;
    public AudioClip audioClip
    {
        get => audioClips[Random.Range(0, audioClips.Length)];
    }
    public AudioClip[] audioClips;
}

public class SoundManager : MonoBehaviour
{
    // 싱글턴
    public static SoundManager instance;

    // 효과음 AudioSource Prefab
    public SFXAudioSource prefab_SFXAudioSource;
    // 배경음악 AudioSource
    private AudioSource bgmAudioSource;

    [SerializeField]
    private Sound[] sounds;

    public Dictionary<Sound.AudioClipName, Sound> soundDictionary = new Dictionary<Sound.AudioClipName, Sound>();

    // 효과음 AudioSource 오브젝트 리스트
    private Queue<SFXAudioSource> sfxAudioSources = new Queue<SFXAudioSource>();
    // 풀링을 위한 비활성화 효과음 AudioSource 오브젝트 리스트
    private Queue<SFXAudioSource> inactiveSFXAudioSources = new Queue<SFXAudioSource>();

    // 효과음 볼륨
    public float sfxVolume = 0.5f;

    SFXAudioSource source;

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
        InitSounds();
    }
    
    // 인스펙터 창에서 받아온 사운드를 Dictionary로 정리
    private void InitSounds()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            soundDictionary.Add(sounds[i].audioClipName, sounds[i]);
        }
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
    public SFXAudioSource ActiveOnShotSFXSound(Sound.AudioClipName audioClipName, Transform target, Vector3 position)
    {
        // 오디오 타입이 오지 않았다면 return
        if (audioClipName == 0)
        {
            return null;
        }

        // 풀에 오브젝트가 없으면 생성
        if (inactiveSFXAudioSources.Count == 0)
        {
            source = Instantiate(prefab_SFXAudioSource, transform);
            source.SetVolume(sfxVolume);
            sfxAudioSources.Enqueue(source);
        }
        else // 풀에 오브젝트가 있으면 액티브
        {
            source = inactiveSFXAudioSources.Dequeue();
            source.gameObject.SetActive(true);
        }
  
        if(target == null)
        {
            source.transform.position = position;
        }
        source.ActiveSound(soundDictionary[audioClipName], target);

        return source;
    }

    //  풀링을 위한 효과음 오브젝트 비활성화
    public void InactiveSFXSound(SFXAudioSource sfxAudioSource)
    {
        sfxAudioSource.StopSound();
        sfxAudioSource.transform.SetParent(transform);
        sfxAudioSource.gameObject.SetActive(false);
        inactiveSFXAudioSources.Enqueue(sfxAudioSource);
    }

}
