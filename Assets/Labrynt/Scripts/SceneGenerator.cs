using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneGenerator : MonoBehaviour
{
    public Corridor start;

    public Corridor deadEndPrefab;
    public Corridor tripleCorridorPrefab;
    public Corridor finishEndPrefab;
    public Corridor corridor4x4Prefab;
    public Corridor turnCorridorPrefab;
    public Corridor3Ways corridor3WaysPrefab;

    public Trap fallingSandTrapPrefab;
    public Trap bladesTrapPrefab;
    public Trap floorSpikesTrapPrefab;
    public Trap horizontalBladesTrapPrefab;
    public Trap tunnelBladesTrapPrefab;
    public Trap wallSpikesTrapPrefab;
    public Trap fallingTrapPrefab;
    public Trap doubleFallingTrapPrefab;

    private Corridor lastPlaced;
    private List<Trap> traps;
    private Corridor lastPlacedTemp;

    private static System.Random random = new System.Random();

    //--------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        InitializeTrapsList();

        lastPlaced = start;

        lastPlaced = PlaceCorridorFrontally(corridor3WaysPrefab, lastPlaced);
        lastPlacedTemp = lastPlaced;
        lastPlaced = PlaceCorridorLeft(corridor3WaysPrefab, lastPlaced);
        lastPlacedTemp = PlaceCorridorRight(corridor3WaysPrefab, lastPlacedTemp);
        PlaceCorridorFrontally(corridor3WaysPrefab, lastPlacedTemp);
        lastPlaced = PlaceCorridorFrontally(corridor3WaysPrefab, lastPlaced);
        PlaceCorridorBackwards(finishEndPrefab, lastPlaced);
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
            doubleFallingTrapPrefab
        };
    }

    //--------------------------------------------------------------------------------------

    private void PlaceRandomTrapFrontally()
    {
        int randNum = random.Next(traps.Count);
        PlaceCorridorFrontally(traps[randNum], lastPlaced);
    }

    //--------------------------------------------------------------------------------------

    private void PlaceObstacleCourseFrontally(int trapsNum, bool separated)
    {
        for (int i = 0; i < trapsNum; i++)
        {
            PlaceRandomTrapFrontally();
            if(separated) PlaceCorridorFrontally(corridor4x4Prefab, lastPlaced);
        }
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceCorridor(Corridor corridorPrefab, Quaternion rotation, int offsetX, int offsetZ, Corridor addToCorridor = null)
    {
        if (addToCorridor == null) addToCorridor = lastPlaced;

        return Instantiate(corridorPrefab,
            new Vector3(addToCorridor.transform.position.x + offsetX, addToCorridor.transform.position.y, addToCorridor.transform.position.z + offsetZ),
            rotation);
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

    private Corridor PlaceCorridorLeft(Corridor corridorPrefab, Corridor addToCorridor)
    {
        int offsetX = 0;
        int offsetZ = 0;
        if (addToCorridor is Corridor3Ways)
        {
            Corridor3Ways lastPlaced3Ways = addToCorridor.GetComponent<Corridor3Ways>();
            offsetX = lastPlaced3Ways.jointLeftOffsetX;
            offsetZ = lastPlaced3Ways.jointLeftOffsetZ;

            Corridor placedCorridor = PlaceCorridor(corridorPrefab, Quaternion.Euler(0f, -90f, 0f), offsetX, offsetZ, addToCorridor);

            placedCorridor.joint1OffsetX = -lastPlaced3Ways.jointRightOffsetZ;
            placedCorridor.joint1OffsetZ = lastPlaced3Ways.jointRightOffsetX;

            return placedCorridor;
        }
        else
        {
            throw new System.Exception("Placing corridor left failed");
        }
    }

    //--------------------------------------------------------------------------------------

    private Corridor PlaceCorridorRight(Corridor corridorPrefab, Corridor addToCorridor)
    {
        int offsetX = 0;
        int offsetZ = 0;
        if (addToCorridor is Corridor3Ways)
        {
            Corridor3Ways lastPlaced3Ways = addToCorridor.GetComponent<Corridor3Ways>();
            offsetX = lastPlaced3Ways.jointRightOffsetX;
            offsetZ = lastPlaced3Ways.jointRightOffsetZ;

            Corridor placedCorridor = PlaceCorridor(corridorPrefab, Quaternion.Euler(0f, 90f, 0f), offsetX, offsetZ, addToCorridor);

            placedCorridor.joint1OffsetX = lastPlaced3Ways.jointLeftOffsetZ;
            placedCorridor.joint1OffsetZ = -lastPlaced3Ways.jointLeftOffsetX;

            return placedCorridor;
        }
        else
        {
            throw new System.Exception("Placing corridor right failed");
        }
    }
}
