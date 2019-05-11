using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SelfDrivingVehicle;

namespace PathfindingForVehicles
{
    //All dimensions should be in relation to the pivot point
    //which is the point we are using in the pathfinding algorithm
    //The pivot point for a vehicle is the rear wheel axis, 
    //and where the trailer is attached to the drag vehicle if we have a trailer
    public class CarData
    {
        //Engine power
        public float maxMotorTorque;
        //Top speed in km/h
        public float maxSpeed;
        //Brakes
        public float maxBrakeTorque;
        
        //Including rear view mirrors
        public float carWidth;
        //Model S says on their website that the turning circle is 37 feet, which is the radius 
        //But this is when driving slow, so should maybe be higher here?
        public float turningRadius;

        //The distance from the transform.position to the pivot point
        //The pivot point is always the rear wheel position
        public float distanceTransformPositionToPivot;
        //The distance to the rear wheels from the pivot
        public float distancePivotToRearWheels;
        //The distance to the front wheels from the pivot
        public float distancePivotToFrontWheels;
        //The distance to the back from the pivot
        public float distancePivotToRear;
        //The distance to the front from the pivot
        public float distancePivotToFront;
        //The distance from pivot to trailer attachment connector
        public float distancePivotToTrailerAttachment;

        //Semi specific, the semi has a cabin that we need to test collision with against the trailer
        public float cabinLength;

        public bool canReverse;

        //PID parameters to make it follow the path
        public PID_Parameters PID_parameters;

        //public CarData()
        //{
        //    this.maxSpeed = 5f;
        //}

        //Length of the entire car
        public float CarLength
        {
            get
            {
                float length = Mathf.Abs(distancePivotToRear) + Mathf.Abs(distancePivotToFront);

                return length;
            }
        }


        //Distance between the front wheels and rear wheels
        public float WheelBase
        {
            get
            {
                float wheelBase = Mathf.Abs(distancePivotToRearWheels) + Mathf.Abs(distancePivotToFrontWheels);

                return wheelBase;
            }
        }


        //The center of the car if we have heading in radians and rear wheel pos
        public Vector3 GetCenterPos(Vector3 rearWheelPos, float heading)
        {
            //To get the center, we calculate the positon of the front, and the position of the back
            //and then take the average
            Vector3 F = GetLocalZPosition(rearWheelPos, heading, distancePivotToFront);
            Vector3 B = GetLocalZPosition(rearWheelPos, heading, distancePivotToRear);

            Vector3 center = (F + B) * 0.5f;

            return center;
        }

        //The trailer attachment point if we have heading in radians and rear wheel pos
        public Vector3 GetTrailerAttachmentPoint(Vector3 rearWheelPos, float heading)
        {
            Vector3 attachmentPoint = GetLocalZPosition(rearWheelPos, heading, distancePivotToTrailerAttachment);

            return attachmentPoint;
        }

        //The rear wheel pos if we have the attachment point of the trailer and its heading
        public Vector3 GetTrailerRearWheelPos(Vector3 attachmentPos, float heading)
        {
            //In the trailer case, the front wheels is where the trailer is attached to the drag vehicle
            Vector3 rearWheelPos = GetLocalZPosition(attachmentPos, heading, -distancePivotToFrontWheels);

            return rearWheelPos;
        }

        //The center of the semi's cabin if we have heading in radians and rear wheel pos
        public Vector3 GetSemiCabinCenter(Vector3 rearWheelPos, float heading)
        {
            //The front of the semi
            Vector3 front = GetLocalZPosition(rearWheelPos, heading, distancePivotToFront);
            //The back of the cabin
            Vector3 back = GetLocalZPosition(rearWheelPos, heading, distancePivotToFront - cabinLength);

            Vector3 center = (front + back) * 0.5f;

            return center;
        }



        /// <summary>
        /// Calculate the car's corner position
        /// </summary>
        /// <param name="centerPos">The car's center position</param>
        /// <param name="heading">The car's heading [radians]</param>
        /// <param name="width">The width of the car [m]</param>
        /// <param name="length">The length of the car [m]</param>
        /// <returns>The car's corner position coordinates (and maybe center position) in an array</returns>
        public static Rectangle GetCornerPositions(Vector3 centerPos, float heading, float width, float length)
        {
            float halfCarWidth = width * 0.5f;
            float halfCarLength = length * 0.5f;

            //Stuff we can calculate once to save time
            Vector3 zOffset = GetLocalZPosition(centerPos, heading, halfCarLength) - centerPos;
            Vector3 xOffset = GetLocalXPosition(centerPos, heading, halfCarWidth) - centerPos;

            Vector3 FR = centerPos + zOffset + xOffset;
            Vector3 FL = centerPos + zOffset - xOffset;
            Vector3 BR = centerPos - zOffset + xOffset;
            Vector3 BL = centerPos - zOffset - xOffset;

            Rectangle cornerPositions = new Rectangle(FL, FR, BL, BR);


            return cornerPositions;
        }



        /// <summary>
        /// Calculate a position which is offset from the cars z-axis when the car has a heading other than 0, similar to local position
        /// </summary>
        /// <param name="pos">The position of the car which is on the local z-axis</param>
        /// <param name="carHeading">The car's heading [radians]</param>
        /// <param name="offsetPos">The offset position in local z direction from the center position [m]</param>
        /// <returns>The offset with rotation</returns>
        public static Vector3 GetLocalZPosition(Vector3 pos, float heading, float offsetPos)
        {
            //Rotate
            float lengthSin = offsetPos * Mathf.Sin(heading);
            float lengthCos = offsetPos * Mathf.Cos(heading);

            //Move
            float x = pos.x + lengthSin;
            float z = pos.z + lengthCos;

            Vector3 offsetPosWithRotation = new Vector3(x, pos.y, z);


            return offsetPosWithRotation;
        }

        //Same as above but in x direction
        public static Vector3 GetLocalXPosition(Vector3 pos, float heading, float offsetPos)
        {
            //Rotate
            float lengthSin = offsetPos * Mathf.Sin((90f * Mathf.Deg2Rad) + heading);
            float lengthCos = offsetPos * Mathf.Cos((90f * Mathf.Deg2Rad) + heading);

            //Move
            float x = pos.x + lengthSin;
            float z = pos.z + lengthCos;

            Vector3 offsetPosWithRotation = new Vector3(x, pos.y, z);


            return offsetPosWithRotation;
        }
    }
}
