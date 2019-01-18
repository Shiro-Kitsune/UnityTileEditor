using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEditor
{
    [System.Serializable]
    public class Destructible : Component
    {
        public int Lives;

        public Destructible()
        {
            Lives = 1;
        }
    }
}