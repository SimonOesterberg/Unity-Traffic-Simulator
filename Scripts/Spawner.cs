using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private List<Vehicle> vehicles;


    void Start()
    {
        foreach (Vehicle vehicle in vehicles) {
            vehicle.currentNode = (Road_Node)transform.parent.gameObject.GetComponent("Road_Node");
            Instantiate(vehicle);
        }
    }
}
