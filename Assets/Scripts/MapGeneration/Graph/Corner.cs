using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class Corner : IEquatable<Corner>
    {
        public readonly Vector3 cornerPos;

        public bool isWater = false;
        public bool isSea = false;
        public bool isCoast = false;
        public bool isBorder = false;
        public float elevation = 0f;
        public float moisture = 0f;
        //public float temperature = 0f;
        public int river = 0;
        public Corner downslope;

        private readonly List<Center> neighborCenters;
        private readonly List<Corner> neighborCorners;

        public IReadOnlyList<Center> NeighborCenters => neighborCenters;
        public IReadOnlyList<Corner> NeighborCorners => neighborCorners;

        public Corner(Vector3 cornerPos)
        {
            this.cornerPos = cornerPos;

            neighborCenters = new List<Center>();
            neighborCorners = new List<Corner>();
        }

        public static void Connect(Corner cornerA, Corner cornerB)
        {
            if (cornerA.neighborCorners.Contains(cornerB) == false)
            {
                cornerA.neighborCorners.Add(cornerB);
                cornerB.neighborCorners.Add(cornerA);
            }
        }

        public void AddNeighborCenter(Center center)
        {
            if (center != null && neighborCenters.Contains(center) == false)
            {
                neighborCenters.Add(center);
            }
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as Corner);
        }

        public bool Equals(Corner other)
        {
            return other != null &&
                   cornerPos.Equals(other.cornerPos);
        }

        public override int GetHashCode()
        {
            return -1942004006 + cornerPos.GetHashCode();
        }

        public static bool operator ==(Corner left, Corner right)
        {
            return EqualityComparer<Corner>.Default.Equals(left, right);
        }

        public static bool operator !=(Corner left, Corner right)
        {
            return !(left == right);
        }
    }
}
