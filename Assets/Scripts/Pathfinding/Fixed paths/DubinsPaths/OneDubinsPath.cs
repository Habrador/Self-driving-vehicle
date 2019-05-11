using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathfindingForVehicles.DubinsPaths
{
    //Will hold data related to one Dubins path so we can sort them
    public class OneDubinsPath
    {
        //The total length of this path
        public float totalLength;

        //Need the individual path length to find the waypoints
        public float[] segmentLengths = new float[3];
        

        //The 2 tangent points we may need to get the exact coordinate where a curve segment starts and ends
        public Vector3 tangent1;
        public Vector3 tangent2;

        //The type, such as RSL
        public Dubins.PathType pathType;

        //The waypoint coordinates of the final path
        public List<Vector3> waypoints;



        public OneDubinsPath(float length1, float length2, float length3, Vector3 tangent1, Vector3 tangent2, Dubins.PathType pathType)
        {
            //Calculate the total length of this path
            this.totalLength = length1 + length2 + length3;

            this.segmentLengths[0] = length1;
            this.segmentLengths[1] = length2;
            this.segmentLengths[2] = length3;

            this.tangent1 = tangent1;
            this.tangent2 = tangent2;

            this.pathType = pathType;
        }



        //Are we turning left or right or driving forward in each segment?
        public int[] GetSteeringWheelPositions()
        {
            int[] steeringWheelPos = new int[3];

            switch (pathType)
            {
                case Dubins.PathType.LRL:
                    steeringWheelPos = new int[] { -1, 1, -1 };
                    break;
                case Dubins.PathType.RLR:
                    steeringWheelPos = new int[] { 1, -1, 1 };
                    break;
                case Dubins.PathType.LSR:
                    steeringWheelPos = new int[] { -1, 0, 1 };
                    break;
                case Dubins.PathType.RSL:
                    steeringWheelPos = new int[] { 1, 0, -1 };
                    break;
                case Dubins.PathType.RSR:
                    steeringWheelPos = new int[] { 1, 0, 1 };
                    break;
                case Dubins.PathType.LSL:
                    steeringWheelPos = new int[] { -1, 0, -1 };
                    break;
            }

            return steeringWheelPos;
        }
    }
}
