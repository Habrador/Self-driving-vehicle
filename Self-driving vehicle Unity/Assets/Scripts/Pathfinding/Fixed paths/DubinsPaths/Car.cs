using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles.DubinsPaths
{
    //Vehicle data needed to generated fixed paths
    public class Car
    {
        //Left, Right, Straight
        public enum Steering { Left, Right, Straight }
        //Forward, Back
        public enum Gear { Forward, Back }

        public Car.Steering steering;

        public Car.Gear gear;

        public Vector3 pos;

        //Should be in radians
        public float heading;

        public float turningRadius;


        public Car(Vector3 pos, float headingInRadians, float turningRadius)
        {
            this.pos = pos;
            this.heading = ReedsSheppPaths.PathLengthMath.M(headingInRadians);
            this.turningRadius = turningRadius;
        }



        public Car(Vector3 pos, float headingInRadians, float turningRadius, Gear gear, Steering steering) : this(pos, headingInRadians, turningRadius)
        {
            this.gear = gear;
            this.steering = steering;
        }



        //Copy data from car to this car
        public Car(Car car)
        {
            this.pos = car.pos;
            this.heading = car.heading;
            this.turningRadius = car.turningRadius;

            this.gear = car.gear;
            this.steering = car.steering;
        }



        //Change car data
        public Car ChangeData(float newXPos, float newZPos, float newHeading)
        {
            Car carCopy = new Car(new Vector3(newXPos, pos.y, newZPos), newHeading, this.turningRadius);

            return carCopy;
        }



        //
        // Getters
        //
        public float HeadingInDegrees
        {
            get { return heading * Mathf.Rad2Deg; }
        }

        public float HeadingInRadians
        {
            get { return heading; }
        }

        public float X
        {
            get { return pos.x; }
        }

        public float Z
        {
            get { return pos.z; }
        }
    }
}
