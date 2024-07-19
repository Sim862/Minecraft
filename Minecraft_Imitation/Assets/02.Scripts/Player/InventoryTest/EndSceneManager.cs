using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToPlayScene()
    {
        GameManager.instance.day = 0;
        GameManager.instance.gameStart = true;
        SceneManager.LoadScene("MergeTestScene");
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("TitleScene");
        GameManager.instance.day = 0;
    }
}
