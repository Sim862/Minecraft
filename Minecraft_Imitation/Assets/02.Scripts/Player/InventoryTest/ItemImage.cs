using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemImage : MonoBehaviour
{
    public int count;
    public Image itemImage;
    public TextMeshProUGUI itemCount;
    public ObjectParticleData.ParticleName particleName;
    public ObjectParticleData.ParticleType particleType;
    public Transform particleObjectTr;
    public bool wasInTakeSlot;
    public bool isPopup = false;

    private void Start()
    {
        if(GetComponentInChildren<TextMeshProUGUI>() != null)
        GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
    }

    public void ChangeItemCnt(int value) // 설치하는 경우 사용. 하나만 쓰기때문.
    {
        count += value;
        if (count < 1)
        {
            if (particleObjectTr != null) 
            { 
                Destroy(particleObjectTr.gameObject);
                particleObjectTr = null;
            }
            Destroy(gameObject);
            return;
        }
        itemCount.text = $"{count}";
    }

    

    public void DuplicateData(ItemImage itemImage)
    {
        itemImage.count = count;
        itemImage.itemImage = this.itemImage;
        itemImage.itemCount = itemCount;
        itemImage.isPopup = isPopup;
        itemImage.particleName = particleName;
        itemImage.particleObjectTr = particleObjectTr; 
        itemImage.wasInTakeSlot = wasInTakeSlot;
    }

    /// <summary>
    /// 우클릭으로 Drag 시작할때 부르는 함수. A.Dup(instance);로 A의 정보를 전달과 동시에 카운트를 반으로 함.
    /// </summary>
    /// <param name="itemImage"></param>
    public void DuplicateDataAndHalfCnt(ItemImage itemImage) 
    {
        DuplicateData(itemImage);
        if (itemImage.count > 1)
        {
            itemImage.ChangeItemCnt(-(itemImage.count / 2));
        }
        else if (itemImage.count <= 1) 
        {
            particleObjectTr = null;
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (isPopup && particleObjectTr != null)
        {
            Destroy(particleObjectTr.gameObject);
        }
    }
}
