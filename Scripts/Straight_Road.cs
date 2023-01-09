using System;
using UnityEngine;

public class Straight_Road : MonoBehaviour {

    [SerializeField]
    private Transform startNode;

    [SerializeField]
    private Transform endNode;

    private Vector3 gizmosPosition;

    private void OnDrawGizmos() {
        Gizmos.DrawLine(new Vector3(startNode.position.x, startNode.position.y, startNode.position.z),
                        new Vector3(endNode.position.x, endNode.position.y, endNode.position.z));
    }
}