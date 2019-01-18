using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEditor
{
    [System.Serializable]
    public class Item : Component
    {
        public int RestoreHealthBy;
        public int IncreaseBombsBy;

        public Item()
        {
        }
    }
}