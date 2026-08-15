using UnityEngine;

namespace Project.Experiment
{
    /// <summary>
    /// 被験者ID・条件順(映像Minimal/Rich)をシーンをまたいで保持する。
    /// Launcherシーンで一度だけ初期化し、以降DontDestroyOnLoadで生存し続ける。
    /// </summary>
    public class ExperimentManager : MonoBehaviour
    {
        public static ExperimentManager Instance { get; private set; }

        public string ParticipantId { get; private set; }

        /// <summary>この被験者が最初に受ける映像条件</summary>
        public Project.Core.VisualCondition FirstVisualCondition { get; private set; }

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Launcherシーンで、被験者IDを入力した直後に1回だけ呼ぶ</summary>
        public void InitializeSession(string participantId)
        {
            ParticipantId = participantId;
            FirstVisualCondition = ConditionCounterbalancer.DetermineFirstCondition(participantId);
            IsInitialized = true;

            Debug.Log($"[ExperimentManager] 被験者ID={participantId}, 最初の映像条件={FirstVisualCondition}");
        }

        /// <summary>
        /// 各Phase(JND_Elasticity等)内で、1番目/2番目どちらの映像条件を今使うべきかを返す。
        /// isSecondHalf=falseなら1番目、trueなら2番目。
        /// </summary>
        public Project.Core.VisualCondition GetVisualCondition(bool isSecondHalf)
        {
            if (!isSecondHalf) return FirstVisualCondition;

            return FirstVisualCondition == Project.Core.VisualCondition.Minimal
                ? Project.Core.VisualCondition.Rich
                : Project.Core.VisualCondition.Minimal;
        }
    }
}
