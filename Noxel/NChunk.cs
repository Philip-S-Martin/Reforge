using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Noxel
{
    abstract class NChunk<T> : MonoBehaviour where T : NObject
    {
        private List<T> Item { get; set; }
        private List<Mesh> BaseMesh { get; set; }
        private List<Mesh> AccentMesh { get; set; }
        public int BaseItemWidth { get; private set; }
        public int AccentWidth { get; private set; }
        public int MeshMax { get; private set; }

        // Generic initialization
        protected void Init(int edges)
        {
            // How many triangles based on num of edges on primary faces
            BaseItemWidth = (edges - 2) * 3;
            // Max number of items per mesh
            MeshMax = (65535 - 65535 % BaseItemWidth) / BaseItemWidth; 
            AccentWidth = 2 * 3; // One quad (two triangles)

            Item = new List<T>();
            BaseMesh = new List<Mesh>();
            AccentMesh = new List<Mesh>();


        }

        public void Insert(T newObject)
        {
            Item.Add(newObject);
            // Assuming we have added to the end of the list
            RenderItem(Item.Count - 1);
        }

        public bool RenderItem(int index)
        {
            return true;
        }

        // Return the first mesh index associated with an item
        public int GetMeshIndex(ref int rawIndex)
        {
            return rawIndex % MeshMax * BaseItemWidth;
        }

        // Return the correct composite mesh associated wwith an item
        public Mesh GetWhichMesh(ref int rawIndex)
        {
            return BaseMesh[rawIndex/(MeshMax * BaseItemWidth)];
        }

        // Return the mesh face's associated Item index
        public int GetItemIndex(int meshIndex, Mesh fromMesh)
        {
            for (int i = 0; i < BaseMesh.Count; i++)
            {
                if (fromMesh == BaseMesh[i])
                {
                    return i * MeshMax + meshIndex / BaseItemWidth;
                }
            }
            return -1;
        }
    }
}
