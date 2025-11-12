using System;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// Mouseコンポーネント削除イベントクラス
    /// Mouseコンポーネントが削除された際に発生する特別なドメインイベント
    /// ComponentManagerによってMouse.txtファイルの削除が検出された時点で生成される
    /// マウス入力制御、UI更新、ゲーム進行制御等の各種処理に通知される
    /// 社会工学デモンストレーションの重要な段階を示すイベント
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class MouseComponentDeletionEvent
    {
        /// <summary>
        /// コンストラクタ
        /// Mouseコンポーネント削除イベントインスタンスを初期化
        /// パラメータなしのシンプルなイベントとして設計
        /// Mouse削除の事実のみを通知し、詳細情報は不要
        /// </summary>
        public MouseComponentDeletionEvent()
        {
            // Mouseコンポーネント削除の事実のみを通知するシンプルなイベント
            // 必要に応じて将来的に削除時刻や削除理由等のデータを追加可能
        }
    }
}