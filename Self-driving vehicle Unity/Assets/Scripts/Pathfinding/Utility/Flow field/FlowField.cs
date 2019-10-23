using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PathfindingForVehicles
{
    //The flow field algorithm on a grid, which finds the shortest path from one cell (or more cells) to all other cells
    public static class FlowField
    {
        //Include corners means we check 8 cells around each cell and not just 4
        public static void Generate(List<FlowFieldNode> startNodes, FlowFieldNode[,] allNodes, bool includeCorners)
        {
            //Reset such as costs and parent nodes, etc
            //Will set set costs to max value
            int mapWidth = allNodes.GetLength(0);

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    allNodes[x, z].Reset();

                    //Find all valid neighbors to this node, so no obstacles are allowed
                    HashSet<FlowFieldNode> neighbors = FindNeighboringNodes(allNodes[x, z], allNodes, mapWidth, includeCorners);

                    allNodes[x, z].neighborNodes = neighbors;
                }
            }


            //The queue with the open nodes
            Queue<FlowFieldNode> openSet = new Queue<FlowFieldNode>();

            //Add the start nodes to the list with open nodes
            for (int i = 0; i < startNodes.Count; i++)
            {
                FlowFieldNode startNode = startNodes[i];

                openSet.Enqueue(startNode);

                //Set the cost of the start node to 0
                startNode.totalCostFlowField = 0f;
                startNode.isInOpenSet = true;

                //The closest start cell to this cell is the cell itself
                startNode.closestStartNodes.Add(startNode.cellPos);
            }


            //Generate the flow field

            //To avoid infinite loop
            int safety = 0;

            //Stop the algorithm if open list is empty
            while (openSet.Count > 0)
            {
                if (safety > 500000)
                {
                    Debug.Log("Flow field stuck in infinite loop");

                    break;
                }
            
                safety += 1;

                //Pick the first node in the open set as the current node, no sorting is needed
                FlowFieldNode currentNode = openSet.Dequeue();

                currentNode.isInOpenSet = false;

                //Explore the neighboring nodes
                HashSet<FlowFieldNode> neighbors = currentNode.neighborNodes;

                foreach (FlowFieldNode neighbor in neighbors)
                {
                    //Cost calculations - The cost added can be different depending on the terrain
                    //This is not a costly operation (doesnt affect time) so no need to precalculate
                    float newCost = currentNode.totalCostFlowField + (currentNode.worldPos - neighbor.worldPos).magnitude;

                    //Update the the cost if it's less than the old cost
                    if (newCost <= neighbor.totalCostFlowField)
                    {
                        neighbor.totalCostFlowField = newCost;

                        //Change to which region this node belongs
                        //Is not always needed but is a fast operation
                        neighbor.region = currentNode.region;
                        //The closest of the start nodes to this node
                        //If they are equally close we need to save both
                        if (newCost == neighbor.totalCostFlowField)
                        {
                            foreach (IntVector2 c in currentNode.closestStartNodes)
                            {
                                neighbor.closestStartNodes.Add(c);
                            }
                        }
                        else
                        {
                            neighbor.closestStartNodes.Clear();

                            foreach (IntVector2 c in currentNode.closestStartNodes)
                            {
                                neighbor.closestStartNodes.Add(c);
                            }
                        }


                        //Add it if it isnt already in the list of open nodes
                        if (!neighbor.isInOpenSet)
                        {
                            openSet.Enqueue(neighbor);

                            neighbor.isInOpenSet = true;
                        }
                    }

                    //Dont need to add the current node back to the open set. If we find a shorter path to it from 
                    //another node, it will be added
                }
            }
        }



        //Find the neighboring nodes to a node by checking all nodes around it
        private static HashSet<FlowFieldNode> FindNeighboringNodes(FlowFieldNode node, FlowFieldNode[,] nodeArray, int mapWidth, bool includeCorners)
        {
            HashSet<IntVector2> neighborCells = new HashSet<IntVector2>();

            //Get the directions we can move in, which are up, left, right, down
            IntVector2[] delta = HelpStuff.delta;

            if (includeCorners)
            {
                delta = HelpStuff.deltaWithCorners;
            }


            //Will track if at least one neighbor is an obstacle, which may be useful to know later
            bool isNeighborObstacle = false;

            for (int i = 0; i < delta.Length; i++)
            {
                IntVector2 cellPos = new IntVector2(node.cellPos.x + delta[i].x, node.cellPos.z + delta[i].z);

                //Is this cell position within the grid?
                if (IsCellPosWithinGrid(cellPos, mapWidth))
                {
                    //Is not a valid neighbor if its obstacle
                    if (!nodeArray[cellPos.x, cellPos.z].isWalkable)
                    {
                        isNeighborObstacle = true;
                    }
                    else
                    {
                        neighborCells.Add(cellPos);
                    }
                }          
            }


            //If we are checking 8 neighbors we have to be careful to not jump diagonally if one cell next to the diagonal is obstacle
            //This is a costly operation (0.3 seconds if we do it for all cells) so only do it if at least one neighbor is obstacle
            if (includeCorners && isNeighborObstacle)
            {
                HashSet<IntVector2> corners = new HashSet<IntVector2>(HelpStuff.deltaJustCorners);

                //Loop through all 8 neighbors
                for (int i = 0; i < delta.Length; i++)
                {
                    //Is this neighbor a corner?
                    if (corners.Contains(delta[i]))
                    {
                        IntVector2 cellPos = new IntVector2(node.cellPos.x + delta[i].x, node.cellPos.z + delta[i].z);

                        //Have we added the corner to the list of neighbors
                        if (!neighborCells.Contains(cellPos))
                        {
                            continue;
                        }

                        //Check if neighbors to the corner are obstacles, if so we cant move to this corner
                        IntVector2 n1 = delta[HelpStuff.ClampListIndex(i + 1, delta.Length)];
                        IntVector2 n2 = delta[HelpStuff.ClampListIndex(i - 1, delta.Length)];

                        IntVector2 cellPos_n1 = new IntVector2(node.cellPos.x + n1.x, node.cellPos.z + n1.z);
                        IntVector2 cellPos_n2 = new IntVector2(node.cellPos.x + n2.x, node.cellPos.z + n2.z);

                        if (!nodeArray[cellPos_n1.x, cellPos_n1.z].isWalkable || !nodeArray[cellPos_n2.x, cellPos_n2.z].isWalkable)
                        {
                            //This is not a valid neighbor so remove it from neighbors
                            neighborCells.Remove(cellPos);
                        }
                    }
                }
            }



            //From cell to node
            HashSet<FlowFieldNode> neighborNodes = new HashSet<FlowFieldNode>();

            foreach (IntVector2 cell in neighborCells)
            {
                neighborNodes.Add(nodeArray[cell.x, cell.z]);
            }

            return neighborNodes;
        }



        //Is a cell position within the grid?
        private static bool IsCellPosWithinGrid(IntVector2 cellPos, int gridSize)
        {
            bool isWithin = false;

            if (cellPos.x >= 0 && cellPos.x < gridSize && cellPos.z >= 0 && cellPos.z < gridSize)
            {
                isWithin = true;
            }

            return isWithin;
        }



        //Calculate the max distance in a flow field
        public static float GetMaxDistance(float[,] flowField)
        {
            int mapWidth = flowField.GetLength(0);
        
            float maxDistance = -1f;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    float distance = flowField[x, z];

                    //float.maxValue is obstacle, so dont include those
                    if (distance > maxDistance && distance < float.MaxValue)
                    {
                        maxDistance = distance;
                    }
                }
            }

            return maxDistance;
        }
    }
}
