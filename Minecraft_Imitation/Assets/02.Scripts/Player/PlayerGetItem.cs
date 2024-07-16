using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGetItem : MonoBehaviour
{

    ObjectParticleData.ParticleName particleKind; // 필드에 있는 오브젝트의 종류,개수,아이콘 가져옴.
    int count;
    Sprite icon;

    public GameObject itemImagePref;
    public GameObject gripPos;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("ObjectParticle"))
        {
            GameObject objectParticle = other.transform.parent.gameObject;
            ObjectParticle objectParticleCs = objectParticle.GetComponent<ObjectParticle>();
            particleKind = objectParticleCs.particleName;
            count = objectParticleCs.count;
            icon = objectParticleCs.icon;

            Destroy(other.transform.parent.gameObject);

            GameObject itemImage = Instantiate(itemImagePref); // 아이템UI

            ItemImage itemImageCs = itemImage.GetComponent<ItemImage>();
            itemImageCs.itemImage.sprite = icon;
            itemImageCs.count = count;
            itemImageCs.itemCount.text = $"{count}";
            itemImageCs.particleKind = particleKind;
            itemImageCs.particleObjectTr = objectParticleCs.particleObject;
            //itemImageCs.particleObjectTr.SetParent(gripPos.transform);
            //itemImageCs.particleObjectTr.localPosition = Vector3.zero;
            InventoryStatic.instance.SetItemPosition(itemImage);
        }
    }
}
