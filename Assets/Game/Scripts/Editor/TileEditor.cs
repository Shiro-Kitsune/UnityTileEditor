using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace TileEditor
{
    public class TileEditor : EditorWindow
    {
        public enum EditState
        {
            Paint,
            Erase
        };
        private EditState state = EditState.Paint;

        TileMap tilemap;
        //Common
        private readonly float toolsWindowWidth = 270;
        //Toolbar
        private int toolbarSelection;
        private string[] toolbarButtons = { "Paint", "Erase" };
        //Tiles
        private int selectedTileId;
        private List<Rect> gridCells;
        private Vector2 tilesScrollPosition;
        //Layers
        private int selectedLayer;
        //Spritesheet
        private Texture spritesheet;
        //Editor
        private Rect editorWindowRect;
        private readonly int mapCellSize = 50;
        private readonly int mapSize = 15;

        private void OnEnable()
        {
            NewTileMap();
            gridCells = new List<Rect>();

            // Create map grid for tiles
            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    float xCoord = x * mapCellSize;
                    float yCoord = y * mapCellSize + 45; // 45 is offset from toolbar
                    Rect r = new Rect(xCoord, yCoord, mapCellSize, mapCellSize);
                    gridCells.Add(r);
                }
            }
        }

        [MenuItem("Tools/Tile Editor")]
        public static void getWindow()
        {
            UnityEditor.EditorWindow.GetWindow<TileEditor>();
        }

        public TileEditor()
        {
            this.titleContent = new GUIContent("Tile Editor");
            this.minSize = new Vector2(1300, 800);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            #region FileMenu
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("File", EditorStyles.toolbarDropDown))
            {
                GenericMenu toolsMenu = new GenericMenu();
                toolsMenu.AddItem(new GUIContent("New"), false, NewTileMap);
                toolsMenu.AddItem(new GUIContent("Load"), false, LoadTileMap);
                toolsMenu.AddItem(new GUIContent("Save"), false, SaveTileMap);
                toolsMenu.AddItem(new GUIContent("Export"), false, ExportTileMap);
                toolsMenu.DropDown(new Rect(0, 0, 0, 16));
            }
            GUILayout.EndHorizontal();
            #endregion

            #region Toolbar
            toolbarSelection = GUILayout.Toolbar(toolbarSelection, toolbarButtons, GUILayout.Width(100));
            if (toolbarSelection == 0)
            {
                state = EditState.Paint;
            }
            else
            {
                state = EditState.Erase;
            }
            #endregion

            #region Windows
            float verticalOffset = 44f;
            BeginWindows();
            editorWindowRect = new Rect(0, verticalOffset, position.width - toolsWindowWidth * 2, position.height - verticalOffset);
            editorWindowRect = GUILayout.Window(1, editorWindowRect, EditorWindow, "Map");

            float layersWindowHeight = (position.height - verticalOffset) * 0.2f;
            Rect layersWindowRect = new Rect(position.width - toolsWindowWidth, verticalOffset, toolsWindowWidth, layersWindowHeight);
            layersWindowRect = GUILayout.Window(2, layersWindowRect, LayersWindow, "Layers");

            float spritesheetWindowHeight = (position.height - verticalOffset) * 0.3f;
            Rect spritesheetWindowRect = new Rect(position.width - toolsWindowWidth, verticalOffset + layersWindowHeight, toolsWindowWidth, spritesheetWindowHeight);
            spritesheetWindowRect = GUILayout.Window(3, spritesheetWindowRect, SpritesheetWindow, "Spritesheet");

            float propertiesWindowHeight = (position.height - verticalOffset) * 0.5f;
            Rect propertiesWindowRect = new Rect(position.width - toolsWindowWidth, verticalOffset + spritesheetWindowHeight + layersWindowHeight, toolsWindowWidth, propertiesWindowHeight);
            propertiesWindowRect = GUILayout.Window(4, propertiesWindowRect, PropertiesWindow, "Tile Properties");

            Rect tilesWindowRect = new Rect(position.width - toolsWindowWidth * 2, verticalOffset, toolsWindowWidth, position.height - verticalOffset);
            tilesWindowRect = GUILayout.Window(5, tilesWindowRect, TilesWindow, "Tiles");
            EndWindows();

            #endregion

            EditorGUILayout.EndVertical();
        }

        private void EditorWindow(int id)
        {
            EditorGUILayout.BeginVertical();

            // Draw sprites on the map
            if (spritesheet)
            {
                for (int i = 0; i < gridCells.Count; ++i)
                {
                    // Loop through all layers and draw sprites corresponding to them
                    foreach (int layer in tilemap.MapRepresentation.Keys)
                    {
                        if (tilemap.MapRepresentation[layer].Cells[i] != -1)
                        {
                            int tileId = tilemap.MapRepresentation[layer].Cells[i];  //Extract tileId corresponding to current grid cell and layer
                            int spriteId = tilemap.Tiles[tileId].SpriteId; // Get tile using its id and extract spriteId from it
                            GUI.DrawTextureWithTexCoords(gridCells[i], spritesheet, tilemap.Sprites[spriteId].DrawArea);
                        }
                    }
                }
            }

            // Listen for mouse down and mouse drag events (only left mouse button)
            Event inputEvent = Event.current;
            if ((inputEvent.type == EventType.MouseDown || inputEvent.type == EventType.MouseDrag) && inputEvent.button == 0)
            {
                // Check if any of map grid cells contain mouse position
                for (int i = 0; i < gridCells.Count; ++i)
                {
                    if (gridCells[i].Contains(inputEvent.mousePosition))
                    {
                        // Paint if paint mode is set and a tile to pint is actually selected
                        if (state == EditState.Paint && selectedTileId >= 0)
                        {
                            tilemap.MapRepresentation[selectedLayer].Cells[i] = tilemap.Tiles[selectedTileId].Id;
                        }
                        // Erase tile from map if erase mode is selected
                        else if (state == EditState.Erase)
                        {
                            tilemap.MapRepresentation[selectedLayer].Cells[i] = -1;
                        }
                        // Use mouse event and request repaint
                        inputEvent.Use();
                        Repaint();
                        break;
                    }
                }
            }

            // Draw helper lines to visualize the map grid
            Handles.BeginGUI();
            Handles.color = Color.blue;
            for (int x = (int)editorWindowRect.x + mapCellSize; x <= mapCellSize * mapSize; x += mapCellSize)
            {
                Handles.DrawLine(new Vector3(x, editorWindowRect.y), new Vector3(x, mapCellSize * 16));
            }
            for (int y = (int)editorWindowRect.y + mapCellSize; y <= mapCellSize * mapSize; y += mapCellSize)
            {
                Handles.DrawLine(new Vector3(editorWindowRect.x, y), new Vector3(mapCellSize * 15, y));
            }
            Handles.EndGUI();

            EditorGUILayout.EndVertical();
        }

        private void LayersWindow(int id)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                CreateNewLayer();
            }
            if (GUILayout.Button("-"))
            {
                DeleteLayer();
            }
            GUILayout.EndHorizontal();

            selectedLayer = EditorGUILayout.IntPopup("Current Layer: ", selectedLayer, tilemap.layerNames.ToArray(), tilemap.layers.ToArray());
        }

        private void TilesWindow(int id)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                CreateNewTile();
            }
            if (GUILayout.Button("-"))
            {
                DeleteTile();
            }
            GUILayout.EndHorizontal();

            // Create selection grid with names of the available tiles
            GUILayout.BeginVertical("Box");
            tilesScrollPosition = GUILayout.BeginScrollView(tilesScrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
            if (tilemap.Tiles.Count > 0)
            {
                // Loop through tiles and construct thair names to display in selection grid
                int tileNum = tilemap.Tiles.Values.Count;
                string[] selStrings = new string[tileNum];

                int index = 0;
                foreach (Tile tile in tilemap.Tiles.Values)
                {
                    selStrings[index] = "Tile" + tile.Id;
                    index++;
                }

                selectedTileId = GUILayout.SelectionGrid(selectedTileId, selStrings, 2);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void SpritesheetWindow(int id)
        {
            EditorGUI.BeginChangeCheck();
            spritesheet = (Texture)EditorGUILayout.ObjectField("Sprite reference", spritesheet, typeof(Texture), false);
            if (EditorGUI.EndChangeCheck())
            {
                // When Spritesheet selection is changed, recalculate sprites' bounds
                if (spritesheet != null)
                {
                    //tilemap.Tileset.SpriteSheetName = System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(spritesheet)); // Get texture's file name with extension
                    tilemap.SpriteSheetName = AssetDatabase.GetAssetPath(spritesheet); // Get texture's path
                    tilemap.Sprites.Clear();

                    int columnNum = 8;
                    int rowNum = 6;
                    float cellWidth = (float)spritesheet.width / columnNum / spritesheet.width;
                    float cellHeight = (float)spritesheet.height / rowNum / spritesheet.height;

                    for (int row = 0; row < rowNum; row++)
                    {
                        for (int col = 0; col < columnNum; col++)
                        {
                            Sprite sprite = new Sprite();
                            sprite.DrawArea = new Rect(col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                            tilemap.Sprites.Add(sprite);
                        }
                    }
                }
            }

            // Draw spritesheet so it can be better seen
            if (spritesheet != null)
            {
                Rect drawArea = new Rect(10, 85, 216f, 152f);
                GUI.DrawTexture(drawArea, spritesheet);
            }
        }

        private void PropertiesWindow(int id)
        {
            EditorGUILayout.BeginVertical();
            // Show tile properties if any tile is selected
            if (selectedTileId >= 0)
            {
                Tile selectedTile = tilemap.Tiles[selectedTileId];

                EditorGUI.BeginChangeCheck();
                Tile.TileTypes type = (Tile.TileTypes)EditorGUILayout.EnumPopup("Tile Type:", selectedTile.TileType);
                if (EditorGUI.EndChangeCheck() && type != selectedTile.TileType)
                {
                    // Change underlying tile's type only when change really happened instead of changing it every frame
                    // Otherwise, all components' values will be constantly reset because of Tile's set TileTypes method
                    selectedTile.TileType = type;
                    AddComponentsToTile(selectedTile);
                }

                switch (selectedTile.TileType)
                {
                    case Tile.TileTypes.Destructible:
                    case Tile.TileTypes.Enemy:
                    case Tile.TileTypes.Player:
                        Destructible destructible = selectedTile.FindComponent<Destructible>();
                        destructible.Lives = EditorGUILayout.IntSlider("Health", destructible.Lives, 1, 5);
                        break;
                    case Tile.TileTypes.Item:
                        Item item = selectedTile.FindComponent<Item>();
                        item.RestoreHealthBy = EditorGUILayout.IntSlider("Restore Health by", item.RestoreHealthBy, 0, 5);
                        item.IncreaseBombsBy = EditorGUILayout.IntSlider("Increase # of bombs by", item.IncreaseBombsBy, 0, 3);
                        break;
                    default:
                        break;
                }

                if (spritesheet != null)
                {
                    EditorGUILayout.LabelField("Sprite");
                    //Draw sprite             
                    selectedTile.SpriteId = EditorGUILayout.IntSlider(selectedTile.SpriteId, 0, tilemap.Sprites.Count - 1);
                    GUI.DrawTextureWithTexCoords(new Rect(10, 150, 64, 64), spritesheet, tilemap.Sprites[selectedTile.SpriteId].DrawArea);
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void NewTileMap()
        {
            tilemap = ScriptableObject.CreateInstance<TileMap>();
            selectedTileId = -1;
            selectedLayer = 0;
            CreateNewLayer();
            tilemap.nextTileId = 0;
        }

        private void LoadTileMap()
        {
            var path = EditorUtility.OpenFilePanel("Load Tile Map Data Asset", Application.dataPath, "asset");

            if (path.Length != 0 && path.Contains(Application.dataPath))
            {
                path = "Assets" + path.Replace(Application.dataPath, "");
                tilemap = AssetDatabase.LoadAssetAtPath<TileMap>(path);
                if (tilemap == null)
                {
                    Debug.Log(path + " does not contain TileMap.");
                }
                else
                {
                    if (tilemap.SpriteSheetName != null)
                    {
                        spritesheet = AssetDatabase.LoadAssetAtPath<Texture2D>(tilemap.SpriteSheetName);
                    }
                }
            }
        }

        private void SaveTileMap()
        {
            var path = EditorUtility.SaveFilePanel("Save Tile Map Data Asset", Application.dataPath, "Tilemap.asset", "asset");

            if (path.Length != 0 && path.Contains(Application.dataPath))
            {
                path = "Assets" + path.Replace(Application.dataPath, "");
                if (!AssetDatabase.Contains(tilemap))
                {
                    AssetDatabase.CreateAsset(tilemap, path);
                }

                foreach (Tile tile in tilemap.Tiles.Values)
                {
                    foreach (Component component in tile.Components)
                    {
                        if (!AssetDatabase.Contains(component))
                        {
                            AssetDatabase.AddObjectToAsset(component, tilemap);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(component));
                        }
                    }
                }
                AssetDatabase.SaveAssets();
            }
        }

        private void ExportTileMap()
        {
            var path = EditorUtility.SaveFilePanel("Export Tile Map as XML file", Application.dataPath, "Tilemap.xml", "xml");

            if (path.Length != 0 && path.Contains(Application.dataPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TileMap));
                    System.IO.TextWriter writer = new System.IO.StreamWriter(path);
                    serializer.Serialize(writer, tilemap);
                    writer.Close();
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
            }
        }

        private void DeleteLayer()
        {
            selectedLayer = tilemap.DeleteLayer(selectedLayer);
        }

        private void CreateNewLayer()
        {
            selectedLayer = tilemap.CreateNewLayer();
        }

        private void CreateNewTile()
        {
            tilemap.Tiles.Add(tilemap.nextTileId, new Tile(tilemap.nextTileId++, 0));
        }

        private void DeleteTile()
        {
            tilemap.Tiles.Remove(selectedTileId);
            // When tile is deleted, we alse need to remove references to it in all layers of the map
            foreach (var entry in tilemap.MapRepresentation)
            {
                for (int i = 0; i < entry.Value.Cells.Count; i++)
                {
                    entry.Value.Cells[i] = -1;
                }
            }
            // Set selectedTileId to -1 if no tiles are left otherwise select the first tile
            selectedTileId = (tilemap.Tiles.Count > 0) ? 0 : -1;
        }

        private void AddComponentsToTile(Tile tile)
        {
            foreach (Component component in tile.Components)
            {
                if (AssetDatabase.Contains(component))
                {
                    AssetDatabase.RemoveObjectFromAsset(component);
                }
            }
            tile.Components.Clear();

            switch (tile.TileType)
            {
                case Tile.TileTypes.Collidable:
                    {
                        tile.Components.Add(ScriptableObject.CreateInstance<Collidable>());
                        break;
                    }
                case Tile.TileTypes.Destructible:
                    {
                        tile.Components.Add(ScriptableObject.CreateInstance<Destructible>());
                        tile.Components.Add(ScriptableObject.CreateInstance<Collidable>());
                        break;
                    }
                case Tile.TileTypes.Player:
                    {
                        tile.Components.Add(ScriptableObject.CreateInstance<Destructible>());
                        tile.Components.Add(ScriptableObject.CreateInstance<Player>());
                        tile.Components.Add(ScriptableObject.CreateInstance<Collidable>());
                        break;
                    }
                case Tile.TileTypes.Enemy:
                    {
                        tile.Components.Add(ScriptableObject.CreateInstance<Destructible>());
                        tile.Components.Add(ScriptableObject.CreateInstance<Enemy>());
                        tile.Components.Add(ScriptableObject.CreateInstance<Collidable>());
                        break;
                    }
                case Tile.TileTypes.Item:
                    {
                        tile.Components.Add(ScriptableObject.CreateInstance<Item>());
                        Collidable collidable = ScriptableObject.CreateInstance<Collidable>();
                        collidable.IsTrigger = true;
                        tile.Components.Add(collidable);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}