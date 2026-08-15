using UnityEngine;
using UnityEngine.UI;

namespace Project.Experiment
{
    /// <summary>
    /// Launcherシーン用。被験者IDを入力してStartを押すと、
    /// ExperimentManagerを初期化してから次のシーンに進む。
    /// 同じGameObject(またはシーン内のどこか)に ExperimentManager と
    /// SceneFlowController がアタッチされている必要がある。
    /// </summary>
    public class LauncherController : MonoBehaviour
    {
        [SerializeField] private InputField participantIdInput;
        [SerializeField] private Button startButton;

        private void Awake()
        {
            startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            string id = participantIdInput.text;
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("[LauncherController] 被験者IDが空です");
                return;
            }

            ExperimentManager.Instance.InitializeSession(id);
            SceneFlowController.Instance.NextScene();
        }
    }
}
