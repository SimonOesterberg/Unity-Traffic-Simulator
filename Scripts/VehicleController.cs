using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class VehicleController : MonoBehaviour {

    // Stores where the vehicle should begin and end its journey
    public LaneNode startNode;
    public LaneNode endNode;

    // Used to give each vehicle their own maximum speed
    public float maxSpeed = 50;

    // Sets how quickly a vehicle can accelerate and decelerate
    public float acceleration = 5;
    public float deceleration = 5;
    


    // Stores the node that is currently travelled from and to
    private LaneNode currentNode;
    private LaneNode nextNode;

    // Stores how long it is until this vehicle arrives at the next lane node
    private float leftUntilNextNode;
    



    // Sets a minimum allowed distance between this and the vehicle in front
    private float minimumDistanceToNextVehicle = 10f;

    // Sets how much space there has to be between two vehicles heading towards a lane node for a merge to be allowed without slowing down
    private float safeMergeDistance = 8.5f;
    
    // The current speed of the vehicle and the speed it aims to have
    private float speed = 0;
    private float targetSpeed;




    // Boolean of wether the vehicle is allowed to start moving to the next lane node or not
    private bool coroutineAllowed;




    // A list that stores the names of the lanenodes that should be travelled on to reach the endNode the quickest
    private List<string> pathToEndNode = new List<string>();

    // Stores where in the pathToEndNode list the vehicle should go next
    private int currentPathStep = 1;



    // Will store this vehicles vision script
    private Vision vision;


    void Start() {
        // On start:

        //Set vision to the vision script
        vision = gameObject.GetComponent(typeof(Vision)) as Vision;

        //Move the vehicle to the startNode and set that lane node as the current node
        transform.position = startNode.transform.position;
        currentNode = startNode;

        // Get the path returned from the Pathfinder script
        pathToEndNode = Pathfinder.getPath(startNode, endNode);

        // Set the targetspeed to the current lanes speedlimit and speed up to that speed
        targetSpeed = currentNode.speedLimit;
        changeSpeed();

        // Allow the vehicle to travel to the next lane node
        coroutineAllowed = true;
    }

    void Update() {
        // On every frame:

        if (coroutineAllowed) {
            // If vehicle is allowed to travel to the next lane node:

            // Add this vehicle to the current lane nodes vehiclesOn list
            currentNode.vehiclesOn.Add(this);

            for (int i = 0; i < currentNode.connectedLaneNodes.Count; i++) {

                LaneNode connection = currentNode.connectedLaneNodes[i];

                if (connection.gameObject.name == pathToEndNode[currentPathStep]) {
                    // If any of the lane nodes connected to the currently traveled from node has the name of the next node in our path to the end node:

                    // Remove this vehicle from the nextNodes vehicleOnTheirWay list
                    if (nextNode != null) {
                        nextNode.vehiclesOnTheirWay.Remove(this);
                    }

                    // Set the next node to the node that matched the name of the next node on the path
                    nextNode = connection;

                    // Set this vehicle as a vehicle on the way to the new next node
                    nextNode.vehiclesOnTheirWay.Add(this);

                    // Go to the next step of our path to the end
                    currentPathStep++;

                    // Start travelling
                    StartCoroutine(followRoad(currentNode.laneTypes[i]));
                    break;
                }
            }
        }

        // Stores the first vehicle in front of this one
        VehicleController vehicleInFront = null;
        
        if (currentNode.vehiclesOn[0] != this) {
            // If this vehicle is not the first one travelling from this lane node:

            // Find the vehicle that is one ahead of this one and set that as the closest vehicle in front
            for (int j = 0; j < currentNode.vehiclesOn.Count; j++) {
                if (currentNode.vehiclesOn[j] == this) {
                    vehicleInFront = currentNode.vehiclesOn[j - 1];
                }
            }
        } else if (nextNode.vehiclesOn.Count > 0) {
            // If no vehicle in front on this node and there are vehicles on the next node:

            // Set the closest vehicle in front to be the last one that entered the next node
            vehicleInFront = nextNode.vehiclesOn.Last();
        }

        // Set the first vehicle that is travelling to the same node as this one
        VehicleController siblingVehicle = null;
        
        if (nextNode.vehiclesOnTheirWay.Count > 1) {
            // If any vehicle is on their way to the same node as this one is:

            for (int j = 0; j < nextNode.vehiclesOnTheirWay.Count; j++) {

                VehicleController vehicleOnItsWay = nextNode.vehiclesOnTheirWay[j];

                if (vehicleOnItsWay != this) {
                    if (siblingVehicle == null) {
                        // The first vehicle that is not this one while siblingVehicle has not been set will become the sibling vehicle
                        siblingVehicle = vehicleOnItsWay;
                    } else if (vehicleOnItsWay.leftUntilNextNode < siblingVehicle.leftUntilNextNode) {
                        // If siblingVehicle has been set and the vehicle in this loop is closer in time to arrive at the next node set it to the siblingVehicle;
                        siblingVehicle = vehicleOnItsWay;
                    }
                    
                }
            }
        }

        // Before any of the speeds from the vehicle in front or sibing vehicles has any effect set the base speed to be that of the speed limit
        targetSpeed = currentNode.speedLimit;

        if (vehicleInFront != null || siblingVehicle != null) {
            // If there is a vehicle in front or travelling to the same node as this one:

            if (siblingVehicle != null && vision.isSeeing(siblingVehicle.gameObject.GetComponent<Collider>()) && siblingVehicle.currentNode.priority >= currentNode.priority && Math.Abs(siblingVehicle.leftUntilNextNode - leftUntilNextNode) >= safeMergeDistance) {
                // If there is a sibling vehicle that this vehicle can see and that is travelling in a way that will result in the merge distance being too small:

                // Set the target speed based on the distance between this vehicles current position and the next nodes position
                targetSpeed = Vector3.Distance(transform.position, nextNode.transform.position);

                // If the new target speed is higher than the speed limit, set the target speed back to the speed limit
                if (targetSpeed > currentNode.speedLimit) {
                    targetSpeed = currentNode.speedLimit;
                }
            }

            if (vehicleInFront != null && vision.isSeeing(vehicleInFront.gameObject.GetComponent<Collider>()) && vehicleInFront.speed < speed && Vector3.Distance(transform.position, vehicleInFront.transform.position) < minimumDistanceToNextVehicle && vehicleInFront.speed <= targetSpeed) {
                // If there is a vehicle in front that this vehicle can see, that is travelling slower than this vehicle and that is travelling slower than the target speed possibly set by a sibling vehicle:

                // Set the target speed to the speed of the vehicle in front
                targetSpeed = vehicleInFront.speed;
            }


        }

        // Change the speed to the new target speed
        changeSpeed();
    }

    // Function used to change the speed
    private void changeSpeed () {

        if (speed < targetSpeed * 0.25 && speed < maxSpeed * 0.25) {
            // If currently travelling slower that the target speed and slower that this vehicles maximum speed:

            // Speed up based on the acceleration of this vehicle
            speed += (2 * acceleration) * Time.deltaTime;
        } else if (speed > targetSpeed * 0.25) {
            // If the vehicle is travelling faster than the target speed:

            if (speed - (10 * deceleration) * Time.deltaTime < 0) {
                // If the algorithm would set the speed to below 0 just set it to 0 instead
                speed = 0;
            } else {
                // Otherwise set the speed based on this vehicles deceleration
                speed -= (10 * deceleration) * Time.deltaTime;
            }
        }
    }


    // Enumerator that makes the car move
    private IEnumerator followRoad(string laneType) {

        // Make it so that the vehicle cannot travel to the next node on the path before traveling this one
        coroutineAllowed = false;

        // Stores this lanes length
        float laneLength = 0;

        if (laneType == "Straight") {
            // If the lane this vehicle is traveling is straight:

            // Set p0 to the start of the lane and p1 to the end and set lane length to the distance between those two points
            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 =  nextNode.transform.position;
            laneLength = Vector3.Distance(p0, p1);

            // Move from 0% to 100% of the lane. How far to move each frame is based on this vehicles speed and the lanes length so that it won't go faster on shorter road than on longer ones
            for (float t = 0; t <= 1; t += Time.deltaTime * (speed / laneLength)) {

                // Set the leftUntilNextNode varibale to be the lane length divided by the speed
                leftUntilNextNode = laneLength / speed;

                // Store the position to travel to, make the vehicle face it and move there
                Vector3 newVehiclePosition = p0 + t * (p1 - p0);
                transform.LookAt(newVehiclePosition);
                transform.position = newVehiclePosition;

                // Continue the loop on the next frame
                yield return new WaitForEndOfFrame();
            }

        } else if (laneType  == "Curved") {
            // If the lane this vehicle is traveling is curved:

            // Stores the transform of the lane handle if it finds it as a child of the current node
            Transform laneHandleTF = null;

            foreach (Transform child in currentNode.transform) {
                if (child.gameObject.name == "Lane Handle to " + nextNode.gameObject.name) {
                    laneHandleTF = child;
                    break;
                }
            }

            //Set p0 to the start of the lane, p1 to the handle and p2 to the end
            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 = laneHandleTF.position;
            Vector3 p2 = nextNode.transform.position;

            // Stores the position of the previous gizmo along the curve
            Vector3 lastGizmoPosition = p0;
                
            // Calculate distances between points along the curved path between p0 and p2 and add them to the laneLength
            for (float t = 0.05f; t <= 1.05f; t += 0.05f) {

                Vector3 gizmoPosition = p1 + 
                                        Mathf.Pow(1 - t, 2) * (p0 - p1) +
                                        Mathf.Pow(t, 2) * (p2 - p1);
                
                Vector3.Distance(lastGizmoPosition, gizmoPosition);
                lastGizmoPosition = gizmoPosition;
            }

            // Move from 0% to 100% of the lane. How far to move each frame is based on this vehicles speed and the lanes length so that it won't go faster on shorter road than on longer ones
            for (float t = 0; t <= 1; t += Time.deltaTime * (speed / laneLength)) {

                // Set the leftUntilNextNode varibale to be the lane length divided by the speed
                leftUntilNextNode = laneLength / speed;

                // Store the position to travel to, make the vehicle face it and move there
                Vector3 vehiclePosition = p1 + Mathf.Pow(1 - t, 2) * (p0 - p1) + Mathf.Pow(t, 2) * (p2 - p1);
                transform.LookAt(vehiclePosition);
                transform.position = vehiclePosition;

                // Continue the loop on the next frame
                yield return new WaitForEndOfFrame();
            }
        }

        // Remove this vehicle from the current nodes vehiclesOn list as we're now done traveling on it and set the current node to the next one
        currentNode.vehiclesOn.Remove(this);
        currentNode = nextNode;

        // If the vehicle has reached the end node, remove it from the scene, otherwise allow the vehicle to travel to the next node
        if (currentPathStep == pathToEndNode.Count) {
            Destroy(this.gameObject);
        } else {
            coroutineAllowed = true;
        }
    }
}
