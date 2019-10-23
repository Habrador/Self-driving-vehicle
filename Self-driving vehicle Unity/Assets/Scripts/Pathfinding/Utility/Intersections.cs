using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingForVehicles
{
    //Check if two rectangles are intersecting in 2d space (x,z)
    //http://www.habrador.com/tutorials/linear-algebra/6-triangle-triangle-intersection/
    public static class Intersections
    {
        //
        // Intersection: rectangle-rectangle in 2d space
        //
        public static bool AreRectangleRectangleIntersecting(Rectangle r1, Rectangle r2)
        {
            //AABB with the rectangles as a first test
            if (!AreIntersectingAABB(r1, r2))
            {
                return false;
            }
        
            //r1_t1 with r2_t1 
            if (AreTriangleTriangleIntersecting(r1.FL, r1.FR, r1.BR, r2.FL, r2.FR, r2.BR))
            {
                return true;
            }
            //r1_t1 with r2_t2 
            if (AreTriangleTriangleIntersecting(r1.FL, r1.FR, r1.BR, r2.FL, r2.BR, r2.BL))
            {
                return true;
            }
            //r1_t2 with r2_t1 
            if (AreTriangleTriangleIntersecting(r1.FL, r1.BR, r1.BL, r2.FL, r2.FR, r2.BR))
            {
                return true;
            }
            //r1_t2 with r2_t2 
            if (AreTriangleTriangleIntersecting(r1.FL, r1.BR, r1.BL, r2.FL, r2.BR, r2.BL))
            {
                return true;
            }

            return false;
        }
    

        //
        // Intersection: triangle-triangle in 2d space
        //
        public static bool AreTriangleTriangleIntersecting(
            Vector3 t1_p1, Vector3 t1_p2, Vector3 t1_p3,
            Vector3 t2_p1, Vector3 t2_p2, Vector3 t2_p3)
        {
            bool isIntersecting = false;

            //Create the triangles
            Triangle triangle1 = new Triangle(t1_p1, t1_p2, t1_p3);
            Triangle triangle2 = new Triangle(t2_p1, t2_p2, t2_p3);

            //Method 1 - use multiple tests: AABB-AABB, line segment-line segment, point in triangle
            isIntersecting = AreTriangleTriangleIntersecting(triangle1, triangle2);

            //Method 2 fast
            //TODO Implement the faster but more complicated version

            return isIntersecting;
        }



        //Method 1: Use multiple tests: AABB-AABB, line segment-line segment, point in triangle
        private static bool AreTriangleTriangleIntersecting(Triangle triangle1, Triangle triangle2)
        {
            bool isIntersecting = false;

            //Step 1. AABB intersection
            if (AreApproximatedRectanglesIntersecting(triangle1, triangle2))
            {
                //Step 2. Line segment-triangle intersection
                if (AreAnyLineSegmentsIntersecting(triangle1, triangle2))
                {
                    isIntersecting = true;
                }
                //Step 3. Point-triangle intersection - if one of the triangles is inside the other
                else if (AreCornersIntersecting(triangle1, triangle2))
                {
                    isIntersecting = true;
                }
            }

            return isIntersecting;
        }



        //Approximate the triangles with rectangles to so can check if the rectangles are intersecting
        //To make the AABB algorithm more general
        private static bool AreApproximatedRectanglesIntersecting(Triangle t1, Triangle t2)
        {
            //Find the size of the bounding box

            //Triangle 1
            float t1_minX = Mathf.Min(t1.p1.x, Mathf.Min(t1.p2.x, t1.p3.x));
            float t1_maxX = Mathf.Max(t1.p1.x, Mathf.Max(t1.p2.x, t1.p3.x));
            float t1_minZ = Mathf.Min(t1.p1.z, Mathf.Min(t1.p2.z, t1.p3.z));
            float t1_maxZ = Mathf.Max(t1.p1.z, Mathf.Max(t1.p2.z, t1.p3.z));

            //Triangle 2
            float t2_minX = Mathf.Min(t2.p1.x, Mathf.Min(t2.p2.x, t2.p3.x));
            float t2_maxX = Mathf.Max(t2.p1.x, Mathf.Max(t2.p2.x, t2.p3.x));
            float t2_minZ = Mathf.Min(t2.p1.z, Mathf.Min(t2.p2.z, t2.p3.z));
            float t2_maxZ = Mathf.Max(t2.p1.z, Mathf.Max(t2.p2.z, t2.p3.z));

            //Are the rectangles intersecting?
            bool isIntersecting = AreIntersectingAABB(t1_minX, t1_maxX, t1_minZ, t1_maxZ, t2_minX, t2_maxX, t2_minZ, t2_maxZ);

            return isIntersecting;
        }



        //
        // Intersection: Line segment - triangle
        //

        //Check if any of the edges that make up one of the triangles is intersecting with any of
        //the edges of the other triangle
        private static bool AreAnyLineSegmentsIntersecting(Triangle t1, Triangle t2)
        {
            bool isIntersecting = false;

            //Loop through all edges
            for (int i = 0; i < t1.lineSegments.Length; i++)
            {
                for (int j = 0; j < t2.lineSegments.Length; j++)
                {
                    //The start/end coordinates of the current line segments
                    Vector3 t1_p1 = t1.lineSegments[i].p1;
                    Vector3 t1_p2 = t1.lineSegments[i].p2;
                    Vector3 t2_p1 = t2.lineSegments[j].p1;
                    Vector3 t2_p2 = t2.lineSegments[j].p2;

                    //Are they intersecting?
                    if (AreLineSegmentsIntersecting(t1_p1, t1_p2, t2_p1, t2_p2))
                    {
                        isIntersecting = true;

                        //To stop the outer for loop
                        i = int.MaxValue - 1;

                        break;
                    }
                }
            }

            return isIntersecting;
        }

        //Check if 2 line segments are intersecting in 2d space
        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        //p1 and p2 belong to line 1, p3 and p4 belong to line 2
        private static bool AreLineSegmentsIntersecting(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            bool isIntersecting = false;

            float denominator = (p4.z - p3.z) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.z - p1.z);

            //Make sure the denominator is != 0, if 0 the lines are parallel
            if (denominator != 0)
            {
                float u_a = ((p4.x - p3.x) * (p1.z - p3.z) - (p4.z - p3.z) * (p1.x - p3.x)) / denominator;
                float u_b = ((p2.x - p1.x) * (p1.z - p3.z) - (p2.z - p1.z) * (p1.x - p3.x)) / denominator;

                //Is intersecting if u_a and u_b are between 0 and 1
                if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                {
                    isIntersecting = true;
                }
            }

            return isIntersecting;
        }



        //
        // Intersection: AABB-AABB (Axis-Aligned Bounding Box) - rectangle-rectangle in 2d space with no orientation
        //
        //When a rectangle is created we are automatically creating the values needed for AABB
        public static bool AreIntersectingAABB(Rectangle r1, Rectangle r2)
        {
            bool areIntersecting = AreIntersectingAABB(
                r1.minX, r1.maxX, r1.minZ, r1.maxZ, 
                r2.minX, r2.maxX, r2.minZ, r2.maxZ);

            return areIntersecting;
        }


        //Assumes that we already know the min and max x and z coordinates of each rectangle
        public static bool AreIntersectingAABB(
            float r1_minX, float r1_maxX, float r1_minZ, float r1_maxZ,
            float r2_minX, float r2_maxX, float r2_minZ, float r2_maxZ)
        {
            //If the min of one box in one dimension is greater than the max of another box then the boxes are not intersecting
            //They have to intersect in 2 dimensions. We have to test if box 1 is to the left or box 2 and vice versa
            bool isIntersecting = true;

            //X axis
            if (r1_minX > r2_maxX)
            {
                isIntersecting = false;
            }
            else if (r2_minX > r1_maxX)
            {
                isIntersecting = false;
            }
            //Z axis
            else if (r1_minZ > r2_maxZ)
            {
                isIntersecting = false;
            }
            else if (r2_minZ > r1_maxZ)
            {
                isIntersecting = false;
            }


            return isIntersecting;
        }



        //
        // Intersection: Point in triangle
        //

        //There's a possibility that one of the triangles is smaller than the other
        //So we have to check if any of the triangle's corners is inside the other triangle
        private static bool AreCornersIntersecting(Triangle t1, Triangle t2)
        {
            bool isIntersecting = false;

            //We only have to test one corner from each triangle
            //Triangle 1 in triangle 2
            if (IsPointInTriangle(t1.p1, t2.p1, t2.p2, t2.p3))
            {
                isIntersecting = true;
            }
            //Triangle 2 in triangle 1
            else if (IsPointInTriangle(t2.p1, t1.p1, t1.p2, t1.p3))
            {
                isIntersecting = true;
            }

            return isIntersecting;
        }



        //Is a point p inside a triangle p1-p2-p3?
        //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
        private static bool IsPointInTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            bool isWithinTriangle = false;

            float denominator = ((p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z));

            float a = ((p2.z - p3.z) * (p.x - p3.x) + (p3.x - p2.x) * (p.z - p3.z)) / denominator;
            float b = ((p3.z - p1.z) * (p.x - p3.x) + (p1.x - p3.x) * (p.z - p3.z)) / denominator;
            float c = 1 - a - b;

            //The point is within the triangle if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
            if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }


        //To store triangle data to get cleaner code
        private struct Triangle
        {
            //Corners of the triangle
            public Vector3 p1, p2, p3;
            //The 3 line segments that make up this triangle
            public LineSegment[] lineSegments;

            public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
            {
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;

                lineSegments = new LineSegment[3];

                lineSegments[0] = new LineSegment(p1, p2);
                lineSegments[1] = new LineSegment(p2, p3);
                lineSegments[2] = new LineSegment(p3, p1);
            }
        }



        //To create a line segment
        private struct LineSegment
        {
            //Start/end coordinates
            public Vector3 p1, p2;

            public LineSegment(Vector3 p1, Vector3 p2)
            {
                this.p1 = p1;
                this.p2 = p2;
            }
        }
    }
}
