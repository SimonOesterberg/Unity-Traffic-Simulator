using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    private Road_Node nextNode;
    private Road_Node currentNode;
    public Road_Node endNode;
    public Road_Node startNode;

    private float minimumDistanceToNextVehicle = 3f;

    private float speed = 0;
    private float targetSpeed;


    public float maxSpeed = 50;
    public float acceleration = 5;
    public float deceleration = 5;

    private float tParam;

    private Vector3 vehiclePosition;

    private bool coroutineAllowed;

    private List<string> pathToEndNode = new List<string>();

    private int currentPathStep = 1;

    private Vision vision;

    private float changeSpeed () {

        float localSpeed = speed;

        if (speed < targetSpeed * 0.25 && speed < maxSpeed * 0.25) {
            localSpeed += (2 * acceleration) * Time.deltaTime;
        } else if (speed > targetSpeed * 0.25) {
            if (localSpeed - (10 * deceleration) * Time.deltaTime < 0) {
                localSpeed = 0;
            } else {
                localSpeed -= (10 * deceleration) * Time.deltaTime;
            }
        }

        return localSpeed;
    }

    void Start() {
        vision = gameObject.GetComponent(typeof(Vision)) as Vision;
        transform.position = startNode.transform.position;
        currentNode = startNode;
        tParam = 0f;
        coroutineAllowed = true;

        pathToEndNode = Pathfinder.getPath(startNode, endNode);
        targetSpeed = currentNode.speedLimit;
        speed = changeSpeed();
    }

    void Update() {

        if (coroutineAllowed) {

            currentNode.vehiclesOn.Add(this);

            for (int i = 0; i < currentNode.connectedNodes.Count; i++) {

                Road_Node connection = currentNode.connectedNodes[i];

                if (connection.gameObject.name == pathToEndNode[currentPathStep]) {

                    if (nextNode != null) {
                        nextNode.vehiclesOnTheirWay.Remove(this);
                    }

                    nextNode = connection;
                    nextNode.vehiclesOnTheirWay.Add(this);
                    currentPathStep++;


                    StartCoroutine(followRoad(currentNode.roadTypes[i]));
                    break;
                }
            }
        }

        Vehicle vehicleInFront = null;
        Vehicle siblingVehicle = null;

        if (currentNode.vehiclesOn[0] != this) {
            for (int j = 0; j < currentNode.vehiclesOn.Count; j++) {
                if (currentNode.vehiclesOn[j] == this) {
                    vehicleInFront = currentNode.vehiclesOn[j - 1];
                }
            }
        } else if (nextNode.vehiclesOn.Count > 0) {
            vehicleInFront = nextNode.vehiclesOn.Last();
        }

        if (nextNode.vehiclesOnTheirWay.Count > 1) {
            for (int j = 0; j < nextNode.vehiclesOnTheirWay.Count; j++) {
                if (nextNode.vehiclesOnTheirWay[j] != this) {
                    siblingVehicle = nextNode.vehiclesOnTheirWay[j];
                    break;
                }
            }
        }

        targetSpeed = currentNode.speedLimit;

        if (vehicleInFront != null || siblingVehicle != null) {

            if (vehicleInFront != null && vision.isSeeing(vehicleInFront.gameObject.GetComponent<Collider>()) && vehicleInFront.speed < speed && Vector3.Distance(transform.position, vehicleInFront.transform.position) < minimumDistanceToNextVehicle) {
                targetSpeed = vehicleInFront.speed;
            } 
            
            if (siblingVehicle != null ) {
                if (siblingVehicle.currentNode.priority > currentNode.priority) {
                        targetSpeed = Vector3.Distance(transform.position, nextNode.transform.position) * 5;

                        if (targetSpeed > currentNode.speedLimit) {
                            targetSpeed = currentNode.speedLimit;
                        }
                }
            }


        }

        speed = changeSpeed();
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

        currentNode.vehiclesOn.Remove(this);
        currentNode = nextNode;
        

        if (currentPathStep == pathToEndNode.Count) {
            Destroy(this.gameObject);
        } else {
            coroutineAllowed = true;
        }
    }
}
