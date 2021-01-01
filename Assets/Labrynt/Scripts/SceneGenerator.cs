using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneGenerator : MonoBehaviour
{
    public Corridor start;

    public Corridor deadEndPrefab;
    public Corridor tripleCorridorPrefab;
    public Corridor fallingSandTrapPrefab;

    private Corridor lastPlaced;

    // Start is called before the first frame update
    void Start()
    {
        lastPlaced = start;

        PlaceCorridor(fallingSandTrapPrefab);
        PlaceCorridor(tripleCorridorPrefab);
        PlaceCorridor(deadEndPrefab);
    }

    private void PlaceCorridor(Corridor corridorPrefab)
    {
        lastPlaced = Instantiate(corridorPrefab,
            new Vector3(lastPlaced.transform.position.x + lastPlaced.joint1OffsetX, lastPlaced.transform.position.y, lastPlaced.transform.position.z + lastPlaced.joint1OffsetZ),
            Quaternion.identity);
    }
}
