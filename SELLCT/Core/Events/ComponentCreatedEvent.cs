using SELLCT.Core.Entities;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// コンポーネント作成イベントクラス
    /// ファイルシステムでコンポーネントファイルが作成された際に発生するドメインイベント
    /// ComponentManagerによってファイル作成が検出された時点で生成される
    /// パズルシステム、対話システム、UI更新等の各種処理に通知される
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class ComponentCreatedEvent
    {
        /// <summary>
        /// 作成されたコンポーネント
        /// 新しく作成されたGameComponentインスタンス
        /// ファイル情報、タイプ、位置等の詳細データを含む
        /// イベントハンドラーがこのプロパティを通じてコンポーネント情報にアクセス
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public GameComponent Component { get; }

        /// <summary>
        /// コンストラクタ
        /// コンポーネント作成イベントインスタンスを初期化
        /// 作成されたコンポーネント情報を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="component">作成されたGameComponentインスタンス</param>
        public ComponentCreatedEvent(GameComponent component)
        {
            // 作成されたコンポーネントをイベントデータとして設定
            Component = component;
        }
    }
}