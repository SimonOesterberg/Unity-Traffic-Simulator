using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private List<Vehicle> vehicles;
    [SerializeField] private List<Road_Node> targets;

    private int vehicleNumberToSpawn = 0;


    void Start() {
         InvokeRepeating("spawnVehicle", 0, 1.0f);
    }

    void spawnVehicle() {
        if (vehicleNumberToSpawn < vehicles.Count) {

            vehicles[vehicleNumberToSpawn].startNode = (Road_Node)transform.parent.gameObject.GetComponent("Road_Node");
            vehicles[vehicleNumberToSpawn].endNode = targets[vehicleNumberToSpawn];
            
            Instantiate(vehicles[vehicleNumberToSpawn]);

            vehicleNumberToSpawn++;
        
        }
    }
}