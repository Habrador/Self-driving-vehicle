using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles.ReedsSheppPaths
{
    //Generates Reeds-Shepp paths
    //Some code from https://github.com/mattbradley/AutonomousCar/tree/master/AutonomousCar/AutonomousCar/PathFinding/ReedsShepp
    public static class ReedsShepp
    {
        /// <summary>
        /// Generate the shortest Reeds-Shepp path
        /// </summary>
        /// <param name="startPos">The position of the car where the path starts</param>
        /// <param name="startHeading">The heading of the car where the path starts [rad]</param>
        /// <param name="endPos">The position of the car where the path ends</param>
        /// <param name="endHeading">The heading of the car where the path ends [rad]</param>
        /// <param name="turningRadius">The truning radius of the car [m]</param>
        /// <param name="wpDistance">The distance between the waypoints that make up the final path</param>
        /// <param name="generateOneWp">Should we generate just 1 waypoint and not all</param>
        /// <returns></returns>
        public static List<RSCar> GetShortestPath(Vector3 startPos, float startHeading, Vector3 endPos, float endHeading, float turningRadius, float wpDistance, bool generateOneWp)
        {
            RSCar carStart = new RSCar(startPos, startHeading);
            RSCar carEnd = new RSCar(endPos, endHeading);

            //The formulas assume we move from(0, 0, 0) to(x, y, theta), and that the turning radius is 1
            //This means we have to move and rotate the goal's position and heading as if the start's position and heading had been at (0,0,0)
            RSCar carEndMod = NormalizeGoalCar(carStart, carEnd, turningRadius);


            //int startTime = Environment.TickCount;

            List<RSCar> shortestPath = FindShortestPath(carStart, carEnd, carEndMod, wpDistance, turningRadius, generateOneWp);
            
            //float timeInSeconds = (float)(Environment.TickCount - startTime) / 1000f;

            //Debug.Log("Ticks to generate <b>Reeds-Shepp:</b> " + (Environment.TickCount - startTime));

            return shortestPath;
        }



        //Same as above but we just want the shortest distance
        public static float GetShortestDistance(Vector3 startPos, float startHeading, Vector3 endPos, float endHeading, float turningRadius)
        {
            RSCar carStart = new RSCar(startPos, startHeading);
            RSCar carEnd = new RSCar(endPos, endHeading);

            //The formulas assume we move from(0, 0, 0) to(x, y, theta), and that the turning radius is 1
            //This means we have to move and rotate the goal's position and heading as if the start's position and heading had been at (0,0,0)
            RSCar carEndMod = NormalizeGoalCar(carStart, carEnd, turningRadius);

            PathSegmentLengths bestPathLengths;
            PathWords bestWord;

            float shortestPathLength = FindShortestPathLength(carEndMod, out bestPathLengths, out bestWord);

            //No path could be found
            if (float.IsPositiveInfinity(shortestPathLength))
            {
                Debug.Log("Cant find a Reeds-Shepp path");

                return shortestPathLength;
            }

            //Convert back to the actual diistance by using the turning radius
            shortestPathLength *= turningRadius;

            return shortestPathLength;
        }



        //Loop through all paths and find the shortest one (if one can be found)
        private static float FindShortestPathLength(RSCar carEndMod, out PathSegmentLengths bestPathLengths, out PathWords bestWord)
        {
            //How many paths are we going to check
            int numPathWords = Enum.GetNames(typeof(PathWords)).Length;

            //Better than using float.MaxValue because we can use float.IsPositiveInfinity(bestPathLength) to test if its infinity
            float shortestPathLength = float.PositiveInfinity;

            bestWord = 0;

            //Will keep track of the length of the best path
            //Some Reeds-Shepp segments have 5 lengths, but 2 of those are known, so we only need 3 to find the shortest path
            bestPathLengths = new PathSegmentLengths(0f, 0f, 0f);

            //Loop through all paths that are enums to find the shortest
            for (int w = 0; w < numPathWords; w++)
            {
                PathWords word = (PathWords)w;

                PathSegmentLengths pathSegmentLengths;

                float pathLength = PathLengthMath.GetLength(carEndMod, word, out pathSegmentLengths);

                if (pathLength < shortestPathLength)
                {
                    shortestPathLength = pathLength;
                    bestWord = word;
                    bestPathLengths = pathSegmentLengths;
                }
            }

            return shortestPathLength;
        }



        //The formulas assume we move from(0, 0, 0) to (x, y, theta), and that the turning radius is 1
        //This means we have to move and rotate the goal's position and heading as if the start's position and heading had been at (0,0,0)
        //But we are using Unity, so the rotation of the start car has to be along the x-axis and not z-axis which is Unity's zero-rotation
        public static RSCar NormalizeGoalCar(RSCar carStart, RSCar carEnd, float turningRadius)
        {
            //Change the position and rotation of the goal car
            Vector3 posDiff = carEnd.pos - carStart.pos;

            //Turning radius is 1
            posDiff /= turningRadius;

            //Unitys coordinate is not the same as the one used in the pdf so we have to make som strange translations
            float headingDiff = PathLengthMath.WrapAngleInRadians((2f * Mathf.PI) - (carEnd.HeadingInRad - carStart.HeadingInRad));

            //Rotate the vector between the cars
            //Add 90 degrees because of unitys coordinate system
            Vector3 newEndPos = Quaternion.Euler(0f, -carStart.HeadingInDegrees + 90f, 0f) * posDiff;

            RSCar carEndMod = new RSCar(newEndPos, headingDiff);

            return carEndMod;
        }



        //Find the shortest Reeds-Shepp path and generate waypoints to follow that path
        private static List<RSCar> FindShortestPath(
            RSCar carStart, RSCar carEnd, RSCar carEndMod, float wpDistance, float turningRadius, bool generateOneWp)
        {
            PathSegmentLengths bestPathLengths;
            PathWords bestWord;

            float shortestPathLength = FindShortestPathLength(carEndMod, out bestPathLengths, out bestWord);

            //No path could be found
            if (float.IsPositiveInfinity(shortestPathLength))
            {
                Debug.Log("Cant find a Reeds-Shepp path");
            
                return null;
            }
            //else
            //{
            //    Debug.Log(bestWord);
            //}


            //Calculate the waypoints to complete this path
            //Use the car's original start position because we no longer need the normalized version 
            List<RSCar> shortestPath = AddWaypoints(bestWord, bestPathLengths, carStart, carEnd, wpDistance, turningRadius, generateOneWp);


            return shortestPath;
        }



        //Add waypoints to a given path
        private static List<RSCar> AddWaypoints(
            PathWords word, PathSegmentLengths pathSegmentLengths, RSCar carStart, RSCar carEnd, float wpDistance, float turningRadius, bool generateOneWp)
        {
            //Find the car settings we need to drive through the path
            List<SegmentSettings> pathSettings = PathSettings.GetSettings(word, pathSegmentLengths);

            if (pathSettings == null)
            {
                Debug.Log("Cant find settings for a path");

                return null;
            }


            //Generate the waypoints

            //Data used when generating the path
            //The pos and heading we will move along the path
            Vector3 pos = carStart.pos;
            float heading = carStart.HeadingInRad;           
            //The distance between each step we take when generating the path, the smaller the better, but is also slower
            float stepDistance = 0.05f;
            //To generate waypoints with a certain distance between them we need to know how far we have driven since the last wp
            float driveDistance = 0f;


            //The waypoints
            List<RSCar> waypoints = new List<RSCar>();

            //Add the first wp
            waypoints.Add(new RSCar(pos, heading, pathSettings[0].gear, pathSettings[0].steering));


            //Loop through all path 3-5 path segments
            for (int i = 0; i < pathSettings.Count; i++)
            {
                SegmentSettings segmentSettings = pathSettings[i];
                
                //How many steps will we take to generate this segment
                //Will always be at least 2 no matter the stepDistance
                int n = (int)Math.Ceiling((segmentSettings.length * turningRadius) / stepDistance);

                //How far will we move each step?
                float stepLength = (segmentSettings.length * turningRadius) / n;

                //Change stuff depending on in which direction we are moving
                float steeringWheelPos = 1f;

                if (segmentSettings.steering == RSCar.Steering.Left)
                {
                    steeringWheelPos = -1f;
                }

                //Invert steering if we are reversing
                if (segmentSettings.gear == RSCar.Gear.Back)
                {
                    steeringWheelPos *= -1f;
                }


                //Drive through this segment in small steps
                for (int j = 0; j < n; j++)
                {
                    //Update position
                    float dx = stepLength * Mathf.Sin(heading);
                    float dz = stepLength * Mathf.Cos(heading);

                    if (segmentSettings.gear == RSCar.Gear.Back)
                    {
                        dx = -dx;
                        dz = -dz;
                    }

                    pos = new Vector3(pos.x + dx, pos.y, pos.z + dz);


                    //Update heading if we are turning
                    if (segmentSettings.steering != RSCar.Steering.Straight)
                    {
                        heading = heading + (stepLength / turningRadius) * steeringWheelPos;
                    }          


                    //Should we generate a new wp?
                    driveDistance += stepLength;

                    if (driveDistance > wpDistance)
                    {
                        waypoints.Add(new RSCar(pos, heading, segmentSettings.gear, segmentSettings.steering));

                        driveDistance = driveDistance - wpDistance;

                        if (generateOneWp)
                        {
                            return waypoints;
                        }
                    }
                }

                //We also need to add the last pos of this segment as waypoint or the path will not be the same
                //if we for example are ignoring the waypoint where we change direction
                waypoints.Add(new RSCar(pos, heading, segmentSettings.gear, segmentSettings.steering));
            }


            //Move the last wp pos to the position of the goal car
            //When we generate waypoints, the accuracy depends on the stepDistance, so is not always hitting the goal exactly
            waypoints[waypoints.Count - 1].pos = carEnd.pos;


            return waypoints;
        }
    }
}
