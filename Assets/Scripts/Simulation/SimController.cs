using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;
using SelfDrivingVehicle;



//Will make it easier if we have several self-driving cars than to contact the self-driving car directly
public class SimController : MonoBehaviour
{
    public static SimController current;

    //The self-driving vehicles
    public Transform car_selfDriving;
    public Transform semi_selfDriving;
    public Transform trailer_selfDriving;
    //The cars we will use as a marker so we can remeber where we clicked with mouse and at which angle
    //It should have no scripts nor colliders attached to it
    public Transform car_marker;
    public Transform semi_marker;
    public Transform semiWithTrailer_marker;
    //This is the one we move with the mouse, which is the same as above, so we can create it by using the above transform
    private Transform car_mouse;
    private Transform semi_mouse;
    private Transform semiWithTrailer_mouse;

    public enum VehicleTypes { None, Car, Semi, Semi_Trailer }

    private VehicleTypes activeVehicle = VehicleTypes.Car;

    //The vehicles start data
    private Vector3 startPos;
    private Quaternion startRot;



    void Awake()
    {
        current = this;

        //Create the other vehicles which we attach to the moise
        GameObject deadCarMouseObj = Instantiate(car_marker.gameObject) as GameObject;
        GameObject deadSemiMouseObj = Instantiate(semi_marker.gameObject) as GameObject;
        GameObject deadSemiWithTrailerMouseObj = Instantiate(semiWithTrailer_marker.gameObject) as GameObject;

        car_mouse = deadCarMouseObj.transform;
        semi_mouse = deadSemiMouseObj.transform;
        semiWithTrailer_mouse = deadSemiWithTrailerMouseObj.transform;

        startPos = car_selfDriving.position;
        startRot = car_selfDriving.rotation;
    }



    private void Start()
    {
        //The car is the model we start with
        ActivateVehicle(car_selfDriving, car_marker, car_mouse, VehicleTypes.Car);
    }



    //Change vehicle (will also reset if we select the current vehicle)
    public void ChangeVehicle(VehicleTypes vehicleType)
    {
        switch(vehicleType)
        {
            case VehicleTypes.Car:
                ActivateVehicle(car_selfDriving, car_marker, car_mouse, VehicleTypes.Car);
                break;
            case VehicleTypes.Semi:
                ActivateVehicle(semi_selfDriving, semi_marker, semi_mouse, VehicleTypes.Semi);
                break;
            case VehicleTypes.Semi_Trailer:
                ActivateSemiWithTrailer();
                break;
        }
    }



    private void DeActivateAllVehicles()
    {
        car_selfDriving.gameObject.SetActive(false);
        car_mouse.gameObject.SetActive(false);
        car_marker.gameObject.SetActive(false);

        semi_selfDriving.gameObject.SetActive(false);
        semi_mouse.gameObject.SetActive(false);
        semi_marker.gameObject.SetActive(false);

        trailer_selfDriving.gameObject.SetActive(false);
        semiWithTrailer_mouse.gameObject.SetActive(false);
        semiWithTrailer_marker.gameObject.SetActive(false);

        UIController.current.SetFoundPathText("");
    }



    private void ActivateVehicle(Transform vehicle, Transform marker, Transform mouse, VehicleTypes type)
    {
        DeActivateAllVehicles();

        //Move to the start pos
        vehicle.position = startPos;
        vehicle.rotation = startRot;

        //Make the models visible
        vehicle.gameObject.SetActive(true);
        mouse.gameObject.SetActive(true);
        //But hide the marker where we want to go
        marker.gameObject.SetActive(false);

        DisplayController.current.ResetGUI();

        StopCar();

        //Stop the car because we move it from the current position so it may still move
        Rigidbody rb = vehicle.GetComponent<Rigidbody>();

        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        activeVehicle = type;
    }



    //The semi with trailer consists of two gameobjects, so need a special method
    private void ActivateSemiWithTrailer()
    {
        //First activate the semi which will drag the trailer
        ActivateVehicle(semi_selfDriving, semi_marker, semi_mouse, VehicleTypes.Semi);

        //Hide the non-moving semis which were activated in the method above
        semi_mouse.gameObject.SetActive(false);
        semi_marker.gameObject.SetActive(false);

        //Move it to the correct position behind the semi
        trailer_selfDriving.position = semi_selfDriving.position;
        trailer_selfDriving.rotation = Quaternion.identity;

        //Make the models visible
        trailer_selfDriving.gameObject.SetActive(true);
        semiWithTrailer_mouse.gameObject.SetActive(true);
        //But hide the marker where we want to go
        semiWithTrailer_marker.gameObject.SetActive(false);

        Rigidbody rb = trailer_selfDriving.GetComponent<Rigidbody>();

        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        activeVehicle = VehicleTypes.Semi_Trailer;
    }



    //
    // Set and get
    //
    //We can have more than one car, such as a truck with trailer, etc

    //The self-driving car moving around
    public Transform GetSelfDrivingCarTrans()
    {
        switch(activeVehicle)
        {
            case VehicleTypes.Car:
                return car_selfDriving;
            case VehicleTypes.Semi:
                return semi_selfDriving;
            case VehicleTypes.Semi_Trailer:
                return semi_selfDriving;
        }

        return null;
    }

    //The transform we move to where we want the car to go
    public Transform GetCarShowingEndPosTrans()
    {
        switch (activeVehicle)
        {
            case VehicleTypes.Car:
                return car_marker;
            case VehicleTypes.Semi:
                return semi_marker;
            case VehicleTypes.Semi_Trailer:
                return semiWithTrailer_marker;
        }

        return null;
    }

    //The transform we move around with the mouse
    public Transform GetCarMouse()
    {
        switch (activeVehicle)
        {
            case VehicleTypes.Car:
                return car_mouse;
            case VehicleTypes.Semi:
                return semi_mouse;
            case VehicleTypes.Semi_Trailer:
                return semiWithTrailer_mouse;
        }

        return null;
    }

    public void SendPathToActiveCar(List<Node> wayPoints, bool isCircular)
    {
        Transform activeCar = GetSelfDrivingCarTrans();

        activeCar.GetComponent<VehicleController>().SendPathToCar(wayPoints, isCircular);
    }

    //Get data such as speed, length, etc belonging to the self-driving car
    public VehicleDataController GetActiveCarData()
    {
        Transform activeCar = GetSelfDrivingCarTrans();

        VehicleDataController carData = activeCar.GetComponent<VehicleController>().GetCarData();

        return carData;
    }


    //Get the trailer transform attached to the mouse if its active
    public Transform TryGetTrailerTransMouse()
    {
        if (activeVehicle == VehicleTypes.Semi_Trailer)
        {
            return semiWithTrailer_mouse.GetComponent<SemiWithTrailer>().trailerTrans;
        }

        return null;
    }

    //Get the trailer transform attached to the self-driving semi if its active
    public Transform TryGetTrailerTrans()
    {
        if (activeVehicle == VehicleTypes.Semi_Trailer)
        {
            return trailer_selfDriving;
        }

        return null;
    }

    //Get the trailer data if its active
    public VehicleDataController TryGetTrailerData()
    {
        if (activeVehicle == VehicleTypes.Semi_Trailer)
        {
            return trailer_selfDriving.GetComponent<VehicleDataController>();
        }

        return null;
    }

    //Stop the active car from driving
    public void StopCar()
    {
        SendPathToActiveCar(null, isCircular: false);

        Transform activeCar = GetSelfDrivingCarTrans();

        activeCar.GetComponent<VehicleController>().StopCar();
    }

    //Can we click (used for menu so we dont set a new car position when clicking on ui element)
    public bool CanClick()
    {
        //Hovering above UI
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        if (UIController.current != null && UIController.current.IsMenuActive())
        {
            return false;
        }

        return true;
    }
}

