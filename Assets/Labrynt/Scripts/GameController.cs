using System;
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
    private int points = 0; // game score
    private bool finished = false;
    private bool removePowerup = true;
    private bool powerupActive = false;
    private bool wannaLeave = false;

    private static int maxFps = 60;
    private GameObject player;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonController;
    private SceneGenerator sceneGenerator;

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
        if (firstPersonController == null) throw new System.Exception("FirstPersonController not found!");
        sceneGenerator = FindObjectOfType<SceneGenerator>();
        if (sceneGenerator == null) throw new System.Exception("SceneGenerator not found!");

        powerupText = GameObject.Find("PowerupText").GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (wannaLeave)
            {
                ExitToMainMenu();
            }
            else
            {
                wannaLeave = true;
                StartCoroutine(ShowConfirmText(3));
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) && Finished)
        {
            RestartScene();
        }
    }

    private IEnumerator ShowConfirmText(float duration)
    {
        TMP_Text text = GameObject.Find("ConfirmText").GetComponent<TMP_Text>();
        text.text = "Press Esc again to leave";
        yield return new WaitForSeconds(duration);
        text.text = "";
        wannaLeave = false;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
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

        int randNum = random.Next(1, 6);

        if (randNum == 1)
        {
            firstPersonController.MultiplyJumpSpeed(1.4f);
            powerupText.text = "Super Jump";
        }
        if (randNum == 2)
        {
            firstPersonController.MultiplyWalkRunSpeeds(1.5f);
            powerupText.text = "Super Speed";
        }
        if (randNum == 3)
        {
            firstPersonController.ChangeGravity(1f);
            powerupText.text = "Low Gravity";
        }
        if (randNum == 4)
        {
            firstPersonController.MakeImmortal();
            powerupText.text = "Immortality";
        }
        if (randNum == 5)
        {
            firstPersonController.MakeImmortal();
            firstPersonController.MultiplyWalkRunSpeeds(1.5f);
            powerupText.text = "Immortality + SS";
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

    public void FinishGame()
    {
        TMP_Text text = GameObject.Find("FinishText").GetComponent<TMP_Text>();
        Finished = true;
        string feedback = "";

        int pointsGenerated = sceneGenerator.GetPointsGenerated();

        if (GetPoints() >= pointsGenerated)
        {
            feedback = "You have trully mastered this game!";
        }
        else if(GetPoints() >= pointsGenerated / 2)
        {
            feedback = "Impressive victory";
        }
        else if(GetPoints() > 0)
        {
            feedback = "Well played";
        }
        else if(GetPoints() == 0)
        {
            feedback = "At least it's not negative";
        }
        else if(GetPoints() < 0 && GetPoints() > -pointsGenerated / 2)
        {
            feedback = "Don't die to improve your score";
        }
        else if(GetPoints() <= -pointsGenerated / 2 && GetPoints() > -pointsGenerated)
        {
            feedback = "Too hard?";
        }
        else if (GetPoints() <= -pointsGenerated && GetPoints() > -pointsGenerated * 2)
        {
            feedback = "Git gud";
        }
        else if (GetPoints() <= -pointsGenerated*2)
        {
            feedback = "How did you even die so many times?";
        }

        text.text = feedback + Environment.NewLine + "Score: " + GetPoints() + Environment.NewLine + "Enter - try again" + Environment.NewLine + "Esc - leave";
    }
}
