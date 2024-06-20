using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;


public class Block : MonoBehaviour
{
    #region �׽�Ʈ�� �����ؾ� ��
    public bool start = false;
    public bool start1 = false;
    public BlockData.BlockType test_Tool;
    public float test_Power;
    #endregion

    public BlockData blockData;

    [SerializeField]
    private bool canBreak;

    [SerializeField]
    private Block_InteractionParticle prefab_Block_InteractionParticle; // ���� �ı��� ������ ����

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


    private void Start()
    {
        blockData = new BlockData(GameObjectData.ObjectKind.Block, BlockData.BlockKind.Dirt, BlockData.BlockType.Pick, 100, Sound.AudioClipName.DirtBreak, Sound.AudioClipName.DirtBroken);
        strength = blockData.strength;
    }

    private void Update()
    {
        if (start)
        {
            start = false;
            Break(test_Tool, test_Power);
        }
        if (start1)
        {
            start1 = false; 
            StopBroke();
        }
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
            print("���� �ı� ����"); 
            strength = blockData.strength;
            StopCoroutine(checkBreak_Coroutine);
            checkBreak_Coroutine = null;

            if (sfxAudioSource != null)
            {
                InActiveSFXSound();
            }
        }
    }


    private void Broken() // ���� �ı�
    {
        StopBroke();
        SoundManager.instance.ActiveSFXSound(blockData.brockBrokenSound, sfxAudioSource, null, true);

        if (prefab_Block_InteractionParticle == null)
        {
            return;
        }
        Instantiate(prefab_Block_InteractionParticle, transform.position, Quaternion.identity);

        print("���� �ı�");
        gameObject.SetActive(false);
        // ���̴� �ʱ�ȭ �߰�
        broken = false;
    }

    
    public void InActiveSFXSound() // ȿ���� ȸ��
    {
        if (sfxAudioSource != null)
        {
            SoundManager.instance.InactiveSFXSound(sfxAudioSource);
            sfxAudioSource = null;
        }
    }

    IEnumerator CheckBreak(float Power)
    {
        print("���� �ı� ����");
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
            // ���̴� �۵�
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
