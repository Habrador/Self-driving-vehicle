using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using PathfindingForVehicles.ReedsSheppPaths;

namespace PathfindingForVehicles
{
    //Hybrid A* pathfinding algorithm
    public static class HybridAStar
    {
        //The distance between each waypoint
        //Should be greater than the hypotenuse of the cell width or node may end up in the same cell
        public static float driveDistance = Mathf.Sqrt((Parameters.cellWidth * Parameters.cellWidth) * 2f) + 0.01f;
        //Used in the loop to easier include reversing
        private static float[] driveDistances = new float[] { driveDistance, -driveDistance};
        //The steering angles we are going to test
        private static float maxAngle = 40f;
        private static float[] steeringAngles = new float[] { -maxAngle * Mathf.Deg2Rad, 0f, maxAngle * Mathf.Deg2Rad };
        //The car will never reach the exact goal position, this is how accurate we want to be
        private const float posAccuracy = 1f;
        private const float headingAccuracy = 10f;
        //The heading resolution (Junior had 5) [degrees]
        private const float headingResolution = 15f;
        private const float headingResolutionTrailer = 15f;
        //To time the different parts of the algorithm 
        private static int timer_selectLowestCostNode;
        private static int timer_addNodeToHeap;
        private static int timer_findChildren;
        private static int timer_isCollidingWithObstacle;
        private static int timer_ReedsSheppNode;
        private static int timer_ReedsSheppHeuristics;
        private static int timer_TrailerCollision;
        //At what distance to should we start expanding Reeds-Shepp nodes
        private static float maxReedsSheppDist = 15f;



        //
        // Generate a path with Hybrid A*
        //
        public static List<Node> GeneratePath(Car startCar, Car endCar, Map map, List<Node> allExpandedNodes, Car startTrailer)
        {
            //Reset timers
            timer_selectLowestCostNode = 0;
            timer_addNodeToHeap = 0;
            timer_findChildren = 0;
            timer_isCollidingWithObstacle = 0;
            timer_ReedsSheppNode = 0;
            timer_ReedsSheppHeuristics = 0;
            timer_TrailerCollision = 0;
            //Other data we want to track
            //How many nodes did we prune?
            int prunedNodes = 0;
            //To track max number of nodes in the heap
            int maxNodesInHeap = 0;


            //Init the data structure we need
            int mapWidth = map.MapWidth;
            //Is faster to cache this than using map.cellData
            Cell[,] cellData = map.cellData;

            //Open nodes - the parameter is how many items can fit in the heap
            //If we lower the heap size it will still find a path, which is more drunk
            Heap<Node> openNodes = new Heap<Node>(200000);
            //int in the dictionaries below is the rounded heading used to enter a cell
            HashSet<int>[,] closedCells = new HashSet<int>[mapWidth, mapWidth];
            //The node in the cell with the lowest g-cost at a certain angle
            Dictionary<int, Node>[,] lowestCostNodes = new Dictionary<int, Node>[mapWidth, mapWidth];
            //Trailer
            //int in the dictionaries below is the rounded heading used to enter a cell
            HashSet<int>[,] closedCellsTrailer = new HashSet<int>[mapWidth, mapWidth];
            HashSet<int>[,] lowestCostNodesTrailer = new HashSet<int>[mapWidth, mapWidth];

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    closedCells[x, z] = new HashSet<int>();
                    lowestCostNodes[x, z] = new Dictionary<int, Node>();

                    //Trailer
                    closedCellsTrailer[x, z] = new HashSet<int>();
                    lowestCostNodesTrailer[x, z] = new HashSet<int>();
                }
            }


            //Create the first node
            //Why rear wheel? Because we need that position when simulating the "skeleton" car
            //and then it's easier if everything is done from the rear wheel positions            
            IntVector2 startCellPos = map.ConvertWorldToCell(startCar.rearWheelPos);

            Node node = new Node(
                previousNode: null,
                rearWheelPos: startCar.rearWheelPos,
                heading: startCar.HeadingInRadians,
                isReversing: false);

            if (startTrailer != null)
            {
                node.TrailerHeadingInRadians = startTrailer.HeadingInRadians;
            }

            node.AddCosts(gCost: 0f, hCost: cellData[startCellPos.x, startCellPos.z].heuristics);

            openNodes.Add(node);


            //The end of the path, which we will return
            Node finalNode = null;


            //bools so we can break out of the main loop
            //Set when search is complete
            bool found = false;
            //Set if we can't find a node to expand  
            bool resign = false;

            //To break out of the loop if it takes too long time
            int iterations = 0;

            IntVector2 goalCellPos = map.ConvertWorldToCell(endCar.rearWheelPos);

            while (!found && !resign)
            {
                if (iterations > 400000)
                {
                    Debug.Log("Stuck in infinite loop");

                    break;
                }

                iterations += 1;

                //If we don't have any nodes to expand
                if (openNodes.Count == 0)
                {
                    resign = true;

                    Debug.Log("Failed to find a path");
                }
                //We have nodes to expand
                else
                {
                    if (openNodes.Count > maxNodesInHeap)
                    {
                        maxNodesInHeap = openNodes.Count;
                    }
                
                    //Get the node with the lowest f cost
                    int timeBefore = Environment.TickCount;

                    Node nextNode = openNodes.RemoveFirst();

                    timer_selectLowestCostNode += Environment.TickCount - timeBefore;


                    //Close this cell
                    IntVector2 cellPos = map.ConvertWorldToCell(nextNode.rearWheelPos);

                    int roundedHeading = HelpStuff.RoundValue(nextNode.HeadingInDegrees, headingResolution);

                    HashSet<int> closedHeadingsInThisCell = closedCells[cellPos.x, cellPos.z];

                    bool haveAlreadyClosedCell = false;

                    //Close the cell, but we can still drive into this cell from another angle
                    if (!closedHeadingsInThisCell.Contains(roundedHeading))
                    {
                        closedHeadingsInThisCell.Add(roundedHeading);
                    }
                    else if (startTrailer == null)
                    {
                        haveAlreadyClosedCell = true;
                    }
                  

                    if (startTrailer != null)
                    {
                        int roundedHeadingTrailer = HelpStuff.RoundValue(nextNode.TrailerHeadingInDegrees, headingResolutionTrailer);

                        HashSet<int> closedTrailerHeadingsInThisCell = closedCellsTrailer[cellPos.x, cellPos.z];

                        if (!closedTrailerHeadingsInThisCell.Contains(roundedHeadingTrailer))
                        {
                            closedTrailerHeadingsInThisCell.Add(roundedHeadingTrailer);
                        }
                        else
                        {
                            haveAlreadyClosedCell = true;
                        }
                    }

                    //We have already expanded a better node with the same heading so dont expand this node
                    if (haveAlreadyClosedCell)
                    {
                        iterations -= 1;

                        continue;
                    }


                    //For debugging
                    allExpandedNodes.Add(nextNode);

                    //Check if the vehicle has reached the target
                    float distanceSqrToGoal = (nextNode.rearWheelPos - endCar.rearWheelPos).sqrMagnitude;

                    //But we also need to make sure the vehiclke has correct heading
                    float headingDifference = Mathf.Abs(endCar.HeadingInDegrees - nextNode.HeadingInDegrees);

                    //If we end up in the same cell or is within a certain distance from the goal
                    if ((distanceSqrToGoal < posAccuracy * posAccuracy || (cellPos.x == goalCellPos.x && cellPos.z == goalCellPos.z)) &&
                        headingDifference < headingAccuracy)
                    {                    
                        found = true;

                        Debug.Log("Found a path");

                        finalNode = nextNode;

                        //Make sure the end node has the same position as the target
                        finalNode.rearWheelPos.x = endCar.rearWheelPos.x;
                        finalNode.rearWheelPos.z = endCar.rearWheelPos.z;
                    }
                    //If we havent found the goal, then expand this node
                    else
                    {
                        //Get all child nodes
                        timeBefore = Environment.TickCount;

                        List<Node> children = GetChildrenToNode(nextNode, map, cellData, startCar.carData, endCar, startTrailer);

                        timer_findChildren += Environment.TickCount - timeBefore;


                        //Should we add any of the child nodes to the open nodes?
                        foreach (Node child in children)
                        {
                            IntVector2 childCell = map.ConvertWorldToCell(child.rearWheelPos);

                            int roundedChildHeading = HelpStuff.RoundValue(child.HeadingInDegrees, headingResolution);

                            //Has this cell been closed with this heading?
                            //If so, it means we already have a path at this cell with this heading, 
                            //and the existing node is cheaper because we have already expanded it
                            if (closedCells[childCell.x, childCell.z].Contains(roundedChildHeading) && startTrailer == null)
                            {                            
                                prunedNodes += 1;

                                continue;
                            }
                            //If we have a trailer
                            else if (closedCells[childCell.x, childCell.z].Contains(roundedChildHeading) && startTrailer != null)
                            {
                                int roundedTrailerHeading = HelpStuff.RoundValue(child.TrailerHeadingInDegrees, headingResolutionTrailer);

                                if (closedCellsTrailer[childCell.x, childCell.z].Contains(roundedTrailerHeading))
                                {
                                    prunedNodes += 1;

                                    continue;
                                }
                            }

                            //Have we already expanded a node with lower cost in this cell at this heading?
                            float costSoFar = child.gCost;

                            //The dictionary with lowest cost nodes in this cell
                            Dictionary<int, Node> nodesWithLowestCost = lowestCostNodes[childCell.x, childCell.z];

                            //Have we expanded with this angle to the cell before?
                            if (nodesWithLowestCost.ContainsKey(roundedChildHeading) && startTrailer == null)
                            {
                                //If the open node has a large gCost then we need to update that node with data
                                //from the child node
                                if (costSoFar < nodesWithLowestCost[roundedChildHeading].gCost)
                                {
                                    //If this child node is better then we should update the node in the open list
                                    //which is faster than deleting the old node and adding this child node
                                    Node existingNode = nodesWithLowestCost[roundedChildHeading];

                                    child.StealDataFromThisNode(existingNode);

                                    //Modify the heap-position of the node already in the open nodes
                                    openNodes.UpdateItem(existingNode);
                                }
                                //If the open node has a smaller gCost, then we dont need this child node, so do nothing

                                prunedNodes += 1;

                                continue;
                            }
                            //We have a trailer
                            else if (nodesWithLowestCost.ContainsKey(roundedChildHeading) && startTrailer != null)
                            {
                                //Have we expanded to this node before with this trailer heading
                                int roundedTrailerHeading = HelpStuff.RoundValue(child.TrailerHeadingInDegrees, headingResolutionTrailer);

                                if (lowestCostNodesTrailer[childCell.x, childCell.z].Contains(roundedTrailerHeading))
                                {
                                    //If the open node has a large gCost then we need to update that node with data
                                    //from the child node
                                    if (costSoFar < nodesWithLowestCost[roundedChildHeading].gCost)
                                    {
                                        //If this child node is better then we should update the node in the open list
                                        //which is faster than deleting the old node and adding this child node
                                        Node existingNode = nodesWithLowestCost[roundedChildHeading];

                                        child.StealDataFromThisNode(existingNode);

                                        //Modify the heap-position of the node already in the open nodes
                                        openNodes.UpdateItem(existingNode);
                                    }
                                    //If the open node has a smaller gCost, then we dont need this child node, so do nothing

                                    prunedNodes += 1;

                                    continue;
                                }
                            }
                            else
                            {
                                //Add the node to the cell with this angle
                                nodesWithLowestCost[roundedChildHeading] = child;

                                if (startTrailer != null)
                                {
                                    int roundedTrailerHeading = HelpStuff.RoundValue(child.TrailerHeadingInDegrees, headingResolutionTrailer);

                                    lowestCostNodesTrailer[childCell.x, childCell.z].Add(roundedTrailerHeading);
                                }
                            }


                            //Dont add the node if its colliding with an obstacle or is outside of map
                            timeBefore = Environment.TickCount;
                            if (ObstaclesDetection.HasCarInvalidPosition(child.rearWheelPos, child.heading, startCar.carData, map))
                            {
                                prunedNodes += 1;

                                continue;
                            }
                            timer_isCollidingWithObstacle += Environment.TickCount - timeBefore;


                            //Trailer obstacle calculations
                            int startTrailerTimer = Environment.TickCount;
                           
                            if (startTrailer != null)
                            {
                                //Now we need to check if this new position is valid by checking for collision with obstacles and the drag vehicle
                                //To do that we need the rear wheel pos of the trailer with this heading
                                //We know where the trailer is attached to the drag vehicle
                                Vector3 trailerAttachmentPoint = startCar.carData.GetTrailerAttachmentPoint(child.rearWheelPos, child.HeadingInRadians);

                                //Now we need the trailer's rear-wheel pos based on the new heading
                                Vector3 trailerRearWheelPos = startTrailer.carData.GetTrailerRearWheelPos(trailerAttachmentPoint, child.TrailerHeadingInRadians);

                                //Obstacle detection
                                //With the environment
                                if (ObstaclesDetection.HasCarInvalidPosition(trailerRearWheelPos, child.TrailerHeadingInRadians, startTrailer.carData, map))
                                {
                                    prunedNodes += 1;

                                    //Debug.Log("Semi trailer environment collision");

                                    continue;
                                }
                                //With the drag vehicle
                                if (ObstaclesDetection.IsTrailerCollidingWithDragVehicle(
                                    child.rearWheelPos, child.HeadingInRadians, startCar.carData,
                                    trailerRearWheelPos, child.TrailerHeadingInRadians, startTrailer.carData))
                                {
                                    prunedNodes += 1;

                                    //Debug.Log("Semi trailer collision");

                                    continue;
                                }
                            }
                            timer_TrailerCollision += Environment.TickCount - startTrailerTimer;


                            timeBefore = Environment.TickCount;

                            openNodes.Add(child);

                            timer_addNodeToHeap += Environment.TickCount - timeBefore;
                        }
                    }
                }
            }


            //Generate the final path when Hybrid A* has found the goal
            List<Node> finalPath = GenerateFinalPath(finalNode);


            //Display how long time everything took
            string display = DisplayController.GetDisplayTimeText(timer_selectLowestCostNode, "Select lowest cost node");

            display += DisplayController.GetDisplayTimeText(timer_addNodeToHeap, "Add new node to heap");

            display += DisplayController.GetDisplayTimeText(timer_findChildren, "Find children");

            display += DisplayController.GetDisplayTimeText(timer_isCollidingWithObstacle, "Is node colliding");

            display += DisplayController.GetDisplayTimeText(timer_ReedsSheppNode, "Reeds-Shepp Node");

            display += DisplayController.GetDisplayTimeText(timer_ReedsSheppHeuristics, "Reeds-Shepp Heuristics");

            display += DisplayController.GetDisplayTimeText(timer_TrailerCollision, "Trailer collision");

            display += DisplayController.GetDisplayText("Max nodes in heap", maxNodesInHeap, ". ");

            display += DisplayController.GetDisplayText("Expanded nodes", allExpandedNodes.Count, ". ");
            
            display += DisplayController.GetDisplayText("Pruned nodes", prunedNodes, null);            

            Debug.Log(display);


            //Display car positions along the final path
            DisplayController.DisplayVehicleAlongPath(finalPath, startCar.carData, startTrailer);


            return finalPath;
        }



        //
        // Get all children to a node
        //
        private static List<Node> GetChildrenToNode(Node currentNode, Map map, Cell[,] cellData, CarData carData, Car endCar, Car startTrailer)
        {
            List<Node> childNodes = new List<Node>();
        
            //To be able to expand we need the simulated vehicle's heading and position
            float heading = currentNode.heading;

            //Expand both forward and reverse
            for (int i = 0; i < driveDistances.Length; i++)
            {
                float driveDistance = driveDistances[i];

                //Expand all steering angles
                for (int j = 0; j < steeringAngles.Length; j++)
                {
                    //Steering angle
                    float alpha = steeringAngles[j];

                    //Turning angle
                    float beta = (driveDistance / carData.WheelBase) * Mathf.Tan(alpha);

                    //Simulate the car driving forward by using a mathematical car model
                    Vector3 newRearWheelPos = VehicleSimulationModels.CalculateNewPosition(heading, beta, driveDistance, currentNode.rearWheelPos);

                    float newHeading = VehicleSimulationModels.CalculateNewHeading(heading, beta);

                    //In which cell did we end up?
                    IntVector2 cellPos = map.ConvertWorldToCell(newRearWheelPos);

                    //Because we are doing obstacle detection later, we have to check if this pos is within the map
                    if (!map.IsCellWithinGrid(cellPos))
                    {
                        continue;
                    }

                    //Generate a new child node
                    Node childNode = new Node(
                       previousNode: currentNode,
                       rearWheelPos: newRearWheelPos,
                       heading: newHeading,
                       isReversing: driveDistance < 0f ? true : false);

                    float heuristics = HeuristicsToReachGoal(cellData, cellPos, childNode, endCar, carData);

                    childNode.AddCosts(
                        gCost: CostToReachNode(childNode, map, cellData),
                        hCost: heuristics);

                    //Calculate the new heading of the trailer if we have a trailer
                    if (startTrailer != null)
                    {
                        //Whats the new trailer heading at this childNode
                        float thetaOld = currentNode.TrailerHeadingInRadians;
                        float thetaOldDragVehicle = currentNode.HeadingInRadians;
                        float D = driveDistance;
                        float d = startTrailer.carData.WheelBase;
                        float newTrailerHeading = VehicleSimulationModels.CalculateNewTrailerHeading(thetaOld, thetaOldDragVehicle, D, d);

                        childNode.TrailerHeadingInRadians = newTrailerHeading;

                        //The trailer sux when reversing so add an extra cost
                        if (childNode.isReversing)
                        {
                            childNode.gCost += Parameters.trailerReverseCost;
                        }
                    }

                    childNodes.Add(childNode);
                }
            }



            //Expand Reeds-Shepp curve and add it as child node if we are "close" to the goal we want to reach
            int timeBefore = Environment.TickCount;

            //Dont do it every node because is expensive
            IntVector2 goalCell = map.ConvertWorldToCell(endCar.rearWheelPos);

            float distanceToEnd = cellData[goalCell.x, goalCell.z].distanceToTarget;

            //The probability should increase the close to the end we are
            float testProbability = Mathf.Clamp01((maxReedsSheppDist - distanceToEnd) / maxReedsSheppDist) * 0.2f;

            float probability = UnityEngine.Random.Range(0f, 1f);

            if ((distanceToEnd < maxReedsSheppDist && probability < testProbability) || (distanceToEnd < 40f && probability < 0.005f))
            {
                List<RSCar> shortestPath = ReedsShepp.GetShortestPath(
                    currentNode.rearWheelPos, 
                    currentNode.heading, 
                    endCar.rearWheelPos, 
                    endCar.HeadingInRadians, 
                    carData.turningRadius, 
                    driveDistance,
                    generateOneWp: true);

                if (shortestPath != null && shortestPath.Count > 1)
                {
                    //The first node in this list is where we currently are so we will use the second node
                    //But we might need to use several Reeds-Shepp nodes because if the path is going from
                    //forward to reverse, we cant ignore the change in direction, so we add a node before the 
                    //length which should be the driving distance

                    //But the easiest is just to add the second node
                    RSCar carToAdd = shortestPath[1];

                    bool isReversing = carToAdd.gear == RSCar.Gear.Back ? true : false;

                    IntVector2 cellPos = map.ConvertWorldToCell(carToAdd.pos);

                    //Because we are doing obstacle detection later, we have to check if this pos is within the map
                    if (map.IsCellWithinGrid(cellPos))
                    {
                        Node childNode = new Node(
                           previousNode: currentNode,
                           rearWheelPos: carToAdd.pos,
                           heading: carToAdd.HeadingInRad,
                           isReversing: isReversing);

                        float heuristics = HeuristicsToReachGoal(cellData, cellPos, childNode, endCar, carData);

                        childNode.AddCosts(
                            gCost: CostToReachNode(childNode, map, cellData),
                            hCost: heuristics);

                        childNodes.Add(childNode);

                        //Debug.Log("Added RS node");
                    }    
                }
            }

            timer_ReedsSheppNode += Environment.TickCount - timeBefore;

            return childNodes;
        }



        //
        // Calculate heuristics
        //
        private static float HeuristicsToReachGoal(Cell[,] cellData, IntVector2 cellPos, Node node, Car endCar, CarData carData)
        {
            float heuristics = cellData[cellPos.x, cellPos.z].heuristics;

            //But if we are close we might want to use the Reeds-Shepp distance as heuristics
            //This distance can be pre-calculated
            if (cellData[cellPos.x, cellPos.z].distanceToTarget < 20f)
            {
                int timeBefore = Environment.TickCount;

                float RS_distance = ReedsShepp.GetShortestDistance(
                    node.rearWheelPos,
                    node.heading,
                    endCar.rearWheelPos,
                    endCar.HeadingInRadians,
                    carData.turningRadius);

                timer_ReedsSheppHeuristics += Environment.TickCount - timeBefore;

                //Should use the max value according to the Junior report
                if (RS_distance > heuristics)
                {
                    heuristics = RS_distance;

                    //Debug.Log("Added Reeds-Shepp heuristics");
                }
            }

            return heuristics;
        }


        //
        // Calculate costs
        //
        private static float CostToReachNode(Node node, Map map, Cell[,] cellData)
        {
            Node previousNode = node.previousNode;

            IntVector2 cellPos = map.ConvertWorldToCell(node.rearWheelPos);


            //Cost 0 - how far have we driven so far
            float costSoFar = previousNode.gCost;

            //Cost 1 - driving distance to reach this node
            //Cant use driveDistance because sometimes we take steps smaller than than when generating Reeds-Shepp curves
            float distanceCost = (node.rearWheelPos - previousNode.rearWheelPos).magnitude;

            //Cost 2 - avoid obstacles by using the voronoi field
            float voronoiCost = Parameters.obstacleCost * cellData[cellPos.x, cellPos.z].voronoiFieldCell.voronoiFieldValue;

            //Cost 3 - reversing because its better to drive forward
            float reverseCost = node.isReversing ? Parameters.reverseCost : 0f;

            //Cost 4 - changing direction of motion from forward to reverse or the opposite because its annoying to sit in such a car
            float switchMotionCost = 0f;

            if ((node.isReversing && !previousNode.isReversing) || (!node.isReversing && previousNode.isReversing))
            {
                switchMotionCost = Parameters.switchingDirectionOfMovementCost;
            }


            //Calculate the final cost
            float cost = costSoFar + distanceCost * (1f + voronoiCost + reverseCost) + switchMotionCost;


            return cost;
        }



        //
        // Generate the final path when Hybrid A* has found the goal node
        //
        private static List<Node> GenerateFinalPath(Node finalNode)
        {
            List<Node> finalPath = new List<Node>();

            //Generate the path
            Node currentNode = finalNode;

            //Loop from the end of the path until we reach the start node
            while (currentNode != null)
            {
                finalPath.Add(currentNode);

                //Get the next node
                currentNode = currentNode.previousNode;
            }

            //If we have found a path 
            if (finalPath.Count > 1)
            {
                //Reverse the list so the finalNode is the last one in the list
                finalPath.Reverse();

                //Make sure the first node has the same driving direction has the second node
                //We dont really need it but it looks better when debugging
                finalPath[0].isReversing = finalPath[1].isReversing;
            }

            return finalPath;
        }
    }
}
