using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class BlockMaterial
{
    public BlockData.BlockKind blockKind;
    public Material material;
}

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    public BlockMaterial[] blockMaterials;
    public BlockData[] blockDatas;

    public Dictionary<BlockData.BlockKind, BlockData> blockDictionary = new Dictionary<BlockData.BlockKind, BlockData>();


    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }


        InitBlockData();
    }

    private void InitBlockData()
    {
        for (int i = 0; i < blockDatas.Length; i++)
        {
            blockDictionary.Add(blockDatas[i].blockKind, blockDatas[i]);
        }
        for (int i = 0; i < blockMaterials.Length; i++)
        {
            if (blockDictionary.ContainsKey(blockMaterials[i].blockKind))
            {
                blockDictionary[blockMaterials[i].blockKind].material = blockMaterials[i].material;
            }
        }
    }
}
