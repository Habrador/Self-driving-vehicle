using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathfindingForVehicles;


//Show the Hybrid A* and the final smooth path with line renderers
//Show the search tree with lines 
//Show the waypoints with lines
public class DisplayShortestPath : MonoBehaviour
{
    //Line renderers
    public LineRenderer lr_HybridAStar;
    public LineRenderer lr_SmoothPathForward;
    public LineRenderer lr_SmoothPathReverse;
    //Materials - we need the colors but the parameters will be reset if we change the script
    //so easier to use materials to get colors
    //These are for the search tree
    public Material lineForwardColor;
    public Material lineReverseColor;

        

    //All expanded nodes so we can display them with lines
    private List<Node> expandedNodes;

    //To display the nodes
    private List<Node> waypointNodes;

    //Should we display the search tree
    private bool shouldDisplaySearchTree = false;

    //Which search tree should we display?
    private bool displayNodeExpansionOrder = false;



    //Should we display the search tree
    public void ActivateDeactivateSearchTree(DisplayController.SearchTreeTypes type)
    {
        switch (type)
        {
            case DisplayController.SearchTreeTypes.None:
                shouldDisplaySearchTree = false;
                break;
            case DisplayController.SearchTreeTypes.Forward_Reverse:
                shouldDisplaySearchTree = true;
                displayNodeExpansionOrder = false;
                break;
            case DisplayController.SearchTreeTypes.Time_of_expansion:
                shouldDisplaySearchTree = true;
                displayNodeExpansionOrder = true;
                break;
        }
    }



    //Called before we create a new path
    public void Reset()
    {
        //Set these to 0 so we dont display the old lines if we fail to find a path
        lr_HybridAStar.positionCount = 0;
        lr_SmoothPathForward.positionCount = 0;
        lr_SmoothPathReverse.positionCount = 0;

        waypointNodes = null;
    }



    //Display the paths from the start to the goal
    public void DisplayDebug(List<Node> finalPathNodes, List<Node> smoothPathNodes)
    {
        //Display the final non-smooth path with a line using the rear wheel positions
        if (finalPathNodes != null)
        {
            List<Vector3> finalPath = new List<Vector3>();

            for (int i = 0; i < finalPathNodes.Count; i++)
            {
                finalPath.Add(finalPathNodes[i].rearWheelPos);
            }

            DisplayOnePath(lr_HybridAStar, finalPath, DisplayController.lineHeightShortestPath);
        }


        //Display the smooth path with a line using the front wheel positions
        if (smoothPathNodes != null)
        {
            List<Vector3> smoothPathForward = new List<Vector3>();

            for (int i = 0; i < smoothPathNodes.Count; i++)
            {
                smoothPathForward.Add(smoothPathNodes[i].frontWheelPos);
            }

            DisplayOnePath(lr_SmoothPathForward, smoothPathForward, DisplayController.lineHeightShortestSmoothPath);
        }


        //Display the smooth path with a line using the front wheel positions
        if (smoothPathNodes != null)
        {
            List<Vector3> smoothPathReverse = new List<Vector3>();

            for (int i = 0; i < smoothPathNodes.Count; i++)
            {
                smoothPathReverse.Add(smoothPathNodes[i].reverseWheelPos);
            }

            DisplayOnePath(lr_SmoothPathReverse, smoothPathReverse, DisplayController.lineHeightShortestSmoothPath);
        }


        this.waypointNodes = smoothPathNodes;
    }



    //Display the search tree
    public void DisplaySearchTree(List<Node> expandedNodes)
    {
        //Display the Hybrid A* search tree with GL lines
        //Cant do that with a custom method because it has to be done in OnRenderObject()
        this.expandedNodes = expandedNodes;
    }



    //Display one path with a line renderer
    private void DisplayOnePath(LineRenderer lineRenderer, List<Vector3> path, float height)
    {
        lineRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = path[i];
            //Need to have different heights to make the lines easier to see
            pos.y = height;

            lineRenderer.SetPosition(i, pos);
        }
    }



    //Lines have to be drawn in OnRenderObject and not in Update
    private void OnRenderObject()
    {
        //Display the waypoints with lines going straight up
        DisplayWaypoints();

        if (shouldDisplaySearchTree)
        {
            //Display the search tree with lines which is faster than using a lot of line renderers
            DisplaySearchTree();
        }
    }



    //Display the Hybrid A* search tree with lines
    private void DisplaySearchTree()
    {
        if (expandedNodes != null && expandedNodes.Count > 0)
        {   
            Material lineMaterial = DisplayController.current.GetLineMaterial();

            //Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();

            //Set transformation matrix for drawing to match our transform
            GL.MultMatrix(transform.localToWorldMatrix);

            //Use quad to get a thicker line
            GL.Begin(GL.LINES);

            for (int i = 0; i < expandedNodes.Count; i++)
            {
                Node thisNode = expandedNodes[i];

                //If the previous node is not null then we cant add a line
                //Is just the first node we add in Hybrid A*
                if (thisNode.previousNode == null)
                {
                    continue;
                }

                //Also need to change the height of the line depending on if we are going forward or reversing
                Vector3 startPos = thisNode.rearWheelPos;
                Vector3 endPos = thisNode.previousNode.rearWheelPos;

                //Set color depending on if we are driving forward or reversing
                if (thisNode.isReversing)
                {
                    startPos.y = DisplayController.lineHeightReverse;
                    endPos.y = DisplayController.lineHeightReverse;
                }
                else
                {
                    startPos.y = DisplayController.lineHeightForward;
                    endPos.y = DisplayController.lineHeightForward;
                }


                //Color
                if (displayNodeExpansionOrder)
                {
                    float grayScale = (float)i / (float)expandedNodes.Count;

                    //So red means the node was expanded early and green means late
                    Color color = new Color(1f - grayScale, grayScale, 0f);

                    GL.Color(color);
                }
                else
                {
                    Color color = thisNode.isReversing ? lineReverseColor.color : lineForwardColor.color;

                    GL.Color(color);
                }

                

                //Draw the line
                GL.Vertex(startPos);
                GL.Vertex(endPos);
            }

            GL.End();
            GL.PopMatrix();
        }
    }



    //Display the waypoints belonging to the smooth path with lines
    private void DisplayWaypoints()
    {
        if (waypointNodes != null && waypointNodes.Count > 0)
        {
            Material lineMaterial = DisplayController.current.GetLineMaterial();

            //Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();

            //Set transformation matrix for drawing to match our transform
            GL.MultMatrix(transform.localToWorldMatrix);

            //Use quad to get a thicker line
            GL.Begin(GL.LINES);

            //GL.Color(Color.white);

            for (int i = 0; i < waypointNodes.Count; i++)
            {
                //The line is going straight up
                Vector3 startPos = waypointNodes[i].frontWheelPos;

                startPos.y = 0f;

                Vector3 endPos = startPos + Vector3.up * DisplayController.waypointHeight;

                if (waypointNodes[i].isReversing)
                {
                    GL.Color(Color.white);
                }
                else
                {
                    GL.Color(Color.black);
                }

                //Draw the line
                GL.Vertex(startPos);
                GL.Vertex(endPos);
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}

