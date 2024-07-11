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
    public ObjectParticleData.ParticleKind particleKind;
    public Transform particleObjectTr;
    public bool wasInTakeSlot;


    public void ChangeItemCnt(int value) // 설치하는 경우 사용. 하나만 쓰기때문.
    {
        count += value;
        if (count < 1)
        {
            Destroy(gameObject);
            return;
        }
        
        itemCount.text = $"{count}";
    }

    private void OnDestroy()
    {
        Destroy(particleObjectTr.gameObject);
    }
}
