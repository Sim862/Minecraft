using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MakingSlot : MonoBehaviour
{
    public GameObject[] dropSlot = new GameObject[9];
    public ObjectParticleData.ParticleName[] particleNames = new ObjectParticleData.ParticleName[9];
    public GameObject takeSlot;
    CombinationData combinationData;
    public GameObject itemImagePref;
    GameObject itemImage;
    ItemImage itemImageCs;
    GameObject inTakeSlotGo;
    public GameObject woodenAxeFac;

    // Start is called before the first frame update
    void Awake()
    {
        takeSlot = transform.GetChild(transform.childCount - 1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SaveAllData()
    {
        
        for(int i = 0; i < dropSlot.Length; i++)
        {
            ItemImage item = null;
            
            if (dropSlot[i] == null) // 4칸짜리일 경우 오류방지.
            {
                continue;
            }
            if (dropSlot[i].transform.childCount != 0)
            {
                item = dropSlot[i].GetComponentInChildren<ItemImage>();
            }
            if(item != null)
            {
                particleNames[i] = item.particleName;
            }
            else if(item == null)
            {
                particleNames[i] = ObjectParticleData.ParticleName.None;
            }
            //print("내 " +i+ "번째 슬롯 자식수는 :  "+dropSlot[i].transform.childCount);
        }
        TransferData();
    }

    void TransferData()
    {
        combinationData = DataManager.instance.GetCombinationData(particleNames.ToList());
        if(combinationData != null)
        {
            if (takeSlot.transform.childCount != 0)
            {
                return;
            }
            else if(takeSlot.transform.childCount == 0)
            {
                itemImage = Instantiate(itemImagePref, takeSlot.transform);
                itemImage.transform.localPosition = Vector3.zero;
                itemImageCs = itemImage.GetComponent<ItemImage>();
                itemImageCs.particleName = combinationData.result;
                itemImageCs.particleType = DataManager.instance.GetObjectParticlePrefab(itemImageCs.particleName).particleType;
                itemImageCs.itemImage.sprite = DataManager.instance.GetObjectParticlePrefab(combinationData.result).icon;
                if(itemImageCs.particleType == ObjectParticleData.ParticleType.Tool && itemImageCs.particleName != ObjectParticleData.ParticleName.Arrow)
                {
                    itemImageCs.count = 1;
                    Destroy(itemImageCs.itemCount.gameObject);
                    if(itemImageCs.particleName == ObjectParticleData.ParticleName.WoodenPickaxe)
                    {
                        GameObject go = Instantiate(woodenAxeFac.transform.GetChild(0).gameObject, PlayerManager.instance.pickPos.transform);
                        go.transform.localPosition = Vector3.zero + new Vector3(0, -0.1f, 0);
                        go.transform.localEulerAngles = new Vector3(16, 160, 5); 
                        itemImageCs.particleObjectTr = go.transform;
                        itemImageCs.particleObjectTr.gameObject.SetActive(false);
                    }
                }
                else
                {
                    itemImageCs.ChangeItemCnt(combinationData.count);
                }
                itemImageCs.wasInTakeSlot = true;
            }
            
        }
        else
        {
            if(takeSlot.transform.childCount != 0)
            {
                Destroy(takeSlot.transform.GetChild(0).gameObject);
            }
        }
    }



}
