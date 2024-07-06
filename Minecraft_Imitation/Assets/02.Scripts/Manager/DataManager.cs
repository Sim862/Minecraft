using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public BlockMaterial[] blockMaterials;  // 블럭 머테리얼 리스트
    public List<BlockData> blockDatas;  // 블럭 데이터 리스트
    public Dictionary<BlockData.BlockKind, BlockData> blockDictionary = new Dictionary<BlockData.BlockKind, BlockData>(); // 블럭 데이터를 건내줄 Dictionary

    public Mob[] monsterList;
    public Dictionary<MobData.MobKind, Mob> monsterDictionary = new Dictionary<MobData.MobKind, Mob>();

    public Mob[] passiveMobList;
    public Dictionary<MobData.MobKind, Mob> passiveMobDictionary = new Dictionary<MobData.MobKind, Mob>();

    public ObjectParticle[] objectParticles;
    public Dictionary<ObjectParticleData.ParticleKind, ObjectParticle> objectParticleDictionary = new Dictionary<ObjectParticleData.ParticleKind, ObjectParticle>();

    private string particleKind_String;
    private BlockData.BlockKind blockKind;
    
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

        ReadBlockData();
        InitBlockData();
        InitObjectParticleList();

    }

    string ReadTxt(string filePath)
    {
        FileStream reader = new FileStream(filePath, FileMode.Open);
        StreamReader data =  new StreamReader(reader);
        string data_str =  data.ReadToEnd();
        data.Close();
        return data_str;
    }

    private void ReadBlockData()
    {
        string blcokData_txt = ReadTxt(Application.dataPath + "/11.Data/BlockData.txt");
        string[] data_Lines = blcokData_txt.Split("\n");
        string[] data_Tap;
        for (int i = 1; i < data_Lines.Length-1; i++)
        {
            data_Tap = data_Lines[i].Split("\t");
            blockDatas.Add(new BlockData(
                (BlockData.BlockKind)Enum.Parse(typeof(BlockData.BlockKind), data_Tap[0]),
                (BlockData.BlockType)Enum.Parse(typeof(BlockData.BlockType), data_Tap[1]),
                float.Parse(data_Tap[2]),
                (Sound.AudioClipName)Enum.Parse(typeof(Sound.AudioClipName), data_Tap[3]),
                (Sound.AudioClipName)Enum.Parse(typeof(Sound.AudioClipName), data_Tap[4]),
                (ObjectParticleData.ParticleKind)Enum.Parse(typeof(ObjectParticleData.ParticleKind), data_Tap[5]))
                );
        }
    }


    private void InitBlockData()
    {
        for (int i = 0; i < blockDatas.Count; i++)
        {
            print(blockDatas[i].blockKind);
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

    private void InitObjectParticleList()
    {
        for (int i = 0; i < objectParticles.Length; i++)
        {
            objectParticleDictionary.Add(objectParticles[i].particleKind, objectParticles[i]);
        }
    }

    public ObjectParticle GetObjectParticlePrefab(ObjectParticleData.ParticleKind particleKind)
    {
        if (objectParticleDictionary.ContainsKey(particleKind))
        {
            return objectParticleDictionary[particleKind];
        }
        return null;
    }

    public BlockData.BlockKind ParticleToBlockKind(ObjectParticleData.ParticleKind particleKind)
    {
        particleKind_String = particleKind.ToString();
        try
        {
            blockKind = (BlockData.BlockKind)Enum.Parse(typeof(BlockData.BlockKind), particleKind_String);
        }
        catch (ArgumentException error)
        {
            blockKind = BlockData.BlockKind.None;
        }
        return blockKind;
    }
}
