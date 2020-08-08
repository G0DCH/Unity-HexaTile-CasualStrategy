using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public struct Biome
    {
        /// <summary>
        /// 바이옴 유니크 아이디
        /// </summary>
        public readonly int id;
        /// <summary>
        /// 바이옴 이름
        /// </summary>
        public readonly string name;
        /// <summary>
        /// 바이옴 대표 색
        /// </summary>
        public readonly Color color;
        /// <summary>
        /// 바이옴 태그
        /// </summary>
        public readonly HashSet<string> tags;

        /// <param name="name">바이옴 이름</param>
        /// <param name="color">바이옴 대표 색</param>
        /// <param name="tags">바이옴 태그</param>
        public Biome(string name, Color color, List<string> tags)
        {
            id = BiomeNameToId(name);
            this.name = name;
            this.color = color;
            this.tags = new HashSet<string>(tags);
        }

        public static int BiomeNameToId(string biomeName)
        {
            return StringHash.SDBMLower(biomeName);
        }
    }
}
