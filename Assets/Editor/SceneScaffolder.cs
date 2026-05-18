using System.Collections.Generic;
using System.IO;
using ShapeConnections.Data;
using ShapeConnections.Game.Grid;
using ShapeConnections.Game.Loop;
using ShapeConnections.Game.UI;
using ShapeConnections.Game.Wiring;
using ShapeConnections.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShapeConnections.Editor
{
    /// <summary>
    /// Editor menu items that build level assets (level-00 Pass-Through, level-01 Cut)
    /// and Game.unity from scratch. Re-runnable: existing files are overwritten so
    /// iterating on scene structure is a one-button rebuild, not manual prefab
    /// surgery. The same Game.unity is reused for each level; switch by re-running
    /// the matching "Build Game Scene" menu item.
    /// </summary>
    public static class SceneScaffolder
    {
        private const string ShapesDir  = "Assets/Shapes";
        private const string NodesDir   = "Assets/Nodes";
        private const string LevelsDir  = "Assets/Levels";
        private const string ScenesDir  = "Assets/Scenes";

        private const string SquareAsset             = ShapesDir + "/SquareNone.asset";
        private const string TriangleAsset           = ShapesDir + "/TriangleNone.asset";
        private const string PassThroughAsset        = NodesDir  + "/PassThrough.asset";
        private const string CutSquareTrianglesAsset = NodesDir  + "/CutSquareToTwoTriangles.asset";
        private const string Level00Asset            = LevelsDir + "/level-00-passthrough.asset";
        private const string Level01Asset            = LevelsDir + "/level-01-cut.asset";
        private const string GameScene               = ScenesDir + "/Game.unity";

        [MenuItem("Tools/Shape Connections/Build Level-00 Assets")]
        public static void BuildLevel00Assets()
        {
            EnsureFolder(ShapesDir);
            EnsureFolder(NodesDir);
            EnsureFolder(LevelsDir);

            var square = CreateOrReplace<ShapeDefinition>(SquareAsset, asset =>
            {
                // ShapeDefinition fields are private; use a SerializedObject to set them.
                var so = new SerializedObject(asset);
                so.FindProperty("kind").enumValueIndex  = (int)ShapeKind.Square;
                so.FindProperty("color").enumValueIndex = (int)ShapeColor.None;
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            var passThrough = CreateOrReplace<PassThroughNodeDefinition>(PassThroughAsset, asset =>
            {
                var so = new SerializedObject(asset);
                var displayProp = so.FindProperty("displayName");
                if (displayProp != null) displayProp.stringValue = "Pass-Through";
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            CreateOrReplace<LevelDefinition>(Level00Asset, asset =>
            {
                var so = new SerializedObject(asset);
                so.FindProperty("gridWidth").intValue  = 3;
                so.FindProperty("gridHeight").intValue = 3;

                var inputs = so.FindProperty("inputs");
                inputs.arraySize = 1;
                inputs.GetArrayElementAtIndex(0).objectReferenceValue = square;

                var targets = so.FindProperty("targets");
                targets.arraySize = 1;
                targets.GetArrayElementAtIndex(0).objectReferenceValue = square;

                var palette = so.FindProperty("palette");
                palette.arraySize = 1;
                var entry = palette.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("node").objectReferenceValue = passThrough;
                entry.FindPropertyRelative("count").intValue = 1;

                so.FindProperty("designerNotes").stringValue =
                    "MVP smoke test: place Pass-Through → wire input → PT → output → Run → Win.";
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SceneScaffolder] Built level-00 assets at {ShapesDir}, {NodesDir}, {LevelsDir}.");
        }

        [MenuItem("Tools/Shape Connections/Build Level-01 Assets")]
        public static void BuildLevel01Assets()
        {
            EnsureFolder(ShapesDir);
            EnsureFolder(NodesDir);
            EnsureFolder(LevelsDir);

            // Level-01 reuses the Square shape that level-00 creates; ensure it exists.
            var square = CreateOrReplace<ShapeDefinition>(SquareAsset, asset =>
            {
                var so = new SerializedObject(asset);
                so.FindProperty("kind").enumValueIndex  = (int)ShapeKind.Square;
                so.FindProperty("color").enumValueIndex = (int)ShapeColor.None;
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            var triangle = CreateOrReplace<ShapeDefinition>(TriangleAsset, asset =>
            {
                var so = new SerializedObject(asset);
                so.FindProperty("kind").enumValueIndex  = (int)ShapeKind.Triangle;
                so.FindProperty("color").enumValueIndex = (int)ShapeColor.None;
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            var cut = CreateOrReplace<CutNodeDefinition>(CutSquareTrianglesAsset, asset =>
            {
                // Display name via SerializedObject (private field on NodeDefinition);
                // shape references via the editor-only EditorConfigure to avoid the
                // cross-asset SerializedObject persistence flakiness seen in Phase 2.
                var so = new SerializedObject(asset);
                var displayProp = so.FindProperty("displayName");
                if (displayProp != null) displayProp.stringValue = "Cut Square→Triangles";
                so.ApplyModifiedPropertiesWithoutUndo();
                asset.EditorConfigure(input: square, top: triangle, bottom: triangle);
            });

            CreateOrReplace<LevelDefinition>(Level01Asset, asset =>
            {
                var so = new SerializedObject(asset);
                so.FindProperty("gridWidth").intValue  = 3;
                so.FindProperty("gridHeight").intValue = 3;

                var inputs = so.FindProperty("inputs");
                inputs.arraySize = 1;
                inputs.GetArrayElementAtIndex(0).objectReferenceValue = square;

                var targets = so.FindProperty("targets");
                targets.arraySize = 2;
                targets.GetArrayElementAtIndex(0).objectReferenceValue = triangle;
                targets.GetArrayElementAtIndex(1).objectReferenceValue = triangle;

                var palette = so.FindProperty("palette");
                palette.arraySize = 1;
                var entry = palette.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("node").objectReferenceValue = cut;
                entry.FindPropertyRelative("count").intValue = 1;

                so.FindProperty("designerNotes").stringValue =
                    "Cut a Square into two Triangles, wire each Cut output to a target socket.";
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SceneScaffolder] Built level-01 assets at {ShapesDir}, {NodesDir}, {LevelsDir}.");
        }

        [MenuItem("Tools/Shape Connections/Build Game Scene")]
        public static void BuildGameScene()
        {
            BuildLevel00Assets();
            BuildGameSceneForLevel(Level00Asset, "level-00");
        }

        [MenuItem("Tools/Shape Connections/Build Game Scene (Level 01)")]
        public static void BuildGameSceneLevel01()
        {
            BuildLevel01Assets();
            BuildGameSceneForLevel(Level01Asset, "level-01");
        }

        private static void BuildGameSceneForLevel(string levelAssetPath, string label)
        {
            EnsureFolder(ScenesDir);

            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(levelAssetPath);
            if (level == null)
            {
                Debug.LogError($"[SceneScaffolder] {label} asset not found at {levelAssetPath} — aborting scene build.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildSceneContents(scene, level);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScene);
            Debug.Log($"[SceneScaffolder] Built {GameScene} for {label}. Open it and press Play to test the loop.");
        }

        private static void BuildSceneContents(Scene scene, LevelDefinition level)
        {
            // ── Camera ──
            var cameraGO = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraGO, scene);
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.10f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 3.5f;
            cameraGO.AddComponent<AudioListener>();
            cameraGO.transform.position = new Vector3(0f, 0f, -10f);

            // ── Grid ──
            var gridGO = new GameObject("GridView");
            SceneManager.MoveGameObjectToScene(gridGO, scene);
            var gridView = gridGO.AddComponent<GridView>();

            // ── Wire drag controller ──
            var wiresGO = new GameObject("WireDragController");
            SceneManager.MoveGameObjectToScene(wiresGO, scene);
            var wires = wiresGO.AddComponent<WireDragController>();

            // ── Level runner ──
            var runnerGO = new GameObject("LevelRunner");
            SceneManager.MoveGameObjectToScene(runnerGO, scene);
            var nodesRoot = new GameObject("Nodes");
            nodesRoot.transform.SetParent(runnerGO.transform, worldPositionStays: false);
            var runner = runnerGO.AddComponent<LevelRunner>();

            // ── UI: Canvas + EventSystem ──
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            var eventSystemGO = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(eventSystemGO, scene);

            // Palette panel (left side)
            var palettePanel = CreateUIPanel("PalettePanel", canvasGO.transform,
                anchorMin: new Vector2(0, 0.2f), anchorMax: new Vector2(0.15f, 0.9f),
                color: new Color(0.1f, 0.1f, 0.12f, 0.6f));
            var paletteLayout = palettePanel.AddComponent<VerticalLayoutGroup>();
            paletteLayout.padding = new RectOffset(8, 8, 8, 8);
            paletteLayout.spacing = 6;
            paletteLayout.childForceExpandHeight = false;
            paletteLayout.childForceExpandWidth = true;

            // Button template (inactive — PaletteUI clones it)
            var buttonTemplate = CreateUIButton("ButtonTemplate", palettePanel.transform, label: "Node ×1");
            buttonTemplate.gameObject.SetActive(false);

            var paletteUI = canvasGO.AddComponent<PaletteUI>();
            paletteUI.EditorConfigure(level, palettePanel.GetComponent<RectTransform>(), buttonTemplate, gridView, nodesRoot.transform);
            EditorUtility.SetDirty(paletteUI);

            // Run button (bottom-right)
            var runPanel = CreateUIPanel("RunPanel", canvasGO.transform,
                anchorMin: new Vector2(0.82f, 0.05f), anchorMax: new Vector2(0.98f, 0.15f),
                color: new Color(0, 0, 0, 0));
            var runButton = CreateUIButton("RunButton", runPanel.transform, label: "Run");
            var runTrigger = runButton.gameObject.AddComponent<ButtonRunTrigger>();

            // Win/Fail panel (centered, hidden by default)
            var winPanel = CreateUIPanel("WinFailPanel", canvasGO.transform,
                anchorMin: new Vector2(0.3f, 0.4f), anchorMax: new Vector2(0.7f, 0.6f),
                color: new Color(0.2f, 0.7f, 0.3f, 0.85f));
            winPanel.SetActive(false);
            var winText = CreateUIText("Message", winPanel.transform, "Solved!");
            var winFail = canvasGO.AddComponent<WinFailPanel>();
            winFail.EditorConfigure(runner, winPanel, winText, winPanel.GetComponent<Image>());
            EditorUtility.SetDirty(winFail);

            // Wire LevelRunner references via the editor-only configure hook.
            runner.EditorConfigure(level, gridView, wires, nodesRoot.transform, runTrigger);
            EditorUtility.SetDirty(runner);
        }

        private static GameObject CreateUIPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Button CreateUIButton(string name, Transform parent, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 32);
            go.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.7f, 1f);
            CreateUIText("Label", go.transform, label);
            return go.GetComponent<Button>();
        }

        private static Text CreateUIText(string name, Transform parent, string content)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.text = content;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            return text;
        }

        private static void AssignField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[SceneScaffolder] Field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static T CreateOrReplace<T>(string assetPath, System.Action<T> configure) where T : ScriptableObject
        {
            EnsureFolder(Path.GetDirectoryName(assetPath));
            var existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null)
            {
                configure(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            configure(asset);
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
