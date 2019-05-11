using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    public class FlowFieldNode
    {
        //If this cell is not an obstacle
        public bool isWalkable;
        //The position this cell has in world space
        public Vector3 worldPos;
        //The position this cell has in cell space
        public IntVector2 cellPos;
        //The node to get to this node
        public FlowFieldNode parent;
        //Is this node in the open set (used in pathfidning)
        public bool isInOpenSet;
        //All neighbors to this node
        public HashSet<FlowFieldNode> neighborNodes;
        //When flow field, the cost should be an int
        //This is the total cost to this node
        public float totalCostFlowField;
        //This is the movement cost from another node to this node
        //Is not the same as the distance between the nodes but a penalty cost (if needed)
        public float movementCostFlowField;
        //Get the direction once between the nodes to make it easier for a lot of ai units to follow it
        public Vector3 flowDirection;
        //To which region does this cell node belong?
        //Is useful if we are finding distance to obstacle and want to know to which obstacle we are flowing
        public int region = -1;
        //Which of all start cells is the closest to this cell - sometimes multiple nodes are equally close
        //This is useful if we want to find the closest obstacle
        public HashSet<IntVector2> closestStartNodes;



        public FlowFieldNode(bool isWalkable, Vector3 worldPos, IntVector2 cellPos)
        {
            this.isWalkable = isWalkable;

            this.worldPos = worldPos;

            this.cellPos = cellPos;

            closestStartNodes = new HashSet<IntVector2>();
        }



        public void Reset()
        {
            parent = null;

            isInOpenSet = false;

            //Reset cost of movement (total cost) to this node to something large
            totalCostFlowField = float.MaxValue;

            //The cost to move to this node
            movementCostFlowField = 0f;            
        }



        //Add a neighbor to this node
        public void AddNeighbor(FlowFieldNode neighbor)
        {
            if (neighborNodes == null)
            {
                neighborNodes = new HashSet<FlowFieldNode>();
            }

            neighborNodes.Add(neighbor);
        }
    }
}
