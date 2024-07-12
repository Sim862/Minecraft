using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItemEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject dragingItem;
    public static ItemImage dragItemImage;
    Transform orgParent;
    Transform dragingParent;
    Transform startPos;
    GameObject nowMakeSlots;
    MakingSlot makingSlot;


    public void OnBeginDrag(PointerEventData eventData)
    {
        dragingItem = gameObject;
        orgParent = transform.parent;
        dragItemImage = GetComponent<ItemImage>();
        transform.SetParent(transform.parent.parent.parent.parent);
        GetComponentInChildren<Image>().raycastTarget = false;
        GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
        nowMakeSlots = InventoryPopup.instance.nowMakeSlots.transform.GetChild(1).gameObject;
        makingSlot = nowMakeSlots.GetComponent<MakingSlot>();
        makingSlot.SaveAllData();
        if (dragItemImage.wasInTakeSlot)
        {
            DecreaseInMakeSlot();
            dragItemImage.wasInTakeSlot = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.layer != LayerMask.NameToLayer("Slot") && eventData.pointerCurrentRaycast.gameObject.layer != LayerMask.NameToLayer("ItemImage"))
        {
            transform.SetParent(orgParent);
            transform.localPosition = Vector3.zero;
        }
        GetComponentInChildren<Image>().raycastTarget = true;
        GetComponentInChildren<TextMeshProUGUI>().raycastTarget = true;
        InventoryPopup.instance.CheckQuickSlot();
    }


    public void SwitchPos(ItemImage previous) // 이미지아이템 위치 바꾸기.
    {
        previous.gameObject.transform.SetParent(orgParent);
        previous.gameObject.transform.localPosition = Vector3.zero;
    }

    public Transform SwitchPos()
    {
        return orgParent;
    }

    void DecreaseInMakeSlot()
    {
        for(int i = 0; i < makingSlot.dropSlot.Length; i++)
        {
            ItemImage item = null;
            if (makingSlot.dropSlot[i] == null)
            {
                continue;
            }
            if (makingSlot.dropSlot[i].transform.childCount != 0)
            {
                item = makingSlot.dropSlot[i].GetComponentInChildren<ItemImage>();
                item.ChangeItemCnt(-1);
            }
        }
    }

}
