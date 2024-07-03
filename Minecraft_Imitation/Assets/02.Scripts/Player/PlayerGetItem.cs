using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGetItem : MonoBehaviour
{

    ObjectParticleData.ParticleKind particleKind; // 필드에 있는 오브젝트의 종류,개수,아이콘 가져옴.
    int count;
    Sprite icon;

    public GameObject itemImagePref;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("ObjectParticle"))
        {
            GameObject objectParticle = other.transform.parent.gameObject;
            ObjectParticle objectParticleCs = objectParticle.GetComponent<ObjectParticle>();
            particleKind = objectParticleCs.particleKind;
            count = objectParticleCs.count;
            icon = objectParticleCs.icon;

            Destroy(other.transform.parent.gameObject);

            GameObject itemImage = Instantiate(itemImagePref);

            ItemImage itemImageCs = itemImage.GetComponent<ItemImage>();
            itemImageCs.itemImage.sprite = icon;
            itemImageCs.count = count;
            itemImageCs.itemCount.text = $"{count}";
            itemImageCs.particleKind = particleKind;

            InventoryStatic.instance.SetItemPosition(itemImage);
        }
    }
}
