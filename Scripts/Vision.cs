using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour {

    // How far the vehicle shuld be able to see
    public int visionRange = 20;



    // A list that will contain all colliders that the vehicle with this Vision script can see
    private List<Collider> collidersInSight = new List<Collider>();

    // A list that will contain all rays that will be sent out when looking
    private List<Ray> visionRays = new List<Ray>();



    // Function that returns true if the passes collider: target can be seen
    public bool isSeeing(Collider target) {

        // Send rays out and update collidersInSight
        lookAround();

        // If the target collider can be seen return true
        foreach (Collider collider in collidersInSight) {

            if (collider == target) {
                return true;
            }
        }

        // If the target collider cannot be seen return false
        return false;
    }

    // Function to send out the rays and update the collidersInSight list
    void lookAround() {

        // Clear the collidersInSight list and visionRays list
        collidersInSight.Clear();
        visionRays.Clear();

        // Add all rays that should be sent out by this vehicle to the visionRays list
        /* Forward Left: */  visionRays.Add(new Ray(transform.position, Quaternion.Euler(0, 5.0f, 0) * transform.forward));
        /* Forward: */       visionRays.Add(new Ray(transform.position, transform.forward));
        /* Forward Right: */ visionRays.Add(new Ray(transform.position, Quaternion.Euler(0, -5.0f, 0) * transform.forward));

        /* Left: */          visionRays.Add(new Ray(transform.position, -transform.right));
        /* Right: */         visionRays.Add(new Ray(transform.position, transform.right));
        
        /* Back Left: */     visionRays.Add(new Ray(transform.position, Quaternion.Euler(0, 5.0f, 0) * -transform.forward));
        /* Back: */          visionRays.Add(new Ray(transform.position, -transform.forward));
        /* Back Right: */    visionRays.Add(new Ray(transform.position, Quaternion.Euler(0, -5.0f, 0) * -transform.forward));

        // Stores the eventual hit of a ray
        RaycastHit hit;

        // If a ray hits a collider add that collider to the collidersInSight list
        foreach (Ray visionRay in visionRays) {
            if (Physics.Raycast(visionRay, out hit, visionRange)) {    
                collidersInSight.Add(hit.collider);
            }
        }

        
    }
}
