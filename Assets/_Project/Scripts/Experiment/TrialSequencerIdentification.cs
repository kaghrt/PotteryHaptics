using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Core;

namespace Project.Experiment
{
    /// <summary>
    /// 案①(質感の種類識別)の試行進行を管理する。
    /// 弾性/粘性のどちらかをランダムに1つ提示し、「弾性/粘性どちらだったか」を2AFCで回答させる。
    /// Identificationシーンで使う。弾性・粘性両方のSurfaceをアサインする必要がある。
    /// </summary>
    public class TrialSequencerIdentification : MonoBehaviour
    {
        [Header("参照(弾性・粘性、両方のSurfaceが必要)")]
        [SerializeField] private VirtualSurfaceBase elasticitySurface;
        [SerializeField] private VirtualSurfaceBase viscositySurface;
        [SerializeField] private ResponseUI responseUI;

        [Header("映像制御(任意。IVisualConditionSwitcherを実装したコンポーネントをアサイン)")]
        [SerializeField] private MonoBehaviour visualConditionSwitcherBehaviour;
        private IVisualConditionSwitcher visualConditionSwitcher;

        [Header("刺激データ(各4種: 弾性強/弱、粘性強/弱)")]
        [SerializeField] private StimulusDefinition elasticityWeak;
        [SerializeField] private StimulusDefinition elasticityStrong;
        [SerializeField] private StimulusDefinition viscosityWeak;
        [SerializeField] private StimulusDefinition viscosityStrong;

        [Header("試行設定")]
        [SerializeField] private int trialsPerStimulus = 15;
        [SerializeField] private float presentationDurationSec = 3f;

        private struct TrialPlan
        {
            public StimulusDefinition stimulus;
            public VisualCondition visualCondition;
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
                    "[TrialSequencerIdentification] visualConditionSwitcherBehaviour が IVisualConditionSwitcher を実装していません", this);
            }

            string participantId = ExperimentManager.Instance != null
                ? ExperimentManager.Instance.ParticipantId
                : "TEST";

            TrialLogger.BeginSession(participantId, "Identification");

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
            var allStimuli = new[] { elasticityWeak, elasticityStrong, viscosityWeak, viscosityStrong };

            for (int half = 0; half < 2; half++)
            {
                bool isSecondHalf = half == 1;
                VisualCondition condition = ExperimentManager.Instance != null
                    ? ExperimentManager.Instance.GetVisualCondition(isSecondHalf)
                    : VisualCondition.Minimal;

                foreach (var stimulus in allStimuli)
                {
                    for (int i = 0; i < trialsPerStimulus; i++)
                    {
                        trialPlans.Add(new TrialPlan { stimulus = stimulus, visualCondition = condition });
                    }
                }
            }

            Shuffle(trialPlans);
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

            var activeSurface = plan.stimulus.textureType == TextureType.Elasticity
                ? elasticitySurface
                : viscositySurface;

            activeSurface.ActiveStimulus = plan.stimulus;
            yield return new WaitForSeconds(presentationDurationSec);

            trialStartTime = Time.time;
            responseUI.Show("弾性(押すと硬さがある)", "Elasticity", "粘性(押すとねばりがある)", "Viscosity");
        }

        private void HandleResponse(string responseValue)
        {
            var plan = trialPlans[currentTrialIndex];
            float reactionTime = Time.time - trialStartTime;

            bool correct = responseValue == plan.stimulus.textureType.ToString();

            var record = new TrialRecord
            {
                participantId = ExperimentManager.Instance != null ? ExperimentManager.Instance.ParticipantId : "TEST",
                phase = "Identification",
                visualCondition = plan.visualCondition.ToString(),
                trialIndex = currentTrialIndex,
                textureType = plan.stimulus.textureType.ToString(),
                stimulusName = plan.stimulus.displayName,
                physicalValue = plan.stimulus.physicalValue,
                relativeIntensityPercent = 0f,
                intensityLevel = plan.stimulus.intensityLevel.ToString(),
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
            Debug.Log("[TrialSequencerIdentification] 全試行完了");
            if (SceneFlowController.Instance != null)
                SceneFlowController.Instance.NextScene();
        }
    }
}
