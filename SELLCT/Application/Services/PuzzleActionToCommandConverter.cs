using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SELLCT.Core.Commands;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;

namespace SELLCT.Application.Services
{
    /// <summary>
    /// PuzzleActionをGameCommandに変換するサービス
    /// </summary>
    public class PuzzleActionToCommandConverter
    {
        /// <summary>
        /// PuzzleActionをGameCommandに変換
        /// </summary>
        /// <param name="action">変換するPuzzleAction</param>
        /// <returns>対応するGameCommand</returns>
        public IGameCommand Convert(PuzzleAction action)
        {
            return action.Type switch
            {
                PuzzleAction.ActionType.ShowDialog => 
                    new ShowDialogCommand(action.Message),
                
                PuzzleAction.ActionType.SetMainButtonVisibility => 
                    new SetVisibilityCommand(SetVisibilityCommand.VisibilityTarget.MainButton, action.IsVisible),
                
                PuzzleAction.ActionType.SetKeyVisibility => 
                    new SetVisibilityCommand(SetVisibilityCommand.VisibilityTarget.Key, action.IsVisible),
                
                PuzzleAction.ActionType.SetTextWindowVisibility => 
                    new SetVisibilityCommand(SetVisibilityCommand.VisibilityTarget.TextWindow, action.IsVisible),
                
                PuzzleAction.ActionType.SetBackgroundVisibility => 
                    new SetVisibilityCommand(SetVisibilityCommand.VisibilityTarget.Background, action.IsVisible),
                
                PuzzleAction.ActionType.ChangeMainButtonContent => 
                    new ChangeButtonContentCommand(ChangeButtonContentCommand.ButtonTarget.MainButton, action.NewContent),
                
                PuzzleAction.ActionType.ChangeNoButtonContent => 
                    new ChangeButtonContentCommand(ChangeButtonContentCommand.ButtonTarget.NoButton, action.NewContent),
                
                PuzzleAction.ActionType.StartExplorer => 
                    new ProcessControlCommand(ProcessControlCommand.ProcessAction.StartExplorer),
                
                PuzzleAction.ActionType.TerminateExplorer => 
                    new ProcessControlCommand(ProcessControlCommand.ProcessAction.TerminateExplorer),
                
                PuzzleAction.ActionType.EnableKeyboardInput => 
                    new ProcessControlCommand(ProcessControlCommand.ProcessAction.EnableKeyboardInput),
                
                PuzzleAction.ActionType.DisableKeyboardInput => 
                    new ProcessControlCommand(ProcessControlCommand.ProcessAction.DisableKeyboardInput),
                
                PuzzleAction.ActionType.EnableMouseInput => 
                    new ProcessControlCommand(ProcessControlCommand.ProcessAction.EnableMouseInput),
                
                PuzzleAction.ActionType.DisableMouseInput => 
                    new ProcessControlCommand(ProcessControlCommand.ProcessAction.DisableMouseInput),
                
                PuzzleAction.ActionType.ResetGame => 
                    new GameStateCommand(GameStateCommand.StateAction.ResetGame),
                
                PuzzleAction.ActionType.ClearMessageQueue => 
                    new GameStateCommand(GameStateCommand.StateAction.ClearMessageQueue),
                
                PuzzleAction.ActionType.EnableNoFunction => 
                    new GameStateCommand(GameStateCommand.StateAction.EnableNoFunction),
                
                PuzzleAction.ActionType.TransitionToPhase2 => 
                    new GameStateCommand(GameStateCommand.StateAction.TransitionToPhase2),
                
                PuzzleAction.ActionType.ExitApplication => 
                    new ExitApplicationCommand(),
                
                PuzzleAction.ActionType.ExitWithMessageBox => 
                    new ExitApplicationCommand(action.Message, 0, true),
                
                PuzzleAction.ActionType.DelayedExitWithMessageBox => 
                    new ExitApplicationCommand(action.Message, action.DelayMilliseconds, true),
                
                // 未対応のアクションタイプ（従来のPuzzleActionHandlerにフォールバック）
                _ => new LegacyPuzzleActionCommand(action)
            };
        }

        /// <summary>
        /// 複数のPuzzleActionを一つのCompositeCommandに変換
        /// </summary>
        /// <param name="actions">変換するPuzzleAction群</param>
        /// <param name="description">コマンドの説明</param>
        /// <returns>CompositeCommand</returns>
        public IGameCommand Convert(IEnumerable<PuzzleAction> actions, string description = "Composite command")
        {
            var commands = actions.Select(Convert).ToArray();
            return new CompositeCommand(description, commands);
        }
    }

    /// <summary>
    /// 未対応のPuzzleActionを従来の方法で実行するためのコマンド
    /// </summary>
    internal class LegacyPuzzleActionCommand : BaseGameCommand
    {
        private readonly PuzzleAction _action;

        public LegacyPuzzleActionCommand(PuzzleAction action)
        {
            _action = action;
        }

        public override string Description => $"Legacy action: {_action.Type}";

        public override async Task ExecuteAsync(IGameContext context)
        {
            // 従来のPuzzleActionHandlerに委譲
            context.ActionHandler.HandleAction(_action);
            await Task.CompletedTask;
        }
    }
}