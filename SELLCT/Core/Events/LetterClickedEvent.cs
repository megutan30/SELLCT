namespace SELLCT.Core.Events
{
    /// <summary>
    /// 手紙クリックイベントクラス
    /// ゲーム内の手紙がユーザーによってクリックされた際に発生するドメインイベント
    /// UI層からの手紙クリック操作を検出して生成される
    /// 手紙システム、パズルシステム、ゲーム進行制御等の各種処理に通知される
    /// 手紙の既読状態更新、次の手紙配信、ゲーム進行制御等に使用される
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class LetterClickedEvent
    {
        /// <summary>
        /// クリックされた手紙のインデックス
        /// 手紙システムで管理される手紙の識別番号
        /// 通常は1から順番に設定される連番
        /// 手紙の既読状態管理や次の手紙配信判定に使用される
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public int LetterIndex { get; }

        /// <summary>
        /// コンストラクタ
        /// 手紙クリックイベントインスタンスを初期化
        /// クリックされた手紙のインデックス情報を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="letterIndex">クリックされた手紙の識別番号（1から開始）</param>
        public LetterClickedEvent(int letterIndex)
        {
            // クリックされた手紙のインデックスをイベントデータとして設定
            LetterIndex = letterIndex;
        }
    }
}