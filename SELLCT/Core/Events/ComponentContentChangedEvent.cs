using SELLCT.Core.Entities;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// コンポーネント内容変更イベントクラス
    /// ファイルシステムでコンポーネントファイルの内容が変更された際に発生するドメインイベント
    /// FileSystemWatcherManagerによってファイル変更が検出された時点で生成される
    /// 作成イベントとは区別され、既存コンポーネントの更新のみを通知する
    /// パズルシステム、対話システム、UI更新等の各種処理に通知される
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class ComponentContentChangedEvent
    {
        /// <summary>
        /// 内容が変更されたコンポーネント
        /// 変更後のGameComponentインスタンス
        /// 更新されたファイル情報、内容、位置等の詳細データを含む
        /// イベントハンドラーがこのプロパティを通じてコンポーネント情報にアクセス
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public GameComponent Component { get; }

        /// <summary>
        /// コンストラクタ
        /// コンポーネント内容変更イベントインスタンスを初期化
        /// 変更されたコンポーネント情報を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="component">内容が変更されたGameComponentインスタンス</param>
        public ComponentContentChangedEvent(GameComponent component)
        {
            // 変更されたコンポーネントをイベントデータとして設定
            Component = component;
        }
    }
}
