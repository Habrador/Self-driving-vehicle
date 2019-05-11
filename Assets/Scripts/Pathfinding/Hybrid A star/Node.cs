using UnityEngine;
using System.Collections;
using System;

namespace PathfindingForVehicles
{
    //One node in the A star algorithm
    public class Node : IHeapItem<Node>
    {
        //The cost to this node
        public float gCost;
        //The estimated cost to the goal from this node = the heuristics
        //Needed so we can calculate the f cost easier
        public float hCost;

        //The data we need for the car
        //The position the car's rear axle should have when following the path
        public Vector3 rearWheelPos;
        //The position the car's front axle should have when following the path
        public Vector3 frontWheelPos;
        //A position we use when reversing
        public Vector3 reverseWheelPos;
        //The heading in radians
        public float heading;
        //Is the car reversing when traveling to this node?
        public bool isReversing;

        //Trailer heading in radians
        private float trailerHeading;

        //The node we took to get here so we can get the final path
        public Node previousNode;

        //The index this node has in the heap, to make sorting nodes faster
        private int heapIndex;


        public Node()
        {

        }


        public Node(Node previousNode, Vector3 rearWheelPos, float heading, bool isReversing)
        {
            this.previousNode = previousNode;
            this.rearWheelPos = rearWheelPos;
            this.heading = heading;
            this.isReversing = isReversing;
        }


        //Cant be done in constructor because we need data from this node to calculate the costs
        public void AddCosts(float gCost, float hCost)
        {
            this.gCost = gCost;
            this.hCost = hCost;
        }



        //The total cost including heuristic (f = g + h)
        public float fCost
        {
            get { return gCost + hCost; }
        }



        //Headings
        public float HeadingInRadians
        {
            get { return this.heading; }
        }

        public float HeadingInDegrees
        {
            get { return this.heading * Mathf.Rad2Deg; }
        }

        public float TrailerHeadingInRadians
        {
            get { return this.trailerHeading; }
            set { this.trailerHeading = value; }
        }

        public float TrailerHeadingInDegrees
        {
            get { return this.trailerHeading * Mathf.Rad2Deg; }
        }



        //Take the data from this node and add it to another node
        public void StealDataFromThisNode(Node other)
        {
            other.gCost = gCost;
            other.hCost = hCost;
            other.rearWheelPos = rearWheelPos;
            other.frontWheelPos = frontWheelPos;
            other.heading = heading;
            other.isReversing = isReversing;
            other.previousNode = previousNode;
            other.trailerHeading = trailerHeading;
        }


        //The heap requires that we implement this
        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }
            set
            {
                heapIndex = value;
            }
        }



        //To compare nodes when sorting the heap
        public int CompareTo(Node nodeToCompare)
        {
            int compare = fCost.CompareTo(nodeToCompare.fCost);

            //If they are equal, use the one that is the closest
            //Will return 1, 0 or -1, so 0 means the f costs are the same
            if (compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }

            return -compare;
        }
    }
}
