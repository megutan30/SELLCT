using SELLCT.Core.Entities;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// コンポーネント削除イベントクラス
    /// ファイルシステムでコンポーネントファイルが削除された際に発生するドメインイベント
    /// ComponentManagerによってファイル削除が検出された時点で生成される
    /// パズルシステム、対話システム、UI更新等の各種処理に通知される
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class ComponentDeletedEvent
    {
        /// <summary>
        /// 削除されたコンポーネント
        /// 削除されたGameComponentインスタンス
        /// 削除前のファイル情報、タイプ、位置等の詳細データを保持
        /// イベントハンドラーがこのプロパティを通じて削除されたコンポーネント情報にアクセス
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public GameComponent Component { get; }

        /// <summary>
        /// コンストラクタ
        /// コンポーネント削除イベントインスタンスを初期化
        /// 削除されたコンポーネント情報を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="component">削除されたGameComponentインスタンス</param>
        public ComponentDeletedEvent(GameComponent component)
        {
            // 削除されたコンポーネントをイベントデータとして設定
            Component = component;
        }
    }
}