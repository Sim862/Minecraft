using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Node(int chunck_X, int chunck_Z, int x, int y, int z, int g, int h)
    {
        this.chunck_X = chunck_X;
        this.chunck_Z = chunck_Z;

        this.x = x;
        this.y = y;
        this.z = z;
        
        this.g = g;
        this.h = h;
    }

    public bool PositionCheck(int x, int y, int z)
    {
        if (this.x != x)
            return false;
        if (this.y != y)
            return false;
        if (this.z != z)
            return false;

        return true;
    }

    public int chunck_X, chunck_Z;
    public int x,y,z;
    public int g, h;
    public int f { get { return g + h; } }
}

public class Entity : MonoBehaviour
{
    public int chunk_x, chunk_z;
    public int blockIndex_x, blockIndex_y, blockIndex_z;
    public int detectionDistance = 8;

    public int objectHeight = 1;
    public int fallHeight = 3;

    public List<Node> finalNodeList;
    private List<Node> opentNodes;
    private List<Node> closedNode;

    private Node current;
    private Chunk currentChunk;
    private Chunk chunk;


    private int local_TargetBlockIndex_x;
    private int local_TargetBlockIndex_y;
    private int local_TargetBlockIndex_z;
    int gap = 0;
    int minF;
    int minF_Index;


    public void initEnitiy(int chunk_x, int chunk_z, int blockIndex_x, int blockIndex_y, int blockIndex_z)
    {
        this.chunk_x = chunk_x;
        this.chunk_z = chunk_z;
        currentChunk = MapManager.instance.GetChunk(chunk_x, chunk_z);
        this.blockIndex_x = blockIndex_x;
        this.blockIndex_y = blockIndex_y;
        this.blockIndex_z = blockIndex_z;
    }

    private static int x, y, z;
    public static int GetH(int current_x, int current_y, int current_z, int target_x, int target_y, int target_z)
    {
        x = Mathf.Abs(current_x - target_x);
        y = Mathf.Abs(current_y - target_y);
        z = Mathf.Abs(current_z - target_z);
        return x + y + z;
    }

    private Vector3[] AStar(Chunk targetChunk, int targetBlockIndex_x, int targetBlockIndex_y, int targetBlockIndex_z, Transform target)
    {
        // 타겟 위치
        local_TargetBlockIndex_x = targetBlockIndex_x;
        local_TargetBlockIndex_y = targetBlockIndex_y;
        local_TargetBlockIndex_z = targetBlockIndex_z;
        // 내 청크 위치 X와 타켓의 청크 위치 X가 다르다면 다른만큼 계산해서 변경
        if (chunk_x == targetChunk.chunk_x)
        {
            gap = targetChunk.chunk_x - chunk_x;
            local_TargetBlockIndex_x += Chunk.x * gap;
        }
        // 내 청크 위치 Z와 타켓의 청크 위치 Z가 다르다면 다른만큼 계산해서 변경
        if (chunk_z == targetChunk.chunk_z)
        {
            gap = targetChunk.chunk_z - chunk_z;
            local_TargetBlockIndex_z += Chunk.x * gap;
        }

        current = new Node(chunk_x, chunk_z, blockIndex_x, blockIndex_y, blockIndex_z, 0,
            GetH(blockIndex_x, blockIndex_y, blockIndex_z, local_TargetBlockIndex_x, local_TargetBlockIndex_y, local_TargetBlockIndex_z));
        opentNodes.Add(current);

        while (true)
        {
            minF = int.MaxValue;

            for (int i = 0; i < opentNodes.Count; i++)
            {
                // f가 제일 작은 오픈노드 가져오기
                if (minF > opentNodes[i].f)
                {
                    minF = opentNodes[i].f;
                    minF_Index = i;
                }
            }

            if (minF == int.MaxValue)
                break; // 오픈노드가 없다면 길없음

            current = opentNodes[minF_Index];
            opentNodes.RemoveAt(minF_Index);
            closedNode.Add(current);

            if (current.PositionCheck(local_TargetBlockIndex_x, local_TargetBlockIndex_y, local_TargetBlockIndex_z))
                break;

            bool canJump = MapManager.instance.CheckJump(currentChunk, current.x, current.y, current.z, objectHeight);

        }
    }

    // 위치가 변경된 노드가 이동 가능한 노드인지 확인하고 오픈노드 리스트에 추가
    private void AddOpenNodes(int index_x, int index_z, bool canJump)
    {
        int chunk_x = current.chunck_X;
        int chunk_z = current.chunck_Z;
        if (index_x < 0)
        {
            chunk_x--;
            index_x = 11;
        }
        else if (index_x > 11)
        {
            chunk_x++;
            index_x = 0;
        }
        if (index_z < 0)
        {
            chunk_z--;
            index_z = 11;
        }
        else if (index_z > 11)
        {
            chunk_z++;
            index_z = 0;
        }


        chunk = MapManager.instance.GetChunk(chunk_x, chunk_z);

        if (MapManager.instance.CheckBlock(chunk, index_x, current.y, index_z, objectHeight, fallHeight, canJump) != int.MaxValue)
        {

        }
    }
}
