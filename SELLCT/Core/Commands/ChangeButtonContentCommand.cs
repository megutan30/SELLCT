using System.Threading.Tasks;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Commands
{
    /// <summary>
    /// ボタンコンテンツを変更するコマンド
    /// </summary>
    public class ChangeButtonContentCommand : BaseGameCommand
    {
        public enum ButtonTarget
        {
            MainButton,
            NoButton
        }

        private readonly ButtonTarget _target;
        private readonly string _newContent;

        public ChangeButtonContentCommand(ButtonTarget target, string newContent)
        {
            _target = target;
            _newContent = newContent;
        }

        public override string Description => $"Change {_target} content to: {_newContent}";

        public override async Task ExecuteAsync(IGameContext context)
        {
            var actionType = _target switch
            {
                ButtonTarget.MainButton => PuzzleAction.ActionType.ChangeMainButtonContent,
                ButtonTarget.NoButton => PuzzleAction.ActionType.ChangeNoButtonContent,
                _ => throw new System.ArgumentOutOfRangeException()
            };

            var action = new PuzzleAction
            {
                Type = actionType,
                NewContent = _newContent
            };

            context.ActionHandler.HandleAction(action);
            await Task.CompletedTask;
        }

        public override bool CanExecute(IGameContext context)
        {
            return !string.IsNullOrEmpty(_newContent);
        }
    }
}