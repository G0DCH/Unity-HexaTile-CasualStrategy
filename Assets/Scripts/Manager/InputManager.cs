using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilePuzzle.Utility;
using UnityEngine;

namespace TilePuzzle
{
    public class InputManager : Singleton<InputManager>
    {
        public event EventHandler<Vector2> OnClick;
        public event EventHandler<Vector2> OnDrag;
        public event EventHandler<float> OnZoom;

        private void Update()
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                OnZoom?.Invoke(this, Input.mouseScrollDelta.y > 0 ? 1 : -1);
            }

            if (Input.GetMouseButtonDown(0))
            {
                OnClick?.Invoke(this, Input.mousePosition);
            }


        }
    }
}
