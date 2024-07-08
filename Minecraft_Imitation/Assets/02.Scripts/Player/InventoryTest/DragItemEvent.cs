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

    GameObject slot;

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragingItem = gameObject;
        orgParent = transform.parent;
        print(orgParent.name);
        dragItemImage = GetComponent<ItemImage>();
        transform.SetParent(transform.parent.parent.parent);
        GetComponentInChildren<Image>().raycastTarget = false;
        GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(eventData.pointerCurrentRaycast.gameObject.layer != LayerMask.NameToLayer("Slot"))
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
        print("SwitchPos됨");
        previous.gameObject.transform.SetParent(orgParent);
        previous.gameObject.transform.localPosition = Vector3.zero;
    }

}
