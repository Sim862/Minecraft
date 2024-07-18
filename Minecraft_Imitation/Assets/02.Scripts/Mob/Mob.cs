using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

[System.Serializable]
public class Node
{
    public Node(PositionData positionData, int g, int h, bool needJump, Node parent)
    {
        this.parent = parent;

        this.positionData = new PositionData(positionData.chunk_X,positionData.chunk_Z,positionData.blockIndex_x,positionData.blockIndex_y,positionData.blockIndex_z);


        this.g = g;
        this.h = h;
        this.needJump = needJump;

    }

    public Node parent;
    public PositionData positionData;
    public int g, h;
    public bool needJump;
    public int f { get { return g + h; } }
}

[System.Serializable]
public class DropItem
{
    public ObjectParticleData.ParticleName dropItem;
    public float probability;
}

public class Mob : MonoBehaviour
{
    protected enum MobState
    {
        Idle,
        Move,
        Attack,
        Hit,
    }

    public Rigidbody rigidbody;
    public Animator animator;

    public MobData mobData { get; private set; }
    public MobData.MobKind mobKind;

    public DropItem[] dropItems;

    public ParticleSystem deathParticle;

    public int detectionDistance = 8;

    protected MobState mobState;

    public bool alive = false;
    public float maxHP = 100;
    public float currHP = 0;
    public float runSpeed = 3f;
    public float normalSpeed = 1.5f;
    public float currSpeed = 1.5f;
    public float rotationSpeed = 300;
    public int objectHeight = 1;
    public int fallHeight = 3;

    public float nextMovementTime = 3;
    public float movementDelayTime = 4;
    public bool needJump;

    // 현재 위치 데이터
    public MobSpawnData mobSpawnData;
    private Chunk currentChunk;

    private static float fallspeedCriteria = -7f;
    private float minVelocity_Y = 0;


    public static float KnockBackPower = 50f;
    protected Transform target;
    private Quaternion targetAngle;

    private Node wayPoint_Current;
    public Vector3 wayPosition = Vector3.zero;
    private Vector3 dir;
    private float wayPositionDistance = 0;
    protected List<Node> wayPoints = new List<Node>();

    private List<Node> openNodes;
    private List<Node> closedNode;

    private Node current_temp;
    private Node nearNode_temp;
    private Chunk chunk_temp;
    private MoveData moveData_temp;

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

    #region 머리 애니메이션
    public Transform head;
    protected Quaternion headDefaultRotation;
    private Vector3 eulerAngle;
    private bool resetHeadRotation = false;

    #endregion

    #region 사운드
    public Sound.AudioClipName idleSound;
    public Sound.AudioClipName deathSound;
    #endregion



    private void Awake()
    {
        mobData = new MobData(mobKind);
        rigidbody = GetComponent<Rigidbody>();
        if (head != null)
        {
            headDefaultRotation = head.rotation;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    public bool init_test = false;
    public bool start_test = false;
    public int c_x, c_z;
    public int b_x, b_y, b_z;
    //public int tc_x, tc_z;
    //public int tb_x, tb_y, tb_z;


    public Node[] finalNodeList;

    public void initEntitiy(int chunk_x, int chunk_z, int blockIndex_x, int blockIndex_y, int blockIndex_z)
    {
        mobSpawnData = new MobSpawnData(mobData, new PositionData(chunk_x, chunk_z, blockIndex_x, blockIndex_y, blockIndex_z));
        transform.position = MapManager.instance.GetObjectPosition(chunk_x, chunk_z, blockIndex_x, blockIndex_y, blockIndex_z);
        currHP = maxHP;
        mobState = MobState.Idle;
        rigidbody.useGravity = true;
        alive = true;

        StartCoroutine(CheckInMap());
    }

    protected void Rotation()
    {

        if (targetTransform != null)
        {
            dir = (targetTransform.position - transform.position).normalized;

            Vector3 cross = Vector3.Cross(transform.forward, new Vector3(targetTransform.position.x - transform.position.x, 0, targetTransform.position.z - transform.position.z).normalized);
            if (cross.y > 0.35)
            {
                transform.Rotate(new Vector3(0, 100 * Time.deltaTime, 0));
            }
            else if (cross.y < -0.35)
            {
                transform.Rotate(new Vector3(0, -100 * Time.deltaTime, 0));
            }
            else
            {
                if (Vector3.Distance(transform.forward, dir) > 0.8) // 내가 뒤를 보고 있을때
                {
                    transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
                }
                else
                {
                    if (head != null)
                    {
                        resetHeadRotation = true;
                        Quaternion lookRoation = Quaternion.LookRotation(targetTransform.position - head.position);
                        //eulerAngle = lookRoation.eulerAngles;
                        //eulerAngle = new Vector3(eulerAngle.y, 0, 0);
                        head.rotation = Quaternion.Slerp(head.rotation, lookRoation, Time.deltaTime * 3);
                    }
                }
            }
        }
        else
        {
            if (resetHeadRotation)
            {
                resetHeadRotation = false;
                head.rotation = headDefaultRotation;
            }
        }
    }

    public void SetChunkData()
    {
        if (currentChunk != mobSpawnData.positionData.chunk)
        {
            if (currentChunk != null)
            {
                if (mobSpawnData.positionData.chunk != null)
                {
                    currentChunk.RemoveMobSpawnData(mobSpawnData, this);
                    currentChunk = mobSpawnData.positionData.chunk;
                    currentChunk.AddMobSpawnData(mobSpawnData, this);
                }
            }
            else
            {
                currentChunk = mobSpawnData.positionData.chunk;
                currentChunk.AddMobSpawnData(mobSpawnData, this);
            }
        }
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

    protected void Death()
    {
        StartCoroutine(Coroutine_Death());
    }

    private IEnumerator Coroutine_Death()
    {
        float angleCheck = 0;
        while (angleCheck < 90)
        {
            transform.Rotate(0, 0, 6);
            angleCheck += 6;
            yield return new WaitForSeconds(0.01f);
        }
        if (deathParticle != null)
        {
            ParticleSystem temp = Instantiate(deathParticle, transform.position, Quaternion.identity);
            Destroy(temp, 2);
        }

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < dropItems.Length; i++)
        {
            if (Random.value < dropItems[i].probability)
            {
                Rigidbody temp = Instantiate(DataManager.instance.GetObjectParticlePrefab(dropItems[i].dropItem),
                    new Vector3(transform.position.x + Random.Range(-0.2f,0.2f), transform.position.y, transform.position.z + Random.Range(-0.2f, 0.2f)),
                    Quaternion.identity, SpawnManager.instance.transform).GetComponent<Rigidbody>();
                temp.AddForce(Vector3.up * 100);
            }
        }

        mobSpawnData.positionData.chunk.RemoveMobSpawnData(mobSpawnData, this);
        SpawnManager.instance.RemoveMob(this);

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

    public int blockEnum;
    protected void SetWayPosition()
    {
        if (wayPoints.Count > 0)
        {
            mobState = MobState.Move;
            wayPoint_Current = wayPoints[0];
            wayPoints.RemoveAt(0);

            //print(gameObject.name + " - " + wayPoint_Current.positionData.chunk_X + " : " + wayPoint_Current.positionData.chunk_Z + " : " + wayPoint_Current.positionData.blockIndex_x + " ; " + wayPoint_Current.positionData.blockIndex_z);
            wayPosition = MapManager.instance.GetObjectPosition(wayPoint_Current.positionData.chunk_X, wayPoint_Current.positionData.chunk_Z,
                wayPoint_Current.positionData.blockIndex_x, wayPoint_Current.positionData.blockIndex_y, wayPoint_Current.positionData.blockIndex_z);
            try
            {
                blockEnum = MapManager.instance.GetChunk(wayPoint_Current.positionData.chunk_X, wayPoint_Current.positionData.chunk_Z).chunkData.blocksEnum[wayPoint_Current.positionData.blockIndex_x, wayPoint_Current.positionData.blockIndex_y, wayPoint_Current.positionData.blockIndex_z];
            }
            catch(Exception error)
            {
                if (animator != null)
                {
                    animator.SetBool("Move", false);
                }
                wayPosition = Vector3.zero;
                needJump = false;
                if (mobState == MobState.Move)
                {
                    mobState = MobState.Idle;

                    currSpeed = normalSpeed;
                    //nextMovementTime = Random.Range(3f, 10f);
                    nextMovementTime = Random.Range(1f, 2f);

                }
            }
            needJump = wayPoint_Current.needJump;
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("Move", false);
            }
            wayPosition = Vector3.zero;
            needJump = false;
            if (mobState == MobState.Move)
            {
                mobState = MobState.Idle;

                currSpeed = normalSpeed;
                //nextMovementTime = Random.Range(3f, 10f);
                nextMovementTime = Random.Range(1f, 2f);

            }
        }
    }

    public float jumpforce = 10;
    protected void Movement()
    {

        if(wayPosition != Vector3.zero && mobState != MobState.Attack)
        {
            wayPositionDistance = Vector3.Distance(transform.position, wayPosition);
            if (wayPositionDistance > 2) // 이동할 위치가 한블럭 보다 크다 = 내 위치가 변했다 -> 이동 취소
            {
                movementDelayTime = 4;
                wayPoints = new List<Node>();
                wayPosition = Vector3.zero;
                SetWayPosition();
                return;
            }
            else if (wayPositionDistance < 0.3f)
            {
                movementDelayTime = 4;
                SetWayPosition();
            }
            else
            {
                movementDelayTime -= Time.deltaTime;
                if (needJump)
                {
                    if (animator != null)
                    {
                        animator.SetBool("Move", false);
                    }
                    if (wayPositionDistance > 1f)
                    {
                        if (wayPosition.y - transform.position.y < -0.2)
                        {
                            rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
                            dir = new Vector3((wayPosition.x - transform.position.x), 0, (wayPosition.z - transform.position.z)).normalized;
                            transform.position += dir * currSpeed * Time.deltaTime;
                        }
                        else
                        {
                            rigidbody.AddForce(Vector3.up * 50);
                        }

                    }
                    else
                    {
                        if(wayPosition.y - transform.position.y > 0.1)
                        {
                            rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0 ,rigidbody.velocity.z);
                        }
                        dir = new Vector3((wayPosition.x - transform.position.x), 0, (wayPosition.z - transform.position.z)).normalized;
                        transform.position += dir * currSpeed * Time.deltaTime;
                    }
                }
                else
                {
                    if (animator != null)
                    {
                        animator.SetBool("Move", true);
                    }

                    dir = new Vector3((wayPosition.x - transform.position.x), 0, (wayPosition.z - transform.position.z)).normalized;
                    transform.position += dir * currSpeed * Time.deltaTime;
                }

                dir = (wayPosition - transform.position).normalized;

                 Vector3 cross = Vector3.Cross(transform.forward, new Vector3(wayPosition.x - transform.position.x, 0, wayPosition.z - transform.position.z).normalized);
                if (cross.y > 0.35)
                {
                    transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
                }
                else if (cross.y < -0.35)
                {
                    transform.Rotate(new Vector3(0, -rotationSpeed * Time.deltaTime, 0));
                }
                else
                {
                    if(Vector3.Distance(transform.forward, dir) > 0.8) // 내가 뒤를 보고 있을때
                    {
                        transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
                    }
                    else
                    {

                        transform.rotation = Quaternion.LookRotation(new Vector3(wayPosition.x-transform.position.x, 0, wayPosition.z - transform.position.z).normalized, Vector3.up);
                    }
                }
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
                SoundManager.instance.ActiveOnShotSFXSound(deathSound, transform, transform.position);
                currHP = 0;
                alive = false;
                Death();
            }
            else
            {
                SoundManager.instance.ActiveOnShotSFXSound(idleSound, transform, transform.position);
                if (target != null) // Vector zero는 낙하데미지
                {
                    SoundManager.instance.ActiveOnShotSFXSound(idleSound, null, transform.position);
                    SoundManager.instance.ActiveOnShotSFXSound(Sound.AudioClipName.Player_Attack, null, target.position);
                    mobState = MobState.Hit;
                    this.target = target;
                    rigidbody.AddForce(Vector3.up * 100);
                    rigidbody.AddForce((transform.position - target.position).normalized  * KnockBackPower * force);
                    nextMovementTime = 1f;
                }
                else
                {
                    SoundManager.instance.ActiveOnShotSFXSound(Sound.AudioClipName.Landing, null, transform.position);
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
        current_temp = new Node(mobSpawnData.positionData, 0, 
            GetH(mobSpawnData.positionData.blockIndex_x + (mobSpawnData.positionData.chunk_X * Chunk.x),mobSpawnData.positionData.blockIndex_y, mobSpawnData.positionData.blockIndex_z + (mobSpawnData.positionData.chunk_Z * Chunk.z),
            targetPositionData.blockIndex_x + (targetPositionData.chunk_X * Chunk.x), targetPositionData.blockIndex_y, targetPositionData.blockIndex_z) + (targetPositionData.chunk_Z * Chunk.z), false, null);
        openNodes.Add(current_temp);
        nearNode_temp = current_temp;


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
                        if(nearNode_temp.h > minH)
                        {
                            nearNode_temp = openNodes[i];
                        }
                    }
                }
            }

            if (minF == int.MaxValue || count <= 0)
            {
                current_temp = nearNode_temp;
                break; // 오픈노드가 없다면 길없음
            }

            current_temp = openNodes[min_Index];
            openNodes.RemoveAt(min_Index);
            closedNode.Add(current_temp);


            if (current_temp.positionData.CheckSamePosition(targetPositionData))
                break;

            canJump = MapManager.instance.CheckJump(current_temp.positionData, objectHeight);


            // 대각선 이동 검사를 위한 bool 값
            plus_X = false;
            minus_X = false;
            plus_Z = false;
            minus_Z = false;

            // 동서남북 검사
            plus_X = AddOpenNodes(current_temp.positionData.blockIndex_x + 1, current_temp.positionData.blockIndex_z, canJump, targetPositionData);
            minus_X = AddOpenNodes(current_temp.positionData.blockIndex_x - 1, current_temp.positionData.blockIndex_z, canJump, targetPositionData);
            plus_Z = AddOpenNodes(current_temp.positionData.blockIndex_x, current_temp.positionData.blockIndex_z + 1, canJump, targetPositionData);
            minus_Z = AddOpenNodes(current_temp.positionData.blockIndex_x, current_temp.positionData.blockIndex_z - 1, canJump, targetPositionData);

        }

        wayPoints.Clear();
        if (current_temp == null)
        {
            current_temp = nearNode_temp;
        }
        while (current_temp != null)
        {
            wayPoints.Add(current_temp);
            current_temp = current_temp.parent;
        }
        wayPoints.Reverse();
    }

    protected void AStar_Random(int runawayDistanceCount = 0)
    {
        // 도망갈 블럭 수
        if (runawayDistanceCount == 0)
        {
            runawayDistanceCount = Random.Range(2, 7);
        }

        openNodes = new List<Node>();
        closedNode = new List<Node>();

        // 시작 위치 셋팅
        current_temp = new Node(mobSpawnData.positionData, 0, 0, false, null);
        openNodes.Add(current_temp);
        nearNode_temp = current_temp;


        // 탐색 시작
        while (runawayDistanceCount > 0)
        {
            runawayDistanceCount--;
            minF = int.MaxValue;

            if (openNodes.Count > 0)
            {
                nearNode_temp = openNodes[Random.Range(0, openNodes.Count)];
                minF = nearNode_temp.f;
            }


            if (minF == int.MaxValue)
            {
                current_temp = null;
                break; // 오픈노드가 없다면 길없음
            }

            current_temp = nearNode_temp;
            openNodes.Remove(nearNode_temp);
            closedNode.Add(nearNode_temp);

            openNodes.Clear();
            //if (current.positionData.blockIndex_x > 11 || current.positionData.blockIndex_z > 11)
            //{
            //    print(gameObject.name + " - " + current.positionData.blockIndex_x + " , " + current.positionData.blockIndex_y + " , " + current.positionData.blockIndex_z);
            //}
            canJump = MapManager.instance.CheckJump(current_temp.positionData, objectHeight);

            plus_X = AddOpenNodes(current_temp.positionData.blockIndex_x + 1, current_temp.positionData.blockIndex_z, canJump);
            minus_X = AddOpenNodes(current_temp.positionData.blockIndex_x - 1, current_temp.positionData.blockIndex_z, canJump);
            plus_Z = AddOpenNodes(current_temp.positionData.blockIndex_x, current_temp.positionData.blockIndex_z + 1, canJump);
            minus_Z = AddOpenNodes(current_temp.positionData.blockIndex_x, current_temp.positionData.blockIndex_z - 1, canJump);

        }

        wayPoints.Clear();
        if (current_temp == null)
        {
            current_temp = nearNode_temp;
        }
        while (current_temp != null)
        {
            wayPoints.Add(current_temp);
            current_temp = current_temp.parent;
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
        current_temp = new Node(mobSpawnData.positionData, 0, 0,false, null);
        openNodes.Add(current_temp);
        nearNode_temp = current_temp;


        // 탐색 시작
        while (runawayDistanceCount > 0)
        {
            runawayDistanceCount--;
            minF = int.MaxValue;

            if (openNodes.Count > 0)
            {
                nearNode_temp = openNodes[Random.Range(0, openNodes.Count)];
                minF = nearNode_temp.f;
            }


            if (minF == int.MaxValue)
            {
                current_temp = null;
                break; // 오픈노드가 없다면 길없음
            }
            current_temp = nearNode_temp;
            openNodes.Remove(nearNode_temp);
            closedNode.Add(nearNode_temp);

            openNodes.Clear();

            Vector3 runawayDirection = (transform.position - target.position).normalized;

            canJump = MapManager.instance.CheckJump(current_temp.positionData, objectHeight);

            if (runawayDirection.x > 0)
            {
                AddOpenNodes(current_temp.positionData.blockIndex_x + 1, current_temp.positionData.blockIndex_z, canJump);
            }
            else
            {
                AddOpenNodes(current_temp.positionData.blockIndex_x - 1, current_temp.positionData.blockIndex_z, canJump);
            }

            if (runawayDirection.z > 0)
            {
                AddOpenNodes(current_temp.positionData.blockIndex_x, current_temp.positionData.blockIndex_z + 1, canJump);
            }
            else
            {
                AddOpenNodes(current_temp.positionData.blockIndex_x, current_temp.positionData.blockIndex_z - 1, canJump);
            }

        }

        wayPoints.Clear();
        if (current_temp == null)
        {
            current_temp = nearNode_temp;
        }
        while (current_temp != null)
        {
            wayPoints.Add(current_temp);
            current_temp = current_temp.parent;
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

        int chunk_X = current_temp.positionData.chunk_X;
        int chunk_Z = current_temp.positionData.chunk_Z;
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
       
        chunk_temp = MapManager.instance.GetChunk(chunk_X, chunk_Z);
        if(chunk_temp == null)
        {
            return false;
        }
        moveData_temp = MapManager.instance.CheckBlock(chunk_temp, index_x,  current_temp.positionData.blockIndex_y, index_z, objectHeight, fallHeight, canJump);

        if (moveData_temp.weight == int.MaxValue) // 벽이라서 못감
        {
            return false;
        }

        PositionData afterPositionData = new PositionData(chunk_X, chunk_Z, index_x, moveData_temp.afterIndexY, index_z);

        foreach (var item in closedNode)
        {
            if (item.positionData.CheckSamePosition(afterPositionData)) // 방문 한적 있음
            {
                return false;
            }
        }

        int g = current_temp.g + moveData_temp.weight;
        int h;
        bool j = false;
        if(moveData_temp.weight >= 20)
        {
            j = true;
        }

        if (targetPositionData != null)
        {
            h = GetH(index_x + (chunk_X * Chunk.x), moveData_temp.afterIndexY, index_z + (chunk_Z * Chunk.z),
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
                    item.parent = current_temp;
                    return true;
                }
            }
        }
        if(index_x > 11 || index_x < 0 || index_z > 11 || index_z < 0)
        {
            print(index_x + " , " + moveData_temp.afterIndexY + " , " + index_x);
        }
        Node temp = new Node(afterPositionData, g, h, j, current_temp);

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

    protected IEnumerator CheckInMap()
    {
        while (alive)
        {
            yield return new WaitForSeconds(5);
            if(transform.position.y < -70)
            {
                mobSpawnData.positionData.chunk?.RemoveMobSpawnData(mobSpawnData, this);
                SpawnManager.instance.RemoveMob(this);
            }
        }
    }
    #endregion
}
