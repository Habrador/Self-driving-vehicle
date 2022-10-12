using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathfindingForVehicles
{
    //Takes care of all obstacle detection related to Hybrid A*
    public static class ObstaclesDetection
    {
        //
        // Check if the car is colliding with an obstacle or is outside of map
        //
        public static bool HasCarInvalidPosition(Vector3 carRearWheelPos, float heading, CarData carData, Map map)
        {
            bool hasInvalidPosition = false;


            //Step 1. Check if the car's rear wheel center is inside of the map
            IntVector2 rearWheelCellPos = map.ConvertWorldToCell(carRearWheelPos);

            if (!map.IsCellWithinGrid(rearWheelCellPos))
            {
                 //This is not a valid position
                 hasInvalidPosition = true;

                return hasInvalidPosition;
            }


            //Step 2. Check if any of the car's corner is outside of the map

            //Make the car bigger than it is to be on the safe side
            float carLength = carData.CarLength + Parameters.marginOfSafety;
            float carWidth = carData.carWidth + Parameters.marginOfSafety;

            Vector3 carCenterPos = carData.GetCenterPos(carRearWheelPos, heading);

            //Find all corners of the car
            Rectangle corners = CarData.GetCornerPositions(carCenterPos, heading, carWidth, carLength);

            //Detect if any of the corners is outside of the map = is the cell the corner is a part of the map
            HashSet<IntVector2> carCellPositions = new HashSet<IntVector2>();

            carCellPositions.Add(map.ConvertWorldToCell(corners.FL));
            carCellPositions.Add(map.ConvertWorldToCell(corners.FR));
            carCellPositions.Add(map.ConvertWorldToCell(corners.BL));
            carCellPositions.Add(map.ConvertWorldToCell(corners.BR));

            foreach (IntVector2 cellPos in carCellPositions)
            {
                if (!map.IsCellWithinGrid(cellPos))
                {
                    //At least one of the corners is outside of the map
                    hasInvalidPosition = true;

                    return hasInvalidPosition;
                }
            }


            //Step 3. Check if some of the car's known positions are far away from an obstacle

            //The car is not colliding with anything if the steps to an obstacle from center of car is greater than the length of the car
            IntVector2 carCenterCellPos = map.ConvertWorldToCell(carCenterPos);

            if (map.cellData[carCenterCellPos.x, carCenterCellPos.z].distanceToClosestObstacle > carData.CarLength * 0.7f)
            {
                //This is a valid position
                hasInvalidPosition = false;

                return hasInvalidPosition;
            }


            //Step 4. Check if the car is hitting an obstacle

            //Use the car's corners and then rectangle-rectangle-intersection with the obstacles
            hasInvalidPosition = IsCarIntersectingWithObstacles(corners, carCenterCellPos, map);        



            return hasInvalidPosition;
        }



        //
        // Check if the trailer is colliding with the drag vehicle
        //
        public static bool IsTrailerCollidingWithDragVehicle(
            Vector3 semiRearWheelPos, float semiHeading, CarData semiData, 
            Vector3 trailerRearWheelPos, float trailerHeading, CarData trailerData)
        {
            bool isColliding = false;

            //Use triangle-traingle intersection so we need the rectangles
            Vector3 trailerCenter = trailerData.GetCenterPos(trailerRearWheelPos, trailerHeading);

            Rectangle trailerRect = CarData.GetCornerPositions(trailerCenter, trailerHeading, trailerData.carWidth * 0.9f, trailerData.CarLength);

            //The semi's cabin rectangle
            Vector3 cabinCenter = semiData.GetSemiCabinCenter(semiRearWheelPos, semiHeading);

            //Make it slightly shorter or too many false collisions
            Rectangle semiRect = CarData.GetCornerPositions(cabinCenter, semiHeading, semiData.carWidth, semiData.cabinLength * 0.95f);

            if (Intersections.AreRectangleRectangleIntersecting(trailerRect, semiRect))
            {            
                return true;
            }

            return isColliding;
        }



        //
        // Test if one path is drivable
        //
        public static bool IsPathDrivable(List<Node> path, CarData carData, Map map)
        {
            //Ignore the first node because we know its drivable
            for (int i = 1; i < path.Count; i++)
            {
                Vector3 carPos = path[i].rearWheelPos;

                float carHeading = path[i].heading;

                if (HasCarInvalidPosition(carPos, carHeading, carData, map))
                {
                    //This path is not drivable
                    return false;
                }

            }

            //This path is drivable
            return true;
        }



        //
        // Use the car's corners and then rectangle-rectangle-intersection with the obstacles to check if the car is intersecting with an obstacle
        //
        private static bool IsCarIntersectingWithObstacles(Rectangle carRectangle, IntVector2 carCenterCellPos, Map map)
        {
            bool hasInvalidPosition = false;

            ////Alternative 1. Find if one of the cells the car is intersecting is an obstacle
            ////This is slower because it requires a lot of triangle-triangle intersections to identify the cells
            ////which is the same as intersecting with the obstacles in the first place
            //bool isCellIntersectedByCarObstacle = FindIfCellBlockedByRectangleIsObstacle(carRectangle, carCenterCellPos, map);

            
            //if (isCellIntersectedByCarObstacle)
            //{
            //    hasInvalidPosition = true;

            //    return hasInvalidPosition;
            //}

            //return false;



            //But if we want more accurate collision detection we have to take into account that the obstacle
            //may not block the entire cell, so the can can maybe move into this position without colliding with an obstacle

            //Find all obstacles that are close to the car, so we dont have to check all obstacles
            //List<Obstacle> obstaclesThatAreClose = FindCloseObstaclesCell(carPos);
            //List<Obstacle> obstaclesThatAreClose = FindCloseObstaclesAABB(carCorners, map);

            //Maybe more efficient to instead of finding obstacles that are close, go through all obstacles
            //because the obstacle detection algorithm is using AABB anyway
            List<Obstacle> obstaclesThatAreClose = map.allObstacles;

            for (int i = 0; i < obstaclesThatAreClose.Count; i++)
            {
                Rectangle obstacleCorners = obstaclesThatAreClose[i].cornerPos;

                //Rectangle-rectangle intersection, which is here multiple triangle-triangle intersection tests
                if (Intersections.AreRectangleRectangleIntersecting(carRectangle, obstacleCorners))
                {
                    hasInvalidPosition = true;

                    return hasInvalidPosition;
                }
            }

            return hasInvalidPosition;
        }



        //
        // Find which cells are blocked by a rectangle if we know one of the cells
        //
        public static HashSet<IntVector2> FindCellsOccupiedByRectangle(Rectangle rectangle, IntVector2 occupiedCell, Map map)
        {
            //We can use a flood-fill algorithm to find the other cells that intersects with the obstacle
            HashSet<IntVector2> occupiedCells = new HashSet<IntVector2>();

            Queue<IntVector2> cellsToCheck = new Queue<IntVector2>();

            cellsToCheck.Enqueue(occupiedCell);

            int safety = 0;

            while (cellsToCheck.Count > 0)
            {
                if (safety > 100000)
                {
                    Debug.Log("Stuck in infinite loop when finding which cells are occupied by a rectangle");

                    break;
                }

                IntVector2 currentCell = cellsToCheck.Dequeue();

                occupiedCells.Add(currentCell);

                //Check neighbors
                IntVector2[] delta = HelpStuff.delta;

                for (int j = 0; j < delta.Length; j++)
                {
                    IntVector2 testCell = new IntVector2(currentCell.x + delta[j].x, currentCell.z + delta[j].z);

                    //Is this cell outside the map?
                    if (!(testCell.x > 0 && testCell.x < map.MapWidth && testCell.z > 0 && testCell.z < map.MapWidth))
                    {
                        continue;
                    }

                    //Is this cell in the list of occupied cells or a cell to check
                    if (occupiedCells.Contains(testCell) || cellsToCheck.Contains(testCell))
                    {
                        continue;
                    }

                    //Is this cell intersecting with the rectangle
                    Vector3 centerPos = map.cellData[testCell.x, testCell.z].centerPos;

                    if (ObstaclesDetection.IsCellIntersectingWithRectangle(centerPos, map.CellWidth, rectangle))
                    {
                        cellsToCheck.Enqueue(testCell);
                    }
                }
            }

            return occupiedCells;
        }




        //
        // If we know a cell is intersecting with a rectangle, find if any other cell thats intersecting with rectangle is an obstacle
        //
        public static bool FindIfCellBlockedByRectangleIsObstacle(Rectangle rectangle, IntVector2 occupiedCell, Map map)
        {
            Cell[,] cellData = map.cellData;
        
            //We can use a flood-fill algorithm to find the other cells that intersects with the obstacle
            HashSet<IntVector2> occupiedCells = new HashSet<IntVector2>();

            Queue<IntVector2> cellsToCheck = new Queue<IntVector2>();

            cellsToCheck.Enqueue(occupiedCell);

            int safety = 0;

            while (cellsToCheck.Count > 0)
            {
                if (safety > 100000)
                {
                    Debug.Log("Stuck in infinite loop when finding which cells are occupied by a rectangle");

                    break;
                }

                IntVector2 currentCell = cellsToCheck.Dequeue();

                occupiedCells.Add(currentCell);

                //Check neighbors
                IntVector2[] delta = HelpStuff.delta;

                for (int j = 0; j < delta.Length; j++)
                {
                    IntVector2 testCell = new IntVector2(currentCell.x + delta[j].x, currentCell.z + delta[j].z);

                    //Is this cell outside the map?
                    if (!(testCell.x > 0 && testCell.x < map.MapWidth && testCell.z > 0 && testCell.z < map.MapWidth))
                    {
                        continue;
                    }

                    //Is this cell in the list of occupied cells or a cell to check
                    if (occupiedCells.Contains(testCell) || cellsToCheck.Contains(testCell))
                    {
                        continue;
                    }

                    //Is this cell intersecting with the rectangle
                    Vector3 centerPos = cellData[testCell.x, testCell.z].centerPos;

                    if (ObstaclesDetection.IsCellIntersectingWithRectangle(centerPos, map.CellWidth, rectangle))
                    {
                        if (cellData[testCell.x, testCell.z].isObstacleInCell)
                        {
                            return true;
                        }

                        cellsToCheck.Enqueue(testCell);
                    }
                }
            }

            return false;
        }



        //
        // Methods that returns obstacles that are close to a car, because it's slow to check ALL obstacles
        //

        //Method 1 - Search through all obstacles to find which are close within a radius
        //private static List<Obstacle> FindCloseObstaclesWithinRadius(Vector3 pos, float radiusSqr)
        //{
        //    //The list with close obstacles
        //    List<Obstacle> closeObstacles = new List<Obstacle>();

        //    //Method 1 - Search through all obstacles to find which are close
        //    //The list with all obstacles in the map
        //    List<Obstacle> allObstacles = ObstaclesController.allObstacles;

        //    //Find close obstacles
        //    for (int i = 0; i < allObstacles.Count; i++)
        //    {
        //        float distSqr = (pos - allObstacles[i].centerPos).sqrMagnitude;

        //        //Add to the list of close obstacles if close enough
        //        if (distSqr < radiusSqr)
        //        {
        //            closeObstacles.Add(allObstacles[i]);
        //        }
        //    }

        //    return closeObstacles;
        //}



        //Method 2 - Find all obstacles the car might collide with by checking surrounding cells
        //might not be accurate because we dont alway knows how far from the car we should search
        //private static List<Obstacle> FindCloseObstaclesCell(Vector3 carPos)
        //{
        //    //The list with close obstacles
        //    List<Obstacle> closeObstacles = new List<Obstacle>();

        //    IntVector2 carCellPos = PathfindingController.ConvertCoordinateToCellPos(carPos);

        //    //Check an area of cells around the car's cell for obstacles
        //    //The car is 5 m long so search 3 m to each side?
        //    int searchArea = 3;

        //    for (int x = -searchArea; x <= searchArea; x++)
        //    {
        //        for (int z = -searchArea; z <= searchArea; z++)
        //        {
        //            IntVector2 cellPos = new IntVector2(carCellPos.x + x, carCellPos.z + z);

        //            //Is this cell within the map?
        //            if (PathfindingController.IsCellWithinGrid(cellPos))
        //            {
        //                //Add all obstacles from this list to the list of close obstacles
        //                List<Obstacle> obstaclesInCell = ObstaclesController.allObstaclesInEachCell[cellPos.x, cellPos.z];

        //                if (obstaclesInCell != null)
        //                {
        //                    for (int i = 0; i < obstaclesInCell.Count; i++)
        //                    {
        //                        //Might add the same obstacle more than one time, but maybe that's not a big problem?
        //                        closeObstacles.Add(obstaclesInCell[i]);
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    return closeObstacles;
        //}



        //Method 3 - Find all obstacles the car might collide with by using AABB
        public static List<Obstacle> FindCloseObstaclesAABB(Rectangle carRect, Map map)
        {
            List<Obstacle> closeObstacles = new List<Obstacle>();

            //The list with all obstacles in the map
            List<Obstacle> allObstacles = map.allObstacles;

            //Find close obstacles
            for (int i = 0; i < allObstacles.Count; i++)
            {
                Rectangle obsRect = allObstacles[i].cornerPos;

                //Are the AABB intersecting?
                if (Intersections.AreIntersectingAABB(carRect, obsRect))
                {
                    closeObstacles.Add(allObstacles[i]);
                }
            }

            return closeObstacles;
        }



        //
        // Find the closest point on an obstacles border to a point
        //
        public static Vector3 FindClosestPointOnObstacle(Obstacle o, Vector3 point)
        {
            Vector2 p = point.XZ();
        
            Vector2 FL = o.cornerPos.FL.XZ();
            Vector2 FR = o.cornerPos.FR.XZ();
            Vector2 BL = o.cornerPos.BL.XZ();
            Vector2 BR = o.cornerPos.BR.XZ();

            //Each obstacle has four edges, so we need to test them all
            Vector2 p1 = HelpStuff.GetClosestPointOnLineSegment(FL, FR, p);
            Vector2 p2 = HelpStuff.GetClosestPointOnLineSegment(FR, BR, p);
            Vector2 p3 = HelpStuff.GetClosestPointOnLineSegment(BR, BL, p);
            Vector2 p4 = HelpStuff.GetClosestPointOnLineSegment(BL, FL, p);

            //Can we speed up by using the normal of the line, and then the dot product to
            //not test lines whose normal point in the opposite direction?

            float d1 = (p1 - p).sqrMagnitude;
            float d2 = (p2 - p).sqrMagnitude;
            float d3 = (p3 - p).sqrMagnitude;
            float d4 = (p4 - p).sqrMagnitude;

            float closestDist = Mathf.Infinity;

            Vector2 closestPos = Vector2.zero;

            if (d1 < closestDist)
            {
                closestDist = d1;

                closestPos = p1;
            }
            if (d2 < closestDist)
            {
                closestDist = d2;

                closestPos = p2;
            }
            if (d3 < closestDist)
            {
                closestDist = d3;

                closestPos = p3;
            }
            if (d4 < closestDist)
            {
                closestDist = d4;

                closestPos = p4;
            }

            return closestPos.XYZ();
        }



        //
        // Is a cell intersecting with a rectangle
        //
        public static bool IsCellIntersectingWithRectangle(Vector3 cellPos, float cellSize, Rectangle obstacle)
        {
            bool isColliding = false;

            float halfCellSize = cellSize * 0.5f;

            Vector3 FL = new Vector3(cellPos.x - halfCellSize, cellPos.y, cellPos.z + halfCellSize);
            Vector3 FR = new Vector3(cellPos.x + halfCellSize, cellPos.y, cellPos.z + halfCellSize);
            Vector3 BL = new Vector3(cellPos.x - halfCellSize, cellPos.y, cellPos.z - halfCellSize);
            Vector3 BR = new Vector3(cellPos.x + halfCellSize, cellPos.y, cellPos.z - halfCellSize);

            Rectangle cell = new Rectangle(FL, FR, BL, BR);

            //Step 1. AABB
            if (Intersections.AreIntersectingAABB(obstacle, cell))
            {
                //Step 2. Triangle-triangle intersections
                if (Intersections.AreRectangleRectangleIntersecting(obstacle, cell))
                {
                    isColliding = true;
                }
            }

            return isColliding;
        }



        //
        // Figure out which cells the obstacle touch
        //
        public static void WhichCellsAreObstacle(Map map)
        {
            int mapWidth = map.MapWidth;


            //The border of the map is always obstacle
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    if (x == 0 || x == mapWidth - 1 || z == 0 || z == mapWidth - 1)
                    {
                        map.cellData[x, z].isObstacleInCell = true;
                    }
                }
            }


            //Loop through all obstacles
            List<Obstacle> allObstacles = map.allObstacles;

            for (int i = 0; i < allObstacles.Count; i++)
            {
                Rectangle obstacleRect = allObstacles[i].cornerPos;

                //Find a start cell from which we can find all other cells that intersects with this obstacle
                //The center of the obstacle is always within the map, so use it
                IntVector2 startCell = map.ConvertWorldToCell(obstacleRect.Center);

                if (!map.IsCellWithinGrid(startCell))
                {
                    Debug.Log("Obstacle center is outside of grid, so can determine which cells it intersects");    

                    continue;
                }

                //Find all cells blocked by this obstacle by using flood-fill
                HashSet<IntVector2> intersectingCells = ObstaclesDetection.FindCellsOccupiedByRectangle(obstacleRect, startCell, map);

                //Mark them as obstacle
                Cell[,] cellData = map.cellData;

                foreach (IntVector2 cell in intersectingCells)
                {
                    cellData[cell.x, cell.z].isObstacleInCell = true;

                    //Add which obstacle is in each cell
                    cellData[cell.x, cell.z].AddObstacleToCell(i);
                }
            }
        }
    }
}
