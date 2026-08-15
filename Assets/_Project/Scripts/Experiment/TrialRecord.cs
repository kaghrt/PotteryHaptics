using System;

namespace Project.Experiment
{
    /// <summary>
    /// 1試行分の記録。CSVの1行に対応する。
    /// 案①(識別課題)・案②(JND測定)の両方で共通のフォーマットとして使う。
    /// </summary>
    [Serializable]
    public class TrialRecord
    {
        public string participantId;
        public string phase;              // 例: "JND_Elasticity", "Identification"
        public string visualCondition;    // "Minimal" or "Rich"
        public int trialIndex;
        public string textureType;        // "Elasticity" or "Viscosity"
        public string stimulusName;
        public float physicalValue;
        public float relativeIntensityPercent; // 案②用。案①では0のまま
        public string intensityLevel;     // 案①用。"Weak"/"Strong"/"NotApplicable"
        public string responseValue;      // 回答の中身(意味は各Sequencerが決める)
        public bool correct;
        public float reactionTimeSec;
        public string timestampIso;
    }
}
