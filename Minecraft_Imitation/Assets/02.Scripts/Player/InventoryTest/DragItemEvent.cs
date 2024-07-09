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

    public void OnBeginDrag(PointerEventData eventData)
    {
        print(1);
        dragingItem = gameObject;
        orgParent = transform.parent;
        dragItemImage = GetComponent<ItemImage>();
        transform.SetParent(transform.parent.parent.parent);
        GetComponentInChildren<Image>().raycastTarget = false;
        GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        print(2);
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        print(3);
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
        print("SwitchPos됨");
        previous.gameObject.transform.SetParent(orgParent);
        previous.gameObject.transform.localPosition = Vector3.zero;
    }

    public Transform SwitchPos()
    {
        print(6);
        return orgParent;
    }

}
