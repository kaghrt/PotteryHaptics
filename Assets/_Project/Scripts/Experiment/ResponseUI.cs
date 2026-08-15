using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Experiment
{
    /// <summary>
    /// 2択(2AFC)の回答UI。ボタン2つとラベルを持ち、押されたら
    /// OnResponseSelected イベントで選択された値(responseValue)を通知する。
    ///
    /// 【Unity側で必要な準備(後のシーン構築ステップで行う)】
    /// Canvas内にPanel(root)を作り、その中にButtonを2つ配置し、
    /// 各ButtonにこのスクリプトのoptionAButton/optionBButton欄をアサインする。
    /// 各ButtonのTextを optionALabel/optionBLabel にアサインする。
    /// </summary>
    public class ResponseUI : MonoBehaviour
    {
        [SerializeField] private GameObject root; // 表示/非表示を切り替える親オブジェクト
        [SerializeField] private Button optionAButton;
        [SerializeField] private Text optionALabel;
        [SerializeField] private Button optionBButton;
        [SerializeField] private Text optionBLabel;

        private string valueA;
        private string valueB;

        public event Action<string> OnResponseSelected;

        private void Awake()
        {
            optionAButton.onClick.AddListener(() => Select(valueA));
            optionBButton.onClick.AddListener(() => Select(valueB));
            Hide();
        }

        public void Show(string labelA, string responseValueA, string labelB, string responseValueB)
        {
            optionALabel.text = labelA;
            optionBLabel.text = labelB;
            valueA = responseValueA;
            valueB = responseValueB;

            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private void Select(string value)
        {
            Hide();
            OnResponseSelected?.Invoke(value);
        }
    }
}
