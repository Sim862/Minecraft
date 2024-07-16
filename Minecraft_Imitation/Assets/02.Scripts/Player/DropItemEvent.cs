using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemEvent : MonoBehaviour
{
    public GameObject objectParticleFac;
    ObjectParticleData.ParticleName particleKind; // 필드에 있는 오브젝트의 종류,개수,아이콘 가져옴.
    int count;
    Sprite icon;
    public Transform cameraPos;
    bool shiftOn;

    int nowUsingSlot = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            shiftOn = true;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            shiftOn = false;
        }
        DropItem();
    }

    // 이미지 변경하고 오브젝트파티클에 정보전달.
    void WhenDropOneChangeImage(int slotNum, ObjectParticle objectParticleCs, bool isOne) 
    {
        GameObject nowUsingObject = InventoryStatic.instance.slots[slotNum];
        GameObject nowUsingObjectInQuick = InventoryPopup.instance.quickSlot[slotNum];
        ItemImage nowItemImage = nowUsingObject.GetComponentInChildren<ItemImage>();
        ItemImage nowItemImageInQuick = nowUsingObjectInQuick.GetComponentInChildren<ItemImage>();
        if (nowItemImage != null && nowItemImageInQuick != null)
        {
            if (isOne)
            {
                objectParticleCs.count = 1;
                TransferDataWithoutCnt(objectParticleCs, nowItemImage);
                nowItemImage.ChangeItemCnt(-1);
                nowItemImageInQuick.ChangeItemCnt(-1);
            }
            else
            {
                objectParticleCs.count = nowItemImage.count;
                TransferDataWithoutCnt(objectParticleCs, nowItemImage);
                if(nowItemImage.particleObjectTr != null)
                {
                    Destroy(nowItemImage.particleObjectTr.gameObject);
                }
                Destroy(nowItemImage.gameObject);
                Destroy(nowItemImageInQuick.gameObject);
                
            }
        }
    }


    void TransferDataWithoutCnt(ObjectParticle objectParticleCs, ItemImage nowItemImage)
    {
        objectParticleCs.particleName = nowItemImage.particleKind;
        objectParticleCs.icon = nowItemImage.itemImage.sprite;
    }

    void DropItem()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            nowUsingSlot = PlayerManager.instance.usingSlot;

            // ObjectParticleKind 받아오는 용도
            GameObject nowUsingObject = InventoryStatic.instance.slots[nowUsingSlot];
            ItemImage nowItemImage = nowUsingObject.GetComponentInChildren<ItemImage>();
            if (nowItemImage == null)
            {
                return;
            }
            // ObjectParticle의 Prefab 받아오기
            ObjectParticle objectParticle = DataManager.instance.GetObjectParticlePrefab(nowItemImage.particleKind);
            // Prefab 생성
            objectParticle = Instantiate(objectParticle);

            if (shiftOn)
            {
                WhenDropOneChangeImage(nowUsingSlot, objectParticle, false); // 다 버릴때 false
            }
            else
            {
                WhenDropOneChangeImage(nowUsingSlot, objectParticle, true); // 하나 버리는 함수.
            }



            objectParticle.transform.position = cameraPos.position + Camera.main.transform.forward; // player한테 cs를 붙였다는 전제하에
            Rigidbody rg = objectParticle.GetComponent<Rigidbody>();
            rg.AddForce(transform.forward * 200);
        }
    }
}
