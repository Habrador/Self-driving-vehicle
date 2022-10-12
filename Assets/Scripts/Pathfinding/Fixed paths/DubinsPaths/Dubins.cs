using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathfindingForVehicles.DubinsPaths
{
    //Generates Dubins paths
    public static class Dubins
    {
        //To keep track of the different paths when debugging
        public enum PathType { RSR, LSL, RSL, LSR, RLR, LRL }

        //The 4 different circles we have that sits to the left/right of the start/goal
        private static Vector3 startLeftCircle;
        private static Vector3 startRightCircle;
        private static Vector3 goalLeftCircle;
        private static Vector3 goalRightCircle;

        //To generate paths we need the position and rotation (heading) of the cars
        private static Car startCar;
        private static Car goalCar;

        //Parameters
        //How far we are driving each update, the accuracy will improve if we lower the driveDistance
        //But not too low because rounding errors will appear and 
        //If step is 0.1, the the path will not end at the end waypoint, so we will get a error
        //Is used to generate the coordinates of a path
        private static float driveStepDistance = 0.05f;
        //The distance between each waypoint, which should be larger than driveStepDistance
        private static float distanceBetweenWaypoints = 1f;



        //Get the shortest path
        public static OneDubinsPath GetShortestPath(Vector3 startPos, float startHeading, Vector3 goalPos, float goalHeading, float turningRadius)
        {
            startCar = new Car(startPos, startHeading, turningRadius);
            goalCar = new Car(goalPos, goalHeading, turningRadius);

           
            List<OneDubinsPath> allPaths = new List<OneDubinsPath>();

            //Position the circles that are to the left/right of the cars
            PositionLeftRightCircles();

            //Find the length of each path with tangent coordinates
            CalculateDubinsPathsLengths(allPaths);

            //If we have paths
            if (allPaths.Count > 0)
            {
                //Sort the list with paths so the shortest path is first
                allPaths.Sort((x, y) => x.totalLength.CompareTo(y.totalLength));

                //Get the shortest path
                OneDubinsPath shortestPath = allPaths[0];

                //Generate the final coordinates of the path from tangent points and segment lengths
                GeneratePathWaypoints(shortestPath);

                return shortestPath;
            }

            //No paths could be found
            return null;
        }



        //Get all valid Dubins paths sorted from shortest to longest
        public static List<OneDubinsPath> GetAllPaths(Vector3 startPos, float startHeading, Vector3 goalPos, float goalHeading, float turningRadius)
        {
            startCar = new Car(startPos, startHeading, turningRadius);
            goalCar = new Car(goalPos, goalHeading, turningRadius);


            List<OneDubinsPath> allPaths = new List<OneDubinsPath>();

            //Position the circles that are to the left/right of the cars
            PositionLeftRightCircles();

            //Find the length of each path with tangent coordinates
            CalculateDubinsPathsLengths(allPaths);

            //If we have paths
            if (allPaths.Count > 0)
            {
                //Sort the list with paths so the shortest path is first
                allPaths.Sort((x, y) => x.totalLength.CompareTo(y.totalLength));

                //Generate the final coordinates of the path from tangent points and segment lengths
                for (int i = 0; i < allPaths.Count; i++)
                {
                    GeneratePathWaypoints(allPaths[i]);
                }

                return allPaths;
            }

            //No paths could be found
            return null;
        }


        //Position the left and right circles that are to the left/right of the target and the car
        private static void PositionLeftRightCircles()
        {
            //Goal pos
            goalRightCircle = DubinsMath.GetRightCircleCenterPos(goalCar);

            goalLeftCircle = DubinsMath.GetLeftCircleCenterPos(goalCar);


            //Start pos
            startRightCircle = DubinsMath.GetRightCircleCenterPos(startCar);

            startLeftCircle = DubinsMath.GetLeftCircleCenterPos(startCar);
        }



        //
        //Calculate the path lengths of all Dubins paths by using tangent points
        //
        private static void CalculateDubinsPathsLengths(List<OneDubinsPath> allPaths)
        {
            //RSR and LSL is only working if the circles don't have the same position
            
            //RSR
            if (startRightCircle != goalRightCircle)
            {
                allPaths.Add(Get_RSR_Path());
            }
            
            //LSL
            if (startLeftCircle != goalLeftCircle)
            {
                allPaths.Add(Get_LSL_Path());
            }


            //RSL and LSR is only working of the circles don't intersect
            float comparisonSqr = startCar.turningRadius * 2f * startCar.turningRadius * 2f;

            //RSL
            if ((startRightCircle - goalLeftCircle).sqrMagnitude > comparisonSqr)
            {
                allPaths.Add(Get_RSL_Path());
            }

            //LSR
            if ((startLeftCircle - goalRightCircle).sqrMagnitude > comparisonSqr)
            {
                allPaths.Add(Get_LSR_Path());
            }


            //With the LRL and RLR paths, the distance between the circles have to be less than 4 * r
            comparisonSqr = 4f * startCar.turningRadius * 4f * startCar.turningRadius;

            //RLR        
            if ((startRightCircle - goalRightCircle).sqrMagnitude < comparisonSqr)
            {
                allPaths.Add(Get_RLR_Path());
            }

            //LRL
            if ((startLeftCircle - goalLeftCircle).sqrMagnitude < comparisonSqr)
            {
                allPaths.Add(Get_LRL_Path());
            }
        }



        //
        // The possible paths
        //

        //RSR
        private static OneDubinsPath Get_RSR_Path()
        {
            //Find both tangent positons
            Vector3 startTangent = Vector3.zero;
            Vector3 goalTangent = Vector3.zero;

            float turningRadius = startCar.turningRadius;

            DubinsMath.LSLorRSR(startRightCircle, goalRightCircle, false, out startTangent, out goalTangent, turningRadius);

            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startRightCircle, startCar.pos, startTangent, false, turningRadius);

            float length2 = (startTangent - goalTangent).magnitude;

            float length3 = DubinsMath.GetArcLength(goalRightCircle, goalTangent, goalCar.pos, false, turningRadius);

            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.RSR);


            return pathData;
        }


        //LSL
        private static OneDubinsPath Get_LSL_Path()
        {
            //Find both tangent positions
            Vector3 startTangent = Vector3.zero;
            Vector3 goalTangent = Vector3.zero;

            float turningRadius = startCar.turningRadius;

            DubinsMath.LSLorRSR(startLeftCircle, goalLeftCircle, true, out startTangent, out goalTangent, turningRadius);

            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startLeftCircle, startCar.pos, startTangent, true, turningRadius);

            float length2 = (startTangent - goalTangent).magnitude;

            float length3 = DubinsMath.GetArcLength(goalLeftCircle, goalTangent, goalCar.pos, true, turningRadius);

            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.LSL);


            return pathData;
        }


        //RSL
        private static OneDubinsPath Get_RSL_Path()
        {
            //Find both tangent positions
            Vector3 startTangent = Vector3.zero;
            Vector3 goalTangent = Vector3.zero;

            float turningRadius = startCar.turningRadius;

            DubinsMath.RSLorLSR(startRightCircle, goalLeftCircle, false, out startTangent, out goalTangent, turningRadius);

            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startRightCircle, startCar.pos, startTangent, false, turningRadius);

            float length2 = (startTangent - goalTangent).magnitude;

            float length3 = DubinsMath.GetArcLength(goalLeftCircle, goalTangent, goalCar.pos, true, turningRadius);

            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.RSL);


            return pathData;
        }


        //LSR
        private static OneDubinsPath Get_LSR_Path()
        {
            //Find both tangent positions
            Vector3 startTangent = Vector3.zero;
            Vector3 goalTangent = Vector3.zero;

            float turningRadius = startCar.turningRadius;

            DubinsMath.RSLorLSR(startLeftCircle, goalRightCircle, true, out startTangent, out goalTangent, turningRadius);

            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startLeftCircle, startCar.pos, startTangent, true, turningRadius);

            float length2 = (startTangent - goalTangent).magnitude;

            float length3 = DubinsMath.GetArcLength(goalRightCircle, goalTangent, goalCar.pos, false, turningRadius);

            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.LSR);


            return pathData;
        }


        //RLR
        private static OneDubinsPath Get_RLR_Path()
        {
            //Find both tangent positions and the position of the 3rd circle
            Vector3 startTangent = Vector3.zero;
            Vector3 goalTangent = Vector3.zero;
            //Center of the 3rd circle
            Vector3 middleCircle = Vector3.zero;

            float turningRadius = startCar.turningRadius;

            DubinsMath.GetRLRorLRLTangents(
                startRightCircle,
                goalRightCircle,
                false,
                out startTangent,
                out goalTangent,
                out middleCircle, 
                turningRadius);

            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startRightCircle, startCar.pos, startTangent, false, turningRadius);

            float length2 = DubinsMath.GetArcLength(middleCircle, startTangent, goalTangent, true, turningRadius);

            float length3 = DubinsMath.GetArcLength(goalRightCircle, goalTangent, goalCar.pos, false, turningRadius);

            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.RLR);


            return pathData;
        }


        //LRL
        private static OneDubinsPath Get_LRL_Path()
        {
            //Find both tangent positions and the position of the 3rd circle
            Vector3 startTangent = Vector3.zero;
            Vector3 goalTangent = Vector3.zero;
            //Center of the 3rd circle
            Vector3 middleCircle = Vector3.zero;

            float turningRadius = startCar.turningRadius;

            DubinsMath.GetRLRorLRLTangents(
                startLeftCircle,
                goalLeftCircle,
                true,
                out startTangent,
                out goalTangent,
                out middleCircle, 
                turningRadius);

            //Calculate the total length of this path
            float length1 = DubinsMath.GetArcLength(startLeftCircle, startCar.pos, startTangent, true, turningRadius);

            float length2 = DubinsMath.GetArcLength(middleCircle, startTangent, goalTangent, false, turningRadius);

            float length3 = DubinsMath.GetArcLength(goalLeftCircle, goalTangent, goalCar.pos, true, turningRadius);

            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.LRL);


            return pathData;
        }



        //
        // Generate the final waypoints we need for navigation
        //
        private static void GeneratePathWaypoints(OneDubinsPath pathData)
        {
            //Store the waypoints of the final path here
            List<Vector3> waypoints = new List<Vector3>();

            //The car that will be simulated along the path
            Car car = startCar;

            //We always have to add the first position manually = the position of the car
            waypoints.Add(car.pos);

            //To be able to get evenly spaced waypoints
            //Will be reset when we reach a waypoint
            float distanceTravelled = 0f;

            //The steering wheel positions of each segment
            //Tturning left (-1), right (1) or driving forward (0) 
            int[] steeringWheelPos = pathData.GetSteeringWheelPositions();

            //Each path consists of 3 segments
            for (int i = 0; i < pathData.segmentLengths.Length; i++)
            {
                AddCoordinatesToSegment(
                    ref car,
                    waypoints,
                    pathData.segmentLengths[i],
                    steeringWheelPos[i],
                    driveStepDistance,
                    distanceBetweenWaypoints,
                    ref distanceTravelled);
            }

            //Make sure the goal endpoint is at the goal position
            waypoints[waypoints.Count - 1] = goalCar.pos;

            //Save the final path in the path data
            pathData.waypoints = waypoints;
        }



        //Simulate a car along the length of a path and add waypoints after some distance travelled
        private static void AddCoordinatesToSegment(
            ref Car car,
            List<Vector3> waypoints,
            float pathLength,
            int steeringWheelPos,
            float driveStepDistance,
            float distanceBetweenWaypoints,
            ref float distanceTravelled)
        {
            //How many driving steps can we fit into this part of the path
            int segments = Mathf.FloorToInt(pathLength / driveStepDistance);

            for (int i = 0; i < segments; i++)
            {
                //Update the position of the car
                car.pos.x += driveStepDistance * Mathf.Sin(car.heading);
                car.pos.z += driveStepDistance * Mathf.Cos(car.heading);

                //Don't update the heading if we are driving straight
                if (steeringWheelPos == 1 || steeringWheelPos == -1)
                {
                    //Update the heading
                    car.heading += (driveStepDistance / car.turningRadius) * steeringWheelPos;
                }

                distanceTravelled += driveStepDistance;

                //Don't add waypoints after each step because too detailed
                if (distanceTravelled > distanceBetweenWaypoints)
                {
                    //Add the new coordinate to the path
                    waypoints.Add(car.pos);

                    distanceTravelled = 0f;
                }
            }
        }
    }
}
