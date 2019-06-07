using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Noxel
{
    class Face
    {
        // 64*3 for pointers
        // 16*4 for adjacent indices
        // 32 for thickness
        // 288 bits
        public NObject Parent { get; private set; }
        private Face[] Adjacent;
        private UInt16[] AdjacentIdx;
        public float Thickness { get; private set; }     
        // Material variable -- Object or simple IO flag for an object?

        // is offset even necessary? could it just be calculated for every fresh render?
        // when we are doing LODs it will be ignored 50% of the time anyway
        // DO PERFORMANCE TESTING
        public SerializableVector3[] Offset { get; protected set; }


        public Face(NObject parent) 
        {
            this.Parent = parent;
            Adjacent = new Face[Parent.Point.Length];
            AdjacentIdx = new UInt16[Parent.Point.Length];

            Offset = new SerializableVector3[4];
        }


        public void SetOffset(int i, Vector3 value)
        {
            if (i < Adjacent.Length && i >= 0)
                Offset[i] = value;
        }



        public Face this[int i]
        {
            get { return Adjacent[i]; }
            set { this.Adjacent[i] = value; }
        }

        // Adjust offsets relating to a specific face
        public bool AdjustOffsets(int iFace)
        {
            
            return true;
        }

        public void SetAdjacent(int index, Face other)
        {
            bool prevMatched = false;
            NPoint[] otherPoint = other.Parent.Point;
            NPoint[] thisPoint = this.Parent.Point;

            for (UInt16 i = 0; i <= thisPoint.Length; i++)
            {
                for(UInt16 n = 0; n <= otherPoint.Length; n++)
                {
                    if(thisPoint[i % thisPoint.Length] == otherPoint[n % otherPoint.Length])
                    {
                        if (prevMatched)
                        {
                            // Should we be storing exactly which edges are adjacent?
                            Adjacent[i - 1] = other;
                            AdjacentIdx[i - 1] = n;
                        }
                        prevMatched = true;
                    }
                    else
                    {
                        prevMatched = false;
                    }
                }
            }
        }

        public bool IsAdjacentTo(Face other)
        {
            return this.Adjacent.Contains<Face>(other);
        }
    }
}
