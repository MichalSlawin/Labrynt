using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using TMPro;

public class GameController : MonoBehaviour
{
    private static float powerupTime = 15.0f;
    private static float floorDissapearTime = 0.1f;
    private static float floorAppearTime = 1f;
    private int points = 0;
    private bool finished = false;
    private bool removePowerup = true;
    private bool powerupActive = false;

    private static int maxFps = 60;
    private GameObject player;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonController;

    public static int MaxFps { get => maxFps; set => maxFps = value; }
    public static float FloorDissapearTime { get => floorDissapearTime; set => floorDissapearTime = value; }
    public static float FloorAppearTime { get => floorAppearTime; set => floorAppearTime = value; }
    public bool Finished { get => finished; set => finished = value; }
    public static float PowerupTime { get => powerupTime; set => powerupTime = value; }
    public bool RemovePowerup { get => removePowerup; set => removePowerup = value; }
    public bool PowerupActive { get => powerupActive; set => powerupActive = value; }

    private System.Random random = new System.Random();
    private TMP_Text powerupText;

    public int GetPoints()
    {
        return points;
    }

    public void ChangePoints(int change)
    {
        points += change;
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = MaxFps;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) throw new System.Exception("Player not found!");
        firstPersonController = player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();

        powerupText = GameObject.Find("PowerupText").GetComponent<TMP_Text>();
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

        if (Input.GetKeyDown(KeyCode.Return) && Finished)
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

    public IEnumerator DisableObjectTemporarily(GameObject gameObject, float disableAfterTime, float enableAfterTime)
    {
        yield return new WaitForSeconds(disableAfterTime);
        gameObject.SetActive(false);
        StartCoroutine(EnableObject(gameObject, enableAfterTime));
    }

    private IEnumerator EnableObject(GameObject gameObject, float enableAfterTime)
    {
        yield return new WaitForSeconds(enableAfterTime);
        gameObject.SetActive(true);
    }

    public IEnumerator UseRandomPowerup(float duration)
    {
        firstPersonController.RestoreOriginalValues();

        int randNum = random.Next(1, 5);

        if (randNum == 1)
        {
            firstPersonController.MultiplyJumpSpeed(1.4f, powerupTime);
            powerupText.text = "Super Jump";
        }
        if (randNum == 2)
        {
            firstPersonController.MultiplyWalkRunSpeeds(1.5f, powerupTime);
            powerupText.text = "Super Speed";
        }
        if (randNum == 3)
        {
            firstPersonController.ChangeGravity(1f, powerupTime);
            powerupText.text = "Low Gravity";
        }
        if (randNum == 4)
        {
            firstPersonController.MakeImmortal(powerupTime);
            powerupText.text = "Immortality";
        }

        PowerupActive = true;

        yield return new WaitForSeconds(duration);

        if(removePowerup)
        {
            firstPersonController.RestoreOriginalValues();
            powerupText.text = "";
            PowerupActive = false;
        }

        removePowerup = true;
    }
}
