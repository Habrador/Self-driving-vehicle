using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    //Data for each cell so we dont need a million arrays
    public class Cell
    {
        //The center of cell in world space
        public Vector3 centerPos;
        //The heuristics we use when finding the shortest path
        public float heuristics;
        //The flow field (potential field), which tells the number of cells to the closest obstacle from each cell
        public float distanceToClosestObstacle;
        //The flow field (potential field), which tells the number of cells to the cell we want to drive to from each cell
        public float distanceToTarget;
        //If there is an obstacle in a cell
        public bool isObstacleInCell;
        //The voronoi field
        public VoronoiFieldCell voronoiFieldCell;
        //Which obstacle in the list of all obstacle is intersecting with this cell
        //Remember this can be null because we marked the border as obstacle
        public HashSet<int> obstaclesListPos;



        public Cell(Vector3 centerPos)
        {
            this.centerPos = centerPos;

            //Init the data
            this.heuristics = float.MaxValue;
            this.distanceToClosestObstacle = 0f;
            this.isObstacleInCell = false;
        }



        //Add obstacle pos
        public void AddObstacleToCell(int listPos)
        {
            if (obstaclesListPos == null)
            {
                obstaclesListPos = new HashSet<int>();
            }

            obstaclesListPos.Add(listPos);
        }
    }
}
