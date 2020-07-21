using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TilePuzzle
{
    public class TestUI : MonoBehaviour
    {
        public Button toggleGraphy;
        public Button toggleGraphySetting;
        public Button showHexagon1;
        public Button showHexagon2;
        public Button randomColor;

        public GameObject graphy;
        public GameObject graphySetting;
        public GameObject hexagon1;
        public GameObject hexagon2;

        private void Start()
        {
            toggleGraphy.onClick.AddListener(() => graphy.SetActive(!graphy.activeSelf));
            toggleGraphySetting.onClick.AddListener(() => graphySetting.SetActive(!graphySetting.activeSelf));
            showHexagon1.onClick.AddListener(() => hexagon1.SetActive(!hexagon1.activeSelf));
            showHexagon2.onClick.AddListener(() => hexagon2.SetActive(!hexagon2.activeSelf));
            randomColor.onClick.AddListener(() =>
            {
                var properties = new MaterialPropertyBlock();
                foreach (Transform child in hexagon2.transform)
                {
                    var meshRenderer = child.GetComponent<MeshRenderer>();
                    properties.SetColor("_Color", new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
                    meshRenderer.SetPropertyBlock(properties);
                }
            });
        }
    }
}
