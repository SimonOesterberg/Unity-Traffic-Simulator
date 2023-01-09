using System;
using UnityEngine;

public class Curved_Road : MonoBehaviour {

    [SerializeField]
    private Transform startNode;

    [SerializeField]
    private Transform handle;

    [SerializeField]
    private Transform endNode;

    private Vector3 gizmosPosition;

    private Vector3 lastGizmosPosition;

    private void OnDrawGizmos() {

        for (float t = 0; t <= 1; t += 0.05f) {

            gizmosPosition = handle.position + 
                             Mathf.Pow(1 - t, 2) * (startNode.position - handle.position) +
                             Mathf.Pow(t, 2) * (endNode.position - handle.position);
            
            if (t != 0f) {
                Gizmos.DrawLine(lastGizmosPosition, gizmosPosition);
            }
            

            

            lastGizmosPosition = gizmosPosition;
        }
    }
}

