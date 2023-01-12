using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour {
    private Vehicle thisVehicle;
    private float initialKph;

    private List<Collider> collidersInSight = new List<Collider>();

    public int visionRange = 1000;

    public bool isSeeing(Collider target) {
        lookAround();

        foreach (Collider collider in collidersInSight) {

            if (collider == target) {
                return true;
            }
        }

        return false;
    }

    void lookAround() {
        collidersInSight.Clear();

        Ray rayForward = new Ray(transform.position, transform.forward);
        Ray rayForwardL = new Ray(transform.position, Quaternion.Euler(0, 5.0f, 0) * transform.forward);
        Ray rayForwardR = new Ray(transform.position, Quaternion.Euler(0, -5.0f, 0) * transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(rayForward, out hit, visionRange) || Physics.Raycast(rayForwardL, out hit, visionRange) || Physics.Raycast(rayForwardR, out hit, visionRange) ) {    
            collidersInSight.Add(hit.collider);
        }
    }

    void Update() {
        Debug.DrawRay(transform.position, transform.forward * visionRange, Color.red);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 5.0f, 0) * transform.forward * visionRange, Color.yellow);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, -5.0f, 0) * transform.forward * visionRange, Color.blue);
    }
}
