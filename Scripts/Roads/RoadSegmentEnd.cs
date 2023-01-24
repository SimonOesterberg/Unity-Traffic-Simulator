using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegmentEnd : MonoBehaviour {
    public RoadNode roadNode = null;
    public List<LaneNode> laneNodesIn = new List<LaneNode>();
    public List<LaneNode> laneNodesOut = new List<LaneNode>();
}
