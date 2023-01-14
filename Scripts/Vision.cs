using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour {
    private VehicleController thisVehicle;
    private float initialKph;

    private List<Collider> collidersInSight = new List<Collider>();

    public int visionRange = 20;

    private List<Ray> visionRays = new List<Ray>();

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

        visionRays.Clear();

        visionRays.Add(new Ray(transform.position, Quaternion.Euler(0, 5.0f, 0) * transform.forward));
        visionRays.Add(new Ray(transform.position, transform.forward));
        visionRays.Add(new Ray(transform.position, Quaternion.Euler(0, -5.0f, 0) * transform.forward));

        visionRays.Add(new Ray(transform.position, -transform.right));
        visionRays.Add(new Ray(transform.position, transform.right));
        
        visionRays.Add(new Ray(transform.position, Quaternion.Euler(0, 5.0f, 0) * -transform.forward));
        visionRays.Add(new Ray(transform.position, -transform.forward));
        visionRays.Add(new Ray(transform.position, Quaternion.Euler(0, -5.0f, 0) * -transform.forward));

        RaycastHit hit;

        foreach (Ray visionRay in visionRays) {
            if (Physics.Raycast(visionRay, out hit, visionRange)) {    
                collidersInSight.Add(hit.collider);
            }
        }

        
    }

    void Update() {
        /* Debug.DrawRay(transform.position, -transform.forward * visionRange, Color.red);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 5.0f, 0) * -transform.forward * visionRange, Color.yellow);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, -5.0f, 0) * -transform.forward * visionRange, Color.blue); */
    }
}
