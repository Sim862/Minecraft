using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SFXAudioSource : MonoBehaviour
{
    public AudioSource audioSource;
    private Sound sound;
    private IEnumerator inactiveSFXSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        ResetCoroutine();
    }

    private void ResetCoroutine()
    {
        inactiveSFXSound = null;
        StopAllCoroutines();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public float GetSoundLength()
    {
        if (sound != null)
        {
            return sound.clipLength;
        }
        return 0;
    }

    public void ActiveSound(Sound sound, bool autoInactive)
    {
        this.sound = sound;

        audioSource.clip = sound.audioClip;
        audioSource.Play();

        if (autoInactive)
        {
            if (inactiveSFXSound != null)
            {
                StopCoroutine(inactiveSFXSound);
            }
            inactiveSFXSound = InactiveSFXSound();
            StartCoroutine(inactiveSFXSound);
        }
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
        yield return new WaitForSeconds(sound.clipLength);
        SoundManager.instance.InactiveSFXSound(this);
        inactiveSFXSound = null;
    }
}
