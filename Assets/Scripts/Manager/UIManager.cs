#pragma warning disable 0649

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

        [Space, SerializeField]
        private Text ageText;

        #region 시대 별로 활성화 될 건물/불가사의 버튼들
        // 시대 별로 활성화 될 건물/불가사의 버튼들
        [Space, SerializeField]
        private List<Button> AncientButtons;
        [SerializeField]
        private List<Button> ClassicalButtons;
        [SerializeField]
        private List<Button> MedievalButtons;
        [SerializeField]
        private List<Button> ReneissanceButtons;
        [SerializeField]
        private List<Button> IndustrialButtons;
        [SerializeField]
        private List<Button> ModernButtons;
        [SerializeField]
        private List<Button> AtomicButtons;
        #endregion

        // 현재 열린 패널
        private GameObject openedPanel;

        [Space, SerializeField]
        private GameObject GameOverPanel;
        private Text GameOverText;
        // 점수
        [Space, SerializeField]
        private Text pointText;

        // 선택한 불가사의 버튼
        private Button SelectedWonderButton = null;

        // 툴팁.
        // 타일 속성, 건물 설명 등을 표기함.
        [SerializeField]
        private Text toolTip;

        private void Start()
        {
            GameOverPanel.SetActive(false);
        }

        // 버튼을 눌러 설치할 타일 선택
        public void ButtonSelect(GameObject tilePrefab)
        {
            if (TileManager.Instance.SelectedTile != null)
            {
                Destroy(TileManager.Instance.SelectedTile.gameObject);
                SelectedWonderButton = null;
            }

            TileManager.Instance.SelectedTile = Instantiate(tilePrefab, Vector3.up * 20f, Quaternion.identity).GetComponent<Tile>();
            TileManager.Instance.SelectedTile.GetComponent<MeshCollider>().enabled = false;

            if (TileManager.Instance.SelectedTile.MyTileBuilding == TileBuilding.Wonder)
            {
                GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
                SelectedWonderButton = selectedObject.GetComponent<Button>();

                if (SelectedWonderButton == null)
                {
                    Debug.LogError("클릭한 UI에 버튼 컴포넌트가 없음.");
                }
            }


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

        // 시대 표기, 요구 점수 텍스트 갱신
        public void UpdateAgeText()
        {
            ageText.text = string.Format("시대 : {0}\n남은 점수 : {1}",
                AgeManager.Instance.WorldAge, Mathf.Clamp(AgeManager.Instance.AgeLimit - GameManager.Instance.Score, 0, int.MaxValue));
        }

        // 현재 시대에 해금되는 건물/불가사의 버튼을 활성화 함.
        public void ActiveBuildingButtons()
        {
            switch (AgeManager.Instance.WorldAge)
            {
                case Age.Ancient:
                    foreach (var button in AncientButtons)
                    {
                        button.interactable = true;
                    }
                    break;
                case Age.Classical:
                    foreach (var button in ClassicalButtons)
                    {
                        button.interactable = true;
                    }
                    break;
                case Age.Medieval:
                    foreach (var button in MedievalButtons)
                    {
                        button.interactable = true;
                    }
                    break;
                case Age.Renaissance:
                    foreach (var button in ReneissanceButtons)
                    {
                        button.interactable = true;
                    }
                    break;
                case Age.Industrial:
                    foreach (var button in IndustrialButtons)
                    {
                        button.interactable = true;
                    }
                    break;
                case Age.Modern:
                    foreach (var button in ModernButtons)
                    {
                        button.interactable = true;
                    }
                    break;
                case Age.Atomic:
                    foreach (var button in AtomicButtons)
                    {
                        button.interactable = true;
                    }
                    break;
                default:
                    break;
            }
        }

        // 게임 오버 패널 켜기
        public void ActiveGameOver()
        {
            GameOverPanel.SetActive(true);
            GameOverText.text = string.Format("GameOver!!\nScore : {0}", GameManager.Instance.Score);
        }

        // 이미 설치한 불가사의의 버튼을 비 활성화 시킴
        public void DisableWonderButton()
        {
            SelectedWonderButton.interactable = false;
            SelectedWonderButton = null;
        }

        // 점수 표기 갱신
        public void RefreshPointText()
        {
            pointText.text = string.Format("Score : {0}\nBuildPoint : {1}",
                GameManager.Instance.Score, GameManager.Instance.BuildPoint);
        }

        // 툴팁 표기 여부
        public void ShowToolTip(bool isShow, Vector3 pos)
        {
            if (isShow)
            {
                toolTip.transform.position = pos;
            }
            toolTip.transform.parent.gameObject.SetActive(isShow);
        }

        public void SetToolTipText(string toolTip)
        {
            this.toolTip.text = toolTip;
        }
    }
}