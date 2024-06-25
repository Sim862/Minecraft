using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static BlockData;


public class Chunk
{
    public static readonly float saveTime = 12; // ûũ ���� ���ð�

    // ûũ ũ��
    public static readonly int x = 12;
    public static readonly int y = 256;
    public static readonly int z = 12;

    // ������
    public Chunk(Vector3 position, int[,,] blocks)
    {
        this.position = position;
        this.blocksEnum = blocks;
        blockParent = new GameObject(position.ToString()).transform;
        blockParent.SetParent(MapManager.instance.transform);
        saveRoutine = Coroutine_SaveChunk(this);
    }

    // ������ �� ������Ʈ �θ�
    public Transform blockParent;
    public Vector3 position = new Vector3(); // transform.postion �ƴ� ûũ ��ġ�� ���ϸ�
    public int[,,] blocksEnum = new int[x, y, z]; // x, y, z
    public Block[,,] blockObjects = new Block[x, y, z]; // x, y, z

    // �� ������� �ִ��� üũ
    public bool needSave = false;

    // �ڷ�ƾ
    public IEnumerator saveRoutine;

    // ��������� ����� 12�� ���� ����
    private IEnumerator Coroutine_SaveChunk(Chunk chunk)
    {
        while (true)
        {
            if (chunk.needSave)
            {
                chunk.needSave = false;
                MapManager.instance.SaveChunk("Chunk" + ((int)chunk.position.x + "_" + ((int)chunk.position.z)),this);
            }

            yield return new WaitForSeconds(Chunk.saveTime);
        }
    }
}

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    public Block blockPrefab;

    private Vector3 playerChunckVector; // transform.postion �ƴ� ûũ ��ġ�� ���ϸ�
    private Chunk[] chunks = new Chunk[9];
    public int[,,] blocks = new int[Chunk.x, Chunk.y, Chunk.z]; // x, y, z
    private BlockData blockData;
    private BlockData.BlockKind blockKind;
    private Block block;
    private Vector3 blockPosition = new Vector3();
    private Vector3 position2 = new Vector3();
    Array enumValues = System.Enum.GetValues(enumType: typeof(BlockData.BlockKind));

    private int value;

    int index_x, index_y, index_z;

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
        Load_9Chunks();
        for (int i = 0; i < chunks.Length; i++)
        {
            InitChunk_CreateBlocks(chunks[i]);
        }

        SetPlayerSpawnPosition();
    }

    public bool create = false;
    public int chunkIndex;
    public Vector3 i;
    private void Update()
    {
        if (create)
        {
            create = false;
            CreateBlock(chunks[chunkIndex], BlockData.BlockKind.Dirt, i);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            StopCoroutine(chunks[i].saveRoutine);
            SaveChunk("Chunk" + ((int)chunks[i].position.x) + "_" + ((int)chunks[i].position.z), chunks[i]);
        }
    }

    private void VectorToInt(Vector3 index)
    {
        index_x = (int)index.x; index_y = (int)index.y; index_z = (int)index.z;
    }

    #region Chunk Load, Save �޼���

    private void SetPlayerChunk() // �÷��̾ ��ġ�� ûũ �� ����
    {
        playerChunckVector = DataManager.instance.PlayerPosition();
        playerChunckVector = new Vector3((int)playerChunckVector.x / Chunk.x, 0, (int)playerChunckVector.z / Chunk.y);
    }

    public void Load_9Chunks()
    {
        SetPlayerChunk();
        int index = 0;
        for (int z = 0; z < 3; z++)
        {
            for (int x = 0; x < 3; x++)
            {
                LoadChunk("Chunk" + (x+playerChunckVector.x) + "_" + (z + playerChunckVector.z)); // ûũ���� �ε� �� blocks���� �� ������ ����
                chunks[index] = new Chunk(new Vector3((x + playerChunckVector.x), 0, (z + playerChunckVector.z)), blocks);
                StartCoroutine(chunks[index].saveRoutine);
                index++;
            }
        }
    }

    public void Save_9Chunks()
    {
        SetPlayerChunk();
        int index = 0;
        for (int z = 0; z < 3; z++)
        {
            for (int x = 0; x < 3; x++)
            {
                SaveChunk("Chunk" + ((int)x + playerChunckVector.x) + "_" + ((int)z + playerChunckVector.z), chunks[index]);
                chunks[index] = new Chunk(new Vector3(((int)x + playerChunckVector.x), 0, ((int)z + playerChunckVector.z)), blocks);
                chunks[index].needSave = false;
                index++;
            }
        }
    }

    public void LoadChunk(string path)
    {
        Stopwatch sw = new Stopwatch();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.dataPath + "/" + path + ".binary", FileMode.Open);

        // ������ ������ ������ ����
        if (file == null)
        {
            for (int x = 0; x < Chunk.x; x++)
            {
                for (int y = 0; y < Chunk.y; y++)
                {
                    if (y < 60)
                        value = (int)BlockData.BlockKind.Dirt;
                    else
                        value = (int)BlockData.BlockKind.None;

                    for (int z = 0; z < Chunk.z; z++)
                    {
                        blocks[x, y, z] = value;
                    }

                }

            }
        }
        else
        {
            blocks = (int[,,])bf.Deserialize(file);
            file.Close();
        }
        
    }

    public void SaveChunk(string path, Chunk chunk)
    {
        print(path);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/" + path + ".binary");
        bf.Serialize(file, chunk.blocksEnum);
        file.Close();
    }
    private void CreateChunk(string path, Chunk chunk)
    {
        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                if (y < 3)
                    value = (int)BlockData.BlockKind.Dirt;
                else
                    value = (int)BlockData.BlockKind.None;

                for (int z = 0; z < Chunk.z; z++)
                {
                    blocks[x, y, z] = value;
                }

            }

        }

        print(path);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/" + path + ".binary");
        bf.Serialize(file, blocks);
        file.Close();
    }

    #endregion

    public void SetPlayerSpawnPosition()
    {
        for (int i = Chunk.y-1; i >= 0; i--)
        {
            if(chunks[4].blocksEnum[11,i,11] != 0) // ������ ���� ���� �ִ��� �˻��� �� ���� ������ ���� ��ġ ����
            {
                DataManager.instance.playerData.spawnPosition =  chunks[4].blockObjects[11,i,11].transform.position + Vector3.up;
                return;
            }
        }
    }

    private void InitChunk_CreateBlocks(Chunk chunk)
    {
       
        //position2 = new Vector3(position1.x, position1.y, position1.z);
        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                for (int z = 0; z < Chunk.z; z++)
                {
                    blockKind = (BlockData.BlockKind)chunk.blocksEnum[x, y, z]; // �� enum ��������
                    CreateBlock(chunk, blockKind, new Vector3(x,y,z));
                    //position2 += Vector3.forward;
                }
                //position2 = new Vector3(position2.x, position2.y, position1.z);
                //position2 += Vector3.up;
            }
            //position2 = new Vector3(position2.x, position1.y, position2.z);
           // position2 += Vector3.right;
        }
    }

    public void CreateBlock(Chunk chunk, BlockData.BlockKind blockKind, Vector3 index)
    {
        if (blockKind != 0)
        {
            if (!DataManager.instance.blockDictionary.ContainsKey(blockKind)) // �� dictonary�� �ش� �� ������ ������ �޼��� Ż��
                return;

            VectorToInt(index);
            chunk.blocksEnum[index_x, index_y, index_z] = (int)blockKind;

            blockPosition = new Vector3(chunk.position.x * Chunk.x + index_x, index_y - 60, chunk.position.z * Chunk.z + index_z); // index ���� ����� ��ġ ����
            block = Instantiate(blockPrefab, blockPosition, Quaternion.identity, chunk.blockParent); // �� ������Ʈ ����

            blockData = DataManager.instance.blockDictionary[blockKind]; // �� dictonary���� �ش�Ǵ� �� ������ ��������
            block.InitBlock(blockData); // �� �������� ���������� �� ������Ʈ ����
            chunk.blockObjects[(int)index.x, (int)index.y, (int)index.z] = block; // �� 3���� �迭�� �� ������Ʈ ����
            chunk.needSave = true;

        }
    }



}
