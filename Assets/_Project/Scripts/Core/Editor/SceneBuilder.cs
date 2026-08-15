using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Project.Core;
using Project.Experiment;
using Object = UnityEngine.Object;

namespace Project.Core.Editor
{
    /// <summary>
    /// JND_Elasticity / JND_Viscosity シーンの中身を自動構築するツール。
    /// GameObjectの作成・コンポーネントの追加・参照の配線・保存までを自動で行う。
    ///
    /// 【使う前の前提】
    /// - 対象シーン(JND_Elasticity.unity 等)が Assets/_Project/Scenes/ に存在すること
    /// - 刺激データ(StimulusSetGeneratorで生成したもの)が既にあること
    /// - シーンを開いて Hierarchy の中身を一旦全部削除し、空の状態で保存してから実行すること
    ///   (このツールは新規オブジェクトを作るだけで、既存の重複は削除しないため)
    /// </summary>
    public static class SceneBuilder
    {
        private const string StimuliFolder = "Assets/_Project/ScriptableObjects/Stimuli";

        [MenuItem("HapticResearch/Build JND_Elasticity Scene")]
        public static void BuildJNDElasticityScene()
        {
            BuildJNDScene(
                sceneName: "JND_Elasticity",
                textureType: TextureType.Elasticity,
                surfaceComponentType: typeof(ElasticitySurface),
                stimulusPrefix: "Elasticity_JND");
        }

        [MenuItem("HapticResearch/Build JND_Viscosity Scene")]
        public static void BuildJNDViscosityScene()
        {
            BuildJNDScene(
                sceneName: "JND_Viscosity",
                textureType: TextureType.Viscosity,
                surfaceComponentType: typeof(ViscositySurface),
                stimulusPrefix: "Viscosity_JND");
        }

        private static void BuildJNDScene(string sceneName, TextureType textureType, Type surfaceComponentType, string stimulusPrefix)
        {
            string scenePath = $"Assets/_Project/Scenes/{sceneName}.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            ClearScene(scene);
            CreateCamera();

            // --- 指トラッキング(ダミー入力) ---
            var dummyInputGO = new GameObject("DummyInput");
            var dummyInput = dummyInputGO.AddComponent<DummyKeyboardFingerInputSource>();

            var fingerTrackerGO = new GameObject("FingerTracker");
            var fingerTracker = fingerTrackerGO.AddComponent<FingerTracker>();
            SetPrivateField(fingerTracker, "positionProviderBehaviour", dummyInput);

            // --- 力覚計算 ---
            var surfaceGO = new GameObject(surfaceComponentType.Name);
            var surface = surfaceGO.AddComponent(surfaceComponentType) as VirtualSurfaceBase;
            SetPrivateField(surface, "fingerTracker", fingerTracker);

            // --- 出力(ダミー) ---
            var outputGO = new GameObject("HapticOutput");
            var output = outputGO.AddComponent<HapticOutputController>();
            SetPrivateField(output, "surface", surface);

            // --- UI ---
            var canvasGO = CreateCanvas();
            var panelGO = CreateUIObject("Panel", canvasGO.transform);
            var panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.6f);
            SetRect(panelGO, new Vector2(600, 200), Vector2.zero);

            var buttonA = CreateButton("ButtonA", panelGO.transform, new Vector2(-150, 0), out var textA);
            var buttonB = CreateButton("ButtonB", panelGO.transform, new Vector2(150, 0), out var textB);

            var responseUIGO = new GameObject("ResponseUI");
            var responseUI = responseUIGO.AddComponent<ResponseUI>();
            SetPrivateField(responseUI, "root", panelGO);
            SetPrivateField(responseUI, "optionAButton", buttonA);
            SetPrivateField(responseUI, "optionALabel", textA);
            SetPrivateField(responseUI, "optionBButton", buttonB);
            SetPrivateField(responseUI, "optionBLabel", textB);

            panelGO.SetActive(false);

            // --- TrialSequencerJND ---
            var sequencerGO = new GameObject("TrialSequencer");
            var sequencer = sequencerGO.AddComponent<TrialSequencerJND>();
            SetPrivateField(sequencer, "textureType", textureType);
            SetPrivateField(sequencer, "surface", surface);
            SetPrivateField(sequencer, "responseUI", responseUI);

            var standard = AssetDatabase.LoadAssetAtPath<StimulusDefinition>($"{StimuliFolder}/{stimulusPrefix}_Base.asset");
            SetPrivateField(sequencer, "standardStimulus", standard);

            var comparisons = new List<StimulusDefinition>
            {
                AssetDatabase.LoadAssetAtPath<StimulusDefinition>($"{StimuliFolder}/{stimulusPrefix}_-30pct.asset"),
                AssetDatabase.LoadAssetAtPath<StimulusDefinition>($"{StimuliFolder}/{stimulusPrefix}_-15pct.asset"),
                AssetDatabase.LoadAssetAtPath<StimulusDefinition>($"{StimuliFolder}/{stimulusPrefix}_Base.asset"),
                AssetDatabase.LoadAssetAtPath<StimulusDefinition>($"{StimuliFolder}/{stimulusPrefix}_+15pct.asset"),
                AssetDatabase.LoadAssetAtPath<StimulusDefinition>($"{StimuliFolder}/{stimulusPrefix}_+30pct.asset"),
            };

            foreach (var c in comparisons)
            {
                if (c == null)
                    Debug.LogWarning($"[SceneBuilder] 刺激データが見つかりません。StimulusSetGeneratorを先に実行してください({stimulusPrefix}系)");
            }

            SetPrivateFieldList(sequencer, "comparisonStimuli", comparisons);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[SceneBuilder] {sceneName} を自動構築しました");
        }

        // ============================================================
        // シーンの初期化・カメラ生成
        // ============================================================

        /// <summary>
        /// シーン内の既存オブジェクトを全部削除する。
        /// 再実行しても重複が増えないようにするための後始末。
        /// </summary>
        private static void ClearScene(UnityEngine.SceneManagement.Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                Object.DestroyImmediate(root);
            }
        }

        /// <summary>
        /// Main Cameraを作る。SceneBuilderは元々あったMain Cameraごと
        /// ClearSceneで消してしまうため、必ずここで作り直す。
        /// </summary>
        private static void CreateCamera()
        {
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraGO.AddComponent<AudioListener>();
            cameraGO.transform.position = new Vector3(0f, 1f, -3f);
        }

        // ============================================================
        // UI生成ヘルパー
        // ============================================================

        private static GameObject CreateCanvas()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }

            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            return canvasGO;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetRect(GameObject go, Vector2 size, Vector2 anchoredPos)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchoredPos, out Text label)
        {
            var go = CreateUIObject(name, parent);
            SetRect(go, new Vector2(200, 60), anchoredPos);
            go.AddComponent<Image>().color = Color.white;
            var button = go.AddComponent<Button>();

            var textGO = CreateUIObject("Text", go.transform);
            SetRect(textGO, new Vector2(200, 60), Vector2.zero);
            label = textGO.AddComponent<Text>();
            label.text = name;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.black;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return button;
        }

        // ============================================================
        // Reflection(SerializedObject)経由でprivate [SerializeField]に値をセットする
        // ============================================================
        private static void SetPrivateField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"[SceneBuilder] フィールドが見つかりません: {fieldName} on {target.GetType().Name}");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetPrivateField(Object target, string fieldName, Enum value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"[SceneBuilder] フィールドが見つかりません: {fieldName} on {target.GetType().Name}");
                return;
            }

            prop.enumValueIndex = Convert.ToInt32(value);
            so.ApplyModifiedProperties();
        }

        private static void SetPrivateFieldList(Object target, string fieldName, List<StimulusDefinition> list)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"[SceneBuilder] フィールドが見つかりません: {fieldName} on {target.GetType().Name}");
                return;
            }

            prop.ClearArray();
            for (int i = 0; i < list.Count; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            }
            so.ApplyModifiedProperties();
        }
    }
}
