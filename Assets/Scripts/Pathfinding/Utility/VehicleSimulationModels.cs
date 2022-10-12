using UnityEngine;
using System.Collections;


namespace PathfindingForVehicles
{
    /// <summary>
    /// Mathematical methods that simulates vehicles
    /// </summary>
    public static class VehicleSimulationModels
    {
        /// <summary>
        /// //Calculate the position of the car after driving distance d with steering angle beta
        /// </summary>
        /// <param name="theta">The car's heading (= rotation) [rad]</param>
        /// <param name="beta">Steering angle [rad]</param>
        /// <param name="d">Driving distance</param>
        /// <param name="rearWheelPos">Current position of the car's rear wheels</param>
        /// <returns>The cars's new rear wheel position</returns>
        public static Vector3 CalculateNewPosition(float theta, float beta, float d, Vector3 rearWheelPos)
        {
            //The coordinate system is not the same as in class "Programming a self-driving car", 
            //so sin and cos are switched

            Vector3 newRearWheelPos = Vector3.zero;

            //Two different calculations depending on the size of the turning angle beta

            //Move forward
            if (Mathf.Abs(beta) < 0.00001f)
            {
                newRearWheelPos.x = rearWheelPos.x + d * Mathf.Sin(theta);
                newRearWheelPos.z = rearWheelPos.z + d * Mathf.Cos(theta);
            }
            //Turn
            else
            {
                //Turning radius 
                float R = d / beta;

                float cx = rearWheelPos.x + Mathf.Cos(theta) * R;
                float cz = rearWheelPos.z - Mathf.Sin(theta) * R;

                newRearWheelPos.x = cx - Mathf.Cos(theta + beta) * R;
                newRearWheelPos.z = cz + Mathf.Sin(theta + beta) * R;
            }

            return newRearWheelPos;
        }



        /// <summary>
        /// Calculate the car's new heading
        /// </summary>
        /// <param name="theta">The car's heading (= rotation) [rad]</param>
        /// <param name="beta">Steering angle [rad]</param>
        /// <returns>The car's new heading</returns>
        public static float CalculateNewHeading(float theta, float beta)
        {
            //Change heading
            theta = theta + beta;

            //Clamp heading - is sometimes causing infinite loop so dont use the old version?
            theta = HelpStuff.WrapAngleInRadians(theta);

            //Clamp heading
            //if (theta > 2f * Mathf.PI)
            //{
            //    theta = theta - 2f * Mathf.PI;
            //}
            //if (theta < 0f)
            //{
            //    theta = 2f * Mathf.PI + theta;
            //}

            //Debug.Log(theta + " " + theta2);

            return theta;
        }



        /// <summary>
        /// Calculate the heading of a trailer attached to a drag vehicle
        /// </summary>
        /// <param name="thetaOld">The trailers old heading [rad]</param>
        /// <param name="thetaOldDragVehicle">The drag vehicles old heading [rad]</param>
        /// <param name="D">Drive distance [m]. Should be negative if we reverse</param>
        /// <param name="d">Distance between trailer attachment point and trailer rear axle [m]</param>
        /// <returns>The trailer's new heading [rad]</returns>
        public static float CalculateNewTrailerHeading(float thetaOld, float thetaOldDragVehicle, float D, float d)
        {
            //According to some D is velocity of the drag vehicle but is not really working
            //From "Planning algorithms" which is different from http://planning.cs.uiuc.edu/node661.html#77556
            //where (thetaOldDragVehicle - thetaOld) is the opposite which gives a mirrored result
            //float theta = thetaOld + (D / d) * Mathf.Sin(thetaOldDragVehicle - thetaOld);

            float theta = thetaOld + ((D / d) * Mathf.Sin(thetaOldDragVehicle - thetaOld));

            //Clamp heading - is sometimes causing infinite loop so dont use?
            theta = HelpStuff.WrapAngleInRadians(theta);

            //Clamp heading
            //if (theta > 2f * Mathf.PI)
            //{
            //    theta = theta - 2f * Mathf.PI;
            //}
            //if (theta < 0f)
            //{
            //    theta = 2f * Mathf.PI + theta;
            //}

            return theta;
        }
    }
}
