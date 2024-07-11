using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSlot : MonoBehaviour
{
    ItemImage itemImage;
    MakingSlot makingSlot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TransferData()
    {
        if(transform.childCount != 0)
        {
            //itemImage = GetComponentInChildren<ItemImage>();
            makingSlot = GetComponentInParent<MakingSlot>();
            makingSlot.SaveAllData();
        }
    }
}
