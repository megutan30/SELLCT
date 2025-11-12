using System;
using System.Collections.Generic;
using System.Linq;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 手紙送信トリガー条件種類列挙型
    /// 自動手紙送信システムで使用される条件パターンを定義
    /// 時間経過、ファイル存在状態、またはその組み合わせによる条件判定に対応
    /// ゲーム進行に応じた適切なタイミングでの手紙配信を実現
    /// </summary>
    public enum LetterTriggerType
    {
        /// <summary>
        /// 時間ベース条件
        /// 指定された時間（秒）が経過した場合に手紙を送信
        /// ファイル状態には依存せず、純粋な時間経過のみで判定
        /// 例: ゲーム開始から30秒後、前回の手紙から60秒後等
        /// </summary>
        TimeOnly,
        
        /// <summary>
        /// 単一ファイル存在条件
        /// 指定された特定のファイルが存在する場合に手紙を送信
        /// ユーザーがファイルを作成したタイミングでの反応に使用
        /// 例: Button.txtファイルが作成された時点
        /// </summary>
        FileExistenceOnly,
        
        /// <summary>
        /// 単一ファイル不存在条件
        /// 指定された特定のファイルが存在しない場合に手紙を送信
        /// ユーザーがファイルを削除したタイミングでの反応に使用
        /// 例: TextWindow.txtファイルが削除された時点
        /// </summary>
        FileNotExistenceOnly,
        
        /// <summary>
        /// 複数ファイル全存在条件
        /// 指定されたすべてのファイルが存在する場合に手紙を送信
        /// 複数の条件が同時に満たされた状況での反応に使用
        /// 例: Button.txt、TextWindow.txt、Mouse.txtすべてが存在
        /// </summary>
        AllFilesExist,
        
        /// <summary>
        /// 複数ファイル任意存在条件
        /// 指定されたファイルのうちいずれかが存在する場合に手紙を送信
        /// 複数の選択肢のうち一つでも実行された状況での反応に使用
        /// 例: Button.txtまたはTextWindow.txtのいずれかが存在
        /// </summary>
        AnyFileExists,
        
        /// <summary>
        /// 複数ファイル全不存在条件
        /// 指定されたすべてのファイルが存在しない場合に手紙を送信
        /// ユーザーが複数のファイルをすべて削除した状況での反応に使用
        /// 例: Button.txt、TextWindow.txt、Mouse.txtすべてが削除された
        /// </summary>
        AllFilesNotExist,
        
        /// <summary>
        /// 複数ファイル任意不存在条件
        /// 指定されたファイルのうちいずれかが存在しない場合に手紙を送信
        /// 部分的な削除や不完全な状態での反応に使用
        /// 例: Button.txtまたはTextWindow.txtのいずれかが削除された
        /// </summary>
        AnyFileNotExists,
        
        /// <summary>
        /// 時間＋単一ファイル存在組合せ条件
        /// 指定時間経過かつ特定ファイルが存在する場合に手紙を送信
        /// 時間制限のあるタスクの完了確認に使用
        /// 例: 30秒経過後にButton.txtが存在している場合
        /// </summary>
        TimeAndFileExistence,
        
        /// <summary>
        /// 時間＋単一ファイル不存在組合せ条件
        /// 指定時間経過かつ特定ファイルが存在しない場合に手紙を送信
        /// 時間制限内でのタスク未完了の検出に使用
        /// 例: 60秒経過後にTextWindow.txtが存在しない場合
        /// </summary>
        TimeAndFileNotExistence,
        
        /// <summary>
        /// 時間＋複数ファイル全存在組合せ条件
        /// 指定時間経過かつすべてのファイルが存在する場合に手紙を送信
        /// 複合タスクの時間制限付き完了確認に使用
        /// 例: 120秒経過後にすべての必須ファイルが存在している場合
        /// </summary>
        TimeAndAllFilesExist,
        
        /// <summary>
        /// 時間＋複数ファイル任意存在組合せ条件
        /// 指定時間経過かついずれかのファイルが存在する場合に手紙を送信
        /// 選択的タスクの時間制限付き部分完了確認に使用
        /// 例: 90秒経過後にいずれかのファイルが存在している場合
        /// </summary>
        TimeAndAnyFileExists,
        
        /// <summary>
        /// 時間＋複数ファイル全不存在組合せ条件
        /// 指定時間経過かつすべてのファイルが存在しない場合に手紙を送信
        /// 完全削除タスクの時間制限付き確認に使用
        /// 例: 60秒経過後にすべてのファイルが削除されている場合
        /// </summary>
        TimeAndAllFilesNotExist,
        
        /// <summary>
        /// 時間＋複数ファイル任意不存在組合せ条件
        /// 指定時間経過かついずれかのファイルが存在しない場合に手紙を送信
        /// 部分削除タスクの時間制限付き確認に使用
        /// 例: 45秒経過後にいずれかのファイルが削除されている場合
        /// </summary>
        TimeAndAnyFileNotExists
    }

    /// <summary>
    /// 手紙送信トリガー条件設定クラス
    /// 自動手紙送信システムの条件判定ロジックを管理
    /// 時間ベース、ファイル存在ベース、またはその組み合わせによる条件を設定可能
    /// ゲーム進行状況に応じて適切なタイミングでの手紙配信を制御
    /// </summary>
    public class LetterTriggerCondition
    {
        /// <summary>
        /// トリガー条件の種類
        /// この手紙送信条件が使用する判定方法を指定
        /// 時間のみ、ファイル存在のみ、またはその組み合わせから選択
        /// LetterTriggerType列挙型の値を使用
        /// </summary>
        public LetterTriggerType TriggerType { get; set; }

        /// <summary>
        /// 時間間隔条件（秒単位）
        /// 時間ベース条件で使用される待機時間
        /// この時間が経過した後に条件判定が実行される
        /// 0の場合は時間条件なしとして扱われる
        /// 例: 30秒後、60秒後、120秒後等
        /// </summary>
        public int TimeIntervalSeconds { get; set; }

        /// <summary>
        /// 必須ファイル名（単一ファイル条件用）
        /// 単一ファイルの存在・不存在を判定する際のファイル名
        /// componentsフォルダ内のファイル名（拡張子なし）を指定
        /// 例: "Button", "TextWindow", "Mouse"
        /// RequiredFileNamesが設定されている場合は無視される
        /// </summary>
        public string RequiredFileName { get; set; }

        /// <summary>
        /// 必須ファイル名リスト（複数ファイル条件用）
        /// 複数ファイルの存在・不存在を判定する際のファイル名配列
        /// componentsフォルダ内のファイル名（拡張子なし）のリスト
        /// 例: ["Button", "TextWindow"], ["Mouse", "Key", "Door"]
        /// RequiredFileNameより優先して使用される
        /// </summary>
        public List<string> RequiredFileNames { get; set; }

        /// <summary>
        /// 手紙識別インデックス
        /// この条件に対応する手紙の識別番号
        /// 手紙配信システムで使用される順序番号
        /// 通常は1から順番に設定される
        /// </summary>
        public int LetterIndex { get; set; }

        /// <summary>
        /// コンストラクタ（単一ファイル条件用）
        /// 単一ファイルの存在・不存在を条件とする手紙送信トリガーを作成
        /// 時間条件との組み合わせも可能
        /// </summary>
        /// <param name="letterIndex">手紙の識別番号（1から開始）</param>
        /// <param name="triggerType">トリガー条件の種類</param>
        /// <param name="timeIntervalSeconds">時間間隔（秒）、0の場合は時間条件なし</param>
        /// <param name="requiredFileName">条件対象のファイル名（拡張子なし）</param>
        public LetterTriggerCondition(int letterIndex, LetterTriggerType triggerType, int timeIntervalSeconds = 0, string requiredFileName = null)
        {
            // 手紙識別番号を設定
            LetterIndex = letterIndex;
            // トリガー条件種類を設定
            TriggerType = triggerType;
            // 時間間隔を設定（0の場合は時間条件なし）
            TimeIntervalSeconds = timeIntervalSeconds;
            // 単一ファイル名を設定
            RequiredFileName = requiredFileName;
            // 複数ファイルリストを空で初期化
            RequiredFileNames = new List<string>();
        }

        /// <summary>
        /// コンストラクタ（複数ファイル条件用）
        /// 複数ファイルの存在・不存在を条件とする手紙送信トリガーを作成
        /// 時間条件との組み合わせも可能
        /// </summary>
        /// <param name="letterIndex">手紙の識別番号（1から開始）</param>
        /// <param name="triggerType">トリガー条件の種類</param>
        /// <param name="timeIntervalSeconds">時間間隔（秒）</param>
        /// <param name="requiredFileNames">条件対象のファイル名リスト（拡張子なし）</param>
        public LetterTriggerCondition(int letterIndex, LetterTriggerType triggerType, int timeIntervalSeconds, List<string> requiredFileNames)
        {
            // 手紙識別番号を設定
            LetterIndex = letterIndex;
            // トリガー条件種類を設定
            TriggerType = triggerType;
            // 時間間隔を設定
            TimeIntervalSeconds = timeIntervalSeconds;
            // 単一ファイル名はnullに設定
            RequiredFileName = null;
            // 複数ファイルリストを設定（nullの場合は空リストで初期化）
            RequiredFileNames = requiredFileNames ?? new List<string>();
        }

        /// <summary>
        /// 時間のみの条件を作成（ファクトリーメソッド）
        /// 指定された時間が経過した時点で手紙を送信する条件を生成
        /// ファイル状態には依存しない純粋な時間ベース条件
        /// </summary>
        /// <param name="letterIndex">手紙の識別番号</param>
        /// <param name="timeIntervalSeconds">経過時間（秒）</param>
        /// <returns>設定済みの手紙送信条件インスタンス</returns>
        public static LetterTriggerCondition CreateTimeOnly(int letterIndex, int timeIntervalSeconds)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.TimeOnly, timeIntervalSeconds);
        }

        /// <summary>
        /// ファイル存在のみの条件を作成（ファクトリーメソッド）
        /// 指定されたファイルが存在する時点で手紙を送信する条件を生成
        /// 時間条件なしでファイル作成タイミングでの即座反応
        /// </summary>
        /// <param name="letterIndex">手紙の識別番号</param>
        /// <param name="requiredFileName">存在確認対象のファイル名（拡張子なし）</param>
        /// <returns>設定済みの手紙送信条件インスタンス</returns>
        public static LetterTriggerCondition CreateFileExistenceOnly(int letterIndex, string requiredFileName)
        {
            return new LetterTriggerCondition(letterIndex, LetterTriggerType.FileExistenceOnly, 0, requiredFileName);
        }

        /// <summary>
        /// ファイル不存在のみの条件を作成（ファクトリーメソッド）
        /// 指定されたファイルが存在しない時点で手紙を送信する条件を生成
        /// 時間条件なしでファイル削除タイミングでの即座反応
        /// </summary>
        /// <param name="letterIndex">手紙の識別番号</param>
        /// <param name="requiredFileName">不存在確認対象のファイル名（拡張子なし）</param>
        /// <returns>設定済みの手紙送信条件インスタンス</returns>
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
        /// この手紙送信条件の内容を人間が読める形式で取得
        /// デバッグ、ログ出力、UI表示等で条件内容を確認するために使用
        /// 設定されたTriggerTypeに応じて適切な説明文を生成
        /// </summary>
        /// <returns>条件の詳細を説明する日本語文字列</returns>
        public string GetDescription()
        {
            // 複数ファイル名が設定されている場合はカンマ区切りで結合、単一ファイルの場合はそのまま使用
            var fileNames = RequiredFileNames?.Count > 0 ? string.Join(", ", RequiredFileNames) : RequiredFileName;
            
            // トリガータイプに応じた説明文を生成
            return TriggerType switch
            {
                // 時間のみの条件
                LetterTriggerType.TimeOnly => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後",
                
                // 単一ファイル存在条件
                LetterTriggerType.FileExistenceOnly => $"手紙{LetterIndex}: {RequiredFileName}ファイル作成時",
                
                // 単一ファイル不存在条件
                LetterTriggerType.FileNotExistenceOnly => $"手紙{LetterIndex}: {RequiredFileName}ファイル削除時",
                
                // 複数ファイル全存在条件
                LetterTriggerType.AllFilesExist => $"手紙{LetterIndex}: [{fileNames}]すべてのファイル存在時",
                
                // 複数ファイル任意存在条件
                LetterTriggerType.AnyFileExists => $"手紙{LetterIndex}: [{fileNames}]いずれかのファイル存在時",
                
                // 複数ファイル全不存在条件
                LetterTriggerType.AllFilesNotExist => $"手紙{LetterIndex}: [{fileNames}]すべてのファイル不存在時",
                
                // 複数ファイル任意不存在条件
                LetterTriggerType.AnyFileNotExists => $"手紙{LetterIndex}: [{fileNames}]いずれかのファイル不存在時",
                
                // 時間＋単一ファイル存在組合せ条件
                LetterTriggerType.TimeAndFileExistence => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ{RequiredFileName}ファイル存在時",
                
                // 時間＋単一ファイル不存在組合せ条件
                LetterTriggerType.TimeAndFileNotExistence => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ{RequiredFileName}ファイル不存在時",
                
                // 時間＋複数ファイル全存在組合せ条件
                LetterTriggerType.TimeAndAllFilesExist => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ[{fileNames}]すべてのファイル存在時",
                
                // 時間＋複数ファイル任意存在組合せ条件
                LetterTriggerType.TimeAndAnyFileExists => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ[{fileNames}]いずれかのファイル存在時",
                
                // 時間＋複数ファイル全不存在組合せ条件
                LetterTriggerType.TimeAndAllFilesNotExist => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ[{fileNames}]すべてのファイル不存在時",
                
                // 時間＋複数ファイル任意不存在組合せ条件
                LetterTriggerType.TimeAndAnyFileNotExists => $"手紙{LetterIndex}: {TimeIntervalSeconds}秒後かつ[{fileNames}]いずれかのファイル不存在時",
                
                // 未定義のトリガータイプの場合
                _ => $"手紙{LetterIndex}: 不明な条件"
            };
        }
    }
}