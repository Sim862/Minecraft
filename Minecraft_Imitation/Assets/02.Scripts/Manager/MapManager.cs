using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;
using static BlockData;
using Random = UnityEngine.Random;

[Serializable]
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
    public Chunk(int chunk_X, int chunk_Z, ChunkData chunkData)
    {
        this.chunk_X = chunk_X;
        this.chunk_Z = chunk_Z;
        this.chunkData = chunkData;
        blockParent = new GameObject(chunk_X+"-"+ chunk_Z).transform;
        blockParent.SetParent(MapManager.instance.transform);
        saveRoutine = Coroutine_SaveChunk(this);
    }

    public List<Mob> mobGameObjects = new List<Mob>();

    // 생성된 블럭 오브젝트 부모
    public Transform blockParent;
    public int chunk_X, chunk_Z; // 청크 위치와 파일명
    public ChunkData chunkData;
    public Block[,,] blockObjects = new Block[x, y, z]; // x, y, z

    // 맵 변경사항 있는지 체크
    public bool needSave = false;

    // 코루틴
    public IEnumerator saveRoutine;

    public bool initMonster = false;

    public void AddMobSpawnData(MobSpawnData mobSpawnData, Mob mob)
    {
        if (!chunkData.mobSpawnDatas.Contains(mobSpawnData))
        {
            chunkData.mobSpawnDatas.Add(mobSpawnData);
            mobGameObjects.Add(mob);
        }
    }
    public void RemoveMobSpawnData(MobSpawnData mobSpawnData, Mob mob)
    {
        if (chunkData.mobSpawnDatas.Contains(mobSpawnData))
        {
            chunkData.mobSpawnDatas.Remove(mobSpawnData);
            mobGameObjects.Remove(mob);
        }
    }



    // 변경사항이 생기면 12초 마다 저장
    private IEnumerator Coroutine_SaveChunk(Chunk chunk)
    {
        while (true)
        {
            if (chunk.needSave)
            {
                chunk.needSave = false;
                MapManager.instance.SaveChunk(chunk.chunk_X, chunk.chunk_Z, this);
            }

            yield return new WaitForSeconds(Chunk.saveTime);
        }
    }
}

[Serializable]
public class ChunkData
{
    public ChunkData(int[,,] blocksEnum, List<MobSpawnData> mobSpawnDatas = null)
    {
        this.blocksEnum = blocksEnum;
        if(mobSpawnDatas == null)
        {
            this.mobSpawnDatas = new List<MobSpawnData>();
        }
    }

    public int[,,] blocksEnum = new int[Chunk.x, Chunk.y, Chunk.z]; // x, y, z
    public List<MobSpawnData> mobSpawnDatas;
}

[Serializable]
public class MobSpawnData
{
    public MobSpawnData(MobData mob, PositionData positionData)
    {
        this.mobData = mob;
        this.positionData = positionData;
    }

    public MobData mobData;
    public PositionData positionData;
}

public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    
    public static Queue<Block> blockPool = new Queue<Block>(12*12*125 *10);

    public Block blockPrefab;

    public PositionData playerPositionData;
    private List<Chunk> chunks = new List<Chunk>();
    private Chunk playerChunk;
    public int[,,] blocks = new int[Chunk.x, Chunk.y, Chunk.z]; // x, y, z
    public ChunkData chunkData;
    private BlockData blockData;
    private BlockData.BlockName blockKind;
    private Block block;
    private Vector3 blockPosition = new Vector3();
    private Vector3 position2 = new Vector3();
    Array enumValues = System.Enum.GetValues(enumType: typeof(BlockData.BlockName));
    private int value;


    MobSpawnData temp_MobSpawnData;
    Mob temp_Mob;

    private GameObject greedyMeshingObject;
    private MeshFilter meshFilter_GreedyMeshingObject;
    private MeshRenderer meshRenderer_GreedyMeshingObject;
    
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
        Load_StartChunks();
        Transform temp = new GameObject("StartPool").transform;
        for (int i = 0; i < 100; i++)
        {
            block = Instantiate(blockPrefab, Vector3.one * -999, Quaternion.identity, temp);
            blockPool.Enqueue(block);
        }

        greedyMeshingObject = new GameObject("greedyMeshingObject");
        meshFilter_GreedyMeshingObject = greedyMeshingObject.AddComponent<MeshFilter>();
        meshRenderer_GreedyMeshingObject = greedyMeshingObject.AddComponent<MeshRenderer>();
        //greedyMeshingObject.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        meshFilter_GreedyMeshingObject.mesh = mesh;


        playerPositionData = new PositionData(0, 0, 0, 0, 0);
        playerPositionData = GetSpawnPositionY(playerPositionData);
        PlayerManager.instance.player.transform.position = GetObjectPosition(playerPositionData.chunk, playerPositionData.blockIndex_x, playerPositionData.blockIndex_y, playerPositionData.blockIndex_z);
    }

    public bool createBlock = false;
    public int chunkIndex;
    public BlockData.BlockName blockKind1;
    public Vector3 chunkBlockIndex;

    Chunk temp_Chunk;

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
        for (int i = 0; i < chunks.Count; i++)
        {
            StopCoroutine(chunks[i].saveRoutine);
            SaveChunk(chunks[i].chunk_X, chunks[i].chunk_Z, chunks[i]);
        }
    }

    #region Chunk Load, Save 메서드

    public void UpdateLoadChunk()
    {
        if (playerPositionData.chunk == playerChunk)
        {
            for (int i = chunks.Count-1; i >= 0; i--)
            {
                if (Vector3.Distance(GetObjectPosition(chunks[i],6,playerPositionData.blockIndex_y,6), PlayerManager.instance.player.transform.position) > 50)
                {
                    Remove_Chunk(chunks[i]);
                    chunks.RemoveAt(i);
                }
            }
            return;
        }
        else
        {

            #region 왼쪽으로 이동했을떄
            if (playerPositionData.chunk_X < playerChunk.chunk_X)
            {
                // 청크 데이터 로드
                InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z + 1);
                InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z);
                InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z - 1);

                #region 왼쪽 아래 (대각선) 이동
                if (playerPositionData.chunk_Z < playerChunk.chunk_Z)
                {
                    // 왼쪽 대각선 아래
                    InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z - 2);
                    InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z - 3);
                    InitChunk(playerChunk.chunk_X - 2, playerChunk.chunk_Z - 3);

                    // -------------------------------------------------------------

                    // 아래
                    InitChunk(playerChunk.chunk_X - 1, playerChunk.chunk_Z - 3);
                    InitChunk(playerChunk.chunk_X, playerChunk.chunk_Z - 3);

                    //// -----------------------------------------------------------

                    // 오른쪽 아래
                    InitChunk(playerChunk.chunk_X + 1, playerChunk.chunk_Z - 3);

                }
                #endregion
                #region 왼쪽으로 만 이동
                else if (playerPositionData.chunk_Z == playerChunk.chunk_Z)
                {
                    // 왼쪽 아래
                    InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z + 2);

                    // --------------------------------------------------------------------------------------

                    // 왼쪽 위
                    InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z - 2);

                }
                #endregion
                #region 왼쪽으로 위 이동
                else
                {
                    // 왼쪽
                    InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z + 2);
                    InitChunk(playerChunk.chunk_X - 3, playerChunk.chunk_Z + 3);
                    InitChunk(playerChunk.chunk_X - 2, playerChunk.chunk_Z + 3);

                    // -------------------------------------------------------------

                    // 아래
                    InitChunk(playerChunk.chunk_X - 1, playerChunk.chunk_Z -+3);
                    InitChunk(playerChunk.chunk_X, playerChunk.chunk_Z + 3);

                    //// -----------------------------------------------------------

                    // 오른쪽 아래
                    InitChunk(playerChunk.chunk_X + 1, playerChunk.chunk_Z + 3);
                }
                #endregion

            }
            #endregion
            #region Z축으로 이동
            else if (playerPositionData.chunk_X == playerChunk.chunk_X)
            {

                #region 아래로 이동
                if (playerPositionData.chunk_Z < playerChunk.chunk_Z)
                {
                    InitChunk(playerChunk.chunk_X + 2, playerChunk.chunk_Z - 3);
                    InitChunk(playerChunk.chunk_X + 1, playerChunk.chunk_Z - 3);

                    // --------------------------------------------------------------------------------------

                    // 아래
                    InitChunk(playerChunk.chunk_X, playerChunk.chunk_Z - 3);

                    // --------------------------------------------------------------------------------------

                    // 왼쪽 아래
                    InitChunk(playerChunk.chunk_X - 1, playerChunk.chunk_Z - 3);
                    InitChunk(playerChunk.chunk_X - 2, playerChunk.chunk_Z - 3);

                }
                #endregion
                #region 위로 이동
                else if (playerPositionData.chunk_Z > playerChunk.chunk_Z)
                {
                    // 오른쪽 위
                    InitChunk(playerChunk.chunk_X + 2, playerChunk.chunk_Z + 3);
                    InitChunk(playerChunk.chunk_X + 1, playerChunk.chunk_Z + 3);

                    // --------------------------------------------------------------------------------------

                    // 위
                    InitChunk(playerChunk.chunk_X, playerChunk.chunk_Z + 3);

                    // --------------------------------------------------------------------------------------

                    // 왼쪽 위
                    InitChunk(playerChunk.chunk_X - 1, playerChunk.chunk_Z + 3);
                    InitChunk(playerChunk.chunk_X - 2, playerChunk.chunk_Z + 3);
                }
                #endregion
            }
            #endregion
            #region 오른쪽으로 이동했을떄
            else
            {
                // 청크 데이터 로드
                InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z + 1);
                InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z);
                InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z - 1);

                #region 오른쪽 아래 (대각선) 이동
                if (playerPositionData.chunk_Z < playerChunk.chunk_Z)
                {
                    // 오른쪽 대각선 아래
                    InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z - 2);
                    InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z - 3);
                    InitChunk(playerChunk.chunk_X + 2, playerChunk.chunk_Z - 3);

                    // -------------------------------------------------------------

                    // 아래
                    InitChunk(playerChunk.chunk_X + 1, playerChunk.chunk_Z - 3);
                    InitChunk(playerChunk.chunk_X, playerChunk.chunk_Z - 3);

                    //// -----------------------------------------------------------

                    // 왼쪽 아래
                    InitChunk(playerChunk.chunk_X - 1, playerChunk.chunk_Z - 3);
                }
                #endregion
                #region 오른쪽으로 만 이동
                else if (playerPositionData.chunk_Z == playerChunk.chunk_Z)
                {
                    // 오른쪽 아래
                    InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z + 2);

                    // --------------------------------------------------------------------------------------

                    // 오른쪽 위
                    InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z - 2);
                }
                #endregion
                #region 오른쪽으로 위 (대각선) 이동
                else
                {
                    // 오른쪽
                    InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z + 2);
                    InitChunk(playerChunk.chunk_X + 3, playerChunk.chunk_Z + 3);
                    InitChunk(playerChunk.chunk_X + 2, playerChunk.chunk_Z + 3);

                    // -------------------------------------------------------------

                    // 아래
                    InitChunk(playerChunk.chunk_X + 1, playerChunk.chunk_Z - +3);
                    InitChunk(playerChunk.chunk_X, playerChunk.chunk_Z + 3);

                    //// -----------------------------------------------------------

                    // 왼쪽 아래
                    InitChunk(playerChunk.chunk_X - 1, playerChunk.chunk_Z + 3);

                }
                #endregion
            }
            #endregion

            playerChunk = playerPositionData.chunk;

            SpawnChunkMonsterData();
        }

    }

    public void Load_StartChunks()
    {
        //SetPlayerChunk();
        int index = 0;
        for (int z = -2; z < 3; z++)
        {
            for (int x = -2; x < 3; x++)
            {
                LoadChunk(x+ playerPositionData.chunk_X, z + playerPositionData.chunk_Z); // 청크파일 로드 후 blocks에서 블럭 데이터 셋팅
                temp_Chunk = new Chunk((int)(x), (int)(z), chunkData);
                if(x == 0  && z == 0)
                {
                    playerChunk = temp_Chunk;
                }
                chunks.Add(temp_Chunk);
                InitChunk_CreateBlocks(temp_Chunk);
                StartCoroutine(temp_Chunk.saveRoutine);
                index++;
            }
        }

        SpawnChunkMonsterData();
    }

    int block_y = 5;
    bool checkY = false;
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
        BinaryFormatter bf = new BinaryFormatter();
        try
        {
            FileStream file = File.Open(Application.dataPath + "/Chunk" + (chunk_X) + "_" + (chunk_Z) + ".binary", FileMode.Open);
            chunkData = (ChunkData)bf.Deserialize(file);
            file.Close();
        }
        catch (FileNotFoundException error)
        {
            for (int x = 0; x < Chunk.x; x++)
            {
                for (int y = 0; y < Chunk.y; y++)
                {
                    if (y < block_y)
                    {
                        if (y == 0)
                            value = (int)BlockData.BlockName.Stone;
                        else
                        {
                            if (Random.value < 0.01f)
                            {
                                value = (int)BlockData.BlockName.DiamondOre;
                            }
                            else if (Random.value < 0.05f)
                            {
                                value = (int)BlockData.BlockName.GoldOre;
                            }
                            else if (Random.value < 0.1)
                            {
                                value = (int)BlockData.BlockName.IronOre;
                            }
                            else
                            {
                                value = (int)BlockData.BlockName.Stone;
                            }
                        }
                    }
                    else if (y < block_y + 3)
                    {
                        value = (int)BlockData.BlockName.Dirt;
                    }
                    else
                        value = (int)BlockData.BlockName.None;

                    for (int z = 0; z < Chunk.z; z++)
                    {
                        blocks[x, y, z] = value;
                    }
                }
            }
            chunkData = new ChunkData(blocks);
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
        bf.Serialize(file, chunk.chunkData);
        file.Close();
    }
    private void CreateChunk(string path, Chunk chunk)
    {
        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                if (y < block_y)
                {
                    if(y == 0)
                        value = (int)BlockData.BlockName.Stone;
                    else
                    {
                        if(Random.value < 0.1f)
                        {
                            value = (int)BlockData.BlockName.DiamondOre;
                        }
                        else if(Random.value < 0.5f)
                        {
                            value = (int)BlockData.BlockName.GoldOre;
                        }
                        else if(Random.value < 1)
                        {
                            value = (int)BlockData.BlockName.IronOre;
                        }
                    }
                }
                else if (y < block_y + 3)
                {
                    value = (int)BlockData.BlockName.Dirt;
                }
                else
                    value = (int)BlockData.BlockName.None;

                for (int z = 0; z < Chunk.z; z++)
                {
                    blocks[x, y, z] = value;
                }

            }

        }
        if(block_y > 35)
        {
            checkY = true;
        }
        else if(block_y < 30)
        {
            checkY = false;
        }

        if (checkY)
        {
            block_y--;
        }
        else
        {
            block_y++;
        }
        ChunkData chunkData = new ChunkData(blocks);

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/" + path + ".binary");
        bf.Serialize(file, chunkData);
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


    private void InitChunk(int x, int z)
    {
        LoadChunk(x, z);
        temp_Chunk = new Chunk(x, z, chunkData);
        chunks.Add(temp_Chunk);
        InitChunk_CreateBlocks(temp_Chunk);
        StartCoroutine(temp_Chunk.saveRoutine);
    }

    // 청크에 있는 모든 블럭 스폰
    private void InitChunk_CreateBlocks(Chunk chunk)
    {
        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                for (int z = 0; z < Chunk.z; z++)
                {
                    blockKind = (BlockData.BlockName)chunk.chunkData.blocksEnum[x, y, z]; // 블럭 enum 가져오기
                    CreateBlock(chunk, blockKind, x, y, z);
                }
            }
        }
    }
    // 특정 블럭 생성
    public void CreateBlock(Chunk chunk, BlockData.BlockName blockKind, int x, int y, int z)
    {
        if (blockKind != 0)
        {
            if (!DataManager.instance.blockDictionary.ContainsKey(blockKind)) // 블럭 dictonary에 해당 블럭 데이터 없으면 메서드 탈출
                return;

            chunk.chunkData.blocksEnum[x, y, z] = (int)blockKind;
            blockPosition = new Vector3(chunk.chunk_X * Chunk.x + x, y + Chunk.defaultY, chunk.chunk_Z * Chunk.z + z); // index 값을 사용해 위치
            blockData = DataManager.instance.blockDictionary[blockKind]; // 블럭 dictonary에서 해당되는 블럭 데이터 가져오기

            if (blockPool.Count > 0)
            {
                block = blockPool.Dequeue();
                block.transform.position = blockPosition;
                block.transform.SetParent(chunk.blockParent);
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
            if (chunk.chunkData.blocksEnum[x,y,z] != 0)
            {
                BreakBlock(chunk.blockObjects[x, y, z]);
            }
        }
    }

    public void BreakBlock(Block block)
    {
        Chunk chunk = block.positionData.chunk;
        chunk.chunkData.blocksEnum[block.positionData.blockIndex_x, block.positionData.blockIndex_y, block.positionData.blockIndex_z] = 0;
        chunk.blockObjects[block.positionData.blockIndex_x, block.positionData.blockIndex_y, block.positionData.blockIndex_z] = null;
        block.gameObject.SetActive(false);
        blockPool.Enqueue(block);
    }

    public void Remove_Chunk(Chunk chunk) // 사각형 블럭만 가능
    {
        for (int i = 0; i < chunk.mobGameObjects.Count; i++)
        {
            SpawnManager.instance?.RemoveMob(chunk.mobGameObjects[i]);
        }
        chunk.mobGameObjects.Clear();

        StopCoroutine(chunk.saveRoutine); // 오른쪽 끝 청크 저장 타이머 종류 후 저장
        SaveChunk(chunk.chunk_X, chunk.chunk_Z, chunk);

        chunk.blockParent.gameObject.SetActive(false);

        // 풀에 블럭 리턴
        for (int x = 0; x < Chunk.x; x++)
        {
            for (int y = 0; y < Chunk.y; y++)
            {
                for (int z = 0; z < Chunk.z; z++)
                {
                    block = chunk.blockObjects[x, y, z];
                    if (block != null)
                    {
                        blockPool.Enqueue(block);
                    }
                }

            }

        }
    }

    private void SpawnChunkMonsterData()
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            if(chunks[i].initMonster == false)
            {
                List<MobSpawnData> mobSpawnDatas_Temp = new List<MobSpawnData>();
                if(chunks[i].chunkData.mobSpawnDatas == null)
                {
                    chunks[i].chunkData.mobSpawnDatas = new List<MobSpawnData>();
                }

                if (chunks[i].chunkData.mobSpawnDatas.Count != 0)
                {
                    for (int j = 0; j < chunks[i].chunkData.mobSpawnDatas.Count; j++)
                    {
                        if(SpawnManager.instance.passiveMonsterCount + SpawnManager.instance.hostileeMonsterCount >= SpawnManager.instance.monsterMaxCount)
                        {
                            break;
                        }
                        temp_MobSpawnData = chunks[i].chunkData.mobSpawnDatas[j];
                        temp_Mob = DataManager.instance.GetMobPrefab(temp_MobSpawnData.mobData.mobKind);
                        temp_Mob = Instantiate(temp_Mob,
                            GetObjectPosition(temp_MobSpawnData.positionData.chunk, temp_MobSpawnData.positionData.blockIndex_x, temp_MobSpawnData.positionData.blockIndex_y, temp_MobSpawnData.positionData.blockIndex_z),
                            Quaternion.Euler(0, UnityEngine.Random.value * 360, 0), SpawnManager.instance.transform);
                        temp_Mob.initEntitiy(chunks[i].chunk_X, chunks[i].chunk_Z, temp_MobSpawnData.positionData.blockIndex_x, temp_MobSpawnData.positionData.blockIndex_y, temp_MobSpawnData.positionData.blockIndex_z);
                        mobSpawnDatas_Temp.Add(temp_Mob.mobSpawnData);
                        chunks[i].mobGameObjects.Add(temp_Mob);
                        SpawnManager.instance.AddMob(temp_Mob);
                    }
                }
                chunks[i].chunkData.mobSpawnDatas = mobSpawnDatas_Temp;
                chunks[i].initMonster = true;
            }
        }
    }

    public void SpawnMonster(MobData.MobKind mobKind, PositionData positionData)
    {
        if (positionData.chunk == null)
        {
            return;
        }
        temp_Mob = DataManager.instance.GetMobPrefab(mobKind);
        temp_Mob = Instantiate(temp_Mob,
            GetObjectPosition(positionData.chunk, positionData.blockIndex_x, positionData.blockIndex_y, positionData.blockIndex_z),
            Quaternion.Euler(0, UnityEngine.Random.value * 360, 0), SpawnManager.instance.transform);
        temp_Mob.initEntitiy(positionData.chunk_X, positionData.chunk_Z, positionData.blockIndex_x, positionData.blockIndex_y, positionData.blockIndex_z);
        positionData.chunk.AddMobSpawnData(temp_Mob.mobSpawnData, temp_Mob);
        SpawnManager.instance.AddMob(temp_Mob);
    }

    #endregion

    public Vector3 GetObjectPosition(Chunk chunk, int x, int y, int z)
    {
        try
        {
            return new Vector3(chunk.chunk_X * Chunk.x + x, y + Chunk.defaultY, chunk.chunk_Z * Chunk.z + z);
        }
        catch(Exception e){

        }
        return Vector3.zero;  // index 값을 사용해 위치 설정
    }
    public Vector3 GetObjectPosition(int chunk_x, int chunk_z, int x, int y, int z)
    {
        return new Vector3(chunk_x * Chunk.x + x, y + Chunk.defaultY, chunk_z * Chunk.z + z); // index 값을 사용해 위치 설정
    }


    public PositionData PositionToBlockData(Vector3 objectPosition)
    {
        int chunk_X, chunk_Z;
        int blockIndex_X, blockIndex_Z;
        if (Mathf.RoundToInt(objectPosition.x) >= 0)
        {
            chunk_X = Mathf.RoundToInt(objectPosition.x) / Chunk.x;
            blockIndex_X = Mathf.RoundToInt(objectPosition.x) % Chunk.x;
        }
        else
        {
            chunk_X = ((Mathf.RoundToInt(objectPosition.x + 1) - Chunk.x) / Chunk.x);
            blockIndex_X = Chunk.x + Mathf.RoundToInt(objectPosition.x) % Chunk.x;
        }
        if (Mathf.RoundToInt(objectPosition.z) >= 0)
        {
            chunk_Z = Mathf.RoundToInt(objectPosition.z) / Chunk.z;
            blockIndex_Z = Mathf.RoundToInt(objectPosition.z) % Chunk.z;
        }
        else
        {
            chunk_Z = ((Mathf.RoundToInt(objectPosition.z + 1) - Chunk.z) / Chunk.z);
            blockIndex_Z = Chunk.z + (Mathf.RoundToInt(objectPosition.z) % Chunk.z);
        }
        if (blockIndex_X > 11)
        {
            blockIndex_X = 0;
        }
        if(blockIndex_Z > 11)
        {
            blockIndex_Z = 0;
        }
   
        PositionData positionData = new PositionData(chunk_X,chunk_Z,blockIndex_X,(int)objectPosition.y - Chunk.defaultY,blockIndex_Z);

        return positionData;
    }

    public PositionData GetSpawnPositionY(PositionData positionData)
    {
        for (int i = Chunk.y-1; i >= 0; i--)
        {
            if(positionData.chunk.chunkData.blocksEnum[positionData.blockIndex_x, i, positionData.blockIndex_z] != 0)
            {
                positionData.blockIndex_y = i+1;
                return positionData;
            }
        }
        positionData.blockIndex_y = 0;
        return positionData;
    }





    #region 블럭 검사

    public Chunk GetChunk(int x, int z)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            if (chunks[i].chunk_X == x && chunks[i].chunk_Z == z)
            {
                return chunks[i];
            }
        }

        return null; // 해당 청크를 찾지 못했음
    }

    // 머리 위에 블럭이 있는지
    public bool CheckJump(PositionData positionData, int objectHeight)
    {
        if (positionData.chunk == null)
        {
            return false;
        }

        if (positionData.blockIndex_y + objectHeight >= Chunk.y) // 월드 최대 높이보다 높다면
        {
            return false;
        }
        if (positionData.blockIndex_x > 11 || positionData.blockIndex_y > 11)
        {
            print(positionData.blockIndex_x + " , " + positionData.blockIndex_y);
        }
        int e;
        try
        {
            e = positionData.chunk.chunkData.blocksEnum[positionData.blockIndex_x, positionData.blockIndex_y + objectHeight, positionData.blockIndex_z];
        }
        catch(IndexOutOfRangeException error)
        {
            return false;
        }
        if (e == 0) // 머리위에 블럭이 없으면 점프 가능
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
            try
            {
                // 블럭 검사
                if (chunk.chunkData.blocksEnum[x, index, z] == 0)
                    groundCheck[i] = 0; // 블럭 없음
                else
                    groundCheck[i] = 1; // 블럭 있음
            }
            catch(IndexOutOfRangeException erro)
            {
                moveData.weight = int.MaxValue;
                return moveData;
            }
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



    #region Greedy Meshing

    private void InitMesh(Chunk chunk)
    {
        
    }

    private void EditMesh()
    {

    }
    

    #endregion
}

public class MeshData
{


    void GenerateMesh(ChunkData chunkData)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        bool[,,] visited = new bool[Chunk.x, Chunk.y, Chunk.z];

        // x,y,z 방향에 대해 면을 병합
        for (int axis = 0; axis < 3; axis++)
        {
            // X, Y, Z 축을 기준으로 다른 두 축(u, v)을 따라 면을 검사하는 방식
            // axis = 0 => y, z
            // axis = 1 => z, x
            // axis = 2 => x, y
            int u = (axis + 1) % 3; // y 또는 z
            int v = (axis + 2) % 3; // z 또는 x

            int[] dims = { Chunk.x, Chunk.y, Chunk.z };

            int[] x = new int[3]; // 현재 블록 위치
            int[] q = new int[3]; // 다음 블록 위치

            // 병행 진행 체크
            q[axis] = 1;

            // 현재 axis 방향을 따라 한 층씩 블록을 검사
            for (x[axis] = -1; x[axis] < dims[axis];)
            {
                int[,] mask = new int[dims[u], dims[v]]; // 현재 진행 면의 UV Mask 배열 초기화
                x[axis]++;

                // 블럭검사
                for (x[u] = 0; x[u] < dims[u]; x[u]++) // axis가 0(x축)일 때 y축 검사
                {
                    for (x[v] = 0; x[v] < dims[v]; x[v]++) // axis가 0(x축)일 때 z축 검사
                    {
                        int blockCurrent = (x[axis] >= 0 && x[axis] < dims[axis]) ? chunkData.blocksEnum[x[0], x[1], x[2]] : 0;
                        int blockNext = (x[axis] + 1 < dims[axis]) ? chunkData.blocksEnum[x[0] + q[0], x[1] + q[1], x[2] + q[2]] : 0;

                        // 둘 다 없거나 있으면 면을 안그려도 됨
                        if ((blockCurrent > 0) != (blockNext > 0))
                        {
                            // 출력할 면 지정 블럭 있으면 바깥면, 없으면 안쪽면
                            mask[x[u], x[v]] = blockCurrent > 0 ? blockCurrent : -blockNext;
                        }
                        else
                        {
                            mask[x[u], x[v]] = 0;
                        }
                    }
                }
                // 마스크를 기반으로 면 병합
                for (int i = 0; i < dims[u]; i++)
                {
                    for (int j = 0; j < dims[v];)
                    {
                        // 그릴 면이 있다면
                        if (mask[i, j] != 0)
                        {
                            int w, h;
                            // 그릴 면에서 부터 이어지는 면까지 검사
                            for (w = 1; i + w < dims[u] && mask[i + w, j] == mask[i, j]; w++) { }
                            // 그릴 면에서 부터 이어지는 면까지 검사
                            for (h = 1; j + h < dims[v] && mask[i, j + h] == mask[i, j]; h++) { }

                            // 면 추가
                            //Vector3 origin = new Vector3(x[0], x[1], x[2]);
                            Vector3 origin = Vector3.zero;
                            origin[axis] = x[axis];

                            Vector3 du = Vector3.zero, dv = Vector3.zero;
                            du[u] = w;
                            dv[v] = h;

                            AddFace(vertices, triangles, origin, du, dv, mask[i, j] > 0);

                            // 병합된 영역을 0으로 설정
                            for (int dw = 0; dw < w; dw++)
                                for (int dh = 0; dh < h; dh++)
                                    mask[i + dw, j + dh] = 0;

                            j += h;
                        }
                        else
                        {
                            j++;
                        }
                    }
                }
            }
        }

        // 생성된 데이터를 Unity Mesh에 적용
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }
    void AddFace(List<Vector3> vertices, List<int> triangles, Vector3 pos, Vector3 du, Vector3 dv, bool frontFace)
    {
        int index = vertices.Count;

        vertices.Add(pos);
        vertices.Add(pos + du);
        vertices.Add(pos + dv);
        vertices.Add(pos + du + dv);

        if (frontFace)
        {
            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 1);
            triangles.Add(index + 3);
            triangles.Add(index + 2);
        }
        else
        {
            triangles.Add(index + 2);
            triangles.Add(index + 1);
            triangles.Add(index + 0);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
            triangles.Add(index + 1);
        }
    }
}