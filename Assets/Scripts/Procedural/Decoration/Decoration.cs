using System;

namespace TilePuzzle.Procedural
{
    /// <summary>
    /// 데코레이션 이름, 타입, 파괴가능 여부
    /// </summary>
    public struct Decoration
    {
        public readonly string name;
        public readonly Type type;
        public readonly bool isDestructible;

        /// <param name="name">데코레이션 이름</param>
        /// <param name="type">데코레이션 타입</param>
        /// <param name="isDestructible">파괴 가능한 데코레이션?</param>
        public Decoration(string name, Type type, bool isDestructible)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.type = type;
            this.isDestructible = isDestructible;
        }

        /// <summary>
        /// 데코레이션 타입
        /// </summary>
        public enum Type
        {
            Vegetation, Mountain, Forest,
        }
    }
}
