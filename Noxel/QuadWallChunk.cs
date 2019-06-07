using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Noxel
{
    class QuadWallChunk : NChunk<QuadWall>
    {
        public QuadWallChunk()
        {
            // Running this first, chaining initialization would
            // run in the incorrect order
            Init(4);
        }
    }
}
