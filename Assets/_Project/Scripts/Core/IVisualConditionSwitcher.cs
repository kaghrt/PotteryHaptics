namespace Project.Core
{
    /// <summary>
    /// 映像条件(Minimal/Rich)を切り替えられるものの約束事。
    /// Experiment層(TrialSequencer)はこのインターフェースだけを知っていればよく、
    /// 実際の実装(VisualController)はVisual層にあってよい。
    /// FingerTracker/IFingerPositionProviderと同じ橋渡しパターン。
    /// </summary>
    public interface IVisualConditionSwitcher
    {
        void SetVisualCondition(VisualCondition condition);
    }
}
