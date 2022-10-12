using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;
using SelfDrivingVehicle;



public class TestFollowPathController : MonoBehaviour 
{
    public Transform waypointsParent;

    public Transform carTrans;

    //To test heading between waypoints calculations
    public Transform obj1;
    public Transform obj2;



    private void Start()
    {
        //Get the waypoints and send them to the car
        List<Node> nodes = GetAllWaypoints();

        //Send the nodes to the car
        FollowPath carPathScript = carTrans.GetComponent<FollowPath>();

        carPathScript.SetPath(nodes, isCircular: true);
    }



    private List<Node> GetAllWaypoints()
    {
        if (waypointsParent == null)
        {
            return null;
        }
    
        //This array will hold all children
        Vector3[] waypoints = new Vector3[waypointsParent.childCount];

        //Fill the array
        int childCount = 0;
        foreach (Transform child in waypointsParent)
        {
            waypoints[childCount] = child.transform.position;
            childCount += 1;
        }

        List<Vector3> waypointsList = new List<Vector3>(waypoints);

        //Add waypoints
        waypointsList = SmoothPathSimple(waypointsList);
        waypointsList = SmoothPathSimple(waypointsList);


        //Standardize
        List<Node> path = new List<Node>();

        foreach (Vector3 pos in waypointsList)
        {
            path.Add(new Node(null, pos, 0f, false));
        }


        //Smooth
        //path = ModifyPath.SmoothPath(path, null, isCircular: true, isDebugOn: false);

        return path;
    }



    //Smooth the path by taking the average of the surrounding nodes
    private List<Vector3> SmoothPathSimple(List<Vector3> waypoints)
    {
        //First add new waypoints between the old to make it easier to smooth the path
        waypoints = AddExtraWaypoints(waypoints);
        waypoints = AddExtraWaypoints(waypoints);


        //Smooth
        List<Vector3> waypointsSmooth = new List<Vector3>();

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 p1 = waypoints[i];

            int iPlusOne = HelpStuff.ClampListIndex(i + 1, waypoints.Count);
            int iMinusOne = HelpStuff.ClampListIndex(i - 1, waypoints.Count);

            Vector3 p0 = waypoints[iMinusOne];
            Vector3 p2 = waypoints[iPlusOne];

            waypointsSmooth.Add((p0 + p1 + p2) / 3f);
        }

        return waypointsSmooth;
    }



    //Add one waypoint between each other waypoint
    private List<Vector3> AddExtraWaypoints(List<Vector3> waypoints)
    {
        List<Vector3> waypointsExtra = new List<Vector3>();

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 p1 = waypoints[i];

            int iPlusOne = HelpStuff.ClampListIndex(i + 1, waypoints.Count);

            Vector3 p2 = waypoints[iPlusOne];

            waypointsExtra.Add(p1);

            waypointsExtra.Add((p1 + p2) * 0.5f);
        }

        return waypointsExtra;
    }



    private void OnDrawGizmos()
    {
        //Display the connection between the waypoints, so we know they are sorted in the correct order
        List<Node> nodes = GetAllWaypoints();

        Gizmos.color = Color.black;

        //So the line doesnt intersect with the ground
        Vector3 heightChange = new Vector3(0f, 0.01f, 0f);

        if (nodes != null && nodes.Count > 1)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
            
                Vector3 p1 = node.rearWheelPos + heightChange;

                int iPlusOne = HelpStuff.ClampListIndex(i + 1, nodes.Count);

                Vector3 p2 = nodes[iPlusOne].rearWheelPos + heightChange;

                Gizmos.DrawLine(p1, p2);

                //First wp should be bigger so we know where it starts
                if (i == 0)
                {
                    Gizmos.DrawSphere(node.rearWheelPos, 0.4f);
                }
                else if (i == 1)
                {
                    Gizmos.DrawSphere(node.rearWheelPos, 0.3f);
                }
                else
                {
                    Gizmos.DrawSphere(node.rearWheelPos, 0.2f);
                }
                
            }
        }



        //Calculate heading (y rotation) if you have two waypoints
        //if (obj1 != null && obj2 != null)
        //{
        //    Gizmos.color = Color.blue;

        //    Gizmos.DrawLine(obj1.position, obj1.position + Vector3.forward * 3f);
        //    Gizmos.DrawLine(obj1.position, obj2.position);


        //    //The real angle we want to duplicate with our own calculations
        //    obj1.LookAt(obj2);
            
        //    //Calculate the same y angle as obj1 has
        //    Vector3 from = Vector3.forward;

        //    Vector3 to = (obj2.position - obj1.position);

        //    float heading = HelpStuff.CalculateAngle(from, to);


        //    Debug.Log(heading + " " + obj1.eulerAngles.y);
        //}
    }
}
