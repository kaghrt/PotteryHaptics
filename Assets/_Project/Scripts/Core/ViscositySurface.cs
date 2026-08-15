using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// 粘性(F = b * v)。X-Z平面(水平)の移動速度で判定する。
    /// 高さがfixedHeightCm付近(±heightToleranceCm)にある間のみ有効。
    /// </summary>
    public class ViscositySurface : VirtualSurfaceBase
    {
        [Header("粘性パラメータ")]
        [Tooltip("この高さ[cm]付近で判定を有効にする")]
        [SerializeField] private float fixedHeightCm = 15f;

        [SerializeField] private float heightToleranceCm = 5f;

        [Tooltip("これ未満の速度は静止とみなし、力を発生させない")]
        [SerializeField] private float minSpeedCmPerSec = 0.5f;

        /// <summary>現在の水平移動速度[cm/s]</summary>
        public float CurrentSpeedCmPerSec { get; private set; }

        protected override float ComputeForce()
        {
            float heightCm = fingerTracker.CurrentPosition.y * 100f;
            bool withinHeightBand = Mathf.Abs(heightCm - fixedHeightCm) <= heightToleranceCm;

            Vector3 velocity = fingerTracker.CurrentVelocity; // m/s
            float speedCmPerSec = new Vector2(velocity.x, velocity.z).magnitude * 100f;

            CurrentSpeedCmPerSec = speedCmPerSec;
            IsInContact = withinHeightBand && speedCmPerSec >= minSpeedCmPerSec;

            if (!IsInContact) return 0f;

            float b = activeStimulus.physicalValue;
            return b * speedCmPerSec; // F = b * v
        }
    }
}
