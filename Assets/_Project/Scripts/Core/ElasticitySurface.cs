using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// 弾性(F = k * x)。Y軸(高さ)の押し込み深度で判定する。
    /// ElasticityTestの実機検証結果を踏襲: 25cmで接触開始(x=0)、5cmで最大押し込み。
    /// </summary>
    public class ElasticitySurface : VirtualSurfaceBase
    {
        [Header("弾性パラメータ")]
        [Tooltip("この高さ[cm]で接触が始まる(x=0)")]
        [SerializeField] private float contactStartHeightCm = 25f;

        [Tooltip("この高さ[cm]で最大押し込みになる")]
        [SerializeField] private float maxPushHeightCm = 5f;

        /// <summary>現在の押し込み深度[cm]。0なら非接触</summary>
        public float CurrentDepthCm { get; private set; }

        protected override float ComputeForce()
        {
            float heightCm = fingerTracker.CurrentPosition.y * 100f; // m -> cm

            float maxDepth = contactStartHeightCm - maxPushHeightCm;
            float depth = Mathf.Clamp(contactStartHeightCm - heightCm, 0f, maxDepth);

            CurrentDepthCm = depth;
            IsInContact = depth > 0f;

            if (!IsInContact) return 0f;

            float k = activeStimulus.physicalValue;
            return k * depth; // F = k * x
        }
    }
}
