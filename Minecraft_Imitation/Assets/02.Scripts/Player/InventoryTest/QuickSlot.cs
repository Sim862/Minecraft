using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlot : MonoBehaviour
{
    ItemImage item;
    bool itemExisted;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.childCount != 0)
        {
            item = GetComponentInChildren<ItemImage>();
            item.isPopup = true;
        }
        if(transform.childCount == 0 && item != null)
        {
            itemExisted = true;
            if (itemExisted)
            {
                item.particleObjectTr.gameObject.SetActive(false);
                itemExisted = false;
            }
        }
    }
}
