using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SFXAudioSource : MonoBehaviour
{
    public AudioSource audioSource;
    private Sound sound;
    private IEnumerator inactiveSFXSound;
    private Transform target;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(target != null)
        {
            transform.position = target.position;
        }
    }

    private void OnDisable()
    {
        target = null;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void ActiveSound(Sound sound, Transform target)
    {
        this.target = target;

        this.sound = sound;
        audioSource.clip = sound.audioClip;
        audioSource.Play();

        StartCoroutine(InactiveSFXSound());
       
    }


    public void StopSound()
    {
        audioSource.Stop();
    }

    public void ReplayAudio()
    {
        audioSource.Play();
    }

    private IEnumerator InactiveSFXSound()
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        SoundManager.instance.InactiveSFXSound(this);
        inactiveSFXSound = null;
    }
}
