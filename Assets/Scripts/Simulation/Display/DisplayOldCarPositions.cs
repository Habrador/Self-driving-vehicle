using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathfindingForVehicles;
using SelfDrivingVehicle;


//Show green rectangles so we know where the car has driven and how well it follows the path
public class DisplayOldCarPositions : MonoBehaviour
{
    //The material used to display the lines
    public Material lineMaterial;

    //Store the car's position here so we know if it has moved
    private Vector3 lastPos;

    //All old car positions
    private List<Rectangle> oldCarPositions = new List<Rectangle>();

    //Should we display the lines?
    private bool shouldDisplayLines = true;



    private void Update()
    {
        AddSquare();
    }



    //Reset the list of old postions
    public void Reset()
    {
        oldCarPositions.Clear();
    }



    //Change if we should display the lines
    public void ChangeDisplay()
    {
        shouldDisplayLines = !shouldDisplayLines;
    }



    //Add a square showing the car's position if it has moved
    private void AddSquare()
    {
        Transform carTrans = SimController.current.GetSelfDrivingCarTrans();

        VehicleDataController carDataController = carTrans.GetComponent<VehicleDataController>();

        //How far has the car driven since last saved position?
        float distSqr = (lastPos - carTrans.position).sqrMagnitude;

        //How far should the car drive before we add a rectangle
        float dist = 2f;

        if (distSqr > dist * dist)
        {
            //Find the corner coordinates of the car at this position
            Vector3 F = carDataController.RearWheelPos(carTrans) + carTrans.forward * carDataController.carData.distancePivotToFront;
            Vector3 B = carDataController.RearWheelPos(carTrans) + carTrans.forward * carDataController.carData.distancePivotToRear;

            Vector3 center = (F + B) * 0.5f;

            float carWidth = carDataController.carData.carWidth;
            float carLength = carDataController.carData.CarLength;

            float heading = carTrans.eulerAngles.y * Mathf.Deg2Rad;

            Rectangle rect = CarData.GetCornerPositions(center, heading, carWidth, carLength);

            //Set the height of the rectangle
            rect.FL.y = DisplayController.oldPosHeight;
            rect.FR.y = DisplayController.oldPosHeight;
            rect.BL.y = DisplayController.oldPosHeight;
            rect.BR.y = DisplayController.oldPosHeight;

            //Save this position
            oldCarPositions.Add(rect);

            lastPos = carTrans.position;
        }
    }



    //Display all old positions with lines
    private void OnRenderObject()
    {
        if (!shouldDisplayLines)
        {
            return;
        }
    
        //Apply the line material
        //If you dont call SetPass, then you'll get basically a random material (whatever was used before) which is not good
        lineMaterial.SetPass(0);

        GL.PushMatrix();

        //Set transformation matrix for drawing to match the transform
        //This will make us draw everything in local space
        //but not needed because the transform is already at 0
        //GL.MultMatrix(transform.localToWorldMatrix);

        //Draw the rectangles
        GL.Begin(GL.LINES);

        for (int i = 0; i < oldCarPositions.Count; i++)
        {
            //The rectangle 
            Rectangle rect = oldCarPositions[i];

            //Draw the rectangle, which consists of 4 line segments
            GL.Vertex(rect.FL);
            GL.Vertex(rect.FR);

            GL.Vertex(rect.FR);
            GL.Vertex(rect.BR);

            GL.Vertex(rect.BR);
            GL.Vertex(rect.BL);

            GL.Vertex(rect.BL);
            GL.Vertex(rect.FL);
        }

        GL.End();

        GL.PopMatrix();
    }
}

