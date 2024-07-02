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
        blockData = new BlockData(BlockData.BlockKind.Dirt, BlockData.BlockType.Pick, 100, Sound.AudioClipName.DirtBreak, Sound.AudioClipName.DirtBroken);
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
            print("�� �ı� ����"); 
            strength = blockData.strength;
            StopCoroutine(checkBreak_Coroutine);
            checkBreak_Coroutine = null;

            if (sfxAudioSource != null)
            {
                InActiveSFXSound();
            }
        }
    }


    private void Broken() // �� �ı�
    {
        StopBroke();
        SoundManager.instance.ActiveSFXSound(blockData.brockBrokenSound, sfxAudioSource, null, true);
        ObjectParticle objectParticle = DataManager.instance.GetObjectParticlePrefab(blockData.objectParticle); // ������ �Ŵ������� ������ Ȯ��
        if (objectParticle != null)
        {
            Instantiate(objectParticle, transform.position, objectParticle.transform.rotation);
        }

        print("�� �ı�");
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
        print("�� �ı� ����");
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
