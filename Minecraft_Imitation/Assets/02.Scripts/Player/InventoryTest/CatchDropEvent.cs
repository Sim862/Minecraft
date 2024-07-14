using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatchDropEvent : MonoBehaviour, IDropHandler
{
    DropSlot dropSlot;
    ItemImage previous;
    ItemImage following;
    bool checkKind;
    int totalCnt;
    int exceededCnt;
    int maxCnt = 64;
    private void Start()
    {
        dropSlot = GetComponent<DropSlot>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        print("드랍이벤트 받음");
        following = DragItemEvent.dragItemImage;
        previous = GetComponentInChildren<ItemImage>();
        if (transform.childCount == 0) // 그 칸에 아무것도 없으면 자리만 바꿔줌.
        {
            DragItemEvent.dragingItem.transform.SetParent(transform);
            DragItemEvent.dragingItem.transform.localPosition = Vector3.zero;
        }
        else // 그 칸에 뭐가 있으면
        {
            CheckKind(previous, following);
            CalculateCount(previous, following);
        }
        if(dropSlot != null)
        {
            dropSlot.TransferData();
        }
    }


    bool CheckKind(ItemImage previous, ItemImage following) // 종류 같은지 검사.
    {
        if (previous.particleKind == following.particleKind)
        {
            return checkKind = true;
        }
        else
        {
            return checkKind = false;
        }
    }

    void CalculateCount(ItemImage previous, ItemImage following) // 카운트 계산.
    {
        if (checkKind) // 종류가 같다면. //0708 진행중.
        {
            if (previous.count + following.count <= maxCnt) // maxCnt == 64
            {
                // 우선 숫자 증가시켜.
                previous.ChangeItemCnt(following.count); // 옮긴느 개수만큼.
                Destroy(following.gameObject); // 옮기던거 파괴.
                InventoryPopup.instance.CheckQuickSlot(); // 파괴하면 Check가 안되서 여기서 직접해줌.
            }
            else // 초과할경우
            {
                totalCnt = previous.count + following.count;
                exceededCnt = totalCnt - maxCnt; // 초과양.
                previous.ChangeItemCnt(maxCnt - previous.count);
                following.ChangeItemCnt(exceededCnt - following.count);

                DragItemEvent followingCs = following.GetComponent<DragItemEvent>();
                following.transform.SetParent(followingCs.SwitchPos());
                following.transform.localPosition = Vector3.zero;
            }
        }
        else // 종류가 다르다면 위치만 서로 바꿔준다.
        {
            //previous = transform.GetChild(0).GetComponent<ItemImage>();
            DragItemEvent followingCs = following.GetComponent<DragItemEvent>();
            previous.transform.SetParent(followingCs.SwitchPos());
            previous.transform.localPosition = Vector3.zero;
            DragItemEvent.dragingItem.transform.SetParent(transform);
            DragItemEvent.dragingItem.transform.localPosition = Vector3.zero;
            
        }
    }


}
