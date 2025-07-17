using System;
using System.Collections.Generic;
using System.Linq;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 手紙送信条件の種類
    /// </summary>
    public enum LetterTriggerType
    {
        /// <summary>
        /// 時間ベース
        /// </summary>
        TimeOnly,
        
        /// <summary>
        /// ファイル存在ベース（単一ファイル）
        /// </summary>
        FileExistenceOnly,
        
        /// <summary>
        /// ファイル不存在ベース（単一ファイル）
        /// </summary>
        FileNotExistenceOnly,
        
        /// <summary>
        /// 複数ファイルがすべて存在
        /// </summary>
        AllFilesExist,
        
        /// <summary>
        /// 複数ファイルのいずれかが存在
        /// </summary>
        AnyFileExists,
        
        /// <summary>
        /// 複数ファイルがすべて不存在
        /// </summary>
        AllFilesNotExist,
        
        /// <summary>
        /// 複数ファイルのいずれかが不存在
        /// </summary>
        AnyFileNotExists,
        
        /// <summary>
        /// 時間とファイル存在の両方（単一ファイル）
        /// </summary>
        TimeAndFileExistence,
        
        /// <summary>
        /// 時間とファイル不存在の両方（単一ファイル）
        /// </summary>
        TimeAndFileNotExistence,
        
        /// <summary>
        /// 時間と複数ファイルがすべて存在
        /// </summary>
        TimeAndAllFilesExist,
        
        /// <summary>
        /// 時間と複数ファイルのいずれかが存在
        /// </summary>
        TimeAndAnyFileExists,
        
        /// <summary>
        /// 時間と複数ファイルがすべて不存在
        /// </summary>
        TimeAndAllFilesNotExist,
        
        /// <summary>
        /// 時間と複数ファイルのいずれかが不存在
        /// </summary>
        TimeAndAnyFileNotExists
    }

    /// <summary>
    /// 手紙送信条件設定
    /// </summary>
    public class LetterTriggerCondition
    {
        /// <summary>
        /// 条件の種類
        /// </summary>
        public LetterTriggerType TriggerType { get; set; }

        /// <summary>
        /// 時間条件（秒）
        /// </summary>
        public int TimeIntervalSeconds { get; set; }

        /// <summary>
        /// ファイル存在条件（componentsフォルダ内のファイル名）
        /// </summary>
        public string RequiredFileName { get; set; }

        /// <summary>
        /// 複数ファイル存在条件（componentsフォルダ内のファイル名リスト）
        /// </summary>
        public List<string> RequiredFileNames { get; set; }

        /// <summary>
        /// 手紙インデックス
        /// </summary>
        public int LetterIndex { get; set; }

        /// <summary>
        /// コンストラクタ（単一ファイル）
        /// </summary>
        public LetterTriggerCondition(int letterIndex, LetterTriggerType triggerType, int timeIntervalSeconds = 0, string requiredFileName = null)
        {
            LetterIndex = letterIndex;
            TriggerType = triggerType;
            TimeIntervalSeconds = timeIntervalSeconds;
            RequiredFileName = requiredFileName;
            RequiredFileNames = new List<string>();
        }

        /// <summary>
        /// コンストラクタ（複数ファイル）
        /// </summary>
        public LetterTriggerCondition(int letterIndex, LetterTriggerType triggerType, int timeIntervalSeconds, List<string> requiredFileNames)
        {
            LetterIndex = letterIndex;
            TriggerType = triggerType;
            TimeIntervalSeconds = timeIntervalSeconds;
            RequiredFileName = null;
            RequiredFileNames = requiredFileNames ?? new List<string>();
        }

        /// <summary>
        /// 時間のみの条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateTimeOnly(int letterIndex, int timeIntervalSeconds)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.TimeOnly, timeIntervalSeconds);
        }

        /// <summary>
        /// ファイル存在のみの条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateFileExistenceOnly(int letterIndex, string requiredFileName)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.FileExistenceOnly, 0, requiredFileName);
        }

        /// <summary>
        /// ファイル不存在のみの条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateFileNotExistenceOnly(int letterIndex, string requiredFileName)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.FileNotExistenceOnly, 0, requiredFileName);
        }

        /// <summary>
        /// 時間とファイル存在の両方の条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateTimeAndFileExistence(int letterIndex, int timeIntervalSeconds, string requiredFileName)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.TimeAndFileExistence, timeIntervalSeconds, requiredFileName);
        }

        /// <summary>
        /// 時間とファイル不存在の両方の条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateTimeAndFileNotExistence(int letterIndex, int timeIntervalSeconds, string requiredFileName)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.TimeAndFileNotExistence, timeIntervalSeconds, requiredFileName);
        }

        /// <summary>
        /// 複数ファイルがすべて存在する条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateAllFilesExist(int letterIndex, List<string> requiredFileNames)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.AllFilesExist, 0, requiredFileNames);
        }

        /// <summary>
        /// 複数ファイルのいずれかが存在する条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateAnyFileExists(int letterIndex, List<string> requiredFileNames)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.AnyFileExists, 0, requiredFileNames);
        }

        /// <summary>
        /// 複数ファイルがすべて不存在の条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateAllFilesNotExist(int letterIndex, List<string> requiredFileNames)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.AllFilesNotExist, 0, requiredFileNames);
        }

        /// <summary>
        /// 複数ファイルのいずれかが不存在の条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateAnyFileNotExists(int letterIndex, List<string> requiredFileNames)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.AnyFileNotExists, 0, requiredFileNames);
        }

        /// <summary>
        /// 時間と複数ファイルがすべて存在する条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateTimeAndAllFilesExist(int letterIndex, int timeIntervalSeconds, List<string> requiredFileNames)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.TimeAndAllFilesExist, timeIntervalSeconds, requiredFileNames);
        }

        /// <summary>
        /// 時間と複数ファイルのいずれかが存在する条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateTimeAndAnyFileExists(int letterIndex, int timeIntervalSeconds, List<string> requiredFileNames)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.TimeAndAnyFileExists, timeIntervalSeconds, requiredFileNames);
        }

        /// <summary>
        /// 時間と複数ファイルがすべて不存在の条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateTimeAndAllFilesNotExist(int letterIndex, int timeIntervalSeconds, List<string> requiredFileNames)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.TimeAndAllFilesNotExist, timeIntervalSeconds, requiredFileNames);
        }

        /// <summary>
        /// 時間と複数ファイルのいずれかが不存在の条件を作成
        /// </summary>
        public static LetterTriggerCondition CreateTimeAndAnyFileNotExists(int letterIndex, int timeIntervalSeconds, List<string> requiredFileNames)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.TimeAndAnyFileNotExists, timeIntervalSeconds, requiredFileNames);
        }

        /// <summary>
        /// 条件の説明文を取得
        /// </summary>
        public string GetDescription()
        {
            var fileNames = RequiredFileNames?.Count > 0 ? string.Join(", ", RequiredFileNames) : RequiredFileName;
            
            return TriggerType switch
            {
                LetterTriggerType.TimeOnly => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後",
                LetterTriggerType.FileExistenceOnly => $"手紙{LetterIndex}: {RequiredFileName}ファイル作成時",
                LetterTriggerType.FileNotExistenceOnly => $"手紙{LetterIndex}: {RequiredFileName}ファイル削除時",
                LetterTriggerType.AllFilesExist => $"手紙{LetterIndex}: [{fileNames}]すべてのファイル存在時",
                LetterTriggerType.AnyFileExists => $"手紙{LetterIndex}: [{fileNames}]いずれかのファイル存在時",
                LetterTriggerType.AllFilesNotExist => $"手紙{LetterIndex}: [{fileNames}]すべてのファイル不存在時",
                LetterTriggerType.AnyFileNotExists => $"手紙{LetterIndex}: [{fileNames}]いずれかのファイル不存在時",
                LetterTriggerType.TimeAndFileExistence => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ{RequiredFileName}ファイル存在時",
                LetterTriggerType.TimeAndFileNotExistence => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ{RequiredFileName}ファイル不存在時",
                LetterTriggerType.TimeAndAllFilesExist => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ[{fileNames}]すべてのファイル存在時",
                LetterTriggerType.TimeAndAnyFileExists => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ[{fileNames}]いずれかのファイル存在時",
                LetterTriggerType.TimeAndAllFilesNotExist => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ[{fileNames}]すべてのファイル不存在時",
                LetterTriggerType.TimeAndAnyFileNotExists => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ[{fileNames}]いずれかのファイル不存在時",
                _ => $"手紙{LetterIndex}: 不明な条件"
            };
        }
    }
}