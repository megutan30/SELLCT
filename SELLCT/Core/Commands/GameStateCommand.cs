using System.Threading.Tasks;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Commands
{
    /// <summary>
    /// ゲーム状態を変更するコマンド
    /// </summary>
    public class GameStateCommand : BaseGameCommand
    {
        public enum StateAction
        {
            ResetGame,
            ClearMessageQueue,
            EnableNoFunction,
            TransitionToPhase2
        }

        private readonly StateAction _stateAction;

        public GameStateCommand(StateAction stateAction)
        {
            _stateAction = stateAction;
        }

        public override string Description => $"Game state: {_stateAction}";

        public override async Task ExecuteAsync(IGameContext context)
        {
            var actionType = _stateAction switch
            {
                StateAction.ResetGame => PuzzleAction.ActionType.ResetGame,
                StateAction.ClearMessageQueue => PuzzleAction.ActionType.ClearMessageQueue,
                StateAction.EnableNoFunction => PuzzleAction.ActionType.EnableNoFunction,
                StateAction.TransitionToPhase2 => PuzzleAction.ActionType.TransitionToPhase2,
                _ => throw new System.ArgumentOutOfRangeException()
            };

            var action = new PuzzleAction
            {
                Type = actionType
            };

            context.ActionHandler.HandleAction(action);
            await Task.CompletedTask;
        }
    }
}