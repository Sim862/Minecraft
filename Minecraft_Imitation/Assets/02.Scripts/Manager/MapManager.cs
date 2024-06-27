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


public class Chunk
{
    public static readonly float saveTime = 12; // 청크 저장 대기시간

    // 청크 크기
    public static readonly int x = 12;
    public static readonly int y = 256;
    public static readonly int z = 12;

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

    public bool create = false;
    public int chunkIndex;
    public Vector3 i;
    private void Update()
    {
        if (create)
        {
            create = false;
            CreateBlock(chunks[chunkIndex], BlockData.BlockKind.Dirt, (int)i.x, (int)i.y, (int)i.z);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            StopCoroutine(chunks[i].saveRoutine);
            SaveChunk("Chunk" + chunks[i].chunk_x + "_" + chunks[i].chunk_x, chunks[i]);
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
                SaveChunk("Chunk" + ((int)x + playerChunckVector.x) + "_" + ((int)z + playerChunckVector.z), chunks[index]);
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

    public void CreateBlock(Chunk chunk, BlockData.BlockKind blockKind, int x, int y, int z)
    {
        if (blockKind != 0)
        {
            if (!DataManager.instance.blockDictionary.ContainsKey(blockKind)) // 블럭 dictonary에 해당 블럭 데이터 없으면 메서드 탈출
                return;

            chunk.blocksEnum[x, y, z] = (int)blockKind;

            blockPosition = new Vector3(chunk.chunk_x * Chunk.x + x, y - 60, chunk.chunk_z * Chunk.z + z); // index 값을 사용해 위치 설정
            block = Instantiate(blockPrefab, blockPosition, Quaternion.identity, chunk.blockParent); // 블럭 오브젝트 생성

            blockData = DataManager.instance.blockDictionary[blockKind]; // 블럭 dictonary에서 해당되는 블럭 데이터 가져오기
            block.InitBlock(blockData); // 블럭 데이터의 설정값으로 블럭 오브젝트 설정
            chunk.blockObjects[x, y, z] = block; // 블럭 3차원 배열에 블럭 오브젝트 저장
            chunk.needSave = true;

        }
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
        if(y + objectHeight >= Chunk.y) // 최대 높이보다 높다면
        {
            return false;
        }

        if (chunk.blocksEnum[x,y+1,z] == 0) // 블럭이 없으면 점프 가능
        {
            return true;
        }

        return false; ; 
    }

    public int CheckBlockY(Chunk chunk, int x, int y, int z, int objectHeight, int fallHeight, bool canJump)
    {
        //int check = fallHeight + objectHeight; // 높이 체크용
        //bool ground = false; // 땅이 있는지

        //if ((y - fallHeight - 1) >0) 
        //{
        //    if (chunk.blocksEnum[x, y - fallHeight - 1,z] == 0) // 땅이 있으면 체크
        //    {
        //        ground = true;
        //    }
        //}


        //for (int i = -fallHeight; i <= 1; i++)
        //{
        //    int checkY = y + i; // 이동할 블럭 높이

        //    if (checkY < 0) // 이동할 블럭 높이 인덱스가 0보다 작다면 다시 반복 
        //    {
        //        continue;

        //    }

        //    if (chunk.blocksEnum[x, checkY, z] == 0) // 해당 위치에 블럭이 없다면
        //    {
        //        if (checkY < y) // 현재 높이보다 낮다면 
        //        {
        //            check--;
        //        }
        //        else // 현재 높이보다 이동 높이가 같거나 크다면 
        //        {
        //            // 현재 위치와 같거나 높아졌지만 체크가 더 크다는 것은
        //            // 낮은 위치 중 블럭이 있었다는 것.
        //            // 그러므로 오브젝트가 이동할 수 있는지 오브젝트의 높이만 체크하면 됨
        //            if (checkY > objectHeight)
        //            {
        //                check = objectHeight;
        //            }

        //            check--;
        //        }
        //    }
        //    else 
        //    {
        //        // 여기에 이동할 수 없는 블럭 리스트 체크 (물, 용암)


        //        // 이동할 수 있으면
        //        // 땅이 있음을 체크
        //        ground = true;
        //    }
        //}
        //if (!ground)
        //{
        //    return false; // 땅이 없음
        //}

        //if (check <= 0) // 충분히 이동 가능
        //{
        //    return true;
        //}

        //return false;
        

        int[] isGround = new int[fallHeight + objectHeight + 1]; // 이동할 위치에 있는 검사할 블럭 리스트
        int index = y + objectHeight + 1; // 블럭데이터 인덱스 위부터
        for (int i = isGround.Length-1; i <= 0; i--)
        {
            // 이동 위치가 최대 높이보다 높으면 못감
            if(index >= Chunk.y)
            {
                isGround[i] = -1;
                index--;
                continue;
            }
            else if(index <= 0) // 이동 위치가 최소 높이보다 낮을때
            {
                isGround[i] = -1;
                continue;
            }

            // 현재 높이보다 이동할 위치가 높은데 점프가 안되면 못감 (가장 높음)
            if(i == isGround.Length-1) 
            {
                if (!canJump)
                {
                    isGround[i] = -1;
                    index--;
                    continue;
                }

                if (chunk.blocksEnum[x, index, z] == 0)
                    isGround[i] = 0;
                else
                    isGround[i] = -1;

                index--;
            }
            else if(i > fallHeight) // 이동할 높이가 내 위치보다 높다면 (가장 높은건 아님)
            {
                for (int j = i; j < isGround.Length - 1; j++) //  이동할 높이보다 높은 위치에 블럭이 있으면 못감
                {
                    if (isGround[i] != 0)
                    {
                        return int.MaxValue;  // maxvalue를 줘 못감을 표시
                    }
                }
            }


            isGround[i] = chunk.blocksEnum[x, index, z];
            index++;
        }
    }

    #endregion


}
