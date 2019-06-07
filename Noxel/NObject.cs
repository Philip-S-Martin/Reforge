using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Noxel
{
    abstract class NObject
    {
        // for 4 sides:
        // 64*4 for pointers
        // 1 * 160 for points, assuming roughly ` unique per face
        // 4 * 16 for accent indices
        // 288 * 2 for faces
        // 1056 bits average, 132 bytes
        public NPoint[] Point { get; private set; }
        public Face Clockwise { get; private set; }
        public Face CounterClockwise { get; private set; }
        public UInt16[] AccentIndices { get; protected set; }

        // Generic initialization
        protected void Init(int edges)
        {
            this.Point = new NPoint[edges];
            this.AccentIndices = new UInt16[edges];
            this.Clockwise = new Face(this);
            this.CounterClockwise = new Face(this);
        }

        // Used to find which two vertices to base new objects on
        public NPoint[] ClosestPair(Vector3 to)
        {
            int[] closest = { 0, 0 };
            float[] dist = { float.MaxValue, float.MaxValue };
            int idx;
            int count = this.Point.Length;

            // very inefficient search, fine for only 2 values
            for (int i = 0; i < count; i++)
            {
                idx = i;
                float iDist = Vector3.Distance(this.Point[i], to);
                for (int c = 0; c < 2; c++)
                {
                    if (iDist < dist[c])
                    {
                        // swap new closest with old before checking next closest
                        int iswp = closest[c];
                        float dswp = dist[c];
                        closest[c] = idx;
                        dist[c] = iDist;
                        idx = iswp;
                        iDist = dswp;
                    }
                }
            }
            NPoint[] result = { this.Point[closest[0]], this.Point[closest[1]] };
            return result;
        }

        protected bool IsPlanar()
        {
            Vector3 baseNormal = Vector3.Cross(Point[0], Point[1]);
            for (int i = 1; i < Point.Length; i++)
            {
                if(Vector3.Cross(Point[i], Point[(i + 1) % Point.Length]) != baseNormal)
                {
                    return false;
                }
            }
            return true;
        }

        // Return false if current array order cannot create an ordered, convex polygon
        protected bool IsOrderedConvex()
        {
            int count = this.Point.Length;
            float angleSum = 0;
            for (int i = 0; i < Point.Length; i++)
            {
                angleSum += Vector3.SignedAngle(Point[i], Point[(i + 1)%count], Point[(i + 2) % count]);
            }
            if(angleSum != (count-2)*180)
            {
                return false;
            }
            return true;
        }

        // Fix the order of points in the object so that they form an easily traversible/renderable shape
        protected abstract void FixOrder();
        

        // In both Cw and Ccw faces, find the adjacent face on
        protected void FixNormals()
        {

        }

        // Figure out if any walls are shared on A and B, and pick the one with the minimum angle
        // Should set BOTH cw and ccw if the first found face already had adjacency
        // How do I choose cw or ccw?
        public bool MatchAdjacent(NPoint a, NPoint b)
        {
            return false;
        }
    }
}
