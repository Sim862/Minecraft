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
    private bool typeCheck = false;
    private ObjectParticle objectParticle;

    private IEnumerator checkBreak_Coroutine;

    public PositionData positionData;

    public List<GameObject> faces = new List<GameObject>();

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnDisable()
    {
        ResetCoroutine();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();   
    }

    private void ResetCoroutine()
    {
        checkBreak_Coroutine = null;
        StopAllCoroutines();
    }

    public void InitBlock(BlockData blockData, PositionData positionData)
    {
        this.blockData = new BlockData(blockData);
        //this.meshRenderer.material = blockData.material;
        this.positionData = positionData;

        broken = false;
        canBreak = true;

        if(blockData.blockName == BlockData.BlockName.CraftingTable)
        {
            tag = "CraftingTable";
        }
        else
        {
            tag = "Block";
        }
    }

    public void Break(BlockData.BlockType blockType, float Power) // 한번만 호출하면 블럭 체력 까이기 시작. 피 까이는 상태.
    {
        if (canBreak && gameObject.activeSelf)
        {
            StopBroke(); // CheckBreak가 실행 중이라면 실행.
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
            StopCoroutine(checkBreak_Coroutine);
            checkBreak_Coroutine = null;
        }
    }


    private void Broken() // 블럭 파괴
    {
        StopBroke();
        SoundManager.instance.ActiveOnShotSFXSound(blockData.brockBrokenSound, null, transform.position);
        objectParticle = DataManager.instance.GetObjectParticlePrefab(blockData.objectParticle);
        if (objectParticle != null)
        {
            objectParticle.UpdateCount(1);
            if (objectParticle != null)
            {
                objectParticle = Instantiate(objectParticle, transform.position, objectParticle.transform.rotation, MapManager.instance.transform);
                objectParticle.GetComponent<Rigidbody>().AddForce(Vector3.up * 50);
            }
        }
        // 쉐이더 초기화 추가
        broken = false;
        canBreak = false;

        MapManager.instance.BreakBlock(this);
    }


    IEnumerator CheckBreak(float Power)
    {
        if (blockData.brockBreakSound != Sound.AudioClipName.None)
        {
            if(sfxAudioSource != null)
            {
                if(sfxAudioSource.audioSource.isVirtual)
                {
                    sfxAudioSource = SoundManager.instance.ActiveOnShotSFXSound(blockData.brockBreakSound, transform, Vector3.zero);
                }
            }
            else
            {
                sfxAudioSource = SoundManager.instance.ActiveOnShotSFXSound(blockData.brockBreakSound, transform, Vector3.zero);
            }
        }
        while (!broken)
        {
            if (blockData.brockBreakSound != Sound.AudioClipName.None && sfxAudioSource.audioSource.isVirtual)
            {
                sfxAudioSource = SoundManager.instance.ActiveOnShotSFXSound(blockData.brockBreakSound, transform, Vector3.zero);
            }
            if (typeCheck)
            {
                blockData.strength -= Power;
            }
            else
            {
                blockData.strength -= 1;
            }
            // 쉐이더 작동
            yield return new WaitForSeconds(0.1f);

            if (blockData.strength <= 0)
            {
                broken = true;
                Broken();
            }
        }
    }

    
}
