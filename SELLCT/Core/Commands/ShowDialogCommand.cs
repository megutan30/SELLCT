using System.Threading.Tasks;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Commands
{
    /// <summary>
    /// ダイアログメッセージを表示するコマンド
    /// </summary>
    public class ShowDialogCommand : BaseGameCommand
    {
        private readonly string _message;

        public ShowDialogCommand(string message)
        {
            _message = message;
        }

        public override string Description => $"Show dialog: {_message}";

        public override async Task ExecuteAsync(IGameContext context)
        {
            // TextWindowコンポーネントが存在する場合のみダイアログを表示
            if (context.ComponentManager.HasTextWindowComponent())
            {
                var action = new PuzzleAction
                {
                    Type = PuzzleAction.ActionType.ShowDialog,
                    Message = _message
                };
                context.ActionHandler.HandleAction(action);
            }
            
            await Task.CompletedTask;
        }

        public override bool CanExecute(IGameContext context)
        {
            return !string.IsNullOrEmpty(_message) && context.ComponentManager.HasTextWindowComponent();
        }
    }
}