using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakingSlot : MonoBehaviour
{
    public GameObject[] dropSlot = new GameObject[9];
    public ObjectParticleData.ParticleKind[] particleKinds = new ObjectParticleData.ParticleKind[9];
    public GameObject takeSlot;
    // Start is called before the first frame update
    void Awake()
    {
        for(int i =0; i < transform.childCount-1; i++)
        {
            dropSlot[i] = transform.GetChild(i).gameObject;
        }
        takeSlot = transform.GetChild(transform.childCount - 1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {



    }

    void SaveAllData()
    {
        for(int i = 0; i < dropSlot.Length; i++)
        {
            ItemImage item = dropSlot[i].GetComponentInChildren<ItemImage>();
            if(item != null)
            {
                particleKinds[i] = item.particleKind;
            }
            else if(item == null)
            {
                particleKinds[i] = ObjectParticleData.ParticleKind.None;
            }
        }
    }

    void TransferData()
    {
        //DataManager.instance.GetCombinationData(particleKinds);
    }



}
