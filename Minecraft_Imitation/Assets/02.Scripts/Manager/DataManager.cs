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

    public BlockMaterial[] blockMaterials;  // �� ���׸��� ����Ʈ
    public BlockData[] blockDatas;  // �� ������ ����Ʈ
    public Dictionary<BlockData.BlockKind, BlockData> blockDictionary = new Dictionary<BlockData.BlockKind, BlockData>(); // �� �����͸� �ǳ��� Dictionary

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
        // �÷��̾� ��ġ �� �������� �����ؾߵ�
        return Vector3.one;
    }
}
