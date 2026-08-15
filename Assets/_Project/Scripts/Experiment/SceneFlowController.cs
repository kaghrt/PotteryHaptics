using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Experiment
{
    /// <summary>
    /// Launcher→JND_Elasticity→JND_Viscosity→Identificationの順にシーンを遷移させる。
    /// 各シーンの終わりで NextScene() を呼ぶだけでよい設計。
    ///
    /// 【注意】シーン名はUnityの Build Settings に登録されている名前と
    /// 完全一致している必要がある。まだシーンを作ってない/登録してない場合は
    /// File > Build Settings からシーンを追加しておくこと。
    /// </summary>
    public class SceneFlowController : MonoBehaviour
    {
        public static SceneFlowController Instance { get; private set; }

        private static readonly string[] SceneOrder =
        {
            "Launcher",
            "JND_Elasticity",
            "JND_Viscosity",
            "Identification",
        };

        private int currentIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentIndex = System.Array.IndexOf(SceneOrder, SceneManager.GetActiveScene().name);
            if (currentIndex < 0) currentIndex = 0;
        }

        public void NextScene()
        {
            currentIndex++;
            if (currentIndex >= SceneOrder.Length)
            {
                Debug.Log("[SceneFlowController] 全フェーズ終了");
                return;
            }

            SceneManager.LoadScene(SceneOrder[currentIndex]);
        }
    }
}
