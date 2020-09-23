using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TilePuzzle
{
    public class WonderToolTipInit: MonoBehaviour
    {
        public List<WonderTile> wonderTiles;
        public WonderDataTable wonderDataTable;

        [Button]
        public void InitTable()
        {
            wonderDataTable.WonderDatas.Clear();

            foreach (var wonderTile in wonderTiles)
            {
                // TilePuzzle.~~ 이 형식으로 나오기 때문에 뒤에꺼만 갖고옴.
                string wonderName = wonderTile.GetType().ToString().Split('.')[1];
                WonderData wonderData = 
                    new WonderData(wonderTile.WonderAge, wonderName, wonderTile.Cost, wonderTile.WonderBonus);
                wonderDataTable.WonderDatas.Add(wonderData);
            }
        }
    }
}