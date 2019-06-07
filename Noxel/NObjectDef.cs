using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Noxel
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    class NObjectDef : System.Attribute
    {
        private string name;
        public int edges;

        public NObjectDef(string name)
        {
            this.name = name;
            edges = 4;
        }
    }
}
