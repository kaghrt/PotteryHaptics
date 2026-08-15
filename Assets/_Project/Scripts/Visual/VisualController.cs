using UnityEngine;
using Project.Core;

namespace Project.Visual
{
    /// <summary>
    /// 映像条件(Minimal/Rich)の切り替えを行う。
    /// Minimal: 中立的なシルエットのみ表示(識別課題重視)
    /// Rich   : 質感を作り込んだフル表示、凹み表現あり(説得力重視)
    ///
    /// IVisualConditionSwitcherを実装しているので、TrialSequencer側からは
    /// このクラスを直接知らなくても、インターフェース経由で呼び出せる。
    /// </summary>
    public class VisualController : MonoBehaviour, IVisualConditionSwitcher
    {
        [SerializeField] private GameObject minimalVisual;
        [SerializeField] private GameObject richVisual;

        public void SetVisualCondition(VisualCondition condition)
        {
            bool isRich = condition == VisualCondition.Rich;
            if (minimalVisual != null) minimalVisual.SetActive(!isRich);
            if (richVisual != null) richVisual.SetActive(isRich);
        }
    }
}
