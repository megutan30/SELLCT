using SELLCT.Core.Entities;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// コンポーネント名前変更イベントクラス
    /// ファイルシステムでコンポーネントファイルの名前が変更された際に発生するドメインイベント
    /// ComponentManagerによってファイル名前変更が検出された時点で生成される
    /// パズルシステム、対話システム、UI更新等の各種処理に通知される
    /// 変更前後の名前情報を含み、名前変更に基づくトリガー条件判定に使用される
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class ComponentRenamedEvent
    {
        /// <summary>
        /// 変更前の名前
        /// ファイル名前変更前の元の名前（拡張子なし）
        /// 名前変更トリガー条件の判定で使用される
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public string OldName { get; }
        
        /// <summary>
        /// 変更後の名前
        /// ファイル名前変更後の新しい名前（拡張子なし）
        /// 名前変更トリガー条件の判定で使用される
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public string NewName { get; }
        
        /// <summary>
        /// 名前変更されたコンポーネント
        /// 新しい名前で更新されたGameComponentインスタンス
        /// 名前変更後のファイル情報、タイプ、位置等の詳細データを含む
        /// イベントハンドラーがこのプロパティを通じてコンポーネント情報にアクセス
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public GameComponent Component { get; }

        /// <summary>
        /// コンストラクタ
        /// コンポーネント名前変更イベントインスタンスを初期化
        /// 変更前後の名前と更新されたコンポーネント情報を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="oldName">変更前のファイル名（拡張子なし）</param>
        /// <param name="newName">変更後のファイル名（拡張子なし）</param>
        /// <param name="component">名前変更後のGameComponentインスタンス</param>
        public ComponentRenamedEvent(string oldName, string newName, GameComponent component)
        {
            // 変更前の名前をイベントデータとして設定
            OldName = oldName;
            // 変更後の名前をイベントデータとして設定
            NewName = newName;
            // 名前変更されたコンポーネントをイベントデータとして設定
            Component = component;
        }
    }
}