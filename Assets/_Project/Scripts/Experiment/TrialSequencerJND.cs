using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Core;

namespace Project.Experiment
{
    /// <summary>
    /// 案②(JND測定、恒常法)の試行進行を管理する。
    /// 基準刺激と比較刺激を順に提示し、「1回目/2回目どちらが強く感じたか」を2AFCで回答させる。
    /// 1シーンにつき弾性 or 粘性のどちらか一方を担当する(textureTypeで指定)。
    ///
    /// JND_Elasticityシーンでは textureType=Elasticity、surface=ElasticitySurfaceをアサイン。
    /// JND_Viscosityシーンでは textureType=Viscosity、surface=ViscositySurfaceをアサイン。
    /// </summary>
    public class TrialSequencerJND : MonoBehaviour
    {
        [Header("この実験が対象とする質感")]
        [SerializeField] private TextureType textureType;

        [Header("参照")]
        [SerializeField] private VirtualSurfaceBase surface;
        [SerializeField] private ResponseUI responseUI;

        [Header("映像制御(任意。IVisualConditionSwitcherを実装したコンポーネントをアサイン)")]
        [SerializeField] private MonoBehaviour visualConditionSwitcherBehaviour;
        private IVisualConditionSwitcher visualConditionSwitcher;

        [Header("刺激データ")]
        [Tooltip("基準刺激(0%)")]
        [SerializeField] private StimulusDefinition standardStimulus;
        [Tooltip("比較刺激5段階(-30/-15/0/+15/+30%)")]
        [SerializeField] private List<StimulusDefinition> comparisonStimuli;

        [Header("試行設定")]
        [SerializeField] private int trialsPerLevel = 20;
        [SerializeField] private float presentationDurationSec = 2f;
        [SerializeField] private bool randomizeOrder = true;

        private struct TrialPlan
        {
            public StimulusDefinition comparison;
            public VisualCondition visualCondition;
            public bool comparisonFirst; // true: 比較刺激→基準刺激の順で提示
        }

        private List<TrialPlan> trialPlans;
        private int currentTrialIndex;
        private float trialStartTime;

        private void Start()
        {
            visualConditionSwitcher = visualConditionSwitcherBehaviour as IVisualConditionSwitcher;
            if (visualConditionSwitcherBehaviour != null && visualConditionSwitcher == null)
            {
                Debug.LogWarning(
                    "[TrialSequencerJND] visualConditionSwitcherBehaviour が IVisualConditionSwitcher を実装していません", this);
            }

            string participantId = ExperimentManager.Instance != null
                ? ExperimentManager.Instance.ParticipantId
                : "TEST";

            TrialLogger.BeginSession(participantId, $"JND_{textureType}");

            BuildTrialPlans();
            currentTrialIndex = 0;
            responseUI.OnResponseSelected += HandleResponse;

            StartCoroutine(RunTrial());
        }

        private void OnDestroy()
        {
            if (responseUI != null)
                responseUI.OnResponseSelected -= HandleResponse;
        }

        private void BuildTrialPlans()
        {
            trialPlans = new List<TrialPlan>();

            // 映像条件は前半/後半で切り替える(ExperimentManagerのカウンターバランスに従う)
            for (int half = 0; half < 2; half++)
            {
                bool isSecondHalf = half == 1;
                VisualCondition condition = ExperimentManager.Instance != null
                    ? ExperimentManager.Instance.GetVisualCondition(isSecondHalf)
                    : VisualCondition.Minimal;

                foreach (var comparison in comparisonStimuli)
                {
                    for (int i = 0; i < trialsPerLevel; i++)
                    {
                        trialPlans.Add(new TrialPlan
                        {
                            comparison = comparison,
                            visualCondition = condition,
                            comparisonFirst = randomizeOrder && Random.value > 0.5f,
                        });
                    }
                }
            }

            if (randomizeOrder)
            {
                Shuffle(trialPlans);
            }
        }

        private static void Shuffle(List<TrialPlan> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private IEnumerator RunTrial()
        {
            if (currentTrialIndex >= trialPlans.Count)
            {
                OnAllTrialsComplete();
                yield break;
            }

            var plan = trialPlans[currentTrialIndex];

            visualConditionSwitcher?.SetVisualCondition(plan.visualCondition);

            var first = plan.comparisonFirst ? plan.comparison : standardStimulus;
            var second = plan.comparisonFirst ? standardStimulus : plan.comparison;

            surface.ActiveStimulus = first;
            yield return new WaitForSeconds(presentationDurationSec);

            surface.ActiveStimulus = second;
            yield return new WaitForSeconds(presentationDurationSec);

            trialStartTime = Time.time;
            responseUI.Show("1回目の方が強かった", "First", "2回目の方が強かった", "Second");
        }

        private void HandleResponse(string responseValue)
        {
            var plan = trialPlans[currentTrialIndex];
            float reactionTime = Time.time - trialStartTime;

            // 「比較刺激の方が強い」と正しく答えたか(relativeIntensityPercentの符号で判定)
            bool comparisonIsStronger = plan.comparison.relativeIntensityPercent > 0f;
            bool respondedComparisonStronger =
                (plan.comparisonFirst && responseValue == "First") ||
                (!plan.comparisonFirst && responseValue == "Second");

            // 基準と同一(0%)の試行は正誤の概念がないため常にfalse扱い(分析時はバイアス確認用に使う)
            bool correct = plan.comparison.relativeIntensityPercent != 0f
                && comparisonIsStronger == respondedComparisonStronger;

            var record = new TrialRecord
            {
                participantId = ExperimentManager.Instance != null ? ExperimentManager.Instance.ParticipantId : "TEST",
                phase = $"JND_{textureType}",
                visualCondition = plan.visualCondition.ToString(),
                trialIndex = currentTrialIndex,
                textureType = textureType.ToString(),
                stimulusName = plan.comparison.displayName,
                physicalValue = plan.comparison.physicalValue,
                relativeIntensityPercent = plan.comparison.relativeIntensityPercent,
                intensityLevel = "NotApplicable",
                responseValue = responseValue,
                correct = correct,
                reactionTimeSec = reactionTime,
                timestampIso = System.DateTime.Now.ToString("o"),
            };

            TrialLogger.LogTrial(record);

            currentTrialIndex++;
            StartCoroutine(RunTrial());
        }

        private void OnAllTrialsComplete()
        {
            Debug.Log($"[TrialSequencerJND] {textureType} 全試行完了");
            if (SceneFlowController.Instance != null)
                SceneFlowController.Instance.NextScene();
        }
    }
}
