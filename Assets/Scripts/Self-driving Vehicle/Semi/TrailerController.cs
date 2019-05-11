using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SelfDrivingVehicle
{
    public class TrailerController : MonoBehaviour
    {
        //Drags
        //Info about each axle, such as if the steering wheel is attached to it
        public List<AxleInfo> axleInfos;

        //The class that takes care of all visuals, such as rotating the wheels
        //private CarVisuals carStandard;


        private void Start()
        {
            //Update the rigidbody
            Rigidbody rb = GetComponent<Rigidbody>();

            rb.centerOfMass = rb.centerOfMass - new Vector3(0f, 0.8f, 0f);

            //rb.maxAngularVelocity = 2f;

            ////Needed to make it more standardized
            //CarData carData = new CarData();

            ////Add other data to carData, which stores all data needed for the car to easier send it to other scripts
            //carData.axleInfos = axleInfos;

            //carStandard = new CarVisuals(carData);

            foreach (AxleInfo axleInfo in axleInfos)
            {
                //if (axleInfo.steering)
                //{
                //    axleInfo.leftWheel.steerAngle = steeringAngle;
                //    axleInfo.rightWheel.steerAngle = steeringAngle;
                //}
                //if (axleInfo.motor)
                //{
                //    axleInfo.leftWheel.motorTorque = motorTorque;
                //    axleInfo.rightWheel.motorTorque = motorTorque;
                //}

                //Have to apply some small torque in the beginning or it will not start moving
                float motorTorque = 0.01f;

                axleInfo.leftWheel.motorTorque = motorTorque;
                axleInfo.rightWheel.motorTorque = motorTorque;

                //axleInfo.leftWheel.brakeTorque = brakeTorque;
                //axleInfo.rightWheel.brakeTorque = brakeTorque;

                //Make to wheel meshes rotate and move from suspension
                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }
        }



        private void Update()
        {
            //Update all methods that are the same no matter if the car is controller by AI or not
            //carStandard.CarStandardUpdate();

            //Add everything to the wheels
            foreach (AxleInfo axleInfo in axleInfos)
            {
                //if (axleInfo.steering)
                //{
                //    axleInfo.leftWheel.steerAngle = steeringAngle;
                //    axleInfo.rightWheel.steerAngle = steeringAngle;
                //}
                //if (axleInfo.motor)
                //{
                //    axleInfo.leftWheel.motorTorque = motorTorque;
                //    axleInfo.rightWheel.motorTorque = motorTorque;
                //}

                //axleInfo.leftWheel.brakeTorque = brakeTorque;
                //axleInfo.rightWheel.brakeTorque = brakeTorque;

                //Make to wheel meshes rotate and move from suspension
                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }
        }



        //Make the wheel meshes rotate and move from suspension
        public void ApplyLocalPositionToVisuals(WheelCollider collider)
        {
            if (collider.transform.childCount == 0)
            {
                return;
            }

            //Get the wheel mesh which is the only child to the wheel collider
            Transform visualWheel = collider.transform.GetChild(0);

            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);

            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }
    }
}
