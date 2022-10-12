using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace PathfindingForVehicles
{
    //Generate different types of heuristics and combine them
    //Hybrid A Star is using the maximum of the following heuristics:
    // - Euclidean Distance - with no obstacles
    // - Flow field (8 neighbors) with obstacles
    // - Reeds-Shepp paths - with no obstcles
    public static class HeuristicsController
    {
        //We calculate differet heurstics over some time to avoid a sudden stop in the simulation
        //Arrays that will store the different heuristics before we combine them
        //Euclidean Distance - with no obstacles
        private static float[,] euclideanHeuristics;
        //Flow field (8 neighbors) with obstacles (also known as dynamic programming or holonomic-with-obstacles)
        private static float[,] flowFieldHeuristics;
        //Reeds-Shepp paths - with no obstcles (also known as non-holonomic-without-obstacles)
        private static float[,] reedsSheppHeuristics;



        //Get the final heuristics from all individual heuristics, which is the maximum of them all
        public static void GenerateFinalHeuristics(Map map)
        {
            int mapWidth = map.MapWidth;

            //Heuristic is the max of the different heuristics
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //Maximum of all heuristics
                    map.cellData[x, z].heuristics = Mathf.Max(flowFieldHeuristics[x, z], euclideanHeuristics[x, z]);

                    //Adding max value breaks the simulation??? Because when we calculate f cost 
                    //we add g + h, which becomes a negative value if h is float.MaxValue and thus a good heuristics
                    //If obstacle, we should give it a high heuristic
                    //if (map.cellData[x, z].isObstacleInCell)
                    //{
                    //    map.cellData[x, z].heuristics = 10000f;
                    //}

                    //map.cellData[x, z].heuristics = euclideanHeuristics[x, z];
                    //map.cellData[x, z].heuristics = flowFieldHeuristics[x, z];

                    if (map.cellData[x, z].isObstacleInCell)
                    {
                        map.cellData[x, z].heuristics = 10000f;
                    }
                }
            }
        }



        //Calculate the euclidean distance from all squares to the target
        public static void EuclideanDistance(Map map, IntVector2 targetCellPos)
        {
            int mapWidth = map.MapWidth;
        
            euclideanHeuristics = new float[mapWidth, mapWidth];

            Vector3 targetPos = map.cellData[targetCellPos.x, targetCellPos.z].centerPos;

            //Populate the heuristics array
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //Don't need to check cells that are obstacles
                    if (!map.cellData[x, z].isObstacleInCell)
                    {
                        Vector3 thisCellPos = map.cellData[x, z].centerPos;

                        //The distance from the center of the square to the target
                        float heuristic = (targetPos - thisCellPos).magnitude;

                        euclideanHeuristics[x, z] = heuristic;
                    }
                }
            }
        }



        //Calculate the shortest path with obstacles from each cell to the target cell we want to reach
        //Is called Dynamic Programming in "Programming self-driving car" but is the same as a flow map
        //Is called holonomic-with-obstacles in the reports
        public static void DynamicProgramming(Map map, IntVector2 targetPos)
        {
            int mapWidth = map.MapWidth;


            //Debug.DrawLine(map.cellData[targetPos.x, targetPos.z].centerPos, Vector3.zero, Color.red, 15f);


            //The final flow field will be stored here, so init it
            FlowFieldNode[,] nodesArray = new FlowFieldNode[mapWidth, mapWidth];

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    bool isWalkable = map.cellData[x, z].isObstacleInCell ? false : true;

                    FlowFieldNode node = new FlowFieldNode(isWalkable, map.cellData[x, z].centerPos, new IntVector2(x, z));

                    nodesArray[x, z] = node;
                }
            }

            //A flow field can have several start nodes, but in this case we have just one, which is the cell we want to reach
            List<FlowFieldNode> startNodes = new List<FlowFieldNode>();

            startNodes.Add(nodesArray[targetPos.x, targetPos.z]);


            //Generate the flow field
            FlowField.Generate(startNodes, nodesArray, includeCorners: true);


            //Save the values
            flowFieldHeuristics = new float[mapWidth, mapWidth];

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //Save the flow field because we will use it to display it on a texture
                    map.cellData[x, z].distanceToTarget = nodesArray[x, z].totalCostFlowField;

                    //This heuristics has to be discounted by a value to be admissible to never overestimate the actual cost
                    flowFieldHeuristics[x, z] = nodesArray[x, z].totalCostFlowField * 0.92621f;
                }
            }


            //Debug.Log("Distance flow field: " + nodesArray[targetPos.x, targetPos.z].totalCostFlowField);
            //Debug.Log("Distance flow field: " + nodesArray[0, 0].totalCostFlowField);
        }
    }
}
