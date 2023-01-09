using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road_Node : MonoBehaviour {

    [SerializeField]
    public List<Road_Node> connectedNodes = new List<Road_Node>();

    [SerializeField]
    public string roadType;

    private void OnDrawGizmos() {

        for (int i = 0; i < connectedNodes.Count; i++) {

            Road_Node target = connectedNodes[i];

            if (target.roadType == "Straight") {

                foreach (Transform child in target.transform) {
                    if (child.gameObject.name == "Handle-" + transform.gameObject.name) {
                        DestroyImmediate(child.gameObject);
                        break;
                    }
                }

                Gizmos.DrawLine(transform.position, target.transform.position);

            } else if (target.roadType == "Curved") {

                Vector3 gizmosPosition;
                Vector3 lastGizmosPosition = new Vector3(0, 0 ,0);
                GameObject handle = null;

                bool handleSpawned = false;

                foreach (Transform child in target.transform) {
                    if (child.gameObject.name == "Handle-" + transform.gameObject.name) {
                        handleSpawned = true;
                        handle = child.gameObject;
                        break;
                    }
                }

                if (!handleSpawned) {
                    Vector3 handlePos = new Vector3((transform.position.x + target.transform.position.x)/2, (transform.position.y + target.transform.position.y)/2, (transform.position.z + target.transform.position.z)/2);

                    handle = new GameObject("Handle-" + transform.gameObject.name);

                    handle.transform.position = handlePos;

                    handle.transform.parent = target.transform;
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
