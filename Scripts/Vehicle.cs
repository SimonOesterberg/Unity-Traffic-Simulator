using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {
    
    [SerializeField]
    private float kph;

    public Road_Node targetNode;

    public Road_Node currentNode;

    private float speed;

    private float tParam;

    private Vector3 vehiclePosition;

    private bool coroutineAllowed;

    void Start() {

        transform.position = targetNode.transform.position;
        tParam = 0f;
        coroutineAllowed = true;
        speed = 0.25f * kph;
    }

    void Update() {
        if (coroutineAllowed) {
            currentNode = targetNode;

            if (targetNode.connectedNodes.Count > 0) {

                System.Random randomNumberGenerator = new System.Random();

                int randomWay = randomNumberGenerator.Next(0, targetNode.connectedNodes.Count);

                targetNode = targetNode.connectedNodes[randomWay];

                StartCoroutine(followRoad(randomWay));
            }
        }
    }

    private IEnumerator followRoad(int connectionNr) {

        coroutineAllowed = false;
        float roadLength = 0;

        if (targetNode.roadTypes[connectionNr] == "Straight") {

            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 =  targetNode.transform.position;

            roadLength = Mathf.Pow((Mathf.Pow(p1.x-p0.x, 2) + Mathf.Pow(p1.y-p0.y, 2) + Mathf.Pow(p1.z-p0.z, 2)), 0.5f);

            while (tParam < 1) {
                tParam += Time.deltaTime * (speed / roadLength);

                Vector3 vehiclePosition = p0 + tParam * (p1 - p0);

                transform.LookAt(vehiclePosition);
                transform.position = vehiclePosition;
                yield return new WaitForEndOfFrame();
            }

        } else if (targetNode.roadTypes[connectionNr]  == "Curved") {

            float roadGizmosT = 0f;
            List<Vector3> roadGizmos = new List<Vector3>();

            Transform handle = null;

            foreach (Transform child in targetNode.transform) {
                if (child.gameObject.name == "Handle-" + currentNode.gameObject.name) {
                    handle = child;
                    break;
                }
            }

            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 = handle.position;
            Vector3 p2 = targetNode.transform.position;

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

                roadLength += Mathf.Pow((Mathf.Pow(g1.x-g0.x, 2) + Mathf.Pow(g1.y-g0.y, 2) + Mathf.Pow(g1.z-g0.z, 2)), 0.5f);
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

        coroutineAllowed = true;
    }
}
