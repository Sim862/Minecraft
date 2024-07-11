using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEditor.PackageManager;
using UnityEngine;
using static BlockData;
public class PositionData
{
    public PositionData(int chunk_X, int chunk_Z, int blockIndex_x, int blockIndex_y, int blockIndex_z)
    {
        this.chunk_X = chunk_X;
        this.chunk_Z = chunk_Z;
        this.blockIndex_x = blockIndex_x;
        this.blockIndex_y = blockIndex_y;
        this.blockIndex_z = blockIndex_z;
    }
    public bool CheckSamePosition(PositionData positionData)
    {
        if (this.chunk_X != positionData.chunk_X)
            return false;
        if (this.chunk_Z != positionData.chunk_Z)
            return false;
        if (this.blockIndex_x != positionData.blockIndex_x)
            return false;
        if (this.blockIndex_y != positionData.blockIndex_y)
            return false;
        if (this.blockIndex_z != positionData.blockIndex_z)
            return false;
        return true;
    }

    public Chunk chunk { get => MapManager.instance.GetChunk(chunk_X, chunk_Z); }
    public int chunk_X;
    public int chunk_Z;
    public int blockIndex_x;
    public int blockIndex_y;
    public int blockIndex_z;
}


public class MoveData
{
    public int afterIndexY = int.MaxValue;
    public int weight;
}

public class Chunk
{
    public static readonly float saveTime = 12; // 청크 저장 대기시간
    public static readonly int MAX_ChunkIndex = 10;

    // 청크 크기
    public static readonly int x = 12;
    public static readonly int y = 256;
    public static readonly int z = 12;
    public static readonly int defaultY = -60;
    // 생성자
    public Chunk(int chunk_X, int chunk_Z, int[,,] blocks)
    {
        this.chunk_X = chunk_X;
        this.chunk_Z = chunk_Z;
        this.blocksEnum = blocks;
        blockParent = new GameObject(chunk_X+"-"+ chunk_Z).transform;
        blockParent.SetParent(MapManager.instance.transform);
        saveRoutine = Coroutine_SaveChunk(this);
    }

    // 생성된 블럭 오브젝트 부모
    public Transform blockParent;
    public int chunk_X, chunk_Z; // 청크 위치와 파일명
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
                MapManager.instance.SaveChunk(chunk_X, chunk_Z,this);
            }

            yield return new WaitForSeconds(Chunk.saveTime);
        }
    }
}

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    
    public static Queue<Block> blockPool = new Queue<Block>();

    public Block blockPrefab;

    public PositionData playerPositionData; // transform.postion 아님 청크 위치와 파일명
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

        playerPositionData = new PositionData(0, 0, 0, 0, 0);
    }

    void Start()
    {
        Load_9Chunks();

    }

    public bool createBlock = false;
    public int chunkIndex;
    public BlockData.BlockKind blockKind1;
    public Vector3 chunkBlockIndex;

    
    public Transform entity;
    public bool move = false;
    public int objectHeight;
    public int fallHeight;
    public bool canJump;


    private void Update()
    {
        //if (createBlock)
        //{
        //    createBlock = false;
        //    //CreateBlock(chunks[chunkIndex], blockKind1, (int)chunkBlockIndex.x, (int)chunkBlockIndex.y, (int)chunkBlockIndex.z);
        //}
        UpdateLoadChunk();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            StopCoroutine(chunks[i].saveRoutine);
            SaveChunk(chunks[i].chunk_X, chunks[i].chunk_Z, chunks[i]);
        }
    }

    #region Chunk Load, Save 메서드

    public void UpdateLoadChunk()
    {
        if (playerPositionData.chunk_X == chunks[4].chunk_X && playerPositionData.chunk_Z == chunks[4].chunk_Z)
        {
            return;
        }
        else
        {
            Chunk[] newChunks = new Chunk[9];
            Chunk temp_Chunk;
            for (int i = 0; i < newChunks.Length; i++)
            {
                newChunks[i] = chunks[i];
            }

            #region 왼쪽으로 이동했을떄
            if (playerPositionData.chunk_X < chunks[4].chunk_X)
            {
                StopCoroutine(chunks[5].saveRoutine); // 오른쪽 끝 청크 저장 타이머 종류 후 저장
                SaveChunk(chunks[5].chunk_X, chunks[5].chunk_Z, chunks[5]);
                ReturnBlockPool_Chunk(chunks[5]);
                // 청크 이동
                for (int i = 4; i < 6; i++)
                {
                    newChunks[i] = chunks[i - 1];
                }

                // 청크 데이터 로드
                LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z);
                newChunks[3] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z, blocks);

                // 블럭 생성
                InitChunk_CreateBlocks(newChunks[3]);
                // 블럭 저장 주기 시작
                StartCoroutine(newChunks[3].saveRoutine);

                #region 왼쪽 아래 (대각선) 이동
                if (playerPositionData.chunk_Z < chunks[4].chunk_Z)
                {
                    // 왼쪽 아래
                    StopCoroutine(chunks[2].saveRoutine);
                    SaveChunk(chunks[2].chunk_X, chunks[2].chunk_Z, chunks[2]);
                    ReturnBlockPool_Chunk(chunks[2]);

                    for (int i = 1; i < 3; i++)
                    {
                        newChunks[i] = chunks[i - 1];
                    }

                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z - 1);
                    newChunks[0] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z - 1, blocks);

                    InitChunk_CreateBlocks(newChunks[0]);
                    StartCoroutine(newChunks[0].saveRoutine);

                    // --------------------------------------------------------

                    // 왼쪽 대각선 아래
                    StopCoroutine(chunks[6].saveRoutine);
                    SaveChunk(chunks[6].chunk_X, chunks[6].chunk_Z, chunks[6]);
                    ReturnBlockPool_Chunk(chunks[6]);

                    for (int i = 3; i >= 0 ; i -= 3)
                    {
                        newChunks[i + 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z - 2);
                    newChunks[0] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z - 2, blocks);

                    InitChunk_CreateBlocks(newChunks[0]);
                    StartCoroutine(newChunks[0].saveRoutine);
                    // -------------------------------------------------------------

                    // 아래
                    StopCoroutine(chunks[7].saveRoutine);
                    SaveChunk(chunks[7].chunk_X, chunks[7].chunk_Z, chunks[7]);
                    ReturnBlockPool_Chunk(chunks[7]);

                    for (int i = 4; i > 0; i -= 3)
                    {
                        newChunks[i + 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X - 1, chunks[4].chunk_Z - 2);
                    newChunks[1] = new Chunk(chunks[4].chunk_X - 1, chunks[4].chunk_Z - 2, blocks);

                    InitChunk_CreateBlocks(newChunks[1]);
                    StartCoroutine(newChunks[1].saveRoutine);
                    //// -----------------------------------------------------------

                    // 오른쪽 아래
                    StopCoroutine(chunks[8].saveRoutine);
                    SaveChunk(chunks[8].chunk_X, chunks[8].chunk_Z, chunks[8]);
                    ReturnBlockPool_Chunk(chunks[8]);

                    for (int i = 5; i >= 0; i -= 3)
                    {
                        newChunks[i + 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X, chunks[4].chunk_Z - 2);
                    newChunks[2] = new Chunk(chunks[4].chunk_X, chunks[4].chunk_Z - 2, blocks);

                    InitChunk_CreateBlocks(newChunks[2]);
                    StartCoroutine(newChunks[2].saveRoutine);

                }
                #endregion
                #region 왼쪽으로 만 이동
                else if (playerPositionData.chunk_Z == chunks[4].chunk_Z)
                {
                    // 왼쪽 아래
                    StopCoroutine(chunks[2].saveRoutine);
                    SaveChunk(chunks[2].chunk_X, chunks[2].chunk_Z, chunks[2]);
                    ReturnBlockPool_Chunk(chunks[2]);

                    for (int i = 1; i < 3; i++)
                    {
                        newChunks[i] = chunks[i - 1];
                    }


                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z - 1);
                    newChunks[0] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z - 1, blocks);

                    InitChunk_CreateBlocks(newChunks[0]);
                    StartCoroutine(newChunks[0].saveRoutine);

                    // --------------------------------------------------------------------------------------

                    // 왼쪽 위
                    StopCoroutine(chunks[8].saveRoutine);
                    SaveChunk(chunks[8].chunk_X, chunks[8].chunk_Z, chunks[8]);
                    ReturnBlockPool_Chunk(chunks[8]);
                    for (int i = 7; i < 9; i++)
                    {
                        newChunks[i] = chunks[i - 1];
                    }

                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 1);
                    newChunks[6] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 1, blocks);

                    InitChunk_CreateBlocks(newChunks[6]);
                    StartCoroutine(newChunks[6].saveRoutine);
                }
                #endregion
                #region 왼쪽으로 위 이동
                else
                {
                    // 왼쪽
                    StopCoroutine(chunks[8].saveRoutine);
                    SaveChunk(chunks[8].chunk_X, chunks[8].chunk_Z, chunks[8]);
                    ReturnBlockPool_Chunk(chunks[8]);

                    for (int i = 7; i < 9; i++)
                    {
                        newChunks[i] = chunks[i - 1];
                    }

                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 1);
                    newChunks[6] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 1, blocks);

                    InitChunk_CreateBlocks(newChunks[6]);
                    StartCoroutine(newChunks[6].saveRoutine);

                    // --------------------------------------------------------

                    // 왼쪽 대각선 위
                    StopCoroutine(chunks[0].saveRoutine);
                    SaveChunk(chunks[0].chunk_X, chunks[0].chunk_Z, chunks[0]);
                    ReturnBlockPool_Chunk(chunks[0]);

                    for (int i = 3; i < 7; i += 3)
                    {
                        newChunks[i - 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 2);
                    newChunks[6] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 2, blocks);

                    InitChunk_CreateBlocks(newChunks[6]);
                    StartCoroutine(newChunks[6].saveRoutine);
                    // -------------------------------------------------------------

                    // 위
                    StopCoroutine(chunks[1].saveRoutine);
                    SaveChunk(chunks[1].chunk_X, chunks[1].chunk_Z, chunks[1]);
                    ReturnBlockPool_Chunk(chunks[1]);

                    for (int i = 4; i < 8; i += 3)
                    {
                        newChunks[i - 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X - 1, chunks[4].chunk_Z + 2);
                    newChunks[7] = new Chunk(chunks[4].chunk_X - 1, chunks[4].chunk_Z + 2, blocks);

                    InitChunk_CreateBlocks(newChunks[7]);
                    StartCoroutine(newChunks[7].saveRoutine);
                    //// -----------------------------------------------------------

                    // 오른쪽 위
                    StopCoroutine(chunks[2].saveRoutine);
                    SaveChunk(chunks[2].chunk_X, chunks[2].chunk_Z, chunks[2]);
                    ReturnBlockPool_Chunk(chunks[2]);

                    for (int i = 5; i < 9; i += 3)
                    {
                        newChunks[i - 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X, chunks[4].chunk_Z + 2);
                    newChunks[8] = new Chunk(chunks[4].chunk_X, chunks[4].chunk_Z + 2, blocks);

                    InitChunk_CreateBlocks(newChunks[8]);
                    StartCoroutine(newChunks[8].saveRoutine);
                }
                #endregion

            }
            #endregion
            else if (playerPositionData.chunk_X == chunks[4].chunk_X)
            {

            }
            else
            {

                StopCoroutine(chunks[3].saveRoutine);
                SaveChunk(chunks[3].chunk_X, chunks[3].chunk_Z, chunks[3]);
                ReturnBlockPool_Chunk(chunks[3]);
                // 청크 이동
                for (int i = 4; i > 2; i--)
                {
                    newChunks[i] = chunks[i + 1];
                }

                // 청크 데이터 로드
                LoadChunk(chunks[4].chunk_X + 2, chunks[4].chunk_Z);
                newChunks[5] = new Chunk(chunks[4].chunk_X + 2, chunks[4].chunk_Z, blocks);

                // 블럭 생성
                InitChunk_CreateBlocks(newChunks[5]);
                // 블럭 저장 주기 시작
                StartCoroutine(newChunks[5].saveRoutine);

                #region 오른쪽 아래 (대각선) 이동
                if (playerPositionData.chunk_Z < chunks[4].chunk_Z)
                {
                    // 오른쪽 아래
                    StopCoroutine(chunks[0].saveRoutine);
                    SaveChunk(chunks[0].chunk_X, chunks[0].chunk_Z, chunks[0]);
                    ReturnBlockPool_Chunk(chunks[0]);

                    for (int i = 0; i < 3; i++)
                    {
                        newChunks[i] = chunks[i + 1];
                    }

                    LoadChunk(chunks[4].chunk_X + 2, chunks[4].chunk_Z - 1);
                    newChunks[2] = new Chunk(chunks[4].chunk_X + 2, chunks[4].chunk_Z - 1, blocks);

                    InitChunk_CreateBlocks(newChunks[2]);
                    StartCoroutine(newChunks[2].saveRoutine);

                    // --------------------------------------------------------

                    // 오른쪽 대각선 아래
                    StopCoroutine(chunks[8].saveRoutine);
                    SaveChunk(chunks[8].chunk_X, chunks[8].chunk_Z, chunks[8]);
                    ReturnBlockPool_Chunk(chunks[8]);

                    for (int i = 5; i >= 0; i -= 3)
                    {
                        newChunks[i + 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X + 2, chunks[4].chunk_Z - 2);
                    newChunks[2] = new Chunk(chunks[4].chunk_X + 2, chunks[4].chunk_Z - 2, blocks);

                    InitChunk_CreateBlocks(newChunks[2]);
                    StartCoroutine(newChunks[2].saveRoutine);
                    // -------------------------------------------------------------

                    // 아래
                    StopCoroutine(chunks[7].saveRoutine);
                    SaveChunk(chunks[7].chunk_X, chunks[7].chunk_Z, chunks[7]);
                    ReturnBlockPool_Chunk(chunks[7]);

                    for (int i = 4; i > 0; i -= 3)
                    {
                        newChunks[i + 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X + 1, chunks[4].chunk_Z - 2);
                    newChunks[1] = new Chunk(chunks[4].chunk_X + 1, chunks[4].chunk_Z - 2, blocks);

                    InitChunk_CreateBlocks(newChunks[1]);
                    StartCoroutine(newChunks[1].saveRoutine);
                    //// -----------------------------------------------------------

                    // 왼쪽 아래
                    StopCoroutine(chunks[6].saveRoutine);
                    SaveChunk(chunks[6].chunk_X, chunks[6].chunk_Z, chunks[6]);
                    ReturnBlockPool_Chunk(chunks[8]);

                    for (int i = 3; i >= 0; i -= 3)
                    {
                        newChunks[i + 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X, chunks[4].chunk_Z - 2);
                    newChunks[0] = new Chunk(chunks[4].chunk_X, chunks[4].chunk_Z - 2, blocks);

                    InitChunk_CreateBlocks(newChunks[0]);
                    StartCoroutine(newChunks[0].saveRoutine);
                    \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\ 여기까지 함
                }
                #endregion
                #region 왼쪽으로 만 이동
                else if (playerPositionData.chunk_Z == chunks[4].chunk_Z)
                {
                    // 왼쪽 아래
                    StopCoroutine(chunks[2].saveRoutine);
                    SaveChunk(chunks[2].chunk_X, chunks[2].chunk_Z, chunks[2]);
                    ReturnBlockPool_Chunk(chunks[2]);

                    for (int i = 1; i < 3; i++)
                    {
                        newChunks[i] = chunks[i - 1];
                    }


                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z - 1);
                    newChunks[0] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z - 1, blocks);

                    InitChunk_CreateBlocks(newChunks[0]);
                    StartCoroutine(newChunks[0].saveRoutine);

                    // --------------------------------------------------------------------------------------

                    // 왼쪽 위
                    StopCoroutine(chunks[8].saveRoutine);
                    SaveChunk(chunks[8].chunk_X, chunks[8].chunk_Z, chunks[8]);
                    ReturnBlockPool_Chunk(chunks[8]);
                    for (int i = 7; i < 9; i++)
                    {
                        newChunks[i] = chunks[i - 1];
                    }

                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 1);
                    newChunks[6] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 1, blocks);

                    InitChunk_CreateBlocks(newChunks[6]);
                    StartCoroutine(newChunks[6].saveRoutine);
                }
                #endregion
                #region 왼쪽으로 위 이동
                else
                {
                    // 왼쪽
                    StopCoroutine(chunks[8].saveRoutine);
                    SaveChunk(chunks[8].chunk_X, chunks[8].chunk_Z, chunks[8]);
                    ReturnBlockPool_Chunk(chunks[8]);

                    for (int i = 7; i < 9; i++)
                    {
                        newChunks[i] = chunks[i - 1];
                    }

                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 1);
                    newChunks[6] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 1, blocks);

                    InitChunk_CreateBlocks(newChunks[6]);
                    StartCoroutine(newChunks[6].saveRoutine);

                    // --------------------------------------------------------

                    // 왼쪽 대각선 위
                    StopCoroutine(chunks[0].saveRoutine);
                    SaveChunk(chunks[0].chunk_X, chunks[0].chunk_Z, chunks[0]);
                    ReturnBlockPool_Chunk(chunks[0]);

                    for (int i = 3; i < 7; i += 3)
                    {
                        newChunks[i - 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 2);
                    newChunks[6] = new Chunk(chunks[4].chunk_X - 2, chunks[4].chunk_Z + 2, blocks);

                    InitChunk_CreateBlocks(newChunks[6]);
                    StartCoroutine(newChunks[6].saveRoutine);
                    // -------------------------------------------------------------

                    // 위
                    StopCoroutine(chunks[1].saveRoutine);
                    SaveChunk(chunks[1].chunk_X, chunks[1].chunk_Z, chunks[1]);
                    ReturnBlockPool_Chunk(chunks[1]);

                    for (int i = 4; i < 8; i += 3)
                    {
                        newChunks[i - 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X - 1, chunks[4].chunk_Z + 2);
                    newChunks[7] = new Chunk(chunks[4].chunk_X - 1, chunks[4].chunk_Z + 2, blocks);

                    InitChunk_CreateBlocks(newChunks[7]);
                    StartCoroutine(newChunks[7].saveRoutine);
                    //// -----------------------------------------------------------

                    // 오른쪽 위
                    StopCoroutine(chunks[2].saveRoutine);
                    SaveChunk(chunks[2].chunk_X, chunks[2].chunk_Z, chunks[2]);
                    ReturnBlockPool_Chunk(chunks[2]);

                    for (int i = 5; i < 9; i += 3)
                    {
                        newChunks[i - 3] = newChunks[i];
                    }

                    LoadChunk(chunks[4].chunk_X, chunks[4].chunk_Z + 2);
                    newChunks[8] = new Chunk(chunks[4].chunk_X, chunks[4].chunk_Z + 2, blocks);

                    InitChunk_CreateBlocks(newChunks[8]);
                    StartCoroutine(newChunks[8].saveRoutine);
                }
                #endregion
            }

            chunks = newChunks;
        }
    }

    public void Load_9Chunks()
    {
        //SetPlayerChunk();
        int index = 0;
        for (int z = -1; z < 2; z++)
        {
            for (int x = -1; x < 2; x++)
            {
                //LoadChunk("Chunk" + (x+playerChunckVector.chunk_X) + "_" + (z + playerChunckVector.chunk_Z)); // 청크파일 로드 후 blocks에서 블럭 데이터 셋팅
                //chunks[index] = new Chunk((int)(x + playerChunckVector.chunk_X), (int)(z + playerChunckVector.chunk_Z), blocks);
                LoadChunk(x+ playerPositionData.chunk_X, z + playerPositionData.chunk_Z); // 청크파일 로드 후 blocks에서 블럭 데이터 셋팅
                chunks[index] = new Chunk((int)(x), (int)(z), blocks);
                StartCoroutine(chunks[index].saveRoutine);
                InitChunk_CreateBlocks(chunks[index]);
                index++;
            }
        }
    }

    //public void Save_9Chunks()
    //{
    //    //SetPlayerChunk();
    //    int index = 0;
    //    for (int z = -1; z < 2; z++)
    //    {
    //        for (int x = -1; x < 2; x++)
    //        {
    //            //SaveChunk("Chunk" + (x + playerChunckVector.chunk_X) + "_" + (z + playerChunckVector.chunk_Z), chunks[index]);
    //            SaveChunk(x, z, chunks[index]);
    //            chunks[index].needSave = false;
    //            index++;
    //        }
    //    }
    //}

    public void LoadChunk(int chunk_X, int chunk_Z)
    {
        if (chunk_X < 0)
        {
            chunk_X = chunk_X % Chunk.MAX_ChunkIndex;
            chunk_X = Chunk.MAX_ChunkIndex + chunk_X;
            if (chunk_X == Chunk.MAX_ChunkIndex)
                chunk_X = 0;
        }
        else if (chunk_X >= Chunk.MAX_ChunkIndex)
        {
            chunk_X = chunk_X % Chunk.MAX_ChunkIndex;
        }

        if (chunk_Z < 0)
        {
            chunk_Z = chunk_Z % Chunk.MAX_ChunkIndex;
            chunk_Z = Chunk.MAX_ChunkIndex + chunk_Z;
            if (chunk_Z == Chunk.MAX_ChunkIndex)
                chunk_Z = 0;
        }
        else if (chunk_Z >= Chunk.MAX_ChunkIndex)
        {
            chunk_Z = chunk_Z % Chunk.MAX_ChunkIndex;
        }
        print(chunk_X + " - " + chunk_Z);
        print("===============================================");
        BinaryFormatter bf = new BinaryFormatter();
        try
        {
            FileStream file = File.Open(Application.dataPath + "/Chunk" + (chunk_X) + "_" + (chunk_Z) + ".binary", FileMode.Open);
            blocks = (int[,,])bf.Deserialize(file);
            file.Close();
        }

        catch (FileNotFoundException error)
        {
            print("파일 없음"); 
            
            for (int x = 0; x < Chunk.x; x++)
            {
                for (int y = 0; y < Chunk.y; y++)
                {
                    if (y < 5)
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
    }

    public void SaveChunk(int chunk_X, int chunk_Z, Chunk chunk)
    {
        if (chunk_X < 0)
        {
            chunk_X = chunk_X % Chunk.MAX_ChunkIndex;
            chunk_X = Chunk.MAX_ChunkIndex + chunk_X;
        }
        else if (chunk_X >= Chunk.MAX_ChunkIndex)
        {
            chunk_X = chunk_X % Chunk.MAX_ChunkIndex;
        }

        if (chunk_Z < 0)
        {
            chunk_Z = chunk_Z % Chunk.MAX_ChunkIndex;
            chunk_Z = Chunk.MAX_ChunkIndex + chunk_Z;
        }
        else if (chunk_Z >= Chunk.MAX_ChunkIndex)
        {
            chunk_Z = chunk_Z % Chunk.MAX_ChunkIndex;
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/Chunk" + (chunk_X) + "_" + (chunk_Z) + ".binary");
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

    //public void SetPlayerSpawnPosition()
    //{
    //    for (int i = Chunk.y-1; i >= 0; i--)
    //    {
    //        if(chunks[4].blocksEnum[11,i,11] != 0) // 위에서 부터 블럭이 있는지 검사한 후 땅이 나오면 스폰 위치 설정
    //        {
    //            DataManager.instance.playerData.spawnPosition =  chunks[4].blockObjects[11,i,11].transform.position + Vector3.up;
    //            return;
    //        }
    //    }
    //}

    // 청크에 있는 모든 블럭 스폰
    private void InitChunk_CreateBlocks(Chunk chunk)
    {
        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                for (int z = 0; z < Chunk.z; z++)
                {
                    blockKind = (BlockData.BlockKind)chunk.blocksEnum[x, y, z]; // 블럭 enum 가져오기
                    CreateBlock(chunk, blockKind, x, y, z);
                }
            }
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
            blockPosition = new Vector3(chunk.chunk_X * Chunk.x + x, y + Chunk.defaultY, chunk.chunk_Z * Chunk.z + z); // index 값을 사용해 위치
            blockData = DataManager.instance.blockDictionary[blockKind]; // 블럭 dictonary에서 해당되는 블럭 데이터 가져오기

            if (blockPool.Count > 0)
            {
                block = blockPool.Dequeue();
                block.transform.position = blockPosition;
                block.transform.SetParent(chunk.blockParent);
                block.gameObject.SetActive(true);
            }
            else
            {
                block = Instantiate(blockPrefab, blockPosition, Quaternion.identity, chunk.blockParent); // 블럭 오브젝트 생성
            }
            block.InitBlock(blockData, new PositionData(chunk.chunk_X,chunk.chunk_Z,x,y,z)); // 블럭 데이터의 설정값으로 블럭 오브젝트 설정
            chunk.blockObjects[x, y, z] = block; // 블럭 3차원 배열에 블럭 오브젝트 저장
            chunk.needSave = true;
        }
        else
        {
            if (chunk.blocksEnum[x,y,z] != 0)
            {
                BreakBlock(chunk.blockObjects[x, y, z]);
            }
        }
    }

    public void BreakBlock(Block block)
    {
        Chunk chunk = block.positionData.chunk;
        chunk.blocksEnum[block.positionData.blockIndex_x, block.positionData.blockIndex_y, block.positionData.blockIndex_z] = 0;
        chunk.blockObjects[block.positionData.blockIndex_x, block.positionData.blockIndex_y, block.positionData.blockIndex_z] = null;
        block.gameObject.SetActive(false);
        blockPool.Enqueue(block);
    }

    public void ReturnBlockPool_Chunk(Chunk chunk) // 사각형 블럭만 가능
    {
        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                for (int z = 0; z < Chunk.z; z++)
                {
                    block = chunk.blockObjects[x, y, z];
                    if(block != null)
                    {
                        block.gameObject.SetActive(false);
                        blockPool.Enqueue(block);
                    }
                }

            }

        }
    }

    #endregion

    public Vector3 GetObjectPosition(Chunk chunk, int x, int y, int z)
    {
        Vector3 vector = new Vector3(chunk.chunk_X * Chunk.x + x, y + Chunk.defaultY, chunk.chunk_Z * Chunk.z + z);

        return new Vector3(chunk.chunk_X * Chunk.x + x, y + Chunk.defaultY, chunk.chunk_Z * Chunk.z + z); // index 값을 사용해 위치 설정
    }

    public PositionData PositionToBlockData(Vector3 objectPosition)
    {
        int chunk_X, chunk_Z;
        int blockIndex_X, blockIndex_Z;
        if (objectPosition.x >= 0)
        {
            chunk_X = Mathf.RoundToInt(objectPosition.x) / Chunk.x;
            blockIndex_X = Mathf.RoundToInt(objectPosition.x) % Chunk.x;
        }
        else
        {
            chunk_X = ((Mathf.RoundToInt(objectPosition.x) - Chunk.x) / Chunk.x);
            blockIndex_X = (Chunk.x - Mathf.RoundToInt(objectPosition.x)) % Chunk.x;
        }
        if (objectPosition.z >= 0)
        {
            chunk_Z = Mathf.RoundToInt(objectPosition.z) / Chunk.z;
            blockIndex_Z = Mathf.RoundToInt(objectPosition.z) % Chunk.z;
        }
        else
        {
            chunk_Z = ((Mathf.RoundToInt(objectPosition.z) - Chunk.z) / Chunk.z);
            blockIndex_Z = (Chunk.z - Mathf.RoundToInt(objectPosition.z)) % Chunk.z;
        }

        PositionData positionData = new PositionData(chunk_X,chunk_Z,blockIndex_X,(int)objectPosition.y - Chunk.defaultY,blockIndex_Z);

        return positionData;
    }

    #region 블럭 검사

    public Chunk GetChunk(int x, int z)
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i].chunk_X == x && chunks[i].chunk_Z == z)
            {
                return chunks[i];
            }
        }

        print(1);
        return null; // 해당 청크를 찾지 못했음
    }

    // 머리 위에 블럭이 있는지
    public bool CheckJump(PositionData positionData, int objectHeight)
    {
        if(positionData.blockIndex_y + objectHeight >= Chunk.y) // 월드 최대 높이보다 높다면
        {
            return false;
        }

        if (positionData.chunk.blocksEnum[positionData.blockIndex_x, positionData.blockIndex_y + objectHeight, positionData.blockIndex_z] == 0) // 머리위에 블럭이 없으면 점프 가능
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
