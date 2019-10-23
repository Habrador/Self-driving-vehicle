using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles.ReedsSheppPaths
{
    //Calculate how long a given reeds-shepp path is
    public static class PathLengthMath
    {
        private const float HALF_PI = Mathf.PI * 0.5f;
        private const float PI = Mathf.PI;
        private const float TWO_PI = Mathf.PI * 2f;

        //Could maybe optimize sin(phi) and cos(phi) because they are always the same

        public static float GetLength(RSCar car, PathWords word, out PathSegmentLengths pathLengths)
        {
            switch (word)
            {
                //8.1: CSC, same turn
                case PathWords.Lf_Sf_Lf: return Lf_Sf_Lf(car, out pathLengths);
                case PathWords.Lb_Sb_Lb: return Lf_Sf_Lf(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Sf_Rf: return Lf_Sf_Lf(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Sb_Rb: return Lf_Sf_Lf(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.2: CSC, different turn
                case PathWords.Lf_Sf_Rf: return Lf_Sf_Rf(car, out pathLengths);
                case PathWords.Lb_Sb_Rb: return Lf_Sf_Rf(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Sf_Lf: return Lf_Sf_Rf(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Sb_Lb: return Lf_Sf_Rf(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.3: C|C|C
                case PathWords.Lf_Rb_Lf: return Lf_Rb_Lf(car, out pathLengths);
                case PathWords.Lb_Rf_Lb: return Lf_Rb_Lf(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Lb_Rf: return Lf_Rb_Lf(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Lf_Rb: return Lf_Rb_Lf(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.4: C|CC
                case PathWords.Lf_Rb_Lb: return Lf_Rb_Lb(car, out pathLengths);
                case PathWords.Lb_Rf_Lf: return Lf_Rb_Lb(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Lb_Rb: return Lf_Rb_Lb(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Lf_Rf: return Lf_Rb_Lb(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.4: CC|C
                case PathWords.Lf_Rf_Lb: return Lf_Rf_Lb(car, out pathLengths);
                case PathWords.Lb_Rb_Lf: return Lf_Rf_Lb(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Lf_Rb: return Lf_Rf_Lb(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Lb_Rf: return Lf_Rf_Lb(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.7: CCu|CuC
                case PathWords.Lf_Ruf_Lub_Rb: return Lf_Ruf_Lub_Rb(car, out pathLengths);
                case PathWords.Lb_Rub_Luf_Rf: return Lf_Ruf_Lub_Rb(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Luf_Rub_Lb: return Lf_Ruf_Lub_Rb(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Lub_Ruf_Lf: return Lf_Ruf_Lub_Rb(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.8: C|CuCu|C
                case PathWords.Lf_Rub_Lub_Rf: return Lf_Rub_Lub_Rf(car, out pathLengths);
                case PathWords.Lb_Ruf_Luf_Rb: return Lf_Rub_Lub_Rf(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Lub_Rub_Lf: return Lf_Rub_Lub_Rf(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Luf_Ruf_Lb: return Lf_Rub_Lub_Rf(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.9: C|C(pi/2)SC, same turn
                case PathWords.Lf_Rbpi2_Sb_Lb: return Lf_Rbpi2_Sb_Lb(car, out pathLengths);
                case PathWords.Lb_Rfpi2_Sf_Lf: return Lf_Rbpi2_Sb_Lb(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Lbpi2_Sb_Rb: return Lf_Rbpi2_Sb_Lb(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Lfpi2_Sf_Rf: return Lf_Rbpi2_Sb_Lb(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.10: C|C(pi/2)SC, different turn
                case PathWords.Lf_Rbpi2_Sb_Rb: return Lf_Rbpi2_Sb_Rb(car, out pathLengths);
                case PathWords.Lb_Rfpi2_Sf_Rf: return Lf_Rbpi2_Sb_Rb(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Lbpi2_Sb_Lb: return Lf_Rbpi2_Sb_Rb(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Lfpi2_Sf_Lf: return Lf_Rbpi2_Sb_Rb(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.9(reversed): CSC(pi / 2) | C, same turn
                case PathWords.Lf_Sf_Rfpi2_Lb: return Lf_Sf_Rfpi2_Lb(car, out pathLengths);
                case PathWords.Lb_Sb_Rbpi2_Lf: return Lf_Sf_Rfpi2_Lb(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Sf_Lfpi2_Rb: return Lf_Sf_Rfpi2_Lb(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Sb_Lbpi2_Rf: return Lf_Sf_Rfpi2_Lb(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.10 (reversed): CSC(pi/2)|C, different turn
                case PathWords.Lf_Sf_Lfpi2_Rb: return Lf_Sf_Lfpi2_Rb(car, out pathLengths);
                case PathWords.Lb_Sb_Lbpi2_Rf: return Lf_Sf_Lfpi2_Rb(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Sf_Rfpi2_Lb: return Lf_Sf_Lfpi2_Rb(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Sb_Rbpi2_Lf: return Lf_Sf_Lfpi2_Rb(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);


                //8.11: C | C(pi / 2)SC(pi / 2) | C
                case PathWords.Lf_Rbpi2_Sb_Lbpi2_Rf: return Lf_Rbpi2_Sb_Lbpi2_Rf(car, out pathLengths);
                case PathWords.Lb_Rfpi2_Sf_Lfpi2_Rb: return Lf_Rbpi2_Sb_Lbpi2_Rf(car.ChangeData(-car.X, car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rf_Lbpi2_Sb_Rbpi2_Lf: return Lf_Rbpi2_Sb_Lbpi2_Rf(car.ChangeData(car.X, -car.Z, -car.HeadingInRad), out pathLengths);
                case PathWords.Rb_Lfpi2_Sf_Rfpi2_Lb: return Lf_Rbpi2_Sb_Lbpi2_Rf(car.ChangeData(-car.X, -car.Z, car.HeadingInRad), out pathLengths);
            }

            pathLengths = new PathSegmentLengths(0f, 0f, 0f);

            return float.PositiveInfinity;
        }



        //
        // Length calculations for the different paths
        //

        //Basic idea (from "Optimal paths for a car that goes both forwards and backwards"):

        //In each formula the objective is to move from (0, 0, 0) to (x, y, phi)

        //We write (r, theta) = R(x, y) for the polar transform: 
        // r * cos(theta) = x
        // r * sin(theta) = y
        // r >= 0
        // -pi <= theta < pi
        //So R(x, y) means that we translate x and y to polar coordinates from the cartesian
        //https://en.wikipedia.org/wiki/Polar_coordinate_system

        //We write phi = M(theta) if phi ≡ theta mod(2*pi) and -pi <= phi < pi, so M(theta) means modulus 2*pi

        //L is the overall length. We say L = ∞ if there's no solution

        //t, u, v are the unknown segment lengths
        //A Reeds-Shepp path consists of 3-5 segments, but only 3 segments have unpredetermined lengths
        //The other segments have each a length of a curve where we drive a length of pi/2 
        //or the same length as another segment in the same path   
        //The unit of length for a straight segment is whatever we want. For a circular arc it is in radians.



        //8.1: CSC, same turn
        public static float Lf_Sf_Lf(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //On page 24 in the Reeds-Shepp report, it says:
            //(u, t) = R(x - sin(phi), y - 1 + cos(phi))
            //v = M(phi - t)
            //L = |t| + |u| + |v|
            //This path can't be optimal if t or v is outside [0, pi]
            R(x - Mathf.Sin(phi), y - 1f + Mathf.Cos(phi), out u, out t);

            v = M(phi - t);


            //Debug.Log(t + " " + u + " " + v);

            //Dont need to check u because its straight
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + v;

            return totalLength;
        }



        //8.2: CSC, different turn
        public static float Lf_Sf_Rf(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            float u1, t1;

            R(x + Mathf.Sin(phi), y - 1f - Mathf.Cos(phi), out u1, out t1);

            if (u1 * u1 < 4f)
            {
                return float.PositiveInfinity;
            }

            u = Mathf.Sqrt((u1 * u1) - 4f);

            float T, theta;

            R(u, 2f, out T, out theta);

            t = M(t1 + theta);

            v = M(t - phi);


            //Debug.Log(t + " " + u + " " + v);

            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + v;

            return totalLength;
        }



        //8.3: C|C|C
        public static float Lf_Rb_Lf(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the c_c_c function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x - Mathf.Sin(phi);
            float eta = y - 1f + Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            if (u1 > 4f)
            {
                return float.PositiveInfinity;
            }

            float alpha = Mathf.Acos(u1 / 4f);

            t = M(HALF_PI + alpha + theta);
            u = M(PI - 2f * alpha);
            v = M(phi - t - u);

            //Check all 3 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(u) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + v;

            return totalLength;
        }



        //8.4: C|CC
        public static float Lf_Rb_Lb(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the c_cc function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x - Mathf.Sin(phi);
            float eta = y - 1f + Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            if (u1 > 4f)
            {
                return float.PositiveInfinity;
            }

            float alpha = Mathf.Acos(u1 / 4f);

            t = M(HALF_PI + alpha + theta);
            u = M(PI - 2f * alpha);
            //This part is the only thing thats different from 8.3
            v = M(t + u - phi);

            //Check all 3 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(u) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + v;

            return totalLength;
        }



        //8.4: CC|C
        public static float Lf_Rf_Lb(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the cc_c function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x - Mathf.Sin(phi);
            float eta = y - 1f + Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            if (u1 > 4f)
            {
                return float.PositiveInfinity;
            }

            u = Mathf.Acos((8f - (u1 * u1)) / 8f);

            float va = Mathf.Sin(u);

            float alpha = Mathf.Asin(2f * va / u1);

            t = M(HALF_PI - alpha + theta);
            
            v = M(t - u - phi);

            //Check all 3 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(u) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + v;

            return totalLength;
        }



        //8.7: CCu|CuC
        public static float Lf_Ruf_Lub_Rb(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the ccu_cuc function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x + Mathf.Sin(phi);
            float eta = y - 1f - Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            if (u1 > 4f)
            {
                return float.PositiveInfinity;
            }

            if (u1 > 2f)
            {
                float alpha = Mathf.Acos((u1 / 4f) - 0.5f);
                
                t = M(HALF_PI + theta - alpha);
                u = M(PI - alpha);
                v = M(phi - t + 2f * u);
            }
            else
            {
                float alpha = Mathf.Acos((u1 / 4f) + 0.5f);

                t = M(HALF_PI + theta + alpha);
                u = M(alpha);
                v = M(phi - t + 2f * u);
            }

            //Check all 3 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(u) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }
            
            path.t = t; path.u = u; path.v = v;
            
            float totalLength = t + u + u + v;

            return totalLength;
        }



        //8.8: C|CuCu|C
        public static float Lf_Rub_Lub_Rf(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the c_cucu_c function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x + Mathf.Sin(phi);
            float eta = y - 1f - Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            if (u1 > 6f)
            {
                return float.PositiveInfinity;
            }

            //This is the part thats different from the original report:
            float va1 = 1.25f - ((u1 * u1) / 16f);

            if (va1 < 0f || va1 > 1f)
            {
                return float.PositiveInfinity;
            }

            u = Mathf.Acos(va1);

            float va2 = Mathf.Sin(u);
            
            float alpha = Mathf.Asin((2f * va2) / u1);
            
            t = M(HALF_PI + theta + alpha);
            v = M(t - phi);

            //Check all 3 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(u) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + u + v;

            return totalLength;
        }



        //8.9: C|C(pi/2)SC, same turn
        public static float Lf_Rbpi2_Sb_Lb(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the c_c2sca function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x - Mathf.Sin(phi);
            float eta = y - 1f + Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            float u1Squared = u1 * u1;

            if (u1Squared < 4f)
            {
                return float.PositiveInfinity;
            }

            //This is the part thats different from the original report:
            float va1 = 1.25f - ((u1 * u1) / 16f);

            if (va1 < 0f || va1 > 1f)
            {
                return float.PositiveInfinity;
            }

            u = Mathf.Sqrt(u1Squared - 4f) - 2f;

            if (u < 0f)
            {
                return float.PositiveInfinity;
            }

            float alpha = Mathf.Atan2(2f, u + 2f);

            t = M(HALF_PI + theta + alpha);
            v = M(t + HALF_PI - phi);

            //Check all 2 curves (pi/2 is always a valid curve)
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + HALF_PI + u + v;

            return totalLength;
        }



        //8.10: C|C(pi/2)SC, different turn
        public static float Lf_Rbpi2_Sb_Rb(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the c_c2scb function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x + Mathf.Sin(phi);
            float eta = y - 1f - Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            if (u1 < 2f)
            {
                return float.PositiveInfinity;
            }

            //This is the part thats different from the original report:
            t = M(HALF_PI + theta);
            u = u1 - 2;
            v = M(phi - t - HALF_PI);

            //Check all 2 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + HALF_PI + u + v;

            return totalLength;
        }



        //8.9 (reversed): CSC(pi/2)|C, same turn
        public static float Lf_Sf_Rfpi2_Lb(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the csc2_ca function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x - Mathf.Sin(phi);
            float eta = y - 1f + Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            float u1Squared = u1 * u1;

            if (u1 < 4f)
            {
                return float.PositiveInfinity;
            }

            //This is the part thats different from the original report:
            u = Mathf.Sqrt(u1Squared - 4f) - 2f;

            if (u < 0f)
            {
                return float.PositiveInfinity;
            }

            float alpha = Mathf.Atan2(u + 2f, 2f);

            t = M(HALF_PI + theta - alpha);
            v = M(t - HALF_PI - phi);


            //Check all 2 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + HALF_PI + v;

            return totalLength;
        }



        //8.10 (reversed): CSC(pi/2)|C, different turn
        public static float Lf_Sf_Lfpi2_Rb(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the csc2_cb function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x + Mathf.Sin(phi);
            float eta = y - 1f - Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            if (u1 < 2f)
            {
                return float.PositiveInfinity;
            }

            //This is the part thats different from the original report:
            t = M(theta);
            u = u1 - 2f;
            v = M(-t - HALF_PI + phi);


            //Check all 2 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + HALF_PI + v;

            return totalLength;
        }



        //8.11: C|C(pi/2)SC(pi/2)|C
        public static float Lf_Rbpi2_Sb_Lbpi2_Rf(RSCar goal, out PathSegmentLengths path)
        {
            float t = 0f; float u = 0f; float v = 0f; path = new PathSegmentLengths(0f, 0f, 0f);

            float x = goal.pos.x;
            float y = goal.pos.z;
            float phi = goal.HeadingInRad;


            //Calculations
            //Uses a modified formula adapted from the c_c2sc2_c function from http://msl.cs.uiuc.edu/~lavalle/cs326a/rs.c
            float xi = x + Mathf.Sin(phi);
            float eta = y - 1f - Mathf.Cos(phi);

            float u1, theta;

            R(xi, eta, out u1, out theta);

            float u1Squared = u1 * u1;

            if (u1Squared < 16f)
            {
                return float.PositiveInfinity;
            }

            //This is the part thats different from the original report:
            u = Mathf.Sqrt(u1Squared - 4f) - 4f;

            if (u < 0f)
            {
                return float.PositiveInfinity;
            }

            float alpha = Mathf.Atan2(2f, u + 4f);

            t = M(HALF_PI + theta + alpha);
            v = M(t - phi);


            //Check all 2 curves
            if (IsCurveSegmentInvalid(t) || IsCurveSegmentInvalid(v))
            {
                return float.PositiveInfinity;
            }

            path.t = t; path.u = u; path.v = v;

            float totalLength = t + u + PI + v;

            return totalLength;
        }



        //
        // Help methods
        //
        private static bool IsCurveSegmentInvalid(float segmentLength)
        {
            //Is invalid if outside [0, pi]
            return segmentLength < 0f || segmentLength > PI;
        }



        //Wrap angle in radians, is called M in the report
        //http://www.technologicalutopia.com/sourcecode/xnageometry/mathhelper.cs.htm
        public static float M(float angle)
        {
            angle = (float)System.Math.IEEERemainder((double)angle, (double)TWO_PI);

            if (angle <= -PI)
            {
                angle += TWO_PI;
            }
            else if (angle > PI)
            {
                angle -= TWO_PI;
            }

            return angle;
        }

        public static float WrapAngleInRadians(float angle)
        {
            return M(angle);
        }



        //From cartesian to polar coordinates, is called R in the report
        private static void R(float x, float y, out float radius, out float angle)
        {
            //Radius
            radius = Mathf.Sqrt(x * x + y * y);
            //Angle
            angle = Mathf.Atan2(y, x);
        }
    }
}
