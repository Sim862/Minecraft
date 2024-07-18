using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGetItem : MonoBehaviour
{

    ObjectParticleData.ParticleName particleName; // 필드에 있는 오브젝트의 종류,개수,아이콘 가져옴.

    Sprite icon;

    public GameObject itemImagePref;
    public GameObject gripPos;
    GameObject itemImage;
    ItemImage itemImageCs;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("ObjectParticle"))
        {
            ObjectParticle objectParticleCs = other.transform.GetComponentInParent<ObjectParticle>();

            Destroy(other.transform.parent.gameObject);

            itemImage = Instantiate(itemImagePref); // 아이템UI
            itemImageCs = itemImage.GetComponent<ItemImage>();

            itemImageCs.itemImage.sprite = objectParticleCs.icon; ;
            itemImageCs.particleName = objectParticleCs.particleName;
            itemImageCs.particleType = objectParticleCs.particleType;
            itemImageCs.particleObjectTr = objectParticleCs.particleObject;
            if(itemImageCs.particleName == ObjectParticleData.ParticleName.Bow)
            {
                Destroy(itemImageCs.itemCount.gameObject);
            }
            else
            {
                itemImageCs.ChangeItemCnt(objectParticleCs.count);

            }

            InventoryStatic.instance.SetItemPosition(itemImage);
        }

        if(other.gameObject.tag == "Arrow")
        {
            if (other.GetComponent<Arrow>().canPickUp == false) return;
            itemImage = Instantiate(itemImagePref); // 아이템UI
            itemImageCs = itemImage.GetComponent<ItemImage>();

            ObjectParticle ob = DataManager.instance.GetObjectParticlePrefab(ObjectParticleData.ParticleName.Arrow);

            itemImageCs.itemImage.sprite = ob.icon;
            itemImageCs.ChangeItemCnt(1);
            itemImageCs.particleName = ob.particleName;
            itemImageCs.particleType = ob.particleType;
            itemImageCs.particleObjectTr = Instantiate(ob.gameObject.transform.GetChild(0).gameObject).transform;

            InventoryStatic.instance.SetItemPosition(itemImage);

            Destroy(other.gameObject);
        }

    }

    
}
