using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    private LaneNode nextNode;
    private LaneNode currentNode;
    public LaneNode endNode;
    public LaneNode startNode;

    private float minimumDistanceToNextVehicle = 10f;

    private float safeMergeDistance = 8.5f;
    
    private float speed = 0;
    private float targetSpeed;


    public float maxSpeed = 50;
    public float acceleration = 5;
    public float deceleration = 5;

    public float leftUntilNextNode;

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

            for (int i = 0; i < currentNode.connectedLaneNodes.Count; i++) {

                LaneNode connection = currentNode.connectedLaneNodes[i];

                if (connection.gameObject.name == pathToEndNode[currentPathStep]) {

                    if (nextNode != null) {
                        nextNode.vehiclesOnTheirWay.Remove(this);
                    }

                    nextNode = connection;
                    nextNode.vehiclesOnTheirWay.Add(this);
                    currentPathStep++;


                    StartCoroutine(followRoad(currentNode.laneTypes[i]));
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
            
            if (siblingVehicle != null && vision.isSeeing(siblingVehicle.gameObject.GetComponent<Collider>()) && siblingVehicle.currentNode.priority >= currentNode.priority && Math.Abs(siblingVehicle.leftUntilNextNode - leftUntilNextNode) >= safeMergeDistance) {

                targetSpeed = Vector3.Distance(transform.position, nextNode.transform.position);

                if (targetSpeed > currentNode.speedLimit) {
                    targetSpeed = currentNode.speedLimit;
                }
            }


        }

        speed = changeSpeed();
    }

    private IEnumerator followRoad(string laneType) {

        coroutineAllowed = false;
        float laneLength = 0;

        if (laneType == "Straight") {

            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 =  nextNode.transform.position;

            laneLength = Vector3.Distance(p0, p1);

            while (tParam < 1) {
                tParam += Time.deltaTime * (speed / laneLength);

                leftUntilNextNode = laneLength / speed;
                Vector3 vehiclePosition = p0 + tParam * (p1 - p0);

                transform.LookAt(vehiclePosition);
                transform.position = vehiclePosition;
                yield return new WaitForEndOfFrame();
            }

        } else if (laneType  == "Curved") {

            float laneGizmosT = 0f;
            List<Vector3> laneGizmos = new List<Vector3>();

            Transform laneHandleTF = null;

            foreach (Transform child in currentNode.transform) {
                if (child.gameObject.name == "Lane Handle to " + nextNode.gameObject.name) {
                    laneHandleTF = child;
                    break;
                }
            }

            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 = laneHandleTF.position;
            Vector3 p2 = nextNode.transform.position;

            while (laneGizmosT < 1) {
                laneGizmosT += Time.deltaTime * speed ;

                laneGizmos.Add(p1 + 
                               Mathf.Pow(1 - laneGizmosT, 2) * (p0 - p1) +
                               Mathf.Pow(laneGizmosT, 2) * (p2 - p1)
                              );
            }

            for (int i = 0; i < laneGizmos.Count - 1; i++) {

                Vector3 g0 = laneGizmos[i];
                Vector3 g1 = laneGizmos[i + 1];

                laneLength += Vector3.Distance(g0, g1);
            }

            while (tParam < 1) {
                tParam += Time.deltaTime * (speed / laneLength);

                leftUntilNextNode = laneLength / speed;

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
