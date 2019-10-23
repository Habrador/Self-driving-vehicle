using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;
using PathfindingForVehicles.ReedsSheppPaths;



//Display the shortest reeds shepp path between the car and the target
public class DisplayReedsShepp : MonoBehaviour
{
    //Line renderers - need maximum of 3 to display the path
    public LineRenderer[] lineArray;
    //Materials
    public Material lineForwardMaterial;
    public Material lineReverseMaterial;



    private void Update()
    {
        //Deactivate all line renderers
        for (int i = 0; i < lineArray.Length; i++)
        {
            lineArray[i].positionCount = 0;
        }

        //The cars we will display the shortest path between
        Transform startCarTrans = SimController.current.GetSelfDrivingCarTrans();

        Transform goalCarTrans = SimController.current.GetCarMouse();

        if (goalCarTrans != null && startCarTrans != null)
        {
            DisplayShortestPath(startCarTrans, goalCarTrans);
        }
    }



    //Get the shortest Reeds-Shepp path and display it
    private void DisplayShortestPath(Transform startCarTrans, Transform goalCarTrans)
    {
        Vector3 startPos = startCarTrans.position;

        float startHeading = startCarTrans.eulerAngles.y * Mathf.Deg2Rad;

        Vector3 goalPos = goalCarTrans.position;

        float goalHeading = goalCarTrans.eulerAngles.y * Mathf.Deg2Rad;

        float turningRadius = SimController.current.GetActiveCarData().carData.turningRadius;

        //Get the shortest Reeds-Shepp path
        List<RSCar> shortestPath = ReedsShepp.GetShortestPath(
            startPos, startHeading, goalPos, goalHeading, turningRadius, wpDistance: 1f, generateOneWp: false);

        //If we found a path
        if (shortestPath != null && shortestPath.Count > 1)
        {
            //Display the path with line renderers
            DisplayPath(shortestPath);
        }
    }



    //Display the Reed Shepp path with line renderers
    private void DisplayPath(List<RSCar> shortestPath)
    {
        List<Vector3> nodes = new List<Vector3>();

        //A path needs between 1 and 3 line renderers
        int lineArrayPos = 0;

        RSCar.Gear currentGear = shortestPath[0].gear;

        for (int i = 0; i < shortestPath.Count; i++)
        {
            nodes.Add(shortestPath[i].pos);

            //This means we have finished this segment of the path and should make a line renderer
            if (shortestPath[i].gear != currentGear)
            {
                bool isReversing = shortestPath[i - 1].gear == RSCar.Gear.Back ? true : false;

                AddPositionsToLineRenderer(nodes, lineArray[lineArrayPos], isReversing);

                //Restart with the next line
                lineArrayPos += 1;

                nodes.Clear();

                currentGear = shortestPath[i].gear;

                //So the lines connect
                nodes.Add(shortestPath[i].pos);
            }
        }

        //The last segment of the line
        bool isReversingLast = shortestPath[shortestPath.Count - 1].gear == RSCar.Gear.Back ? true : false;

        AddPositionsToLineRenderer(nodes, lineArray[lineArrayPos], isReversingLast);
    }




    //Display path positions with a line renderer
    private void AddPositionsToLineRenderer(List<Vector3> nodes, LineRenderer lineRenderer, bool isReversing)
    {
        if (nodes.Count > 0)
        {
            List<Vector3> linePositions = new List<Vector3>();

            //The height of the line
            float lineHeight = DisplayController.reedsSheppHeight;

            for (int i = 0; i < nodes.Count; i++)
            {
                Vector3 pos = new Vector3(nodes[i].x, lineHeight, nodes[i].z);

                linePositions.Add(pos);
            }

            Vector3[] linePositionsArray = linePositions.ToArray();

            lineRenderer.positionCount = linePositionsArray.Length;

            lineRenderer.SetPositions(linePositionsArray);

            if (isReversing)
            {
                lineRenderer.material = lineReverseMaterial;
            }
            else
            {
                lineRenderer.material = lineForwardMaterial;
            }
        }
    }
}

