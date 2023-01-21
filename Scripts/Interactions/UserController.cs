using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserController : MonoBehaviour {

    [SerializeField] private float cameraSpeed = 10;
    [SerializeField] private RoadNode roadNode;
    private string newRoadLanesType = null;

    private string selectedTool = null;
    private bool interactionsBusy = false;

    private GameObject previewNode = null;
    private RoadNode newRoadStart = null;
    private RoadNode newRoadEnd = null;
    private Handle newRoadHandle = null;
    private bool clickAllowed = true;

    private Transform nodeToMoveTF = null;

    [SerializeField] private Material laneMaterial;

    [SerializeField] private Handle roadHandlePrefab;

    private float amountOfPointsAlongCurve = 20;

     void Update() {
        if (Input.GetMouseButtonUp(0)) {
            clickAllowed = true;
        }

        if (!interactionsBusy) {
            if (Input.GetMouseButtonDown(0) && clickAllowed && !EventSystem.current.IsPointerOverGameObject()) {
                clickAllowed = false;

                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && hit.collider) {
                    if (selectedTool == "Create Road") {
                        if (hit.collider.tag == "Road") {
                            newRoadStart = hit.collider.gameObject.GetComponent(typeof(RoadNode)) as RoadNode;
                        } else if (hit.collider.tag == "Terrain") {
                            newRoadStart = Instantiate(roadNode, hit.point, Quaternion.identity);
                            newRoadStart.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                        }

                        if (newRoadLanesType == "Straight") {
                            RoadNode endPreview = Instantiate(roadNode, hit.point, Quaternion.identity);
                            endPreview.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                            previewNode = endPreview.gameObject;
                        } else if (newRoadLanesType == "Curved") {
                            
                            Handle handlePreview = Instantiate(roadHandlePrefab, hit.point, Quaternion.identity);
                            handlePreview.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                            previewNode = handlePreview.gameObject;
                        }

                        newRoadStart.gameObject.layer = 0;
                        interactionsBusy = true;
                    }
                }
            }
        } else {
            if (selectedTool == "Create Road") {
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)) {
                    
                    previewNode.transform.position = hit.point;

                    
                    newRoadStart.transform.LookAt(hit.point);
                    previewNode.transform.rotation = newRoadStart.transform.rotation;

                    if (Input.GetMouseButtonDown(0) && clickAllowed && !EventSystem.current.IsPointerOverGameObject()) {
                        clickAllowed = false;

                        Destroy(previewNode.gameObject);
                        newRoadEnd = Instantiate(roadNode, hit.point, previewNode.transform.rotation);
                        newRoadStart.connectedRoadNodes.Add(newRoadEnd);

                        if (newRoadLanesType == "Curved") {

                            Vector3 roadHandlePosition = Vector3.Lerp(newRoadStart.transform.position, newRoadEnd.transform.position, 0.5f);

                            newRoadHandle = Instantiate(roadHandlePrefab, roadHandlePosition, Quaternion.identity, newRoadStart.transform);
                            newRoadHandle.transform.LookAt(newRoadEnd.transform.position);

                            newRoadStart.roadHandles.Add(newRoadHandle.gameObject);
                            

                            nodeToMoveTF = newRoadEnd.transform;
                        }

                        drawRoad(newRoadStart, newRoadEnd);

                        if (newRoadLanesType == "Straight") {
                            interactionsBusy = false;
                        } else if (newRoadLanesType == "Curved") {
                            selectedTool = "Move Node";
                        }
                    }
                }
            } else if (selectedTool == "Move Node") {

                nodeToMoveTF.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                Vector3 roadCenter = Vector3.Lerp(newRoadStart.transform.position, newRoadEnd.transform.position, 0.5f);

                if (Physics.Raycast(ray, out hit)) {
                    
                    nodeToMoveTF.position = hit.point;

                    newRoadStart.transform.LookAt(newRoadHandle.transform);
                    newRoadEnd.transform.LookAt(newRoadEnd.transform.position - (newRoadHandle.transform.position - newRoadEnd.transform.position));
                   

                    newRoadHandle.transform.rotation = Quaternion.Lerp(newRoadStart.transform.rotation, newRoadEnd.transform.rotation, 0.5f);

                    float handlesDistanceFromCenter = Vector3.Distance(newRoadHandle.transform.position, roadCenter);

                    float newScale = 1 + handlesDistanceFromCenter * 0.005f; 

                    newRoadHandle.transform.localScale = new Vector3(newScale, newScale, newScale);

                    foreach (Transform laneNodeTF in newRoadStart.transform) {
                        if ((laneNodeTF.gameObject.GetComponent(typeof(LaneNode)) as LaneNode) != null) {

                            LaneNode laneNodeNode = laneNodeTF.gameObject.GetComponent(typeof(LaneNode)) as LaneNode;

                            for (int i = 0; i < laneNodeNode.connectedLaneNodes.Count; i++) {
                                laneNodeNode.laneLines[i].SetPositions(getPointsAlongCurve(laneNodeTF.position, laneNodeNode.laneHandles[i].transform.position, laneNodeNode.connectedLaneNodes[i].transform.position));
                            }
                        }
                    }

                    foreach (Transform laneNodeTF in newRoadEnd.transform) {
                        if ((laneNodeTF.gameObject.GetComponent(typeof(LaneNode)) as LaneNode) != null) {

                            LaneNode laneNodeNode = laneNodeTF.gameObject.GetComponent(typeof(LaneNode)) as LaneNode;

                            for (int i = 0; i < laneNodeNode.connectedLaneNodes.Count; i++) {
                                laneNodeNode.laneLines[i].SetPositions(getPointsAlongCurve(laneNodeTF.position, laneNodeNode.laneHandles[i].transform.position, laneNodeNode.connectedLaneNodes[i].transform.position));
                            }
                        }
                    }

                    if (Input.GetMouseButtonDown(0) && clickAllowed && !EventSystem.current.IsPointerOverGameObject()) {
                        nodeToMoveTF.gameObject.layer = 0;

                        clickAllowed = false;
                        interactionsBusy = false;
                        selectedTool = "Create Road";
                    }
                }
            }
        }
    }

    private void drawRoad(RoadNode roadNodeFrom, RoadNode roadNodeTo) {

        int mostLanes = new [] { roadNodeFrom.laneNodesRight.Count, roadNodeFrom.laneNodesLeft.Count, roadNodeTo.laneNodesRight.Count, roadNodeTo.laneNodesLeft.Count }.Max();

        for (int i = 0; i < mostLanes; i++) {

            LaneNode laneNodeFrom;
            LaneNode laneNodeTo;

            if (i < roadNodeFrom.laneNodesRight.Count || i < roadNodeTo.laneNodesRight.Count) {
                if (i < roadNodeFrom.laneNodesRight.Count) {
                    laneNodeFrom = roadNodeFrom.laneNodesRight[i];
                } else {
                    laneNodeFrom = roadNodeFrom.laneNodesRight.Last();
                }

                if (i < roadNodeTo.laneNodesRight.Count) {
                    laneNodeTo = roadNodeTo.laneNodesRight[i];
                } else {
                    laneNodeTo = roadNodeTo.laneNodesRight.Last();
                }

                laneNodeFrom.connectedLaneNodes.Add(laneNodeTo);
                laneNodeFrom.laneTypes.Add(newRoadLanesType);

                if (newRoadLanesType == "Straight") {
                    drawStraightLane(laneNodeFrom, laneNodeTo);
                } else if (newRoadLanesType == "Curved") {
                    drawCurvedLane(laneNodeFrom, laneNodeTo, "Right");
                }
            }

            if (i < roadNodeFrom.laneNodesLeft.Count || i < roadNodeTo.laneNodesLeft.Count) {
                if (i < roadNodeFrom.laneNodesLeft.Count) {
                    laneNodeFrom = roadNodeFrom.laneNodesLeft[i];
                } else {
                    laneNodeFrom = roadNodeFrom.laneNodesLeft.Last();
                }

                if (i < roadNodeTo.laneNodesLeft.Count) {
                    laneNodeTo = roadNodeTo.laneNodesLeft[i];
                } else {
                    laneNodeTo = roadNodeTo.laneNodesLeft.Last();
                }

                laneNodeTo.connectedLaneNodes.Add(laneNodeFrom);
                laneNodeTo.laneTypes.Add(newRoadLanesType);

                if (newRoadLanesType == "Straight") {
                    drawStraightLane(laneNodeTo, laneNodeFrom);
                } else if (newRoadLanesType == "Curved") {
                    drawCurvedLane(laneNodeTo, laneNodeFrom, "Left");
                }
            }

            
        }
    }

    private void drawLaneLine(LaneNode startLaneNode, Vector3[] points) {
        LineRenderer lr = new GameObject("Lane").AddComponent<LineRenderer>();
        lr.transform.parent = startLaneNode.transform;
        lr.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        lr.positionCount = points.Length;
        lr.sortingOrder = 1;
        lr.material = laneMaterial;
        lr.material.color = Color.green;

        lr.SetPositions(points);

        startLaneNode.laneLines.Add(lr);
    }

    private void drawStraightLane(LaneNode laneNodeFrom, LaneNode laneNodeTo) {
        drawLaneLine(laneNodeFrom, new Vector3[] {laneNodeFrom.transform.position, laneNodeTo.transform.position});
    }

    private void drawCurvedLane(LaneNode laneNodeFrom, LaneNode laneNodeTo, string laneSide) {
        // If path to next lane node should be curved then:

        Vector3 laneHandlePosition = new Vector3(0, 0, 0);
        Transform roadNodeFromTF;
        Transform roadNodeToTF;

        if (laneSide == "Right") {
            roadNodeFromTF = laneNodeFrom.transform.parent;
            roadNodeToTF = laneNodeTo.transform.parent;
        } else {
            roadNodeFromTF = laneNodeTo.transform.parent;
            roadNodeToTF = laneNodeFrom.transform.parent;
        }

        RoadNode roadNodeFrom = roadNodeFromTF.gameObject.GetComponent(typeof(RoadNode)) as RoadNode;
        RoadNode roadNodeTo = roadNodeToTF.gameObject.GetComponent(typeof(RoadNode)) as RoadNode;

        // Instatiate a handle at the midpoint of this and the connected lane node and give it the correct name
        laneHandlePosition = Vector3.Lerp(laneNodeFrom.transform.position, laneNodeTo.transform.position, 0.5f);

        Transform laneHandleParentTF = null;

        for (int i = 0; i < roadNodeFrom.connectedRoadNodes.Count; i++) {
            if (roadNodeFrom.connectedRoadNodes[i] == roadNodeTo) {
                laneHandleParentTF = roadNodeFrom.roadHandles[i].transform;
                break;
            }
        }

        GameObject newLaneHandleGO = new GameObject("Lane Handle");
        newLaneHandleGO.transform.localPosition = laneHandlePosition;

        newLaneHandleGO.transform.parent = laneHandleParentTF;
        

        laneNodeFrom.laneHandles.Add(newLaneHandleGO);

        // Draw dotted lines between the handle and the start/end point of the path
        Debug.DrawLine(laneNodeFrom.transform.position, laneHandlePosition, Color.yellow, .5f);
        Debug.DrawLine(laneNodeTo.transform.position, laneHandlePosition, Color.yellow, .5f);

        // Stores the position of the previous gizmo along the path
        
 
        drawLaneLine(laneNodeFrom, getPointsAlongCurve(laneNodeFrom.transform.position, laneHandlePosition, laneNodeTo.transform.position));
    }

    private Vector3[] getPointsAlongCurve(Vector3 p0, Vector3 p1, Vector3 p2) {

        Vector3[] points = new Vector3[(int)amountOfPointsAlongCurve + 1];
        int pointToAdd = 0;

        // Draw lines between points along the curved path between this and the targetnode
        for (float t = 0; t <= 1f + (1 / amountOfPointsAlongCurve); t += (1 / amountOfPointsAlongCurve)) {

            points[pointToAdd] = p1 + 
                                 Mathf.Pow(1 - t, 2) * (p0 - p1) +
                                 Mathf.Pow(t, 2) * (p2 - p1);
            
            pointToAdd++;
        }

        return points;
    }

    public void selectToolCreateRoad() {
        selectedTool = "Create Road";
    }

    public void selectLanesTypeStraight() {
        newRoadLanesType = "Straight";
    }

    public void selectLanesTypeCurved() {
        newRoadLanesType = "Curved";
    }
}
