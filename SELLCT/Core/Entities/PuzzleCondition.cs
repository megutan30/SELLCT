namespace SELLCT.Core.Entities
{
    /// <summary>
    /// パズル実行条件を定義するクラス
    /// </summary>
    public class PuzzleCondition
    {
        /// <summary>
        /// 条件の種類
        /// </summary>
        public enum ConditionType
        {
            /// <summary>
            /// アクション実行回数による条件
            /// </summary>
            ActionCount,

            /// <summary>
            /// イベント完了状態による条件
            /// </summary>
            EventCompleted,

            /// <summary>
            /// コンポーネント存在による条件
            /// </summary>
            ComponentExists,

            /// <summary>
            /// カスタム変数による条件
            /// </summary>
            Variable,

            /// <summary>
            /// 常に実行
            /// </summary>
            Always
        }

        /// <summary>
        /// 比較演算子
        /// </summary>
        public enum ComparisonOperator
        {
            /// <summary>
            /// 等しい
            /// </summary>
            Equal,

            /// <summary>
            /// 等しくない
            /// </summary>
            NotEqual,

            /// <summary>
            /// より大きい
            /// </summary>
            GreaterThan,

            /// <summary>
            /// より小さい
            /// </summary>
            LessThan,

            /// <summary>
            /// 以上
            /// </summary>
            GreaterThanOrEqual,

            /// <summary>
            /// 以下
            /// </summary>
            LessThanOrEqual
        }

        /// <summary>
        /// 条件の種類
        /// </summary>
        public ConditionType Type { get; set; }

        /// <summary>
        /// 条件のキー（アクション名、イベント名、変数名、コンポーネント名など）
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 期待値
        /// </summary>
        public object ExpectedValue { get; set; }

        /// <summary>
        /// 比較演算子
        /// </summary>
        public ComparisonOperator Operator { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PuzzleCondition()
        {
            Operator = ComparisonOperator.Equal;
        }

        /// <summary>
        /// 条件が満たされているかチェック
        /// </summary>
        public bool IsSatisfied(GameState gameState, Infrastructure.Services.ComponentManager componentManager)
        {
            switch (Type)
            {
                case ConditionType.ActionCount:
                    var actualCount = gameState.GetActionCount(Key);
                    return CompareValues(actualCount, ExpectedValue, Operator);

                case ConditionType.EventCompleted:
                    var isCompleted = gameState.IsEventCompleted(Key);
                    return CompareValues(isCompleted, ExpectedValue, Operator);

                case ConditionType.ComponentExists:
                    bool exists;
                    // SELLCTフォルダ内のファイルパスかどうかをチェック
                    if (Key.Contains("/") || Key.Contains("\\"))
                    {
                        exists = componentManager.ComponentExistsByPath(Key);
                        System.Diagnostics.Debug.WriteLine($"[PuzzleCondition] ComponentExists check by path - Key: {Key}, Exists: {exists}");
                    }
                    else
                    {
                        exists = componentManager.GetComponent(Key) != null;
                        System.Diagnostics.Debug.WriteLine($"[PuzzleCondition] ComponentExists check by name - Key: {Key}, Exists: {exists}");
                    }
                    var result = CompareValues(exists, ExpectedValue, Operator);
                    System.Diagnostics.Debug.WriteLine($"[PuzzleCondition] ComponentExists result - Key: {Key}, Exists: {exists}, Expected: {ExpectedValue}, Operator: {Operator}, Result: {result}");
                    return result;

                case ConditionType.Variable:
                    var variableValue = gameState.GetVariable<object>(Key);
                    return CompareValues(variableValue, ExpectedValue, Operator);

                case ConditionType.Always:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 値を比較する
        /// </summary>
        private bool CompareValues(object actual, object expected, ComparisonOperator op)
        {
            if (actual == null && expected == null)
                return op == ComparisonOperator.Equal;

            if (actual == null || expected == null)
                return op == ComparisonOperator.NotEqual;

            // 数値比較
            if (actual is int actualInt && expected is int expectedInt)
            {
                return op switch
                {
                    ComparisonOperator.Equal => actualInt == expectedInt,
                    ComparisonOperator.NotEqual => actualInt != expectedInt,
                    ComparisonOperator.GreaterThan => actualInt > expectedInt,
                    ComparisonOperator.LessThan => actualInt < expectedInt,
                    ComparisonOperator.GreaterThanOrEqual => actualInt >= expectedInt,
                    ComparisonOperator.LessThanOrEqual => actualInt <= expectedInt,
                    _ => false
                };
            }

            // ブール値比較
            if (actual is bool actualBool && expected is bool expectedBool)
            {
                return op switch
                {
                    ComparisonOperator.Equal => actualBool == expectedBool,
                    ComparisonOperator.NotEqual => actualBool != expectedBool,
                    _ => false
                };
            }

            // 文字列比較
            if (actual is string actualString && expected is string expectedString)
            {
                return op switch
                {
                    ComparisonOperator.Equal => actualString == expectedString,
                    ComparisonOperator.NotEqual => actualString != expectedString,
                    _ => false
                };
            }

            // オブジェクト比較
            return op switch
            {
                ComparisonOperator.Equal => actual.Equals(expected),
                ComparisonOperator.NotEqual => !actual.Equals(expected),
                _ => false
            };
        }
    }
}