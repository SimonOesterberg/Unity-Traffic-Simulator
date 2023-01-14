using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    // Lists of which vehicles to spawn and where they should be heading
    [SerializeField] private List<VehicleController> vehicles;
    [SerializeField] private List<LaneNode> targets;



    // stores which vehicle to spawn currently
    private int vehicleNumberToSpawn = 0;



    void Start() {
        // Spawn the vehicles in the vehicles list every second
        InvokeRepeating("spawnVehicle", 0, 1.0f);
    }

    void spawnVehicle() {
        if (vehicleNumberToSpawn < vehicles.Count) {
            // If there's still vehicles to spawn:

            // Set the new vehicles start node to the parent of the spawner and the target to the corresponding target in the targets list and instantiate the vehicle
            vehicles[vehicleNumberToSpawn].startNode = (LaneNode)transform.parent.gameObject.GetComponent("LaneNode");
            vehicles[vehicleNumberToSpawn].endNode = targets[vehicleNumberToSpawn];
            Instantiate(vehicles[vehicleNumberToSpawn]);

            // Set the next vehicle in the vehicle list to be spawned next
            vehicleNumberToSpawn++;
        } else {
            // If no vehicles left to spawn, stop the repeater
            CancelInvoke();
        }
    }
}