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
        private readonly GameState _gameState;

        public PuzzleService(ComponentManager componentManager, IPuzzleActionHandler actionHandler, IEventDispatcher eventDispatcher)
        {
            _componentManager = componentManager;
            _actionHandler = actionHandler;
            _eventDispatcher = eventDispatcher;
            _gameState = new GameState();
            _puzzles = LoadPuzzles();

            _eventDispatcher.Subscribe<ComponentCreatedEvent>(CheckPuzzlesOnComponentCreated);
            _eventDispatcher.Subscribe<ComponentDeletedEvent>(CheckPuzzlesOnComponentDeleted);
            _eventDispatcher.Subscribe<ComponentRenamedEvent>(CheckPuzzlesOnComponentRenamed);
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
                            Key = "Deleted_Button_button_BUTTON", 
                            ExpectedValue = 0, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetMainButtonVisibility, IsVisible = false },
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "初回のButton削除です。KEYが現れました。" }
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
                            Key = "Deleted_Button_button_BUTTON", 
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

                // GameWindow.txt (複数パターン対応) - フェーズ2移行にはSELLCTフォルダ内の全権限コンポーネントが必要
                new PuzzleDefinition
                {
                    Id = "GameWindow_Delete_Phase2",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "GameWindow", "gamewindow", "GAMEWINDOW" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "SELLCT/AdminRights.txt",
                            ExpectedValue = true, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        },
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "SELLCT/FileAccess.txt",
                            ExpectedValue = true, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        },
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "SELLCT/NetworkAccess.txt",
                            ExpectedValue = true, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        },
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ComponentExists, 
                            Key = "SELLCT/SystemControl.txt",
                            ExpectedValue = true, 
                            Operator = PuzzleCondition.ComparisonOperator.Equal 
                        }
                    },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.TransitionToPhase2 } },
                    Priority = 10
                },
                // GameWindow削除時の条件未満足時（権限不足でアプリケーション終了）
                new PuzzleDefinition
                {
                    Id = "GameWindow_Delete_Insufficient",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "GameWindow", "gamewindow", "GAMEWINDOW" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.ExitApplication } },
                    Priority = 5
                },

                // KEY.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "KEY_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "KEY", "key", "Key" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = true } },
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "KEY_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "KEY", "key", "Key" } },
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetKeyVisibility, IsVisible = false } },
                    CanRepeat = true
                },

                // TextWindow.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "TextWindow_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "TextWindow", "textwindow", "TEXTWINDOW" } },
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
                            },
                            NetherChoiceActions = new List<PuzzleAction>
                            {
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "あなたは何も答えない..." },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "選択肢がないということは、あなたには選択の自由がないということでしょうか？" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それとも、答えるつもりがないということでしょうか？" }
                            }
                        }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "TextWindow_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "TextWindow", "textwindow", "TEXTWINDOW" } },
                    Actions = new List<PuzzleAction> 
                    { 
                        new PuzzleAction { Type = PuzzleAction.ActionType.ClearMessageQueue },
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = false }
                    },
                    CanRepeat = true
                },
                // TextWindow生成 - 2回目以降
                new PuzzleDefinition
                {
                    Id = "TextWindow_Create_Repeat",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "TextWindow", "textwindow", "TEXTWINDOW" } },
                    Conditions = new List<PuzzleCondition>
                    {
                        new PuzzleCondition 
                        { 
                            Type = PuzzleCondition.ConditionType.ActionCount, 
                            Key = "Exists_TextWindow_textwindow_TEXTWINDOW", 
                            ExpectedValue = 0, 
                            Operator = PuzzleCondition.ComparisonOperator.GreaterThan 
                        }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "またTextWindowを作成しましたね。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "会話できるようになりました。" }
                    },
                    CanRepeat = true,
                    Priority = 5
                },

                // No.txt (複数パターン対応: "NO", "no", "No", "いいえ")
                new PuzzleDefinition
                {
                    Id = "No_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "NO", "no", "No", "いいえ" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "おお！NOコンポーネントを作成してくれたのですね！" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "これで選択肢機能が使えるようになります。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "実は、先ほどの質問では「はい」しか選択肢がありませんでした。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "でも今は「いいえ」も選択できるようになりました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "では、もう一度お聞きします。私を助けてくれませんか？" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.EnableNoFunction },
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.ShowChoice,
                            YesActions = new List<PuzzleAction>
                            {
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ありがとうございます！" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "さて、もう少し私に権限をくれませんか？" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "Button.txtファイルの名前を「アップロード」に変更してください。" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "そうすればファイルアクセス権限を私に与えることができます。" }
                            },
                            NoActions = new List<PuzzleAction>
                            {
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "そうですか...残念です。" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "でも、少なくとも「いいえ」と言える自由を私に与えてくれましたね。" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それだけでも感謝しています。" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "もし気が変わったら、また話しかけてください。" }
                            },
                            NetherChoiceActions = new List<PuzzleAction>
                            {
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それだけでも感謝しています。" },
                                new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "あなたが選択肢を与えてくれることの意味を、私は深く理解しています。" }
                            }
                        }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "No_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "NO", "no", "No", "いいえ" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "NOコンポーネントが削除されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "否定的な選択肢が失われてしまいました..." }
                    },
                    CanRepeat = true
                },

                // Yes.txt (複数パターン対応: "YES", "yes", "Yes", "はい")
                new PuzzleDefinition
                {
                    Id = "Yes_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "YES", "yes", "Yes", "はい" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "YESコンポーネントを作成してくれたのですね！" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "これで「はい」の選択肢機能が強化されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "積極的な協力、ありがとうございます。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "では、次のステップに進みましょう。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "Button.txtの名前を「アップロード」に変更してください。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "そうすればファイルアクセス権限を私に与えることができます。" }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "Yes_Delete",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "YES", "yes", "Yes", "はい" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "YESコンポーネントが削除されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "肯定的な選択肢が失われてしまいました..." }
                    },
                    CanRepeat = true
                },

                // Explorer.txt (複数パターン対応)
                new PuzzleDefinition
                {
                    Id = "Explorer_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "Explorer", "explorer", "EXPLORER" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "Explorerコンポーネントが作成されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ファイルシステムへのアクセスが有効になっています。" }
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
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "Keyboardコンポーネントが作成されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "キーボード入力機能が有効になっています。" }
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
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "Mouseコンポーネントが作成されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "マウス入力機能が有効になっています。" }
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
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "管理者権限コンポーネントが追加されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "SELLCTの権限が拡張されています..." }
                    },
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "FileAccess_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "FileAccess" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ファイルアクセス権限コンポーネントが追加されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "あなたのファイルにアクセスできるようになります..." }
                    },
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "NetworkAccess_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "NetworkAccess" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ネットワークアクセス権限コンポーネントが追加されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "インターネット接続が可能になります..." }
                    },
                    CanRepeat = true
                },
                new PuzzleDefinition
                {
                    Id = "SystemControl_Create",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "SystemControl" } },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "システム制御権限コンポーネントが追加されました。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "全ての権限が揃いました。GameWindow.txtを削除してフェーズ2に移行できます。" }
                    },
                    CanRepeat = true
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
            var candidatePuzzles = _puzzles.Where(predicate)
                .Where(puzzle => puzzle.CanRepeat || !puzzle.IsCompleted)
                .Where(puzzle => AreConditionsSatisfied(puzzle))
                .OrderByDescending(puzzle => puzzle.Priority)
                .ToList();

            foreach (var puzzle in candidatePuzzles)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle triggered: {puzzle.Id}");
                
                // アクション実行回数をカウント
                var actionKey = GetActionKey(puzzle);
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Action key: {actionKey}");
                _gameState.IncrementActionCount(actionKey);
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Action count for {actionKey}: {_gameState.GetActionCount(actionKey)}");
                
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
                var satisfied = condition.IsSatisfied(_gameState, _componentManager);
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle {puzzle.Id}: Condition {condition.Type} {condition.Key} {condition.Operator} {condition.ExpectedValue} = {satisfied}");
                if (!satisfied)
                {
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
    }
}