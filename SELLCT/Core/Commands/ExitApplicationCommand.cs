using System.Threading.Tasks;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Commands
{
    /// <summary>
    /// アプリケーション終了コマンド
    /// </summary>
    public class ExitApplicationCommand : BaseGameCommand
    {
        private readonly string _message;
        private readonly int _delayMilliseconds;
        private readonly bool _showMessageBox;

        public ExitApplicationCommand(string message = null, int delayMilliseconds = 0, bool showMessageBox = false)
        {
            _message = message;
            _delayMilliseconds = delayMilliseconds;
            _showMessageBox = showMessageBox;
        }

        public override string Description => $"Exit application with message: {_message}, delay: {_delayMilliseconds}ms";

        public override async Task ExecuteAsync(IGameContext context)
        {
            PuzzleAction.ActionType actionType;
            
            if (_showMessageBox && !string.IsNullOrEmpty(_message))
            {
                if (_delayMilliseconds > 0)
                {
                    actionType = PuzzleAction.ActionType.DelayedExitWithMessageBox;
                }
                else
                {
                    actionType = PuzzleAction.ActionType.ExitWithMessageBox;
                }
            }
            else
            {
                actionType = PuzzleAction.ActionType.ExitApplication;
            }

            var action = new PuzzleAction
            {
                Type = actionType,
                Message = _message,
                DelayMilliseconds = _delayMilliseconds
            };

            context.ActionHandler.HandleAction(action);
            await Task.CompletedTask;
        }
    }
}