using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles.ReedsSheppPaths
{
    //Calculate the settings a car need to complete a given reeds-shepp path
    public static class PathSettings
    {
        public static List<SegmentSettings> GetSettings(PathWords word, PathSegmentLengths pathLengths)
        {
            switch (word)
            {
                //8.1: CSC, same turn
                case PathWords.Lf_Sf_Lf: return Lf_Sf_Lf_path(pathLengths);
                case PathWords.Lb_Sb_Lb: return TimeFlip(Lf_Sf_Lf_path(pathLengths));
                case PathWords.Rf_Sf_Rf: return Reflect(Lf_Sf_Lf_path(pathLengths));
                case PathWords.Rb_Sb_Rb: return Reflect(TimeFlip(Lf_Sf_Lf_path(pathLengths)));


                //8.2: CSC, different turn
                case PathWords.Lf_Sf_Rf: return Lf_Sf_Rf_path(pathLengths);
                case PathWords.Lb_Sb_Rb: return TimeFlip(Lf_Sf_Rf_path(pathLengths));
                case PathWords.Rf_Sf_Lf: return Reflect(Lf_Sf_Rf_path(pathLengths));
                case PathWords.Rb_Sb_Lb: return Reflect(TimeFlip(Lf_Sf_Rf_path(pathLengths)));


                //8.3: C|C|C
                case PathWords.Lf_Rb_Lf: return Lf_Rb_Lf_path(pathLengths);
                case PathWords.Lb_Rf_Lb: return TimeFlip(Lf_Rb_Lf_path(pathLengths));
                case PathWords.Rf_Lb_Rf: return Reflect(Lf_Rb_Lf_path(pathLengths));
                case PathWords.Rb_Lf_Rb: return Reflect(TimeFlip(Lf_Rb_Lf_path(pathLengths)));


                //8.4: C|CC
                case PathWords.Lf_Rb_Lb: return Lf_Rb_Lb_path(pathLengths);
                case PathWords.Lb_Rf_Lf: return TimeFlip(Lf_Rb_Lb_path(pathLengths));
                case PathWords.Rf_Lb_Rb: return Reflect(Lf_Rb_Lb_path(pathLengths));
                case PathWords.Rb_Lf_Rf: return Reflect(TimeFlip(Lf_Rb_Lb_path(pathLengths)));


                //8.4: CC|C
                case PathWords.Lf_Rf_Lb: return Lf_Rf_Lb_path(pathLengths);
                case PathWords.Lb_Rb_Lf: return TimeFlip(Lf_Rf_Lb_path(pathLengths));
                case PathWords.Rf_Lf_Rb: return Reflect(Lf_Rf_Lb_path(pathLengths));
                case PathWords.Rb_Lb_Rf: return Reflect(TimeFlip(Lf_Rf_Lb_path(pathLengths)));


                //8.7: CCu|CuC
                case PathWords.Lf_Ruf_Lub_Rb: return Lf_Ruf_Lub_Rb_path(pathLengths);
                case PathWords.Lb_Rub_Luf_Rf: return TimeFlip(Lf_Ruf_Lub_Rb_path(pathLengths));
                case PathWords.Rf_Luf_Rub_Lb: return Reflect(Lf_Ruf_Lub_Rb_path(pathLengths));
                case PathWords.Rb_Lub_Ruf_Lf: return Reflect(TimeFlip(Lf_Ruf_Lub_Rb_path(pathLengths)));


                //8.8: C|CuCu|C
                case PathWords.Lf_Rub_Lub_Rf: return Lf_Rub_Lub_Rf_path(pathLengths);
                case PathWords.Lb_Ruf_Luf_Rb: return TimeFlip(Lf_Rub_Lub_Rf_path(pathLengths));
                case PathWords.Rf_Lub_Rub_Lf: return Reflect(Lf_Rub_Lub_Rf_path(pathLengths));
                case PathWords.Rb_Luf_Ruf_Lb: return Reflect(TimeFlip(Lf_Rub_Lub_Rf_path(pathLengths)));


                //8.9: C|C(pi/2)SC, same turn
                case PathWords.Lf_Rbpi2_Sb_Lb: return Lf_Rbpi2_Sb_Lb_path(pathLengths);
                case PathWords.Lb_Rfpi2_Sf_Lf: return TimeFlip(Lf_Rbpi2_Sb_Lb_path(pathLengths));
                case PathWords.Rf_Lbpi2_Sb_Rb: return Reflect(Lf_Rbpi2_Sb_Lb_path(pathLengths));
                case PathWords.Rb_Lfpi2_Sf_Rf: return Reflect(TimeFlip(Lf_Rbpi2_Sb_Lb_path(pathLengths)));


                //8.10: C|C(pi/2)SC, different turn
                case PathWords.Lf_Rbpi2_Sb_Rb: return Lf_Rbpi2_Sb_Rb_path(pathLengths);
                case PathWords.Lb_Rfpi2_Sf_Rf: return TimeFlip(Lf_Rbpi2_Sb_Rb_path(pathLengths));
                case PathWords.Rf_Lbpi2_Sb_Lb: return Reflect(Lf_Rbpi2_Sb_Rb_path(pathLengths));
                case PathWords.Rb_Lfpi2_Sf_Lf: return Reflect(TimeFlip(Lf_Rbpi2_Sb_Rb_path(pathLengths)));


                //8.9 (reversed): CSC(pi/2)|C, same turn
                case PathWords.Lf_Sf_Rfpi2_Lb: return Lf_Sf_Rfpi2_Lb_path(pathLengths);
                case PathWords.Lb_Sb_Rbpi2_Lf: return TimeFlip(Lf_Sf_Rfpi2_Lb_path(pathLengths));
                case PathWords.Rf_Sf_Lfpi2_Rb: return Reflect(Lf_Sf_Rfpi2_Lb_path(pathLengths));
                case PathWords.Rb_Sb_Lbpi2_Rf: return Reflect(TimeFlip(Lf_Sf_Rfpi2_Lb_path(pathLengths)));


                //8.10 (reversed): CSC(pi/2)|C, different turn
                case PathWords.Lf_Sf_Lfpi2_Rb: return Lf_Sf_Lfpi2_Rb_path(pathLengths);
                case PathWords.Lb_Sb_Lbpi2_Rf: return TimeFlip(Lf_Sf_Lfpi2_Rb_path(pathLengths));
                case PathWords.Rf_Sf_Rfpi2_Lb: return Reflect(Lf_Sf_Lfpi2_Rb_path(pathLengths));
                case PathWords.Rb_Sb_Rbpi2_Lf: return Reflect(TimeFlip(Lf_Sf_Lfpi2_Rb_path(pathLengths)));


                //8.11: C|C(pi/2)SC(pi/2)|C
                case PathWords.Lf_Rbpi2_Sb_Lbpi2_Rf: return Lf_Rbpi2_Sb_Lbpi2_Rf_path(pathLengths);
                case PathWords.Lb_Rfpi2_Sf_Lfpi2_Rb: return TimeFlip(Lf_Rbpi2_Sb_Lbpi2_Rf_path(pathLengths));
                case PathWords.Rf_Lbpi2_Sb_Rbpi2_Lf: return Reflect(Lf_Rbpi2_Sb_Lbpi2_Rf_path(pathLengths));
                case PathWords.Rb_Lfpi2_Sf_Rfpi2_Lb: return Reflect(TimeFlip(Lf_Rbpi2_Sb_Lbpi2_Rf_path(pathLengths)));
            }

            return null;
        }



        //Time-flip transform method from the report, which interchanges + and -
        //l+ r- s- l- -> l- r+ s+ l+
        private static List<SegmentSettings> TimeFlip(List<SegmentSettings> pathSettings)
        {
            foreach (SegmentSettings settings in pathSettings)
            {
                //Set it to forward
                RSCar.Gear flippedGear = RSCar.Gear.Forward;
                
                //If the current setting is forward, then flip
                if (settings.gear == RSCar.Gear.Forward)
                {
                    flippedGear = RSCar.Gear.Back;
                }

                settings.gear = flippedGear;
            }

            return pathSettings;
        }



        //Reflect transform fromt the report, which interchanges r and l 
        //l+ r- s- l- -> r+ l- s- r-
        private static List<SegmentSettings> Reflect(List<SegmentSettings> pathSettings)
        {
            foreach (SegmentSettings settings in pathSettings)
            {
                //Ignore if w are going straight
                if (settings.steering == RSCar.Steering.Straight)
                {
                    continue;
                }
            
                //Set it to right
                RSCar.Steering flippedSteering = RSCar.Steering.Right;

                //If the current setting is right, then flip
                if (settings.steering == RSCar.Steering.Right)
                {
                    flippedSteering = RSCar.Steering.Left;
                }

                settings.steering = flippedSteering;
            }

            return pathSettings;
        }


        //Backwards transform, which follow the path in reverse order, 
        //but with timeflip so the individual segments are transversed in the same direction


        //
        // Settings for individual paths
        //

        //8.1: CSC, same turn
        public static List<SegmentSettings> Lf_Sf_Lf_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Straight, RSCar.Gear.Forward, pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, pathLength.v));

            return pathSettings;
        }

        //8.2: CSC, different turn
        public static List<SegmentSettings> Lf_Sf_Rf_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Straight, RSCar.Gear.Forward, pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right,    RSCar.Gear.Forward, pathLength.v));

            return pathSettings;
        }

        //8.3: C|C|C
        public static List<SegmentSettings> Lf_Rb_Lf_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right, RSCar.Gear.Back,    pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Forward, pathLength.v));

            return pathSettings;
        }

        //8.4: C|CC
        public static List<SegmentSettings> Lf_Rb_Lb_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right, RSCar.Gear.Back,    pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Back,    pathLength.v));

            return pathSettings;
        }

        //8.4: CC|C
        public static List<SegmentSettings> Lf_Rf_Lb_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right, RSCar.Gear.Forward, pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Back,    pathLength.v));

            return pathSettings;
        }

        //8.7: CCu|CuC
        public static List<SegmentSettings> Lf_Ruf_Lub_Rb_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right, RSCar.Gear.Forward, pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Back,    pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right, RSCar.Gear.Back,    pathLength.v));

            return pathSettings;
        }

        //8.8: C|CuCu|C
        public static List<SegmentSettings> Lf_Rub_Lub_Rf_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right, RSCar.Gear.Back,    pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,  RSCar.Gear.Back,    pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right, RSCar.Gear.Forward, pathLength.v));

            return pathSettings;
        }

        //8.9: C|C(pi/2)SC, same turn
        public static List<SegmentSettings> Lf_Rbpi2_Sb_Lb_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right,    RSCar.Gear.Back,    Mathf.PI * 0.5f));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Straight, RSCar.Gear.Back,    pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Back,    pathLength.v));

            return pathSettings;
        }

        //8.10: C|C(pi/2)SC, different turn
        public static List<SegmentSettings> Lf_Rbpi2_Sb_Rb_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right,    RSCar.Gear.Back,    Mathf.PI * 0.5f));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Straight, RSCar.Gear.Back,    pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right,    RSCar.Gear.Back,    pathLength.v));

            return pathSettings;
        }

        //8.9 (reversed): CSC(pi/2)|C, same turn
        public static List<SegmentSettings> Lf_Sf_Rfpi2_Lb_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Straight, RSCar.Gear.Forward, pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right,    RSCar.Gear.Forward, Mathf.PI * 0.5f));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Back,    pathLength.v));

            return pathSettings;
        }

        //8.10 (reversed): CSC(pi/2)|C, different turn
        public static List<SegmentSettings> Lf_Sf_Lfpi2_Rb_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Straight, RSCar.Gear.Forward, pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, Mathf.PI * 0.5f));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right,    RSCar.Gear.Back,    pathLength.v));

            return pathSettings;
        }

        //8.11: C|C(pi/2)SC(pi/2)|C
        public static List<SegmentSettings> Lf_Rbpi2_Sb_Lbpi2_Rf_path(PathSegmentLengths pathLength)
        {
            List<SegmentSettings> pathSettings = new List<SegmentSettings>();

            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Forward, pathLength.t));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right,    RSCar.Gear.Back,    Mathf.PI * 0.5f));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Straight, RSCar.Gear.Back,    pathLength.u));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Left,     RSCar.Gear.Back,    Mathf.PI * 0.5f));
            pathSettings.Add(new SegmentSettings(RSCar.Steering.Right,    RSCar.Gear.Forward, pathLength.v));

            return pathSettings;
        }
    }
}
