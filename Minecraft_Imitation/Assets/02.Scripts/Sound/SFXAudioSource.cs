using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SFXAudioSource : MonoBehaviour
{
    private AudioSource audioSource;
    private int count = 0;
    private int count_Check;
    private SFXSound sound;
    private Block block;

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void ActiveSound(SFXSound sound, Block block)
    {
        count++;
        this.sound = sound;

        audioSource.clip = sound.audioClip;
        audioSource.Play();

        if (block != null)
            this.block = block;

        StartCoroutine(InactiveSFXSound());
    }


    private IEnumerator InactiveSFXSound()
    {
        count_Check = count;
        yield return new WaitForSeconds(sound.length);
        if(count_Check == count)
        {
            if (count == int.MaxValue)
                count = 0;

            if(block != null)
            {
                block = null;
                block.InActiveBrokenSound();
            }
            else
            {
                SoundManager.instance.InactiveSFXSound(this);
            }
        }
    }
}
