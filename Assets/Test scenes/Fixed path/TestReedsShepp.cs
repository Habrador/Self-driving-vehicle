using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles.ReedsSheppPaths;

public class TestReedsShepp : MonoBehaviour
{
    public Transform startTrans;
    public Transform endTrans;



    private void OnDrawGizmos()
    {
        if (startTrans == null || endTrans == null)
        {
            return;
        }
    

        //Generate the Reeds-Shepp path with some turning radius
        //Get the shortest path
        Vector3 startPos = startTrans.position;
        Vector3 endPos = endTrans.position;

        float startRot = startTrans.rotation.eulerAngles.y * Mathf.Deg2Rad;
        float endRot = endTrans.rotation.eulerAngles.y * Mathf.Deg2Rad;


        //Get the shortest path
        List<RSCar> shortestPath = ReedsShepp.GetShortestPath(startPos, startRot, endPos, endRot, 12f, 1f, generateOneWp: false);


        DisplayPathNodes(shortestPath);
    }



    private void DisplayPathNodes(List<RSCar> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 p = path[i].pos;

            //Black node means the car is reversing
            Gizmos.color = path[i].gear == RSCar.Gear.Back ? Color.black : Color.white;

            Gizmos.DrawSphere(p, 0.1f);
        }
    }
}
