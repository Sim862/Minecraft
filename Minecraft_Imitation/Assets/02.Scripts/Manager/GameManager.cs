using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform DirectionalLight;
    private float bgm = 30;
    private bool bgmStart = false;
    void Start()
    {
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
    }
}
