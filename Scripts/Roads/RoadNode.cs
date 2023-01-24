using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadNode : MonoBehaviour
{
    public List<RoadSegmentEnd> roadSegmentEnds = new List<RoadSegmentEnd>();

    private float size = 1;

    public void addRoadSegmentEnd(RoadSegmentEnd segmentEndToAdd) {
        segmentEndToAdd.roadNode = this;
        float roadSegmentSize = 0;

        if (segmentEndToAdd.laneNodesIn.Count() != 0 && segmentEndToAdd.laneNodesOut.Count() != 0) {
            roadSegmentSize = Vector3.Distance(segmentEndToAdd.laneNodesIn.Last().transform.position, segmentEndToAdd.laneNodesOut.Last().transform.position);
        } else if (segmentEndToAdd.laneNodesIn.Count() != 0) {
            roadSegmentSize = Vector3.Distance(segmentEndToAdd.laneNodesIn.First().transform.position, segmentEndToAdd.laneNodesIn.Last().transform.position);
        } else if (segmentEndToAdd.laneNodesOut.Count() != 0) {
            roadSegmentSize = Vector3.Distance(segmentEndToAdd.laneNodesOut.First().transform.position, segmentEndToAdd.laneNodesOut.Last().transform.position);
        }
        

        float newSize = (roadSegmentSize * ((1 + MathF.Sqrt(2)) / 2)) * 2;

        if (newSize > size) {
            size = newSize;
            transform.localScale = new Vector3(size, size, size);
        }

        changeRoadSegmentsPosition(segmentEndToAdd);
        connectRoadSegmentLanes(segmentEndToAdd);

        roadSegmentEnds.Add(segmentEndToAdd);        
    }

    public void changeRoadSegmentsPosition(RoadSegmentEnd roadSegmentEnd) {

        RoadSegment parentRoadSegment = roadSegmentEnd.transform.parent.gameObject.GetComponent(typeof(RoadSegment)) as RoadSegment;
        Vector3 targetRoadHandlePosition = parentRoadSegment.roadHandle.transform.position;

        roadSegmentEnd.transform.position = GetComponent<Collider>().ClosestPoint(targetRoadHandlePosition);
    }

    private void connectRoadSegmentLanes(RoadSegmentEnd newRoadSegmentEnd) {

        foreach (RoadSegmentEnd oldRoadSegmentEnd in roadSegmentEnds) {

            int mostLanes = new [] {oldRoadSegmentEnd.laneNodesOut.Count, oldRoadSegmentEnd.laneNodesIn.Count, newRoadSegmentEnd.laneNodesOut.Count, newRoadSegmentEnd.laneNodesIn.Count}.Max();

            for (int i = 0; i < mostLanes; i++) {

                LaneNode laneNodeFrom;
                LaneNode laneNodeTo;

                if ((i < oldRoadSegmentEnd.laneNodesOut.Count || i < newRoadSegmentEnd.laneNodesIn.Count) && oldRoadSegmentEnd.laneNodesOut.Count != 0 && newRoadSegmentEnd.laneNodesIn.Count !=0) {

                    if (i < oldRoadSegmentEnd.laneNodesOut.Count) {
                        laneNodeFrom = oldRoadSegmentEnd.laneNodesOut[i];
                    } else {
                        laneNodeFrom = oldRoadSegmentEnd.laneNodesOut.Last();
                    }

                    if (i < newRoadSegmentEnd.laneNodesIn.Count) {
                        laneNodeTo = newRoadSegmentEnd.laneNodesIn[i];
                    } else {
                        laneNodeTo = newRoadSegmentEnd.laneNodesIn.Last();
                    }

                    laneNodeFrom.addConnection(laneNodeTo);
                }

                if ((i < newRoadSegmentEnd.laneNodesOut.Count || i < oldRoadSegmentEnd.laneNodesIn.Count) && newRoadSegmentEnd.laneNodesOut.Count != 0 && oldRoadSegmentEnd.laneNodesIn.Count !=0) {
                    if (i < newRoadSegmentEnd.laneNodesOut.Count) {
                        laneNodeFrom = newRoadSegmentEnd.laneNodesOut[i];
                    } else {
                        laneNodeFrom = newRoadSegmentEnd.laneNodesOut.Last();
                    }

                    if (i < oldRoadSegmentEnd.laneNodesIn.Count) {
                        laneNodeTo = oldRoadSegmentEnd.laneNodesIn[i];
                    } else {
                        laneNodeTo = oldRoadSegmentEnd.laneNodesIn.Last();
                    }

                    laneNodeFrom.addConnection(laneNodeTo);
                }
            }
        }
    }

    public void reDrawLaneLines() {
        foreach (RoadSegmentEnd roadSegmentEnd in roadSegmentEnds) {
            foreach (LaneNode laneInRoadNode in roadSegmentEnd.laneNodesOut) {

                foreach (Handle laneHandle in laneInRoadNode.laneHandles) {

                    Vector3 newHandlePos;

                    Vector3 point1 = laneHandle.from.transform.position;
                    Vector3 point2 = laneHandle.to.transform.position;

                    Vector3 fromDirection = laneHandle.from.transform.forward;
                    Vector3 toDirection = -laneHandle.to.transform.forward;

                    Vector3 centerPosition = Vector3.Lerp(point1, point2, 0.5f);
                    float distanceToCenter = Vector3.Distance(point1, centerPosition);

                    point1 = point1 + (fromDirection * distanceToCenter);
                    point2 = point2 + (toDirection * distanceToCenter);

                    newHandlePos = Vector3.Lerp(point1, point2, 0.5f);

                    laneHandle.transform.position = newHandlePos;
                }
                laneInRoadNode.resetHandlesHeight();
                laneInRoadNode.reDrawLaneLines();
            }
        }
    }
}
