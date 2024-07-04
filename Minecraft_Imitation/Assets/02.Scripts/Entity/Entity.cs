using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

using Random = UnityEngine.Random;

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
    protected enum EntityState
    {
        Idle,
        Move,
        Attack,
        Hit,
    }

    public Rigidbody rigidbody;

    public MobData mobData;
    public MobData.MobKind mobKind;

    public PositionData positionData;

    public int detectionDistance = 8;

    protected EntityState entityState;

    public float maxHP = 100;
    public float currHP = 0;
    public float runSpeed = 3f;
    public float normalSpeed = 1.5f;
    public float currSpeed = 1.5f;
    public float rotationSpeed = 300;
    public int objectHeight = 1;
    public int fallHeight = 3;

    public float movementDelayTime = 3;

    private static float fallspeedCriteria = -7f;
    private float minVelocity_Y = 0;


    public static float KnockBackPower = 50f;
    protected Transform target;
    private Quaternion targetAngle;

    private Node wayPoint_Current;
    private Vector3 wayPosition = Vector3.zero;
    private float wayPositionDistance = 0;
    protected List<Node> wayPoints = new List<Node>();

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

    bool canJump = false;
    // 대각선 이동 검사를 위한 bool 값
    bool plus_X = false;
    bool minus_X = false;
    bool plus_Z = false;
    bool minus_Z = false;


    // 시야 영역의 반지름
    public float viewRadius = 7;
    // 시야 각도
    [Range(0, 360)]
    public float viewAngle = 160;
   
    public LayerMask targetMask;  // 타겟으로 지정할 오브젝트들
    public LayerMask obstacleMask; // 시야의 장애물로 지정할 오브젝트들

    // 포착된 오브젝트 리스트
    public List<Transform> visibleTargets = new List<Transform>();

    protected Transform targetTransform;


    private void Awake()
    {
        mobData = new MobData(mobKind);
        rigidbody = GetComponent<Rigidbody>();
    }

    public bool init_test = false;
    public bool start_test = false;
    public int c_x, c_z;
    public int b_x, b_y, b_z;
    //public int tc_x, tc_z;
    //public int tb_x, tb_y, tb_z;


    public Node[] finalNodeList;

    //private void Update()
    //{
    //    if (init_test)
    //    {
    //        init_test = false;
    //        initEntitiy(c_x, c_z, b_x, b_y, b_z);
    //    }

    //    if (start_test)
    //    {
    //        start_test = false;
    //        PositionData nextPosition = new PositionData(tc_x, tc_z, tb_x, tb_y, tb_z);
    //        wayPoints = AStar_Runaway(target);
    //        finalNodeList = wayPoints.ToArray();
    //        SetWayPosition();
    //    }


    //    Movement();
    //}

    public void initEntitiy(int chunk_x, int chunk_z, int blockIndex_x, int blockIndex_y, int blockIndex_z)
    {
        positionData = new PositionData(chunk_x, chunk_z, blockIndex_x, blockIndex_y, blockIndex_z);

        transform.position = MapManager.instance.GetObjectPosition(positionData.chunk, blockIndex_x, blockIndex_y, blockIndex_z);
        currHP = maxHP;
        entityState = EntityState.Idle;
        rigidbody.useGravity = true;
    }
    protected void Fall()
    {
        if (rigidbody.velocity.y < minVelocity_Y)
        {
            minVelocity_Y = rigidbody.velocity.y;
        }
        else if (rigidbody.velocity.y == 0 && minVelocity_Y < fallspeedCriteria)
        {
            float dmg = (minVelocity_Y - fallspeedCriteria) * 0.2f;

            UpdateHP(null, dmg, 0);
            minVelocity_Y = 0;
        }
    }

    private void SetChunkPositionData()
    {
        PositionData positionData = MapManager.instance.PositionToBlockData(transform.position);
    }

    private int x, y, z;
    public int GetH(int worldPosition_current_x, int worldPosition_current_y, int worldPosition_current_z, 
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

    protected void SetWayPosition()
    {
        if (wayPoints.Count > 0)
        {
            entityState = EntityState.Move;
            wayPoint_Current = wayPoints[0];
            wayPoints.RemoveAt(0);

            wayPosition = MapManager.instance.GetObjectPosition(wayPoint_Current.positionData.chunk,
                wayPoint_Current.positionData.blockIndex_x, wayPoint_Current.positionData.blockIndex_y, wayPoint_Current.positionData.blockIndex_z);
        }
        else
        {
            wayPosition = Vector3.zero;
            if (entityState == EntityState.Move)
            {
                entityState = EntityState.Idle;

                currSpeed = normalSpeed;
                movementDelayTime = Random.Range(3f, 10f);
            }
        }
    }

    protected void Movement()
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
            transform.position += (wayPosition - transform.position).normalized * runSpeed * Time.deltaTime;
            Vector3 cross = Vector3.Cross(transform.forward, new Vector3(wayPosition.x - transform.position.x, 0, wayPosition.z - transform.position.z).normalized);

            if(cross.y > 0.1)
            {
                transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
            }
            else if(cross.y < -0.1)
            {
                transform.Rotate(new Vector3(0, -rotationSpeed * Time.deltaTime, 0));
            }
            positionData = MapManager.instance.PositionToBlockData(transform.position);
            
            wayPositionDistance = Vector3.Distance(transform.position, wayPosition);
            if(wayPositionDistance < 0.1f)
            {
                SetWayPosition();
            }
        }
    }


    protected void Runaway()
    {
        AStar_Runaway(null);
        SetWayPosition();
    }
    public void UpdateHP (Transform target, float dmg, float force)
    {
        currHP += dmg;

        if (dmg < 0)
        {

            if (currHP <= 0)
            {
                currHP = 0;
                // 죽는 이벤트 추가
            }
            else
            {
                if (target != null) // Vector zero는 낙하데미지
                {
                    entityState = EntityState.Hit;
                    this.target = target;
                    rigidbody.AddForce((transform.position - target.position).normalized+ Vector3.up * KnockBackPower * force);
                    movementDelayTime = 1.5f;
                }
            }
        }

        if (currHP > maxHP)
        {
            currHP = maxHP;
        }
    }



    #region A* 알고리즘
    protected void AStar(PositionData targetPositionData, Transform target)
    {
        
        int count = 30;

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


            if (current.positionData.CheckSamePosition(targetPositionData))
                break;

            canJump = MapManager.instance.CheckJump(current.positionData, objectHeight);


            // 대각선 이동 검사를 위한 bool 값
            plus_X = false;
            minus_X = false;
            plus_Z = false;
            minus_Z = false;

            // 동서남북 검사
            plus_X = AddOpenNodes(current.positionData.blockIndex_x + 1, current.positionData.blockIndex_z, canJump, targetPositionData);
            minus_X = AddOpenNodes(current.positionData.blockIndex_x - 1, current.positionData.blockIndex_z, canJump, targetPositionData);
            plus_Z = AddOpenNodes(current.positionData.blockIndex_x, current.positionData.blockIndex_z + 1, canJump, targetPositionData);
            minus_Z = AddOpenNodes(current.positionData.blockIndex_x, current.positionData.blockIndex_z - 1, canJump, targetPositionData);

        }

        wayPoints.Clear();
        if (current == null)
        {
            current = nearNode;
        }
        while (current != null)
        {
            wayPoints.Add(current);
            current = current.parent;
        }
        wayPoints.Reverse();
    }

    protected void AStar_Random()
    {
        // 도망갈 블럭 수
        int runawayDistanceCount = Random.Range(2,7);

        openNodes = new List<Node>();
        closedNode = new List<Node>();

        // 시작 위치 셋팅
        current = new Node(positionData, 0, 0, null);
        openNodes.Add(current);
        nearNode = current;


        // 탐색 시작
        while (runawayDistanceCount > 0)
        {
            runawayDistanceCount--;
            minF = int.MaxValue;

            if (openNodes.Count > 0)
            {
                nearNode = openNodes[Random.Range(0, openNodes.Count)];
                minF = nearNode.f;
            }


            if (minF == int.MaxValue)
            {
                current = null;
                break; // 오픈노드가 없다면 길없음
            }
            current = nearNode;
            openNodes.Remove(nearNode);
            closedNode.Add(nearNode);

            openNodes.Clear();

            print(current.positionData.chunk_X + ", " + current.positionData.chunk_Z + ", " +
                current.positionData.blockIndex_x + ", " + current.positionData.blockIndex_y + ", " + current.positionData.blockIndex_z);

            canJump = MapManager.instance.CheckJump(current.positionData, objectHeight);

            plus_X = AddOpenNodes(current.positionData.blockIndex_x + 1, current.positionData.blockIndex_z, canJump);
            minus_X = AddOpenNodes(current.positionData.blockIndex_x - 1, current.positionData.blockIndex_z, canJump);
            plus_Z = AddOpenNodes(current.positionData.blockIndex_x, current.positionData.blockIndex_z + 1, canJump);
            minus_Z = AddOpenNodes(current.positionData.blockIndex_x, current.positionData.blockIndex_z - 1, canJump);

        }

        wayPoints.Clear();
        if (current == null)
        {
            current = nearNode;
        }
        while (current != null)
        {
            wayPoints.Add(current);
            current = current.parent;
        }
        wayPoints.Reverse();
    }

    protected void AStar_Runaway(Transform target)
    {
        if(target == null)
        {
            target = this.target;
        }

        currSpeed = runSpeed;
        // 도망갈 블럭 수
        int runawayDistanceCount = 10;

        openNodes = new List<Node>();
        closedNode = new List<Node>();

        // 시작 위치 셋팅
        current = new Node(positionData, 0, 0, null);
        openNodes.Add(current);
        nearNode = current;


        // 탐색 시작
        while (runawayDistanceCount > 0)
        {
            runawayDistanceCount--;
            minF = int.MaxValue;

            if (openNodes.Count > 0)
            {
                nearNode = openNodes[Random.Range(0, openNodes.Count)];
                minF = nearNode.f;
            }


            if (minF == int.MaxValue)
            {
                current = null;
                break; // 오픈노드가 없다면 길없음
            }
            current = nearNode;
            openNodes.Remove(nearNode);
            closedNode.Add(nearNode);

            openNodes.Clear();

            Vector3 runawayDirection = (transform.position - target.position).normalized;

            print(current.positionData.chunk_X + ", " + current.positionData.chunk_Z + ", " +
                current.positionData.blockIndex_x + ", " + current.positionData.blockIndex_y + ", " + current.positionData.blockIndex_z);

            canJump = MapManager.instance.CheckJump(current.positionData, objectHeight);

            if (runawayDirection.x > 0)
            {
                AddOpenNodes(current.positionData.blockIndex_x + 1, current.positionData.blockIndex_z, canJump);
            }
            else
            {
                AddOpenNodes(current.positionData.blockIndex_x - 1, current.positionData.blockIndex_z, canJump);
            }

            if (runawayDirection.z > 0)
            {
                AddOpenNodes(current.positionData.blockIndex_x, current.positionData.blockIndex_z + 1, canJump);
            }
            else
            {
                AddOpenNodes(current.positionData.blockIndex_x, current.positionData.blockIndex_z - 1, canJump);
            }

        }

        wayPoints.Clear();
        if (current == null)
        {
            current = nearNode;
        }
        while (current != null)
        {
            wayPoints.Add(current);
            current = current.parent;
        }
        wayPoints.Reverse();
    }

    // 위치가 변경된 노드가 이동 가능한 노드인지 확인하고 오픈노드 리스트에 추가
    private bool AddOpenNodes(int index_x, int index_z, bool canJump, PositionData targetPositionData = null)
    {
        if (targetPositionData != null)
        {
            // 값이 오염되지 않게 깊은 복사
            targetPositionData = new PositionData(targetPositionData.chunk_X, targetPositionData.chunk_Z, targetPositionData.blockIndex_x,
                targetPositionData.blockIndex_y, targetPositionData.blockIndex_z);
        }

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
        int h;
        if (targetPositionData != null)
        {
            h = GetH(index_x + (chunk_X * Chunk.x), moveData.afterIndexY, index_z + (chunk_Z * Chunk.z),
            targetPositionData.blockIndex_x + (targetPositionData.chunk_X * Chunk.x), targetPositionData.blockIndex_y, targetPositionData.blockIndex_z + (targetPositionData.chunk_Z * Chunk.z));
        }
        else
        {
            h = 10;
        }
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
        Node temp = new Node(afterPositionData, g, h, current);

        openNodes.Add(temp);
        
        return true;
    }

    #endregion


    #region 시야각으로 오브젝트 탐색
    protected void FindVisibleTargets()
    {
        visibleTargets.Clear();
        // viewRadius를 반지름으로 한 원 영역 내 targetMask 레이어인 콜라이더를 모두 가져옴
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // 플레이어와 forward와 target이 이루는 각이 설정한 각도 내라면
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

                // 타겟으로 가는 레이캐스트에 obstacleMask가 걸리지 않으면 visibleTargets에 Add
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }

        if(visibleTargets.Count > 0)
        {
            targetTransform = visibleTargets[0];
        }
        else
        {
            targetTransform = null;
        }
    }

    protected IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    #endregion
}
