using UnityEngine;
using UnityEngine.UI;

namespace TilePuzzle
{
    public class UIManager : Utility.Singleton<UIManager>
    {
        private enum PanelFlag { Building, WonderButton, Wonder }

        private PanelFlag panelFlag = PanelFlag.Building;

        [SerializeField]
        private GameObject BuildingButtonPanel;
        [SerializeField]
        private GameObject WonderButtonPanel;

        [SerializeField]
        private Text ageText;

        // 현재 열린 패널
        private GameObject openedPanel;

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

        // 건물, 불가사의 패널 전환
        public void ChangePanel()
        {
            if (panelFlag == PanelFlag.Building)
            {
                BuildingButtonPanel.SetActive(false);
                WonderButtonPanel.SetActive(true);
                panelFlag = PanelFlag.WonderButton;
                openedPanel = WonderButtonPanel;
            }
            else if (panelFlag == PanelFlag.WonderButton)
            {
                WonderButtonPanel.SetActive(false);
                BuildingButtonPanel.SetActive(true);
                panelFlag = PanelFlag.Building;
                openedPanel = BuildingButtonPanel;
            }
        }

        // 해당 시대의 불가사의 패널을 염.
        public void OpenWonderPanel(GameObject panel)
        {
            WonderButtonPanel.SetActive(false);
            panel.SetActive(true);
            openedPanel = panel;
            panelFlag = PanelFlag.Wonder;
        }

        // 이전 패널로 돌아가기.
        public void ReturnButton()
        {
            if (panelFlag == PanelFlag.WonderButton)
            {
                WonderButtonPanel.SetActive(false);
                BuildingButtonPanel.SetActive(true);
                panelFlag = PanelFlag.Building;
                openedPanel = BuildingButtonPanel;
            }
            else if (panelFlag == PanelFlag.Wonder)
            {
                openedPanel.SetActive(false);
                WonderButtonPanel.SetActive(true);
                panelFlag = PanelFlag.WonderButton;
                openedPanel = WonderButtonPanel;
            }
        }

        // 시대 표기 텍스트 갱신
        public void UpdateAgeText()
        {
            ageText.text = AgeManager.Instance.WorldAge.ToString();
        }
    }
}