using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NextScene : MonoBehaviour
{
    public bool PlayWithAI;

    public int mode;

    public void Next()
    {
        SceneManager.LoadScene("Slider/Scenes/Demo");
    }

    public void PlayGameScene()
    {
        SceneManager.LoadScene("CheckersScene");
        DontDestroyOnLoad(gameObject);
        //PlayWithAI = false;
        mode = 1;
    }

    public void PrevMenu()
    {
        SceneManager.LoadScene("Menu");

        Server s = FindObjectOfType<Server>();
        if(s != null)
        {
            Destroy(s.gameObject);
        }

        Client c = FindObjectOfType<Client>();
        if(c != null)
        {
            Destroy(c.gameObject);
        }
        
        GameManager gm = FindObjectOfType<GameManager>();
        if(gm != null)
        {
            Destroy(gm.gameObject);
        }

        NextScene nextScene = FindObjectOfType<NextScene>();
        if(nextScene != null)
        {
            Destroy(nextScene.gameObject);
        }

    }

    public void NextNewMenu()
    {
        SceneManager.LoadScene("NewMenu");
        mode = 2;
    }

    public void SinglePlay()
    {   
        SceneManager.LoadScene("CheckersScene");
        DontDestroyOnLoad(gameObject);
        //PlayWithAI = true;
        mode = 3;
    }

}
