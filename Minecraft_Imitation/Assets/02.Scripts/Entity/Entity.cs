using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class Node
{
    public Node(int chunk_X, int chunk_Z, int x, int y, int z, int g, int h, Node parent)
    {
        this.parent = parent;

        this.chunk_X = chunk_X;
        this.chunk_Z = chunk_Z;

        this.x = x;
        this.y = y;
        this.z = z;
        
        this.g = g;
        this.h = h;
    }

    public bool CheckSamePosition(int chunk_X, int chunk_Z, int x, int y, int z)
    {
        if (this.chunk_X != chunk_X)
            return false;
        if (this.chunk_Z != chunk_Z)
            return false;
        if (this.x != x)
            return false;
        if (this.y != y)
            return false;
        if (this.z != z)
            return false;
        return true;
    }

    public Node parent;
    public int chunk_X, chunk_Z;
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

    public List<Node> nodeList;
    public List<Node> finalNodeList;
    private List<Node> openNodes;
    private List<Node> closedNode;

    private Node current;
    private Chunk currentChunk;
    private Chunk chunk;

    private MoveData moveData;

    private int local_TargetBlockIndex_x;
    private int local_TargetBlockIndex_y;
    private int local_TargetBlockIndex_z;
    private int gap = 0;
    private int minF;
    private int minH;
    private int min_Index;

    public bool init_test = false;
    public bool start_test = false;
    public int c_x, c_z;
    public int b_x, b_y, b_z;
    public int tc_x, tc_z;
    public int tb_x, tb_y, tb_z;
    public GameObject trueWay;
    public NewBehaviourScript1 openWay;


    public Node[] testNodes;
    private void Update()
    {
        if (init_test)
        {
            init_test = false;
            initEnitiy(c_x, c_z, b_x, b_y, b_z);
        }

        if (start_test)
        {
            start_test = false;
            testNodes = AStar(MapManager.instance.GetChunk(tc_x, tc_z), tb_x, tb_y, tb_z, null).ToArray();
        }
    }

    public void initEnitiy(int chunk_x, int chunk_z, int blockIndex_x, int blockIndex_y, int blockIndex_z)
    {
        this.chunk_x = chunk_x;
        this.chunk_z = chunk_z;
        currentChunk = MapManager.instance.GetChunk(chunk_x, chunk_z);
        this.blockIndex_x = blockIndex_x;
        this.blockIndex_y = blockIndex_y;
        this.blockIndex_z = blockIndex_z;

        transform.position = MapManager.instance.GetObjectPosition(currentChunk, blockIndex_x, blockIndex_y, blockIndex_z, objectHeight);
    }

    private static int x, y, z;
    public static int GetH(int current_x, int current_y, int current_z, int target_x, int target_y, int target_z)
    {
        x = Mathf.Abs(current_x - target_x);
        z = Mathf.Abs(current_z - target_z);
        
        y = 0;
        if(current_y > target_y) // 떨어지는건 높이 상관 없이 비용 10
        {
            y = 1;
        }
        else if(current_y < target_y) // 올라가는건 1블럭당 비용 10
        {
            y = target_y - current_y;
        }
        return (x + y + z)* 10;
    }

    private List<Node> AStar(Chunk targetChunk, int targetBlockIndex_x, int targetBlockIndex_y, int targetBlockIndex_z, Transform target)
    {
        // 리스트 초기화
        nodeList = new List<Node>();
        openNodes = new List<Node>();
        closedNode = new List<Node>();

        // 타겟 위치
        local_TargetBlockIndex_x = targetBlockIndex_x;
        local_TargetBlockIndex_y = targetBlockIndex_y;
        local_TargetBlockIndex_z = targetBlockIndex_z;
        // 내 청크 위치 X와 타켓의 청크 위치 X가 다르다면 다른만큼 계산해서 변경
        if (chunk_x != targetChunk.chunk_x)
        {
            gap = targetChunk.chunk_x - chunk_x;
            local_TargetBlockIndex_x += Chunk.x * gap;
        }
        // 내 청크 위치 Z와 타켓의 청크 위치 Z가 다르다면 다른만큼 계산해서 변경
        if (chunk_z != targetChunk.chunk_z)
        {
            gap = targetChunk.chunk_z - chunk_z;
            local_TargetBlockIndex_z += Chunk.z * gap;
        }

        // 시작 위치 셋팅
        current = new Node(chunk_x, chunk_z, blockIndex_x, blockIndex_y, blockIndex_z, 0,
            GetH(blockIndex_x, blockIndex_y, blockIndex_z, local_TargetBlockIndex_x, local_TargetBlockIndex_y, local_TargetBlockIndex_z), null);
        nodeList.Add(current);
        openNodes.Add(current);

        bool plus_X = false;
        bool minus_X = false;
        bool plus_Z = false;
        bool minus_Z = false;
        int sss = 0;
        // 탐색 시작
        while (true)
        {
            minF = int.MaxValue;
            minH = int.MaxValue;
            for (int i = 0; i < openNodes.Count; i++)
            {
                // f가 제일 작은 오픈노드 가져오기
                if (minF >= openNodes[i].f)
                {
                    if (minH > openNodes[i].h)
                    {
                        minF = openNodes[i].f;
                        minH = openNodes[i].h;
                        min_Index = i;
                    }
                }
            }

            if (minF == int.MaxValue)
            {
                current = null;
                break; // 오픈노드가 없다면 길없음
            }

            current = openNodes[min_Index];
            openNodes.RemoveAt(min_Index);
            closedNode.Add(current);
           
            GameObject temp = Instantiate(trueWay, MapManager.instance.GetObjectPosition(MapManager.instance.GetChunk(current.chunk_X, current.chunk_Z), current.x, current.y, current.z, objectHeight), Quaternion.identity);

            if (current.CheckSamePosition(targetChunk.chunk_x, targetChunk.chunk_z, targetBlockIndex_x, targetBlockIndex_y, targetBlockIndex_z))
                break;
            temp.name = "test" + sss++.ToString();
            bool canJump = MapManager.instance.CheckJump(MapManager.instance.GetChunk(current.chunk_X,current.chunk_Z), current.x, current.y, current.z, objectHeight);

            plus_X = AddOpenNodes(current.x + 1, current.z, canJump, targetChunk, targetBlockIndex_x, targetBlockIndex_y, targetBlockIndex_z);
            minus_X = AddOpenNodes(current.x - 1, current.z, canJump, targetChunk, targetBlockIndex_x, targetBlockIndex_y, targetBlockIndex_z);
            plus_Z = AddOpenNodes(current.x, current.z + 1, canJump, targetChunk, targetBlockIndex_x, targetBlockIndex_y, targetBlockIndex_z);
            minus_Z = AddOpenNodes(current.x, current.z - 1, canJump, targetChunk, targetBlockIndex_x, targetBlockIndex_y, targetBlockIndex_z);

        }

        List<Node> way = new List<Node>();
        while (current != null)
        {
            way.Add(current);
            current = current.parent;
        }

        way.Reverse();
        return way;
    }

    // 위치가 변경된 노드가 이동 가능한 노드인지 확인하고 오픈노드 리스트에 추가
    private bool AddOpenNodes(int index_x, int index_z, bool canJump, Chunk targetChunk, int targetBlockIndex_x, int targetBlockIndex_y, int targetBlockIndex_z)
    {
        int chunk_x = current.chunk_X;
        int chunk_z = current.chunk_Z;
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
        moveData = MapManager.instance.CheckBlock(chunk, index_x, current.y, index_z, objectHeight, fallHeight, canJump);

        if (moveData.weight == int.MaxValue) // 벽이라서 못감
        {
            return false;
        }

        foreach (var item in closedNode)
        {
            if (item.CheckSamePosition(chunk_x, chunk_z, index_x, moveData.afterIndexY, index_z)) // 방문 한적 있음
            {
                return false;
            }
        }



        // 인덱스를 전체 좌표로 변환
        if (chunk_x != targetChunk.chunk_x)
        {
            gap = targetChunk.chunk_x - chunk_x;
            targetBlockIndex_x += Chunk.x * gap;
        }
        if (chunk_z != targetChunk.chunk_z)
        {
            gap = targetChunk.chunk_z - chunk_z;
            targetBlockIndex_z += Chunk.z * gap;
        }

        Node temp = null;
        // 오픈 노드 리스트에 같은 노드가 있을때
        foreach (var item in nodeList)
        {
            if(item.CheckSamePosition(chunk_x, chunk_z, index_x, moveData.afterIndexY, index_z))
            {
                temp = item;
            }
        }
        int g = current.g + moveData.weight;
        int h = GetH(index_x, moveData.afterIndexY, index_z, targetBlockIndex_x, targetBlockIndex_y, targetBlockIndex_z);
        if (temp != null)
        {
            // 이번 g가 전에 설정한 g보다 작다면
            if (temp.g > g)
            {
                temp = new Node(chunk_x, chunk_z, index_x, moveData.afterIndexY, index_z, g,h, current);
            }
        }
        else
        {
            temp = new Node(chunk_x, chunk_z, index_x, moveData.afterIndexY, index_z, g, h, current);
            openNodes.Add(temp);
            nodeList.Add(temp);
        }

        NewBehaviourScript1 te = Instantiate(openWay, MapManager.instance.GetObjectPosition(MapManager.instance.GetChunk(chunk_x, chunk_z), index_x, 
            moveData.afterIndexY, index_z,objectHeight),Quaternion.identity);
        te.g = g;
        te.h = h;
        te.name = count.ToString();
        count++;
        return true;
    }

    int count = 0;
}
