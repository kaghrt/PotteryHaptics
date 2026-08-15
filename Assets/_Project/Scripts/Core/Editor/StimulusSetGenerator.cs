using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project.Core.Editor
{
    /// <summary>
    /// StimulusDefinitionアセットの一括生成ツール。
    ///
    /// 使い方:
    /// 1. 下の [基準値] を、実機で計測/仮決めした値に書き換える
    /// 2. Unityメニュー "HapticResearch > Generate All Stimulus Sets" を実行
    /// 3. Assets/_Project/ScriptableObjects/Stimuli/ 以下にアセットが生成される
    ///
    /// 大学で実機の値が確定したら、下の baseValue を書き換えて再実行すれば
    /// 既存アセットは上書き(同名パスに再生成)される。
    /// </summary>
    public static class StimulusSetGenerator
    {
        private const string OutputFolder = "Assets/_Project/ScriptableObjects/Stimuli";

        // ============================================================
        // 基準値(TODO: 大学での実機キャリブレーション後に差し替える)
        // ============================================================
        private const float ElasticityBaseK = 1.0f; // 仮値。実機のMax Force For Full Intensity等を踏まえて調整
        private const float ViscosityBaseB = 1.0f;  // 仮値。ViscosityTestの実機調整結果を踏まえて調整

        // 恒常法(案②)の相対強度[%]。弾性・粘性共通フォーマット
        private static readonly float[] JNDRelativePercents = { -30f, -15f, 0f, 15f, 30f };

        // 識別課題(案①)の強弱倍率。基準値に対する比率
        private const float IdentificationWeakRatio = 0.7f;
        private const float IdentificationStrongRatio = 1.3f;

        [MenuItem("HapticResearch/Generate All Stimulus Sets")]
        public static void GenerateAll()
        {
            EnsureFolder();

            GenerateJNDSet(TextureType.Elasticity, ElasticityBaseK, "Elasticity_JND");
            GenerateJNDSet(TextureType.Viscosity, ViscosityBaseB, "Viscosity_JND");

            GenerateIdentificationPair(TextureType.Elasticity, ElasticityBaseK);
            GenerateIdentificationPair(TextureType.Viscosity, ViscosityBaseB);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[StimulusSetGenerator] 全ての刺激データを生成しました: " + OutputFolder);
        }

        [MenuItem("HapticResearch/Generate Elasticity JND Set Only")]
        public static void GenerateElasticityOnly()
        {
            EnsureFolder();
            GenerateJNDSet(TextureType.Elasticity, ElasticityBaseK, "Elasticity_JND");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("HapticResearch/Generate Viscosity JND Set Only")]
        public static void GenerateViscosityOnly()
        {
            EnsureFolder();
            GenerateJNDSet(TextureType.Viscosity, ViscosityBaseB, "Viscosity_JND");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void GenerateJNDSet(TextureType type, float baseValue, string namePrefix)
        {
            foreach (var pct in JNDRelativePercents)
            {
                var asset = ScriptableObject.CreateInstance<StimulusDefinition>();
                asset.textureType = type;
                asset.relativeIntensityPercent = pct;
                asset.physicalValue = baseValue * (1f + pct / 100f);
                asset.intensityLevel = StimulusDefinition.IntensityLevel.NotApplicable;

                string sign = pct == 0f ? "Base" : (pct > 0f ? $"+{pct:0}pct" : $"{pct:0}pct");
                asset.displayName = $"{namePrefix}_{sign}";

                string path = $"{OutputFolder}/{namePrefix}_{sign}.asset";
                CreateOrReplaceAsset(asset, path);
            }
        }

        private static void GenerateIdentificationPair(TextureType type, float baseValue)
        {
            CreateIdentificationAsset(type, StimulusDefinition.IntensityLevel.Weak,
                baseValue * IdentificationWeakRatio);
            CreateIdentificationAsset(type, StimulusDefinition.IntensityLevel.Strong,
                baseValue * IdentificationStrongRatio);
        }

        private static void CreateIdentificationAsset(TextureType type, StimulusDefinition.IntensityLevel level, float value)
        {
            var asset = ScriptableObject.CreateInstance<StimulusDefinition>();
            asset.textureType = type;
            asset.intensityLevel = level;
            asset.physicalValue = value;
            asset.displayName = $"{type}_Identification_{level}";

            string path = $"{OutputFolder}/{type}_Identification_{level}.asset";
            CreateOrReplaceAsset(asset, path);
        }

        private static void CreateOrReplaceAsset(StimulusDefinition asset, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<StimulusDefinition>(path);
            if (existing != null)
            {
                // 既存アセットの中身だけ上書きする(参照してるTrialSequencer側のリンクを壊さないため)
                existing.textureType = asset.textureType;
                existing.physicalValue = asset.physicalValue;
                existing.relativeIntensityPercent = asset.relativeIntensityPercent;
                existing.intensityLevel = asset.intensityLevel;
                existing.displayName = asset.displayName;
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(asset);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
            }
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
                AssetDatabase.Refresh();
            }
        }
    }
}
