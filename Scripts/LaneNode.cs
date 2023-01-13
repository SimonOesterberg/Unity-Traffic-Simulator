using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LaneNode : MonoBehaviour {

    public float speedLimit = 100;
    public int priority = 0;

    [SerializeField] private GameObject laneHandle;

    public List<LaneNode> connectedLaneNodes = new List<LaneNode>();

    [System.NonSerialized] public List<Vehicle> vehiclesOn = new List<Vehicle>();
    [System.NonSerialized] public List<Vehicle> vehiclesOnTheirWay = new List<Vehicle>();

    public List<string> laneTypes = new List<string>();

    private void OnDrawGizmos() {

        for (int i = 0; i < connectedLaneNodes.Count; i++) {

            LaneNode target = connectedLaneNodes[i];

            if (laneTypes[i] == "Straight") {

                foreach (Transform child in transform) {
                    if (child.gameObject.name == "Handle-" + target.gameObject.name) {
                        DestroyImmediate(child.gameObject);
                        break;
                    }
                }

                Gizmos.DrawLine(transform.position, target.transform.position);

            } else if (laneTypes[i] == "Curved") {

                Vector3 gizmosPosition;
                Vector3 lastGizmosPosition = new Vector3(0, 0 ,0);
                GameObject handleGO = null;

                bool handleSpawned = false;

                foreach (Transform child in transform) {
                    if (child.gameObject.name == "Lane Handle to " + target.gameObject.name) {
                        handleSpawned = true;
                        handleGO = child.gameObject;
                        break;
                    }
                }

                if (!handleSpawned) {
                    Vector3 handlePos = new Vector3((transform.position.x + target.transform.position.x)/2, (transform.position.y + target.transform.position.y)/2, (transform.position.z + target.transform.position.z)/2);

                    handleGO = Instantiate(laneHandle, handlePos, Quaternion.identity, transform);

                    handleGO.name = "Lane Handle to " + target.gameObject.name;
                }
                
                for (float t = 0; t <= 1; t += 0.05f) {

                    gizmosPosition = handleGO.transform.position + 
                                    Mathf.Pow(1 - t, 2) * (transform.position - handleGO.transform.position) +
                                    Mathf.Pow(t, 2) * (target.transform.position - handleGO.transform.position);
                    
                    if (t != 0) {
                        Gizmos.DrawLine(lastGizmosPosition, gizmosPosition);
                    }

                    lastGizmosPosition = gizmosPosition;
                }

                Handles.DrawDottedLine(transform.position, handleGO.transform.position, 4.0f);
                Handles.DrawDottedLine(target.transform.position, handleGO.transform.position, 4.0f);
            }
        }
    }
}
