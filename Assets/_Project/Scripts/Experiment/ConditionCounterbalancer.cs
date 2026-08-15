namespace Project.Experiment
{
    /// <summary>
    /// 被験者IDから、映像条件(Minimal/Rich)の提示順を決める。
    /// 被験者間で半々になるよう、IDに含まれる数字の偶数/奇数で振り分ける単純な方式。
    /// (例: "P01"→末尾の1は奇数→Richが先。"P02"→偶数→Minimalが先)
    /// </summary>
    public static class ConditionCounterbalancer
    {
        public static Project.Core.VisualCondition DetermineFirstCondition(string participantId)
        {
            int lastDigit = ExtractLastDigit(participantId);
            return (lastDigit % 2 == 0)
                ? Project.Core.VisualCondition.Minimal
                : Project.Core.VisualCondition.Rich;
        }

        private static int ExtractLastDigit(string participantId)
        {
            if (string.IsNullOrEmpty(participantId))
                return 0;

            for (int i = participantId.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(participantId[i]))
                {
                    return participantId[i] - '0';
                }
            }

            // 数字が含まれないIDの場合は文字列のハッシュ値で代用
            return System.Math.Abs(participantId.GetHashCode()) % 10;
        }
    }
}
