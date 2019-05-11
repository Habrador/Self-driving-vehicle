using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles;



namespace SelfDrivingVehicle
{
    //Stores specific car's data, so we can have several different cars
    public class VehicleDataController : MonoBehaviour
    {
        public enum VehicleTypes { Car, Semi, Trailer };

        public VehicleTypes vehicleType;

        public bool debugOn;

        [HideInInspector] public CarData carData;

        //Needed so we can get the car's current speed because we are calculating the speed in that script
        private VehicleController carController;



        private void Awake()
        {
            carController = GetComponent<VehicleController>();

            Vector3 carCenter = transform.position;

            //Get the data for this vehicle
            if (vehicleType == VehicleTypes.Car)
            {
                carData = GetCarData();
            }
            else if (vehicleType == VehicleTypes.Semi)
            {
                carData = GetSemiData();
            }
            else if (vehicleType == VehicleTypes.Trailer)
            {
                carData = GetTrailerData();
            }
        }



        //
        // Get methods
        //
        public Vector3 RearWheelPos(Transform carTrans)
        {
            Vector3 rearWheelPos = carTrans.position + carTrans.forward * carData.distanceTransformPositionToPivot;

            return rearWheelPos;
        }

        public Vector3 FrontWheelPos(Transform carTrans)
        {
            Vector3 frontWheelPos = RearWheelPos(carTrans) + carTrans.forward * carData.distancePivotToFrontWheels;

            return frontWheelPos;
        }

        public Vector3 MirroredFrontWheelPos(Transform carTrans)
        {
            Vector3 frontWheelPos = RearWheelPos(carTrans) - carTrans.forward * carData.distancePivotToFrontWheels;

            return frontWheelPos;
        }

        public float HeadingInDegrees(Transform carTrans)
        {
            float heading = carTrans.eulerAngles.y;

            return heading;
        }

        public float HeadingInRadians(Transform carTrans)
        {
            float heading = carTrans.eulerAngles.y * Mathf.Deg2Rad;

            return heading;
        }

        public Transform GetCarTransform()
        {
            return transform;
        }

        public float GetSpeed_kmph()
        {
            return carController.GetCarSpeed_kmph();
        }



        //
        // Data for each vehicle
        //
        private CarData GetCarData()
        {
            CarData carData = new CarData();

            //Engine power
            carData.maxMotorTorque = 500f;
            //Top speed in km/h. Has to be top speed because we need the top speed when making calculations
            carData.maxSpeed = 200f;
            //Brakes
            carData.maxBrakeTorque = 800f;
            //PID parameters to make it follow the path
            carData.PID_parameters = new PID_Parameters(30f, 0.0f, 20f);
            //Turning radius at some speed
            carData.turningRadius = 12f;

            //Data we need to set by measuring with DrawGizmos

            //The distance from the transform.position to the pivot point
            //The pivot point is always the rear wheels position
            carData.distanceTransformPositionToPivot = -1.43f;
            //The width of the car - including rear view mirrors
            carData.carWidth = 2.15f;
            //The distance to the rear wheels from the center, which is not always wheelbase / 2!
            carData.distancePivotToRearWheels = 0f;
            //The distance to the front wheels from the center, which is not always wheelbase / 2!
            carData.distancePivotToFrontWheels = 2.97f;
            //The distance to the back from the center, which is not always carLength / 2
            carData.distancePivotToRear = -1.1f;
            //The distance to the front from the center, which is not always carLength / 2
            carData.distancePivotToFront = 3.9f;
            //The distance from center to trailer attachment connector
            carData.distancePivotToTrailerAttachment = -1.1f;

            carData.canReverse = true;

            return carData;
        }



        private CarData GetSemiData()
        {
            CarData carData = new CarData();

            //Engine power
            carData.maxMotorTorque = 1500f;
            //Top speed in km/h
            carData.maxSpeed = 150f;
            //Brakes
            carData.maxBrakeTorque = 800f;
            //PID parameters to make it follow the path
            carData.PID_parameters = new PID_Parameters(30f, 0.0f, 20f);
            //Turning radius at some speed
            carData.turningRadius = 20f;

            //Data we need to set by measuring with DrawGizmos

            //The distance from the transform.position to the pivot point
            carData.distanceTransformPositionToPivot = -2.17f;
            //The width of the car - including rear view mirrors
            carData.carWidth = 3.05f;
            //The distance to the rear wheels from the center, which is not always wheelbase / 2!
            //The Semi has two rear wheels, so this is the distance to the center between these 
            carData.distancePivotToRearWheels = 0f;
            //The distance to the front wheels from the center, which is not always wheelbase / 2!
            carData.distancePivotToFrontWheels = 4.26f;
            //The distance to the back from the center, which is not always carLength / 2
            carData.distancePivotToRear = -1.55f;
            //The distance to the front from the center, which is not always carLength / 2
            carData.distancePivotToFront = 5.5f;
            //The distance from center to trailer attachment connector
            carData.distancePivotToTrailerAttachment = -0.15f;

            //Semi specific, the semi has a cabin that we need to test collision with against the trailer
            carData.cabinLength = 3.9f;

            carData.canReverse = true;

            return carData;
        }



        private CarData GetTrailerData()
        {
            CarData carData = new CarData();

            //Engine power
            //carData.maxMotorTorque = 1000f;
            //Top speed in km/h
            //carData.maxSpeed = 200f;
            //Brakes
            //carData.maxBrakeTorque = 800f;
            //PID parameters to make it follow the path
            //carData.PID_parameters = new PID_Parameters(10f, 0f, 5f);
            //Model S says on their website that the turning circle is 37 feet, which is the radius 
            //But this is when driving slow, so should maybe be higher here?
            //carData.turningRadius = 12f;

            //Data we need to set by measuring with DrawGizmos

            //The distance from the transform.position to the pivot point
            carData.distanceTransformPositionToPivot = -8.6f;
            //The width of the car - including rear view mirrors
            //Remember that the trailers wheel are wider than the container
            carData.carWidth = 2.95f;
            //The distance to the rear wheels from the center, which is not always
            //The Trailer has two rear wheels, so this is the distance to the center between these 
            carData.distancePivotToRearWheels = 0f;
            //The distance to the front wheels from the center, 
            //which in this case is where the trailer is attached to the drag vehicle
            carData.distancePivotToFrontWheels = 8.6f;
            //The distance to the back from the pivot
            carData.distancePivotToRear = -2.55f;
            //The distance to the front from the center
            carData.distancePivotToFront = 9.9f;
            //The distance from center to trailer attachment connector if we want to attach a trailer to this trailer
            carData.distancePivotToTrailerAttachment = -2.55f;

            return carData;
        }



        //
        // Debug
        //
        private void OnDrawGizmos()
        {
            if (!debugOn)
            {
                return;
            }
        
            //Get the data for this vehicle
            if (vehicleType == VehicleTypes.Car)
            {
                carData = GetCarData();
            }
            else if (vehicleType == VehicleTypes.Semi)
            {
                carData = GetSemiData();
            }
            else if (vehicleType == VehicleTypes.Trailer)
            {
                carData = GetTrailerData();
            }

            //Box of the car
            TestCarSize();

            //Distance to wheels
            TestWheelPositions();

            //Trailer hook attachment
            TestTrailerattachment();


            if (vehicleType == VehicleTypes.Semi)
            {
                TestCabinSize();
            }
        }



        private void TestCarSize()
        {
            Vector3 rearWheelPos = transform.position + transform.forward * carData.distanceTransformPositionToPivot;

            //Vector3 F = pivotPos + transform.forward * carData.distancePivotToFront;
            //Vector3 B = pivotPos + transform.forward * carData.distancePivotToRear;

            //Vector3 FL = F - transform.right * carData.carWidth * 0.5f;
            //Vector3 FR = F + transform.right * carData.carWidth * 0.5f;

            //Vector3 BL = B - transform.right * carData.carWidth * 0.5f;
            //Vector3 BR = B + transform.right * carData.carWidth * 0.5f;

            //Gizmos.DrawLine(FL, FR);
            //Gizmos.DrawLine(FR, BR);
            //Gizmos.DrawLine(BR, BL);
            //Gizmos.DrawLine(BL, FL);


            //Vector3 centerPos = (F + B) * 0.5f;

            float heading = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            Vector3 centerPos = carData.GetCenterPos(rearWheelPos, heading);

            //float heading = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            Rectangle size = CarData.GetCornerPositions(centerPos, heading, carData.carWidth, carData.CarLength);

            Gizmos.color = Color.white;

            Gizmos.DrawLine(size.FL, size.FR);
            Gizmos.DrawLine(size.FR, size.BR);
            Gizmos.DrawLine(size.BR, size.BL);
            Gizmos.DrawLine(size.BL, size.FL);
        }



        private void TestWheelPositions()
        {
            Vector3 pivotPos = transform.position + transform.forward * carData.distanceTransformPositionToPivot;

            Vector3 F = pivotPos + transform.forward * carData.distancePivotToFrontWheels;
            Vector3 B = pivotPos + transform.forward * carData.distancePivotToRearWheels;
            
            Vector3 FL = F - transform.right * carData.carWidth * 0.5f;
            Vector3 FR = F + transform.right * carData.carWidth * 0.5f;

            Vector3 BL = B - transform.right * carData.carWidth * 0.5f;
            Vector3 BR = B + transform.right * carData.carWidth * 0.5f;

            Gizmos.color = Color.black;

            Gizmos.DrawLine(FL, FR);
            Gizmos.DrawLine(BL, BR);
        }



        private void TestTrailerattachment()
        {
            Vector3 pivotPos = transform.position + transform.forward * carData.distanceTransformPositionToPivot;

            Vector3 B = pivotPos + transform.forward * carData.distancePivotToTrailerAttachment;

            Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(B, 0.3f);
        }



        private void TestCabinSize()
        {
            Vector3 pivotPos = transform.position + transform.forward * carData.distanceTransformPositionToPivot;

            Vector3 F = pivotPos + transform.forward * (carData.distancePivotToFront - carData.cabinLength);

            Vector3 FL = F - transform.right * carData.carWidth * 0.5f;
            Vector3 FR = F + transform.right * carData.carWidth * 0.5f;

            Gizmos.color = Color.white;

            Gizmos.DrawLine(FL, FR);
        }
    }
}
