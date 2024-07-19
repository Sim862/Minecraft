using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public Transform DirectionalLight;

    public bool gameStart { get; private set; }

    public Transform sun;
    public Transform moon;
    public Transform[] cloud;
    private List<Transform> clouds = new List<Transform>();
    private Transform playerTransform;

    public int day = 0;
    public float time = 0;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
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
        DayCycle();
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
        DirectionalLight.Rotate(Vector3.right * Time.deltaTime);
        time += Time.deltaTime;
        if (time > 360)
        {
            time -= 360;
            day++;
        }
        if (playerTransform != null)
        {
            MoveSunAndMoon();
            MoveCloud();
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
        while (gameStart)
        {
            if (SoundManager.instance.BGMActiveChecK())
            {
                ActiveBGM();
            }
            yield return new WaitForSeconds(Random.Range(30, 60));
        }
    }
}
