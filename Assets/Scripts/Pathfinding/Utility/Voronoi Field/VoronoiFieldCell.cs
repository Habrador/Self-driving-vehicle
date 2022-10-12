using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    public struct VoronoiFieldCell
    {
        //A Generalized Voronoi Diagram (GVD) is a Voronoi Diagram but on a grid
        //It includes the location of nearest obstacle cell, the distance to nearest obstacle, the distance to nearest voronoi edge
        //We are here going to use the Brushfire algorithm to generate it

        //The traversal cost at each cell, which is what we need to display the field
        //and in the pathfinding algorithm to add a cost so the vehicle avoids obstacles
        public float voronoiFieldValue;
        //Data we need to get the traversal cost
        //To which obstacle region does this cell belong, -1 means no regions 
        public int region;
        //Is this a voronoi edge
        public bool isVoronoiEdge;
        //The pos of this cell in world space
        public Vector3 worldPos;
        //Is this an obstacle
        public bool isObstacle;
        //The distance to closest obstacle d_obs
        public float distanceToClosestObstacle;
        //The distance to closest edge d_edg
        public float distanceToClosestEdge;
        //The cell with the closest obstacle (can be multiple cells if the distance is equal)
        public HashSet<IntVector2> closestObstacleCells;
        //The cell with the closest edge (can be multiple cells if the distance is equal)
        public HashSet<IntVector2> closestEdgeCells;



        //getters to easier use the same variables as the equations
        public float alpha 
        {
            get { return Parameters.voronoi_alpha; }
        }

        public float d_obs_max
        {
            get { return Parameters.d_o_max; }
        }

        public Vector3 ClosestObstaclePos(Vector3 pos, VoronoiFieldCell[,] cells)
        {
            Vector3 closest = Vector3.one * -1f;

            float closestDist = Mathf.Infinity;

            foreach (IntVector2 c in closestObstacleCells)
            {
                float distSqr = (pos - cells[c.x, c.z].worldPos).sqrMagnitude;
            
                if (distSqr < closestDist)
                {
                    closestDist = distSqr;

                    closest = cells[c.x, c.z].worldPos;
                }
            }

            return closest;
        }

        public Vector3 ClosestEdgePos(Vector3 pos, VoronoiFieldCell[,] cells)
        {
            Vector3 closest = Vector3.one * -1f;

            float closestDist = Mathf.Infinity;

            foreach (IntVector2 c in closestEdgeCells)
            {
                float distSqr = (pos - cells[c.x, c.z].worldPos).sqrMagnitude;

                if (distSqr < closestDist)
                {
                    closestDist = distSqr;

                    closest = cells[c.x, c.z].worldPos;
                }
            }

            return closest;
        }
    }
}
