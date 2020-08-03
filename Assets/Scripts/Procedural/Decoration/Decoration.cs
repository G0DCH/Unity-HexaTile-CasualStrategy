using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilePuzzle.Procedural
{
    public struct Decoration
    {
        public string name;
        public Type type;
        public bool isDestructible;

        public Decoration(string name, Type type, bool isDestructible)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.type = type;
            this.isDestructible = isDestructible;
        }

        public enum Type
        {
            Vegetation, Mountain, Forest,
        }
    }
}
