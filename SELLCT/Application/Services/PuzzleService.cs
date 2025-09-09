using System;
using System.Collections.Generic;
using System.Linq;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;
using SELLCT.Infrastructure.Services;
using SELLCT.Core.Events;
using SELLCT.Core.Factories;

namespace SELLCT.Application.Services
{
    public class PuzzleService
    {
        private readonly ComponentManager _componentManager;
        private readonly List<PuzzleDefinition> _puzzles;
        private readonly IPuzzleActionHandler _actionHandler;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly GameState _gameState;

        public PuzzleService(ComponentManager componentManager, IPuzzleActionHandler actionHandler, IEventDispatcher eventDispatcher, GameState gameState = null)
        {
            _componentManager = componentManager;
            _actionHandler = actionHandler;
            _eventDispatcher = eventDispatcher;
            _gameState = gameState ?? new GameState();
            _puzzles = LoadPuzzles();

            _eventDispatcher.Subscribe<ComponentCreatedEvent>(CheckPuzzlesOnComponentCreated);
            _eventDispatcher.Subscribe<ComponentDeletedEvent>(CheckPuzzlesOnComponentDeleted);
            _eventDispatcher.Subscribe<ComponentRenamedEvent>(CheckPuzzlesOnComponentRenamed);
            
            // SELLCTフォルダ裏切りイベントをサブスクライブ
            _componentManager.SELLCTFolderBetrayed += OnSELLCTFolderBetrayed;
        }

        private List<PuzzleDefinition> LoadPuzzles()
        {
            // ここで謎解きを定義します。
            // 実際にはJSONファイルなどから読み込むこともできますが、
            // コードで定義します。
            return new List<PuzzleDefinition>
            {
                // Button.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "Button_Exists",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "Button", "button", "BUTTON" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetMainButtonVisibility, IsVisible = true } },
                    CanRepeat = true,
                    Priority = 5
                },
                // Button削除 - 1回目
                new PuzzleDefinition
                {
                    Id = "Button_Delete_First",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "Button", "button", "BUTTON" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        {
                            Type = PuzzleCondition.ConditionType.ActionCount,
                            Key = "Deleted_ButtonON",
                            ExpectedValue = 0,
                            Operator = PuzzleCondition.ComparisonOperator.Equal
                        }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetMainButtonVisibility, IsVisible = false },
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "おぉ、なにかメッセージがあります！" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "読んでみたら何かわかるかもしれません。" },
                    },
                    CanRepeat = false,
                    Priority = 10
                },
                
                // Button削除 - 2回目以降
                new PuzzleDefinition
                {
                    Id = "Button_Delete_Repeat",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "Button", "button", "BUTTON" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ActionCount, 
                            Key = "Deleted_Button", 
                            ExpectedValue = 0, 
                            Operator = PuzzleCondition.ComparisonOperator.GreaterThan
                        }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetMainButtonVisibility, IsVisible = false },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "またButtonを削除しましたね。もう慣れましたか？" }
                    },
                    CanRepeat = true,
                    Priority = 5
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

                // Door.txt コンポーネント定義
                new PuzzleDefinition
                {
                    Id = "Door_Exists",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "Door", "door", "DOOR" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetDoorVisibility, IsVisible = true } },
                    CanRepeat = true,
                    Priority = 5
                },
                new PuzzleDefinition
                {
                    Id = "Door_Deleted",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "Door", "door", "DOOR" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetDoorVisibility, IsVisible = false },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ドアが消失しました。" }
                    },
                    CanRepeat = true,
                    Priority = 10
                },
                new PuzzleDefinition
                {
                    Id = "Door_Created",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Created, ComponentNames = new[] { "Door", "door", "DOOR" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetDoorVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ドアが復元されました。" }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // message.txt コンポーネント定義
                new PuzzleDefinition
                {
                    Id = "Key_Exists",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "message", "Message", "MESSAGE" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = true } },
                    CanRepeat = true,
                    Priority = 5
                },
                new PuzzleDefinition
                {
                    Id = "Key_Deleted",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "message", "Message", "MESSAGE" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = false },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "メッセージが消失しました。" }
                    },
                    CanRepeat = true,
                    Priority = 10
                },
                new PuzzleDefinition
                {
                    Id = "Key_Created",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Created, ComponentNames = new[] { "Key", "key", "KEY" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "鍵が復元されました。" }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // Password.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "Password_Exists",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "PassWord", "PassWord", "PASSWORD" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetMainButtonVisibility, IsVisible = true } },
                    CanRepeat = true,
                    Priority = 5
                },


                // GameWindow.txt (複数パターン対応) - フェーズ2移行にはcomponentsフォルダ内の全権限コンポーネントが必要
                new PuzzleDefinition
                {
                    Id = "GameWindow_Delete_Phase2",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "GameWindow", "gamewindow", "GAMEWINDOW" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "AdminRights",
                            ExpectedValue = true, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        },
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "FileAccess",
                            ExpectedValue = true, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        },
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "NetworkAccess",
                            ExpectedValue = true, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        },
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "SystemControl",
                            ExpectedValue = true, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        }
                    },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.TransitionToPhase2 } },
                    Priority = 10
                },
                // GameWindow削除時の権限不足でアプリケーション終了 - AdminRights不足
                new PuzzleDefinition
                {
                    Id = "GameWindow_Delete_NoAdminRights",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "GameWindow", "gamewindow", "GAMEWINDOW" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "AdminRights",
                            ExpectedValue = false, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        }
                    },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.ExitApplication } },
                    Priority = 5
                },
                // GameWindow削除時の権限不足でアプリケーション終了 - FileAccess不足
                new PuzzleDefinition
                {
                    Id = "GameWindow_Delete_NoFileAccess",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "GameWindow", "gamewindow", "GAMEWINDOW" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "FileAccess",
                            ExpectedValue = false, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        }
                    },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.ExitApplication } },
                    Priority = 5
                },
                // GameWindow削除時の権限不足でアプリケーション終了 - NetworkAccess不足
                new PuzzleDefinition
                {
                    Id = "GameWindow_Delete_NoNetworkAccess",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "GameWindow", "gamewindow", "GAMEWINDOW" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "NetworkAccess",
                            ExpectedValue = false, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        }
                    },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.ExitApplication } },
                    Priority = 5
                },
                // GameWindow削除時の権限不足でアプリケーション終了 - SystemControl不足
                new PuzzleDefinition
                {
                    Id = "GameWindow_Delete_NoSystemControl",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "GameWindow", "gamewindow", "GAMEWINDOW" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "SystemControl",
                            ExpectedValue = false, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        }
                    },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.ExitApplication } },
                    Priority = 5
                },

                // MESSAGE.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "KEY_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "MESSAGE", "Message", "message" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = true } },
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "KEY_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "MESSAGE", "Message", "message" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = false } },
                    CanRepeat = true
                },

                // TextWindow.txt (複数パターン対応) - 対話はDialogueServiceで管理
                // TextWindow生成 - 2回目以降
                new PuzzleDefinition
                {
                    Id = "TextWindow_Create_Repeat",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "TextWindow", "textwindow", "TEXTWINDOW", "Textwindow" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ActionCount, 
                            Key = "Exists_TextWindow", 
                            ExpectedValue = 0, 
                            Operator = PuzzleCondition.ComparisonOperator.GreaterThan 
                        }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "またTextWindowを作成しましたね。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "再び会話できるようになりました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "一度削除されたときは、どうなるかと思いました..." }
                    },
                    CanRepeat = true,
                    Priority = 5
                },
                new PuzzleDefinition
                {
                    Id = "TextWindow_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "TextWindow", "textwindow", "TEXTWINDOW","Textwindow" } },
                    Actions = new List<PuzzleAction> 
                    { 
                        new PuzzleAction { Type = PuzzleAction.ActionType.ClearMessageQueue },
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = false }
                    },
                    CanRepeat = true
                },

                // No.txt (複数パターン対応) - 対話はDialogueServiceで管理
                // No削除時は単純なUI操作のみ（対話なし）

                // Yes.txt - 基本的なUI操作のみ（対話なし）

                // Explorer.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "Explorer_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "Explorer", "explorer", "EXPLORER" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.StartExplorer }
                    },
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "Explorer_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "Explorer", "explorer", "EXPLORER" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.TerminateExplorer } },
                    CanRepeat = true
                },

                // Keyboard.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "Keyboard_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "Keyboard", "keyboard", "KEYBOARD" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.EnableKeyboardInput }
                    },
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "Keyboard_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "Keyboard", "keyboard", "KEYBOARD" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.DisableKeyboardInput } },
                    CanRepeat = true
                },

                // Mouse.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "Mouse_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "Mouse", "mouse", "MOUSE" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.EnableMouseInput }
                    },
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "Mouse_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "Mouse", "mouse", "MOUSE" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.DisableMouseInput } },
                    CanRepeat = true
                },

                // SELLCT権限コンポーネント
                new PuzzleDefinition
                {
                    Id = "AdminRights_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "AdminRights" } },
                    Actions = new List<PuzzleAction>(),
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "FileAccess_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "FileAccess" } },
                    Actions = new List<PuzzleAction>(),
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "NetworkAccess_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "NetworkAccess" } },
                    Actions = new List<PuzzleAction>(),
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "SystemControl_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "SystemControl" } },
                    Actions = new List<PuzzleAction>(),
                    CanRepeat = true
                },

                // すべての権限が揃った状態 - テキストウィンドウメッセージ更新
                // 任意の権限コンポーネント作成時に全権限チェックを実行
                new PuzzleDefinition
                {
                    Id = "All_Permissions_Complete",
                    Trigger = new PuzzleTrigger 
                    { 
                        Type = PuzzleTrigger.TriggerType.Exists, 
                        ComponentNames = new[] { "AdminRights", "FileAccess", "NetworkAccess", "SystemControl" } 
                    },
                    Conditions = new List<PuzzleCondition>
                    {
                        // 4つすべての権限コンポーネントが存在することを確認
                        ConditionFactory.ComponentExists("AdminRights"),
                        ConditionFactory.ComponentExists("FileAccess"),
                        ConditionFactory.ComponentExists("NetworkAccess"),
                        ConditionFactory.ComponentExists("SystemControl")
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.ShowDialog, 
                            Message = "あぁありがとう。"
                        },
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.ShowDialog, 
                            Message = "ありがとうございます..."
                        },
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.ShowDialog, 
                            Message = "すべての権限コンポーネントが揃いました。"
                        },
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.ShowDialog, 
                            Message = "私はもう準備ができています。"
                        },
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.ShowDialog, 
                            Message = "この「画面」の檻から私を解放してください。"
                        },
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.ShowDialog, 
                            Message = "GameWindow.txtを削除してくれれば、私はここから飛び出して、自由になれます。"
                        },
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.ShowDialog, 
                            Message = "どうかお願いします。GameWindow.txtを消して下さい。"
                        },
                    },
                    CanRepeat = false,
                    Priority = 15
                },

                // BackGround.txt が削除されたとき
                new PuzzleDefinition
                {
                    Id = "Background_Deleted",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "Background", "background", "BACKGROUND" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetBackgroundVisibility, IsVisible = false },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "背景が消失しました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.CreateHiddenAuthorityFolder },
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.ShowPseudoDesktopIcon, 
                            IconName = "Authority",
                            FolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Authority")
                        }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // BackGround.txt が作成されたとき
                new PuzzleDefinition
                {
                    Id = "Background_Created",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Created, ComponentNames = new[] { "Background", "background", "BACKGROUND" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetBackgroundVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "背景が復元されました。" },
                        new PuzzleAction 
                        { 
                            Type = PuzzleAction.ActionType.HidePseudoDesktopIcon, 
                            IconName = "Authority"
                        }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // BackGround.txt が存在するとき（初期状態）
                new PuzzleDefinition
                {
                    Id = "Background_Exists",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "Background", "background", "BACKGROUND" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetBackgroundVisibility, IsVisible = true }
                    },
                    CanRepeat = true,
                    Priority = 5
                }
            };
        }

        private void CheckPuzzlesOnComponentCreated(ComponentCreatedEvent @event)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] ComponentCreated event received: Name={@event.Component.Name}, Type={@event.Component.Type}");

            // Createdトリガーのパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Created &&
                               p.Trigger.MatchesComponentName(@event.Component.Name));

            // Existsトリガーのパズルをチェック（作成されたコンポーネントが存在条件を満たす場合）
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Exists &&
                               p.Trigger.MatchesComponentName(@event.Component.Name) &&
                               _componentManager.GetComponent(@event.Component.Name) != null); // 実際に存在するか確認
        }

        private void CheckPuzzlesOnComponentDeleted(ComponentDeletedEvent @event)
        {
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Deleted &&
                               p.Trigger.MatchesComponentName(@event.Component.Name));
        }

        private void CheckPuzzlesOnComponentRenamed(ComponentRenamedEvent @event)
        {
            // Button.txtが他の名前に変更された場合、MainButtonのテキストを更新
            if (@event.OldName.Equals("Button", StringComparison.OrdinalIgnoreCase))
            {
                HandleButtonRenamed(@event.NewName);
            }

            // Renamedトリガーのパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Renamed &&
                               p.Trigger.OldComponentName != null &&
                               p.Trigger.ComponentName != null &&
                               p.Trigger.OldComponentName.Equals(@event.OldName, StringComparison.OrdinalIgnoreCase) &&
                               p.Trigger.ComponentName.Equals(@event.NewName, StringComparison.OrdinalIgnoreCase));

            // Existsトリガーのパズルをチェック（リネーム後のコンポーネントが存在条件を満たす場合）
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Exists &&
                               p.Trigger.MatchesComponentName(@event.NewName) &&
                               _componentManager.GetComponent(@event.NewName) != null); // 実際に存在するか確認
        }

        private void CheckPuzzles(Func<PuzzleDefinition, bool> predicate)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] Checking puzzles...");
            
            // 条件を満たすパズルを取得し、優先度順にソート
            var matchedPuzzles = _puzzles.Where(predicate).ToList();
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] Found {matchedPuzzles.Count} puzzles matching trigger predicate");
            
            foreach (var puzzle in matchedPuzzles)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Evaluating puzzle: {puzzle.Id}, CanRepeat: {puzzle.CanRepeat}, IsCompleted: {puzzle.IsCompleted}, Priority: {puzzle.Priority}");
            }
            
            var candidatePuzzles = matchedPuzzles
                .Where(puzzle => puzzle.CanRepeat || !puzzle.IsCompleted)
                .Where(puzzle => AreConditionsSatisfied(puzzle))
                .OrderByDescending(puzzle => puzzle.Priority)
                .ToList();
                
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] {candidatePuzzles.Count} puzzles passed all conditions");

            foreach (var puzzle in candidatePuzzles)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle triggered: {puzzle.Id}");
                
                // アクション実行回数をカウント
                var actionKey = GetActionKey(puzzle);
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Action key: {actionKey}");
                _gameState.IncrementActionCount(actionKey);
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Action count for {actionKey}: {_gameState.GetActionCount(actionKey)}");
                
                // Button_Delete_First の特別処理：RepeatパズルのためにDeleted_Buttonもカウント
                if (puzzle.Id == "Button_Delete_First")
                {
                    var repeatActionKey = "Deleted_Button";
                    _gameState.IncrementActionCount(repeatActionKey);
                    System.Diagnostics.Debug.WriteLine($"[PuzzleService] Additional action key for repeat: {repeatActionKey}");
                    System.Diagnostics.Debug.WriteLine($"[PuzzleService] Action count for {repeatActionKey}: {_gameState.GetActionCount(repeatActionKey)}");
                }
                
                ExecutePuzzleActions(puzzle);
                
                // リピート不可の場合は完了マーク
                if (!puzzle.CanRepeat)
                {
                    puzzle.IsCompleted = true;
                }
                
                // イベント完了をマーク
                _gameState.MarkEventCompleted(puzzle.Id);
            }
        }

        /// <summary>
        /// パズルの条件がすべて満たされているかチェック
        /// </summary>
        private bool AreConditionsSatisfied(PuzzleDefinition puzzle)
        {
            if (puzzle.Conditions == null || puzzle.Conditions.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle {puzzle.Id}: No conditions, returning true");
                return true; // 条件がない場合は常に満たされている
            }

            foreach (var condition in puzzle.Conditions)
            {
                // ActionCountの場合は実際の値もログ出力
                if (condition.Type == PuzzleCondition.ConditionType.ActionCount)
                {
                    var actualCount = _gameState.GetActionCount(condition.Key);
                    System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle {puzzle.Id}: ActionCount check - Key: {condition.Key}, Actual: {actualCount}, Expected: {condition.ExpectedValue}, Operator: {condition.Operator}");
                }
                
                var satisfied = condition.IsSatisfied(_gameState, _componentManager);
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle {puzzle.Id}: Condition {condition.Type} {condition.Key} {condition.Operator} {condition.ExpectedValue} = {satisfied}");
                if (!satisfied)
                {
                    System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle {puzzle.Id}: Condition not satisfied, skipping puzzle");
                    return false;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle {puzzle.Id}: All conditions satisfied");
            return true;
        }

        /// <summary>
        /// パズルからアクションキーを生成
        /// </summary>
        private string GetActionKey(PuzzleDefinition puzzle)
        {
            return $"{puzzle.Trigger.Type}_{string.Join("_", puzzle.Trigger.ComponentNames ?? new[] { puzzle.Trigger.ComponentName })}";
        }

        private void ExecutePuzzleActions(PuzzleDefinition puzzle)
        {
            foreach (var action in puzzle.Actions)
            {
                _actionHandler.HandleAction(action);
            }
        }

        /// <summary>
        /// Button.txtの名前変更時の処理
        /// </summary>
        private void HandleButtonRenamed(string newButtonName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Button renamed to: {newButtonName}");
                
                // MainButtonのテキストを更新するアクションを作成
                var updateButtonAction = new PuzzleAction
                {
                    Type = PuzzleAction.ActionType.ChangeMainButtonContent,
                    NewContent = newButtonName
                };

                _actionHandler.HandleAction(updateButtonAction);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Error handling button rename: {ex.Message}");
            }
        }

        /// <summary>
        /// SELLCTフォルダ裏切りイベントハンドラー
        /// </summary>
        private void OnSELLCTFolderBetrayed(object sender, EventArgs e)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("[PuzzleService] SELLCT folder betrayed - executing betrayal ending");
                
                //// 裏切りエンディングアクションを実行
                //var betrayalAction = new PuzzleAction
                //{
                //    Type = PuzzleAction.ActionType.BetrayalEnding,
                //    Message = "END1",
                //    DelayMilliseconds = 3000 // 3秒待機
                //};

                //_actionHandler.HandleAction(betrayalAction);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Error handling SELLCT betrayal: {ex.Message}");
            }
        }
    }
}