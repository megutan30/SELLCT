using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// ゲームの状態を管理するクラス
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// アクション実行回数
        /// </summary>
        public Dictionary<string, int> ActionCounts { get; set; }

        /// <summary>
        /// 完了済みイベント
        /// </summary>
        public HashSet<string> CompletedEvents { get; set; }

        /// <summary>
        /// カスタム変数
        /// </summary>
        public Dictionary<string, object> Variables { get; set; }

        /// <summary>
        /// 総コンポーネント数
        /// </summary>
        public int TotalComponents { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameState()
        {
            ActionCounts = new Dictionary<string, int>();
            CompletedEvents = new HashSet<string>();
            Variables = new Dictionary<string, object>();
            TotalComponents = 0;
        }

        /// <summary>
        /// アクション実行回数を増加
        /// </summary>
        public void IncrementActionCount(string actionKey)
        {
            if (ActionCounts.ContainsKey(actionKey))
            {
                ActionCounts[actionKey]++;
            }
            else
            {
                ActionCounts[actionKey] = 1;
            }
        }

        /// <summary>
        /// アクション実行回数を取得
        /// </summary>
        public int GetActionCount(string actionKey)
        {
            return ActionCounts.ContainsKey(actionKey) ? ActionCounts[actionKey] : 0;
        }

        /// <summary>
        /// イベントを完了済みとしてマーク
        /// </summary>
        public void MarkEventCompleted(string eventKey)
        {
            CompletedEvents.Add(eventKey);
        }

        /// <summary>
        /// イベントが完了済みかチェック
        /// </summary>
        public bool IsEventCompleted(string eventKey)
        {
            return CompletedEvents.Contains(eventKey);
        }

        /// <summary>
        /// カスタム変数を設定
        /// </summary>
        public void SetVariable(string key, object value)
        {
            Variables[key] = value;
        }

        /// <summary>
        /// カスタム変数を取得
        /// </summary>
        public T GetVariable<T>(string key, T defaultValue = default(T))
        {
            if (Variables.ContainsKey(key) && Variables[key] is T)
            {
                return (T)Variables[key];
            }
            return defaultValue;
        }

        /// <summary>
        /// Buttonヒント関連の状態プロパティ
        /// </summary>
        public bool IsButtonHintTriggered
        {
            get => GetVariable("IsButtonHintTriggered", false);
            set => SetVariable("IsButtonHintTriggered", value);
        }

        public bool IsMessageRevealed
        {
            get => GetVariable("IsMessageRevealed", false);
            set => SetVariable("IsMessageRevealed", value);
        }

        public bool ButtonHintShown
        {
            get => GetVariable("ButtonHintShown", false);
            set => SetVariable("ButtonHintShown", value);
        }
    }
}