using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LaneNode : MonoBehaviour {

    // Sets a maximum speed that is allowed on the lanes from this lane
    public float speedLimit = 100;

    // Gives the lane node a priority so that correct merging can occur
    public int priority = 0;

    // Lists of all lanes that this lane node leads to and what type of road should be inbetween
    public List<LaneNode> connectedLaneNodes = new List<LaneNode>();
    public List<string> laneTypes = new List<string>();

    public List<LineRenderer> laneLines = new List<LineRenderer>();

    public List<GameObject> laneHandles = new List<GameObject>();

    public List<Vector3[]> lanePaths = new List<Vector3[]>();



    // List of all vehicles currently travelling to and from this lane node
    [System.NonSerialized] public List<VehicleController> vehiclesOnTheirWay = new List<VehicleController>();
    [System.NonSerialized] public List<VehicleController> vehiclesOn = new List<VehicleController>();
}
