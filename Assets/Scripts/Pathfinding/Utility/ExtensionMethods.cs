using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    public static class ExtensionMethods
    {
        //Vector3 -> Vector2
        public static Vector2 XZ(this Vector3 v_3D)
        {
            Vector2 v_2D = new Vector2(v_3D.x, v_3D.z);

            return v_2D;
        }

        //Vector2 -> Vector3
        public static Vector3 XYZ(this Vector2 v_2D)
        {
            Vector3 v_3D = new Vector3(v_2D.x, 0f, v_2D.y);

            return v_3D;
        }
    }
}
