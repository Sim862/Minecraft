using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerData
{
    public Vector3 spawnPosition;
}

[System.Serializable]
public class BlockMaterial
{
    public BlockData.BlockKind blockKind;
    public Material material;
}

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    public PlayerData playerData = new PlayerData(); 

    public BlockMaterial[] blockMaterials;  // 블럭 머테리얼 리스트
    public BlockData[] blockDatas;  // 블럭 데이터 리스트
    public Dictionary<BlockData.BlockKind, BlockData> blockDictionary = new Dictionary<BlockData.BlockKind, BlockData>(); // 블럭 데이터를 건내줄 Dictionary

    public Entity[] monsterList;
    public Entity[] passiveMobList;
    public Dictionary<MobData.MobKind, Entity> monsterDictionary = new Dictionary<MobData.MobKind, Entity>();
    public Dictionary<MobData.MobKind, Entity> passiveMobDictionary = new Dictionary<MobData.MobKind, Entity>();


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

    public Vector3 PlayerPosition()
    {
        // 플레이어 위치 값 리턴으로 수정해야됨
        return Vector3.one;
    }
}
