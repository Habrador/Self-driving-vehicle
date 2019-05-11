using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;

//Takes care of all debugging
public class DisplayController : MonoBehaviour
{
    public static DisplayController current;

    //The object with all debug componets
    public GameObject displayObj;

    //Display the search tree and final path
    private DisplayShortestPath pathDisplay;
    //Display old car positions
    private DisplayOldCarPositions displayOldCarPositions;
    //The grid, showing each cell with lines
    private DisplayGrid displayGrid;
    //Obstacles, showing the flowfield
    private DisplayDataOnTexture displayDataOnTexture;

    //The line material used to display all lines, which can be the same for all
    private Material lineMaterial;

    //The different heights, so the objects dont intersect with each other
    //The paths
    public const float lineHeightForward = 0.02f;
    public const float lineHeightReverse = 0.01f;
    public const float lineHeightShortestPath = 0.03f;
    public const float lineHeightShortestSmoothPath = 0.04f;
    //The waypoints of the final path are displayed as lines going straight up
    public const float waypointHeight = 0.5f;
    //The obstacleflowfield quad 
    public const float flowFieldHeight = 0.005f;
    //The green rectangle around the cars old positions
    public const float oldPosHeight = 0.006f;
    //The height of the grid
    public const float gridHeight = 0.015f;
    //The height of the Reeds-Shepp paths 
    public const float reedsSheppHeight = 0.05f;

    //The different textures we want to display by using a dropdown
    public enum TextureTypes { None, Flowfield_Obstacle, Flowfield_Target, Voronoi_Field, Voronoi_Diagram }
    //The different search trees we want to display
    public enum SearchTreeTypes { None, Time_of_expansion, Forward_Reverse }



    private void Awake()
    {
        current = this;

        pathDisplay = displayObj.GetComponent<DisplayShortestPath>();

        displayOldCarPositions = displayObj.GetComponent<DisplayOldCarPositions>();

        displayGrid = displayObj.GetComponent<DisplayGrid>();

        displayDataOnTexture = displayObj.GetComponent<DisplayDataOnTexture>();
    }



    //Reset
    public void ResetGUI()
    {
        pathDisplay.Reset();

        displayOldCarPositions.Reset();
    }


    
    //
    // Send stuff to display
    //
    
    //Send path to display
    public void DisplayFinalPath(List<Node> finalPath, List<Node> smoothPath)
    {
        pathDisplay.DisplayDebug(finalPath, smoothPath);
    }

    //Send search tree
    public void DisplaySearchTree(List<Node> expandedNodes)
    {
        pathDisplay.DisplaySearchTree(expandedNodes);
    }



    //
    // Change if we should display something
    //

    public void ChangeDisplayGrid()
    {
        displayGrid.ChangeDisplay();
    }

    public void ChangeDisplaySearchTree(SearchTreeTypes type)
    {
        pathDisplay.ActivateDeactivateSearchTree(type);
    }

    public void ChangeDisplayCarPositions()
    {
        displayOldCarPositions.ChangeDisplay();
    }



    //
    // Display data on textures
    //

    public void DisplayTexture(TextureTypes textureType)
    {
        switch (textureType)
        {
            case TextureTypes.None:
                displayDataOnTexture.DisplayNoTexture();
                break;

            case TextureTypes.Flowfield_Obstacle:
                displayDataOnTexture.DisplayTexture_FlowField_DistanceToObstacle();
                break;

            case TextureTypes.Flowfield_Target:
                displayDataOnTexture.DisplayTexture_FlowField_DistanceToTarget();
                break;

            case TextureTypes.Voronoi_Field:
                displayDataOnTexture.DisplayTexture_VoronoiField();
                break;

            case TextureTypes.Voronoi_Diagram:
                displayDataOnTexture.DisplayTexture_VoronoiDiagram();
                break;
        }
    }



    //
    // Generate data on textures
    //

    public void GenerateTexture(Map map, TextureTypes textureType) 
    {
        switch (textureType)
        {
            case TextureTypes.Flowfield_Obstacle:
                displayDataOnTexture.GenerateTexture_FlowField_DistanceToObstacle(map);
                break;

            case TextureTypes.Flowfield_Target:
                displayDataOnTexture.GenerateTexture_FlowField_DistanceToTarget(map);
                break;

            case TextureTypes.Voronoi_Field:
                displayDataOnTexture.GenerateTexture_VoronoiField(map);
                break;

            case TextureTypes.Voronoi_Diagram:
                displayDataOnTexture.GenerateTexture_VoronoiDiagram(map);
                break;
        }
    }



    //
    // Create a material for the line used to display the grid
    //
    public Material GetLineMaterial()
    {
        if (!lineMaterial)
        {
            //Unity has a built-in shader that is useful for drawing simple colored things
            Shader shader = Shader.Find("Hidden/Internal-Colored");

            lineMaterial = new Material(shader);

            //So the material is not saved anywhere
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;

            //Turn on alpha blending
            //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            //Turn backface culling off
            //lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            //Turn off depth writes to make it transparent
            //lineMaterial.SetInt("_ZWrite", 0);

            //If you want the lines to render "above" the object
            //lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        return lineMaterial;
    }



    //
    // Help function to get how many seconds it took
    //
    public static string GetDisplayTimeText(int startTimeTicks, int endTimeTicks, string text)
    {
        int totalTicks = endTimeTicks - startTimeTicks;

        string output = GetDisplayTimeText(totalTicks, text);

        return output;
    }


    public static string GetDisplayTimeText(int ticks, string text)
    {
        //Convert from ticks to seconds
        float timeInSeconds = (float)ticks / 1000f;

        //Is not working because time is often less than 1 second so we need decimals
        //System.TimeSpan ts = System.TimeSpan.FromTicks(ticks);

        //float timeInSeconds = ts.Seconds;

        string output = "<b>" + text + ":</b> " + timeInSeconds + " s. ";

        return output;
    }


    //If we want to display something such as total nodes
    public static string GetDisplayText(string text, int amount, string units)
    {
        string output = "<b>" + text + ":</b> " + amount + units;

        return output;
    }



    //
    // Display vehicle positions along a path
    //
    public static void DisplayVehicleAlongPath(List<Node> path, CarData carData, Car trailer)
    {
        if (path == null || path.Count == 0)
        {
            return;
        }

        //Debug.Log(path.Count);

        for (int i = 0; i < path.Count; i++)
        {
            //if (i % 20 == 0)
            //{
            //    continue;
            //}

            Node node = path[i];
        
            //The car
            Vector3 carCenter = carData.GetCenterPos(node.rearWheelPos, node.heading);

            Rectangle carRect = CarData.GetCornerPositions(carCenter, node.heading, carData.carWidth, carData.CarLength);

            //DrawRect(carRect, Color.white, 90f);

            //Debug.Log(node.rearWheelPos);

            //Cabin 
            Vector3 cabinCenterPos = carData.GetSemiCabinCenter(node.rearWheelPos, node.heading);

            //DrawLine(carCenter, cabinCenterPos, Color.blue, 90f);

            Rectangle cabinRect = CarData.GetCornerPositions(cabinCenterPos, node.heading, carData.carWidth, carData.cabinLength);

            //DrawRect(cabinRect, Color.white, 90f);


            //The trailer
            if (trailer == null)
            {
                continue;
            }

            Vector3 trailerAttachmentPos = carData.GetTrailerAttachmentPoint(node.rearWheelPos, node.heading);

            //Now we need the trailer's rear-wheel pos based on the new heading
            CarData trailerData = trailer.carData;

            Vector3 trailerRearWheelPos = trailerData.GetTrailerRearWheelPos(trailerAttachmentPos, node.TrailerHeadingInRadians);

            //DrawLine(carCenter, trailerRearWheelPos, Color.blue, 90f);

            Vector3 trailerCenter = trailerData.GetCenterPos(trailerRearWheelPos, node.TrailerHeadingInRadians);

            Rectangle trailerRect = CarData.GetCornerPositions(trailerCenter, node.TrailerHeadingInRadians, trailerData.carWidth, trailerData.CarLength);

            DrawRect(trailerRect, Color.red, 90f);
            
        }
    }



    private static void DrawRect(Rectangle rect, Color color, float time)
    {
        Vector3 height = Vector3.up * 0.05f;
    
        Debug.DrawLine(rect.FL + height, rect.FR + height, color, time);
        Debug.DrawLine(rect.FR + height, rect.BR + height, color, time);
        Debug.DrawLine(rect.BR + height, rect.BL + height, color, time);
        Debug.DrawLine(rect.BL + height, rect.FL + height, color, time);
    }

    private static void DrawLine(Vector3 p1, Vector3 p2, Color color, float time)
    {
        Vector3 height = Vector3.up * 0.05f;

        Debug.DrawLine(p1 + height, p2 + height, color, time);
    }

}

