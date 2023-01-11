using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour {

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

    public static List<string> getPath(Road_Node startNode, Road_Node endNode) {

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
