using System.Threading.Tasks;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Commands
{
    /// <summary>
    /// プロセス制御（起動/終了）を行うコマンド
    /// </summary>
    public class ProcessControlCommand : BaseGameCommand
    {
        public enum ProcessAction
        {
            StartExplorer,
            TerminateExplorer,
            EnableKeyboardInput,
            DisableKeyboardInput,
            EnableMouseInput,
            DisableMouseInput
        }

        private readonly ProcessAction _processAction;

        public ProcessControlCommand(ProcessAction processAction)
        {
            _processAction = processAction;
        }

        public override string Description => $"Process control: {_processAction}";

        public override async Task ExecuteAsync(IGameContext context)
        {
            var actionType = _processAction switch
            {
                ProcessAction.StartExplorer => PuzzleAction.ActionType.StartExplorer,
                ProcessAction.TerminateExplorer => PuzzleAction.ActionType.TerminateExplorer,
                ProcessAction.EnableKeyboardInput => PuzzleAction.ActionType.EnableKeyboardInput,
                ProcessAction.DisableKeyboardInput => PuzzleAction.ActionType.DisableKeyboardInput,
                ProcessAction.EnableMouseInput => PuzzleAction.ActionType.EnableMouseInput,
                ProcessAction.DisableMouseInput => PuzzleAction.ActionType.DisableMouseInput,
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