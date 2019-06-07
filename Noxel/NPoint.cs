using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace Assets.Noxel
{
    class NPoint
    {
        // 64 + 32*3 = 160
        public SerializableVector3 Position { get; set; }
        List<NObject> Children;

        public static implicit operator Vector3(NPoint p)
        {
            return p.Position;
        }

        public bool Equals(Vector3 v3)
        {
            return this.Position == v3;
        }
    }
}
