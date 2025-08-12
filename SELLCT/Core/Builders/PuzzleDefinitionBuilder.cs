using System.Collections.Generic;
using SELLCT.Core.Entities;

namespace SELLCT.Core.Builders
{
    /// <summary>
    /// PuzzleDefinitionをFluent APIで構築するビルダー
    /// </summary>
    public class PuzzleDefinitionBuilder
    {
        private readonly PuzzleDefinition _puzzle;

        private PuzzleDefinitionBuilder(string id)
        {
            _puzzle = new PuzzleDefinition
            {
                Id = id,
                Actions = new List<PuzzleAction>(),
                Conditions = new List<PuzzleCondition>()
            };
        }

        /// <summary>
        /// 新しいPuzzleDefinitionBuilderを作成
        /// </summary>
        public static PuzzleDefinitionBuilder Create(string id)
        {
            return new PuzzleDefinitionBuilder(id);
        }

        /// <summary>
        /// コンポーネント存在時のパズルを作成
        /// </summary>
        public static PuzzleDefinitionBuilder OnComponentExists(params string[] componentNames)
        {
            var id = $"{string.Join("_", componentNames)}_Exists";
            var builder = new PuzzleDefinitionBuilder(id);
            builder._puzzle.Trigger = new PuzzleTrigger
            {
                Type = PuzzleTrigger.TriggerType.Exists,
                ComponentNames = componentNames
            };
            return builder;
        }

        /// <summary>
        /// コンポーネント削除時のパズルを作成
        /// </summary>
        public static PuzzleDefinitionBuilder OnComponentDeleted(params string[] componentNames)
        {
            var id = $"{string.Join("_", componentNames)}_Deleted";
            var builder = new PuzzleDefinitionBuilder(id);
            builder._puzzle.Trigger = new PuzzleTrigger
            {
                Type = PuzzleTrigger.TriggerType.Deleted,
                ComponentNames = componentNames
            };
            return builder;
        }

        /// <summary>
        /// コンポーネント作成時のパズルを作成
        /// </summary>
        public static PuzzleDefinitionBuilder OnComponentCreated(params string[] componentNames)
        {
            var id = $"{string.Join("_", componentNames)}_Created";
            var builder = new PuzzleDefinitionBuilder(id);
            builder._puzzle.Trigger = new PuzzleTrigger
            {
                Type = PuzzleTrigger.TriggerType.Created,
                ComponentNames = componentNames
            };
            return builder;
        }

        /// <summary>
        /// コンポーネントリネーム時のパズルを作成
        /// </summary>
        public static PuzzleDefinitionBuilder OnComponentRenamed(string oldName, string newName)
        {
            var id = $"{oldName}_Renamed_To_{newName}";
            var builder = new PuzzleDefinitionBuilder(id);
            builder._puzzle.Trigger = new PuzzleTrigger
            {
                Type = PuzzleTrigger.TriggerType.Renamed,
                OldComponentName = oldName,
                ComponentName = newName
            };
            return builder;
        }

        /// <summary>
        /// 説明を設定
        /// </summary>
        public PuzzleDefinitionBuilder WithDescription(string description)
        {
            _puzzle.Description = description;
            return this;
        }

        /// <summary>
        /// 実行条件を追加
        /// </summary>
        public PuzzleDefinitionBuilder When(PuzzleCondition condition)
        {
            _puzzle.Conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// 初回実行時のみの条件を追加
        /// </summary>
        public PuzzleDefinitionBuilder WhenFirstTime()
        {
            var condition = new PuzzleCondition
            {
                Type = PuzzleCondition.ConditionType.ActionCount,
                Key = GetActionKey(),
                ExpectedValue = 0,
                Operator = PuzzleCondition.ComparisonOperator.Equal
            };
            _puzzle.Conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// 2回目以降の実行条件を追加
        /// </summary>
        public PuzzleDefinitionBuilder WhenNotFirstTime()
        {
            var condition = new PuzzleCondition
            {
                Type = PuzzleCondition.ConditionType.ActionCount,
                Key = GetActionKey(),
                ExpectedValue = 0,
                Operator = PuzzleCondition.ComparisonOperator.GreaterThan
            };
            _puzzle.Conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// コンポーネントが存在する条件を追加
        /// </summary>
        public PuzzleDefinitionBuilder WhenComponentExists(string componentName)
        {
            var condition = new PuzzleCondition
            {
                Type = PuzzleCondition.ConditionType.ComponentExists,
                Key = componentName,
                ExpectedValue = true,
                Operator = PuzzleCondition.ComparisonOperator.Equal
            };
            _puzzle.Conditions.Add(condition);
            return this;
        }

        /// <summary>
        /// アクションを追加
        /// </summary>
        public PuzzleDefinitionBuilder Do(PuzzleAction action)
        {
            _puzzle.Actions.Add(action);
            return this;
        }

        /// <summary>
        /// 複数のアクションを追加
        /// </summary>
        public PuzzleDefinitionBuilder Do(params PuzzleAction[] actions)
        {
            foreach (var action in actions)
            {
                _puzzle.Actions.Add(action);
            }
            return this;
        }

        /// <summary>
        /// メッセージを表示するアクション（便利メソッド）
        /// </summary>
        public PuzzleDefinitionBuilder ShowMessage(string message)
        {
            var action = new PuzzleAction
            {
                Type = PuzzleAction.ActionType.ShowDialog,
                Message = message
            };
            _puzzle.Actions.Add(action);
            return this;
        }

        /// <summary>
        /// UI要素の可視性を設定（便利メソッド）
        /// </summary>
        public PuzzleDefinitionBuilder ShowMainButton(bool visible = true)
        {
            var action = new PuzzleAction
            {
                Type = PuzzleAction.ActionType.SetMainButtonVisibility,
                IsVisible = visible
            };
            _puzzle.Actions.Add(action);
            return this;
        }

        /// <summary>
        /// キーの可視性を設定（便利メソッド）
        /// </summary>
        public PuzzleDefinitionBuilder ShowKey(bool visible = true)
        {
            var action = new PuzzleAction
            {
                Type = PuzzleAction.ActionType.SetKeyVisibility,
                IsVisible = visible
            };
            _puzzle.Actions.Add(action);
            return this;
        }

        /// <summary>
        /// テキストウィンドウの可視性を設定（便利メソッド）
        /// </summary>
        public PuzzleDefinitionBuilder ShowTextWindow(bool visible = true)
        {
            var action = new PuzzleAction
            {
                Type = PuzzleAction.ActionType.SetTextWindowVisibility,
                IsVisible = visible
            };
            _puzzle.Actions.Add(action);
            return this;
        }

        /// <summary>
        /// プロセス起動（便利メソッド）
        /// </summary>
        public PuzzleDefinitionBuilder StartExplorer()
        {
            var action = new PuzzleAction
            {
                Type = PuzzleAction.ActionType.StartExplorer
            };
            _puzzle.Actions.Add(action);
            return this;
        }

        /// <summary>
        /// 繰り返し実行可能に設定
        /// </summary>
        public PuzzleDefinitionBuilder CanRepeat(bool canRepeat = true)
        {
            _puzzle.CanRepeat = canRepeat;
            return this;
        }

        /// <summary>
        /// 優先度を設定
        /// </summary>
        public PuzzleDefinitionBuilder WithPriority(int priority)
        {
            _puzzle.Priority = priority;
            return this;
        }

        /// <summary>
        /// PuzzleDefinitionを構築
        /// </summary>
        public PuzzleDefinition Build()
        {
            return _puzzle;
        }

        /// <summary>
        /// アクションキーを生成（条件判定用）
        /// </summary>
        private string GetActionKey()
        {
            if (_puzzle.Trigger?.ComponentNames != null)
            {
                return $"{_puzzle.Trigger.Type}_{string.Join("_", _puzzle.Trigger.ComponentNames)}";
            }
            return $"{_puzzle.Trigger?.Type}_{_puzzle.Trigger?.ComponentName}";
        }
    }
}