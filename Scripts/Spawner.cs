using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private List<Vehicle> vehicles;

    private int vehicleNumberToSpawn = 0;


    void Start() {
         InvokeRepeating("spawnVehicle", 0, 1.0f);
    }

    void spawnVehicle() {
        if (vehicleNumberToSpawn < vehicles.Count) {

            vehicles[vehicleNumberToSpawn].currentNode = (Road_Node)transform.parent.gameObject.GetComponent("Road_Node");
            
            //Instantiate(vehicles[vehicleNumberToSpawn]);

            vehicleNumberToSpawn++;
        
        }
    }
}