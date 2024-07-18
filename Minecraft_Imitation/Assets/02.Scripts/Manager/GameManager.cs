using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform DirectionalLight;
    private float bgm = 30;
    private bool bgmStart = false;

    public Transform[] cloud;
    private List<Transform> clouds = new List<Transform>();
    private Vector3 playerPos;
    void Start()
    {
        ActiveCloud();
        SoundManager.instance.ActiveBGM(false);
    }

    // Update is called once per frame
    void Update()
    {
        DirectionalLight.Rotate(Vector3.right * Time.deltaTime * 3);
        bgm -= Time.deltaTime;
        if(bgm < 0 && !bgmStart)
        {
            bgmStart = true;
            SoundManager.instance.ActiveBGM();
        }

        MoveCloud();
    }

    private void ActiveCloud()
    {
        playerPos = PlayerManager.instance.player.transform.position;
        for (int i = 0; i < cloud.Length; i++)
        {
            clouds.Add(Instantiate(cloud[i], new Vector3(playerPos.x + Random.Range(-1500, 1500), Random.Range(100, 150), playerPos.z + Random.Range(-1500, 1500)), cloud[i].rotation, transform));
        }
    }

    private void MoveCloud()
    {
        playerPos = PlayerManager.instance.player.transform.position;
        for (int i = 0; i < clouds.Count; i++)
        {
            clouds[i].transform.position += Vector3.forward * Time.deltaTime * 3;
            if(Vector3.Distance(new Vector3(clouds[i].position.x, 0, clouds[i].position.z), new Vector3(playerPos.x, 0 , playerPos.z)) > 1600)
            {
                clouds[i].position = (playerPos - clouds[i].position).normalized * 1500;
                
            }
        }
    }
}
