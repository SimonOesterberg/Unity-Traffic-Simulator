using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {
    
    [SerializeField]
    private float kph;

    private Road_Node nextNode;

    public Road_Node currentNode;

    public Road_Node endNode;
    private Road_Node startNode;

    private float speed;

    private float tParam;

    private Vector3 vehiclePosition;

    private bool coroutineAllowed;

    private List<string> pathToEndNode = new List<string>();

    private int currentPathStep = 1;

    void Start() {
        transform.position = currentNode.transform.position;
        startNode = currentNode;
        tParam = 0f;
        coroutineAllowed = true;
        speed = 0.25f * kph;

        pathToEndNode = getPathToEndNode();
    }

    void Update() {
        if (coroutineAllowed) {

            for (int i = 0; i < currentNode.connectedNodes.Count; i++) {

                Road_Node connection = currentNode.connectedNodes[i];

                if (connection.gameObject.name == pathToEndNode[currentPathStep]) {
                    nextNode = connection;
                    currentPathStep++;
                    StartCoroutine(followRoad(currentNode.roadTypes[i]));
                    break;
                }
            }
        }
    }

    private IEnumerator followRoad(string roadType) {

        coroutineAllowed = false;
        float roadLength = 0;

        if (roadType == "Straight") {

            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 =  nextNode.transform.position;

            roadLength = Mathf.Pow((Mathf.Pow(p1.x-p0.x, 2) + Mathf.Pow(p1.y-p0.y, 2) + Mathf.Pow(p1.z-p0.z, 2)), 0.5f);

            while (tParam < 1) {
                tParam += Time.deltaTime * (speed / roadLength);

                Vector3 vehiclePosition = p0 + tParam * (p1 - p0);

                transform.LookAt(vehiclePosition);
                transform.position = vehiclePosition;
                yield return new WaitForEndOfFrame();
            }

        } else if (roadType  == "Curved") {

            float roadGizmosT = 0f;
            List<Vector3> roadGizmos = new List<Vector3>();

            Transform handle = null;

            foreach (Transform child in currentNode.transform) {
                if (child.gameObject.name == "Handle-" + nextNode.gameObject.name) {
                    handle = child;
                    break;
                }
            }

            Vector3 p0 = currentNode.transform.position;
            Vector3 p1 = handle.position;
            Vector3 p2 = nextNode.transform.position;

            while (roadGizmosT < 1) {
                roadGizmosT += Time.deltaTime * speed ;

                roadGizmos.Add(p1 + 
                               Mathf.Pow(1 - roadGizmosT, 2) * (p0 - p1) +
                               Mathf.Pow(roadGizmosT, 2) * (p2 - p1)
                              );
            }

            for (int i = 0; i < roadGizmos.Count - 1; i++) {

                Vector3 g0 = roadGizmos[i];
                Vector3 g1 = roadGizmos[i + 1];

                roadLength += Mathf.Pow((Mathf.Pow(g1.x-g0.x, 2) + Mathf.Pow(g1.y-g0.y, 2) + Mathf.Pow(g1.z-g0.z, 2)), 0.5f);
            }

            while (tParam < 1) {
                tParam += Time.deltaTime * (speed / roadLength);

                vehiclePosition = p1 + 
                                  Mathf.Pow(1 - tParam, 2) * (p0 - p1) +
                                  Mathf.Pow(tParam, 2) * (p2 - p1);
                
                transform.LookAt(vehiclePosition);
                transform.position = vehiclePosition;
                yield return new WaitForEndOfFrame();
            }
        }

        tParam = 0f;

        currentNode = nextNode;

        if (currentPathStep == pathToEndNode.Count) {
            Object.Destroy(this.gameObject);
        } else {
            coroutineAllowed = true;
        }
    }

    public class LocalNode {
        public float g = 0;
        public float h = 0;
        public float f = 0;

        public LocalNode parent;
        public Road_Node node;

        public LocalNode(LocalNode parent, Road_Node node) {
            this.parent = parent;
            this.node = node;
        }
    }

    private List<string> getPathToEndNode() {

        List<string> path = new List<string>();

        LocalNode localStartNode = new LocalNode(null, startNode);
        LocalNode localEndNode   = new LocalNode(null, endNode  );
        LocalNode currentLocalNode;

        List<LocalNode> nodesToVisit = new List<LocalNode>();
        nodesToVisit.Add(localStartNode);

        List<LocalNode> visitedNodes = new List<LocalNode>();

        int loops = 0;

        while (nodesToVisit.Count > 0 && loops < 200) {

            loops++;

            currentLocalNode = nodesToVisit[0];

            for (int i = 0; i < nodesToVisit.Count; i++) {
                if (nodesToVisit[i].f < currentLocalNode.f) {
                    currentLocalNode = nodesToVisit[i];
                }
            }

            visitedNodes.Add(currentLocalNode);
            nodesToVisit.Remove(currentLocalNode);

            if (currentLocalNode.node.gameObject.name == localEndNode.node.gameObject.name) {
                
                LocalNode current = currentLocalNode;

                while (current != null) {
                    path.Add(current.node.gameObject.name);
                    current = current.parent;
                }

                path.Reverse();

                return path;
            }

            List<LocalNode> neighbours = new List<LocalNode>();

            foreach (Road_Node connectedNode in currentLocalNode.node.connectedNodes) {
                neighbours.Add(new LocalNode(currentLocalNode, connectedNode));
            }

            

            foreach (LocalNode neighbour in neighbours) {
                
                bool hasBeenVisited = false;

                foreach (LocalNode visitedNeighbour in visitedNodes) {
                    if (neighbour == visitedNeighbour) {
                        hasBeenVisited = true;
                        break;
                    }
                }

                if (!hasBeenVisited) {
                    Vector3 p0 = neighbour.node.transform.position;
                    Vector3 p1 = localEndNode.node.transform.position;

                    neighbour.g = currentLocalNode.g + 1;
                    neighbour.h = Mathf.Pow((Mathf.Pow(p1.x-p0.x, 2) + Mathf.Pow(p1.y-p0.y, 2) + Mathf.Pow(p1.z-p0.z, 2)), 0.5f);
                    neighbour.f = neighbour.g + neighbour.h;

                    foreach (LocalNode open_node in nodesToVisit) {
                        if (neighbour == open_node && neighbour.g > open_node.g) {
                            nodesToVisit.Remove(open_node);
                        }
                    }

                    nodesToVisit.Add(neighbour);
                }
            }
        }
        return path;
    }
}
