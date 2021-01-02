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

    private Corridor lastPlaced;

    // Start is called before the first frame update
    void Start()
    {
        lastPlaced = start;

        PlaceCorridorFrontally(bladesTrapPrefab);
        PlaceCorridorFrontally(corridor4x4Prefab);
        PlaceCorridorFrontally(floorSpikesTrapPrefab);
        PlaceCorridorBackwards(finishEndPrefab);
    }

    private void PlaceCorridor(Corridor corridorPrefab, Quaternion rotation)
    {
        lastPlaced = Instantiate(corridorPrefab,
            new Vector3(lastPlaced.transform.position.x + lastPlaced.joint1OffsetX, lastPlaced.transform.position.y, lastPlaced.transform.position.z + lastPlaced.joint1OffsetZ),
            rotation);
    }

    private void PlaceCorridorFrontally(Corridor corridorPrefab)
    {
        PlaceCorridor(corridorPrefab, Quaternion.identity);
    }

    private void PlaceCorridorBackwards(Corridor corridorPrefab)
    {
        lastPlaced.joint1OffsetX += corridorPrefab.joint1OffsetX + 2;
        lastPlaced.joint1OffsetZ -= 2;
        PlaceCorridor(corridorPrefab, Quaternion.Euler(0f, 180f, 0f));
    }
}
