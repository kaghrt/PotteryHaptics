using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// 触覚出力の窓口。
    /// 【今の状態】実機未接続のため、SendForceToDevice()の中身はダミー(ログ出力のみ)。
    /// 【大学での作業】この中身をUltrahaptics SDK経由の実出力に差し替える。
    ///
    /// 過去の学習事項(引き継ぎメモより):
    /// AmplitudeModulationEmitterのコンストラクタはデバイスに自動接続するため、
    /// addDevice()を明示的に呼ぶと"already claimed"エラーになる。実装時に注意。
    /// </summary>
    public class HapticOutputController : MonoBehaviour
    {
        [SerializeField] private VirtualSurfaceBase surface;

        [Header("デバッグログ")]
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private float logIntervalSec = 0.5f;

        private float logTimer;

        private void Update()
        {
            if (surface == null) return;

            SendForceToDevice(surface.CurrentForce, surface.IsInContact);

            if (!logToConsole) return;

            logTimer += Time.deltaTime;
            if (logTimer < logIntervalSec) return;

            logTimer = 0f;
            Debug.Log($"[HapticOutputController(dummy)] Force={surface.CurrentForce:0.00}, Contact={surface.IsInContact}");
        }

        private void SendForceToDevice(float force, bool isInContact)
        {
            // TODO(大学で実装): AmplitudeModulationEmitter等を使った実際の出力に差し替える。
            // isInContact=false のとき出力を止めるかどうかは実機の挙動を見て決める。
        }
    }
}
