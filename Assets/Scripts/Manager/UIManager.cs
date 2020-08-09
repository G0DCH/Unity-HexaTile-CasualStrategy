using UnityEngine;

namespace TilePuzzle
{
    public class UIManager : Utility.Singleton<UIManager>
    {
        private bool isBuildingOn = false;

        public GameObject BuildingButtonPanel;
        public GameObject WonderButtonPanel;

        // 타일 선택
        public void ButtonSelect(GameObject tilePrefab)
        {

            if (TileManager.Instance.SelectedTile != null)
            {
                Destroy(TileManager.Instance.SelectedTile.gameObject);
            }

            TileManager.Instance.SelectedTile = Instantiate(tilePrefab, Vector3.up * 20f, Quaternion.identity).GetComponent<Tile>();
            TileManager.Instance.SelectedTile.GetComponent<MeshCollider>().enabled = false;

            TileManager.Instance.SelectedTile.MakeGrid(TileManager.Instance.GridPrefab);
            TileManager.Instance.SelectedTile.TurnGrid(false);
        }

        public void ChangePanel()
        {
            if (isBuildingOn)
            {
                BuildingButtonPanel.SetActive(false);
                WonderButtonPanel.SetActive(true);
                isBuildingOn = false;
            }
            else
            {
                WonderButtonPanel.SetActive(false);
                BuildingButtonPanel.SetActive(true);
                isBuildingOn = true;
            }
        }
    }
}