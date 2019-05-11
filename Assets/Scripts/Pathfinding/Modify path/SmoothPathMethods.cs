using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    //Methods that smooth a path
    public static class SmoothPathMethods
    {
        //Smooth a path with batch gradient descent (x = x - gamma * grad(x))
        //https://www.youtube.com/watch?v=umAeJ7LMCfU
        public static void GradientDescent(
            List<Vector3> path, 
            List<bool> isNodeFixed, 
            List<Obstacle> obstacles,
            Map map,
            bool isCircular, 
            float alpha, float beta, float gamma, float delta,
            bool isDebugOn)
        {
            //Using map.VoronoiField is slow in each iteration, so should cache
            VoronoiFieldCell[,] voronoiField = null;

            if (map != null && map.VoronoiField != null)
            {
                voronoiField = map.VoronoiField;
            }



            //The list with smooth coordinates
            List<Vector3> smoothPath = new List<Vector3>();

            //Add the old positions
            for (int i = 0; i < path.Count; i++)
            {
                smoothPath.Add(path[i]);
            }


            //Stop smoothing when all points have moved a distance less than this distance
            //If 0.00001 we need more than 1000 iterations
            float tolerance = 0.001f;

            //How far has all points together moved this iteration?
            //We are comparing the distance sqr instead of magnitude which is faster
            float totalChangeSqr = tolerance * tolerance;

            //So we dont end up in an infinite loop, is generally < 100
            int iterations = 0;

            //If we find a nan value its super slow to print that we did each iteration, so we do it once in the end
            bool hasFoundNan = false;

            while (totalChangeSqr >= tolerance * tolerance)
            {
                if (iterations > 1000)
                {
                    break;
                }

                iterations += 1;

                totalChangeSqr = 0f;

                //We are using surrounding values, so we need an array where we add values found during the iteration
                List<Vector3> newValuesThisIteration = new List<Vector3>(smoothPath.Count);

                for (int i = 0; i < path.Count; i++)
                {
                    //Dont move nodes that are fixed
                    if (isNodeFixed[i])
                    {
                        newValuesThisIteration.Add(smoothPath[i]);
                    
                        continue;
                    }

                    //Clamp when we reach end and beginning of list
                    //The first and last node should be set to fixed if the path is not set to circular
                    int i_plus_one = HelpStuff.ClampListIndex(i + 1, path.Count);

                    int i_minus_one = HelpStuff.ClampListIndex(i - 1, path.Count);


                    //Smooth!

                    //1. Minimize the distance between the smooth path and the original path
                    Vector3 newSmoothPos = smoothPath[i] + alpha * (path[i] - smoothPath[i]);


                    //2. Minimize the distance between this position and the surrounding positions
                    newSmoothPos += beta * (smoothPath[i_plus_one] + smoothPath[i_minus_one] - 2f * smoothPath[i]);


                    //3. Maximize the distance to the closest obstacle
                    //Is sometimes unstable because we use the closest obstacle, so is bouncing back and forth 
                    //until the loop stops because of infinite check
                    //if (obstacles != null)
                    //{
                    //    Vector3 closestObstaclePos = FindClosestObstaclePos(smoothPath[i], obstacles);

                    //    Vector3 dirToObstacle = closestObstaclePos - smoothPath[i];

                    //    //Ignore obstacles far away
                    //    float maxDist = 10f;

                    //    if (dirToObstacle.sqrMagnitude < maxDist * maxDist)
                    //    {
                    //        float distanceToObstacle = dirToObstacle.magnitude;

                    //        //To make obstacles closer more important
                    //        float scaler = 1f - (distanceToObstacle / maxDist);

                    //        newSmoothPos -= gamma * dirToObstacle.normalized * scaler;
                    //    }
                    //}

                    //We can also use the voronoi field to find the closest obstacle
                    if (voronoiField != null && gamma > 0f)
                    {
                        Vector3 nodePos = smoothPath[i];

                        //Get the data for this cell
                        IntVector2 cellPos = map.ConvertWorldToCell(nodePos);

                        //The data for this cell
                        VoronoiFieldCell v = voronoiField[cellPos.x, cellPos.z];

                        Vector3 closestObstaclePos = v.ClosestObstaclePos(nodePos, voronoiField);

                        if (closestObstaclePos.x != -1f)
                        {
                            Vector3 dirToObstacle = closestObstaclePos - nodePos;

                            //Ignore obstacles far away
                            float maxDist = 10f;

                            if (dirToObstacle.sqrMagnitude < maxDist * maxDist)
                            {
                                float distanceToObstacle = dirToObstacle.magnitude;

                                //To make obstacles closer more important
                                float scaler = 1f - (distanceToObstacle / maxDist);

                                newSmoothPos -= gamma * dirToObstacle.normalized * scaler;
                            }
                        }
                    }



                    //4. Use the Voronoi field to push vertices away from obstacles
                    //We need to find the derivative of the voronoi field function with respect to:
                    //- distance to obstacle edge - should be maximized
                    //- distance to voronoi edge - should be minimized
                    //...to optimize the distance to both edges
                    //This is kinda slow and useless since we are already pushing away the path from obstacles above?
                    if (voronoiField != null && delta > 0f)
                    {
                        Vector3 nodePos = smoothPath[i];

                        //Get the data for this cell
                        IntVector2 cellPos = map.ConvertWorldToCell(nodePos);

                        //The data for this cell
                        VoronoiFieldCell v = voronoiField[cellPos.x, cellPos.z];

                        //Same each iteration
                        float d_max = v.d_obs_max;
                        float a = v.alpha;

                        Vector3 closestObstaclePos = v.ClosestObstaclePos(nodePos, voronoiField);
                        Vector3 closestEdgePos = v.ClosestEdgePos(nodePos, voronoiField);

                        //If we are inside an obstacle when debugging, we wont have a closest
                        if (closestObstaclePos.x != -1f && closestEdgePos.x != -1f)
                        {
                            //The directions 
                            Vector3 dirToObstacle = closestObstaclePos - nodePos;
                            Vector3 dirToEdge = closestEdgePos - nodePos;

                            //The distances
                            float d_obs = dirToObstacle.magnitude;
                            float d_edg = dirToEdge.magnitude;


                            //The distance to voronoi edge
                            float upper_edge = a * d_obs * (d_obs - d_max) * (d_obs - d_max);
                            float lower_edge = d_max * d_max * (d_obs + a) * (d_edg + d_obs) * (d_edg + d_obs);

                            newSmoothPos -= delta * (upper_edge / lower_edge) * (-dirToEdge / d_edg);


                            //The distance to voronoi obstacle
                            float upper_obs = a * d_edg * (d_obs - d_max) * ((d_edg + 2f * d_max + a) * d_obs + (d_max + 2f * a) + a * d_max);
                            float lower_obs = d_max * d_max * (d_obs + a) * (d_obs + a) * (d_obs + d_edg) * (d_obs + d_edg);

                            newSmoothPos += delta * (upper_obs / lower_obs) * (dirToObstacle / d_obs);
                        }
                    }


                    //Sometimes the algorithm is unstable and shoots the vertices far away
                    //Maybe better to check for Nan in each part, so if the pos is NaN after optimizing to obstacle, dont add it???
                    if (float.IsNaN(newSmoothPos.x) || float.IsNaN(newSmoothPos.x) || float.IsNaN(newSmoothPos.x))
                    {
                        newSmoothPos = smoothPath[i];

                        hasFoundNan = true;
                    }

                    //Check if the new pos is within the map and if it is a value
                    if (map != null && !map.IsPosWithinGrid(newSmoothPos))
                    {
                        newSmoothPos = smoothPath[i];
                    }


                    //How far did we move the position this update?
                    totalChangeSqr += (newSmoothPos - smoothPath[i]).sqrMagnitude;


                    newValuesThisIteration.Add(newSmoothPos);
                }

                //Add the new values we created this iteration
                for (int i = 0; i < smoothPath.Count; i++)
                {
                    smoothPath[i] = newValuesThisIteration[i];
                }
            }

            if (isDebugOn)
            {
                string debugText = DisplayController.GetDisplayText("Smooth path iterations", iterations, null);

                Debug.Log(debugText);
            }

            if (hasFoundNan)
            {
                Debug.Log("Found Nan value");
            }
            


            //Add the new smooth positions to the original waypoints
            for (int i = 0; i < path.Count; i++)
            {
                path[i] = smoothPath[i];
            }
        }



        //Find closest point on closest obstacle
        private static Vector3 FindClosestObstaclePos(Vector3 pathPos, List<Obstacle> obstacles)
        {
            Vector3 closest = Vector3.zero;

            float closestDistSqr = float.MaxValue;

            for (int i = 0; i < obstacles.Count; i++)
            {
                //Alternative 1. Center of obstacle
                //Is not always working because obstacles may have different size
                //Vector3 obstaclePos = obstacles[i].centerPos;

                //Alternative 2. closest point on obstacle edges
                Vector3 obstaclePos = ObstaclesDetection.FindClosestPointOnObstacle(obstacles[i], pathPos);

                float distSqr = (obstaclePos - pathPos).sqrMagnitude;
                
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;

                    closest = obstaclePos;
                }
            }

            return closest;
        }



        //Smooth a path with constrained batch gradient descent (x = x - gamma * grad(x))
        //https://www.youtube.com/watch?v=a3whnj4H4DQ
        public static void ConstrainedGradientDescent(
            List<Vector3> path,
            List<bool> isNodeFixed,
            Map map,
            bool isCircular,
            float gamma,
            bool isDebugOn)
        {
            //The list with smooth coordinates
            List<Vector3> smoothPath = new List<Vector3>();

            //Add the old positions
            for (int i = 0; i < path.Count; i++)
            {
                smoothPath.Add(path[i]);
            }
            //Debug.Log(smoothPath.Count);

            //Stop smoothing when all points have moved a distance less than this distance
            //If 0.00001 we need more than 1000 iterations
            float tolerance = 0.001f;

            //How far has all points together moved this iteration?
            //We are comparing the distance sqr instead of magnitude which is faster
            float totalChangeSqr = tolerance * tolerance;

            //So we dont end up in an infinite loop, is generally < 100
            int iterations = 0;

            //If we find a nan value its super slow to print that we did each iteration, so we do it once in the end
            bool hasFoundNan = false;

            while (totalChangeSqr >= tolerance * tolerance)
            {
                if (iterations > 1000)
                {
                    break;
                }

                iterations += 1;

                totalChangeSqr = 0f;

                //We are using surrounding values, so we need an array where we add values found during the iteration
                List<Vector3> newValuesThisIteration = new List<Vector3>(smoothPath.Count);

                for (int i = 0; i < path.Count; i++)
                {
                    //Dont move nodes that are fixed
                    if (isNodeFixed[i])
                    {
                        newValuesThisIteration.Add(smoothPath[i]);

                        continue;
                    }

                    //Clamp when we reach end and beginning of list
                    //The first and last node should be set to fixed if the path is not set to circular
                    int i_plus_one = HelpStuff.ClampListIndex(i + 1, path.Count);

                    int i_minus_one = HelpStuff.ClampListIndex(i - 1, path.Count);

                    int i_plus_two = HelpStuff.ClampListIndex(i + 2, path.Count);

                    int i_minus_two = HelpStuff.ClampListIndex(i - 2, path.Count);

                    if (!isCircular)
                    {
                        if (i_plus_two == 0)
                        {
                            i_plus_two = path.Count - 1;
                        }
                        if (i_minus_two == path.Count - 1)
                        {
                            i_minus_two = 0;
                        }
                    }

                    //Smooth!
                    Vector3 newSmoothPos = smoothPath[i];

                    //1. Minimize the distance between this position and the surrounding positions
                    newSmoothPos += gamma * (smoothPath[i_plus_one] + smoothPath[i_minus_one] - 2f * smoothPath[i]);


                    //2. Right side
                    newSmoothPos += gamma * 0.5f * (2f * smoothPath[i_minus_one] - smoothPath[i_minus_two] - smoothPath[i]);


                    //3. Left side
                    newSmoothPos += gamma * 0.5f * (2f * smoothPath[i_plus_one] - smoothPath[i_plus_two] - smoothPath[i]);



                    //Sometimes the algorithm is unstable and shoots the vertices far away
                    //Maybe better to check for Nan in each part, so if the pos is NaN after optimizing to obstacle, dont add it???
                    if (float.IsNaN(newSmoothPos.x) || float.IsNaN(newSmoothPos.x) || float.IsNaN(newSmoothPos.x))
                    {
                        newSmoothPos = smoothPath[i];

                        hasFoundNan = true;
                    }

                    //Check if the new pos is within the map and if it is a value
                    if (map != null && !map.IsPosWithinGrid(newSmoothPos))
                    {
                        newSmoothPos = smoothPath[i];
                    }


                    //How far did we move the position this update?
                    totalChangeSqr += (newSmoothPos - smoothPath[i]).sqrMagnitude;


                    newValuesThisIteration.Add(newSmoothPos);
                }

                //Add the new values we created this iteration
                for (int i = 0; i < smoothPath.Count; i++)
                {
                    smoothPath[i] = newValuesThisIteration[i];
                }
            }

            if (isDebugOn)
            {
                string debugText = DisplayController.GetDisplayText("Smooth path iterations", iterations, null);

                Debug.Log(debugText);
            }

            if (hasFoundNan)
            {
                Debug.Log("Found Nan value");
            }



            //Add the new smooth positions to the original waypoints
            for (int i = 0; i < path.Count; i++)
            {
                path[i] = smoothPath[i];
            }
        }
    }
}
