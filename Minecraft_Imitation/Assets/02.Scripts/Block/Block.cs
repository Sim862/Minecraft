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
    private bool canBreak = true;

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

    public void Break(BlockData.BlockType blockType, float Power) // 한번만 호출하면 블럭 체력 까이기 시작. 피 까이는 상태.
    {
        print("break 시작");
        if (canBreak)
        {
            StopBroke(); // CheckBreak가 실행 중이라면 실행.
            print("break 시작 및 canBreak");
            if (blockData.blockType == blockType)
            {
                typeCheck = true;
            }
            else
            {
                typeCheck = false;
            }
            checkBreak_Coroutine = CheckBreak(Power); // 블럭파괴 시작 함수.
            StartCoroutine(checkBreak_Coroutine);
        }
    }

    public void StopBroke() // 블럭 캐는거 중단. 중단되는 경우때 무조건 호출해야함. 피 까이는거 중단으로 바꿔주는 함수.
    {
        if (checkBreak_Coroutine != null) // CheckBreak가 실행 중이라면
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
        ObjectParticle objectParticle = DataManager.instance.GetObjectParticlePrefab(blockData.objectParticle); // ������ �Ŵ������� ������ Ȯ��
        if (objectParticle != null)
        {
            Instantiate(objectParticle, transform.position, objectParticle.transform.rotation);
        }

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
