using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static ObjectParticleData;


public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    #region Block Data Variable
    public BlockMaterial[] blockMaterials;  // 블럭 머테리얼 리스트
    public List<BlockData> blockDatas;  // 블럭 데이터 리스트
    public Dictionary<BlockData.BlockName, BlockData> blockDictionary = new Dictionary<BlockData.BlockName, BlockData>(); // 블럭 데이터를 건내줄 Dictionary
    #endregion

    #region Combination Data Variable
    public List<CombinationData> combinationDatas = new List<CombinationData>(); 
    private List<CombinationData> currectDataList = new List<CombinationData>();
    int inven_Y = 0;
    int inven_Check_Y = 1;
    int inven_X = 0;
    int inven_Check_X = 0;
    #endregion

    public Mob[] mobList;
    public Dictionary<MobData.MobKind, Mob> mobDictionary = new Dictionary<MobData.MobKind, Mob>();

    public ObjectParticle[] objectParticles_Block;
    public ObjectParticle[] objectParticles_Tool;
    public ObjectParticle[] objectParticles_Food;
    public Dictionary<ObjectParticleData.ParticleName, ObjectParticle> objectParticleDictionary = new Dictionary<ObjectParticleData.ParticleName, ObjectParticle>();

    private string particleKind_String;
    private BlockData.BlockName blockKind;

    int start_X = int.MaxValue;
    int end_X = int.MinValue;
    int startY = int.MaxValue;
    int endY = int.MinValue;
    bool check_Y = false;
    int count_Y = 1;

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

        InitData();

    }

    private void InitData()
    {
        // 블럭 데이터 초기화
        ReadBlockData();
        InitBlockData();
        // 파티클 데이터 초기화
        InitObjectParticleList();
        // 조합 데이터 초기화
        ReadCombinationData();
        // 몹 데이터 초기화
        InitMobList();
    }


    // Path를 통해 파일을 열고 텍스트 읽어오기 
    private string ReadTxt(string filePath)
    {
        FileStream reader = new FileStream(filePath, FileMode.Open);
        StreamReader data =  new StreamReader(reader);
        string data_str =  data.ReadToEnd();
        data.Close();
        return data_str;
    }

    #region 블럭

    // 블럭 데이터 파싱
    private void ReadBlockData()
    {
        string blcokData_txt = ReadTxt(Application.streamingAssetsPath + "/Data/BlockData.txt");
        string[] data_Lines = blcokData_txt.Split("\n");
        string[] data_Tap;
        for (int i = 1; i < data_Lines.Length-1; i++)
        {
            data_Tap = data_Lines[i].Split("\t");
            blockDatas.Add(new BlockData(
                (BlockData.BlockName)Enum.Parse(typeof(BlockData.BlockName), data_Tap[0]),
                (BlockData.BlockType)Enum.Parse(typeof(BlockData.BlockType), data_Tap[1]),
                float.Parse(data_Tap[2]),
                (Sound.AudioClipName)Enum.Parse(typeof(Sound.AudioClipName), data_Tap[3]),
                (Sound.AudioClipName)Enum.Parse(typeof(Sound.AudioClipName), data_Tap[4]),
                (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[5]))
                );
        }
    }


    // 블럭 데이터 Dictionary 설정
    private void InitBlockData()
    {
        for (int i = 0; i < blockDatas.Count; i++)
        {
            blockDictionary.Add(blockDatas[i].blockName, blockDatas[i]);
        }
        for (int i = 0; i < blockMaterials.Length; i++)
        {
            if (blockDictionary.ContainsKey(blockMaterials[i].blockKind))
            {
                blockDictionary[blockMaterials[i].blockKind].material = blockMaterials[i].material;
            }
        }
    }
#endregion


    #region 조합식

    // 조합식 데이터 파싱
    private void ReadCombinationData()
    {
        string blcokData_txt = ReadTxt(Application.streamingAssetsPath + "/Data/CombinationData.txt");
        string[] data_Lines = blcokData_txt.Split("\n");
        string[] data_Tap;
        for (int i = 1; i < data_Lines.Length - 1; i++)
        {
            data_Tap = data_Lines[i].Split("\t");
            combinationDatas.Add(
                new CombinationData(
                    (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[0]),
                    int.Parse(data_Tap[1]),
                    new List<ObjectParticleData.ParticleName>()
                    {
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[2]),
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[3]),
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[4]),
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[5]),
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[6]),
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[7]),
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[8]),
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[9]),
                        (ObjectParticleData.ParticleName)Enum.Parse(typeof(ObjectParticleData.ParticleName), data_Tap[10])
                    }
                )
            );

        }
    }


    public CombinationData GetCombinationData(List<ObjectParticleData.ParticleName> inven)
    {
        currectDataList.Clear();

        // x, y 축 검사
        start_X = int.MaxValue;
        end_X = int.MinValue;
        startY = int.MaxValue;
        endY = int.MinValue;
        check_Y = false;
        count_Y = 1;

        inven_X = 0;
        inven_Y = 0;
        for (int i = 0; i < inven.Count; i++)
        {
            if (inven[i] != ParticleName.None)
            {
                check_Y = true;
                inven_Y = count_Y;
                if ((i % 3) + 1 < start_X)
                {
                    start_X = i % 3 + 1;
                }
                if((i % 3) +1 > end_X)
                {
                    end_X = i % 3 + 1;
                }
            }

            if((i+1)%3 == 0)
            {
                if (check_Y)
                {
                    count_Y++;
                }
            }
        }
        print(start_X + ", " + end_X);
        if (start_X != int.MaxValue)
        {
            inven_X = end_X - start_X + 1;    
        }

        // x, y 비교
        for (int i = 0; i < combinationDatas.Count; i++)
        {
            if (combinationDatas[i].x == inven_X)
            {
                if (combinationDatas[i].y == inven_Y)
                {
                    currectDataList.Add(combinationDatas[i]);
                }
            }
        }
        for (int i = 0; i < currectDataList.Count; i++)
        {
            print(currectDataList[i].result + " : " + currectDataList[i].x + " , " + currectDataList[i].y);
        }
        if (currectDataList.Count == 0)
        {
            return null;
        }

        // None아닌 데이터 제일 앞으로 오게 정렬
        for (int i = 0; i < inven.Count; i++)
        {
            if (inven[i] != ObjectParticleData.ParticleName.None)
            {
                break;
            }
            else
            {
                inven.RemoveAt(i);
                i--;
            }
        }
        // Count가 9가 될때 까지 None 넣어줌
        while (inven.Count < 9)
        {
            inven.Add(ObjectParticleData.ParticleName.None);
        }

        // x, y 축이 맞는 데이터랑 enum 비교
        for (int i = 0; i < currectDataList.Count; i++)
        {
            for (int j = 0; j < currectDataList[i].particleKinds.Length; j++)
            {
                if (currectDataList[i].particleKinds[j] != inven[j]) // enum이 틀리면 부적합
                {
                    break;
                }
                else
                {
                    if(j >= 8) // 인덱스가 8까지 enum이 맞다면 적합
                    {
                        
                        return currectDataList[i]; // 적합 enum return
                    }
                }
            }
        }

        // 적합한게 없었으니 None
        return null;
    }
#endregion


    #region 오브젝트 파티클

    // 오브젝트 파티클 Dictionary 초기화
    private void InitObjectParticleList()
    {
        for (int i = 0; i < objectParticles_Block.Length; i++)
        {
            objectParticleDictionary.Add(objectParticles_Block[i].particleName, objectParticles_Block[i]);
        }

        for (int i = 0; i < objectParticles_Tool.Length; i++)
        {
            objectParticleDictionary.Add(objectParticles_Tool[i].particleName, objectParticles_Tool[i]);
        }

        for (int i = 0; i < objectParticles_Food.Length; i++)
        {
            objectParticleDictionary.Add(objectParticles_Food[i].particleName, objectParticles_Food[i]);
        }
        
    }
    
    // 오브젝트 파티클 가져오기
    public ObjectParticle GetObjectParticlePrefab(ObjectParticleData.ParticleName particleKind)
    {
        if (objectParticleDictionary.ContainsKey(particleKind))
        {
            return objectParticleDictionary[particleKind];
        }
        return null;
    }


    // 파티클로 블럭 데이터 가져오기
    public BlockData.BlockName ParticleToBlockKind(ObjectParticleData.ParticleName particleKind)
    {
        particleKind_String = particleKind.ToString();
        try
        {
            blockKind = (BlockData.BlockName)Enum.Parse(typeof(BlockData.BlockName), particleKind_String);
        }
        catch (ArgumentException error)
        {
            blockKind = BlockData.BlockName.None;
        }
        return blockKind;
    }

    #endregion


    #region 몹

    // Mob Dictionary 초기화
    private void InitMobList()
    {
        for (int i = 0; i < mobList.Length; i++)
        {
            mobDictionary.Add(mobList[i].mobKind, mobList[i]);
        }
    }

    // Getter
    public Mob GetMobPrefab(MobData.MobKind mobKind)
    {
        if (mobDictionary.ContainsKey(mobKind))
        {
            return mobDictionary[mobKind];
        }

        return null;
    }

    #endregion
}
