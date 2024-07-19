using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventWithBox : MonoBehaviour
{
    BlockData.BlockType test = BlockData.BlockType.Pick;
    public float breakPower = 5;
    public float attackPower = 5;
    public float pushForce = 3;
    public GameObject effectFac;
    Block OnClickBlockCs;
    GameObject OnClickBlock = null;
    GameObject OnHitRayBlock;
    Mob mob;
    
    float effCool = 0.5f;
    float currTime = 0;

    bool isClicking = false;

    void Update()
    {

        Ray camRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit camHitInfo = new RaycastHit();
        bool anyHit = Physics.Raycast(camRay, out camHitInfo, PlayerManager.instance.aimRange, (-1) - (1 << LayerMask.NameToLayer("ObjectParticle")));

        if (anyHit) OnHitRayBlock = camHitInfo.transform.gameObject;// 무언가 맞았다면
        if (anyHit && Input.GetMouseButtonDown(0) && !PlayerManager.onInventory &&!PlayerManager.instance.isBow) // 좌클릭을 하면
        {
            isClicking = true;
            OnClickBlock = OnHitRayBlock; // 블럭을 저장하고
            if (OnClickBlock.GetComponent<Block>() != null)
            {
                OnClickBlockCs = OnClickBlock.GetComponent<Block>(); // 그 블럭의 컴포넌트를 저장
            }
            if (OnClickBlockCs != null)
                OnClickBlockCs.Break(test, breakPower); // 그 블럭의 Break를 호출

            if(camHitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Mob")) // 좌클릭시 공격 코드.
            {
                print(camHitInfo.transform.gameObject.name);
                mob = camHitInfo.transform.GetComponent<Mob>();
                mob.UpdateHP(transform, -attackPower, pushForce);
                print(mob.currHP);
            }

            // effect 스폰 1회성
            if (anyHit && camHitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Block"))
            {
                currTime = 0;
                GameObject effect = Instantiate(effectFac);
                effect.transform.position = camHitInfo.point;
                effect.transform.forward = camHitInfo.normal;
            }
        }
        if (Input.GetMouseButtonUp(0))// 좌클릭을 떼면
        {
            isClicking = false;
            if (OnClickBlockCs != null)
            {
                OnClickBlockCs.StopBroke(); // 그 블럭의 stop을 호출하고
            }
            OnClickBlock = null; // 그 블럭 값을 null로 바꿈.

        }

        // 좌클릭 중에 대상이 바뀌면
        if (isClicking == true && OnClickBlock != OnHitRayBlock)
        {
            // Break를 진행중이던 블럭의 stop을 호출하고
            if (OnClickBlockCs != null)
                OnClickBlockCs.StopBroke();
            OnClickBlock = OnHitRayBlock;// 대상이 바뀌었음을 전달.
            // 그리고 그 대상으로 다시 Break 진행.
            if (OnClickBlock.GetComponent<Block>() != null)
            {
                OnClickBlockCs = OnClickBlock.GetComponent<Block>(); // 그 블럭의 컴포넌트를 저장
            }
            if (OnClickBlockCs != null)
                OnClickBlockCs.Break(test, breakPower);
        }

        if (anyHit && isClicking && camHitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Block") && !PlayerManager.instance.isBow)
        {
            if (PlayerManager.onInventory)
            {
                OnClickBlockCs.StopBroke();
                isClicking = false;
                return;
            }
            currTime += Time.deltaTime;
            if (currTime > effCool)
            {
                currTime = 0;
                GameObject effect = Instantiate(effectFac);
                effect.transform.position = camHitInfo.point;
                effect.transform.forward = camHitInfo.normal;
            }
        }


    }
}
