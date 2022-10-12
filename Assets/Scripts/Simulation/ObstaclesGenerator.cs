using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathfindingForVehicles
{
    //Generates all obstacles, u�ncluding the flowfield showing the distance to the closest obstacle
    public class ObstaclesGenerator : MonoBehaviour
    {
        //Drags
        //The parent of the obstacle to get a cleaner workspace
        public Transform obstaclesParent;
        //Obstacle cube we add to the scene
        public GameObject obstaclePrefabObj;



        public void InitObstacles(Map map, Vector3 startPos)
        {
            //Generate obstacles
            GenerateObstacles(map, startPos);

            int mapWidth = map.MapWidth;

            //Figure out which cells the obstacle touch and set them to blocked by obstacle
            ObstaclesDetection.WhichCellsAreObstacle(map);

            //Generate the flow field showing how far to the closest obstacle from each cell
            GenerateObstacleFlowField(map, check8Cells: true);

            //Generate the voronoi field
            VoronoiFieldCell[,] voronoiField = VoronoiField.GenerateField(map.CellCenterArray, map.CellObstacleArray);

            for (int x = 0; x < map.MapWidth; x++)
            {
                for (int z = 0; z < map.MapWidth; z++)
                {
                    map.cellData[x, z].voronoiFieldCell = voronoiField[x, z];
                }
            }
        }



        //Generate obstacles and return the center coordinates of them in a list 
        //We need the car data so we can avoid adding obstacles at that position
        private void GenerateObstacles(Map map, Vector3 startPos)
        {
            //The rectangle where the car starts so we can remove obstacles in that area
            float marginOfSafety = 10f;

            float halfLength = (4f + 11f + marginOfSafety) * 0.5f;
            float halfWidth = (3f + marginOfSafety) * 0.5f;

            //The center pos is not the startPos because the semi is not the center of trailer + semi
            startPos += Vector3.forward * -6f;

            Vector3 FL = startPos + Vector3.forward * halfLength - Vector3.right * halfWidth;
            Vector3 FR = startPos + Vector3.forward * halfLength + Vector3.right * halfWidth;
            Vector3 BL = startPos - Vector3.forward * halfLength - Vector3.right * halfWidth;
            Vector3 BR = startPos - Vector3.forward * halfLength + Vector3.right * halfWidth;

            Rectangle avoidRect = new Rectangle(FL, FR, BL, BR);

            for (int i = 0; i < Parameters.obstaclesToAdd; i++)
            {
                AddObstacle(map, avoidRect);
            }
        }




        //Instantiate one cube and add its position to the array
        void AddObstacle(Map map, Rectangle avoidRect)
        {
            //Generate random coordinates in the map
            float posX = Random.Range(1f, map.MapWidth - 1f);
            float posZ = Random.Range(1f, map.MapWidth - 1f);
            //Rotation
            float rotY = Random.Range(0f, 360f);
            //Size
            float sizeX = Random.Range(Parameters.minObstacleSize, Parameters.maxObstacleSize);
            float sizeZ = Random.Range(Parameters.minObstacleSize, Parameters.maxObstacleSize);

            Vector3 pos = new Vector3(posX, 0.5f, posZ);

            Quaternion rot = Quaternion.Euler(0f, rotY, 0f);

            Vector3 scale = new Vector3(sizeX, 1f, sizeZ);

            //Update the prefab with the new data
            obstaclePrefabObj.transform.position = pos;
            obstaclePrefabObj.transform.rotation = rot;
            obstaclePrefabObj.transform.localScale = scale;

            Obstacle newObstacle = new Obstacle(obstaclePrefabObj.transform);


            //The obstacle shouldnt intersect with the start area
            if (Intersections.AreRectangleRectangleIntersecting(avoidRect, newObstacle.cornerPos))
            {
                return;
            }


            //Add a new obstacle object at this position
            Instantiate(obstaclePrefabObj, obstaclesParent);

            map.allObstacles.Add(newObstacle);
        }



        //Generate the flow field showing distance to closest obstacle from each cell
        private void GenerateObstacleFlowField(Map map, bool check8Cells)
        {
            int mapWidth = map.MapWidth;

            //The flow field will be stored in this array
            FlowFieldNode[,] flowField = new FlowFieldNode[mapWidth, mapWidth];

            //Init
            Cell[,] cellData = map.cellData;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    //All nodes are walkable because we are generating the flow from each obstacle
                    bool isWalkable = true;

                    FlowFieldNode node = new FlowFieldNode(isWalkable, cellData[x,z].centerPos, new IntVector2(x, z));

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
                    if (cellData[x, z].isObstacleInCell)
                    {
                        startNodes.Add(flowField[x, z]);
                    }
                }
            }

            //Generate the flow field
            FlowField.Generate(startNodes, flowField, check8Cells);


            //Add the values to the celldata that belongs to the map
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    cellData[x, z].distanceToClosestObstacle = flowField[x, z].totalCostFlowField;
                }
            }
        }
    }
}
