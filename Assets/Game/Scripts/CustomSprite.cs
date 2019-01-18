using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEditor
{
    [System.Serializable]
    public class Sprite
    {
        public Rect DrawArea;

        public Sprite()
        {
            this.DrawArea = new Rect(0, 0, 32, 32);
        }
    }
}