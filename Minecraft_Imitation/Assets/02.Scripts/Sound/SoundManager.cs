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
    // �̱���
    public static SoundManager instance;

    // ȿ���� AudioSource Prefab
    public SFXAudioSource prefab_SFXAudioSource;
    // ������� AudioSource
    private AudioSource bgmAudioSource;

    // ȿ���� AudioSource ������Ʈ ����Ʈ
    private Queue<SFXAudioSource> sfxAudioSources = new Queue<SFXAudioSource>();
    // Ǯ���� ���� ��Ȱ��ȭ ȿ���� AudioSource ������Ʈ ����Ʈ
    private Queue<SFXAudioSource> inactiveSFXAudioSources = new Queue<SFXAudioSource>();

    // ȿ���� ����
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
    public SFXAudioSource ActiveSFXSound(SFXSound sound, SFXAudioSource audioSource, Transform parent, Block block = null)
    {
        SFXAudioSource source = null;
        if (audioSource != null) // ȿ���� �ߺ� ����
        {
            source = audioSource;
        }
        else
        {
            // Ǯ�� ������Ʈ�� ������ ����
            if(inactiveSFXAudioSources.Count == 0)
            {
                source = Instantiate(prefab_SFXAudioSource, transform);
                sfxAudioSources.Enqueue(source);
            }
            else // Ǯ�� ������Ʈ�� ������ ��Ƽ��
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

    //  Ǯ���� ���� ȿ���� ������Ʈ ��Ȱ��ȭ
    public void InactiveSFXSound(SFXAudioSource sfxAudioSource)
    {
        sfxAudioSource.transform.SetParent(transform);
        sfxAudioSource.gameObject.SetActive(false);
        inactiveSFXAudioSources.Enqueue(sfxAudioSource);
    }

}
