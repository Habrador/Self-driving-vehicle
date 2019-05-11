using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathfindingForVehicles
{
    //Modifies a path, such as smoothing it or moving the waypoints so the car can easier follow the path
    public static class ModifyPath
    {
        //Smooth a path
        public static List<Node> SmoothPath(List<Node> path, Map map, bool isCircular, bool isDebugOn)
        {
            //Find which nodes the smoother cant move, such as the start position
            //Has the same index as the path, so isNodeFixed[0] is the first node in the path
            List<bool> isNodeFixed = FindFixedNodes(path, isCircular);

            List<Obstacle> obstacles = null;

            if (map != null)
            {
                obstacles = map.allObstacles;
            }


            //Smooth the front wheel path
            //To make it more general, we should use a List<Vector3> instead of Node
            List<Vector3> frontCoordinates = new List<Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                frontCoordinates.Add(path[i].frontWheelPos);
            }

            SmoothPathMethods.GradientDescent(
                   frontCoordinates,
                   isNodeFixed,
                   obstacles,
                   map,
                   isCircular,
                   Parameters.alpha, Parameters.beta, Parameters.gamma, Parameters.delta,
                   isDebugOn);


            //But this new path might not be a valid path
            //A report is suggesting that you should check the smooth path for collisions. If you find a node
            //that's colliding, then this node should be the same as the Hybrid A* node which is collision free and set to fixed,
            //and then run the smooth algorithm again until no collisions can be found

            //The smooth path should be a new list so we can display both paths
            List<Node> smoothPath = new List<Node>();

            for (int i = 0; i < path.Count; i++)
            {
                //Create a new node with the data we need for the smooth path
                Node clonedNode = new Node();
                
                path[i].StealDataFromThisNode(clonedNode);

                clonedNode.frontWheelPos = frontCoordinates[i];

                smoothPath.Add(clonedNode);
            }



            //Smooth the reverse wheel path
            //To make it more general, we should use a List<Vector3> instead of Node
            List<Vector3> rearCoordinates = new List<Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                rearCoordinates.Add(path[i].reverseWheelPos);
            }

            SmoothPathMethods.GradientDescent(
                   rearCoordinates,
                   isNodeFixed,
                   obstacles,
                   map,
                   isCircular,
                   Parameters.alpha, Parameters.beta, Parameters.gamma, Parameters.delta,
                   isDebugOn);

            //Add the coordinates to the smooth path
            for (int i = 0; i < rearCoordinates.Count; i++)
            {
                smoothPath[i].reverseWheelPos = rearCoordinates[i];
            }


            //Smooth the path again
            //The distance between waypoints is still roughly 1 m, which can lead to aprupt steering
            //So we need to smooth it again by adding waypoints between the old waypoints by using non-parametric interpolation, 
            //and then smooth it (just the curvature not the distance to obstacle) while the original waypoints are fixed
            //The distance between these new waypoints should be 5-10 cm (according to the official Junior report)
            isNodeFixed = new List<bool>();

            smoothPath = AddWaypoints(smoothPath, isNodeFixed, isCircular);

            //Smooth the new waypoints
            List<Vector3> coordinatesF = new List<Vector3>();
            List<Vector3> coordinatesB = new List<Vector3>();

            for (int i = 0; i < smoothPath.Count; i++)
            {
                coordinatesF.Add(smoothPath[i].frontWheelPos);
                coordinatesB.Add(smoothPath[i].reverseWheelPos);
            }

            SmoothPathMethods.ConstrainedGradientDescent(coordinatesF, isNodeFixed, map, isCircular, 0.2f, isDebugOn);
            SmoothPathMethods.ConstrainedGradientDescent(coordinatesB, isNodeFixed, map, isCircular, 0.2f, isDebugOn);

            //Add the new coordinates to the nodes
            for (int i = 0; i < coordinatesF.Count; i++)
            {
                //Create a new node with the data we need for the smooth path
                smoothPath[i].frontWheelPos = coordinatesF[i];
                smoothPath[i].reverseWheelPos = coordinatesB[i];
            }


            //Sometimes we fail to smooth the path because gradient descent can be unstable
            if (smoothPath.Count > 0)
            {
                return smoothPath;
            }
            else
            {
                Debug.Log("Couldnt smooth path");

                return null;
            }
        }



        //To smooth the path we need to know which nodes are fixed and shouldnt be moved when smoothing to keep the shape of the path
        private static List<bool> FindFixedNodes(List<Node> path, bool isCircular)
        {
            List<bool> isNodeFixed = new List<bool>();

            //Step 1
            //Find the points where we change direction from forward -> reverse or the opposite and fix them
            for (int i = 0; i < path.Count; i++)
            {
                //Add the first and the last wp as fixed if the path is not circular
                if ((i == 0 || i == path.Count - 1) && !isCircular)
                {
                    isNodeFixed.Add(true);
                }
                //Add the wp where we are going from forward -> reverse or the opposite
                else if (path[i].isReversing != path[HelpStuff.ClampListIndex(i + 1, path.Count)].isReversing)
                {
                    isNodeFixed.Add(true);
                }
                //This node is not fixed
                else
                {
                    isNodeFixed.Add(false);
                }
            }


            //Step 2
            //Fix the waypoints before a change in direction of the car will not smoothly move to that waypoint
            //This is not needed when if we have moved the position of the path to the front wheels
            //Then this gives a worse result
            //Need a temp list or all waypoints will be set to fixed when we loop though the list
            //List<bool> isNodeFixedTemp = new List<bool>(isNodeFixed);

            //for (int i = 1; i < isNodeFixed.Count - 1; i++)
            //{
            //    //If the previous node is fixed or the upcoming node is fixed, then this node should be fixed
            //    if (isNodeFixed[i - 1] || isNodeFixed[i + 1])
            //    {
            //        isNodeFixedTemp[i] = true;
            //    }
            //}

            ////Transfer the new true values from the temp list to the list we are actually using
            //for (int i = 0; i < isNodeFixedTemp.Count; i++)
            //{
            //    if (isNodeFixedTemp[i])
            //    {
            //        isNodeFixed[i] = true;
            //    }
            //}

            //But the second to last node should not be fixed, because the last node is often not at the correct position
            //because Hybrid A* is not always finding the exact solution
            //if (isNodeFixed.Count > 1)
            //{
            //    isNodeFixed[isNodeFixed.Count - 2] = false;
            //}


            return isNodeFixed;
        }



        //We generated this path by using the rear-wheel position. To easier be able to make the vehicle follow the path
        //we should translate the path to the front axle, by using the the distance between the front- and rear axle
        //We might also want the mirrored version of the front axle along the rear axle, to get a path we can use when reversing
        public static void CalculateFrontAxlePositions(List<Node> path, CarData carData, Vector3 vehicleStartDir, Vector3 vehicleEndDir, bool isMirrored)
        {
            //Move the waypoints in the heading direction with a distance 
            //This distance is the same as the distance between the front axle and rear axle (= wheel base)
            float moveDistance = carData.WheelBase;

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 dir_before, dir_after;

                if (i == 0)
                {
                    dir_before = vehicleStartDir;
                }
                else
                {
                    dir_before = (path[i].rearWheelPos - path[i - 1].rearWheelPos).normalized;

                    if (path[i].isReversing)
                    {
                        dir_before *= -1f;
                    }
                }

                if (i == path.Count - 1)
                {
                    dir_after = vehicleEndDir;
                }
                else
                {
                    dir_after = (path[i + 1].rearWheelPos - path[i].rearWheelPos).normalized;

                    if (path[i + 1].isReversing)
                    {
                        dir_after *= -1f;
                    }
                }

                //Vector3 dir = (dir_before + dir_after) * 0.5f;

                Vector3 dir = dir_after;

                if (isMirrored)
                {
                    dir *= -1f;
                }

                if (!isMirrored)
                {
                    path[i].frontWheelPos = path[i].rearWheelPos + dir * moveDistance;
                }
                else
                {
                    path[i].reverseWheelPos = path[i].rearWheelPos + dir * moveDistance;
                }

                //path[i].frontWheelPos = path[i].rearWheelPos;
            }
        }



        //Add waypoints to the path to make it easier for the vehicle to follow
        private static List<Node> AddWaypoints(List<Node> path, List<bool> isNodeFixed, bool isCircular)
        {
            //How many new nodes should we add?
            int newNodesToAdd = 2;

            //We need to add the new positions to a new list because we are using the length of the old list in the loop
            List<Node> newNodes = new List<Node>();

            //Add the first node
            newNodes.Add(path[0]);
            //All old nodes are fixed
            isNodeFixed.Add(true);


            for (int i = 1; i < path.Count; i++)
            {            
                //Add new nodes between the previous node and this node
                float distB = (path[i].reverseWheelPos - path[i - 1].reverseWheelPos).magnitude;
                float distF = (path[i].frontWheelPos - path[i - 1].frontWheelPos).magnitude;

                Vector3 dirB = (path[i].reverseWheelPos - path[i - 1].reverseWheelPos).normalized;
                Vector3 dirF = (path[i].frontWheelPos - path[i - 1].frontWheelPos).normalized;

                //Distance between new nodes
                float distBetweenB = distB / (float)(newNodesToAdd + 1);
                float distBetweenF = distF / (float)(newNodesToAdd + 1);

                Vector3 currentPosB = path[i - 1].reverseWheelPos;
                Vector3 currentPosF = path[i - 1].frontWheelPos;

                for (int j = 0; j < newNodesToAdd; j++)
                {
                    currentPosB += dirB * distBetweenB;
                    currentPosF += dirF * distBetweenF;

                    Node newNode = new Node();

                    newNode.frontWheelPos = currentPosF;
                    newNode.reverseWheelPos = currentPosB;
                    newNode.isReversing = path[i].isReversing;

                    newNodes.Add(newNode);

                    //None of the new nodes are fixed
                    isNodeFixed.Add(false);
                }

                newNodes.Add(path[i]);
                isNodeFixed.Add(true);
            }

            return newNodes;


            //Add nodes between last and first if is circular
            //if (isCircular)
            //{
            //    int lastPosInt = pathOld.Count - 1;

            //    //Add new nodes between the previous node and this node
            //    float dist = (pathOld[lastPosInt].rearWheelPos - pathOld[0].rearWheelPos).magnitude;

            //    Vector3 dir = (pathOld[0].rearWheelPos - pathOld[lastPosInt].rearWheelPos).normalized;

            //    //How many nodes should we add
            //    int nodesToAdd = Mathf.FloorToInt(dist / distBetweenNewNodes);

            //    Vector3 currentPos = pathOld[lastPosInt].rearWheelPos;

            //    for (int j = 0; j < nodesToAdd; j++)
            //    {
            //        currentPos += dir * distBetweenNewNodes;

            //        Node newNode = new Node(null, currentPos, 0f, pathOld[0].isReversing);

            //        path.Add(newNode);

            //        //None of the new nodes are fixed
            //        isNodeFixed.Add(false);
            //    }
            //}
        }
    }
}
