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
        TransferData();
    }

    public void TransferData()
    {
        
        if(transform.childCount != 0)
        {
            print("transferData 실행");
            print("자식 수 : "+transform.childCount);
            print(transform.GetChild(0).gameObject.name);
            //itemImage = GetComponentInChildren<ItemImage>();
            makingSlot = GetComponentInParent<MakingSlot>();
            makingSlot.SaveAllData();
        }
    }
}
