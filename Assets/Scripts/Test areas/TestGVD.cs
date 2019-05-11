using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;



public class TestGVD : MonoBehaviour 
{
    public List<Transform> obstaclesTrans;

    public Transform testPos;



    private void OnDrawGizmos()
    {
        GenerateGVD();
    }



    //Generate the Generalized Voronoi Diagram
    private void GenerateGVD()
    {
        //Will automatically generate the center of each cell
        Map map = new Map(30, 1f);



        //Add which cells are obstacles
        List<Obstacle> obstacles = new List<Obstacle>();

        for (int i = 0; i < obstaclesTrans.Count; i++)
        {
            Obstacle obstacle = new Obstacle(obstaclesTrans[i]);

            obstacles.Add(obstacle);
        }

        map.allObstacles = obstacles;

        ObstaclesDetection.WhichCellsAreObstacle(map);



        //Generate the Voronoi field that determines the traversal cost at each cell
        VoronoiFieldCell[,] voronoiField = VoronoiField.GenerateField(map.CellCenterArray, map.CellObstacleArray);

        //To get the same random colors each time or they will constantly change color
        Random.InitState(0);

        //Find how many regions we have
        int regions = -1;
        for (int z = 0; z < map.MapWidth; z++)
        {
            for (int x = 0; x < map.MapWidth; x++)
            {
                if (voronoiField[x, z].region > regions)
                {
                    regions = voronoiField[x, z].region;
                }
            }
        }

        //Regions start at 0 so we have to add 1
        regions += 1;

        //Generate the random colors for each region
        List<Color> regionColors = new List<Color>();

        for (int i = 0; i < regions; i++)
        {
            regionColors.Add(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
        }

        //Debug.Log(regionColors.Count);
        //Debug.Log(regions);

        //Display the grid
        for (int z = 0; z < map.MapWidth; z++)
        {
            for (int x = 0; x < map.MapWidth; x++)
            {
                Vector3 cellPos = map.cellData[x, z].centerPos;

                //Gizmos.color = map.cellData[x, z].isObstacleInCell ? Color.red : Color.white;

                //Gizmos.DrawCube(cellPos, new Vector3(cellSize * 0.95f, 0.01f, cellSize * 0.95f));

                //continue;

                //if (voronoiField[x, z].region == -1)
                //{
                //    Gizmos.color = Color.white;
                //}
                //else if (voronoiField[x, z].isObstacle)
                //{
                //    Gizmos.color = Color.red;
                //}
                //else if (voronoiField[x, z].isVoronoiEdge)
                //{
                //    Gizmos.color = Color.gray;
                //}
                //else
                //{
                //    Gizmos.color = regionColors[voronoiField[x, z].region];
                //}

                //Color for voronoi regions
                Gizmos.color = regionColors[voronoiField[x, z].region];


                //Color for voronoi field
                float rgb = 1f - voronoiField[x, z].voronoiFieldValue;

                //Apparently, Gizmos.color is drawing at half color intensity, so we should double to get the correct color
                rgb *= 2f;

                //Gizmos.color = new Color(rgb, rgb, rgb, 1.0f);

                float cubeSize = map.CellWidth * 0.95f;

                Gizmos.DrawCube(cellPos, new Vector3(cubeSize, 0.01f, cubeSize));
            }
        }
    }
}
