namespace SELLCT.Core.Entities
{
    /// <summary>
    /// パズル実行条件を定義するクラス
    /// パズルがトリガーされても、この条件が満たされなければ実行されない
    /// ゲーム状態、アクション回数、コンポーネント存在等の複雑な条件判定を可能にする
    /// 複数の条件をすべて満たした場合のみパズルが実行される（AND条件）
    /// </summary>
    public class PuzzleCondition
    {
        /// <summary>
        /// 条件の種類
        /// パズル実行の判定に使用する情報源の種類を定義
        /// 各種類に応じて異なる判定ロジックが適用される
        /// </summary>
        public enum ConditionType
        {
            /// <summary>
            /// アクション実行回数による条件
            /// 特定のアクション（ファイル作成、削除等）が指定回数実行されたかを判定
            /// 例: 「Buttonを3回以上削除した場合」「初回のTextWindow作成時のみ」
            /// </summary>
            ActionCount,

            /// <summary>
            /// イベント完了状態による条件
            /// 特定のワンタイムイベントが完了済みかどうかを判定
            /// 例: 「Phase2がまだ開始されていない場合」「チュートリアル完了後」
            /// </summary>
            EventCompleted,

            /// <summary>
            /// コンポーネント存在による条件
            /// 指定されたコンポーネント（ファイル）が存在するかどうかを判定
            /// パス指定とファイル名指定の両方に対応
            /// 例: 「TextWindowが存在する場合」「YES/NOファイルの存在確認」
            /// </summary>
            ComponentExists,

            /// <summary>
            /// カスタム変数による条件
            /// GameStateに保存されたカスタム変数の値を判定
            /// ヒント表示状態、ユーザーアクションフラグ等で使用
            /// 例: 「ヒントがまだ表示されていない場合」「特定の設定が有効な場合」
            /// </summary>
            Variable,

            /// <summary>
            /// 常に実行
            /// 条件なしで常に実行される（デバッグ・テスト用）
            /// 他の条件と組み合わせる場合は注意が必要
            /// </summary>
            Always
        }

        /// <summary>
        /// 比較演算子
        /// 実際の値と期待値を比較するための演算子を定義
        /// 数値、ブール値、文字列の比較に対応
        /// </summary>
        public enum ComparisonOperator
        {
            /// <summary>
            /// 等しい（==）
            /// 実際の値が期待値と完全に一致する場合
            /// 例: アクション回数が正確に3回、変数がtrueの場合
            /// </summary>
            Equal,

            /// <summary>
            /// 等しくない（!=）
            /// 実際の値が期待値と異なる場合
            /// 例: イベントが未完了、ファイルが存在しない場合
            /// </summary>
            NotEqual,

            /// <summary>
            /// より大きい（>）
            /// 実際の数値が期待値より大きい場合
            /// 数値比較でのみ使用可能
            /// 例: アクション回数が5回より多い場合
            /// </summary>
            GreaterThan,

            /// <summary>
            /// より小さい（<）
            /// 実際の数値が期待値より小さい場合
            /// 数値比較でのみ使用可能
            /// 例: アクション回数が3回未満の場合
            /// </summary>
            LessThan,

            /// <summary>
            /// 以上（>=）
            /// 実際の数値が期待値以上の場合
            /// 数値比較でのみ使用可能
            /// 例: アクション回数が3回以上の場合
            /// </summary>
            GreaterThanOrEqual,

            /// <summary>
            /// 以下（<=）
            /// 実際の数値が期待値以下の場合
            /// 数値比較でのみ使用可能
            /// 例: アクション回数が5回以下の場合
            /// </summary>
            LessThanOrEqual
        }

        /// <summary>
        /// 条件の種類
        /// この条件で判定する情報の種類を指定
        /// ActionCount、EventCompleted、ComponentExists、Variable、Alwaysのいずれか
        /// </summary>
        public ConditionType Type { get; set; }

        /// <summary>
        /// 条件のキー（アクション名、イベント名、変数名、コンポーネント名など）
        /// 条件の種類に応じて異なる意味を持つ：
        /// - ActionCount: "Created_Button"等のアクションキー
        /// - EventCompleted: イベント識別子
        /// - ComponentExists: コンポーネント名またはファイルパス
        /// - Variable: GameStateのカスタム変数名
        /// - Always: 無視される
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 期待値
        /// 比較対象となる値。条件の種類に応じて異なる型：
        /// - ActionCount: int（実行回数）
        /// - EventCompleted: bool（完了状態）
        /// - ComponentExists: bool（存在状態）
        /// - Variable: object（任意の型）
        /// - Always: 無視される
        /// </summary>
        public object ExpectedValue { get; set; }

        /// <summary>
        /// 比較演算子
        /// 実際の値と期待値を比較する方法を指定
        /// デフォルトはEqual（等しい）
        /// </summary>
        public ComparisonOperator Operator { get; set; }

        /// <summary>
        /// コンストラクタ
        /// 安全なデフォルト値で条件インスタンスを初期化
        /// 比較演算子はEqualに設定される
        /// </summary>
        public PuzzleCondition()
        {
            // 最も一般的な比較演算子をデフォルトに設定
            Operator = ComparisonOperator.Equal;
        }

        /// <summary>
        /// 条件が満たされているかチェック
        /// 現在のゲーム状態と条件設定を比較して、パズル実行可能かどうかを判定
        /// 各条件タイプに応じた適切な判定ロジックを実行
        /// </summary>
        /// <param name="gameState">現在のゲーム状態（アクション回数、イベント完了状態、変数等）</param>
        /// <param name="componentManager">コンポーネント管理サービス（ファイル存在確認用）</param>
        /// <returns>true: 条件満足でパズル実行可能, false: 条件未満足でパズル実行不可</returns>
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
        /// 実際の値と期待値を指定された比較演算子で比較
        /// 型安全性を保ちつつ、数値・ブール値・文字列・オブジェクトの比較に対応
        /// null値の適切な処理も含む
        /// </summary>
        /// <param name="actual">実際の値（ゲーム状態から取得）</param>
        /// <param name="expected">期待値（条件設定で指定）</param>
        /// <param name="op">比較演算子</param>
        /// <returns>true: 比較条件満足, false: 比較条件不満足</returns>
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