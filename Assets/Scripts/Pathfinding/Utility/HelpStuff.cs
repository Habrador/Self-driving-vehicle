using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    public static class HelpStuff
    {
        //Movements used when investigating the cells around a cell in a grid
        //No corners
        public static IntVector2[] delta = {
                   new IntVector2(0, 1),
                   new IntVector2(1, 0),
                   new IntVector2(0, -1),
                   new IntVector2(-1, 0)
                   };

        //With corners 
        //Is ordered in circular fashion so we can identify cells next to corners
        //Is still circular if x and z are flipped
        public static IntVector2[] deltaWithCorners = {
                   new IntVector2(1, 0), //R
                   new IntVector2(1, -1), //BR
                   new IntVector2(0, -1), //B
                   new IntVector2(-1, -1), //BL
                   new IntVector2(-1, 0), //L
                   new IntVector2(-1, 1), //TL
                   new IntVector2(0, 1), //T
                   new IntVector2(1, 1) //TR
                   };

        //Just the corners
        public static IntVector2[] deltaJustCorners = {
                   new IntVector2(1, 1),
                   new IntVector2(-1, -1),
                   new IntVector2(1, -1),
                   new IntVector2(-1, 1)
                   };
                           


        //Clamp list indices
        //Will even work if index is larger/smaller than listSize, so can loop multiple times
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }



        //Find the closest point on a line segment from a point
        //From https://www.youtube.com/watch?v=KHuI9bXZS74
        //Maybe better version https://stackoverflow.com/questions/3120357/get-closest-point-to-a-line
        public static Vector2 GetClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 a_p = p - a;
            Vector2 a_b = b - a;

            //Square magnitude of AB vector
            float sqrMagnitudeAB = a_b.sqrMagnitude;

            //The DOT product of a_p and a_b  
            float ABAPproduct = Vector2.Dot(a_p, a_b);

            //The normalized "distance" from a to the closest point  
            float distance = ABAPproduct / sqrMagnitudeAB;

            //This point may not be on the line segment, if so return one of the end points
            //Check if P projection is over vectorAB     
            if (distance < 0)
            {
                return a;
            }
            else if (distance > 1)
            {
                return b;
            }
            else
            {
                return a + a_b * distance;
            }
        }

        //Find the closest point on a line from a point
        public static Vector2 GetClosestPointOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 a_p = p - a;
            Vector2 a_b = b - a;

            //Square magnitude of AB vector
            float sqrMagnitudeAB = a_b.sqrMagnitude;

            //The DOT product of a_p and a_b  
            float ABAPproduct = Vector2.Dot(a_p, a_b);

            //The normalized "distance" from a to the closest point  
            float distance = ABAPproduct / sqrMagnitudeAB;

            //This point may not be on the line segment, if so return one of the end points
            return a + a_b * distance;
        }



        //Remap value from range 1 to range 2
        public static float Remap(float value, float low1, float high1, float low2, float high2)
        {
            float remappedValue = low2 + (value - low1) * ((high2 - low2) / (high1 - low1));

            return remappedValue;
        }



        //Calculate an angle measured in 360 degrees
        //Vector3.Angle is measured in 180 degrees
        //From should be Vector3.forward if you measure y angle, and to is the direction
        public static float CalculateAngle(Vector3 from, Vector3 to)
        {
            return Quaternion.FromToRotation(from, to).eulerAngles.y;
        }



        //Add value to average
        //http://www.bennadel.com/blog/1627-create-a-running-average-without-storing-individual-values.htm
        //count - how many values does the average consist of
        public static float AddValueToAverage(float oldAverage, float valueToAdd, float count)
        {
            float newAverage = ((oldAverage * count) + valueToAdd) / (count + 1f);

            return newAverage;
        }



        //Round a value to nearest int value determined by roundValue
        //So if roundValue is 5, we round 11 to 10 because we want to go in steps of 5
        //such as 0, 5, 10, 15
        public static int RoundValue(float value, float roundValue)
        {
            int roundedValue = (int)(Mathf.Round(value / roundValue) * roundValue);

            return roundedValue;
        }



        //Lerp exponentially between 2 values (is also working if a > b)
        public static float Eerp(float a, float b, float t)
        {
            float exponentiallyLerpedValue = a * Mathf.Pow(b / a, t);

            return exponentiallyLerpedValue;
        }



        //Wrap angle in radians, is called M in the report
        //http://www.technologicalutopia.com/sourcecode/xnageometry/mathhelper.cs.htm
        public static float WrapAngleInRadians(float angle)
        {
            float PI = Mathf.PI;
            float TWO_PI = PI * 2f;
        
            angle = (float)System.Math.IEEERemainder((double)angle, (double)TWO_PI);

            //if (angle <= -PI)
            //{
            //    angle += TWO_PI;
            //}
            //else if (angle > PI)
            //{
            //    angle -= TWO_PI;
            //}

            if (angle > 2f * PI)
            {
                angle = angle - 2f * PI;
            }
            if (angle < 0f)
            {
                angle = 2f * PI + angle;
            }

            return angle;
        }
    }
}

