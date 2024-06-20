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
        DirtBreak
    }
    public AudioClipName audioClipName;
    public AudioClip audioClip;
    public float clipLength;
}

public class SoundManager : MonoBehaviour
{
    // �̱���
    public static SoundManager instance;

    // ȿ���� AudioSource Prefab
    public SFXAudioSource prefab_SFXAudioSource;
    // ������� AudioSource
    private AudioSource bgmAudioSource;

    [SerializeField]
    private Sound[] sounds;

    public Dictionary<Sound.AudioClipName, Sound> soundDictionary = new Dictionary<Sound.AudioClipName, Sound>();

    // ȿ���� AudioSource ������Ʈ ����Ʈ
    private Queue<SFXAudioSource> sfxAudioSources = new Queue<SFXAudioSource>();
    // Ǯ���� ���� ��Ȱ��ȭ ȿ���� AudioSource ������Ʈ ����Ʈ
    private Queue<SFXAudioSource> inactiveSFXAudioSources = new Queue<SFXAudioSource>();

    // ȿ���� ����
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
    
    // �ν����� â���� �޾ƿ� ���带 Dictionary�� ����
    private void InitSounds()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            sounds[i].clipLength = sounds[i].audioClip.length;
            soundDictionary.Add(sounds[i].audioClipName, sounds[i]);
        }
    }


    
    // ȿ���� ���� ����
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

    // ȿ���� audioSource Ȱ��ȭ
    public SFXAudioSource ActiveSFXSound(Sound.AudioClipName audioClipName, SFXAudioSource audioSource, Transform parent, bool autoInactive)
    {
        // ����� Ÿ���� ���� �ʾҴٸ� return
        if (audioClipName == 0)
        {
            return null;
        }

        if (audioSource != null) // ȿ���� �ߺ� ����
        {
            source = audioSource;
        }
        else
        {
            // Ǯ�� ������Ʈ�� ������ ����
            if (inactiveSFXAudioSources.Count == 0)
            {
                source = Instantiate(prefab_SFXAudioSource, transform);
                source.SetVolume(sfxVolume);
                sfxAudioSources.Enqueue(source);
            }
            else // Ǯ�� ������Ʈ�� ������ ��Ƽ��
            {
                source = inactiveSFXAudioSources.Dequeue();
                source.gameObject.SetActive(true);
            }
        }
        if (parent != null)
        {
            source.transform.SetParent(parent);
        }
        source.transform.localPosition = Vector3.zero;
        source.ActiveSound(soundDictionary[audioClipName], autoInactive);

        return source;
    }

    //  Ǯ���� ���� ȿ���� ������Ʈ ��Ȱ��ȭ
    public void InactiveSFXSound(SFXAudioSource sfxAudioSource)
    {
        sfxAudioSource.StopSound();
        sfxAudioSource.transform.SetParent(transform);
        sfxAudioSource.gameObject.SetActive(false);
        inactiveSFXAudioSources.Enqueue(sfxAudioSource);
    }

}
