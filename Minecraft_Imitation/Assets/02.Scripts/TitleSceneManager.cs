using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
  

    public void GoToPlayScene()
    {
        if(GameManager.instance != null)
        {
            GameManager.instance.gameStart = true;
        }
        SceneManager.LoadScene("MergeTestScene");
    }

    public void EndGame()
    {
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

}
