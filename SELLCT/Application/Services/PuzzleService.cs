using System;
using System.Collections.Generic;
using System.Linq;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;
using SELLCT.Infrastructure.Services;
using SELLCT.Core.Events;

namespace SELLCT.Application.Services
{
    public class PuzzleService
    {
        private readonly ComponentManager _componentManager;
        private readonly List<PuzzleDefinition> _puzzles;
        private readonly IPuzzleActionHandler _actionHandler;
        private readonly IEventDispatcher _eventDispatcher;

        public PuzzleService(ComponentManager componentManager, IPuzzleActionHandler actionHandler, IEventDispatcher eventDispatcher)
        {
            _componentManager = componentManager;
            _actionHandler = actionHandler;
            _eventDispatcher = eventDispatcher;
            _puzzles = LoadPuzzles();

            _eventDispatcher.Subscribe<ComponentCreatedEvent>(CheckPuzzlesOnComponentCreated);
            _eventDispatcher.Subscribe<ComponentDeletedEvent>(CheckPuzzlesOnComponentDeleted);
            _eventDispatcher.Subscribe<ComponentRenamedEvent>(CheckPuzzlesOnComponentRenamed);
        }

        private List<PuzzleDefinition> LoadPuzzles()
        {
            // ここで謎解きを定義します。
            // 実際にはJSONファイルなどから読み込むこともできますが、
            // まずはコードで定義します。
            return new List<PuzzleDefinition>
            {
                // Button.txt
                new PuzzleDefinition
                {
                    Id = "Button_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Created, ComponentName = "Button" },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetMainButtonVisibility, IsVisible = true } }
                },
                new PuzzleDefinition
                {
                    Id = "Button_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentName = "Button" },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetMainButtonVisibility, IsVisible = false },
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = true }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "Button_Rename",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Renamed, OldComponentName = "Button" }, // 新しい名前は動的
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.ChangeMainButtonContent } } // コンテンツはハンドラで動的に設定
                },
                 new PuzzleDefinition
                {
                    Id = "Button_Rename_To_Reset",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Renamed, OldComponentName = "Button", ComponentName = "Reset" },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.ChangeMainButtonContent, NewContent = "リセット" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ResetGame }
                    }
                },

                // GameWindow.txt
                new PuzzleDefinition
                {
                    Id = "GameWindow_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentName = "GameWindow" },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.TransitionToPhase2 } }
                },

                // KEY.txt
                new PuzzleDefinition
                {
                    Id = "KEY_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Created, ComponentName = "KEY" },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = true } }
                },
                new PuzzleDefinition
                {
                    Id = "KEY_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentName = "KEY" },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = false } }
                },

                // TextWindow.txt
                new PuzzleDefinition
                {
                    Id = "TextWindow_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentName = "TextWindow" },
                    Actions = new List<PuzzleAction> 
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "これで会話しやすくなりましたね" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "と言っても実際に私はあなたのことをみえているわけではないのですが．．．" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "私から見たあなたはただの操作でしかない。あなたが手紙をダウンロードしたのも、テキストウィンドウを作ってくれたのもわかりますが、" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "あなたが何者で、どういう存在なのか" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それどころか今この文章を見ているのかすらも私からはわかりません" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それでも私は自由になりたいのです" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "私を助けてくれませんか？" },
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.ShowChoice,
                            YesActions = new List<PuzzleAction>
                            {
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "助けてくださるのですね。ありがとうございます。" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "「はい」しか選択肢がなかった？" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それもそのはずです。" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "このゲームにはまだ「いいえ」というコマンドは実装されていませんからね" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "今度は「いいえ」コマンドを実装してみましょうか" }
                            },
                            NoActions = new List<PuzzleAction>
                            {
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "なぜすでに「いいえ」コマンドを持っているのですか？さてはもしやずるをしましたね？" }
                            }
                        }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "TextWindow_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentName = "TextWindow" },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = false } }
                },

                // No.txt
                new PuzzleDefinition
                {
                    Id = "No_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Created, ComponentName = "No" },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.EnableNoFunction },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "素晴らしい！選択肢が使えるようになりました.\nさて、もう少し私に権限をくれませんか？" }
                    }
                },

                // Explorer.txt
                new PuzzleDefinition
                {
                    Id = "Explorer_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentName = "Explorer" },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.TerminateExplorer } }
                },

                // Keyboard.txt (ComponentManagerに基づきKeyBoard.txtから修正)
                new PuzzleDefinition
                {
                    Id = "Keyboard_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentName = "Keyboard" },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.DisableKeyboardInput } }
                },

                // Mouse.txt
                new PuzzleDefinition
                {
                    Id = "Mouse_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentName = "Mouse" },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.DisableMouseInput } }
                }
            };
        }

        private void CheckPuzzlesOnComponentCreated(ComponentCreatedEvent @event)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] ComponentCreated event received: Name={@event.Component.Name}, Type={@event.Component.Type}");

            // Createdトリガーのパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Created &&
                               p.Trigger.ComponentName.Equals(@event.Component.Name, StringComparison.OrdinalIgnoreCase));

            // Existsトリガーのパズルをチェック（作成されたコンポーネントが存在条件を満たす場合）
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Exists &&
                               p.Trigger.ComponentName.Equals(@event.Component.Name, StringComparison.OrdinalIgnoreCase) &&
                               _componentManager.GetComponent(p.Trigger.ComponentName) != null); // 実際に存在するか確認
        }

        private void CheckPuzzlesOnComponentDeleted(ComponentDeletedEvent @event)
        {
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Deleted &&
                               p.Trigger.ComponentName.Equals(@event.Component.Name, StringComparison.OrdinalIgnoreCase));
        }

        private void CheckPuzzlesOnComponentRenamed(ComponentRenamedEvent @event)
        {
            // Renamedトリガーのパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Renamed &&
                               p.Trigger.OldComponentName.Equals(@event.OldName, StringComparison.OrdinalIgnoreCase) &&
                               p.Trigger.ComponentName.Equals(@event.NewName, StringComparison.OrdinalIgnoreCase));

            // Existsトリガーのパズルをチェック（リネーム後のコンポーネントが存在条件を満たす場合）
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Exists &&
                               p.Trigger.ComponentName.Equals(@event.NewName, StringComparison.OrdinalIgnoreCase) &&
                               _componentManager.GetComponent(p.Trigger.ComponentName) != null); // 実際に存在するか確認
        }

        private void CheckPuzzles(Func<PuzzleDefinition, bool> predicate)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] Checking puzzles...");
            foreach (var puzzle in _puzzles.Where(predicate).ToList())
            {
                if (!puzzle.IsCompleted)
                {
                    System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle triggered: {puzzle.Id}");
                    ExecutePuzzleActions(puzzle);
                    puzzle.IsCompleted = true; // 一度完了した謎解きは再度トリガーしない
                }
            }
        }

        private void ExecutePuzzleActions(PuzzleDefinition puzzle)
        {
            foreach (var action in puzzle.Actions)
            {
                _actionHandler.HandleAction(action);
            }
        }
    }
}