using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;

//Translate some data (such as a flow field) on the grid to a color and display it on a quad
public class DisplayDataOnTexture : MonoBehaviour
{
    //Display the texture on this quad
    public Transform quad;

    //The textures we generate once when we change some data
    private Texture texture_DistanceToObstacle;
    private Texture texture_DistanceToTarget;
    private Texture texture_VoronoiField;
    private Texture texture_VoronoiDiagram;

    //Need an enum to keep track of which texture is visible
    //so when we generate a new texture when the simulation is running, we can assign it
    //private enum VisibleTexture { None, FF_Obstacle, FF_Target, VoronoiField, VoronoiDiagram }

    private DisplayController.TextureTypes visibleTexture;



    private void Start()
    {
        //Resize the quads to fit the grid
        int mapWidth = Parameters.mapWidth;

        Vector3 centerPos = new Vector3(mapWidth * 0.5f, DisplayController.flowFieldHeight, mapWidth * 0.5f);

        Vector3 scale = new Vector3(mapWidth, mapWidth, 1f);

        //Add the data to the quads
        quad.position = centerPos;

        quad.localScale = scale;

        //Deactivate it
        quad.gameObject.SetActive(false);

        visibleTexture = DisplayController.TextureTypes.None;
    }



    //
    // Display a texture on the quad
    //

    private bool DisplayTexture(Texture texture)
    {
        //Add the texture to the quad
        if (texture != null)
        {
            quad.GetComponent<MeshRenderer>().material.mainTexture = texture;

            ActivateQuad();

            return true;
        }
        else
        {
            Debug.Log("No texture has been created");

            return false;
        }
    }

    //Texture: None
    public void DisplayNoTexture()
    {
        DeActivateQuad();

        visibleTexture = DisplayController.TextureTypes.None;
    }

    //Texture: Distance to closest obstacle
    public void DisplayTexture_FlowField_DistanceToObstacle()
    {
        if (DisplayTexture(texture_DistanceToObstacle))
        {
            visibleTexture = DisplayController.TextureTypes.Flowfield_Obstacle;
        }
    }

    //Texture: Distance to target
    public void DisplayTexture_FlowField_DistanceToTarget()
    {
        if (DisplayTexture(texture_DistanceToTarget))
        {
            visibleTexture = DisplayController.TextureTypes.Flowfield_Target;
        }
    }

    //Texture: Voronoi field
    public void DisplayTexture_VoronoiField()
    {
        if (DisplayTexture(texture_VoronoiField))
        {
            visibleTexture = DisplayController.TextureTypes.Voronoi_Field;
        }
    }

    //Texture: Voronoi field
    public void DisplayTexture_VoronoiDiagram()
    {
        if(DisplayTexture(texture_VoronoiDiagram))
        {
            visibleTexture = DisplayController.TextureTypes.Voronoi_Diagram;
        }
    }



    //
    // Generate textures when some data has changed
    //

    //Texture: Distance to closest obstacle
    public void GenerateTexture_FlowField_DistanceToObstacle(Map map)
    {
        int mapWidth = map.MapWidth;

        float[,] flowField = new float[mapWidth, mapWidth];

        bool[,] isObstacle = new bool[mapWidth, mapWidth];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                flowField[x, z] = map.cellData[x, z].distanceToClosestObstacle;

                isObstacle[x, z] = map.cellData[x, z].isObstacleInCell;
            }
        }

        texture_DistanceToObstacle = GenerateFlowFieldTexture(flowField, isObstacle, isBlackWhite: false);
    }



    //Texture: Distance to the target we want to drive to
    public void GenerateTexture_FlowField_DistanceToTarget(Map map)
    {
        int mapWidth = map.MapWidth;

        float[,] flowField = new float[mapWidth, mapWidth];

        bool[,] isObstacle = new bool[mapWidth, mapWidth];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                flowField[x, z] = map.cellData[x, z].distanceToTarget;

                isObstacle[x, z] = map.cellData[x, z].isObstacleInCell;
            }
        }

        texture_DistanceToTarget = GenerateFlowFieldTexture(flowField, isObstacle, isBlackWhite: false);

        //If this texture is visible, we should assign the texture to the material
        if (visibleTexture == DisplayController.TextureTypes.Flowfield_Target)
        {
            DisplayTexture_FlowField_DistanceToTarget();
        }
    }



    //Texture: Voronoi field
    public void GenerateTexture_VoronoiField(Map map)
    {
        int mapWidth = map.MapWidth;

        float[,] flowField = new float[mapWidth, mapWidth];

        bool[,] isObstacle = new bool[mapWidth, mapWidth];


        Cell[,] cellData = map.cellData;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                flowField[x, z] = cellData[x, z].voronoiFieldCell.voronoiFieldValue;

                isObstacle[x, z] = cellData[x, z].isObstacleInCell;
            }
        }

        texture_VoronoiField = GenerateFlowFieldTexture(flowField, isObstacle, isBlackWhite: true);
    }



    //Texture: Voronoi diagram
    public void GenerateTexture_VoronoiDiagram(Map map)
    {
        int mapWidth = map.MapWidth;

        Cell[,] cellData = map.cellData;

        //To get the same random colors each time or they will constantly change color
        Random.InitState(0);

        //Find how many regions we have
        int regions = -1;
        for (int z = 0; z < map.MapWidth; z++)
        {
            for (int x = 0; x < map.MapWidth; x++)
            {
                if (cellData[x, z].voronoiFieldCell.region > regions)
                {
                    regions = cellData[x, z].voronoiFieldCell.region;
                }
            }
        }

        //Regions start at 0 so we have to add 1
        regions += 1;

        //Generate the random colors for each region
        List<Color> regionColors = new List<Color>();

        for (int i = 0; i < regions; i++)
        {
            regionColors.Add(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
        }

        //Create a texture on which we will display the information
        Texture2D texture = GenerateNewDebugTexture(mapWidth);

        //Generate the colors
        //More efficient to generate the colors once and then add the array to the texture
        Color[] colors = new Color[mapWidth * mapWidth];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                int region = cellData[x, z].voronoiFieldCell.region;

                Color thisColor = regionColors[region];

                colors[z * mapWidth + x] = thisColor;
            }
        }

        //Add the colors to the texture
        texture.SetPixels(colors);

        texture.Apply();

        texture_VoronoiDiagram = texture;
    }



    //Generate a flowfield texture
    private Texture GenerateFlowFieldTexture(float[,] flowField, bool[,] isObstacle, bool isBlackWhite)
    {
        //Assume the flow field is always square
        int mapWidth = flowField.GetLength(0);

        //Create a texture on which we will display the information
        Texture2D texture = GenerateNewDebugTexture(mapWidth);

        //Debug flow field by changing the color of the cells 
        //To display the grid with a grayscale, we need the max distance to the node furthest away
        float maxDistance = FlowField.GetMaxDistance(flowField);


        //Generate the colors
        //More efficient to generate the colors once and then add the array to the texture
        Color[] colors = new Color[mapWidth * mapWidth];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                //Default, meaning unreachable cell
                Color thisColor = Color.blue;

                float distance = flowField[x, z];

                if (isObstacle[x, z])
                {
                    thisColor = Color.black;
                }
                //If this is not an obstacle or a cell that was unreachable in the flowfield
                else if (distance < float.MaxValue)
                {
                    float rgb = 1f - (distance / maxDistance);

                    if (isBlackWhite)
                    {
                        thisColor = new Vector4(rgb, rgb, rgb, 1.0f);
                    }
                    //Red-green scale
                    else
                    {
                        thisColor = new Vector4(1f - rgb, rgb, 0f, 1.0f);
                    }
                }

                colors[z * mapWidth + x] = thisColor;
            }
        }


        //Add the colors to the texture
        texture.SetPixels(colors);

        texture.Apply();


        return texture;
    }



    //Create a texture on which we will display the information
    private Texture2D GenerateNewDebugTexture(int mapWidth)
    {
        //Create a texture on which we will display the information
        Texture2D debugTexture = new Texture2D(mapWidth, mapWidth);

        //Change texture settings to make it look better
        debugTexture.filterMode = FilterMode.Point;
        //debugTexture.filterMode = FilterMode.Bilinear;

        debugTexture.wrapMode = TextureWrapMode.Clamp;

        return debugTexture;
    }



    //Active/deactivate the quad
    private void ActivateQuad()
    {
        quad.gameObject.SetActive(true);
    }

    private void DeActivateQuad()
    {
        quad.gameObject.SetActive(false);
    }
}

