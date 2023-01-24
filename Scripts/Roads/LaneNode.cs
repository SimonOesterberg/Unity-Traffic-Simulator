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

    public List<LineRenderer> laneLines = new List<LineRenderer>();

    public List<Handle> laneHandles = new List<Handle>();

    public List<Vector3[]> lanePaths = new List<Vector3[]>();

    private float laneLineSubdivisions = 20;

    public Material laneMaterial;



    // List of all vehicles currently travelling to and from this lane node
    [System.NonSerialized] public List<VehicleController> vehiclesOnTheirWay = new List<VehicleController>();
    [System.NonSerialized] public List<VehicleController> vehiclesOn = new List<VehicleController>();

    public void addConnection(LaneNode newConnection) {
        // Instatiate a handle at the midpoint of this and the connected lane node and give it the correct name
        Vector3 laneHandlePosition = Vector3.Lerp(transform.position, newConnection.transform.position, 0.5f);

        RoadSegment thisLanesRoadSegment = transform.parent.parent.gameObject.GetComponent(typeof(RoadSegment)) as RoadSegment;

        RoadSegmentEnd thisLanesRoadSegmentEnd = transform.parent.gameObject.GetComponent(typeof(RoadSegmentEnd)) as RoadSegmentEnd;
        RoadSegmentEnd targetLanesRoadSegmentEnd = newConnection.transform.parent.gameObject.GetComponent(typeof(RoadSegmentEnd)) as RoadSegmentEnd;

        Transform laneHandlesParentTF;

        if (thisLanesRoadSegmentEnd.laneNodesOut.Contains(this) && targetLanesRoadSegmentEnd.laneNodesIn.Contains(newConnection)) {
            laneHandlesParentTF = thisLanesRoadSegmentEnd.roadNode.transform;
        } else {
            laneHandlesParentTF = thisLanesRoadSegment.roadHandle.transform;
        }

        Handle newLaneHandle = new GameObject("Lane Handle").AddComponent<Handle>();
        newLaneHandle.transform.localPosition = laneHandlePosition;
        newLaneHandle.transform.LookAt(newConnection.transform.position);
        newLaneHandle.transform.parent = laneHandlesParentTF;
        newLaneHandle.from = this;
        newLaneHandle.to = newConnection;
        

        laneHandles.Add(newLaneHandle);
        connectedLaneNodes.Add(newConnection);


        drawLaneLine(newConnection, newLaneHandle);
    }

    public void drawLaneLine(LaneNode connection, Handle laneHandle) {

            Vector3 laneHandlePosition = laneHandle.transform.position;
            Vector3 connectedLaneNodePosition = connection.transform.position;

            Vector3[] points = getPointsAlongCurve(transform.position, laneHandlePosition, connectedLaneNodePosition);

            LineRenderer lr = new GameObject("Lane Line").AddComponent<LineRenderer>();
            lr.transform.parent = transform;
            lr.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            lr.positionCount = points.Length;
            lr.sortingOrder = 1;
            lr.material = laneMaterial;
            lr.material.color = Color.green;

            lr.SetPositions(points);

            laneLines.Add(lr);
    }

    public void reDrawLaneLines() {

        for (int i = 0; i < connectedLaneNodes.Count; i++) {

            Vector3 laneHandlePosition = laneHandles[i].transform.position;
            Vector3 connectedLaneNodePosition = connectedLaneNodes[i].transform.position;

            Vector3[] points = getPointsAlongCurve(transform.position, laneHandlePosition, connectedLaneNodePosition);
            laneLines[i].SetPositions(points);
        }
    }



    private Vector3[] getPointsAlongCurve(Vector3 p0, Vector3 p1, Vector3 p2) {

        Vector3[] points = new Vector3[(int)laneLineSubdivisions + 1];
        int pointToAdd = 0;

        for (float t = 0; t <= 1f + (1 / laneLineSubdivisions); t += (1 / laneLineSubdivisions)) {

            points[pointToAdd] = p1 + 
                                 Mathf.Pow(1 - t, 2) * (p0 - p1) +
                                 Mathf.Pow(t, 2) * (p2 - p1);
            
            pointToAdd++;
        }

        return points;
    }

    public void resetHandlesHeight() {

        foreach (Handle laneHandle in laneHandles) {
            
            Vector3 oldHandlePosition = laneHandle.transform.position;
            float newHandleHeight = laneHandle.transform.parent.position.y;

            laneHandle.transform.position = new Vector3(oldHandlePosition.x, newHandleHeight, oldHandlePosition.z);
        } 
    }
}
