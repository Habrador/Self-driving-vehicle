using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathfindingForVehicles;



namespace SelfDrivingVehicle
{
    //Will make the car follow the given path
    public class FollowPath : MonoBehaviour
    {
        //Waypoints related
        //This is the waypoint the car is heading towards
        private int currentWayPointIndex = 1;
        //The waypoints the car will follow
        private List<Node> wayPoints;
        //Is the path circular?
        private bool isCircular = false;
        //The speed the car should have to reach each waypoint
        private List<float> wantedSpeeds;

        //Scripts
        //The script that controls if the car is driving forward/reverse/stop
        private VehicleController carScript;
        //The script with car data
        private VehicleDataController carDataController;

        //The PID controller used to find the steering angle
        PIDController pidController;



        void Start()
        {
            carScript = GetComponent<VehicleController>();

            carDataController = GetComponent<VehicleDataController>();

            pidController = new PIDController();
        }



        void Update()
        {
            //Do we have enough waypoints?
            if (wayPoints != null && wayPoints.Count > 1)
            {
                //Set if the car should drive forward or reverse
                UpdateCarMovement();

                //Calculate the steering angle we should have to follow the path
                CalculateSteeringAngle();

                TryChangeWaypoint();

                //Display the waypoint we are heading towards
                //Vector3 waypointPos = GetWayPointPos(currentWayPointIndex);

                //Debug.DrawRay(waypointPos, Vector3.up * 5f, Color.red);
            }
            //If not, then stop the car
            else
            {
                carScript.StopCar();

                //print("Stop car");
            }
        }



        //Change if the car should drive forward, reverse, or brake
        void UpdateCarMovement()
        {
            if (wayPoints[currentWayPointIndex].isReversing)
            {
                carScript.MoveCarReverse();
            }
            else
            {
                carScript.MoveCarForward();
            }

            //Change the car's speed depending on the speed we should have to reach this waypoint
            float wantedSpeed = wantedSpeeds[currentWayPointIndex];

            //But if the vehicle is deviating from the path, it should have a lower speed

            //The vector between the waypoints
            bool isReversing = wayPoints[currentWayPointIndex].isReversing;

            //bool isReversing = false;

            Vector3 wp2 = GetWaypointPos(currentWayPointIndex, isReversing);
            Vector3 wp1 = GetWaypointPos(currentWayPointIndex - 1, isReversing);

            Vector3 wpVec = (wp2 - wp1).normalized;

            //The angle between the front vector of the vehicle and the vector between the waypoints
            float angle = Vector3.Angle(this.transform.forward, wpVec);
            
            if (angle > 10f)
            {
                float deviateFromPathSpeed = Parameters.maxPathFollowSpeed * 0.5f;

                if (deviateFromPathSpeed < wantedSpeed)
                {
                    wantedSpeed = deviateFromPathSpeed;

                    //Debug.Log("Is driving slower because deviating from path");
                }
            }

            carScript.SetWantedSpeed(wantedSpeed);
        }



        //Calculate the speed the car should have to reach each waypoint
        void CalculateWaypointSpeeds(List<Node> wayPoints)
        {
            wantedSpeeds = new List<float>();

            //Init them to the wanted speed we have specified
            for (int i = 0; i < wayPoints.Count; i++)
            {
                float maxSpeed = Parameters.maxPathFollowSpeed;

                float wantedSpeed = maxSpeed;

                //Slower if reversing
                if (wayPoints[i].isReversing)
                {
                    wantedSpeed *= 0.5f; 
                }

                //Slow down if we are close to a turning point (reverse-forward or end point)
                float distanceToTurningPoint = 0f;

                bool isReversing = wayPoints[i].isReversing;

                for (int j = i + 1; j < wayPoints.Count; j++)
                {
                    distanceToTurningPoint += (wayPoints[j - 1].frontWheelPos - wayPoints[j].frontWheelPos).magnitude;

                    //Stop looping if this is a truning point
                    if (j == wayPoints.Count - 1 || isReversing != wayPoints[j].isReversing)
                    {
                        break;
                    }
                }


                float minDistance = 10f;

                if (distanceToTurningPoint < minDistance)
                {
                    //Slow down the closer we are
                    wantedSpeed = (distanceToTurningPoint / minDistance) * maxSpeed;

                    //But dont slow dont too much
                    wantedSpeed = Mathf.Clamp(wantedSpeed, maxSpeed * 0.1f, maxSpeed);
                }

                wantedSpeeds.Add(wantedSpeed);
            }
        }



        //Change waypoint if we have passed the waypoint we were heading towards
        void TryChangeWaypoint()
        {
            //Should always be measured from the rear wheels because that's what is used when generating the path
            //But in the final version we are moving the waypoints forward, so it depends on if we are reversing or not
            //bool isReversing = wayPoints[currentWayPointIndex].isReversing;

            bool isReversing = false;

            Vector3 wp2 = GetWaypointPos(currentWayPointIndex, isReversing);
            Vector3 wp1 = GetWaypointPos(currentWayPointIndex - 1, isReversing);

            Vector3 frontAxle = isReversing ? carDataController.MirroredFrontWheelPos(transform) : carDataController.FrontWheelPos(transform);

            //If we have reached the waypoint we are aiming for
            if (CalculateProgress(frontAxle, wp1, wp2) > 1f)
            {
                currentWayPointIndex += 1;

                //Clamp when we have reached the last waypoint so we start all over again
                if (currentWayPointIndex > wayPoints.Count - 1 && isCircular)
                {
                    currentWayPointIndex = 0;
                }
                //The path in not circular so we should stop following it
                else if (currentWayPointIndex > wayPoints.Count - 1)
                {
                    wayPoints = null;

                    //Stop the car when we have reached the end of the path
                    carScript.StopCar();
                }

                //Debug.Log(currentWayPointIndex);
            }
        }



        //Calculate cross track error CTE from pos to closest point on path
        //CTE is generally measured from front wheels
        //From https://www.udacity.com/course/viewer#!/c-cs373/l-48696626/e-48403941/m-48716166
        public float CalculateCTE(Vector3 carPos, Vector3 wp_from, Vector3 wp_to)
        {
            Vector2 closestPos2D = HelpStuff.GetClosestPointOnLine(wp_from.XZ(), wp_to.XZ(), carPos.XZ());

            Vector3 progressCoordinate = closestPos2D.XYZ();

            //Display where this coordinate is
            //Debug.DrawLine(progressCoordinate, carPos);

            float CTE = (carPos - progressCoordinate).magnitude;

            //Debug.Log(CTE);

            //We need to determine if CTE is negative or positive so we can steer in the direction we need
            //Is the car to the right or to the left of the upcoming waypoint
            //http://forum.unity3d.com/threads/left-right-test-function.31420/
            Vector3 toCarVec = carPos - wp_from;
            Vector3 toWaypointVec = wp_to - wp_from;

            Vector3 perp = Vector3.Cross(toCarVec, toWaypointVec);

            float dir = Vector3.Dot(perp, Vector3.up);

            //The car is right of the waypoint
            if (dir > 0f)
            {
                CTE *= -1f;
            }

            return CTE;
        }



        //Calculate how far we have progressed on the segment from the waypoint to the waypoint we are heading towards
        //From https://www.udacity.com/course/viewer#!/c-cs373/l-48696626/e-48403941/m-48716166
        //Returns > 1 if we have passed the waypoint
        float CalculateProgress(Vector3 carPos, Vector3 wp_from, Vector3 wp_to)
        {
            float Rx = carPos.x - wp_from.x;
            float Rz = carPos.z - wp_from.z;

            float deltaX = wp_to.x - wp_from.x;
            float deltaZ = wp_to.z - wp_from.z;

            //If progress is > 1 then the car has passed the waypoint
            float progress = ((Rx * deltaX) + (Rz * deltaZ)) / ((deltaX * deltaX) + (deltaZ * deltaZ));

            //Debug.Log(progress);

            return progress;
        }



        //Get a clamped waypoint from a list of waypoints
        public Vector3 GetWaypointPos(int index, bool isReversing)
        {
            int waypointIndex = HelpStuff.ClampListIndex(index, wayPoints.Count);

            //Vector3 waypointPos = wayPoints[waypointIndex].frontWheelPos;

            Vector3 waypointPos = isReversing ? wayPoints[waypointIndex].reverseWheelPos : wayPoints[waypointIndex].frontWheelPos;           

            return waypointPos;
        }



        //Get the heading if we are driving from p1 to p2 [degrees]
        //Is the same transform y rotation the car would have had if it was driving straight between the waypoints
        //public float GetWPHeadingInDegrees()
        //{
        //    Vector3 p1 = GetWaypointPos(currentWayPointIndex - 1);
        //    Vector3 p2 = GetWaypointPos(currentWayPointIndex);

        //    Vector3 from = Vector3.forward;

        //    Vector3 to = p2 - p1;

        //    float heading = HelpStuff.CalculateAngle(from, to);

        //    return heading;
        //}



        //
        // Steering angle to follow the path
        //
        private void CalculateSteeringAngle()
        {
            float steeringAngle = 0f;

            //Method 1. Without PID controller
            //steeringAngle = CalculateSteerAngle() * CalculateAutomaticSteering();


            //Method 2. With PID controller
            //Get the cross track error
            bool isReversing = wayPoints[currentWayPointIndex].isReversing;

            Vector3 wp2 = GetWaypointPos(currentWayPointIndex, isReversing);
            Vector3 wp1 = GetWaypointPos(currentWayPointIndex - 1, isReversing);

            //If we are driving forward, this is the distance to the position the car should be at as measured from the front axle
            //If we are reversing, we have to measure CTE by using the mirrored front-axle-path and then using the mirrored front-axle
            Vector3 frontAxle = isReversing ? carDataController.MirroredFrontWheelPos(this.transform) : carDataController.FrontWheelPos(this.transform);

            //Is following the path better if we move these positions where we measure CTE
            if (!isReversing)
            {
                frontAxle += transform.forward * 2f;
            }
            else
            {
                frontAxle -= transform.forward * 2f;
            }

            float CTE = CalculateCTE(frontAxle, wp1, wp2);

            steeringAngle = pidController.GetNewValue(CTE, carDataController.carData.PID_parameters);


            ////Method 3. The Stanley Method
            ////The distance from the front axle to the closest point on the desired path [m]
            //float cte_front_axle = CalculateCTE(carDataController.GetFrontWheelPos(transform)) * -1f;
            ////The heading error, which is the difference between current heading and the heading of the path [rad]
            //float theta_heading_error = (transform.eulerAngles.y - GetWPHeadingInDegrees()) * Mathf.Deg2Rad;
            ////Gain parameter
            //float k = 5f;
            ////Current velocity [m/s]
            //float v = carScript.GetCarSpeed_mps();
            ////The desired front wheel deflection [rad]
            ////When CTE != 0, the car is steering towards a lookahead point determined by k and v
            //steeringAngle = theta_heading_error + Mathf.Atan((k * cte_front_axle) / v);
            ////Convert from radians to degrees
            //steeringAngle *= Mathf.Rad2Deg;

            //Not needed because we determine this in the car script
            //if (wayPoints[currentWayPointIndex].isReversing)
            //{
            //    steeringAngle *= -1f;
            //}

            //Debug.Log(theta_heading_error);

            carScript.SetWantedSteeringAngle(steeringAngle);
        }



        //
        // Set methods
        //

        //Set a new path, can be null, meaning the car will not follow a path
        public void SetPath(List<Node> wayPoints, bool isCircular)
        {
            //if (wayPoints.Count < 2)
            //{
            //    Debug.Log("Cant send path to car");
            
            //    return;
            //}
        
            this.wayPoints = wayPoints;

            //Restart the path
            //Always begin at 1 because we need the 0th wp to calculate if we have passed the waypoint
            currentWayPointIndex = 1;

            this.isCircular = isCircular;

            //Calculate the speed the car should have to reach each waypoint
            if (wayPoints != null)
            {
                CalculateWaypointSpeeds(wayPoints);
            }
        }
    }
}
