using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform DirectionalLight;
    private float bgm = 30;
    private bool bgmStart = false;

    public Transform sun;
    public Transform moon;
    public Transform[] cloud;
    private List<Transform> clouds = new List<Transform>();
    private Transform playerTransform;
    void Start()
    {
        SoundManager.instance.ActiveBGM(false);
        playerTransform = PlayerManager.instance.player.transform;
        ActiveCloud();
    }

    // Update is called once per frame
    void Update()
    {
        DirectionalLight.Rotate(Vector3.right * Time.deltaTime );
        //DirectionalLight_Moon.Rotate(Vector3.right * Time.deltaTime * 6);
        bgm -= Time.deltaTime;
        if (bgm < 0 && !bgmStart)
        {
            bgmStart = true;
            SoundManager.instance.ActiveBGM();
        }

        if (playerTransform != null)
        {
            MoveSunAndMoon();
            MoveCloud();
        }
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
   
    private void MoveSunAndMoon()
    {
        sun.position = playerTransform.position + (-DirectionalLight.forward * 1000f);
        sun.LookAt(playerTransform);

        moon.position = playerTransform.position + (DirectionalLight.forward * 1000f);
        moon.LookAt(playerTransform);

    }
}
