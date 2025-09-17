using System.Windows;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// ゲームウィンドウ位置変更イベントクラス
    /// ゲームウィンドウの位置が変更された際に発生するドメインイベント
    /// ウィンドウドラッグやプログラムによる位置変更が検出された時点で生成される
    /// UI同期、位置記録、ゲーム状態管理等の各種処理に通知される
    /// ウィンドウ位置の追跡やレイアウト管理で使用される
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class GameWindowPositionChangedEvent
    {
        /// <summary>
        /// 変更後のウィンドウ位置
        /// 画面上の新しいウィンドウ位置座標
        /// X、Y座標はピクセル単位で表現される
        /// 左上角を原点とする画面座標系での位置
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// コンストラクタ
        /// ゲームウィンドウ位置変更イベントインスタンスを初期化
        /// 変更後のウィンドウ位置情報を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="position">変更後のウィンドウ位置（画面座標系でのPoint）</param>
        public GameWindowPositionChangedEvent(Point position)
        {
            // 変更後のウィンドウ位置をイベントデータとして設定
            Position = position;
        }

        /// <summary>
        /// 文字列表現を取得
        /// イベントの内容を人間が読める形式で出力
        /// デバッグやログ出力で使用される
        /// ウィンドウ位置座標を整数形式で表示
        /// </summary>
        /// <returns>イベント内容を説明する文字列</returns>
        public override string ToString()
        {
            return $"GameWindowPositionChanged: ({Position.X:F0}, {Position.Y:F0})";
        }
    }
}