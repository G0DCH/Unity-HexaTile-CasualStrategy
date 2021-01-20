#pragma warning disable 0649

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

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

        // 현재 열린 패널
        private GameObject openedPanel;

        [Space, SerializeField]
        private GameObject GameOverPanel;
        [SerializeField]
        private Text GameOverText;
        // 점수
        [Space, SerializeField]
        private Text pointText;
        [SerializeField]
        private Text expectBonusText;

        // 선택한 불가사의 버튼
        private Button SelectedWonderButton = null;

        // 툴팁.
        // 타일 속성, 건물 설명 등을 표기함.
        [SerializeField]
        private Text toolTip;

        #region 시대 별로 활성화 될 건물/불가사의 버튼들
        [Title("Buttons Per Age")]
        [SerializeField]
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

        private Dictionary<string, Button> WonderButtonMap { get; } = new Dictionary<string, Button>();

        private void Start()
        {
            GameOverPanel.SetActive(false);
        }

        // 버튼을 눌러 설치할 타일 선택
        public void ButtonSelect(GameObject tilePrefab)
        {
            if (TileManager.Instance.SelectedTile != null)
            {
                if (!(TileManager.Instance.SelectedTile is WonderTile))
                {
                    Destroy(TileManager.Instance.SelectedTile.gameObject);
                }
                else
                {
                    TileManager.Instance.SelectedTile.gameObject.SetActive(false);
                }
                SelectedWonderButton = null;
            }

            TileManager.Instance.InstantiateTile(tilePrefab);

            if (TileManager.Instance.SelectedTile.MyTileBuilding == TileBuilding.Wonder)
            {
                GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
                SelectedWonderButton = selectedObject.GetComponent<Button>();

                if (SelectedWonderButton == null)
                {
                    Debug.LogError("클릭한 UI에 버튼 컴포넌트가 없음.");
                }
            }
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

        // 현재 시대에 해금되는 건물/불가사의 버튼을 활성화,
        // 버튼 맵에 추가함.
        public void InitBuildingButtons()
        {
            switch (AgeManager.Instance.WorldAge)
            {
                case Age.Ancient:
                    InitBuildingButtons(AncientButtons);
                    break;
                case Age.Classical:
                    InitBuildingButtons(ClassicalButtons);
                    break;
                case Age.Medieval:
                    InitBuildingButtons(MedievalButtons);
                    break;
                case Age.Renaissance:
                    InitBuildingButtons(ReneissanceButtons);
                    break;
                case Age.Industrial:
                    InitBuildingButtons(IndustrialButtons);
                    break;
                case Age.Modern:
                    InitBuildingButtons(ModernButtons);
                    break;
                case Age.Atomic:
                    InitBuildingButtons(AtomicButtons);
                    break;
                default:
                    break;
            }
        }

        private void InitBuildingButtons(List<Button> buttons)
        {
            foreach (var button in buttons)
            {
                button.interactable = true;
                WonderButtonMap.Add(button.name.Replace("Button", string.Empty), button);
            }
        }    

        // 게임 오버 패널 켜기
        public void ActiveGameOver()
        {
            GameOverPanel.SetActive(true);
            GameOverText.text = string.Format("GameOver!!\nScore : {0}", GameManager.Instance.Score);
        }

        public void ActiveWin()
        {
            GameOverPanel.SetActive(true);
            GameOverText.text = string.Format("You Win!!!\nScore : {0}", GameManager.Instance.Score);
        }

        // 이미 설치한 불가사의의 버튼을 비 활성화 시킴
        public void DisableWonderButton(string wonderButtonName = null)
        {
            if (wonderButtonName != null)
            {
                if (WonderButtonMap.TryGetValue(wonderButtonName, out Button wonderButton))
                {
                    SelectedWonderButton = wonderButton;
                }
            }

            if (SelectedWonderButton != null)
            {
                SelectedWonderButton.interactable = false;
            }
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

        public void ShowExpectBonus(int expectBonus, Vector3 tilePosition)
        {
            expectBonusText.transform.position = Camera.main.WorldToScreenPoint(tilePosition);
            expectBonusText.text = string.Format("+{0}", expectBonus);
            TurnExpectBonus(true);
        }

        public void TurnExpectBonus(bool isActive)
        {
            expectBonusText.gameObject.SetActive(isActive);
            if (!isActive)
            {
                expectBonusText.text = string.Empty;
            }
        }
    }
}