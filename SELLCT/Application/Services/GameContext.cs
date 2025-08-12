using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;
using SELLCT.Infrastructure.Services;

namespace SELLCT.Application.Services
{
    /// <summary>
    /// ゲームコマンド実行コンテキストの実装
    /// </summary>
    public class GameContext : IGameContext
    {
        public GameState GameState { get; }
        public ComponentManager ComponentManager { get; }
        public IPuzzleActionHandler ActionHandler { get; }
        public IEventDispatcher EventDispatcher { get; }
        public MetaGameController MetaGameController { get; }

        public GameContext(
            GameState gameState,
            ComponentManager componentManager,
            IPuzzleActionHandler actionHandler,
            IEventDispatcher eventDispatcher,
            MetaGameController metaGameController)
        {
            GameState = gameState;
            ComponentManager = componentManager;
            ActionHandler = actionHandler;
            EventDispatcher = eventDispatcher;
            MetaGameController = metaGameController;
        }
    }
}