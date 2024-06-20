using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;


public class Block : MonoBehaviour
{
    public BlockData blockData;

    [SerializeField]
    private bool canBreak;

    [SerializeField]
    private Block_InteractionParticle prefab_Block_InteractionParticle; // 블럭 파괴시 나오는 잔해
    [SerializeField]
    private SFXSound brokenSound;
    private SFXAudioSource breakAudioSource = null;
    

    private bool broken = false;
    private float strength;
    private bool typeCheck = false;

    private IEnumerator corutine;

    private void OnEnable()
    {
        strength = blockData.strength;
    }

    public bool start = false;
    public BlockData.BlockType tool;
    public float power;

    private void Start()
    {
        blockData = new BlockData(GameObjectData.ObjectKind.Block, BlockData.BlockKind.Dirt, BlockData.BlockType.Pick, 100);
        strength = blockData.strength;
    }

    private void Update()
    {
        if (start)
        {
            start = false;
            Broke(tool, power);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Broke(BlockData.BlockType blockType, float Power)
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
            strength = blockData.strength;
            corutine = CheckBreak(Power);
            StartCoroutine(corutine);
        }
    }

    public void StopBroke()
    {
        if (corutine != null)
        {
            print("블럭 파괴 종료");
            StopCoroutine(corutine);
            corutine = null;
        }
    }

    private void Broken() // 블럭 파괴
    {
        StopBroke();
        if (brokenSound != null)
        {
            breakAudioSource = SoundManager.instance.ActiveSFXSound(brokenSound, breakAudioSource, transform);
        }
        if (prefab_Block_InteractionParticle == null)
        {
            return;
        }
        Instantiate(prefab_Block_InteractionParticle, transform.position, Quaternion.identity);

        gameObject.SetActive(false);
        // 쉐이더 초기화 추가
        broken = false;
    }

    public void InActiveBrokenSound() // 효과음 회수
    {
        if (breakAudioSource != null)
        {
            SoundManager.instance.InactiveSFXSound(breakAudioSource);
            breakAudioSource = null;
        }
    }

    IEnumerator CheckBreak(float Power)
    {
        while (!broken)
        {
            print("블럭 파괴 중");
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
            if(strength <= 0)
            {
                broken = true;
                Broken();
            }
        }
    }

}
