using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public Transform DirectionalLight;

    public bool gameStart { get; set; }

    public Transform sun;
    public Transform moon;
    public Transform[] cloud;
    private List<Transform> clouds = new List<Transform>();
    private Transform playerTransform;

    float cheat = 1f;
    public int day = 0;
    public float time = 0;

    private bool changeSpawnMaxCount = false;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    void Start()
    {
        gameStart = true;
        SoundManager.instance.ActiveBGM(false);
        playerTransform = PlayerManager.instance.player.transform;
        ActiveCloud();
        StartCoroutine(BGMCycle());
        
        // 몹 스폰 시작
        SpawnManager.instance.StartSpawnMob();
    }

    private void Update()
    {
        if (gameStart)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                cheat = 100f;
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                cheat = 1f;
            }
            DayCycle();
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void ActiveCloud()
    {
        if (playerTransform != null)
        {
            for (int i = 0; i < cloud.Length; i++)
            {
                clouds.Add(Instantiate(cloud[i], new Vector3(playerTransform.position.x + Random.Range(-1500, 1500), Random.Range(100, 150), playerTransform.position.z + Random.Range(-1500, 1500)), cloud[i].rotation, transform));
            }
        }
    }

    private void MoveCloud()
    {
        for (int i = 0; i < clouds.Count; i++)
        {
            clouds[i].transform.position += Vector3.forward * Time.deltaTime * 3;
            if(Vector3.Distance(new Vector3(clouds[i].position.x, 0, clouds[i].position.z), new Vector3(playerTransform.position.x, 0 , playerTransform.position.z)) > 1600)
            {
                clouds[i].position = (playerTransform.position - clouds[i].position).normalized * 1500;
                
            }
        }
    }
   
    private void DayCycle()
    {
        if (DirectionalLight != null)
        DirectionalLight.Rotate(Vector3.right * Time.deltaTime * cheat);
        time += Time.deltaTime*cheat;
        if (time > 360)
        {
            time -= 360;
            day++; 
            changeSpawnMaxCount = false;
            SpawnManager.instance.monsterMaxCount /= 2;
        }
        else if (time > 180 && !changeSpawnMaxCount)
        {
            changeSpawnMaxCount = true;
            SpawnManager.instance.monsterMaxCount *= 2;
        }

        if (playerTransform != null)
        {
            MoveSunAndMoon();
            MoveCloud();
        }
        if(day >= 10)
        {
            gameStart = false;
            SceneManager.LoadScene("EndScene");
        }
    }

    private void MoveSunAndMoon()
    {
        sun.position = playerTransform.position + (-DirectionalLight.forward * 1000f);
        sun.LookAt(playerTransform);

        moon.position = playerTransform.position + (DirectionalLight.forward * 1000f);
        moon.LookAt(playerTransform);

    }

    private void ActiveBGM()
    {
        SoundManager.instance.ActiveBGM();
    }

    IEnumerator BGMCycle()
    {
        while (true)
        {
            if (SoundManager.instance.BGMActiveChecK())
            {
                ActiveBGM();
            }
            yield return new WaitForSeconds(Random.Range(30, 60));
        }
    }
}
