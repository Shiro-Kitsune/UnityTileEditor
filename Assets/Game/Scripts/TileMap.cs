using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace TileEditor
{
    [System.Serializable]
    public class TileMap : ScriptableObject
    {
        // Layer information
        [XmlIgnore]
        public int nextLayerId;
        [XmlIgnore]
        public List<string> layerNames = new List<string>();
        [XmlIgnore]
        public List<int> layers = new List<int>();
        // Key is layer number, value is an actual map grid
        [XmlElement("Map")]
        public GridDictionary MapRepresentation = new GridDictionary();

        // Tileset information
        public string SpriteSheetName;
        public List<Sprite> Sprites = new List<Sprite>();
        public TileDictionary Tiles = new TileDictionary();
        [XmlIgnore]
        public int nextTileId;

        // Creates layer and returns its ID
        public int CreateNewLayer()
        {
            layers.Add(nextLayerId);
            layerNames.Add("Layer" + nextLayerId);
            MapRepresentation.Add(nextLayerId, new MapGrid());
            return nextLayerId++;
        }

        // Deletes layer and returns ID of the next layer to select in editor
        public int DeleteLayer(int layerToRemove)
        {
            // Allow layer removal only if there are more than 1 layer
            if (layers.Count > 1)
            {
                layers.Remove(layerToRemove);
                layerNames.Remove("Layer" + layerToRemove);
                MapRepresentation.Remove(layerToRemove);
            }
            return layers[0];
        }
    }

    // Dictionary is not serializable by default, so it must be overriden
    [System.Serializable]
    public class GridDictionary : SerializableDictionary<int, MapGrid>
    {
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer valueSerializer = new XmlSerializer(typeof(MapGrid));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
            {
                return;
            }

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Layer");
                reader.ReadStartElement("Id");
                int key = reader.ReadElementContentAsInt();
                reader.ReadEndElement();

                MapGrid value = (MapGrid)valueSerializer.Deserialize(reader);

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer valueSerializer = new XmlSerializer(typeof(MapGrid));

            foreach (int key in this.Keys)
            {
                writer.WriteStartElement("Layer");
                writer.WriteStartElement("Id");
                writer.WriteValue(key);
                writer.WriteEndElement();

                MapGrid value = this[key];
                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();
            }
        }
    }

    // Dictionary is not serializable by default, so it must be overriden
    [System.Serializable]
    public class TileDictionary : SerializableDictionary<int, Tile>
    {
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer valueSerializer = new XmlSerializer(typeof(Tile));

            foreach (int key in this.Keys)
            {
                Tile value = this[key];
                valueSerializer.Serialize(writer, value);
            }
        }
    }
}