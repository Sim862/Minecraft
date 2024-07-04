using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemInMouseEvent : MonoBehaviour
{
    public GameObject objectParticleFac;
    ObjectParticleData.ParticleKind particleKind; // 필드에 있는 오브젝트의 종류,개수,아이콘 가져옴.
    int count;
    Sprite icon;
    public Transform cameraPos;

    int nowUsingSlot = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            nowUsingSlot = PlayerManager.instance.usingSlot;
            GameObject objectParticle = Instantiate(objectParticleFac);
            ObjectParticle objectParticleCs = objectParticle.GetComponent<ObjectParticle>();
            WhenDropOneChangeImage(nowUsingSlot, objectParticleCs);

            objectParticle.transform.position = cameraPos.position + Camera.main.transform.forward; // player한테 cs를 붙였다는 전제하에
            Rigidbody rg = objectParticle.GetComponent<Rigidbody>();
            rg.AddForce(transform.forward * 200);
        }
    }

    // 이미지 변경하고 오브젝트파티클에 정보전달.
    void WhenDropOneChangeImage(int slotNum, ObjectParticle objectParticleCs) 
    {
        print("Doing");
        GameObject nowUsingObject = InventoryStatic.instance.slots[slotNum];
        GameObject nowUsingObjectInQuick = InventoryPopup.instance.quickSlot[slotNum];
        ItemImage nowItemImage = nowUsingObject.GetComponentInChildren<ItemImage>();
        ItemImage nowItemImageInQuick = nowUsingObjectInQuick.GetComponentInChildren<ItemImage>();

        objectParticleCs.count = 1;
        TransferDataWithoutCnt(objectParticleCs, nowItemImage);
        nowItemImage.ChangeItemCnt(-1);
        nowItemImageInQuick.ChangeItemCnt(-1);


    }


    void TransferDataWithoutCnt(ObjectParticle objectParticleCs, ItemImage nowItemImage)
    {
        objectParticleCs.particleKind = nowItemImage.particleKind;
        objectParticleCs.icon = nowItemImage.itemImage.sprite;
    }
}
