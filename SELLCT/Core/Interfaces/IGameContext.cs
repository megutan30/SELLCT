using SELLCT.Core.Entities;
using SELLCT.Infrastructure.Services;

namespace SELLCT.Core.Interfaces
{
    /// <summary>
    /// ゲームコマンド実行時に必要なコンテキスト情報を提供
    /// </summary>
    public interface IGameContext
    {
        /// <summary>
        /// ゲーム状態
        /// </summary>
        GameState GameState { get; }
        
        /// <summary>
        /// コンポーネント管理者
        /// </summary>
        ComponentManager ComponentManager { get; }
        
        /// <summary>
        /// アクションハンドラー（UI操作用）
        /// </summary>
        IPuzzleActionHandler ActionHandler { get; }
        
        /// <summary>
        /// イベントディスパッチャー
        /// </summary>
        IEventDispatcher EventDispatcher { get; }
        
        /// <summary>
        /// メタゲーム制御（フェーズ2用）
        /// </summary>
        MetaGameController MetaGameController { get; }
    }
}