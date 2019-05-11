using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathfindingForVehicles;



namespace SelfDrivingVehicle
{
    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        //Is this wheel powered by a motor?
        public bool motor;
        //Can this wheel steer?
        public bool steering;
    }



    //From http://docs.unity3d.com/Manual/WheelColliderTutorial.html
    public class VehicleController : MonoBehaviour
    {
        public List<AxleInfo> axleInfos;

        //An nimation curve that will deterine the wheel angle as a function of speed
        public AnimationCurve wheelAngleCurve;

        //Speed calculations
        //Speed in km/h
        private float currentSpeed = 0f;
        private Vector3 lastPosition = Vector3.zero;

        //Reference to the PID controller
        private PIDController PIDScript;
        //Reference to the script that makes the car follow a path
        private FollowPath followPathScript;
        //Reference to the car data belonging to this car
        private VehicleDataController carDataController;

        //Driving modes
        private enum CarMode { Forward, Reverse, Stop };

        private CarMode carMode = CarMode.Stop;

        //Average the steering angles to simulate the time it takes to turn the wheels
        private float averageSteeringAngle = 0f;
        //The steering angle we should have to follow the path
        private float wantedSteeringAngle = 0f;
        //The speed we should have to follow the path
        public float wantedSpeed = 0f;



        void Awake()
        {
            //Move the center of mass down
            Rigidbody carRB = GetComponent<Rigidbody>();

            carRB.centerOfMass = carRB.centerOfMass - new Vector3(0f, 0.8f, 0f);

            //Get the scripts we will need
            //PIDScript = GetComponent<PIDController>();

            carDataController = GetComponent<VehicleDataController>();

            followPathScript = GetComponent<FollowPath>();
        }



        void FixedUpdate()
        {
            AddMotorAndSteering();
            CalculateSpeed();
        }



        void AddMotorAndSteering()
        {
            //Manual control
            //float motorTorque = maxMotorTorque * Input.GetAxis("Vertical");
            //float steeringAngle = CalculateSteerAngle() * Input.GetAxis("Horizontal");

            float steeringAngle = 0f;
            float motorTorque = 0f;
            float brakeTorque = 0f;

            //Self-driving control
            if (carMode == CarMode.Forward && Mathf.Abs(currentSpeed) < wantedSpeed)
            {
                motorTorque = carDataController.carData.maxMotorTorque;

                //Get the steering angle for the steering wheels
                //Has to be in either forward or reverse, because we need a path to
                //get the steering angle
                steeringAngle = GetSteeringAngle();

                //Debug.Log(steeringAngle);
            }
            else if (carMode == CarMode.Reverse && Mathf.Abs(currentSpeed) < wantedSpeed)
            {
                //Reversing should be slower
                motorTorque = -carDataController.carData.maxMotorTorque;

                //Get the steering angle for the steering wheels
                steeringAngle = GetSteeringAngle();
            }
            //Stop
            else
            {
                brakeTorque = carDataController.carData.maxBrakeTorque;
            }



            //Add everything to the wheels
            foreach (AxleInfo axleInfo in axleInfos)
            {
                if (axleInfo.steering)
                {
                    float currentSteeringAngle = axleInfo.leftWheel.steerAngle;

                    //Simulate a steering wheel by turning to the angle we want over some time
                    float newSteeringAngle = Mathf.Lerp(currentSteeringAngle, steeringAngle, Time.deltaTime * Parameters.steeringWheelSpeed);

                    axleInfo.leftWheel.steerAngle = newSteeringAngle;
                    axleInfo.rightWheel.steerAngle = newSteeringAngle;
                }
                if (axleInfo.motor)
                {
                    axleInfo.leftWheel.motorTorque = motorTorque;
                    axleInfo.rightWheel.motorTorque = motorTorque;
                }

                axleInfo.leftWheel.brakeTorque = brakeTorque;
                axleInfo.rightWheel.brakeTorque = brakeTorque;

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



        //Calculate the current speed in km/h
        private void CalculateSpeed()
        {
            //First calculate the distance of the transform between the fixedupdate calls
            //Now you know the m/fixedupdate
            //Divide by Time.deltaTime to get m/s
            //Multiply with 3.6 to get km/h
            currentSpeed = ((transform.position - lastPosition).magnitude / Time.deltaTime) * 3.6f;

            //Save the position for the next update
            lastPosition = transform.position;

            //Debug.Log(currentSpeed);
        }



        //Used if we don't have a PID controller
        //float CalculateAutomaticSteering()
        //{
        //    Vector3 currentWaypoint = pathControllerScript.GetWayPointPos(pathControllerScript.currentWayPointIndex);

        //    //Transforms the waypoint's position from world space to the car's local space
        //    //Like if the waypoint wiuld be the child of the car
        //    //The height of the waypoint doesn't matter
        //    Vector3 steerVector = transform.InverseTransformPoint(new Vector3(
        //        currentWaypoint.x,
        //        transform.position.y,
        //        currentWaypoint.z
        //        ));

        //    //Will define if we should steer left or right to get to the waypoint
        //    //Is 1 if the wp is to the right of the car
        //    //Is -1 if the wp is to the left of the car
        //    //Is 0 if the wp is infront or at the back of the car
        //    float steering = steerVector.x / steerVector.magnitude;

        //    return steering;
        //}



        //Calculate the steering angle the car should have at this speed
        float GetSteeringAngle()
        {
            //Get the max steering angle the car is allowed to have at this speed
            float maxSteeringAngle = GetMaxSteeringAngle();

            //The wanted steering angle is set in follow path script
            float steeringAngle = Mathf.Clamp(wantedSteeringAngle, -maxSteeringAngle, maxSteeringAngle);

            if (carMode == CarMode.Reverse)
            {
                steeringAngle *= -1f;
            }

            return steeringAngle;
        }



        //A fast car can't turn the wheels as much as it can when driving slower
        float GetMaxSteeringAngle()
        {
            float lowSpeedSteerAngle = 45f;
            float highSpeedSteerAngle = 5f;

            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / carDataController.carData.maxSpeed);

            //Lerp exponentially
            float wheelAngle = highSpeedSteerAngle + HelpStuff.Eerp(lowSpeedSteerAngle, highSpeedSteerAngle, speedFactor);

            return wheelAngle;
        }



        //
        // Set and get methods
        //

        public void StopCar()
        {
            carMode = CarMode.Stop;
        }

        public void MoveCarForward()
        {
            carMode = CarMode.Forward;
        }

        public void MoveCarReverse()
        {
            carMode = CarMode.Reverse;
        }

        public float GetCarSpeed_kmph()
        {
            return currentSpeed;
        }

        public float GetCarSpeed_mps()
        {
            return currentSpeed / 3.6f;
        }

        public VehicleDataController GetCarData()
        {
            return carDataController;
        }

        public void SendPathToCar(List<Node> wayPoints, bool isCircular)
        {
            followPathScript.SetPath(wayPoints, isCircular);
        }

        public void SetWantedSteeringAngle(float angle)
        {
            this.wantedSteeringAngle = angle;
        }

        public void SetWantedSpeed(float speed)
        {
            this.wantedSpeed = speed;
        }
    }
}
