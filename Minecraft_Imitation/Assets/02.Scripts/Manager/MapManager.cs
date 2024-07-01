using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;
using static BlockData;


public class MoveData
{
    public int afterIndexY = int.MaxValue;
    public int weight;
}

public class Chunk
{
    public static readonly float saveTime = 12; // ûũ ���� ���ð�

    // ûũ ũ��
    public static readonly int x = 12;
    public static readonly int y = 256;
    public static readonly int z = 12;
    public static readonly int defaultY = -60;
    // ������
    public Chunk(int chunk_x, int chunk_z, int[,,] blocks)
    {
        this.chunk_x = chunk_x;
        this.chunk_z = chunk_z;
        this.blocksEnum = blocks;
        blockParent = new GameObject(chunk_x+"-"+ chunk_z).transform;
        blockParent.SetParent(MapManager.instance.transform);
        saveRoutine = Coroutine_SaveChunk(this);
    }

    // ������ �� ������Ʈ �θ�
    public Transform blockParent;
    public int chunk_x, chunk_z; // ûũ ��ġ�� ���ϸ�
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
                MapManager.instance.SaveChunk("Chunk" + (chunk_x + "_" + chunk_z),this);
            }

            yield return new WaitForSeconds(Chunk.saveTime);
        }
    }
}

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    public Queue<Block> blockPool = new Queue<Block>();

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

    public bool createBlock = false;
    public int chunkIndex;
    public Vector3 chunkBlockIndex;

    
    public Transform entity;
    public bool move = false;
    public int objectHeight;
    public int fallHeight;
    public bool canJump;


    private void Update()
    {
        if (createBlock)
        {
            createBlock = false;
            CreateBlock(chunks[chunkIndex], BlockData.BlockKind.Dirt, (int)chunkBlockIndex.x, (int)chunkBlockIndex.y, (int)chunkBlockIndex.z);
        }

        if (move)
        {
            move = false;
            Instantiate(entity, new Vector3(chunkBlockIndex.x + chunks[chunkIndex].chunk_x * 12, chunkBlockIndex.y + Chunk.defaultY, chunkBlockIndex.z + chunks[chunkIndex].chunk_z * 12), Quaternion.identity);
            canJump = CheckJump(chunks[chunkIndex], (int)chunkBlockIndex.x, (int)chunkBlockIndex.y, (int)chunkBlockIndex.z, objectHeight);
            print(CheckBlock(chunks[chunkIndex], (int)chunkBlockIndex.x+1, (int)chunkBlockIndex.y, (int)chunkBlockIndex.z, objectHeight, fallHeight, canJump));
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            StopCoroutine(chunks[i].saveRoutine);
            SaveChunk("Chunk" + chunks[i].chunk_x + "_" + chunks[i].chunk_z, chunks[i]);
        }
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
                chunks[index] = new Chunk((int)(x + playerChunckVector.x), (int)(z + playerChunckVector.z), blocks);
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
                SaveChunk("Chunk" + (x + playerChunckVector.x) + "_" + (z + playerChunckVector.z), chunks[index]);
                chunks[index] = new Chunk((int)(x + playerChunckVector.x), (int)(z + playerChunckVector.z), blocks);
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

    #region ���� ���� �޼���

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

    // ûũ�� �ִ� ��� �� ����
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
                    CreateBlock(chunk, blockKind, x, y, z);
                    //position2 += Vector3.forward;
                }
                //position2 = new Vector3(position2.x, position2.y, position1.z);
                //position2 += Vector3.up;
            }
            //position2 = new Vector3(position2.x, position1.y, position2.z);
           // position2 += Vector3.right;
        }
    }

    // Ư�� �� ����
    public void CreateBlock(Chunk chunk, BlockData.BlockKind blockKind, int x, int y, int z)
    {
        if (blockKind != 0)
        {
            if (!DataManager.instance.blockDictionary.ContainsKey(blockKind)) // �� dictonary�� �ش� �� ������ ������ �޼��� Ż��
                return;

            chunk.blocksEnum[x, y, z] = (int)blockKind;

            blockPosition = new Vector3(chunk.chunk_x * Chunk.x + x, y + Chunk.defaultY, chunk.chunk_z * Chunk.z + z); // index ���� ����� ��ġ ����
            block = Instantiate(blockPrefab, blockPosition, Quaternion.identity, chunk.blockParent); // �� ������Ʈ ����

            blockData = DataManager.instance.blockDictionary[blockKind]; // �� dictonary���� �ش�Ǵ� �� ������ ��������
            block.InitBlock(blockData); // �� �������� ���������� �� ������Ʈ ����
            chunk.blockObjects[x, y, z] = block; // �� 3���� �迭�� �� ������Ʈ ����
            chunk.needSave = true;

        }
    }

    public void InActiveBlock(Chunk chunk,Block block ,int x, int y, int z) // �簢�� ���� ����
    {
        chunk.blocksEnum[x, y, z] = 0; // 0�� ���� ���ٴ� �ǹ� enum
        block.gameObject.SetActive(false);
        blockPool.Enqueue(block);
    }

    #endregion

    public Vector3 GetObjectPosition(Chunk chunk, int x, int y, int z, int height)
    {
        if(Chunk.y <= y + height -1) // ������Ʈ ���̰� ���� �ִ� ���̺��� ������
        {
            return new Vector3(0,-100,0);
        }

        return new Vector3(chunk.chunk_x * Chunk.x + x, y + Chunk.defaultY + height -1, chunk.chunk_z * Chunk.z + z); // index ���� ����� ��ġ ����
    }

    public Vector3 GetBlockPosition(Chunk chunk, int x, int y, int z)
    {
        return new Vector3(chunk.chunk_x * Chunk.x + x, Chunk.defaultY + y, chunk.chunk_z * Chunk.z + z); 
    }

    #region �� �˻�

    public Chunk GetChunk(int x, int z)
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i].chunk_x == x)
            {
                if (chunks[i].chunk_z == z)
                {
                    return chunks[i];
                }
            }
        }

        return null; // �ش� ûũ�� ã�� ������
    }

    // �Ӹ� ���� ���� �ִ���
    public bool CheckJump(Chunk chunk, int x, int y, int z, int objectHeight)
    {
        if(y + objectHeight >= Chunk.y) // ���� �ִ� ���̺��� ���ٸ�
        {
            return false;
        }

        if (chunk.blocksEnum[x,y+objectHeight,z] == 0) // �Ӹ����� ���� ������ ���� ����
        {
            return true;
        }

        return false; ; 
    }

    public MoveData CheckBlock(Chunk chunk, int x, int y, int z, int objectHeight, int fallHeight, bool canJump)
    {
        int[] groundCheck = new int[fallHeight + 1 + objectHeight ]; // �̵��� ��ġ�� �ִ� �˻��� �� ����Ʈ  // ������ �� + �� ��ġ + �Ӹ� �� ��
        int index = y + objectHeight; // �������� �ε��� ������
        int value = 0;
        MoveData moveData = new MoveData();
        for (int i = groundCheck.Length - 1; i >= 0; i--)
        {
            // �̵� ��ġ�� ���� �ִ� ���̺��� ������ ����
            if (index >= Chunk.y)
            {
                groundCheck[i] = -1;
                index--;
                continue;
            }
            else if (index <= 0) // �̵� ��ġ�� ���� �ּ� ���̺��� ������ �ش� ��ġ �̵� �Ұ�
            {
                groundCheck[i] = -1;
                index--;
                continue;
            }

            // �� �˻�
            if (chunk.blocksEnum[x, index, z] == 0)
                groundCheck[i] = 0; // �� ����
            else
                groundCheck[i] = 1; // �� ����

            // ������ ����ؾ� �̵��Ҽ� �ִ� ��ġ
            if (i == groundCheck.Length -1)
            {
                if (!canJump) // �Ӹ��� ���� ���� ��� ������ ���� == ������ ���� ���� �ö��� ���Ѵ�.
                {
                    groundCheck[i] = 1; // ���� �ִ� ������ ���
                    continue;
                }
            }

            index--;
        }

        for (int i = groundCheck.Length - 1; i >= 0; i--)
        {

            if (i == groundCheck.Length - 1)
            {
                if (groundCheck[i] == 0)
                {
                    value = 20;
                }
            }
            else if (i >= fallHeight) //�� ���� ��ġ�� �� üũ
            {
                if (objectHeight == 1)  // ������ũ Ű�� 1�϶�
                {
                    if (groundCheck[i] != 0) // �� �տ� ���� ������
                    {
                        if (value != 20) // ������ �Ұ��� �ϴٸ� ����
                        {
                            moveData.weight = int.MaxValue;
                            return  moveData;
                        }
                        else
                        {
                            moveData.afterIndexY = y + 1;
                            moveData.weight = 20;
                            return moveData;
                        }
                    }
                }
                else
                {
                    //�� ���� ��ġ�� �ɸ��� �� ������ ����
                    if (i > fallHeight+1)
                    {
                        if (groundCheck[i] != 0)
                        {
                            moveData.weight = int.MaxValue;
                            return moveData;
                        }
                    }
                    else // �� �� �տ� 
                    {
                        // ���� �ִٸ� 
                        if (groundCheck[i] != 0) 
                        {
                            // ������ �����ϸ� �̵� ����
                            if (value == 20)
                            {
                                moveData.afterIndexY = y + 1;
                                moveData.weight = 20;
                                return moveData;
                            }
                            else // ������ �Ұ����ϸ� �渷����
                            {
                                moveData.weight = int.MaxValue;
                                return moveData;
                            }
                        }
                    }
                }
            }
            else if(i == fallHeight - 1)
            {
                if (groundCheck[i] != 0)
                {
                    moveData.afterIndexY = y;
                    moveData.weight = 10;
                    return moveData;
                }
            }
            else if (i < fallHeight - 1) // ���� ������ ������ �� üũ
            {
                y--;
                if (groundCheck[i] != 0) // ���� ������ ���� ����
                {
                    moveData.afterIndexY = y;
                    moveData.weight = 10;
                    return moveData;
                }
            }
        }

        // ���� ������ ������ ���� �������� �̵� �Ұ�
        moveData.weight = int.MaxValue;
        return moveData;

    }
    #endregion


}
