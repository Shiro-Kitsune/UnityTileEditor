using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEditor
{
    [System.Serializable]
    public class Collidable : Component
    {
        public bool IsTrigger;

        public Collidable()
        {
        }

        public Collidable(bool isTrigger)
        {
            IsTrigger = isTrigger;
        }
    }
}