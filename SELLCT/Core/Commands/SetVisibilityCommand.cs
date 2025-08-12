using System.Threading.Tasks;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Commands
{
    /// <summary>
    /// UI要素の可視性を設定するコマンド
    /// </summary>
    public class SetVisibilityCommand : BaseGameCommand
    {
        public enum VisibilityTarget
        {
            MainButton,
            Key,
            TextWindow,
            Background
        }

        private readonly VisibilityTarget _target;
        private readonly bool _isVisible;

        public SetVisibilityCommand(VisibilityTarget target, bool isVisible)
        {
            _target = target;
            _isVisible = isVisible;
        }

        public override string Description => $"Set {_target} visibility to {_isVisible}";

        public override async Task ExecuteAsync(IGameContext context)
        {
            var actionType = _target switch
            {
                VisibilityTarget.MainButton => PuzzleAction.ActionType.SetMainButtonVisibility,
                VisibilityTarget.Key => PuzzleAction.ActionType.SetKeyVisibility,
                VisibilityTarget.TextWindow => PuzzleAction.ActionType.SetTextWindowVisibility,
                VisibilityTarget.Background => PuzzleAction.ActionType.SetBackgroundVisibility,
                _ => throw new System.ArgumentOutOfRangeException()
            };

            var action = new PuzzleAction
            {
                Type = actionType,
                IsVisible = _isVisible
            };

            context.ActionHandler.HandleAction(action);
            await Task.CompletedTask;
        }
    }
}