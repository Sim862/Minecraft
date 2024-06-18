using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Block : MonoBehaviour
{
    public enum BlockType
    {
        Dirt,
        Wood,
        Water,
        Stone
    }

    [SerializeField]
    private Sprite icon;
    public Sprite GetIcon() => icon;

    [SerializeField]
    private Block_InteractionParticle prefab_Block_InteractionParticle; // ºí·° ÆÄ±«½Ã ³ª¿À´Â ÀÜÇØ
    [SerializeField]
    private SFXSound brokenSound;
    private SFXAudioSource brokeAudioSources = null;

    private void Broken()
    {
        if (brokenSound != null)
            brokeAudioSources = SoundManager.instance.ActiveSFXSound(brokenSound, brokeAudioSources, transform.position);
        if (prefab_Block_InteractionParticle == null)
            return;
        Instantiate(prefab_Block_InteractionParticle, transform.position, Quaternion.identity);
    }

    public void InActiveBrokenSound()
    {
        brokeAudioSources = null;
    }
}
