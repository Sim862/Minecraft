using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlot : MonoBehaviour
{
    ItemImage item;
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
    }
}
