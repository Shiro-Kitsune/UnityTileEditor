using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace TileEditor
{
    [System.Serializable]
    [XmlInclude(typeof(Collidable))]
    [XmlInclude(typeof(Destructible))]
    [XmlInclude(typeof(Enemy))]
    [XmlInclude(typeof(Item))]
    [XmlInclude(typeof(Player))]
    [XmlRoot("Component")]
    public class Component : ScriptableObject
    {
        public Component()
        {
        }
    }
}