using SELLCT.Core.Entities;

namespace SELLCT.Core.Factories
{
    /// <summary>
    /// パズル条件を生成するファクトリー
    /// </summary>
    public static class ConditionFactory
    {
        /// <summary>
        /// アクション実行回数の条件を作成
        /// </summary>
        public static PuzzleCondition ActionCount(string key, int expectedValue, PuzzleCondition.ComparisonOperator op = PuzzleCondition.ComparisonOperator.Equal)
        {
            return new PuzzleCondition
            {
                Type = PuzzleCondition.ConditionType.ActionCount,
                Key = key,
                ExpectedValue = expectedValue,
                Operator = op
            };
        }

        /// <summary>
        /// 初回実行の条件を作成
        /// </summary>
        public static PuzzleCondition FirstTime(string actionKey)
        {
            return ActionCount(actionKey, 0, PuzzleCondition.ComparisonOperator.Equal);
        }

        /// <summary>
        /// 2回目以降の実行条件を作成
        /// </summary>
        public static PuzzleCondition NotFirstTime(string actionKey)
        {
            return ActionCount(actionKey, 0, PuzzleCondition.ComparisonOperator.GreaterThan);
        }

        /// <summary>
        /// コンポーネント存在条件を作成
        /// </summary>
        public static PuzzleCondition ComponentExists(string componentName, bool shouldExist = true)
        {
            return new PuzzleCondition
            {
                Type = PuzzleCondition.ConditionType.ComponentExists,
                Key = componentName,
                ExpectedValue = shouldExist,
                Operator = PuzzleCondition.ComparisonOperator.Equal
            };
        }

        /// <summary>
        /// コンポーネントが存在しない条件を作成
        /// </summary>
        public static PuzzleCondition ComponentNotExists(string componentName)
        {
            return ComponentExists(componentName, false);
        }

        /// <summary>
        /// 変数値の条件を作成
        /// </summary>
        public static PuzzleCondition Variable(string key, object expectedValue, PuzzleCondition.ComparisonOperator op = PuzzleCondition.ComparisonOperator.Equal)
        {
            return new PuzzleCondition
            {
                Type = PuzzleCondition.ConditionType.Variable,
                Key = key,
                ExpectedValue = expectedValue,
                Operator = op
            };
        }

        /// <summary>
        /// フラグがtrueの条件を作成
        /// </summary>
        public static PuzzleCondition FlagTrue(string flagName)
        {
            return Variable(flagName, true);
        }

        /// <summary>
        /// フラグがfalseの条件を作成
        /// </summary>
        public static PuzzleCondition FlagFalse(string flagName)
        {
            return Variable(flagName, false);
        }

        /// <summary>
        /// イベント完了条件を作成
        /// </summary>
        public static PuzzleCondition EventCompleted(string eventId, bool shouldBeCompleted = true)
        {
            return new PuzzleCondition
            {
                Type = PuzzleCondition.ConditionType.EventCompleted,
                Key = eventId,
                ExpectedValue = shouldBeCompleted,
                Operator = PuzzleCondition.ComparisonOperator.Equal
            };
        }

        /// <summary>
        /// 複数の権限コンポーネントが全て存在する条件を作成
        /// </summary>
        public static PuzzleCondition[] AllPermissionsExist()
        {
            return new[]
            {
                ComponentExists("AdminRights"),
                ComponentExists("FileAccess"),
                ComponentExists("NetworkAccess"),
                ComponentExists("SystemControl")
            };
        }
    }
}