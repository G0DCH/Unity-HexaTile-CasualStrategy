using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class World : MonoBehaviour
    {
        [SerializeField, Required] private TerrainRenderer terrainRenderer;
        [SerializeField, Required] private DecorationRenderer decorationRenderer;

        private HexagonInfo[] hexagonInfos;
        private DecorationInfo?[] decorationInfos;

        public Vector2Int WorldSize { get; protected set; }

        private void Awake()
        {
            Debug.Assert(terrainRenderer != null, $"Missing {nameof(terrainRenderer)}");
            Debug.Assert(decorationRenderer != null, $"Missing {nameof(decorationRenderer)}");
        }

        private void Start()
        {
            // init pool
        }

        public void InitializeWorld(Vector2Int worldSize, TerrainData terrainData, DecorationData decorationData)
        {
            WorldSize = worldSize;

            hexagonInfos = new HexagonInfo[WorldSize.x * WorldSize.y];
            for (int i = 0; i < hexagonInfos.Length; i++)
            {
                Center center = terrainData.centers[i];
                hexagonInfos[i] = new HexagonInfo
                {
                    hexPos = center.hexPos,
                    isWater = center.isWater,
                    isSea = center.isSea,
                    isCoast = center.isCoast,
                    hasRiver = center.NeighborCorners.Any(x => x.river > 0),
                    biome = terrainData.biomeTable.biomeDictionary[center.biomeId],
                };
            }
            decorationInfos = decorationData.decorationInfos;

            terrainRenderer.SpawnHexagonTerrains(terrainData);
            decorationRenderer.SpawnDecorations(WorldSize, decorationData.renderDatas);

            if (FindObjectOfType<TileManager>() != null)
            {
                TileManager.Instance.InitTileMap();
            }
        }

        [Button]
        protected void CleanUp()
        {
            hexagonInfos = null;
            decorationInfos = null;

            terrainRenderer.CleanUpHexagons();
            decorationRenderer.CleanUpDecorations();
        }

        /// <summary>
        /// 특정 위치에 있는 <see cref="HexagonInfo"/> 반환
        /// </summary>
        public HexagonInfo GetHexagonInfoAt(HexagonPos hexPos)
        {
            Vector2Int arrayXY = hexPos.ToArrayXY();
            int index = arrayXY.x + arrayXY.y * WorldSize.x;
            if (index < 0 || index >= hexagonInfos.Length)
            {
                throw new IndexOutOfRangeException(nameof(hexPos));
            }

            return hexagonInfos[index];
        }

        /// <summary>
        /// 범위 내에 있는 모든 <see cref="HexagonInfo"/>들을 반환
        /// </summary>
        /// <param name="hexPos">중심 위치</param>
        /// <param name="distanceFrom">중심으로 부터 최소 거리</param>
        /// <param name="distanceTo">중심으로 부터 최대 거리</param>
        public IEnumerable<HexagonInfo> GetHexagonInfosInRange(HexagonPos hexPos, int distanceFrom, int distanceTo)
        {
            foreach (int index in GetRangeIndexes(hexPos, distanceFrom, distanceTo))
            {
                yield return hexagonInfos[index];
            }
        }

        /// <summary>
        /// 특정 위치에 있는 <see cref="DecorationInfo"/> 반환
        /// </summary>
        public DecorationInfo? GetDecorationInfoAt(HexagonPos hexPos)
        {
            int index = hexPos.ToArrayIndex(WorldSize.x);
            if (index < 0 || index >= hexagonInfos.Length)
            {
                throw new IndexOutOfRangeException(nameof(hexPos));
            }

            return decorationInfos[index];
        }

        /// <summary>
        /// 범위 내에 있는 모든 <see cref="DecorationInfo"/>들을 반환
        /// </summary>
        /// <param name="hexPos">중심 위치</param>
        /// <param name="distanceFrom">중심으로 부터 최소 거리</param>
        /// <param name="distanceTo">중심으로 부터 최대 거리</param>
        public IEnumerable<DecorationInfo?> GetNeighborDecorationInfos(HexagonPos hexPos, int distanceFrom, int distanceTo)
        {
            foreach (int index in GetRangeIndexes(hexPos, distanceFrom, distanceTo))
            {
                yield return decorationInfos[index];
            }
        }

        private IEnumerable<int> GetRangeIndexes(HexagonPos hexPos, int from, int to)
        {
            if (from < 0 || to < from)
            {
                throw new InvalidOperationException($"잘못된 범위, from: {from}, to: {to}");
            }

            for (int hexZ = -to; hexZ <= to; hexZ++)
            {
                for (int hexX = -to; hexX <= to; hexX++)
                {
                    HexagonPos neighborHexOffset = new HexagonPos(hexX, hexZ);
                    int offsetDistance = neighborHexOffset.HexagonDistance;
                    if (offsetDistance < from || offsetDistance > to)
                    {
                        continue;
                    }

                    Vector2Int neighborXY = (hexPos + neighborHexOffset).ToArrayXY();
                    if (neighborXY.x < 0 || neighborXY.x >= WorldSize.x || neighborXY.y < 0 || neighborXY.y >= WorldSize.y)
                    {
                        continue;
                    }

                    int neighborIndex = neighborXY.x + neighborXY.y * WorldSize.x;
                    yield return neighborIndex;
                }
            }
        }
    }
}