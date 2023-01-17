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

    void Start() {
        /* if (connectedRoadNodes.Count == 0) {

            int mostLanes = Math.Max(laneNodesLeft.Count, laneNodesRight.Count);

            for (int i = 0; i < mostLanes; i++) {

                LaneNode rightLaneNodeToConnect;
                LaneNode leftLaneNodeToConnect;

                if (i < laneNodesLeft.Count) {
                    leftLaneNodeToConnect = laneNodesLeft[i];
                } else {
                    leftLaneNodeToConnect = laneNodesLeft.Last();
                }

                if (i < laneNodesRight.Count) {
                    rightLaneNodeToConnect = laneNodesRight[i];
                } else {
                    rightLaneNodeToConnect = laneNodesRight.Last();
                }

                rightLaneNodeToConnect.connectedLaneNodes.Add(leftLaneNodeToConnect);
                rightLaneNodeToConnect.laneTypes.Add("Curved");
            }
        } */
    }

    void Update()
    {
        
    }
}
