using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Noxel
{
    class QuadWall : NObject
    {
        
        public QuadWall()
        {
            // Running this first, chaining initialization would
            // run in the incorrect order
            Init(4);
        }

        // to should be a standardized position created by comparing from(if it exists), the ray cast, and the ray hit
        public QuadWall(NObject fromObject, Vector3 from, Vector3 to) : this()
        {
            // Set two points to match the nearest two on the object we are starting from
            // Only using indices 0 and 1 is important, because correcting quad rotational
            // traversal order is done by swapping 3 and 4 to create order 1, 2, 3, 4
            NPoint[] adjacent = fromObject.ClosestPair(from).ToArray();
            this.Point[0] = adjacent[0];
            this.Point[1] = adjacent[1];

        }
        
        // Fix the order of points in the object so that they form an easily traversible/renderable shape
        protected override void FixOrder()
        {
            // Because this is a convex quad, reordering only requires swapping any 2 indices.
            if (!IsOrderedConvex())
            {
                NPoint swapper = Point[3];
                Point[3] = Point[4];
                Point[4] = swapper;
            }
        }
    }
}
