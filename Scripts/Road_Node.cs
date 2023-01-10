using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road_Node : MonoBehaviour {

    public List<Road_Node> connectedNodes = new List<Road_Node>();

    public List<string> roadTypes = new List<string>();

    private void OnDrawGizmos() {

        for (int i = 0; i < connectedNodes.Count; i++) {

            Road_Node target = connectedNodes[i];

            if (roadTypes[i] == "Straight") {

                foreach (Transform child in transform) {
                    if (child.gameObject.name == "Handle-" + target.gameObject.name) {
                        DestroyImmediate(child.gameObject);
                        break;
                    }
                }

                Gizmos.DrawLine(transform.position, target.transform.position);

            } else if (roadTypes[i] == "Curved") {

                Vector3 gizmosPosition;
                Vector3 lastGizmosPosition = new Vector3(0, 0 ,0);
                GameObject handle = null;

                bool handleSpawned = false;

                foreach (Transform child in transform) {
                    if (child.gameObject.name == "Handle-" + target.gameObject.name) {
                        handleSpawned = true;
                        handle = child.gameObject;
                        break;
                    }
                }

                if (!handleSpawned) {
                    Vector3 handlePos = new Vector3((transform.position.x + target.transform.position.x)/2, (transform.position.y + target.transform.position.y)/2, (transform.position.z + target.transform.position.z)/2);

                    handle = new GameObject("Handle-" + target.gameObject.name);
                    
                    handle.transform.position = handlePos;
                    handle.transform.parent = transform;
                }
                
                for (float t = 0; t <= 1; t += 0.05f) {

                    gizmosPosition = handle.transform.position + 
                                    Mathf.Pow(1 - t, 2) * (transform.position - handle.transform.position) +
                                    Mathf.Pow(t, 2) * (target.transform.position - handle.transform.position);
                    
                    if (t != 0) {
                        Gizmos.DrawLine(lastGizmosPosition, gizmosPosition);
                    }

                    lastGizmosPosition = gizmosPosition;
                }
            }
        }
    }
}
