namespace SELLCT.Core.Events
{
    /// <summary>
    /// 隠しアイテム公開イベントクラス
    /// ゲーム内の隠されたアイテムやフォルダがユーザーに公開された際に発生するドメインイベント
    /// パズルアクションやゲーム進行により隠しコンテンツが解放された時点で生成される
    /// UI更新、ゲーム進行制御、統計情報更新等の各種処理に通知される
    /// 隠しフォルダの表示、新しいコンポーネントの解放、特別なゲーム要素の開放等に使用される
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class HiddenItemRevealedEvent
    {
        /// <summary>
        /// 公開されたアイテムの内部名
        /// システム内部で使用されるアイテムの識別名
        /// ファイル名やフォルダ名等の実際の名前
        /// 例: "Authority", "SecretDocument.txt", "HiddenFolder"
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public string ItemName { get; }
        
        /// <summary>
        /// 公開されたアイテムの表示名
        /// ユーザーインターフェースに表示される名前
        /// 内部名と異なる場合もあり、より分かりやすい表現を使用
        /// 例: "権限フォルダ", "秘密文書", "隠されたフォルダ"
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public string DisplayName { get; }
        
        /// <summary>
        /// 公開されたアイテムが格納されているフォルダ
        /// アイテムの親フォルダのパスまたは名前
        /// アイテムの場所特定やファイルシステム操作で使用
        /// 例: "components", "Authority", "SELLCT"
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public string Folder { get; }

        /// <summary>
        /// コンストラクタ
        /// 隠しアイテム公開イベントインスタンスを初期化
        /// 公開されたアイテムの詳細情報を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="itemName">公開されたアイテムの内部識別名</param>
        /// <param name="displayName">ユーザーに表示されるアイテム名</param>
        /// <param name="folder">アイテムが格納されているフォルダ名</param>
        public HiddenItemRevealedEvent(string itemName, string displayName, string folder)
        {
            // 公開されたアイテムの内部名をイベントデータとして設定
            ItemName = itemName;
            // 公開されたアイテムの表示名をイベントデータとして設定
            DisplayName = displayName;
            // 公開されたアイテムのフォルダをイベントデータとして設定
            Folder = folder;
        }
    }
}