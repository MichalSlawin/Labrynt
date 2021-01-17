using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private int maxFps = 60;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = maxFps;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GeneratedScene");
    }
}
