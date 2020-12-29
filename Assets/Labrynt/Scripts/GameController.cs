using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static float floorDissapearTime = 0.5f;

    private static int maxFps = 60;
    private GameObject player;

    public static int MaxFps { get => maxFps; set => maxFps = value; }
    public static float FloorDissapearTime { get => floorDissapearTime; set => floorDissapearTime = value; }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = MaxFps;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) throw new System.Exception("Player not found!");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            RestartScene();
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public IEnumerator DisableObjectTemporarily(GameObject gameObject, float afterTime)
    {
        yield return new WaitForSeconds(afterTime);
        gameObject.SetActive(false);
        StartCoroutine(EnableObject(gameObject, afterTime*2));
    }

    private IEnumerator EnableObject(GameObject gameObject, float afterTime)
    {
        yield return new WaitForSeconds(afterTime);
        gameObject.SetActive(true);
    }
}
