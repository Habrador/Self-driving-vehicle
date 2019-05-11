using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;

//Display the search grid with lines and quads
public class DisplayGrid : MonoBehaviour
{
    //Should we display the grid
    private bool shouldDisplayGrid;

    //The color of the grid - black is too dark
    private Color gridColor = new Color(0.4f, 0.4f, 0.4f);



    private void Start()
    {
        shouldDisplayGrid = false;
    }


    //Change if we should display the grid
    public void ChangeDisplay()
    {
        shouldDisplayGrid = !shouldDisplayGrid;
    }


    //Lines should be drawn in OnRenderObject and not in Update
    private void OnRenderObject()
    {
        if (shouldDisplayGrid)
        {
            DisplayGridWithLines();
        }
    }



    //Display the grid with lines
    private void DisplayGridWithLines()
    {
        Material lineMaterial = DisplayController.current.GetLineMaterial();

        //Use this material
        //If you dont call SetPass, then you'll get basically a random material (whatever was used before) which is not good
        lineMaterial.SetPass(0);

        GL.PushMatrix();

        //Set transformation matrix for drawing to match the transform
        GL.MultMatrix(transform.localToWorldMatrix);

        //Begin drawing 3D primitives
        GL.Begin(GL.LINES);

        GL.Color(gridColor);

        float xCoord = 0f;
        float zCoord = 0f;

        //The height is actually in local coordinates
        float lineHeight = DisplayController.gridHeight;

        int gridSize = Parameters.mapWidth;

        float cellSize = Parameters.cellWidth;

        for (int x = 0; x <= gridSize; x++)
        {
            //x
            Vector3 lineStartX = new Vector3(xCoord, lineHeight, zCoord);

            Vector3 lineEndX = new Vector3(xCoord, lineHeight, zCoord + (gridSize * cellSize));

            //Draw the line
            GL.Vertex(lineStartX);
            GL.Vertex(lineEndX);


            //z
            Vector3 lineStartZ = new Vector3(zCoord, lineHeight, xCoord);

            Vector3 lineEndZ = new Vector3(zCoord + (gridSize * cellSize), lineHeight, xCoord);

            //Draw the line
            GL.Vertex(lineStartZ);
            GL.Vertex(lineEndZ);

            xCoord += cellSize;
        }

        GL.End();

        GL.PopMatrix();
    }
}

