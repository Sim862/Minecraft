using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryStatic : MonoBehaviour
{
    public static InventoryStatic instance;
    public GameObject[] slots = new GameObject[9]; // 퀵슬롯 배열.
    public GameObject test;
    public GameObject highlight;
    int totalCnt;
    public int exceededCnt;
    int maxCnt = 64;

    

    ItemImage previous;
    ItemImage now;


    // Start is called before the first frame update
    void Awake()
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

    // Update is called once per frame
    void Update()
    {
        Highlight();
    }


    public void SetItemPosition(GameObject item)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount == 5)
            {
                // slot에 있는 아이템이미지의 정보를 가져와
                ItemImage itemInSlot = slots[i].GetComponentInChildren<ItemImage>();
                
                if (itemInSlot.count == 64) continue;
                ItemImage itemImage = item.GetComponentInChildren<ItemImage>();
                if(itemImage.particleKind == itemInSlot.particleKind)
                {
                    if(itemInSlot.count + itemImage.count <= maxCnt) // maxCnt == 64
                    {
                        // 우선 숫자 증가시켜.
                        itemInSlot.ChangeItemCnt(itemImage.count); // 주운 개수만큼 증가.
                        InventoryPopup.instance.ChangeSameCnt(i, itemInSlot.count);
                        break;
                    }
                    else // 초과할경우
                    {
                        totalCnt = itemInSlot.count + itemImage.count;
                        exceededCnt = totalCnt - maxCnt; // 초과양.
                        itemInSlot.ChangeItemCnt(maxCnt - itemInSlot.count);
                        itemImage.ChangeItemCnt(exceededCnt - itemImage.count);
                        InventoryPopup.instance.ChangeSameCnt(i, itemInSlot.count);
                        
                    }
                }
            }
            else if (slots[i].transform.childCount == 4)
            {
                item.transform.parent = slots[i].transform;
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;
                GameObject item2 = Instantiate(item);
                InventoryPopup.instance.SetItemPositionInQuickSlot(item2, i);
                return;
            }
            if(i == slots.Length - 1)
            {
                InventoryPopup.instance.SetItemPosInInven(item);
            }
        }
    }

    public void Highlight()
    {
        highlight.transform.localPosition = slots[PlayerManager.instance.usingSlot].transform.localPosition;
    }

    public void SetVisualObject()
    {
        // 아이템을 얻으면 아이템 정보를 저장.
        // 아이템이 사라지면 아이템 정보를 previous에 저장하고
        //
    }


    public void CheckIsUsing(int i, bool isUsing)
    {
        SlotStatic slotStatic = slots[i].GetComponent<SlotStatic>();
        slotStatic.isUsing = isUsing;
    }
}
