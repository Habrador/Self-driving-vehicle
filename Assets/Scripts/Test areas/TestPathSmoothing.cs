using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles.ReedsSheppPaths;
using PathfindingForVehicles;


//Test pathsmoothing algorithm by generating a reeds-shepp path, add noise to it, and then smooth it
public class TestPathSmoothing : MonoBehaviour
{
    public Transform startTrans;
    public Transform endTrans;

    public List<Transform> obstaclesTrans;



    private void OnDrawGizmos()
    {
        Map map = InitMap();
        
        SmoothThisPath(map);
    }



    private Map InitMap()
    {
        //Generate the map, which is needed so we can test the voronoi field which pushes the path away from obstacles
        Map map = new Map(30, 1f);


        //Init the obstacles data needed to push the path away from these obstacles
        List<Obstacle> obstacles = new List<Obstacle>();

        for (int i = 0; i < obstaclesTrans.Count; i++)
        {
            Obstacle obs = new Obstacle(obstaclesTrans[i]);

            obstacles.Add(obs);
        }


        //Find which cells are obstacles
        map.allObstacles = obstacles;

        ObstaclesDetection.WhichCellsAreObstacle(map);


        //Generate the voronoi field
        VoronoiFieldCell[,] voronoiField = VoronoiField.GenerateField(map.CellCenterArray, map.CellObstacleArray);

        for (int x = 0; x < map.MapWidth; x++)
        {
            for (int z = 0; z < map.MapWidth; z++)
            {
                map.cellData[x, z].voronoiFieldCell = voronoiField[x, z];
            }
        }


        //Display
        DisplayMap(map);


        return map;
    }



    private void SmoothThisPath(Map map)
    {        
        //Generate the Reeds-Shepp path with some turning radius
        //Get the shortest path
        Vector3 startPos = startTrans.position;
        Vector3 endPos = endTrans.position;

        float startRot = startTrans.rotation.eulerAngles.y * Mathf.Deg2Rad;
        float endRot = endTrans.rotation.eulerAngles.y * Mathf.Deg2Rad;


        //Make sure the start and end are within the map
        if (!(map.IsPosWithinGrid(startPos) && map.IsPosWithinGrid(endPos)))
        {
            return;
        }


        //Get the shortest path
        List<RSCar> shortestPath = ReedsShepp.GetShortestPath(startPos, startRot, endPos, endRot, 12f, 1f, generateOneWp: false);


        //Make it unsmooth to easier see the difference
        Random.InitState(0);

        for (int i = 1; i < shortestPath.Count - 1; i++)
        {
            Vector3 p = shortestPath[i].pos;

            float dist = 0.4f;

            p.x += Random.Range(-dist, dist);
            p.z += Random.Range(-dist, dist);

            shortestPath[i].pos = p;
        }


        //To Node data formart
        List<Node> nodes = new List<Node>();

        for (int i = 1; i < shortestPath.Count - 1; i++)
        {
            Node previousNode = null;
            Vector3 pos = shortestPath[i].pos;
            float heading = shortestPath[i].HeadingInRad;
            bool isReversing = shortestPath[i].gear == RSCar.Gear.Back ? true : false;

            Node node = new Node(previousNode, pos, heading, isReversing);

            nodes.Add(node);
        }


        //Smooth the path and push it away from obstacles
        List<Node> smoothPath = ModifyPath.SmoothPath(nodes, map, false, isDebugOn: true);


        //Display
        DisplayPath(nodes, Color.white);
        DisplayPathNodes(nodes, Color.black);

        DisplayPath(smoothPath, Color.blue);
        //DisplayPathNodes(smoothPath, Color.black);
    }



    private void DisplayMap(Map map)
    {
        for (int x = 0; x < map.MapWidth; x++)
        {
            for (int z = 0; z < map.MapWidth; z++)
            {
                float cellWidth = map.CellWidth * 0.95f;

                Vector3 cellCenter = map.cellData[x, z].centerPos + new Vector3(0f, -0.5f, 0f);


                Gizmos.color = map.cellData[x, z].isObstacleInCell ? Color.red : Color.white;

                Gizmos.DrawCube(cellCenter, new Vector3(cellWidth, 0.01f, cellWidth));
            }
        }
    }



    private void DisplayPath(List<Node> path, Color color)
    {
        Gizmos.color = color;

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 p1 = path[i - 1].rearWheelPos;
            Vector3 p2 = path[i].rearWheelPos;

            Gizmos.DrawLine(p1, p2);
        }
    }



    private void DisplayPathNodes(List<Node> path, Color color)
    {
        Gizmos.color = color;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 p = path[i].rearWheelPos;

            Gizmos.DrawWireSphere(p, 0.1f);
        }
    }
}

