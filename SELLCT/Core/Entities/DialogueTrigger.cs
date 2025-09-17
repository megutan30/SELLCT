namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 対話フロートリガークラス
    /// 対話フローを開始するためのファイルシステムイベントトリガーを定義
    /// ファイル作成、削除、名前変更、存在確認等のイベントに対応
    /// ComponentManagerからのイベント通知を受けて対話システムを起動する
    /// </summary>
    public class DialogueTrigger
    {
        /// <summary>
        /// トリガーイベントの種類
        /// このトリガーが反応するファイルシステムイベントのタイプ
        /// Created、Deleted、Exists、Renamedのいずれかを指定
        /// 各タイプに応じて異なる条件判定ロジックが適用される
        /// </summary>
        public TriggerType Type { get; set; }
        
        /// <summary>
        /// 対象コンポーネント名配列（複数パターン対応）
        /// トリガーが反応する複数のファイル名パターンを定義
        /// 大文字・小文字の違いや類似名への対応に使用
        /// 例: ["Button", "button", "BUTTON"]で大文字小文字を問わず反応
        /// ComponentNameより優先して使用される
        /// </summary>
        public string[] ComponentNames { get; set; }
        
        /// <summary>
        /// 変更前コンポーネント名（Renamedタイプ専用）
        /// ファイル名前変更イベントで使用する変更前の名前
        /// Type=Renamedの場合にのみ有効
        /// ComponentNameと組み合わせて「旧名前 → 新名前」の変更を検出
        /// </summary>
        public string OldComponentName { get; set; }
        
        /// <summary>
        /// 対象コンポーネント名（単一パターン）
        /// トリガーが反応する単一のファイル名（拡張子なし）
        /// ComponentNamesが設定されている場合は無視される
        /// 例: "Button", "TextWindow", "Mouse"
        /// Renamedタイプの場合は変更後の新しい名前として使用
        /// </summary>
        public string ComponentName { get; set; }
        
        /// <summary>
        /// トリガーイベント種類列挙型
        /// 対話フローを開始するファイルシステムイベントの種類を定義
        /// ComponentManagerによって検出・配信されるイベントに対応
        /// </summary>
        public enum TriggerType
        {
            /// <summary>
            /// ファイル作成イベント
            /// ユーザーが新しいファイルをcomponentsフォルダに作成した時
            /// 例: Button.txt、TextWindow.txt等の新規作成
            /// </summary>
            Created,
            
            /// <summary>
            /// ファイル削除イベント
            /// ユーザーがファイルをcomponentsフォルダから削除した時
            /// 例: Button.txtの削除、Mouse.txtの削除等
            /// </summary>
            Deleted,
            
            /// <summary>
            /// ファイル存在確認イベント
            /// 指定されたファイルが存在している間、継続的に実行される
            /// CanRepeat=trueの対話フローと組み合わせて状態維持に使用
            /// </summary>
            Exists,
            
            /// <summary>
            /// ファイル名前変更イベント
            /// ユーザーがファイル名を変更した時
            /// OldComponentNameとComponentNameを使用して変更前後を判定
            /// </summary>
            Renamed
        }
        
        /// <summary>
        /// コンポーネント名マッチング判定
        /// 指定されたコンポーネント名がこのトリガーの対象かどうかをチェック
        /// ComponentNamesとComponentNameの両方を考慮して判定を行う
        /// 大文字・小文字を無視した比較を実行する
        /// </summary>
        /// <param name="componentName">チェック対象のコンポーネント名</param>
        /// <returns>true: トリガー対象で対話フロー実行, false: トリガー対象外で無視</returns>
        public bool MatchesComponentName(string componentName)
        {
            // 複数名前パターンが設定されている場合は優先的に使用
            if (ComponentNames != null)
            {
                // 配列内のいずれかの名前と一致するかをチェック
                foreach (var name in ComponentNames)
                {
                    // 大文字・小文字を無視した文字列比較
                    if (string.Equals(name, componentName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                // 配列内のどの名前とも一致しない場合
                return false;
            }
            
            // 単一名前パターンの場合の比較（大文字・小文字無視）
            return string.Equals(ComponentName, componentName, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}