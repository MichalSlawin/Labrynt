using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SceneGenerator : MonoBehaviour
{
    public Corridor start;

    public Corridor deadEndPrefab;
    public Corridor deadEndShortPrefab;
    public Corridor tripleCorridorPrefab;
    public Corridor finishEndPrefab;
    public Corridor finishEndShortPrefab;
    public Corridor corridor4x4Prefab;
    public Corridor turnCorridorPrefab;
    public Corridor pointEndShortPrefab;
    public Corridor powerupEndShortPrefab;
    public Corridor secretEndShortPrefab;
    public Corridor secret2EndShortPrefab;
    public Corridor portalEndShortPrefab;
    public Corridor3Ways corridor3WaysPrefab;
    public Corridor3Ways corridor3WaysShortPrefab;

    public Trap fallingSandTrapPrefab;
    public Trap bladesTrapPrefab;
    public Trap floorSpikesTrapPrefab;
    public Trap horizontalBladesTrapPrefab;
    public Trap tunnelBladesTrapPrefab;
    public Trap wallSpikesTrapPrefab;
    public Trap fallingTrapPrefab;
    public Trap doubleFallingTrapPrefab;
    public Trap multipleFallingSandsTrapPrefab;
    public Trap coveredFallingSandsTrapPrefab;
    public Trap horizontalRotatedBladesTrapPrefab;
    public Trap tunnelBladesTrapHardPrefab;
    public Trap bladesTrapHardPrefab;
    public Trap hiddenFloorSpikesTrapPrefab;

    private Corridor lastPlaced;
    private List<Trap> traps;
    private bool finishPlaced = false;
    private bool secretPlaced = false;
    private int placedCorridorsCount = 0;
    private GameController gameController;
    private bool canBuildLeft = true;
    private bool canBuildRight = true;
    private List<Vector3> occupiedPositions;
    private bool firstPlacedDown = false;
    private int placeFinishAfter = 0;

    private const int MIN_CORRIDORS_IN_LINE_NUM = 3;
    private const int MAX_CORRIDORS_IN_LINE_NUM = 5;
    private const int MIN_PLACED_CORRIDORS = 30;
    private const int MAX_PLACED_CORRIDORS = 100;
    private const int RAND_CORRIDOR_MIN_NUM = 1;
    private const int RAND_CORRIDOR_MAX_NUM = 5;
    private const int SECRET_CHANCE = 30;

    private static System.Random random = new System.Random();

    private int pointsCollected = 0;
    private int pointsGenerated = 0;

    private TMP_Text pointsText;

    //--------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        placeFinishAfter = random.Next(MIN_PLACED_CORRIDORS/2, MAX_PLACED_CORRIDORS/2);

        gameController = FindObjectOfType<GameController>();
        if (gameController == null) throw new System.Exception("Game controller not found!");

        InitializeTrapsList();
        InitializeOccupiedPositionsList();

        lastPlaced = start;

        int randNum = random.Next(MIN_CORRIDORS_IN_LINE_NUM-1, MAX_CORRIDORS_IN_LINE_NUM-1);
        PlaceObstacleCourseFrontally(randNum);

        lastPlaced = PlaceCorridorFrontally(corridor3WaysShortPrefab, lastPlaced);
        Corridor lastPlacedTemp = lastPlaced;
        GenerateMazeLeft(lastPlacedTemp, 1);
        GenerateMazeRight(lastPlacedTemp, 1);

        canBuildLeft = canBuildRight = false;
        GenerateMazeUp(lastPlacedTemp, 1);

        if(!finishPlaced)
        {
            Debug.Log("No finish!");
            gameController.RestartScene();
        }

        if(placedCorridorsCount < MIN_PLACED_CORRIDORS)
        {
            Debug.Log("Too few corridors!");
            gameController.RestartScene();
        }

        InitializePointsCounter();

        int respawnCount = GameObject.FindGameObjectsWithTag("Respawn").Length;
        int pointCount = GameObject.FindGameObjectsWithTag("Point").Length;
        int powerupCount = GameObject.FindGameObjectsWithTag("Powerup").Length;
        
        if(respawnCount < 3)
        {
            Debug.Log("Too few respawns!");
            gameController.RestartScene();
        }

        if(pointCount < 4)
        {
            Debug.Log("Too few points!");
            gameController.RestartScene();
        }

        if(powerupCount < 3)
        {
            Debug.Log("Too few powerups!");
            gameController.RestartScene();
        }

        if (secretPlaced) Debug.Log("it's a secret!");
    }

    public int GetPointsCollected()
    {
        return pointsCollected;
    }

    public int GetPointsGenerated()
    {
        return pointsGenerated;
    }

    public void IncreasePointsCollected()
    {
        pointsCollected++;

        if(pointsCollected == pointsGenerated)
        {
            GameObject blockade = GameObject.FindGameObjectWithTag("FinishBlockade");
            if (blockade == null) throw new Exception("FinishBlockade not found");
            Destroy(blockade);
        }
    }

    //--------------------------------------------------------------------------------------

    private void InitializePointsCounter()
    {
        pointsGenerated = GameObject.FindGameObjectsWithTag("Point").Length;
        pointsText = GameObject.Find("PointsText").GetComponent<TMP_Text>();
        UpdatePointsCounter();
    }

    public void UpdatePointsCounter()
    {
        pointsText.text = pointsCollected + " / " + pointsGenerated;
    }

    //--------------------------------------------------------------------------------------

    private void InitializeOccupiedPositionsList()
    {
        occupiedPositions = new List<Vector3>
        {
            start.transform.position
        };
    }

    //--------------------------------------------------------------------------------------

    private void GenerateMazeUp(Corridor addToCorridor, int placedInRow)
    {
        int randNum = random.Next(RAND_CORRIDOR_MIN_NUM, RAND_CORRIDOR_MAX_NUM);
        
        if(randNum == 1)
        {
            lastPlaced = PlaceCorridorFrontally(corridor3WaysShortPrefab, addToCorridor);
            Corridor lastPlacedTemp = lastPlaced;

            if (canBuildLeft) GenerateMazeLeft(lastPlacedTemp, 1);
            else PlaceClosedCorridorLeft();

            if (canBuildRight) GenerateMazeRight(lastPlacedTemp, 1);
            else PlaceClosedCorridorRight();

            lastPlaced = lastPlacedTemp;
        }
        else if (randNum >= 2 && randNum <= 3)
        {
            lastPlaced = PlaceCorridorFrontally(corridor4x4Prefab, addToCorridor);
        }
        else if (randNum >= 4)
        {
            lastPlaced = PlaceCorridorFrontally(GetRandomTrap(), addToCorridor);
        }

        randNum = random.Next(1, 3);

        if(placedInRow >= MAX_CORRIDORS_IN_LINE_NUM || (randNum == 2 && placedInRow >= MIN_CORRIDORS_IN_LINE_NUM))
        {
            PlaceClosedCorridorUp();
        }
        else
        {
            GenerateMazeUp(lastPlaced, ++placedInRow);
        }
    }

    //--------------------------------------------------------------------------------------

    private void PlaceClosedCorridorUp()
    {
        int randNum = random.Next(1, 4);

        Corridor lastPlacedTemp = PlaceCorridorFrontally(GetRandomTrap(), lastPlaced);

        if (placedCorridorsCount > placeFinishAfter && !finishPlaced && randNum == 2)
        {
            PlaceCorridorBackwards(finishEndShortPrefab, lastPlacedTemp);
            finishPlaced = true;
        }
        else
        {
            randNum = random.Next(1, 7);
            int randNumSecret = random.Next(1, SECRET_CHANCE);
            if(randNumSecret == 15 && !secretPlaced)
            {
                secretPlaced = true;
                randNumSecret = random.Next(1, 3);
                if(randNumSecret == 1)
                    PlaceCorridorBackwards(secretEndShortPrefab, lastPlacedTemp);
                else
                    PlaceCorridorBackwards(secret2EndShortPrefab, lastPlacedTemp);
            }
            else if(randNum == 1)
            {
                PlaceCorridorBackwards(deadEndShortPrefab, lastPlacedTemp);
            }
            else if(randNum == 2 || randNum == 3)
            {
                PlaceCorridorBackwards(powerupEndShortPrefab, lastPlacedTemp);
            }
            else
            {
                PlaceCorridorBackwards(pointEndShortPrefab, lastPlacedTemp);
            }
        }
    }

    //--------------------------------------------------------------------------------------

    private void GenerateMazeDown(Corridor addToCorridor, int placedInRow, bool fromRight = false)
    {
        int randNum = random.Next(RAND_CORRIDOR_MIN_NUM, RAND_CORRIDOR_MAX_NUM);

        if (randNum == 1)
        {
            lastPlaced = PlaceCorridorDown(corridor3WaysShortPrefab, addToCorridor, fromRight);
            Corridor lastPlacedTemp = lastPlaced;

            if (canBuildLeft) GenerateMazeLeft(lastPlacedTemp, 1);
            else PlaceClosedCorridorLeft();

            if (canBuildRight) GenerateMazeRight(lastPlacedTemp, 1);
            else PlaceClosedCorridorRight();

            lastPlaced = lastPlacedTemp;
        }
        else if (randNum >= 2 && randNum <= 3)
        {
            lastPlaced = PlaceCorridorDown(corridor4x4Prefab, addToCorridor, fromRight);
        }
        else if (randNum >= 4)
        {
            lastPlaced = PlaceCorridorDown(GetRandomTrap(), addToCorridor, fromRight);
        }

        randNum = random.Next(1, 3);

        if (placedInRow >= MAX_CORRIDORS_IN_LINE_NUM || (randNum == 2 && placedInRow >= MIN_CORRIDORS_IN_LINE_NUM))
        {
            PlaceClosedCorridorDown();
        }
        else
        {
            GenerateMazeDown(lastPlaced, ++placedInRow);
        }
    }

    //--------------------------------------------------------------------------------------

    private void PlaceClosedCorridorDown()
    {
        Corridor lastPlacedTemp = PlaceCorridorDown(GetRandomTrap(), lastPlaced);

        int randNum = random.Next(1, 4);
        if (placedCorridorsCount > placeFinishAfter && !finishPlaced && randNum == 2)
        {
            PlaceCorridorDown(finishEndShortPrefab, lastPlacedTemp);
            finishPlaced = true;
        }
        else
        {
            randNum = random.Next(1, 7);
            int randNumSecret = random.Next(1, SECRET_CHANCE);
            if (randNumSecret == 15 && !secretPlaced)
            {
                secretPlaced = true;
                randNumSecret = random.Next(1, 3);
                if (randNumSecret == 1)
                    PlaceCorridorDown(secretEndShortPrefab, lastPlacedTemp);
                else
                    PlaceCorridorDown(secret2EndShortPrefab, lastPlacedTemp);
            }
            else if (randNum == 1)
            {
                PlaceCorridorDown(deadEndShortPrefab, lastPlacedTemp);
            }
            else if (randNum == 2 || randNum == 3)
            {
                PlaceCorridorDown(powerupEndShortPrefab, lastPlacedTemp);
            }
            else
            {
                PlaceCorridorDown(pointEndShortPrefab, lastPlacedTemp);
            }
        }
    }

    //--------------------------------------------------------------------------------------

    private void GenerateMazeLeft(Corridor addToCorridor, int placedInRow)
    {
        int randNum = random.Next(RAND_CORRIDOR_MIN_NUM, RAND_CORRIDOR_MAX_NUM);

        if (addToCorridor is Corridor3Ways)
        {
            lastPlaced = PlaceCorridorLeft(GetRandomTrap(), addToCorridor);
            lastPlaced = PlaceCorridorLeft(GetRandomTrap(), lastPlaced);
        }
        else if (randNum == 1)
        {
            lastPlaced = PlaceCorridorLeft(corridor3WaysShortPrefab, addToCorridor);
            Corridor lastPlacedTemp = lastPlaced;

            canBuildLeft = canBuildRight = false;
            GenerateMazeUp(lastPlacedTemp, 1);
            firstPlacedDown = true;
            GenerateMazeDown(lastPlacedTemp, 1);
            canBuildLeft = canBuildRight = true;

            lastPlaced = lastPlacedTemp;
        }
        else if (randNum >= 2 && randNum <= 3)
        {
            lastPlaced = PlaceCorridorLeft(corridor4x4Prefab, addToCorridor);
        }
        else if (randNum >= 4)
        {
            lastPlaced = PlaceCorridorLeft(GetRandomTrap(), addToCorridor);
        }

        randNum = random.Next(1, 3);

        if (placedInRow >= MAX_CORRIDORS_IN_LINE_NUM || (randNum == 2 && placedInRow >= MIN_CORRIDORS_IN_LINE_NUM))
        {
            PlaceCorridorLeft(portalEndShortPrefab, lastPlaced, true);
        }
        else
        {
            GenerateMazeLeft(lastPlaced, ++placedInRow);
        }
    }

    //--------------------------------------------------------------------------------------

    private void PlaceClosedCorridorLeft()
    {
        int randNum = random.Next(1, 4);
        if (placedCorridorsCount > placeFinishAfter && !finishPlaced && randNum == 2)
        {
            PlaceCorridorLeft(finishEndShortPrefab, lastPlaced, true);
            finishPlaced = true;
        }
        else
        {
            randNum = random.Next(1, 7);
            int randNumSecret = random.Next(1, SECRET_CHANCE);
            if (randNumSecret == 15 && !secretPlaced)
            {
                secretPlaced = true;
                randNumSecret = random.Next(1, 3);
                if (randNumSecret == 1)
                    PlaceCorridorLeft(secretEndShortPrefab, lastPlaced, true);
                else
                    PlaceCorridorLeft(secret2EndShortPrefab, lastPlaced, true);

            }
            else if (randNum == 1)
            {
                PlaceCorridorLeft(deadEndShortPrefab, lastPlaced, true);
            }
            else if (randNum == 2 || randNum == 3)
            {
                PlaceCorridorLeft(powerupEndShortPrefab, lastPlaced, true);
            }
            else
            {
                PlaceCorridorLeft(pointEndShortPrefab, lastPlaced, true);
            }
        }
    }

    //--------------------------------------------------------------------------------------

    private void GenerateMazeRight(Corridor addToCorridor, int placedInRow)
    {
        int randNum = random.Next(RAND_CORRIDOR_MIN_NUM, RAND_CORRIDOR_MAX_NUM);

        if (addToCorridor is Corridor3Ways)
        {
            lastPlaced = PlaceCorridorRight(GetRandomTrap(), addToCorridor);
            lastPlaced = PlaceCorridorRight(GetRandomTrap(), lastPlaced);
        }
        else if (randNum == 1)
        {
            lastPlaced = PlaceCorridorRight(corridor3WaysShortPrefab, addToCorridor);
            Corridor lastPlacedTemp = lastPlaced;

            canBuildLeft = canBuildRight = false;
            GenerateMazeUp(lastPlacedTemp, 1);
            firstPlacedDown = true;
            GenerateMazeDown(lastPlacedTemp, 1, true);
            canBuildLeft = canBuildRight = true;

            lastPlaced = lastPlacedTemp;
        }
        else if (randNum >= 2 && randNum <= 3)
        {
            lastPlaced = PlaceCorridorRight(corridor4x4Prefab, addToCorridor);
        }
        else if (randNum >= 4)
        {
            lastPlaced = PlaceCorridorRight(GetRandomTrap(), addToCorridor);
        }

        randNum = random.Next(1, 3);

        if (placedInRow >= MAX_CORRIDORS_IN_LINE_NUM || (randNum == 2 && placedInRow >= MIN_CORRIDORS_IN_LINE_NUM))
        {
            //PlaceClosedCorridorRight();
            PlaceCorridorRight(portalEndShortPrefab, lastPlaced, true);
        }
        else
        {
            GenerateMazeRight(lastPlaced, ++placedInRow);
        }
    }

    //--------------------------------------------------------------------------------------

    private void PlaceClosedCorridorRight()
    {
        int randNum = random.Next(1, 4);
        if (placedCorridorsCount > placeFinishAfter && !finishPlaced && randNum == 2)
        {
            PlaceCorridorRight(finishEndShortPrefab, lastPlaced, true);
            finishPlaced = true;
        }
        else
        {
            randNum = random.Next(1, 7);
            int randNumSecret = random.Next(1, SECRET_CHANCE);
            if (randNumSecret == 15 && !secretPlaced)
            {
                secretPlaced = true;
                randNumSecret = random.Next(1, 3);
                if (randNumSecret == 1)
                    PlaceCorridorRight(secretEndShortPrefab, lastPlaced, true);
                else
                    PlaceCorridorRight(secret2EndShortPrefab, lastPlaced, true);

            }
            else if (randNum == 1)
            {
                PlaceCorridorRight(deadEndShortPrefab, lastPlaced, true);
            }
            else if (randNum == 2 || randNum == 3)
            {
                PlaceCorridorRight(powerupEndShortPrefab, lastPlaced, true);
            }
            else
            {
                PlaceCorridorRight(pointEndShortPrefab, lastPlaced, true);
            }
        }
    }

    //--------------------------------------------------------------------------------------

    private void InitializeTrapsList()
    {
        traps = new List<Trap>
        {
            fallingSandTrapPrefab,
            bladesTrapPrefab,
            floorSpikesTrapPrefab,
            horizontalBladesTrapPrefab,
            tunnelBladesTrapPrefab,
            wallSpikesTrapPrefab,
            fallingTrapPrefab,
            doubleFallingTrapPrefab,
            multipleFallingSandsTrapPrefab,
            coveredFallingSandsTrapPrefab,
            horizontalRotatedBladesTrapPrefab,
            tunnelBladesTrapHardPrefab,
            bladesTrapHardPrefab,
            hiddenFloorSpikesTrapPrefab
        };
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceRandomTrapFrontally()
    {
        return PlaceCorridorFrontally(GetRandomTrap(), lastPlaced);
    }

    //--------------------------------------------------------------------------------------

    private Trap GetRandomTrap()
    {
        int randNum = random.Next(traps.Count);
        return traps[randNum];
    }

    //--------------------------------------------------------------------------------------

    private void PlaceObstacleCourseFrontally(int trapsNum)
    {
        for (int i = 0; i < trapsNum; i++)
        {
            lastPlaced = PlaceRandomTrapFrontally();
        }
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceCorridor(Corridor corridorPrefab, Quaternion rotation, int offsetX, int offsetZ, Corridor addToCorridor = null)
    {
        if (addToCorridor == null) addToCorridor = lastPlaced;

        placedCorridorsCount++;
        if(placedCorridorsCount > MAX_PLACED_CORRIDORS)
        {
            Debug.Log("Too many corridors");
            gameController.RestartScene();
        }
        
        Vector3 newPosition = new Vector3(addToCorridor.transform.position.x + offsetX, addToCorridor.transform.position.y, addToCorridor.transform.position.z + offsetZ);
        if(occupiedPositions.Contains(newPosition))
        {
            return lastPlaced;
        }
        else
        {
            occupiedPositions.Add(newPosition);
            return Instantiate(corridorPrefab, newPosition, rotation);
        }
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceCorridorFrontally(Corridor corridorPrefab, Corridor addToCorridor)
    {
        return PlaceCorridor(corridorPrefab, Quaternion.identity, addToCorridor.joint1OffsetX, addToCorridor.joint1OffsetZ, addToCorridor);
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceCorridorBackwards(Corridor corridorPrefab, Corridor addToCorridor)
    {
        addToCorridor.joint1OffsetX += corridorPrefab.joint1OffsetX + 2;
        addToCorridor.joint1OffsetZ -= 2;
        return PlaceCorridor(corridorPrefab, Quaternion.Euler(0f, 180f, 0f), addToCorridor.joint1OffsetX, addToCorridor.joint1OffsetZ, addToCorridor);
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceCorridorLeft(Corridor corridorPrefab, Corridor addToCorridor, bool backwards = false)
    {
        int offsetX = addToCorridor.joint1OffsetZ;
        int offsetZ = addToCorridor.joint1OffsetX;
        if (addToCorridor is Corridor3Ways)
        {
            Corridor3Ways lastPlaced3Ways = addToCorridor.GetComponent<Corridor3Ways>();
            offsetX = lastPlaced3Ways.jointLeftOffsetX;
            offsetZ = lastPlaced3Ways.jointLeftOffsetZ;
        }

        Quaternion rotation = Quaternion.Euler(0f, -90f, 0f);
        if(backwards)
        {
            offsetX += 2;
            offsetZ += corridorPrefab.joint1OffsetX + 2;
            rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        Corridor placedCorridor = PlaceCorridor(corridorPrefab, rotation, offsetX, offsetZ, addToCorridor);

        int offsetXTemp = placedCorridor.joint1OffsetX;
        int offsetZTemp = placedCorridor.joint1OffsetZ;
        
        if(placedCorridor is Corridor3Ways)
        {
            Corridor3Ways placedCorridor3Ways = placedCorridor.GetComponent<Corridor3Ways>();

            placedCorridor.joint1OffsetX = -placedCorridor3Ways.jointRightOffsetZ;
            placedCorridor.joint1OffsetZ = placedCorridor3Ways.jointRightOffsetX;

            placedCorridor3Ways.jointLeftOffsetX = offsetZTemp;
            placedCorridor3Ways.jointLeftOffsetZ = offsetXTemp;
        }

        return placedCorridor;
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceCorridorRight(Corridor corridorPrefab, Corridor addToCorridor, bool backwards = false)
    {
        int offsetX = addToCorridor.joint1OffsetZ;
        int offsetZ = -addToCorridor.joint1OffsetX;
        if (addToCorridor is Corridor3Ways)
        {
            Corridor3Ways lastPlaced3Ways = addToCorridor.GetComponent<Corridor3Ways>();
            offsetX = lastPlaced3Ways.jointRightOffsetX;
            offsetZ = lastPlaced3Ways.jointRightOffsetZ;
        }
        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);
        if (backwards)
        {
            offsetX -= 2;
            offsetZ -= corridorPrefab.joint1OffsetX + 2;
            rotation = Quaternion.Euler(0f, -90f, 0f);
        }

        Corridor placedCorridor = PlaceCorridor(corridorPrefab, rotation, offsetX, offsetZ, addToCorridor);

        int offsetXTemp = placedCorridor.joint1OffsetX;
        int offsetZTemp = placedCorridor.joint1OffsetZ;

        if (placedCorridor is Corridor3Ways)
        {
            Corridor3Ways placedCorridor3Ways = placedCorridor.GetComponent<Corridor3Ways>();

            placedCorridor.joint1OffsetX = placedCorridor3Ways.jointLeftOffsetZ;
            placedCorridor.joint1OffsetZ = -placedCorridor3Ways.jointLeftOffsetX;

            placedCorridor3Ways.jointRightOffsetX = offsetZTemp;
            placedCorridor3Ways.jointRightOffsetZ = -offsetXTemp;
        }

        return placedCorridor;
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceCorridorDown(Corridor corridorPrefab, Corridor addToCorridor, bool fromRight = false)
    {
        int offsetX = -corridorPrefab.joint1OffsetX;
        int offsetZ = addToCorridor.joint1OffsetZ;

        if (addToCorridor is Corridor3Ways && firstPlacedDown)
        {
            Corridor3Ways lastPlaced3Ways = addToCorridor.GetComponent<Corridor3Ways>();
            offsetX = -lastPlaced3Ways.joint1OffsetX - corridorPrefab.joint1OffsetX;
            if (fromRight) offsetX -= 4;
            offsetZ = lastPlaced3Ways.joint1OffsetZ;

            firstPlacedDown = false;
        }

        Quaternion rotation = Quaternion.identity;

        Corridor placedCorridor = PlaceCorridor(corridorPrefab, rotation, offsetX, offsetZ, addToCorridor);

        return placedCorridor;
    }
    
}
