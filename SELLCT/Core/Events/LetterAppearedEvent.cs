namespace SELLCT.Core.Events
{
    /// <summary>
    /// 手紙出現イベントクラス
    /// 自動手紙送信システムによって新しい手紙が画面に表示された際に発生するドメインイベント
    /// 手紙配信システムからの手紙表示処理完了時に生成される
    /// UI更新、ゲーム進行制御、統計情報更新等の各種処理に通知される
    /// 手紙の配信履歴管理や次の手紙配信スケジュール調整に使用される
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class LetterAppearedEvent
    {
        /// <summary>
        /// 出現した手紙のインデックス
        /// 手紙システムで管理される手紙の識別番号
        /// 通常は1から順番に設定される連番
        /// 手紙の配信履歴管理や次の配信スケジュール判定に使用される
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public int LetterIndex { get; }

        /// <summary>
        /// コンストラクタ
        /// 手紙出現イベントインスタンスを初期化
        /// 出現した手紙のインデックス情報を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="letterIndex">出現した手紙の識別番号（1から開始）</param>
        public LetterAppearedEvent(int letterIndex)
        {
            // 出現した手紙のインデックスをイベントデータとして設定
            LetterIndex = letterIndex;
        }
    }
}