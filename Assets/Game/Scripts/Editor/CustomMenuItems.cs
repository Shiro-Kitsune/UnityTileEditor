using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TileEditor
{
    public class CustomMenuItems : Editor
    {
        [MenuItem("Assets/Create/Tile Map")]
        public static void createTileMapMenuItem()
        {
            createScriptableObject(typeof(TileMap), "Save Tile Map Data Asset", "Tilemap-001.asset");
        }

        private static void createScriptableObject(System.Type _type, string saveTitle = "Save Asset", string defaultAssetName = "Scriptable.asset")
        {
            string path = EditorUtility.SaveFilePanel(saveTitle, Application.dataPath, defaultAssetName, "asset");
            if (path.Length != 0 && path.Contains(Application.dataPath))
            {
                ScriptableObject asset = ScriptableObject.CreateInstance(_type);
                path = "Assets" + path.Replace(Application.dataPath, "");
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = asset;
            }
        }
    }
}