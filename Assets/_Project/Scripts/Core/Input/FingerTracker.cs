using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// 指位置トラッキングの窓口。
    /// positionProviderBehaviour に IFingerPositionProvider を実装したコンポーネントを
    /// Inspectorでアサインして使う(実機ならLeap用Provider、今はDummyKeyboardFingerInputSource)。
    ///
    /// 大学で実機に切り替える際は、このコンポーネントは触らず、
    /// positionProviderBehaviour の参照先だけ差し替えればよい設計。
    /// </summary>
    public class FingerTracker : MonoBehaviour
    {
        [Tooltip("IFingerPositionProvider を実装したコンポーネントをアサインする")]
        [SerializeField] private MonoBehaviour positionProviderBehaviour;

        private IFingerPositionProvider provider;

        public Vector3 CurrentPosition { get; private set; }
        public Vector3 CurrentVelocity { get; private set; }
        public bool IsTracking { get; private set; }

        private Vector3 previousPosition;
        private bool hasPreviousPosition;

        private void Awake()
        {
            provider = positionProviderBehaviour as IFingerPositionProvider;
            if (provider == null)
            {
                Debug.LogError(
                    "[FingerTracker] positionProviderBehaviour が IFingerPositionProvider を実装していません。" +
                    "Inspectorでのアサインを確認してください。", this);
            }
        }

        private void Update()
        {
            if (provider == null) return;

            IsTracking = provider.IsTracking;
            if (!IsTracking)
            {
                CurrentVelocity = Vector3.zero;
                hasPreviousPosition = false;
                return;
            }

            CurrentPosition = provider.GetFingerPosition();

            if (hasPreviousPosition && Time.deltaTime > 0f)
            {
                CurrentVelocity = (CurrentPosition - previousPosition) / Time.deltaTime;
            }

            previousPosition = CurrentPosition;
            hasPreviousPosition = true;
        }
    }
}
