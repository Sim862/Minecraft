using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPopup : MonoBehaviour
{
    public static InventoryPopup instance;
    public GameObject[] quickSlot = new GameObject[0];
    public GameObject[] inven = new GameObject[0];
    int usingSlot;
    GameObject staticGo;
    GameObject quickGo;
    Transform staticItem;
    Transform quickItem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeSameCnt(int slotNum, int changeToValue)  // 증감 하는 함수.
    {
        ItemImage pop = quickSlot[slotNum].GetComponentInChildren<ItemImage>();
        pop.ChangeItemCnt(-pop.count + changeToValue); 
    }


    public void SetItemPositionInQuickSlot(GameObject item, int i)
    {
        item.transform.parent = quickSlot[i].transform;
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
    }

    public void SetItemPosInInven(GameObject item) // Quick슬롯에 자리 없을때 들어오게.
    {
        for(int i =0; i< inven.Length; i++)
        {
            if (inven[i].transform.childCount == 0)
            {
                item.transform.parent = inven[i].transform;
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;
                return;
            }
        }
    }

    public void CheckQuickSlot() // 퀵슬롯과 static슬롯 동기화 함수.
    {
        // Drag로 인해 아이템이 생기거나 사라질때
        // InvenStatic에도 이미지 및 데이터 동기화.
        for(int i = 0; i < 9; i++)
        {
            if (InventoryStatic.instance.slots[i].transform.childCount != 4) // 이미지가 있으면 저장.
            {
                staticItem = InventoryStatic.instance.slots[i].transform.GetChild(4);
                staticGo = staticItem.gameObject;
            }
            if (quickSlot[i].transform.childCount == 1) // 퀵슬롯에 있으면
            {
                // 똑같은 이미지 생성.
                quickItem = instance.quickSlot[i].transform.GetChild(0);
                quickGo = quickItem.gameObject;
                if (InventoryStatic.instance.slots[i].transform.childCount != 4) // 이미 static에 있으면
                {
                    ItemImage quick = quickGo.GetComponent<ItemImage>();
                    ItemImage staticCs = staticGo.GetComponent<ItemImage>();
                    if(quick.particleKind == staticCs.particleKind) // 종류가 같으면
                    {
                        staticCs.ChangeItemCnt(quick.count-staticCs.count); // quick.count값으로 교체
                    }
                    else // 종류가 다르면
                    {
                        staticCs.particleKind = quick.particleKind;
                        staticCs.itemImage.sprite = quick.itemImage.sprite;
                        staticCs.ChangeItemCnt(quick.count - staticCs.count);
                    }
                }
                else if(InventoryStatic.instance.slots[i].transform.childCount == 4) // static에 아이템 없으면
                {
                    GameObject sameImage = Instantiate(quickGo, InventoryStatic.instance.slots[i].transform);
                    sameImage.transform.localPosition = Vector3.zero;
                }
            }
            else if (quickSlot[i].transform.childCount == 0) // 퀵슬롯에 아이템이 없으면
            {
                Destroy(staticGo); // static 슬롯도 없애버림.
            }
            staticItem = null; // 초기화
            staticGo = null;
        }
    }

}
