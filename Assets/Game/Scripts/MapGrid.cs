using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace TileEditor
{
    [System.Serializable]
    public class MapGrid
    {
        public List<int> Cells;

        public MapGrid()
        {
            Cells = new List<int>();
            for (int i = 0; i < 225; i++)
            {
                Cells.Add(-1);
            }
        }
    }
}