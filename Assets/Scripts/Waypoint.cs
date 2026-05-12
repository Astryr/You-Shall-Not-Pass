using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    // we need this script to to detect waypoints
    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
