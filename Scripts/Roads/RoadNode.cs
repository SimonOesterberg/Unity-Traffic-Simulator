using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadNode : MonoBehaviour
{

    public List<LaneNode> laneNodesRight = new List<LaneNode>();
    public List<LaneNode> laneNodesLeft = new List<LaneNode>();

    public List<RoadNode> connectedRoadNodes = new List<RoadNode>();
    public List<GameObject> roadHandles = new List<GameObject>();

    void Start() {
    }

    void Update()
    {
        
    }
}
