using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// 実機なしでの動作確認用。キーボードで指位置をシミュレートする。
    ///
    /// 操作:
    ///   W / S      : 高さ(Y軸)を上下 (押し込み/引く。弾性のテスト用)
    ///   矢印キー    : 水平(X,Z)移動 (斜め/こねる動きのテスト用。粘性のテスト用)
    ///
    /// 大学で実機に切り替える際は、このコンポーネントをオフにして、
    /// FingerTrackerのpositionProviderBehaviourをLeap用のProviderに差し替えるだけでよい。
    /// </summary>
    public class DummyKeyboardFingerInputSource : MonoBehaviour, IFingerPositionProvider
    {
        [Header("初期位置[cm]")]
        [SerializeField] private float startHeightCm = 25f;

        [Header("移動速度")]
        [SerializeField] private float heightSpeedCmPerSec = 10f;
        [SerializeField] private float horizontalSpeedCmPerSec = 10f;

        [Header("高さの可動範囲[cm]")]
        [SerializeField] private float minHeightCm = 0f;
        [SerializeField] private float maxHeightCm = 40f;

        private float heightCm;
        private float xCm;
        private float zCm;

        public bool IsTracking => true; // ダミーなので常にtrue扱い

        private void Awake()
        {
            heightCm = startHeightCm;
            xCm = 0f;
            zCm = 0f;
        }

        private void Update()
        {
            // 高さ(Y軸): 弾性テスト用
            if (Input.GetKey(KeyCode.W)) heightCm -= heightSpeedCmPerSec * Time.deltaTime;
            if (Input.GetKey(KeyCode.S)) heightCm += heightSpeedCmPerSec * Time.deltaTime;
            heightCm = Mathf.Clamp(heightCm, minHeightCm, maxHeightCm);

            // 水平(X,Z): 粘性テスト用。矢印キーのみ使用(W/Sとの競合を避けるため)
            if (Input.GetKey(KeyCode.LeftArrow)) xCm -= horizontalSpeedCmPerSec * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow)) xCm += horizontalSpeedCmPerSec * Time.deltaTime;
            if (Input.GetKey(KeyCode.UpArrow)) zCm += horizontalSpeedCmPerSec * Time.deltaTime;
            if (Input.GetKey(KeyCode.DownArrow)) zCm -= horizontalSpeedCmPerSec * Time.deltaTime;
        }

        public Vector3 GetFingerPosition()
        {
            // cm -> m に変換。FingerTracker以降は全てメートル単位で扱う
            return new Vector3(xCm / 100f, heightCm / 100f, zCm / 100f);
        }

#if UNITY_EDITOR
        [Header("デバッグ表示用(読み取り専用)")]
        [SerializeField, Tooltip("実行中の値を確認用。手で編集しても反映されない")]
        private float debugHeightCm;
        [SerializeField]
        private float debugXCm;
        [SerializeField]
        private float debugZCm;

        private void LateUpdate()
        {
            debugHeightCm = heightCm;
            debugXCm = xCm;
            debugZCm = zCm;
        }
#endif
    }
}
