using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class Node
{
    public Node(PositionData positionData, int g, int h, Node parent)
    {
        this.parent = parent;

        this.positionData = new PositionData(positionData.chunk_X,positionData.chunk_Z,positionData.blockIndex_x,positionData.blockIndex_y,positionData.blockIndex_z);


        this.g = g;
        this.h = h;
    }

    public Node parent;
    public PositionData positionData;
    public int g, h;
    public int f { get { return g + h; } }
}

public class Entity : MonoBehaviour
{
    public MobData mobData;
    public MobData.ObjectKind objectKind;
    public MobData.MobKind mobKind;

    public PositionData positionData;

    public int detectionDistance = 8;

    public float speed = 3f;
    public int objectHeight = 1;
    public int fallHeight = 3;

    private Node wayPoint_Current;
    private Vector3 wayPosition = Vector3.zero;
    private float wayPositionDistance = 0;
    private List<Node> wayPoints = new List<Node>();

    private List<Node> openNodes;
    private List<Node> closedNode;

    private Node current;
    private Node nearNode;
    private Chunk chunk;

    private MoveData moveData;

    private int local_TargetBlockIndex_x;
    private int local_TargetBlockIndex_y;
    private int local_TargetBlockIndex_z;
    private int gap = 0;
    private int minF;
    private int minH;
    private int min_Index;



    private void Awake()
    {
        mobData = new MobData(mobKind);
    }

    public bool init_test = false;
    public bool start_test = false;
    public int c_x, c_z;
    public int b_x, b_y, b_z;
    public int tc_x, tc_z;
    public int tb_x, tb_y, tb_z;
    public NewBehaviourScript1 trueWay;
    public NewBehaviourScript1 trueWay1;


    public List<Node> finalNodeList;

    private void Update()
    {
        if (init_test)
        {
            init_test = false;
            initEntitiy(c_x, c_z, b_x, b_y, b_z);
        }

        if (start_test)
        {
            start_test = false;
            PositionData nextPosition = new PositionData(tc_x, tc_z, tb_x, tb_y, tb_z);
            wayPoints = AStar(nextPosition, null);
            finalNodeList = wayPoints;
            SetWayPosition();
        }

        Movement();
    }

    public void initEntitiy(int chunk_x, int chunk_z, int blockIndex_x, int blockIndex_y, int blockIndex_z)
    {
        positionData = new PositionData(chunk_x, chunk_z, blockIndex_x, blockIndex_y, blockIndex_z);

        transform.position = MapManager.instance.GetObjectPosition(positionData.chunk, blockIndex_x, blockIndex_y, blockIndex_z);
    }

    private void SetChunkPositionData()
    {
        PositionData positionData = MapManager.instance.PositionToChunkData(transform.position);
    }

    private static int x, y, z;
    public static int GetH(int worldPosition_current_x, int worldPosition_current_y, int worldPosition_current_z, 
        int target_x, int target_y, int target_z)
    {
        x = Mathf.Abs(worldPosition_current_x - target_x);
        z = Mathf.Abs(worldPosition_current_z - target_z);
        
        y = 0;
        if(worldPosition_current_y > target_y) // 떨어지는건 높이 상관 없이 비용 10
        {
            y = 1;
        }
        else if(worldPosition_current_y < target_y) // 올라가는건 1블럭당 비용 10
        {
            y = target_y - worldPosition_current_y;
        }
        return (x + y + z)* 10;
    }

    private void SetWayPosition()
    {
        if(wayPoints.Count > 0)
        {
            wayPoint_Current = wayPoints[0];
            wayPoints.RemoveAt(0);

            wayPosition = MapManager.instance.GetBlockPosition(wayPoint_Current.positionData.chunk, 
                wayPoint_Current.positionData.blockIndex_x, wayPoint_Current.positionData.blockIndex_y, wayPoint_Current.positionData.blockIndex_z);
        }
        else
        {
            wayPosition = Vector3.zero;
        }
    }

    private void Movement()
    {
        if(wayPosition != Vector3.zero)
        {
            wayPositionDistance = Vector3.Distance(transform.position, wayPosition);
            if (wayPositionDistance > 1.5) // 이동할 위치가 한블럭 보다 크다 = 내 위치가 변했다 -> 이동 취소
            {
                wayPoints = new List<Node>();
                wayPosition = Vector3.zero;
                return;
            }

            transform.position += (wayPosition - transform.position).normalized * speed * Time.deltaTime;
            wayPositionDistance = Vector3.Distance(transform.position, wayPosition);
            if(wayPositionDistance < 0.3f)
            {
                SetWayPosition();
            }
        }
    }

    private List<Node> AStar(PositionData targetPositionData, Transform target)
    {
        int count = 100;

        // 리스트 초기화
        openNodes = new List<Node>();
        closedNode = new List<Node>();

       

        // 시작 위치 셋팅
        current = new Node(positionData, 0, GetH(positionData.blockIndex_x + (positionData.chunk_X * Chunk.x), positionData.blockIndex_y, positionData.blockIndex_z + (positionData.chunk_Z * Chunk.z),
            targetPositionData.blockIndex_x + (targetPositionData.chunk_X * Chunk.x), targetPositionData.blockIndex_y, targetPositionData.blockIndex_z) + (targetPositionData.chunk_Z * Chunk.z), null);
        openNodes.Add(current);
        nearNode = current;


        // 탐색 시작
        while (true)
        {
            count--;
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
                        if(nearNode.h > minH)
                        {
                            nearNode = openNodes[i];
                        }
                    }
                }
            }

            if (minF == int.MaxValue || count <= 0)
            {
                current = nearNode;
                break; // 오픈노드가 없다면 길없음
            }

            current = openNodes[min_Index];
            openNodes.RemoveAt(min_Index);
            closedNode.Add(current);


            //Instantiate(trueWay1, MapManager.instance.GetObjectPosition(current.positionData.chunk, current.positionData.blockIndex_x, current.positionData.blockIndex_y, current.positionData.blockIndex_z), Quaternion.identity);

            if (current.positionData.CheckSamePosition(targetPositionData))
                break;



            bool canJump = MapManager.instance.CheckJump(current.positionData, objectHeight);


            // 대각선 이동 검사를 위한 bool 값
            bool plus_X = false;
            bool minus_X = false;
            bool plus_Z = false;
            bool minus_Z = false;

            // 동서남북 검사
            plus_X = AddOpenNodes(current.positionData.blockIndex_x + 1, current.positionData.blockIndex_z, canJump, targetPositionData);
            minus_X = AddOpenNodes(current.positionData.blockIndex_x - 1, current.positionData.blockIndex_z, canJump, targetPositionData);
            plus_Z = AddOpenNodes(current.positionData.blockIndex_x, current.positionData.blockIndex_z + 1, canJump, targetPositionData);
            minus_Z = AddOpenNodes(current.positionData.blockIndex_x, current.positionData.blockIndex_z - 1, canJump, targetPositionData);

        }

        List<Node> way = new List<Node>();
        if(current == null)
        {
            current = nearNode;
        }
        while (current != null)
        {
            way.Add(current);
            current = current.parent;
        }
        way.Reverse();
        return way;
    }

    // 위치가 변경된 노드가 이동 가능한 노드인지 확인하고 오픈노드 리스트에 추가
    private bool AddOpenNodes(int index_x, int index_z, bool canJump, PositionData targetPositionData)
    {
        targetPositionData = new PositionData(targetPositionData.chunk_X, targetPositionData.chunk_Z, targetPositionData.blockIndex_x, 
            targetPositionData.blockIndex_y, targetPositionData.blockIndex_z);

        int chunk_X = current.positionData.chunk_X;
        int chunk_Z = current.positionData.chunk_Z;
        if (index_x < 0)
        {
            chunk_X--;
            index_x = 11;
        }
        else if (index_x > 11)
        {
            chunk_X++;
            index_x = 0;
        }
        if (index_z < 0)
        {
            chunk_Z--;
            index_z = 11;
        }
        else if (index_z > 11)
        {
            chunk_Z++;
            index_z = 0;
        }

        chunk = MapManager.instance.GetChunk(chunk_X, chunk_Z);
        moveData = MapManager.instance.CheckBlock(chunk, index_x,  current.positionData.blockIndex_y, index_z, objectHeight, fallHeight, canJump);

        if (moveData.weight == int.MaxValue) // 벽이라서 못감
        {
            return false;
        }

        PositionData afterPositionData = new PositionData(chunk_X, chunk_Z, index_x, moveData.afterIndexY, index_z);

        foreach (var item in closedNode)
        {
            if (item.positionData.CheckSamePosition(afterPositionData)) // 방문 한적 있음
            {
                return false;
            }
        }



        int g = current.g + moveData.weight;
        int h = GetH(index_x + (chunk_X * Chunk.x), moveData.afterIndexY, index_z + (chunk_Z * Chunk.z), 
            targetPositionData.blockIndex_x +(targetPositionData.chunk_X * Chunk.x), targetPositionData.blockIndex_y, targetPositionData.blockIndex_z + (targetPositionData.chunk_Z * Chunk.z));

        if(index_z == targetPositionData.blockIndex_z)
        // 오픈 노드 리스트에 같은 노드가 있을때
        foreach (var item in openNodes)
        {
            if (item.positionData.CheckSamePosition(afterPositionData))
            {
                if (item.g > g)
                {
                    item.positionData = afterPositionData;
                    item.g = g;
                    item.h = h;
                    item.parent = current;
                    return true;
                }
            }
        }

        NewBehaviourScript1 a = Instantiate(trueWay, MapManager.instance.GetObjectPosition(afterPositionData.chunk,afterPositionData.blockIndex_x,afterPositionData.blockIndex_y,afterPositionData.blockIndex_z), Quaternion.identity);
        a.name = c.ToString();
        a.h = h;
        a.g = g;
        c++;
        Node temp = new Node(afterPositionData, g, h, current);

        openNodes.Add(temp);
        
        return true;
    }
    int c = 0;

}
