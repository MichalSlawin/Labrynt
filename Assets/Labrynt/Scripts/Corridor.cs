using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor : MonoBehaviour
{
    public int jointsNum = 1;
    public int joint1OffsetX = 0;
    public int joint1OffsetZ = 0;
    public int joint2OffsetX = 0;
    public int joint2OffsetZ = 0;
    public int joint3OffsetX = 0;
    public int joint3OffsetZ = 0;
    public bool isTrap = false;

    // Start is called before the first frame update
    void Start()
    {
        if (jointsNum > 3) throw new System.Exception("Corridor can have 3 joints maximum");
    }
}
