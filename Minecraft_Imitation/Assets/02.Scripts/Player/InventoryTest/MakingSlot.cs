using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakingSlot : MonoBehaviour
{
    public GameObject[] dropSlot = new GameObject[9];
    public List<ObjectParticleData.ParticleKind> particleKinds = new List<ObjectParticleData.ParticleKind>();
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

    void TransferData()
    {
        //DataManager.instance.GetCombinationData(particleKinds);
    }



}
