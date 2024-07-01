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
    public static readonly float saveTime = 12; // 청크 저장 대기시간

    // 청크 크기
    public static readonly int x = 12;
    public static readonly int y = 256;
    public static readonly int z = 12;
    public static readonly int defaultY = -60;
    // 생성자
    public Chunk(int chunk_x, int chunk_z, int[,,] blocks)
    {
        this.chunk_x = chunk_x;
        this.chunk_z = chunk_z;
        this.blocksEnum = blocks;
        blockParent = new GameObject(chunk_x+"-"+ chunk_z).transform;
        blockParent.SetParent(MapManager.instance.transform);
        saveRoutine = Coroutine_SaveChunk(this);
    }

    // 생성된 블럭 오브젝트 부모
    public Transform blockParent;
    public int chunk_x, chunk_z; // 청크 위치와 파일명
    public int[,,] blocksEnum = new int[x, y, z]; // x, y, z
    public Block[,,] blockObjects = new Block[x, y, z]; // x, y, z

    // 맵 변경사항 있는지 체크
    public bool needSave = false;

    // 코루틴
    public IEnumerator saveRoutine;

    // 변경사항이 생기면 12초 마다 저장
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

    private Vector3 playerChunckVector; // transform.postion 아님 청크 위치와 파일명
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

    #region Chunk Load, Save 메서드

    private void SetPlayerChunk() // 플레이어가 위치한 청크 값 설정
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
                LoadChunk("Chunk" + (x+playerChunckVector.x) + "_" + (z + playerChunckVector.z)); // 청크파일 로드 후 blocks에서 블럭 데이터 셋팅
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

        // 파일이 없으면 평지로 설정
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

    #region 생성 관련 메서드

    public void SetPlayerSpawnPosition()
    {
        for (int i = Chunk.y-1; i >= 0; i--)
        {
            if(chunks[4].blocksEnum[11,i,11] != 0) // 위에서 부터 블럭이 있는지 검사한 후 땅이 나오면 스폰 위치 설정
            {
                DataManager.instance.playerData.spawnPosition =  chunks[4].blockObjects[11,i,11].transform.position + Vector3.up;
                return;
            }
        }
    }

    // 청크에 있는 모든 블럭 스폰
    private void InitChunk_CreateBlocks(Chunk chunk)
    {
       
        //position2 = new Vector3(position1.x, position1.y, position1.z);
        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                for (int z = 0; z < Chunk.z; z++)
                {
                    blockKind = (BlockData.BlockKind)chunk.blocksEnum[x, y, z]; // 블럭 enum 가져오기
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

    // 특정 블럭 생성
    public void CreateBlock(Chunk chunk, BlockData.BlockKind blockKind, int x, int y, int z)
    {
        if (blockKind != 0)
        {
            if (!DataManager.instance.blockDictionary.ContainsKey(blockKind)) // 블럭 dictonary에 해당 블럭 데이터 없으면 메서드 탈출
                return;

            chunk.blocksEnum[x, y, z] = (int)blockKind;

            blockPosition = new Vector3(chunk.chunk_x * Chunk.x + x, y + Chunk.defaultY, chunk.chunk_z * Chunk.z + z); // index 값을 사용해 위치 설정
            block = Instantiate(blockPrefab, blockPosition, Quaternion.identity, chunk.blockParent); // 블럭 오브젝트 생성

            blockData = DataManager.instance.blockDictionary[blockKind]; // 블럭 dictonary에서 해당되는 블럭 데이터 가져오기
            block.InitBlock(blockData); // 블럭 데이터의 설정값으로 블럭 오브젝트 설정
            chunk.blockObjects[x, y, z] = block; // 블럭 3차원 배열에 블럭 오브젝트 저장
            chunk.needSave = true;

        }
    }

    public void InActiveBlock(Chunk chunk,Block block ,int x, int y, int z) // 사각형 블럭만 가능
    {
        chunk.blocksEnum[x, y, z] = 0; // 0은 블럭이 없다는 의미 enum
        block.gameObject.SetActive(false);
        blockPool.Enqueue(block);
    }

    #endregion

    public Vector3 GetObjectPosition(Chunk chunk, int x, int y, int z, int height)
    {
        if(Chunk.y <= y + height -1) // 오브젝트 높이가 월드 최대 높이보다 높을때
        {
            return new Vector3(0,-100,0);
        }

        return new Vector3(chunk.chunk_x * Chunk.x + x, y + Chunk.defaultY + height -1, chunk.chunk_z * Chunk.z + z); // index 값을 사용해 위치 설정
    }

    public Vector3 GetBlockPosition(Chunk chunk, int x, int y, int z)
    {
        return new Vector3(chunk.chunk_x * Chunk.x + x, Chunk.defaultY + y, chunk.chunk_z * Chunk.z + z); 
    }

    #region 블럭 검사

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

        return null; // 해당 청크를 찾지 못했음
    }

    // 머리 위에 블럭이 있는지
    public bool CheckJump(Chunk chunk, int x, int y, int z, int objectHeight)
    {
        if(y + objectHeight >= Chunk.y) // 월드 최대 높이보다 높다면
        {
            return false;
        }

        if (chunk.blocksEnum[x,y+objectHeight,z] == 0) // 머리위에 블럭이 없으면 점프 가능
        {
            return true;
        }

        return false; ; 
    }

    public MoveData CheckBlock(Chunk chunk, int x, int y, int z, int objectHeight, int fallHeight, bool canJump)
    {
        int[] groundCheck = new int[fallHeight + 1 + objectHeight ]; // 이동할 위치에 있는 검사할 블럭 리스트  // 떨어질 블럭 + 내 위치 + 머리 위 블럭
        int index = y + objectHeight; // 블럭데이터 인덱스 위부터
        int value = 0;
        MoveData moveData = new MoveData();
        for (int i = groundCheck.Length - 1; i >= 0; i--)
        {
            // 이동 위치가 월드 최대 높이보다 높으면 못감
            if (index >= Chunk.y)
            {
                groundCheck[i] = -1;
                index--;
                continue;
            }
            else if (index <= 0) // 이동 위치가 월드 최소 높이보다 낮을때 해당 위치 이동 불가
            {
                groundCheck[i] = -1;
                index--;
                continue;
            }

            // 블럭 검사
            if (chunk.blocksEnum[x, index, z] == 0)
                groundCheck[i] = 0; // 블럭 없음
            else
                groundCheck[i] = 1; // 블럭 있음

            // 점프를 사용해야 이동할수 있는 위치
            if (i == groundCheck.Length -1)
            {
                if (!canJump) // 머리위 블럭이 있을 경우 점프를 못함 == 나보다 높은 블럭에 올라가지 못한다.
                {
                    groundCheck[i] = 1; // 블럭이 있는 것으로 취급
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
            else if (i >= fallHeight) //내 몸통 위치의 블럭 체크
            {
                if (objectHeight == 1)  // 오브젝크 키가 1일때
                {
                    if (groundCheck[i] != 0) // 내 앞에 블럭이 있을때
                    {
                        if (value != 20) // 점프가 불가능 하다면 못감
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
                    //내 몸통 위치에 걸리는 블럭 있으면 못감
                    if (i > fallHeight+1)
                    {
                        if (groundCheck[i] != 0)
                        {
                            moveData.weight = int.MaxValue;
                            return moveData;
                        }
                    }
                    else // 내 발 앞에 
                    {
                        // 블럭이 있다면 
                        if (groundCheck[i] != 0) 
                        {
                            // 점프가 가능하면 이동 가능
                            if (value == 20)
                            {
                                moveData.afterIndexY = y + 1;
                                moveData.weight = 20;
                                return moveData;
                            }
                            else // 점프가 불가능하면 길막혔음
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
            else if (i < fallHeight - 1) // 낙하 가능한 높이의 블럭 체크
            {
                y--;
                if (groundCheck[i] != 0) // 블럭이 있으면 낙하 가능
                {
                    moveData.afterIndexY = y;
                    moveData.weight = 10;
                    return moveData;
                }
            }
        }

        // 낙하 가능한 높이의 블럭도 없었으니 이동 불가
        moveData.weight = int.MaxValue;
        return moveData;

    }
    #endregion


}
