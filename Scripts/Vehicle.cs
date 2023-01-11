using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {
    
    [System.NonSerialized] public float kph = 0;

    private Road_Node nextNode;

    private Road_Node currentNode;

    public Road_Node endNode;
    public Road_Node startNode;

    private float speed;
    public float targetKph;

    private float tParam;

    private Vector3 vehiclePosition;

    private bool coroutineAllowed;

    private List<string> pathToEndNode = new List<string>();

    private int currentPathStep = 1;

    void Start() {
        transform.position = startNode.transform.position;
        currentNode = startNode;
        tParam = 0f;
        coroutineAllowed = true;

        pathToEndNode = Pathfinder.getPath(startNode, endNode);
    }

    void Update() {

        if (kph < targetKph) {
            kph += 10 * Time.deltaTime;
            speed = 0.25f * kph;
        } else if (kph > targetKph) {
            kph -= 50 * Time.deltaTime;
            speed = 0.25f * kph;
        }

        if (coroutineAllowed) {

            for (int i = 0; i < currentNode.connectedNodes.Count; i++) {

                Road_Node connection = currentNode.connectedNodes[i];

                if (connection.gameObject.name == pathToEndNode[currentPathStep]) {
                    nextNode = connection;
                    currentPathStep++;
                    StartCoroutine(followRoad(currentNode.roadTypes[i]));
                    break;
                }
            }
        }
    }

    private IEnumerator followRoad(string roadType) {

        coroutineAllowed = false;
        float roadLength = 0;

        if (roadType == "Straight") {

            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 =  nextNode.transform.position;

            roadLength = Vector3.Distance(p0, p1);

            while (tParam < 1) {
                tParam += Time.deltaTime * (speed / roadLength);

                Vector3 vehiclePosition = p0 + tParam * (p1 - p0);

                transform.LookAt(vehiclePosition);
                transform.position = vehiclePosition;
                yield return new WaitForEndOfFrame();
            }

        } else if (roadType  == "Curved") {

            float roadGizmosT = 0f;
            List<Vector3> roadGizmos = new List<Vector3>();

            Transform handle = null;

            foreach (Transform child in currentNode.transform) {
                if (child.gameObject.name == "Handle-" + nextNode.gameObject.name) {
                    handle = child;
                    break;
                }
            }

            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 = handle.position;
            Vector3 p2 = nextNode.transform.position;

            while (roadGizmosT < 1) {
                roadGizmosT += Time.deltaTime * speed ;

                roadGizmos.Add(p1 + 
                               Mathf.Pow(1 - roadGizmosT, 2) * (p0 - p1) +
                               Mathf.Pow(roadGizmosT, 2) * (p2 - p1)
                              );
            }

            for (int i = 0; i < roadGizmos.Count - 1; i++) {

                Vector3 g0 = roadGizmos[i];
                Vector3 g1 = roadGizmos[i + 1];

                roadLength += Vector3.Distance(g0, g1);
            }

            while (tParam < 1) {
                tParam += Time.deltaTime * (speed / roadLength);

                vehiclePosition = p1 + 
                                  Mathf.Pow(1 - tParam, 2) * (p0 - p1) +
                                  Mathf.Pow(tParam, 2) * (p2 - p1);
                
                transform.LookAt(vehiclePosition);
                transform.position = vehiclePosition;
                yield return new WaitForEndOfFrame();
            }
        }

        tParam = 0f;

        currentNode = nextNode;

        if (currentPathStep == pathToEndNode.Count) {
            Object.Destroy(this.gameObject);
        } else {
            coroutineAllowed = true;
        }
    }
}
