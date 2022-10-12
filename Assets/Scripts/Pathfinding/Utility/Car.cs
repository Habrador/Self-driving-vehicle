using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    public class Car
    {
        //The car's rear wheel pos
        public Vector3 rearWheelPos;
        //The car's heading in radians
        private float heading;
        //Data that belongs to this car
        public CarData carData;



        public Car(Vector3 rearWheelPos, float heading, CarData carData)
        {
            this.rearWheelPos = rearWheelPos;
            this.heading = heading;
            this.carData = carData;
        }



        public Car(Transform carTrans, SelfDrivingVehicle.VehicleDataController carDataController)
        {
            this.carData = carDataController.carData;

            this.rearWheelPos = carDataController.RearWheelPos(carTrans);

            this.heading = carDataController.HeadingInRadians(carTrans);
        }



        //Headings
        public float HeadingInRadians
        {
            get { return heading; }
        }

        public float HeadingInDegrees
        {
            get { return heading * Mathf.Rad2Deg; }
        }
    }
}
