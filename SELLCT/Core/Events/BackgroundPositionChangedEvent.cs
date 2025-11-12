using System.Windows;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// 背景位置変更イベントクラス
    /// Backgroundコンポーネントの位置が変更された際に発生するドメインイベント
    /// UI要素の位置変更をファイルシステムイベントと同様にパズルシステムで処理するために使用
    /// SetBackgroundPositionメソッドからの位置変更を検出してパズル条件評価を実行
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class BackgroundPositionChangedEvent
    {
        /// <summary>
        /// 変更後の背景位置
        /// Canvas上での新しい背景位置座標
        /// X、Y座標はピクセル単位で表現される
        /// Canvasの左上角を原点とする座標系での位置
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 対象コンポーネント名
        /// 位置変更されたコンポーネントの名前
        /// パズルトリガーの条件評価で使用される
        /// 通常は "Background" が設定される
        /// </summary>
        public string ComponentName { get; }

        /// <summary>
        /// コンストラクタ
        /// 背景位置変更イベントインスタンスを初期化
        /// 変更後の位置情報とコンポーネント名を受け取り、イベントデータとして設定
        /// </summary>
        /// <param name="position">変更後の背景位置（Canvas座標系でのPoint）</param>
        /// <param name="componentName">対象コンポーネント名（通常は "Background"）</param>
        public BackgroundPositionChangedEvent(Point position, string componentName)
        {
            Position = position;
            ComponentName = componentName;
        }

        /// <summary>
        /// 文字列表現を取得
        /// イベントの内容を人間が読める形式で出力
        /// デバッグやログ出力で使用される
        /// 位置座標とコンポーネント名を整数形式で表示
        /// </summary>
        /// <returns>イベント内容を説明する文字列</returns>
        public override string ToString()
        {
            return $"BackgroundPositionChanged: {ComponentName} -> ({Position.X:F0}, {Position.Y:F0})";
        }
    }
}