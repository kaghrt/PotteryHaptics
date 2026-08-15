using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Project.Experiment
{
    /// <summary>
    /// TrialRecordをCSVファイルに追記していく。
    /// 出力先はApplication.persistentDataPath/Logs/ (Windowsなら
    /// %USERPROFILE%\AppData\LocalLow\<会社名>\PotteryHaptics\Logs\)。
    /// </summary>
    public static class TrialLogger
    {
        private const string Header =
            "participantId,phase,visualCondition,trialIndex,textureType,stimulusName," +
            "physicalValue,relativeIntensityPercent,intensityLevel,responseValue,correct,reactionTimeSec,timestampIso";

        private static string currentPath;

        /// <summary>フェーズ開始時に1回だけ呼ぶ。新しいCSVファイルを作る</summary>
        public static void BeginSession(string participantId, string phase)
        {
            string folder = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(folder);

            string fileName = $"{participantId}_{phase}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            currentPath = Path.Combine(folder, fileName);

            File.WriteAllText(currentPath, Header + "\n", Encoding.UTF8);
            Debug.Log($"[TrialLogger] ログ出力先: {currentPath}");
        }

        public static void LogTrial(TrialRecord record)
        {
            if (currentPath == null)
            {
                Debug.LogError("[TrialLogger] BeginSession() を先に呼んでください");
                return;
            }

            string line = string.Join(",",
                Escape(record.participantId),
                Escape(record.phase),
                Escape(record.visualCondition),
                record.trialIndex.ToString(),
                Escape(record.textureType),
                Escape(record.stimulusName),
                record.physicalValue.ToString("F4"),
                record.relativeIntensityPercent.ToString("F1"),
                Escape(record.intensityLevel),
                Escape(record.responseValue),
                record.correct.ToString(),
                record.reactionTimeSec.ToString("F3"),
                Escape(record.timestampIso));

            File.AppendAllText(currentPath, line + "\n", Encoding.UTF8);
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\""))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
