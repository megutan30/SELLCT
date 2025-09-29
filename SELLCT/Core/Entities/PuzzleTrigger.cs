using System;
using System.Linq;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// パズルトリガークラス
    /// パズルが実行される条件となるファイルシステムイベントを定義
    /// ユーザーのファイル操作（作成、削除、名前変更等）をゲームイベントに変換
    /// ComponentManagerからのイベント通知に反応してパズルシステムを起動
    /// </summary>
    public class PuzzleTrigger
    {
        /// <summary>
        /// トリガータイプ列挙型
        /// パズルを実行するファイルシステムイベントの種類を定義
        /// 各イベントはComponentManagerによって検出・配信される
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
            /// ファイル名前変更イベント
            /// ユーザーがファイル名を変更した時
            /// OldComponentNameとComponentNameを使用して変更前後を判定
            /// </summary>
            Renamed,
            
            /// <summary>
            /// ファイル存在確認イベント
            /// 指定されたファイルが存在している間、継続的に実行される
            /// CanRepeat=trueのパズルと組み合わせて状態維持に使用
            /// </summary>
            Exists,
            
            /// <summary>
            /// Authorityフォルダ開封イベント
            /// 隠されたAuthorityフォルダが発見・開封された時の特別イベント
            /// ゲーム進行の重要な節目で使用
            /// </summary>
            AuthorityFolderOpened,

            /// <summary>
            /// コンポーネント位置変更イベント
            /// UIコンポーネント（Background等）の位置が変更された時
            /// Canvas座標での位置変更を検出してパズル条件を評価
            /// MinX、MaxX、MinY、MaxYプロパティと組み合わせて条件判定
            /// </summary>
            PositionChanged
        }

        /// <summary>
        /// トリガータイプ
        /// このトリガーが反応するファイルシステムイベントの種類
        /// Created、Deleted、Renamed、Exists、AuthorityFolderOpenedのいずれか
        /// </summary>
        public TriggerType Type { get; set; }
        
        /// <summary>
        /// 対象コンポーネント名（単一）
        /// トリガーが反応する単一のファイル名（拡張子なし）
        /// ComponentNamesが設定されている場合は無視される
        /// 例: "Button", "TextWindow", "Mouse"
        /// </summary>
        public string ComponentName { get; set; }
        
        /// <summary>
        /// 対象コンポーネント名配列（複数パターン対応）
        /// トリガーが反応する複数のファイル名パターン
        /// 大文字・小文字の違いや類似名に対応するため
        /// 例: ["Button", "button", "BUTTON"]
        /// </summary>
        public string[] ComponentNames { get; set; }
        
        /// <summary>
        /// 変更前コンポーネント名（Renamedタイプ専用）
        /// ファイル名前変更イベントで使用する変更前の名前
        /// Type=Renamedの場合のみ有効
        /// ComponentNameと組み合わせて"OldName → NewName"の変更を検出
        /// </summary>
        public string OldComponentName { get; set; }

        /// <summary>
        /// X座標の最小値（PositionChangedタイプ専用）
        /// コンポーネントのX座標がこの値以上の場合にトリガー発動
        /// nullの場合は下限なし
        /// Type=PositionChangedの場合に使用
        /// </summary>
        public double? MinX { get; set; }

        /// <summary>
        /// X座標の最大値（PositionChangedタイプ専用）
        /// コンポーネントのX座標がこの値以下の場合にトリガー発動
        /// nullの場合は上限なし
        /// Type=PositionChangedの場合に使用
        /// </summary>
        public double? MaxX { get; set; }

        /// <summary>
        /// Y座標の最小値（PositionChangedタイプ専用）
        /// コンポーネントのY座標がこの値以上の場合にトリガー発動
        /// nullの場合は下限なし
        /// Type=PositionChangedの場合に使用
        /// </summary>
        public double? MinY { get; set; }

        /// <summary>
        /// Y座標の最大値（PositionChangedタイプ専用）
        /// コンポーネントのY座標がこの値以下の場合にトリガー発動
        /// nullの場合は上限なし
        /// Type=PositionChangedの場合に使用
        /// </summary>
        public double? MaxY { get; set; }

        /// <summary>
        /// 指定された名前がこのトリガーの対象かチェック
        /// ComponentNamesとComponentNameの両方を考慮して判定
        /// 大文字・小文字を無視した比較を実行
        /// </summary>
        /// <param name="name">チェック対象のコンポーネント名</param>
        /// <returns>true: トリガー対象, false: トリガー対象外</returns>
        public bool MatchesComponentName(string name)
        {
            // 複数名前パターンが設定されている場合は優先的に使用
            if (ComponentNames != null && ComponentNames.Length > 0)
            {
                // 配列内のいずれかの名前と一致するかチェック（大文字・小文字無視）
                return ComponentNames.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            
            // 単一名前パターンの場合の比較（大文字・小文字無視）
            return ComponentName != null && ComponentName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 指定された位置がこのトリガーの位置条件を満たすかチェック
        /// PositionChangedタイプのトリガーでのみ使用
        /// MinX、MaxX、MinY、MaxYの各条件を評価して総合判定
        /// </summary>
        /// <param name="x">チェック対象のX座標</param>
        /// <param name="y">チェック対象のY座標</param>
        /// <returns>true: 位置条件を満たす, false: 位置条件を満たさない</returns>
        public bool MatchesPositionCondition(double x, double y)
        {
            // X座標の最小値チェック
            if (MinX.HasValue && x < MinX.Value)
                return false;

            // X座標の最大値チェック
            if (MaxX.HasValue && x > MaxX.Value)
                return false;

            // Y座標の最小値チェック
            if (MinY.HasValue && y < MinY.Value)
                return false;

            // Y座標の最大値チェック
            if (MaxY.HasValue && y > MaxY.Value)
                return false;

            // すべての条件を満たす場合はtrue
            return true;
        }
    }
}