using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NextScene : MonoBehaviour
{
    public bool PlayWithAI;

    public GameMode mode;

    public void Next()
    {
        SceneManager.LoadScene("Slider/Scenes/Demo");
    }

    public void OnHotSeat()
    {
        SceneManager.LoadScene("CheckersScene");
        DontDestroyOnLoad(gameObject);
        mode = GameMode.HotSeat;
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

    public void OnMultiPlayer()
    {
        SceneManager.LoadScene("NewMenu");
        mode = GameMode.MultiPlayer;
    }

    public void OnSinglePlay()
    {   
        SceneManager.LoadScene("CheckersScene");
        DontDestroyOnLoad(gameObject);
        mode = GameMode.Bot;
    }

}
