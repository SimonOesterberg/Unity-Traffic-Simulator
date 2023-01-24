using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserController : MonoBehaviour {

    [SerializeField] private float cameraSpeed = 10;
    [SerializeField] private List<RoadSegment> roadSegmentTypes = new List<RoadSegment>();
    private int selectedRoadSegmentType = 0;
    [SerializeField] private RoadNode roadNode;
    private string newRoadLanesType = null;

    private string selectedTool = null;
    private bool interactionsBusy = false;

    private RoadNode previewNode = null;
    private RoadSegmentEnd newRoadSegmentStart = null;

    private RoadSegment newRoadSegment = null;
    private RoadSegmentEnd newRoadSegmentEnd = null;
    private Handle newRoadSegmentHandle = null;
    private bool clickAllowed = true;

    private RoadNode newRoadNode = null;

    private Transform nodeToMoveTF = null;

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
                            newRoadNode = hit.collider.gameObject.GetComponent(typeof(RoadNode)) as RoadNode;
                        } else if (hit.collider.tag == "Terrain") {
                            newRoadNode = Instantiate(roadNode, hit.point, Quaternion.identity);
                        }

                        newRoadSegment = Instantiate(roadSegmentTypes[selectedRoadSegmentType], newRoadNode.transform.position, Quaternion.identity);

                        newRoadSegmentStart = newRoadSegment.roadSegmentEnds[0];
                        newRoadSegmentEnd = newRoadSegment.roadSegmentEnds[1];
                        newRoadSegmentHandle = newRoadSegment.roadHandle;

                        newRoadNode.addRoadSegmentEnd(newRoadSegmentStart);

                        previewNode = Instantiate(roadNode, hit.point, Quaternion.identity);
                        previewNode.addRoadSegmentEnd(newRoadSegmentEnd);
                        previewNode.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

                        interactionsBusy = true;
                    }
                }
            }
        } else {
            if (selectedTool == "Create Road") {
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit)) {

                    if (hit.collider.tag == "Road") {
                        previewNode.transform.position = hit.collider.transform.position;
                    } else if (hit.collider.tag == "Terrain") {
                        previewNode.transform.position = hit.point;
                    }

                    newRoadNode.changeRoadSegmentsPosition(newRoadSegmentStart);
                    previewNode.changeRoadSegmentsPosition(newRoadSegmentEnd);

                    newRoadSegmentHandle.transform.position = Vector3.Lerp(newRoadSegmentStart.transform.position, newRoadSegmentEnd.transform.position, 0.5f);

                    newRoadSegmentStart.transform.LookAt(hit.point);
                    newRoadSegmentHandle.transform.rotation = newRoadSegmentStart.transform.rotation;
                    newRoadSegmentEnd.transform.rotation = newRoadSegmentStart.transform.rotation;

                    

                    newRoadSegment.reDrawLaneLines();
                    previewNode.reDrawLaneLines();
                    newRoadNode.reDrawLaneLines();

                    if (Input.GetMouseButtonDown(0) && clickAllowed && !EventSystem.current.IsPointerOverGameObject()) {
                        clickAllowed = false;

                        if (hit.collider.tag == "Road") {
                            if (newRoadLanesType == "Straight") {

                                Destroy(previewNode.gameObject);

                                previewNode = null;
                                RoadNode newRoadEndNode = hit.collider.gameObject.GetComponent(typeof(RoadNode)) as RoadNode;

                                newRoadEndNode.addRoadSegmentEnd(newRoadSegmentEnd);
                                newRoadEndNode.changeRoadSegmentsPosition(newRoadSegmentEnd);

                                newRoadSegmentHandle.transform.position = Vector3.Lerp(newRoadSegmentStart.transform.position, newRoadSegmentEnd.transform.position, 0.5f);

                                newRoadEndNode.reDrawLaneLines();
                                newRoadSegment.reDrawLaneLines();

                                interactionsBusy = false;
                            }
                        } else if (hit.collider.tag == "Terrain") {
                            if (newRoadLanesType == "Straight") {

                                previewNode.gameObject.layer = LayerMask.NameToLayer("Default");
                                previewNode = null;

                                interactionsBusy = false;

                            } else if (newRoadLanesType == "Curved") {

                                nodeToMoveTF = previewNode.transform;
                                selectedTool = "Move Node";
                            }
                        }
                    }
                }
            } else if (selectedTool == "Move Node") {
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                Vector3 roadCenter = Vector3.Lerp(newRoadSegmentStart.transform.position, newRoadSegmentEnd.transform.position, 0.5f);

                if (Physics.Raycast(ray, out hit)) {
                    
                    nodeToMoveTF.position = hit.point;

                    if (previewNode != null) {
                        previewNode.changeRoadSegmentsPosition(newRoadSegmentEnd);
                    }

                    newRoadSegmentStart.transform.LookAt(newRoadSegmentHandle.transform);
                    newRoadSegmentEnd.transform.LookAt(newRoadSegmentEnd.transform.position - (newRoadSegmentHandle.transform.position - newRoadSegmentEnd.transform.position));
                   
                    newRoadSegmentHandle.transform.rotation = Quaternion.Lerp(newRoadSegmentStart.transform.rotation, newRoadSegmentEnd.transform.rotation, 0.5f);

                    float handlesDistanceFromCenter = Vector3.Distance(newRoadSegmentHandle.transform.position, roadCenter);

                    float newScale = 1 + handlesDistanceFromCenter * 0.005f; 

                    newRoadSegmentHandle.transform.localScale = new Vector3(newScale, newScale, newScale);

                    newRoadSegment.reDrawLaneLines();

                    if (Input.GetMouseButtonDown(0) && clickAllowed && !EventSystem.current.IsPointerOverGameObject()) {
                        clickAllowed = false;

                        if (hit.collider.tag == "Road") {

                            Destroy(previewNode.gameObject);
                            previewNode = null;

                            RoadNode newRoadEndNode = hit.collider.gameObject.GetComponent(typeof(RoadNode)) as RoadNode;

                            newRoadEndNode.addRoadSegmentEnd(newRoadSegmentEnd);
                            newRoadEndNode.changeRoadSegmentsPosition(newRoadSegmentEnd);
                            newRoadEndNode.reDrawLaneLines();

                            newRoadSegment.reDrawLaneLines();
                        } else if (hit.collider.tag == "Terrain") {
                            if (previewNode != null) {
                                previewNode.gameObject.layer = LayerMask.NameToLayer("Default");
                                previewNode = null;
                            }
                        }

                        

                        interactionsBusy = false;
                        selectedTool = "Create Road";
                    }
                }
            }
        }
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

    public void selectnewRoadSegment() {

        if (selectedRoadSegmentType != roadSegmentTypes.Count() - 1) {
            selectedRoadSegmentType++;
        } else {
            selectedRoadSegmentType = 0;
        }
    }
}
