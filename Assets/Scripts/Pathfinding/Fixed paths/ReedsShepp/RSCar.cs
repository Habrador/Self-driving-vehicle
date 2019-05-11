using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles.ReedsSheppPaths
{
    //Vehicle data needed to generated fixed paths
    public class RSCar
    {
        //Left, Right, Straight
        public enum Steering { Left, Right, Straight }
        //Forward, Back
        public enum Gear { Forward, Back }

        public RSCar.Steering steering;

        public RSCar.Gear gear;

        public Vector3 pos;

        //Should be in radians
        private float heading;



        public RSCar(Vector3 pos, float headingInRadians)
        {
            this.pos = pos;
            this.heading = ReedsSheppPaths.PathLengthMath.M(headingInRadians);
        }



        public RSCar(Vector3 pos, float headingInRadians, Gear gear, Steering steering) : this(pos, headingInRadians)
        {
            this.gear = gear;
            this.steering = steering;
        }



        //Copy data from car to this car
        public RSCar(RSCar car)
        {
            this.pos = car.pos;
            this.heading = car.heading;

            this.gear = car.gear;
            this.steering = car.steering;
        }



        //Change car data
        public RSCar ChangeData(float newXPos, float newZPos, float newHeading)
        {
            RSCar carCopy = new RSCar(new Vector3(newXPos, pos.y, newZPos), newHeading);

            return carCopy;
        }



        //
        // Getters
        //
        public float HeadingInDegrees
        {
            get { return heading * Mathf.Rad2Deg; }
        }

        public float HeadingInRad
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
