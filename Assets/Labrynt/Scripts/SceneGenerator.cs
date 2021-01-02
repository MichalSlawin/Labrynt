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

    private static System.Random random = new System.Random();

    //--------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        InitializeTrapsList();

        lastPlaced = start;

        PlaceObstacleCourse(10, false);

        PlaceCorridorBackwards(finishEndPrefab);
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

    private void PlaceRandomTrap()
    {
        int randNum = random.Next(traps.Count);
        PlaceCorridorFrontally(traps[randNum]);
    }

    //--------------------------------------------------------------------------------------

    private void PlaceObstacleCourse(int trapsNum, bool separated)
    {
        for (int i = 0; i < trapsNum; i++)
        {
            PlaceRandomTrap();
            if(separated) PlaceCorridorFrontally(corridor4x4Prefab);
        }
    }

    //--------------------------------------------------------------------------------------

    private void PlaceCorridor(Corridor corridorPrefab, Quaternion rotation)
    {
        lastPlaced = Instantiate(corridorPrefab,
            new Vector3(lastPlaced.transform.position.x + lastPlaced.joint1OffsetX, lastPlaced.transform.position.y, lastPlaced.transform.position.z + lastPlaced.joint1OffsetZ),
            rotation);
    }

    //--------------------------------------------------------------------------------------

    private void PlaceCorridorFrontally(Corridor corridorPrefab)
    {
        PlaceCorridor(corridorPrefab, Quaternion.identity);
    }

    //--------------------------------------------------------------------------------------

    private void PlaceCorridorBackwards(Corridor corridorPrefab)
    {
        lastPlaced.joint1OffsetX += corridorPrefab.joint1OffsetX + 2;
        lastPlaced.joint1OffsetZ -= 2;
        PlaceCorridor(corridorPrefab, Quaternion.Euler(0f, 180f, 0f));
    }
}
