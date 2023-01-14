using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private List<VehicleController> vehicles;
    [SerializeField] private List<LaneNode> targets;

    private int vehicleNumberToSpawn = 0;


    void Start() {
         InvokeRepeating("spawnVehicle", 0, 1.0f);
    }

    void spawnVehicle() {
        if (vehicleNumberToSpawn < vehicles.Count) {

            vehicles[vehicleNumberToSpawn].startNode = (LaneNode)transform.parent.gameObject.GetComponent("LaneNode");
            vehicles[vehicleNumberToSpawn].endNode = targets[vehicleNumberToSpawn];
            
            Instantiate(vehicles[vehicleNumberToSpawn]);

            vehicleNumberToSpawn++;
        
        }
    }
}