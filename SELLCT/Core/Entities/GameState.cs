using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// ゲームの状態を管理するクラス
    /// プレイヤーの行動、パズルの進行状態、ゲームフラグを一元管理
    /// パズルシステムの条件判定やゲーム進行制御に使用される
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// アクション実行回数
        /// 各アクション（ファイル作成、削除など）が何回実行されたかを記録
        /// キー: "アクションタイプ_コンポーネント名" 形式（例: "Created_Button"）
        /// </summary>
        public Dictionary<string, int> ActionCounts { get; set; }

        /// <summary>
        /// 完了済みイベント
        /// 一度だけ実行するパズルやイベントのIDを保持
        /// 重複実行防止や進行管理に使用される
        /// </summary>
        public HashSet<string> CompletedEvents { get; set; }

        /// <summary>
        /// カスタム変数
        /// 任意のゲーム状態を保持するフレキシブルなストレージ
        /// ヒント表示状態、ユーザーアクションフラグなどを保持
        /// </summary>
        public Dictionary<string, object> Variables { get; set; }

        /// <summary>
        /// 総コンポーネント数
        /// ゲーム中に作成されたコンポーネントの総数
        /// パズルの進行判定や統計情報として使用
        /// </summary>
        public int TotalComponents { get; set; }

        /// <summary>
        /// コンストラクタ
        /// GameStateインスタンスを初期化し、各コレクションを空の状態で作成
        /// ゲーム開始時のクリーンな状態を設定
        /// </summary>
        public GameState()
        {
            // アクション実行回数を管理する辞書を初期化
            ActionCounts = new Dictionary<string, int>();
            // 完了イベントを記録するセットを初期化
            CompletedEvents = new HashSet<string>();
            // カスタム変数を保持する辞書を初期化
            Variables = new Dictionary<string, object>();
            // コンポーネント数をゼロで初期化
            TotalComponents = 0;
        }

        /// <summary>
        /// アクション実行回数を増加
        /// 指定されたアクションの実行回数をカウントアップ
        /// 初回実行の場合は1で初期化、それ以降はインクリメント
        /// </summary>
        /// <param name="actionKey">アクションを識別するキー（例: "Created_Button"）</param>
        public void IncrementActionCount(string actionKey)
        {
            // 既にカウントが存在する場合はインクリメント
            if (ActionCounts.ContainsKey(actionKey))
            {
                ActionCounts[actionKey]++;
            }
            else
            {
                // 初回実行の場合は1で初期化
                ActionCounts[actionKey] = 1;
            }
        }

        /// <summary>
        /// アクション実行回数を取得
        /// 指定されたアクションが何回実行されたかを取得
        /// パズルシステムの条件判定で使用される
        /// </summary>
        /// <param name="actionKey">アクションを識別するキー</param>
        /// <returns>実行回数（未実行の場合は0）</returns>
        public int GetActionCount(string actionKey)
        {
            // キーが存在する場合は実際の回数、そうでなければ0を返す
            return ActionCounts.ContainsKey(actionKey) ? ActionCounts[actionKey] : 0;
        }

        /// <summary>
        /// イベントを完了済みとしてマーク
        /// 一度だけ実行すべきイベントやパズルを完了済みとして記録
        /// 重複実行を防止するために使用される
        /// </summary>
        /// <param name="eventKey">完了したイベントを識別するキー</param>
        public void MarkEventCompleted(string eventKey)
        {
            // 完了イベントセットにキーを追加、重複は自動的に除外される
            CompletedEvents.Add(eventKey);
        }

        /// <summary>
        /// イベントが完了済みかチェック
        /// 指定されたイベントが既に完了しているかどうかを判定
        /// パズルの重複実行防止や条件判定で使用
        /// </summary>
        /// <param name="eventKey">チェックしたいイベントのキー</param>
        /// <returns>true: 完了済み, false: 未完了</returns>
        public bool IsEventCompleted(string eventKey)
        {
            // 完了イベントセットにキーが含まれているかどうかをチェック
            return CompletedEvents.Contains(eventKey);
        }

        /// <summary>
        /// カスタム変数を設定
        /// 任意のキーでゲーム状態を保持するための汎用メソッド
        /// ヒント表示状態、ユーザーアクションフラグなどを記録
        /// </summary>
        /// <param name="key">変数を識別するキー</param>
        /// <param name="value">設定する値（任意の型）</param>
        public void SetVariable(string key, object value)
        {
            // 辞書にキーと値を保存、既存の場合は上書き
            Variables[key] = value;
        }

        /// <summary>
        /// カスタム変数を取得
        /// 指定されたキーの値を指定された型で取得
        /// 型安全性を保ち、キーが存在しないか型が合わない場合はデフォルト値を返す
        /// </summary>
        /// <typeparam name="T">取得したい値の型</typeparam>
        /// <param name="key">変数を識別するキー</param>
        /// <param name="defaultValue">キーが存在しない場合のデフォルト値</param>
        /// <returns>変数の値またはデフォルト値</returns>
        public T GetVariable<T>(string key, T defaultValue = default(T))
        {
            // キーが存在し、かつ値が指定された型でキャスト可能な場合
            if (Variables.ContainsKey(key) && Variables[key] is T)
            {
                return (T)Variables[key];
            }
            // そうでない場合はデフォルト値を返す
            return defaultValue;
        }

        /// <summary>
        /// Buttonヒント関連の状態プロパティ
        /// ユーザーがボタンを動かすためのヒントシステムを管理
        /// </summary>
        /// <summary>
        /// ボタンヒントがトリガーされたかどうか
        /// ユーザーが一定時間操作しなかった場合にヒント表示を開始するフラグ
        /// </summary>
        public bool IsButtonHintTriggered
        {
            get => GetVariable("IsButtonHintTriggered", false);
            set => SetVariable("IsButtonHintTriggered", value);
        }

        /// <summary>
        /// メッセージが公開されたかどうか
        /// ボタンの後ろに隠されたメッセージが表示されたかどうかのフラグ
        /// </summary>
        public bool IsMessageRevealed
        {
            get => GetVariable("IsMessageRevealed", false);
            set => SetVariable("IsMessageRevealed", value);
        }

        /// <summary>
        /// ボタンヒントが表示されたかどうか
        /// ユーザーにボタンを動かすためのヒントが既に表示されたかどうかのフラグ
        /// </summary>
        public bool ButtonHintShown
        {
            get => GetVariable("ButtonHintShown", false);
            set => SetVariable("ButtonHintShown", value);
        }

        /// <summary>
        /// Authority関連のヒント状態プロパティ
        /// ゲームの核心であるAuthorityフォルダを探すためのヒントシステムを管理
        /// </summary>
        /// <summary>
        /// Authorityフォルダが公開されたかどうか
        /// 隠されたAuthorityフォルダがユーザーに発見されたかどうかのフラグ
        /// </summary>
        public bool IsAuthorityFolderRevealed
        {
            get => GetVariable("IsAuthorityFolderRevealed", false);
            set => SetVariable("IsAuthorityFolderRevealed", value);
        }

        /// <summary>
        /// Authorityヒント1が表示されたかどうか
        /// Authorityフォルダを探すための最初のヒントが表示されたかどうかのフラグ
        /// </summary>
        public bool AuthorityHint1Shown
        {
            get => GetVariable("AuthorityHint1Shown", false);
            set => SetVariable("AuthorityHint1Shown", value);
        }

        /// <summary>
        /// Authorityヒント2が表示されたかどうか
        /// Authorityフォルダ探索の為の第2次ヒントが表示されたかどうかのフラグ
        /// </summary>
        public bool AuthorityHint2Shown
        {
            get => GetVariable("AuthorityHint2Shown", false);
            set => SetVariable("AuthorityHint2Shown", value);
        }

        /// <summary>
        /// Authorityヒント3が表示されたかどうか
        /// Authorityフォルダ探索の為の最終ヒントが表示されたかどうかのフラグ
        /// </summary>
        public bool AuthorityHint3Shown
        {
            get => GetVariable("AuthorityHint3Shown", false);
            set => SetVariable("AuthorityHint3Shown", value);
        }

        /// <summary>
        /// ゲームウィンドウが移動されたかどうか
        /// ユーザーがゲームウィンドウをドラッグして移動させたかどうかのフラグ
        /// </summary>
        public bool IsGameWindowMoved
        {
            get => GetVariable("IsGameWindowMoved", false);
            set => SetVariable("IsGameWindowMoved", value);
        }
    }
}