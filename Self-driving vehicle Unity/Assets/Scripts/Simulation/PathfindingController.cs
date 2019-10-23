using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using PathfindingForVehicles;
using SelfDrivingVehicle;

//Takes care of all pathfinding
public class PathfindingController : MonoBehaviour
{
    public static PathfindingController current;
    
    //Map data
    public Map map;

    //External scripts
    private DisplayController displayController;
        


    void Awake()
    {
        current = this;
        
        displayController = GetComponent<DisplayController>();
    }



    void Start()
    {
        //Create the map with cell data we need
        map = new Map(Parameters.mapWidth, Parameters.cellWidth);

        //Generate obstacles
        //Has to do it from this script or the obstacles might be created after this script has finished and mess things up
        //Need the start car so we can avoid adding obstacles on it
        //Car startCar = new Car(SimController.current.GetSelfDrivingCarTrans(), SimController.current.GetActiveCarData());
        Vector3 startPos = SimController.current.GetSelfDrivingCarTrans().position;

        int startTime = Environment.TickCount;
            
        this.GetComponent<ObstaclesGenerator>().InitObstacles(map, startPos);
            
        string timeText = DisplayController.GetDisplayTimeText(startTime, Environment.TickCount, "Generate obstacles and Voronoi diagram");

        Debug.Log(timeText);


        //Create the textures showing various stuff, such as the distance to nearest obstacle
        //debugController.DisplayCellObstacleIntersection(map);
        displayController.GenerateTexture(map, DisplayController.TextureTypes.Flowfield_Obstacle);
        displayController.GenerateTexture(map, DisplayController.TextureTypes.Voronoi_Field);
        displayController.GenerateTexture(map, DisplayController.TextureTypes.Voronoi_Diagram);
    }



    void Update()
    {
        //The menu is active or we are above ui element
        if (SimController.current != null && !SimController.current.CanClick())
        {
            return;
        }

        //Try to find a path if we press left mouse
        if (Input.GetMouseButtonDown(0))
        {
            //Check if the mouse car has a valid position
            if (HasMouseCarValidPosition())
            {            
                //Stop the self-driving car
                SimController.current.StopCar();

                //Wait for the self-driving car to stop before trying to find a path
                StartCoroutine(WaitForCarToStop());
            }
            else
            {
                Debug.Log("The end position is not valid!");

                UIController.current.SetFoundPathText("Path is blocked");
            }
        }
    }



    //Has the car we move with the mouse a valid position?
    private bool HasMouseCarValidPosition()
    {
        //Get the car transform we have attached to the mouse
        Car carMouse = new Car(SimController.current.GetCarMouse(), SimController.current.GetActiveCarData());

        //If the trailer is active we have to check it as well for collision
        bool hasTrailerValidPosition = true;

        Transform trailerTrans = SimController.current.TryGetTrailerTransMouse();

        if (trailerTrans != null)
        {
            Car trailerMouse = new Car(trailerTrans, SimController.current.TryGetTrailerData());

            if (!HasCarValidPosition(trailerMouse))
            {
                hasTrailerValidPosition = false;
            }
        }


        if (HasCarValidPosition(carMouse) && hasTrailerValidPosition)
        {
            return true;
        }

        return false;
    }



    //Wait for the car to stop before we generate a new path 
    //or it might have passed the start position of the path when the path has been generated
    IEnumerator WaitForCarToStop()
    {
        //Get the car's current speed
        VehicleDataController carDataController = SimController.current.GetActiveCarData();

        //Will continue looping until the car has a lower speed then 5 km/h
        while (Mathf.Abs(carDataController.GetSpeed_kmph()) > 5f)
        {
            yield return null;
        }

        //Now we need to check again if the target position is possible because we might have moved the mouse while the car was braking
        if (HasMouseCarValidPosition())
        {
            //Move the marker car to the end of the path so we know know where the path should end and at which heading
            Car carMouse = new Car(SimController.current.GetCarMouse(), SimController.current.GetActiveCarData());

            Transform carShowingEndPos = SimController.current.GetCarShowingEndPosTrans();

            carShowingEndPos.position = carMouse.carData.GetCenterPos(carMouse.rearWheelPos, carMouse.HeadingInRadians);
            carShowingEndPos.rotation = Quaternion.Euler(new Vector3(0f, carMouse.HeadingInDegrees, 0f));

            carShowingEndPos.gameObject.SetActive(true);


            //The car has stopped and the target is a valid positon, so try to generate a path
            StartCoroutine(GeneratePath(carMouse));

            yield break;
        }
        else
        {
            Debug.Log("The car cant move to this position");
        }
    }



    //Generate a path and send it to the car
    //We have to do it over some time to avoid a sudden stop in the simulation
    IEnumerator GeneratePath(Car goalCar)
    {
        //Get the start positions    

        //The self-driving car
        Car startCar = new Car(SimController.current.GetSelfDrivingCarTrans(), SimController.current.GetActiveCarData());

        //The trailer (if any)
        Car startTrailer = null;

        Transform trailerTrans = SimController.current.TryGetTrailerTrans();

        if (trailerTrans != null)
        {
            startTrailer = new Car(trailerTrans, SimController.current.TryGetTrailerData());
        }


        //First we have to check if the self-driving car is inside of the grid
        if (!map.IsPosWithinGrid(startCar.rearWheelPos))
        {
            Debug.Log("The car is outside of the grid");

            yield break;
        }

        //Which cell do we want to reach? We have already checked that this cell is valid
        IntVector2 targetCell = map.ConvertWorldToCell(goalCar.rearWheelPos);            

        //To measure time, is measured in tick counts
        int startTime = 0;
        //To display how long time each part took
        string timeText = "";



        //
        // Calculate Heuristics
        //

        //Calculate euclidean distance heuristic
        startTime = Environment.TickCount;

        HeuristicsController.EuclideanDistance(map, targetCell);

        timeText += DisplayController.GetDisplayTimeText(startTime, Environment.TickCount, "Euclidean Distance");

        yield return new WaitForSeconds(0.05f);


        //Calculate dynamic programing = flow field
        startTime = Environment.TickCount;

        HeuristicsController.DynamicProgramming(map, targetCell);

        timeText += DisplayController.GetDisplayTimeText(startTime, Environment.TickCount, "Dynamic Programming");

        yield return new WaitForSeconds(0.05f);


        //Calculate the final heuristics
        HeuristicsController.GenerateFinalHeuristics(map);



        //
        // Generate the shortest path with Hybrid A*
        //

        //List with all expanded nodes for debugging, so we can display the search tree
        List<Node> expandedNodes = new List<Node>();
            
        startTime = Environment.TickCount;
            
        //The output is finalPath and expandedNodes
        List<Node> finalPath = HybridAStar.GeneratePath(startCar, goalCar, map, expandedNodes, startTrailer);

        timeText += DisplayController.GetDisplayTimeText(startTime, Environment.TickCount, "Hybrid A Star");

        if (finalPath == null || finalPath.Count == 0)
        {
            UIController.current.SetFoundPathText("Failed to find a path!");
        }
        else
        {
            UIController.current.SetFoundPathText("Found a path!");
        }

        //
        // Smooth the path and send it to the car
        //

        //If we have found a path
        List<Node> smoothPath = null;

        if (finalPath != null && finalPath.Count > 0)
        {
            //Modify the path to make it easier for the vehicle to follow it
            //Step 1. Hybrid A* is using the rear wheel axle to generate the path, but it's easier for the car to follow it
            //if we also know where the path should have been if we had used the front axle
            Vector3 vehicleStartDir = SimController.current.GetSelfDrivingCarTrans().forward;
            
            Vector3 vehicleEndDir = SimController.current.GetCarShowingEndPosTrans().forward;

            ModifyPath.CalculateFrontAxlePositions(finalPath, startCar.carData, vehicleStartDir, vehicleEndDir, isMirrored: false);

            //When reversing we should track a path which is a path that goes along the front axle
            //but the front axle is mirrored along the rear axle
            ModifyPath.CalculateFrontAxlePositions(finalPath, startCar.carData, vehicleStartDir, vehicleEndDir, isMirrored: true);


            //Smooth the path by making it smoother and adding waypoints to make it easier for the car to follow the path 
            startTime = Environment.TickCount;

            smoothPath = ModifyPath.SmoothPath(finalPath, map, isCircular: false, isDebugOn: true);

            timeText += DisplayController.GetDisplayTimeText(startTime, Environment.TickCount, "Smooth path");


            //The car will immediatelly start following the path
            SimController.current.SendPathToActiveCar(smoothPath, isCircular: false);
        }



        //
        // Display the results
        //

        //Display how long time the different parts took
        Debug.Log(timeText);

        //Reset display
        displayController.ResetGUI();

        //Always display the search tree even if we havent found a path to the goal
        displayController.DisplaySearchTree(expandedNodes);

        //Generate the flow field heuristic texture
        displayController.GenerateTexture(map, DisplayController.TextureTypes.Flowfield_Target);

        //Display the different paths
        displayController.DisplayFinalPath(finalPath, smoothPath);


        yield return null;
    }



    //Check if the target car has a valid position
    private bool HasCarValidPosition(Car car)
    {
        bool hasValidPosition = false;

        if (!ObstaclesDetection.HasCarInvalidPosition(car.rearWheelPos, car.HeadingInRadians, car.carData, map))
        {
            hasValidPosition = true;
        }

        return hasValidPosition;
    }
}
