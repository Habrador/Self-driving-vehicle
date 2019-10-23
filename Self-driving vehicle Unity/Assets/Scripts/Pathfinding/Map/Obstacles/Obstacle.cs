using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    //A help struct to better control the obstacles
    public class Obstacle
    {
        //The center of the obstacle
        public Vector3 centerPos;

        //The coordinates of the corners in 2d space
        public Rectangle cornerPos;

        public Obstacle(Transform trans)
        {
            this.centerPos = trans.position;

            //All obstacles are rectangles
            float zHalfLength = trans.localScale.z * 0.5f;
            float xHalfLength = trans.localScale.x * 0.5f;

            Vector3 FL = trans.position + trans.forward * zHalfLength - trans.right * xHalfLength;
            Vector3 FR = trans.position + trans.forward * zHalfLength + trans.right * xHalfLength;
            Vector3 BL = trans.position - trans.forward * zHalfLength - trans.right * xHalfLength;
            Vector3 BR = trans.position - trans.forward * zHalfLength + trans.right * xHalfLength;

            this.cornerPos = new Rectangle(FL, FR, BL, BR);
        }
    }
}
