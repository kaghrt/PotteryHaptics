using UnityEngine;
using Project.Core;

namespace Project.Visual
{
    /// <summary>
    /// 粘性テスト用の軌道ガイド。指の動きに追従し、TrailRendererで軌跡(発光する軌跡)を残す。
    /// 同じGameObjectに Unity標準の TrailRenderer コンポーネントをアタッチしておく必要がある。
    ///
    /// 見た目(色・太さ・持続時間)はTrailRenderer側のInspectorで調整する。
    /// </summary>
    [RequireComponent(typeof(TrailRenderer))]
    public class MovementGuideTrail : MonoBehaviour
    {
        [SerializeField] private FingerTracker fingerTracker;

        [Tooltip("接触中(判定範囲内)だけ軌跡を表示したい場合はViscositySurfaceを指定する。未指定なら常に表示")]
        [SerializeField] private ViscositySurface viscositySurface;

        private TrailRenderer trail;

        private void Awake()
        {
            trail = GetComponent<TrailRenderer>();
        }

        private void Update()
        {
            if (fingerTracker == null) return;

            transform.position = fingerTracker.CurrentPosition;

            if (viscositySurface != null)
            {
                trail.emitting = viscositySurface.IsInContact;
            }
        }
    }
}
