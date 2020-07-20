using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    public class UIManager : Utility.Singleton<UIManager>
    {
        // 타일 선택
        public void ButtonSelect(GameObject tilePrefab)
        {

            if (TileManager.Instance.SelectedTile != null)
            {
                Destroy(TileManager.Instance.SelectedTile.gameObject);
            }

            TileManager.Instance.SelectedTile = Instantiate(tilePrefab, Vector3.up * 20f, Quaternion.identity).GetComponent<Tile>();
            TileManager.Instance.SelectedTile.GetComponent<MeshCollider>().enabled = false;

            // 타입 변경
            string[] tileName = TileManager.Instance.SelectedTile.name.Split('(');
            TileType tileType = TileManager.Instance.StringToType(tileName[0]);
            TileManager.Instance.SelectedTile.ChangeTileType(tileType);

            TileManager.Instance.SelectedTile.ChangeMaterial(true);
            TileManager.Instance.SelectedTile.MakeGrid(TileManager.Instance.GridPrefab);
            TileManager.Instance.SelectedTile.TurnGrid(false);
        }
    }
}