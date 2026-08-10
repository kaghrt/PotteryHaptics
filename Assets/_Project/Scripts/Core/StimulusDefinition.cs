using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// 質感の種類。案①(識別課題)・案②(JND測定)の両方で共通して使う。
    /// 慣性(Inertia)は今回のメイン実験からは除外しているが、
    /// 将来的な拡張を見込んで列挙型自体には残してある。
    /// </summary>
    public enum TextureType
    {
        Elasticity,
        Viscosity,
        // Inertia, // 今回は未使用。粘性側の実装が固まり次第、追加候補として検討する。
    }

    /// <summary>
    /// 映像提示条件。両実験(案①/案②)で共通のパラメータとして使う。
    /// Minimal: 識別課題重視、映像は最小限(中立的なシルエットのみ)
    /// Rich   : 説得力(疑似触覚)重視、映像を作り込む
    /// </summary>
    public enum VisualCondition
    {
        Minimal,
        Rich,
    }

    /// <summary>
    /// 1つの刺激(=1つの器 or 1回のこねる動作)を定義するデータ。
    /// 恒常法(案②)では基準刺激からの相対強度(-30/-15/0/+15/+30%など)を、
    /// 識別課題(案①)では強/弱の2水準を、このアセットで表現する。
    ///
    /// Assets > Create > HapticResearch > Stimulus Definition から生成する想定。
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewStimulusDefinition",
        menuName = "HapticResearch/Stimulus Definition",
        order = 0)]
    public class StimulusDefinition : ScriptableObject
    {
        [Header("質感タイプ")]
        public TextureType textureType;

        [Header("物理パラメータ")]
        [Tooltip("弾性の場合はk値、粘性の場合はb値。基準刺激からの相対値ではなく実値を入れる。")]
        public float physicalValue;

        [Header("恒常法(案②)用: 基準刺激に対する相対強度[%]")]
        [Tooltip("基準刺激なら0。比較刺激は-30/-15/+15/+30などを想定。案①では未使用でも構わない。")]
        public float relativeIntensityPercent;

        [Header("識別課題(案①)用: 強度水準")]
        public IntensityLevel intensityLevel = IntensityLevel.NotApplicable;

        [Header("表示名(ログ・Inspector確認用)")]
        public string displayName;

        public enum IntensityLevel
        {
            NotApplicable,
            Weak,
            Strong,
        }
    }
}
