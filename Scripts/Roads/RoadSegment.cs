using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadSegment : MonoBehaviour {
    
    public Handle roadHandle;
    
    private int laneNodesInCount;
    private int laneNodesOutCount;

    public List<RoadSegmentEnd> roadSegmentEnds = new List<RoadSegmentEnd>();

    void Start() {

        laneNodesInCount = roadSegmentEnds[0].laneNodesIn.Count;
        laneNodesOutCount = roadSegmentEnds[0].laneNodesOut.Count;

        List<LaneNode> laneNodesIn = roadSegmentEnds[0].laneNodesIn.Concat(roadSegmentEnds[1].laneNodesIn).ToList();
        List<LaneNode> laneNodesOut = roadSegmentEnds[0].laneNodesOut.Concat(roadSegmentEnds[1].laneNodesOut).ToList();

        int mostLanes = Math.Max(laneNodesInCount, laneNodesOutCount);

        for (int i = 0; i < mostLanes; i++) {

            if (i < laneNodesInCount) {
                roadSegmentEnds[0].laneNodesIn[i].addConnection(roadSegmentEnds[1].laneNodesOut[i]);
            }

            if (i < laneNodesOutCount) {
                roadSegmentEnds[1].laneNodesIn[i].addConnection(roadSegmentEnds[0].laneNodesOut[i]);
            }
        }
    }

    public void reDrawLaneLines() {
        List<LaneNode> allLaneNodes = roadSegmentEnds[0].laneNodesIn.Concat(roadSegmentEnds[0].laneNodesOut).Concat(roadSegmentEnds[1].laneNodesIn).Concat(roadSegmentEnds[1].laneNodesOut).ToList();

        foreach (LaneNode laneNode in allLaneNodes) {
            laneNode.resetHandlesHeight();
            laneNode.reDrawLaneLines();
        }
    }
}
