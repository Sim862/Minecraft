using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;


public class Block : MonoBehaviour
{

    public BlockData blockData;
    public MeshRenderer meshRenderer;

    [SerializeField]
    private bool canBreak;

    [SerializeField]
    private ObjectParticle prefab_ObjectParticle; // 블럭 파괴시 나오는 아이템

    private Sound brokenSound;
    private SFXAudioSource sfxAudioSource = null;
    

    private bool broken = false;
    private float strength;
    private bool typeCheck = false;

    private IEnumerator checkBreak_Coroutine;

    private void OnEnable()
    {
        strength = blockData.strength;
    }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        blockData = new BlockData(GameObjectData.ObjectKind.Block, BlockData.BlockKind.Dirt, BlockData.BlockType.Pick, 100, Sound.AudioClipName.DirtBreak, Sound.AudioClipName.DirtBroken);
        strength = blockData.strength;
    }

    private void OnDisable()
    {
        ResetCoroutine();
    }

    private void ResetCoroutine()
    {
        checkBreak_Coroutine = null;
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        InActiveSFXSound();
    }

    public void InitBlock(BlockData blockData)
    {
        this.blockData = blockData;
        this.meshRenderer.material = blockData.material;
    }

    public void Break(BlockData.BlockType blockType, float Power)
    {
        if (canBreak)
        {
            StopBroke();

            if (blockData.blockType == blockType)
            {
                typeCheck = true;
            }
            else
            {
                typeCheck = false;
            }
            checkBreak_Coroutine = CheckBreak(Power);
            StartCoroutine(checkBreak_Coroutine);
        }
    }

    public void StopBroke()
    {
        if (checkBreak_Coroutine != null)
        {
            print("블럭 파괴 중지"); 
            strength = blockData.strength;
            StopCoroutine(checkBreak_Coroutine);
            checkBreak_Coroutine = null;

            if (sfxAudioSource != null)
            {
                InActiveSFXSound();
            }
        }
    }


    private void Broken() // 블럭 파괴
    {
        StopBroke();
        SoundManager.instance.ActiveSFXSound(blockData.brockBrokenSound, sfxAudioSource, null, true);

        if (prefab_ObjectParticle == null)
        {
            return;
        }
        Instantiate(prefab_ObjectParticle, transform.position, Quaternion.identity);

        print("블럭 파괴");
        gameObject.SetActive(false);
        // 쉐이더 초기화 추가
        broken = false;
    }

    
    public void InActiveSFXSound() // 효과음 회수
    {
        if (sfxAudioSource != null)
        {
            SoundManager.instance.InactiveSFXSound(sfxAudioSource);
            sfxAudioSource = null;
        }
    }

    IEnumerator CheckBreak(float Power)
    {
        print("블럭 파괴 시작");
        sfxAudioSource = SoundManager.instance.ActiveSFXSound(blockData.brockBreakSound, sfxAudioSource, transform, false);
        float clipLength = sfxAudioSource.GetSoundLength();
        float clipLengthCheck = 0;
        while (!broken)
        {
            if (clipLengthCheck >= clipLength)
            {
                sfxAudioSource.ReplayAudio();
                clipLengthCheck = 0;
            }
            if (typeCheck)
            {
                strength -= Power;
            }
            else
            {
                strength -= 1;
            }
            // 쉐이더 작동
            yield return new WaitForSeconds(0.1f);
            clipLengthCheck += 0.1f;

            if (strength <= 0)
            {
                broken = true;
                Broken();
            }
        }
    }

}
