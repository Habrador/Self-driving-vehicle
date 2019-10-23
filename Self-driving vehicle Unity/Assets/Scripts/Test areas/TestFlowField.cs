using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;



public class TestFlowField : MonoBehaviour 
{
    public Transform ballTrans;



    private void OnDrawGizmos()
    {
        GenerateFlowField();
    }



    private void GenerateFlowField()
    {
        Map map = new Map(30, 1f);


        //Generate the center pos of each cell and random obstacles
        Random.InitState(0);

        for (int z = 0; z < map.MapWidth; z++)
        {
            for (int x = 0; x < map.MapWidth; x++)
            {
                if (Random.Range(0f, 1f) < 0.1f)
                {
                    map.cellData[x, z].isObstacleInCell = true;
                }
            }
        }


        //Generate the flow field if the target is within the map
        IntVector2 ballPos = map.ConvertWorldToCell(ballTrans.position);

        if (!map.IsCellWithinGrid(ballPos))
        {
            return;
        }

        FlowFieldNode[,] flowField = GenerateFlowField(map, ballPos);


        //Display the flow field
        float[,] flowDistances = new float[map.MapWidth, map.MapWidth];

        for (int z = 0; z < map.MapWidth; z++)
        {
            for (int x = 0; x < map.MapWidth; x++)
            {
                flowDistances[x, z] = flowField[x, z].totalCostFlowField;
            }
        }

        

        //Find the max value to easier display a color, which is 0-1
        float max = FlowField.GetMaxDistance(flowDistances);

        for (int z = 0; z < map.MapWidth; z++)
        {
            for (int x = 0; x < map.MapWidth; x++)
            {
                Vector3 cellPos = map.cellData[x, z].centerPos;

                //Grayscale
                float grayScale = flowDistances[x, z] / max;

                //A common way to display a flow field is to use red-green
                //where green decreases and red increases with distance to the goal
                Vector3 rgb = new Vector3(grayScale, 1f - grayScale, 0f);

                //Apparently, Gizmos.color is drawing at half color intensity, so we should double to get the correct color
                //rgb *= 2f;

                Gizmos.color = new Color(rgb.x, rgb.y, rgb.z);

                //Not accessible
                if (flowDistances[x, z] == float.MaxValue)
                {
                    Gizmos.color = Color.blue;
                }
                //Obstacle
                if (map.cellData[x, z].isObstacleInCell)
                {
                    Gizmos.color = Color.black;
                }

                float gizmosCellSize = map.CellWidth * 1f;

                Gizmos.DrawCube(cellPos, new Vector3(gizmosCellSize, 0.01f, gizmosCellSize));
            }
        }



        //Display which flowfield target is the closest
        HashSet<IntVector2> flowFieldTargetCellPos = flowField[ballPos.x, ballPos.z].closestStartNodes;

        foreach (IntVector2 c in flowFieldTargetCellPos)
        {
            Vector3 flowFieldTarget = map.cellData[c.x, c.z].centerPos;

            Gizmos.color = Color.white;

            Gizmos.DrawLine(flowFieldTarget, ballTrans.position);
        }
    }



    public static FlowFieldNode[,] GenerateFlowField(Map map, IntVector2 targetPos)
    {
        //The final flow field will be stored here, so init it
        FlowFieldNode[,] nodesArray = new FlowFieldNode[map.MapWidth, map.MapWidth];

        for (int x = 0; x < map.MapWidth; x++)
        {
            for (int z = 0; z < map.MapWidth; z++)
            {
                bool isWalkable = !map.cellData[x, z].isObstacleInCell;

                //bool isWalkable = true;

                FlowFieldNode node = new FlowFieldNode(isWalkable, map.cellData[x, z].centerPos, new IntVector2(x, z));

                nodesArray[x, z] = node;
            }
        }


        //A flow field can have several start nodes
        List<FlowFieldNode> startNodes = new List<FlowFieldNode>();

        //To a single target
        //startNodes.Add(nodesArray[targetPos.x, targetPos.z]);

        //To all obstacles
        for (int x = 0; x < map.MapWidth; x++)
        {
            for (int z = 0; z < map.MapWidth; z++)
            {
                if (map.cellData[x, z].isObstacleInCell)
                {
                    startNodes.Add(nodesArray[x, z]);
                }
            }
        }




        //Generate the flow field
        FlowField.Generate(startNodes, nodesArray, includeCorners: true);

        //Debug.Log("Distance flow field: " + nodesArray[targetPos.x, targetPos.z].totalCostFlowField);
        //Debug.Log("Distance flow field: " + nodesArray[0, 0].totalCostFlowField);


        return nodesArray;
    }
}
