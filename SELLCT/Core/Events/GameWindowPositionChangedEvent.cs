using System.Windows;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// ゲームウィンドウの位置が変更されたときに発火されるイベント
    /// </summary>
    public class GameWindowPositionChangedEvent
    {
        /// <summary>
        /// 新しいウィンドウ位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="position">新しいウィンドウ位置</param>
        public GameWindowPositionChangedEvent(Point position)
        {
            Position = position;
        }

        public override string ToString()
        {
            return $"GameWindowPositionChanged: ({Position.X:F0}, {Position.Y:F0})";
        }
    }
}