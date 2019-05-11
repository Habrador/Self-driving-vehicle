using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PathfindingForVehicles;
using SelfDrivingVehicle;
using System;

//Takes care of all user input
public class UIController : MonoBehaviour
{
    public static UIController current;

    //Drags
    public GameObject aboutMenuObj;
    public GameObject descriptionMenuObj;
    public Text speedText;
    public Dropdown dropdownSearchTree;
    public Dropdown dropdownDisplayTexture;
    public Dropdown dropdownChangeVehicle;
    public Text ifFoundPathText;



    private void Awake()
    {
        current = this;    
    }



    void Start()
    {
        aboutMenuObj.SetActive(false);
        descriptionMenuObj.SetActive(false);

        StartCoroutine(UpdateSpeedText());

        ifFoundPathText.text = "";

        //Init the dropdowns
        //Search tree
        List<string> searchTreeOptionsStrings = new List<string>();

        int numOfEnums1 = Enum.GetNames(typeof(DisplayController.SearchTreeTypes)).Length;

        for (int i = 0; i < numOfEnums1; i++)
        {
            DisplayController.SearchTreeTypes word = (DisplayController.SearchTreeTypes)i;

            searchTreeOptionsStrings.Add(word.ToString());
        }

        dropdownSearchTree.AddOptions(searchTreeOptionsStrings);


        //Texture
        List<string> textureOptionsStrings = new List<string>();

        int numOfEnums2 = Enum.GetNames(typeof(DisplayController.TextureTypes)).Length;

        for (int i = 0; i < numOfEnums2; i++)
        {
            DisplayController.TextureTypes word = (DisplayController.TextureTypes)i;

            textureOptionsStrings.Add(word.ToString());
        }

        dropdownDisplayTexture.AddOptions(textureOptionsStrings);


        //Vehicles
        List<string> vehicleStrings = new List<string>();

        int numOfEnums3 = Enum.GetNames(typeof(SimController.VehicleTypes)).Length;

        for (int i = 0; i < numOfEnums3; i++)
        {
            SimController.VehicleTypes word = (SimController.VehicleTypes)i;

            vehicleStrings.Add(word.ToString());
        }

        dropdownChangeVehicle.AddOptions(vehicleStrings);
    }



    void Update()
    {
        //Show hide menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (aboutMenuObj.activeInHierarchy)
            {
                aboutMenuObj.SetActive(false);
                descriptionMenuObj.SetActive(false);
            }
            else
            {
                aboutMenuObj.SetActive(true);
                descriptionMenuObj.SetActive(true);
            }
        }
    }

    

    //
    // Buttons and toggles
    //

    //Is the menu active
    public bool IsMenuActive()
    {
        return aboutMenuObj.activeInHierarchy;
    }

    //Quit the game
    public void QuitGame()
    {
        Application.Quit();
    }

    //Check boxes
    public void DisplayGrid()
    {
        if (DisplayController.current != null)
        {
            DisplayController.current.ChangeDisplayGrid();
        }
    }

    public void DisplayOldCarPositions()
    {
        if (DisplayController.current != null)
        {
            DisplayController.current.ChangeDisplayCarPositions();
        }
    }

    //Drop downs
    //When choosing this method from the inspector, pick the uppermost one below "Dynamic int" and not "Static parameters"
    public void DisplaySearchTree(int index)
    {
        if (DisplayController.current == null)
        {
            return;
        }

        //Which texture did we select in the dropdown
        DisplayController.SearchTreeTypes type = (DisplayController.SearchTreeTypes)index;

        DisplayController.current.ChangeDisplaySearchTree(type);
    }

    //When choosing this method from the inspector, pick the uppermost one below "Dynamic int" and not "Static parameters"
    public void DisplayTexture(int index)
    {
        if (DisplayController.current == null)
        {
            return;
        }

        //Which texture did we select in the dropdown
        DisplayController.TextureTypes type = (DisplayController.TextureTypes)index;

        DisplayController.current.DisplayTexture(type);
    }

    //Change vehicle (will also reset if we select the current vehicle)
    public void ChangeVehicle(int index)
    {
        if (SimController.current == null)
        {
            return;
        }

        //Which vehicle did we select in the dropdown
        SimController.VehicleTypes type = (SimController.VehicleTypes)index;

        SimController.current.ChangeVehicle(type);


        //Now we need to select the first enum again which is none, because if we have selected the car
        //and try to select it again from the drop down, it will not work
        dropdownChangeVehicle.value = 0;
    }



    //The text box where we display if we found a path or not
    public void SetFoundPathText(string text)
    {
        ifFoundPathText.text = text;
    }



    //
    // Update speed
    //

    //Dont need to update speed every frame
    private IEnumerator UpdateSpeedText()
    {
        while (true)
        {
            if (SimController.current != null)
            {
                //Get the speed in km/h
                VehicleDataController dataController = SimController.current.GetActiveCarData();

                float speed = dataController.GetSpeed_kmph();

                //float speed = 0f;

                //Round
                int speedInt = Mathf.RoundToInt(speed);

                //Display the speed
                speedText.text = speedInt + " km/h";
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
}

