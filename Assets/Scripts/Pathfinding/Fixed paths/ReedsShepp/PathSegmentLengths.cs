using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PathfindingForVehicles.ReedsSheppPaths
{
    //Each path consists of 3 segments calles tuv that have different lengths
    //There may be more segment, but they have constant length
    public struct PathSegmentLengths
    {
        public float t, u, v;

        public PathSegmentLengths(float t, float u, float v)
        {
            this.t = t;
            this.u = u;
            this.v = v;
        }
    }
}
