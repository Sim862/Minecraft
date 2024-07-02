using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public BlockMaterial[] blockMaterials;  // 블럭 머테리얼 리스트
    public BlockData[] blockDatas;  // 블럭 데이터 리스트
    public Dictionary<BlockData.BlockKind, BlockData> blockDictionary = new Dictionary<BlockData.BlockKind, BlockData>(); // 블럭 데이터를 건내줄 Dictionary

    public Entity[] monsterList;
    public Dictionary<MobData.MobKind, Entity> monsterDictionary = new Dictionary<MobData.MobKind, Entity>();

    public Entity[] passiveMobList;
    public Dictionary<MobData.MobKind, Entity> passiveMobDictionary = new Dictionary<MobData.MobKind, Entity>();

    public ObjectParticle[] objectParticles;
    public Dictionary<ObjectParticleData.ParticleKind, ObjectParticle> objectParticleDictionary = new Dictionary<ObjectParticleData.ParticleKind, ObjectParticle>();


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
}
