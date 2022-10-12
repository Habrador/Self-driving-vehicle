using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PathfindingForVehicles.ReedsSheppPaths
{
    //The settings the car should have to complete this path segment
    //Each Reeds-Shepp path consists of 3-5 of these segments
    public class SegmentSettings
    {
        //The total length of this segment
        public float length;
        //Which way are we steering in this segment?
        public RSCar.Steering steering;
        //Are we driwing forward or reverse?
        public RSCar.Gear gear;



        public SegmentSettings(RSCar.Steering steering, RSCar.Gear gear, float length)
        {
            this.steering = steering;
            this.gear = gear;
            this.length = length;
        }
    }
}
