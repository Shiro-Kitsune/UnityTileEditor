using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace TileEditor
{
    [System.Serializable]
    public class Tile
    {
        public enum TileTypes
        {
            None,
            Player,
            Enemy,
            Item,
            Collidable,
            Destructible
        }
        [XmlIgnore]
        public TileTypes TileType;

        public int Id;
        public int SpriteId;
        public List<Component> Components;

        public Tile()
        {
            this.Id = -1;
            Components = new List<Component>();
        }

        public Tile(int id, int spriteId)
        {
            this.Id = id;
            SpriteId = spriteId;
            Components = new List<Component>();
        }

        public T FindComponent<T>() where T : Component
        {
            return (T)Components.Find(c => c.GetType() == typeof(T));
        }
    }
}