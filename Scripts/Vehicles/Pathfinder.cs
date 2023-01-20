using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour {

    // Class used to store the nodes and their g, h and f values and which node led to that node
    private class PathNode {
        public float g = 0;
        public float h = 0;
        public float f = 0;

        public PathNode parent;
        public LaneNode laneNode;

        public PathNode(PathNode parent, LaneNode laneNode) {
            this.parent = parent;
            this.laneNode = laneNode;
        }
    }

    // Function that returns the names of each lane node that should be travelled to reach a vehicles end node
    public static List<int> getPath(LaneNode startNode, LaneNode endNode) {

        // The list that should be returned by the function, starts empty
        List<int> path = new List<int>();

        // Create a path node of the start and end lane node
        PathNode pathStartNode = new PathNode(null, startNode);
        PathNode pathEndNode   = new PathNode(null, endNode  );
        PathNode currentPathNode;

        // Lists to store nodes still to visit and nodes that have been visited
        List<PathNode> pathNodesToVisit = new List<PathNode>();
        List<PathNode> visitedPathNodes = new List<PathNode>();

        // Add the start node of the path to the list of path nodes to visit
        pathNodesToVisit.Add(pathStartNode);

        while (pathNodesToVisit.Count > 0) {
            // While there are still nodes to visit and the end node has not been found:

            // Set the current path node to the first node in the list if paths to visit
            currentPathNode = pathNodesToVisit[0];

            // If any of the other nodes in the list of nodes to visit have a better f value, set that to the current path node
            for (int i = 1; i < pathNodesToVisit.Count; i++) {
                if (pathNodesToVisit[i].f < currentPathNode.f) {
                    currentPathNode = pathNodesToVisit[i];
                }
            }

            // Add the current path nodes to the visited list and remove it from the list of path nodes to visit
            visitedPathNodes.Add(currentPathNode);
            pathNodesToVisit.Remove(currentPathNode);

            if (currentPathNode.laneNode.GetInstanceID() == pathEndNode.laneNode.GetInstanceID()) {
                // If the current path node has the same name as the end node:

                // Add all path nodes on the way to the end node to the path list
                while (currentPathNode != null) {
                    path.Add(currentPathNode.laneNode.GetInstanceID());
                    currentPathNode = currentPathNode.parent;
                }

                // Reverse and return it
                path.Reverse();
                return path;
            }

            // List of connected path nodes to the current path node
            List<PathNode> connectedPathNodes = new List<PathNode>();

            // Add all of the connected nodes to the connected path nodes list
            foreach (LaneNode connectedNode in currentPathNode.laneNode.connectedLaneNodes) {
                connectedPathNodes.Add(new PathNode(currentPathNode, connectedNode));
            }

            foreach (PathNode connectedPathNode in connectedPathNodes) {
                // For every connected path node:

                // Boolean to store wether a path node has already been visited before or not
                bool hasBeenVisited = false;

                // If the connected node is in the list of visited path nodes set hasBeenVisited to true
                foreach (PathNode visitedPathNode in visitedPathNodes) {
                    if (connectedPathNode == visitedPathNode) {
                        hasBeenVisited = true;
                        break;
                    }
                }

                if (!hasBeenVisited) {
                    // If the connected path node has not been visited before:

                    // Store the position of the current path nodes laneNode and end laneNode as p0 and p1
                    Vector3 p0 = connectedPathNode.laneNode.transform.position;
                    Vector3 p1 = endNode.transform.position;

                    // Set the g value to the steps from the start node, h to the distance to the end node and f to the sum of g and h
                    connectedPathNode.g = currentPathNode.g + 1;
                    connectedPathNode.h = Vector3.Distance(p0, p1);
                    connectedPathNode.f = connectedPathNode.g + connectedPathNode.h;

                    // If the connected path node is in the list of path nodes to visit and this is a closer way to it, remove the other way to it
                    foreach (PathNode pathNodeToVisit in pathNodesToVisit) {
                        if (connectedPathNode == pathNodeToVisit && connectedPathNode.g > pathNodeToVisit.g) {
                            pathNodesToVisit.Remove(pathNodeToVisit);
                        }
                    }

                    // Add the connected path node to the list of path nodes left to visit
                    pathNodesToVisit.Add(connectedPathNode);
                }
            }
        }

        // If the end was not found, return the empty path
        return path;
    }
}
