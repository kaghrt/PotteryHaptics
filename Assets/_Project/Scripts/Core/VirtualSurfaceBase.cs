using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// 力覚計算の共通基底クラス。
    /// FingerTrackerから指位置/速度を受け取り、StimulusDefinition(k値/b値)を使って
    /// 現在の力の大きさ(CurrentForce)を毎フレーム計算する。
    ///
    /// ElasticitySurface(Y軸ベース)とViscositySurface(X-Z速度ベース)が
    /// これを継承し、ComputeForce()だけをそれぞれ実装する。
    /// </summary>
    public abstract class VirtualSurfaceBase : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] protected FingerTracker fingerTracker;

        [Header("刺激データ")]
        [Tooltip("実行中にTrialSequencerから差し替えられる想定。Inspectorでの値は動作確認用の仮設定")]
        [SerializeField] protected StimulusDefinition activeStimulus;

        public float CurrentForce { get; private set; }
        public bool IsInContact { get; protected set; }

        public StimulusDefinition ActiveStimulus
        {
            get => activeStimulus;
            set => activeStimulus = value;
        }

        protected virtual void Update()
        {
            if (fingerTracker == null || activeStimulus == null || !fingerTracker.IsTracking)
            {
                CurrentForce = 0f;
                IsInContact = false;
                return;
            }

            CurrentForce = ComputeForce();
        }

        /// <summary>
        /// 現在フレームの力の大きさを計算する。IsInContactもここで更新すること。
        /// </summary>
        protected abstract float ComputeForce();
    }
}
