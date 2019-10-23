using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    //The potential field, which in this case is a voronoi field
    //Each cell in the field has a value between 0-1 and determines the distance to nearest obstacle and nearest voronoi edge
    //If you are standing on an edge, you are equally close to both obstacles that belongs to that edge  
    public static class VoronoiField
    {
        public static VoronoiFieldCell[,] GenerateField(Vector3[,] cellPositions, bool[,] isObstacle)
        {
            int gridSize = isObstacle.GetLength(0);

            //Init the data structure
            VoronoiFieldCell[,] voronoiField = new VoronoiFieldCell[gridSize, gridSize];

            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    voronoiField[x, z].worldPos = cellPositions[x, z];

                    voronoiField[x, z].isObstacle = isObstacle[x, z];
                }
            }


            //Step 1. Find out which occupied cell belongs to one or more obstacle areas by using a flood-fill algorithm
            //cell[5, 7] belongs to area 2, which is occupied by one or more obstacles connected.
            FindObstacleRegions(voronoiField);


            //Step 2. Run a flow-field algorithm to determine the closest distance to an obstacle 
            //and which cell with obstacle in it is the closest (may be multiple cells)
            FindVoronoiRegions(voronoiField);


            //Step 3. Find which cells are voronoi edges by searching through all cells, and if one of the surrounding cells belongs to 
            //another obstacle then the cell is on the edge, so the voronoi edge is not one-cell thick
            FindVoronoiEdges(voronoiField);


            //Step 4. Find the distance from each cell to the nearest voronoi edge, which can be done with a flow field from each edge
            //Will also find which voronoi edge cell is the closest (may be multiple cells)
            FindDistanceFromEdgeToCell(voronoiField);


            //Step 5. Create the voronoi field which tells the traversal cost at each cell
            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    //[0, 1] Is highest close to obstacles, and lowest close to voronoi edges
                    float rho = 0f;

                    if (voronoiField[x, z].isObstacle)
                    {
                        rho = 1f;
                    }
                    else if (voronoiField[x, z].isVoronoiEdge)
                    {
                        rho = 0f;
                    }
                    else
                    {
                        //The distance from the cell to the nearest obstacle
                        float d_o = voronoiField[x, z].distanceToClosestObstacle;
                        //The distance from the cell to the nearest oronoi edge
                        float d_v = voronoiField[x, z].distanceToClosestEdge;
                        //The falloff rate
                        float alpha = Parameters.voronoi_alpha;
                        //The maximum effective range of the field
                        //If d_0 >= d_o_max, the field is 0
                        float d_o_max = Parameters.d_o_max;

                        rho = (alpha / (alpha + d_o)) * (d_v / (d_o + d_v)) * (((d_o - d_o_max) * (d_o - d_o_max)) / (d_o_max * d_o_max));
                    }

                    voronoiField[x, z].voronoiFieldValue = rho;
                }
            }


            //Find the max and min values for debug
            //float min = float.MaxValue;
            //float max = float.MinValue;

            //for (int x = 0; x < gridSize; x++)
            //{
            //    for (int z = 0; z < gridSize; z++)
            //    {
            //        float value = voronoiField[x, z].voronoiFieldValue;

            //        if (value < min)
            //        {
            //            min = value;
            //        }
            //        if (value > max)
            //        {
            //            max = value;
            //        }
            //    } 
            //}

            //Debug.Log("Voronoi field max: " + max + " min: " + min);


            return voronoiField;
        }



        //
        // Find the islands of cells that are occupied by one or more obstacles
        //

        //Regions are integers beginning at 0, -1 means no region
        private static void FindObstacleRegions(VoronoiFieldCell[,] voronoiField)
        {
            int mapWidth = voronoiField.GetLength(0);

            //Init
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    voronoiField[x, z].region = -1;
                }
            }

            //Find the regions
            int regionNumber = 0;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //Is not a region if it's not an obstacle
                    if (!voronoiField[x, z].isObstacle)
                    {
                        continue;
                    }
                    //Is this cell already a part of a region?
                    if (voronoiField[x, z].region != -1)
                    {
                        continue;
                    }

                    //Find the region beginning at this cell with a flood fill
                    FloodFillObstacle(new IntVector2(x, z), voronoiField, regionNumber);

                    //Move to the next region
                    regionNumber += 1;
                }
            }
        }



        //Fill the area until we reach cells that are not obstacles 
        private static void FloodFillObstacle(IntVector2 startCell, VoronoiFieldCell[,] voronoiField, int regionNumber)
        {
            int mapWidth = voronoiField.GetLength(0);

            Queue<IntVector2> cellsToInvestigate = new Queue<IntVector2>();

            cellsToInvestigate.Enqueue(startCell);

            int safety = 0;

            while (cellsToInvestigate.Count > 0)
            {
                if (safety > 100000)
                {
                    Debug.Log("Stuck in infinite loop when flood filling Voronoi field obstacle");

                    break;
                }

                safety += 1;

                IntVector2 currentCell = cellsToInvestigate.Dequeue();

                //Give the cell the region we are in
                voronoiField[currentCell.x, currentCell.z].region = regionNumber;


                //Investigate neighbors
                //We shouldnt include corners, because then we can jump on the diagonal, which is not what we want
                IntVector2[] delta = HelpStuff.delta;

                for (int i = 0; i < delta.Length; i++)
                {
                    IntVector2 neighbor = new IntVector2(currentCell.x + delta[i].x, currentCell.z + delta[i].z);

                    //Is this cell within the map?
                    if (neighbor.x >= 0 && neighbor.x < mapWidth && neighbor.z >= 0 && neighbor.z < mapWidth)
                    {
                        //Is this cell an obstacle
                        if (voronoiField[neighbor.x, neighbor.z].isObstacle)
                        {
                            //Has this cell not been added a region
                            if (voronoiField[neighbor.x, neighbor.z].region == -1)
                            {
                                //Add it to the list of neighbors if it's not in the queue
                                if (!cellsToInvestigate.Contains(neighbor))
                                {
                                    cellsToInvestigate.Enqueue(neighbor);
                                }
                            }
                        }
                    }
                }
            }
        }



        //
        // Find the voronoi regions
        //

        //Each cell in a region is closer to the obstacle in the region than to any other obstacle
        //Will also find the closest obstacle cell from each cell
        private static void FindVoronoiRegions(VoronoiFieldCell[,] voronoiField)
        {
            int mapWidth = voronoiField.GetLength(0);

            //The flow field nodes will be stored in this array
            FlowFieldNode[,] flowField = new FlowFieldNode[mapWidth, mapWidth];

            //Init
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //All nodes are walkable because we are generating the flow from each obstacle
                    bool isWalkable = true;

                    FlowFieldNode node = new FlowFieldNode(isWalkable, voronoiField[x, z].worldPos, new IntVector2(x, z));

                    node.region = voronoiField[x, z].region;

                    flowField[x, z] = node;
                }
            }

            //A flow field can have several start nodes, which are the obstacles in this case
            List<FlowFieldNode> startNodes = new List<FlowFieldNode>();

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //If this is an obstacle
                    if (voronoiField[x, z].isObstacle)
                    {
                        startNodes.Add(flowField[x, z]);
                    }
                }
            }


            //Generate the flow field
            FlowField.Generate(startNodes, flowField, includeCorners: true);


            //Add the values to the celldata that belongs to the map
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    voronoiField[x, z].distanceToClosestObstacle = flowField[x, z].totalCostFlowField;

                    voronoiField[x, z].region = flowField[x, z].region;

                    voronoiField[x, z].closestObstacleCells = flowField[x, z].closestStartNodes;

                    //Now we can calculate the euclidean distance to the closest obstacle, which is more accurate then the flow field distance
                    HashSet<IntVector2> closest = voronoiField[x, z].closestObstacleCells;

                    if (closest != null && closest.Count > 0)
                    {
                        foreach (IntVector2 c in closest)
                        {
                            float distance = (voronoiField[c.x, c.z].worldPos - voronoiField[x, z].worldPos).magnitude;

                            voronoiField[x, z].distanceToClosestObstacle = distance;

                            //If we have multiple cells that are equally close, we only need to test one of them
                            break;
                        }
                    }
                }
            }
        }



        //
        // Find the voronoi edges
        //

        //Find which cells are voronoi edges by searching through all cells, and if one of the surrounding cells belongs to 
        //another obstacle then the cell is on the edge, so the voronoi edge is not one-cell thick
        private static void FindVoronoiEdges(VoronoiFieldCell[,] voronoiField)
        {
            int mapWidth = voronoiField.GetLength(0);

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {                
                    int currentRegion = voronoiField[x, z].region;

                    //If any of the surrounding cells is in another region, then this is an edge
                    //Should not check corners because that results in a fatter line
                    IntVector2[] delta = HelpStuff.delta;

                    for (int i = 0; i < delta.Length; i++)
                    {
                        IntVector2 neighbor = new IntVector2(x + delta[i].x, z + delta[i].z);

                        //Is this cell within the map?
                        if (neighbor.x >= 0 && neighbor.x < mapWidth && neighbor.z >= 0 && neighbor.z < mapWidth)
                        {
                            //If this cell is in another region
                            if (voronoiField[neighbor.x, neighbor.z].region != currentRegion)
                            {
                                voronoiField[x, z].isVoronoiEdge = true;

                                break;
                            }
                        }
                    }
                }
            }
        }



        //
        // Find the distance from each cell to the nearest voronoi edge
        //

        //This can be done with a flow field from each edge
        private static void FindDistanceFromEdgeToCell(VoronoiFieldCell[,] voronoiField)
        {
            int mapWidth = voronoiField.GetLength(0);

            //The flow field will be stored in this array
            FlowFieldNode[,] flowField = new FlowFieldNode[mapWidth, mapWidth];

            //Init
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //All nodes are walkable because we are generating the flow from each obstacle
                    bool isWalkable = voronoiField[x, z].isObstacle ? false : true;

                    FlowFieldNode node = new FlowFieldNode(isWalkable, voronoiField[x, z].worldPos, new IntVector2(x, z));

                    //node.region = voronoiField[x, z].region;

                    flowField[x, z] = node;
                }
            }

            //A flow field can have several start nodes, which are the obstacles in this case
            List<FlowFieldNode> startNodes = new List<FlowFieldNode>();

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //If this is an edge
                    if (voronoiField[x, z].isVoronoiEdge)
                    {
                        startNodes.Add(flowField[x, z]);
                    }
                }
            }


            //Generate the flow field
            FlowField.Generate(startNodes, flowField, includeCorners: true);


            //Add the values to the celldata that belongs to the map
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    voronoiField[x, z].distanceToClosestEdge = flowField[x, z].totalCostFlowField;

                    voronoiField[x, z].closestEdgeCells = flowField[x, z].closestStartNodes;

                    //Now we can calculate the euclidean distance to the closest obstacle, which is more accurate then the flow field distance
                    HashSet<IntVector2> closest = voronoiField[x, z].closestEdgeCells;

                    if (closest != null && closest.Count > 0)
                    {
                        foreach (IntVector2 c in closest)
                        {
                            float distance = (voronoiField[c.x, c.z].worldPos - voronoiField[x, z].worldPos).magnitude;

                            voronoiField[x, z].distanceToClosestEdge = distance;

                            //If we have multiple cells that are equally close, we only need to test one of them
                            break;
                        }
                    }
                }
            }
        }
    }
}
