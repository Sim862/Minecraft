using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotStatic : MonoBehaviour
{
    // 이거 사용 안함.
    ItemImage item;
    public GameObject particle;
    public bool isUsing;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HighLight();
    }

    void HighLight()
    {
        if(transform.childCount != 4)
        {
            
            item = transform.GetComponentInChildren<ItemImage>();
            item.isPopup = false;
            if (item != null && item.particleObjectTr != null)
            {
                particle = item.particleObjectTr.gameObject;
                if (isUsing)
                {
                    item.particleObjectTr.gameObject.SetActive(true);
                }
                else if (!isUsing)
                {
                    item.particleObjectTr.gameObject.SetActive(false);
                }
            }
        }
        else if(transform.childCount == 4)
        {
            item = null;
        }
    }
}
