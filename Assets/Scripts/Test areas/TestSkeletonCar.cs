using UnityEngine;
using System.Collections;
using PathfindingForVehicles;

//Creates a skeleton car to test the math needed for the Hybrid A*
public class TestSkeletonCar : MonoBehaviour
{
    //Drags
    public GameObject cornerFL;
    public GameObject cornerFR;
    public GameObject cornerBL;
    public GameObject cornerBR;

    public GameObject trailerObj;
    public GameObject trailerObj2;
    public GameObject trailerObj3;


    //Data we need
    float wheelBase = 2.959f;
    float carSpeed = 10f;
    //The rear wheels position in relation to the attachment point
    float rearWheelOffset = -2.5f;
    float carWidth = 0.95f * 2f;
    float carLength = 2.44f * 2f;
    //Where is the trailer attached in relation to the pivot of the drag vehicle
    private float trailerAttachmentZOffset = -0.425f;


    void Start()
    {
        //If we have a trailer, move it to the attachment point
        if (trailerObj != null)
        {
            Vector3 dragVehiclePos = transform.position;

            float dragVehicleHeading = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            Vector3 attachmentPoint = CarData.GetLocalZPosition(dragVehiclePos, dragVehicleHeading, trailerAttachmentZOffset);

            trailerObj.transform.position = attachmentPoint;
        }

        if (trailerObj2 != null)
        {
            Vector3 dragVehiclePos = trailerObj.transform.position;

            float dragVehicleHeading = trailerObj.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            float trailerAttachmentZOffset = trailerObj2.GetComponent<TrailerTest>().trailerAttachmentZOffset;

            Vector3 attachmentPoint = CarData.GetLocalZPosition(dragVehiclePos, dragVehicleHeading, trailerAttachmentZOffset);

            trailerObj2.transform.position = attachmentPoint;
        }

        if (trailerObj3 != null)
        {
            Vector3 dragVehiclePos = trailerObj2.transform.position;

            float dragVehicleHeading = trailerObj2.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            float trailerAttachmentZOffset = trailerObj3.GetComponent<TrailerTest>().trailerAttachmentZOffset;

            Vector3 attachmentPoint = CarData.GetLocalZPosition(dragVehiclePos, dragVehicleHeading, trailerAttachmentZOffset);

            trailerObj3.transform.position = attachmentPoint;
        }
    }



    void Update()
    {
        TestDrive();
    }



    void TestDrive()
    {
        //Distance between the wheels (= wheelbase)
        float L = wheelBase;

        //Steering angle in radians
        float alpha = 20f * Mathf.Deg2Rad;
        //Heading direction in radians
        float theta = transform.eulerAngles.y * Mathf.Deg2Rad;

        //Manual control
        //Driving distance each update
        float d = carSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
        alpha *= Input.GetAxis("Horizontal");

        //Turning angle
        float beta = (d / L) * Mathf.Tan(alpha);

        //The position of the rear wheels
        Vector3 rearWheelPos = CarData.GetLocalZPosition(transform.position, theta, rearWheelOffset);

        //Get the new position of the rear wheels
        Vector3 newRearWheelPos = VehicleSimulationModels.CalculateNewPosition(theta, beta, d, rearWheelPos);

        //Get the new heading
        float newHeading = VehicleSimulationModels.CalculateNewHeading(theta, beta);

        //Get the new center position of the car
        Vector3 newCenterPos = CarData.GetLocalZPosition(newRearWheelPos, newHeading, rearWheelOffset * -1f);


        //Add the new center position to the car
        transform.position = newCenterPos;

        //Add the new heading to the car
        Vector3 currentRot = transform.rotation.eulerAngles;

        Vector3 newRotation = new Vector3(0f, newHeading * Mathf.Rad2Deg, 0f);

        transform.rotation = Quaternion.Euler(newRotation);


        //Update the trailer
        if (trailerObj != null)
        {
            float thetaOld = trailerObj.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            UpdateTrailer(theta, d, transform, trailerObj, trailerAttachmentZOffset);

            if (trailerObj2 != null)
            {
                TrailerTest trailerData2 = trailerObj2.transform.GetComponent<TrailerTest>();

                float thetaOld2 = trailerObj2.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

                UpdateTrailer(thetaOld, d, trailerObj.transform, trailerObj2, trailerData2.trailerAttachmentZOffset);

                if (trailerObj3 != null)
                {
                    TrailerTest trailerData3 = trailerObj3.transform.GetComponent<TrailerTest>();

                    //float thetaOld3 = trailerObj3.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

                    UpdateTrailer(thetaOld2, d, trailerObj2.transform, trailerObj3, trailerData3.trailerAttachmentZOffset);
                }
            }
        }
            


        UpdateCorners(newCenterPos, newHeading);
    }



    private void UpdateTrailer(float thetaOldCar, float D, Transform dragVehicle, GameObject trailer, float trailerAttachmentZOffset)
    {
        TrailerTest trailerData = trailer.transform.GetComponent<TrailerTest>();
        

        //Move the trailer to the attachment point - Should we use old or new values????
        Vector3 dragVehiclePos = dragVehicle.position;

        float dragVehicleHeading = dragVehicle.rotation.eulerAngles.y * Mathf.Deg2Rad;

        Vector3 attachmentPoint = CarData.GetLocalZPosition(dragVehiclePos, dragVehicleHeading, trailerAttachmentZOffset);

        trailer.transform.position = attachmentPoint;


        //Update rotation
        float theta = trailer.transform.eulerAngles.y * Mathf.Deg2Rad;

        //Distance to rear axle from connection point
        float d = Mathf.Abs(trailerData.rearWheelZOffset);

        float thetaNew = VehicleSimulationModels.CalculateNewTrailerHeading(theta, thetaOldCar, D, d);


        //Update the new values
        Vector3 newRotation = new Vector3(0f, thetaNew * Mathf.Rad2Deg, 0f);

        trailer.transform.rotation = Quaternion.Euler(newRotation);
    }



    //Update the corners to see if we can identify the coordinates from geometry
    void UpdateCorners(Vector3 carCenterPos, float carHeading)
    {
        Rectangle cornerPos = CarData.GetCornerPositions(carCenterPos, carHeading, carWidth, carLength);

        AddCoordinates(cornerFR, cornerPos.FR.x, cornerPos.FR.z);
        AddCoordinates(cornerFL, cornerPos.FL.x, cornerPos.FL.z);
        AddCoordinates(cornerBR, cornerPos.BR.x, cornerPos.BR.z);
        AddCoordinates(cornerBL, cornerPos.BL.x, cornerPos.BL.z);
    }



    //Add new coordinates to a gameobject
    void AddCoordinates(GameObject gameObj, float newX, float newZ)
    {
        Vector3 newPos = gameObj.transform.position;

        newPos.x = newX;
        newPos.z = newZ;

        gameObj.transform.position = newPos;
    }
}

