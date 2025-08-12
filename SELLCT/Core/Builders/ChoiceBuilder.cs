using System.Collections.Generic;
using SELLCT.Core.Entities;

namespace SELLCT.Core.Builders
{
    /// <summary>
    /// DialogueChoiceを構築するためのヘルパークラス
    /// </summary>
    public static class Choice
    {
        /// <summary>
        /// Yes選択肢を作成
        /// </summary>
        public static DialogueChoiceBuilder Yes(string text = "はい")
        {
            return new DialogueChoiceBuilder(ChoiceType.Yes, text);
        }

        /// <summary>
        /// No選択肢を作成
        /// </summary>
        public static DialogueChoiceBuilder No(string text = "いいえ")
        {
            return new DialogueChoiceBuilder(ChoiceType.No, text);
        }

        /// <summary>
        /// カスタム選択肢を作成
        /// </summary>
        public static DialogueChoiceBuilder Custom(string text)
        {
            return new DialogueChoiceBuilder(ChoiceType.Custom, text);
        }
    }

    /// <summary>
    /// DialogueChoiceを構築するビルダー
    /// </summary>
    public class DialogueChoiceBuilder
    {
        private readonly DialogueChoice _choice;

        internal DialogueChoiceBuilder(ChoiceType type, string text)
        {
            _choice = new DialogueChoice
            {
                Type = type,
                Text = text
            };
        }

        /// <summary>
        /// 次のノードIDを設定
        /// </summary>
        public DialogueChoiceBuilder GoTo(string nextNodeId)
        {
            _choice.NextNodeId = nextNodeId;
            return this;
        }

        /// <summary>
        /// 条件を追加
        /// </summary>
        public DialogueChoiceBuilder When(PuzzleCondition condition)
        {
            _choice.Conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// アクションを追加
        /// </summary>
        public DialogueChoiceBuilder Do(PuzzleAction action)
        {
            _choice.Actions.Add(action);
            return this;
        }

        /// <summary>
        /// 複数のアクションを追加
        /// </summary>
        public DialogueChoiceBuilder Do(params PuzzleAction[] actions)
        {
            foreach (var action in actions)
            {
                _choice.Actions.Add(action);
            }
            return this;
        }

        /// <summary>
        /// DialogueChoiceを構築
        /// </summary>
        public DialogueChoice Build()
        {
            return _choice;
        }

        /// <summary>
        /// DialogueChoiceへの暗黙的変換
        /// </summary>
        public static implicit operator DialogueChoice(DialogueChoiceBuilder builder)
        {
            return builder.Build();
        }
    }
}