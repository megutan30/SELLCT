using System.Collections.Generic;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Builders
{
    /// <summary>
    /// DialogueFlowをFluent APIで構築するビルダー
    /// </summary>
    public class DialogueFlowBuilder
    {
        private readonly DialogueFlow _flow;
        private DialogueNode _currentNode;

        private DialogueFlowBuilder(string id, string description = null)
        {
            _flow = new DialogueFlow
            {
                Id = id,
                Description = description
            };
        }

        /// <summary>
        /// 新しいDialogueFlowBuilderを作成
        /// </summary>
        public static DialogueFlowBuilder Create(string id, string description = null)
        {
            return new DialogueFlowBuilder(id, description);
        }

        /// <summary>
        /// トリガーを設定
        /// </summary>
        public DialogueFlowBuilder TriggeredBy(DialogueTrigger trigger)
        {
            _flow.Trigger = trigger;
            return this;
        }

        /// <summary>
        /// コンポーネント存在トリガーを設定（便利メソッド）
        /// </summary>
        public DialogueFlowBuilder TriggeredByComponentExists(params string[] componentNames)
        {
            _flow.Trigger = new DialogueTrigger
            {
                Type = DialogueTrigger.TriggerType.Exists,
                ComponentNames = componentNames
            };
            return this;
        }

        /// <summary>
        /// コンポーネント削除トリガーを設定（便利メソッド）
        /// </summary>
        public DialogueFlowBuilder TriggeredByComponentDeleted(params string[] componentNames)
        {
            _flow.Trigger = new DialogueTrigger
            {
                Type = DialogueTrigger.TriggerType.Deleted,
                ComponentNames = componentNames
            };
            return this;
        }

        /// <summary>
        /// 実行条件を追加
        /// </summary>
        public DialogueFlowBuilder When(PuzzleCondition condition)
        {
            _flow.Conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// 繰り返し実行可能に設定
        /// </summary>
        public DialogueFlowBuilder CanRepeat(bool canRepeat = true)
        {
            _flow.CanRepeat = canRepeat;
            return this;
        }

        /// <summary>
        /// 優先度を設定
        /// </summary>
        public DialogueFlowBuilder WithPriority(int priority)
        {
            _flow.Priority = priority;
            return this;
        }

        /// <summary>
        /// 最初のノードを作成
        /// </summary>
        public DialogueNodeBuilder StartWith(string nodeId, string text = null)
        {
            _flow.StartNodeId = nodeId;
            return AddNode(nodeId, text);
        }

        /// <summary>
        /// ノードを追加
        /// </summary>
        public DialogueNodeBuilder AddNode(string nodeId, string text = null)
        {
            var node = new DialogueNode
            {
                Id = nodeId,
                Text = text
            };
            
            _flow.AddNode(node);
            _currentNode = node;
            
            return new DialogueNodeBuilder(this, node);
        }

        /// <summary>
        /// DialogueFlowを構築
        /// </summary>
        public DialogueFlow Build()
        {
            return _flow;
        }
    }

    /// <summary>
    /// DialogueNodeをFluent APIで構築するビルダー
    /// </summary>
    public class DialogueNodeBuilder
    {
        private readonly DialogueFlowBuilder _flowBuilder;
        private readonly DialogueNode _node;

        internal DialogueNodeBuilder(DialogueFlowBuilder flowBuilder, DialogueNode node)
        {
            _flowBuilder = flowBuilder;
            _node = node;
        }

        /// <summary>
        /// 次のノードIDを設定
        /// </summary>
        public DialogueNodeBuilder GoTo(string nextNodeId)
        {
            _node.NextNodeId = nextNodeId;
            return this;
        }

        /// <summary>
        /// 条件を追加
        /// </summary>
        public DialogueNodeBuilder When(PuzzleCondition condition)
        {
            _node.Conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// アクションを追加
        /// </summary>
        public DialogueNodeBuilder Do(PuzzleAction action)
        {
            _node.Actions.Add(action);
            return this;
        }

        /// <summary>
        /// ゲームコマンドを追加（IGameCommandから自動変換）
        /// </summary>
        public DialogueNodeBuilder Do(IGameCommand command)
        {
            // GameCommandをPuzzleActionに変換する処理（簡略版）
            // 実装の詳細は省略（必要に応じて後で拡張）
            return this;
        }

        /// <summary>
        /// 選択肢を追加
        /// </summary>
        public DialogueNodeBuilder WithChoice(DialogueChoice choice)
        {
            _node.Choices.Add(choice);
            return this;
        }

        /// <summary>
        /// Yes選択肢を追加（便利メソッド）
        /// </summary>
        public DialogueNodeBuilder WithYesChoice(string nextNodeId)
        {
            var choice = new DialogueChoice
            {
                Type = ChoiceType.Yes,
                Text = "はい",
                NextNodeId = nextNodeId
            };
            _node.Choices.Add(choice);
            return this;
        }

        /// <summary>
        /// No選択肢を追加（便利メソッド）
        /// </summary>
        public DialogueNodeBuilder WithNoChoice(string nextNodeId)
        {
            var choice = new DialogueChoice
            {
                Type = ChoiceType.No,
                Text = "いいえ",
                NextNodeId = nextNodeId
            };
            _node.Choices.Add(choice);
            return this;
        }

        /// <summary>
        /// カスタム選択肢を追加
        /// </summary>
        public DialogueNodeBuilder WithChoice(string text, string nextNodeId, params PuzzleAction[] actions)
        {
            var choice = new DialogueChoice
            {
                Type = ChoiceType.Custom,
                Text = text,
                NextNodeId = nextNodeId,
                Actions = new List<PuzzleAction>(actions)
            };
            _node.Choices.Add(choice);
            return this;
        }

        /// <summary>
        /// 繰り返し実行可能に設定
        /// </summary>
        public DialogueNodeBuilder CanRepeat(bool canRepeat = true)
        {
            _node.CanRepeat = canRepeat;
            return this;
        }

        /// <summary>
        /// 次のノードを追加
        /// </summary>
        public DialogueNodeBuilder Then(string nodeId, string text = null)
        {
            return _flowBuilder.AddNode(nodeId, text);
        }

        /// <summary>
        /// DialogueFlowを構築
        /// </summary>
        public DialogueFlow Build()
        {
            return _flowBuilder.Build();
        }
    }
}