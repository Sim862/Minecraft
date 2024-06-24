using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static BlockData;


public class Chunk
{
    public static readonly int x = 12;
    public static readonly int y = 256;
    public static readonly int z = 12;
    public Chunk(Vector2 position, int[,,] blocks)
    {
        this.position = position;
        this.blocksEnum = blocks;
    }

    public Vector3 position = new Vector3();
    public int[,,] blocksEnum = new int[x, y, z]; // x, y, z
    public Block[,,] blockObjects = new Block[x, y, z]; // x, y, z
}

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    public Block blockPrefab;

    private Chunk[] chunks = new Chunk[10];
    public int[,,] blocks = new int[Chunk.x, Chunk.y, Chunk.z]; // x, y, z
    private BlockData blockData;
    private BlockData.BlockKind blockKind;
    private Block block;
    private Vector3 position1 = new Vector3();
    private Vector3 position2 = new Vector3();
    Array enumValues = System.Enum.GetValues(enumType: typeof(BlockData.BlockKind));
   
    int value;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        Load10Chunk();
        for (int i = 0; i < chunks.Length; i++)
        {
            CreateChunk(chunks[i]);
        }
    }

    #region Chunk 메서드

    public void Load10Chunk()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            LoadChunk("Chunk" + i + "_" + 0);
            chunks[i] = new Chunk(new Vector3(i, 0, 0), blocks);
        }
    }

    public void Save10Chunk()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            SaveChunk("Chunk" + i + "_" + 0);
        }
    }

    public void LoadChunk(string path)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.dataPath + "/" + path + ".binary", FileMode.Open);

        // 파일이 없으면 평지로 설정
        if (file == null)
        {
            for (int i = 0; i < Chunk.x; i++)
            {
                for (int j = 0; j < Chunk.y; j++)
                {
                    if (j < 60)
                        value = (int)BlockData.BlockKind.Dirt;
                    else
                        value = (int)BlockData.BlockKind.None;

                    for (int k = 0; k < Chunk.z; k++)
                    {
                        blocks[i, j, k] = value;
                    }

                }

            }
        }
        else
        {
            blocks = (int[,,])bf.Deserialize(file);
            file.Close();
        }
        sw.Stop();
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
    }

    public void SaveChunk(string path)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < Chunk.x; i++)
        {
            for (int j = 0; j < Chunk.y; j++)
            {
                if (j < 50)
                    value = (int)BlockData.BlockKind.Dirt;
                else
                    value = (int)BlockData.BlockKind.None;

                for (int k = 0; k < Chunk.z; k++)
                {
                    blocks[i, j, k] = value;
                }

            }

        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/" + path + ".binary");
        bf.Serialize(file, blocks);
        file.Close();
        sw.Stop();
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
    }


    #endregion



    public void CreateChunk(Chunk chunk)
    {
        blocks = chunk.blocksEnum;
        position1 = new Vector3(chunk.position.x * Chunk.x, -60, chunk.position.z * Chunk.z);
        position2 = new Vector3(position1.x, position1.y, position1.z);

        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                for (int z = 0; z < Chunk.z; z++)
                {
                    blockKind = (BlockData.BlockKind)blocks[x, y, z];
                    if (blockKind != 0)
                    {
                        blockData = DataManager.instance.blockDictionary[blockKind];
                        block = Instantiate(blockPrefab, position2, Quaternion.identity, transform);
                        block.InitBlock(blockData);
                        chunk.blockObjects[x, y, z] = block;
                    }
                    position2+= Vector3.forward;
                }
                position2 = new Vector3(position2.x, position2.y, position1.z);
                position2 += Vector3.up;
            }
            position2 = new Vector3(position2.x, position1.y, position2.z);
            position2 += Vector3.right;
        }
    }

}
