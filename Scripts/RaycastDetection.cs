using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDetection : MonoBehaviour {

    private Vehicle thisVehicle;
    private float initialKph;

    void Start() {
        thisVehicle = (Vehicle)transform.gameObject.GetComponent("Vehicle");
        initialKph = thisVehicle.targetKph;
    }

    // See Order of Execution for Event Functions for information on FixedUpdate() and Update() related to physics queries
    void FixedUpdate()
    {
        Ray rayForward = new Ray(transform.position, transform.forward);
        Ray rayForwardL = new Ray(transform.position, Quaternion.Euler(0, 5.0f, 0) * transform.forward);
        Ray rayForwardR = new Ray(transform.position, Quaternion.Euler(0, -5.0f, 0) * transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(rayForward, out hit, 10) || Physics.Raycast(rayForwardL, out hit, 10) || Physics.Raycast(rayForwardR, out hit, 10) ) {

            if (hit.collider.tag == "Vehicle") {

                if (hit.distance < 3) {
                    Vehicle hitVehicle = (Vehicle)hit.collider.gameObject.GetComponent("Vehicle");
                    thisVehicle.targetKph = hitVehicle.targetKph;
                } else {
                    thisVehicle.targetKph = initialKph;
                }
            }
        } else {
            thisVehicle.targetKph = initialKph;
        }
    }

    void Update() {
        Debug.DrawRay(transform.position, transform.forward * 10, Color.red);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 5.0f, 0) * transform.forward * 10, Color.yellow);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, -5.0f, 0) * transform.forward * 10, Color.blue);
    }
}
