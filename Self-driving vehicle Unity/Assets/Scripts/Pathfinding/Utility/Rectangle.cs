using UnityEngine;
using System.Collections;

namespace PathfindingForVehicles
{
    //A rectangle with 4 corners
    public struct Rectangle
    {
        public Vector3 FL, FR, BL, BR;

        //The AABB of the rectangle, which may speed up obstacle intersection tests
        public float minX, minZ, maxX, maxZ;


        public Rectangle(Vector3 FL, Vector3 FR, Vector3 BL, Vector3 BR)
        {
            this.FL = FL;
            this.FR = FR;
            this.BL = BL;
            this.BR = BR;

            //Create the AABB
            this.minX = Mathf.Min(FL.x, Mathf.Min(FR.x, Mathf.Min(BL.x, BR.x)));
            this.minZ = Mathf.Min(FL.z, Mathf.Min(FR.z, Mathf.Min(BL.z, BR.z)));
            this.maxX = Mathf.Max(FL.x, Mathf.Max(FR.x, Mathf.Max(BL.x, BR.x)));
            this.maxZ = Mathf.Max(FL.z, Mathf.Max(FR.z, Mathf.Max(BL.z, BR.z)));
        }


        //The center is the average of the four corners
        public Vector3 Center
        {
            get 
            {
                Vector3 center = (FL + FR + BL + BR) / 4f;

                return center;
            }
        }
    }
}
