using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LaneNode : MonoBehaviour {

    // The laneHandle prefab
    [SerializeField] private GameObject laneHandlePrefab;

    // Sets a maximum speed that is allowed on the lanes from this lane
    public float speedLimit = 100;

    // Gives the lane node a priority so that correct merging can occur
    public int priority = 0;

    // Lists of all lanes that this lane node leads to and what type of road should be inbetween
    public List<LaneNode> connectedLaneNodes = new List<LaneNode>();
    public List<string> laneTypes = new List<string>();



    // List of all vehicles currently travelling to and from this lane node
    [System.NonSerialized] public List<VehicleController> vehiclesOnTheirWay = new List<VehicleController>();
    [System.NonSerialized] public List<VehicleController> vehiclesOn = new List<VehicleController>();

    

    // Function that shows how vehicles will travel on the lane by drawing lines
    private void OnDrawGizmos() {

        // For every lane nodes connected to this lane node
        for (int i = 0; i < connectedLaneNodes.Count; i++) {

            
            string targetLaneNodeName = connectedLaneNodes[i].gameObject.name;
            Vector3 targetLaneNodePosition = connectedLaneNodes[i].transform.position;

            if (laneTypes[i] == "Straight") {

                // If path to next lane node should be straight then:
                
                // Draw a straight line between the lane nodes
                Gizmos.DrawLine(transform.position, targetLaneNodePosition);

            } else if (laneTypes[i] == "Curved") {

                // If path to next lane node should be curved then:

                Vector3 laneHandlePosition = transform.position;

                bool handleExist = false;

                // If there already exist a handle for this lane node, set the laneHandlePosition to its position
                foreach (Transform child in transform) {
                    if (child.gameObject.name == "Lane Handle to " + targetLaneNodeName) {
                        laneHandlePosition = child.position;
                        handleExist = true;
                        break;
                    }
                }

                if (!handleExist) {
                    
                    // If no handle was found:

                    // Instatiate a handle at the midpoint of this and the connected lane node and give it the correct name
                    laneHandlePosition = Vector3.Lerp(transform.position, targetLaneNodePosition, 0.5f);
                    GameObject newLaneHandle = Instantiate(laneHandlePrefab, laneHandlePosition, Quaternion.identity, transform);
                    newLaneHandle.name = "Lane Handle to " + targetLaneNodeName;
                }

                // Draw dotted lines between the handle and the start/end point of the path
                Handles.DrawDottedLine(transform.position, laneHandlePosition, 4.0f);
                Handles.DrawDottedLine(targetLaneNodePosition, laneHandlePosition, 4.0f);

                // Stores the position of the previous gizmo along the path
                Vector3 lastGizmoPosition = transform.position;
                
                // Draw lines between points along the curved path between this and the targetnode
                for (float t = 0.05f; t <= 1.05f; t += 0.05f) {

                    Vector3 gizmoPosition = laneHandlePosition + 
                                            Mathf.Pow(1 - t, 2) * (transform.position - laneHandlePosition) +
                                            Mathf.Pow(t, 2) * (targetLaneNodePosition - laneHandlePosition);
                    
                    Gizmos.DrawLine(lastGizmoPosition, gizmoPosition);
                    lastGizmoPosition = gizmoPosition;
                }

            }
            
            if (laneTypes[i] != "Curved") {
                // Remove any handles as they're not neccesary if lanetype is not curved
                foreach (Transform child in transform) {
                    if (child.gameObject.name == "Handle-" + targetLaneNodeName) {
                        DestroyImmediate(child.gameObject);
                        break;
                    }
                }
            }
        }
    }
}
