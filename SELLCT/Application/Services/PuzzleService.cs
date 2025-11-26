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
    /// <summary>
    /// パズルサービス実装クラス
    /// ゲームの中核となるパズルシステムの実装
    /// Clean ArchitectureのApplication層に配置されたドメインサービス
    /// ファイルシステムイベントに基づく自動パズル解決システムを提供
    /// 社会工学教育ゲームのメインゲームロジックを実行
    /// </summary>
    public class PuzzleService
    {
        /// <summary>
        /// コンポーネント管理サービス
        /// ファイルシステム監視とコンポーネントライフサイクル管理を担当
        /// パズル条件判定でファイル存在確認等に使用
        /// </summary>
        private readonly ComponentManager _componentManager;
        
        /// <summary>
        /// パズル定義リスト
        /// ゲームで使用される全パズルの定義を格納
        /// Factoryパターンで初期化時に構築される
        /// </summary>
        private readonly List<PuzzleDefinition> _puzzles;
        
        /// <summary>
        /// パズルアクションハンドラー
        /// パズル解決時のアクション実行を担当
        /// Clean Architectureの依存性逆転原則に従った抽象化
        /// </summary>
        private readonly IPuzzleActionHandler _actionHandler;
        
        /// <summary>
        /// イベントディスパッチャー
        /// ドメインイベントの購読と発行を管理
        /// パズルシステムとゲーム全体の連携を実現
        /// </summary>
        private readonly IEventDispatcher _eventDispatcher;
        
        /// <summary>
        /// ゲーム状態管理オブジェクト
        /// アクション履歴、変数、イベント完了状態等を管理
        /// パズル条件判定の基盤となる状態ストア
        /// </summary>
        private readonly GameState _gameState;

        /// <summary>
        /// コンストラクタ
        /// パズルサービスを初期化し、必要なイベントハンドラーを登録
        /// ゲーム開始と同時にファイルシステムの監視を開始し、パズル定義を読み込む
        /// </summary>
        /// <param name="componentManager">コンポーネント管理サービス</param>
        /// <param name="actionHandler">パズルアクション実行ハンドラー</param>
        /// <param name="eventDispatcher">イベント配信システム</param>
        /// <param name="gameState">ゲーム状態（nullの場合は新規作成）</param>
        public PuzzleService(ComponentManager componentManager, IPuzzleActionHandler actionHandler, IEventDispatcher eventDispatcher, GameState gameState = null)
        {
            // 依存関係を設定
            _componentManager = componentManager;
            _actionHandler = actionHandler;
            _eventDispatcher = eventDispatcher;
            _gameState = gameState ?? new GameState();  // gameStateがnullの場合は新規作成
            
            // パズル定義を読み込み
            _puzzles = LoadPuzzles();

            // ファイルシステムイベントに対するパズルチェック処理を登録
            _eventDispatcher.Subscribe<ComponentCreatedEvent>(CheckPuzzlesOnComponentCreated);     // ファイル作成時
            _eventDispatcher.Subscribe<ComponentContentChangedEvent>(CheckPuzzlesOnComponentContentChanged); // ファイル内容変更時
            _eventDispatcher.Subscribe<ComponentDeletedEvent>(CheckPuzzlesOnComponentDeleted);     // ファイル削除時
            _eventDispatcher.Subscribe<ComponentRenamedEvent>(CheckPuzzlesOnComponentRenamed);     // ファイル名変更時
            _eventDispatcher.Subscribe<AuthorityFolderOpenedEvent>(CheckPuzzlesOnAuthorityFolderOpened); // Authorityフォルダ開封時
            _eventDispatcher.Subscribe<BackgroundPositionChangedEvent>(CheckPuzzlesOnBackgroundPositionChanged); // 背景位置変更時
            
            // 特別なイベント：SELLCTフォルダの裏切り行為（プレイヤーがファイルを削除した場合）
            _componentManager.SELLCTFolderBetrayed += OnSELLCTFolderBetrayed;
        }

        /// <summary>
        /// パズル定義を読み込む
        /// ゲーム内で使用される全てのパズルとその条件、アクションを定義
        /// ファイルシステムのイベントに対応するゲームロジックを設定
        /// 将来的にはJSONファイル等の外部設定から読み込み可能
        /// </summary>
        /// <returns>定義済みパズルのリスト</returns>
        private List<PuzzleDefinition> LoadPuzzles()
        {
            // 全パズル定義をリストで返す
            // 各パズルは特定のトリガー（ファイル操作）に対してアクション（ゲーム効果）を実行
            return new List<PuzzleDefinition>
            {
                // 【Buttonコンポーネント存在時パズル】
                // Buttonファイルが存在する間、メインボタンを表示状態に保つ
                new PuzzleDefinition
                {
                    Id = "Button_Exists",                                                                                 // パズル識別子
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Exists, ComponentNames = new[] { "Button", "button", "BUTTON" } }, // トリガー：Button関連ファイルの存在
                    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.SetMainButtonVisibility, IsVisible = true } }, // アクション：メインボタン表示
                    CanRepeat = true,                                                                                     // 繰り返し実行可能
                    Priority = 5                                                                                          // 優先度（低）
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


                // GameWindow.txt 削除時のフェーズ2移行は無効化（ウィンドウ×ボタンで移行）
                // 以下のパズルはコメントアウト - ウィンドウクローズ時に権限チェックして移行
                //new PuzzleDefinition
                //{
                //    Id = "GameWindow_Delete_Phase2",
                //    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.Deleted, ComponentNames = new[] { "GameWindow", "gamewindow", "GAMEWINDOW" } },
                //    Conditions = new List<PuzzleCondition>
                //    {
                //        new PuzzleCondition
                //        {
                //            Type = PuzzleCondition.ConditionType.ComponentExists,
                //            Key = "AdminRights",
                //            ExpectedValue = true,
                //            Operator = PuzzleCondition.ComparisonOperator.Equal
                //        },
                //        new PuzzleCondition
                //        {
                //            Type = PuzzleCondition.ConditionType.ComponentExists,
                //            Key = "FileAccess",
                //            ExpectedValue = true,
                //            Operator = PuzzleCondition.ComparisonOperator.Equal
                //        },
                //        new PuzzleCondition
                //        {
                //            Type = PuzzleCondition.ConditionType.ComponentExists,
                //            Key = "NetworkAccess",
                //            ExpectedValue = true,
                //            Operator = PuzzleCondition.ComparisonOperator.Equal
                //        },
                //        new PuzzleCondition
                //        {
                //            Type = PuzzleCondition.ConditionType.ComponentExists,
                //            Key = "SystemControl",
                //            ExpectedValue = true,
                //            Operator = PuzzleCondition.ComparisonOperator.Equal
                //        }
                //    },
                //    Actions = new List<PuzzleAction> { new PuzzleAction { Type = PuzzleAction.ActionType.TransitionToPhase2 } },
                //    Priority = 10
                //},
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
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message ="背景が消えて、デスクトップ上にAuthorityフォルダが見えています！！" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "......" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "どうやらゲーム画面の後ろにあって開けないようですね…" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ゲーム画面自体をButtonのようにどうにか動かせないでしょうか..." },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "私が調べる限り、coponentsフォルダの中にGameWindow.txtがあるようです..." },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それを動かすことができるかもしれません" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "GameWindow.txtはcomponentsフォルダのどこかにあります。見えていないのならもしかしたら隠されているのかもしれません。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.CreateHiddenAuthorityFolder },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowPseudoDesktopIcon, IconName = "Authority" },
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.StartAuthorityHints,
                            DelayMilliseconds = 120000 // 2分後に最初のヒントを表示
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
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // Background位置変更でAuthority表示（X >= 290）
                new PuzzleDefinition
                {
                    Id = "Background_Position_Right_Authority",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.PositionChanged,
                        ComponentNames = new[] { "Background", "background", "BACKGROUND" },
                        MinX = 290
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "背景が消えて、デスクトップ上にAuthorityフォルダが見えています！！" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "......" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "どうやらゲーム画面の後ろにあって開けないようですね…" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ゲーム画面自体をButtonのようにどうにか動かせないでしょうか..." },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "私が調べる限り、coponentsフォルダの中にGameWindow.txtがあるようです..." },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それを動かすことができるかもしれません" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "GameWindow.txtはcomponentsフォルダのどこかにあります。見えていないのならもしかしたら隠されているのかもしれません。" }
                    },
                    CanRepeat = false, // 一度だけ実行
                    Priority = 15
                },

                // Background位置変更でAuthority表示（X <= -490）
                new PuzzleDefinition
                {
                    Id = "Background_Position_Left_Authority",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.PositionChanged,
                        ComponentNames = new[] { "Background", "background", "BACKGROUND" },
                        MaxX = -490
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "背景が消えて、デスクトップ上にAuthorityフォルダが見えています！！" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "......" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "どうやらゲーム画面の後ろにあって開けないようですね…" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "ゲーム画面自体をButtonのようにどうにか動かせないでしょうか..." },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "私が調べる限り、coponentsフォルダの中にGameWindow.txtがあるようです..." },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "それを動かすことができるかもしれません" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "GameWindow.txtはcomponentsフォルダのどこかにあります。見えていないのならもしかしたら隠されているのかもしれません。" }
                    },
                    CanRepeat = false, // 一度だけ実行
                    Priority = 15
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
                },

                // Authorityフォルダ開封時の説明メッセージ
                new PuzzleDefinition
                {
                    Id = "Authority_FolderOpened",
                    Trigger = new PuzzleTrigger { Type = PuzzleTrigger.TriggerType.AuthorityFolderOpened },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "Authorityフォルダを開きましたね！" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "中には4つの権限ファイルが入っています。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "AdminRights.txt - 管理者権限" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "FileAccess.txt - ファイルアクセス権限" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "SystemControl.txt - システム制御権限" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "NetworkAccess.txt - ネットワークアクセス権限" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "これらのファイルをcomponentsフォルダに移してください。" },
                        new PuzzleAction { Type = PuzzleAction.ActionType.ShowDialog, Message = "そうすることで、私により多くの権限を与えることができます。" }
                    },
                    CanRepeat = true,
                    Priority = 5
                },

                // === BGM/SE音量制御パズル ===
                // BGM.txt内容変更時: 音量をパースして設定
                new PuzzleDefinition
                {
                    Id = "BGM_VolumeChanged",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.ContentChanged,
                        ComponentNames = new[] { "BGM", "bgm" }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetBgmVolume }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // SE.txt内容変更時: 音量をパースして設定
                new PuzzleDefinition
                {
                    Id = "SE_VolumeChanged",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.ContentChanged,
                        ComponentNames = new[] { "SE", "se" }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.SetSeVolume }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // === BGM/SE停止/再開パズル ===
                // BGM.txt削除時: BGM完全停止
                new PuzzleDefinition
                {
                    Id = "BGM_Deleted",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Deleted,
                        ComponentNames = new[] { "BGM", "bgm" }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.DisableBGM }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // BGM.txt作成時: BGM再開
                new PuzzleDefinition
                {
                    Id = "BGM_Created",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Created,
                        ComponentNames = new[] { "BGM", "bgm" }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.EnableBGM }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // SE.txt削除時: SE無効化
                new PuzzleDefinition
                {
                    Id = "SE_Deleted",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Deleted,
                        ComponentNames = new[] { "SE", "se" }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.DisableSE }
                    },
                    CanRepeat = true,
                    Priority = 10
                },

                // SE.txt作成時: SE有効化
                new PuzzleDefinition
                {
                    Id = "SE_Created",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Created,
                        ComponentNames = new[] { "SE", "se" }
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction { Type = PuzzleAction.ActionType.EnableSE }
                    },
                    CanRepeat = true,
                    Priority = 10
                }
            };
        }

        private void CheckPuzzlesOnComponentCreated(ComponentCreatedEvent @event)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] ComponentCreated event received: Name={@event.Component.Name}, Type={@event.Component.Type}");

            // コンポーネント作成SEを再生（BGM/SE.txtは除外）
            if (@event.Component.Name != "BGM" && @event.Component.Name != "SE" &&
                @event.Component.Name != "bgm" && @event.Component.Name != "se")
            {
                _actionHandler.HandleAction(new PuzzleAction { Type = PuzzleAction.ActionType.PlayComponentSound });
            }

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
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] ComponentDeleted event received: Name={@event.Component.Name}");

            // コンポーネント削除SEを再生（BGM/SE.txtは除外）
            if (@event.Component.Name != "BGM" && @event.Component.Name != "SE" &&
                @event.Component.Name != "bgm" && @event.Component.Name != "se")
            {
                _actionHandler.HandleAction(new PuzzleAction { Type = PuzzleAction.ActionType.PlayComponentSound });
            }

            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Deleted &&
                               p.Trigger.MatchesComponentName(@event.Component.Name));
        }

        private void CheckPuzzlesOnComponentContentChanged(ComponentContentChangedEvent @event)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] ComponentContentChanged event received: Name={@event.Component.Name}, Type={@event.Component.Type}");

            // ContentChangedトリガーのパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.ContentChanged &&
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

        private void CheckPuzzlesOnAuthorityFolderOpened(AuthorityFolderOpenedEvent @event)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] AuthorityFolderOpened event received: Path={@event.FolderPath}");

            // Authorityフォルダが存在しない場合は、まず作成アクションを実行
            if (!System.IO.Directory.Exists(@event.FolderPath))
            {
                System.Diagnostics.Debug.WriteLine("[PuzzleService] Authority folder doesn't exist, creating hidden folder first");

                // CreateHiddenAuthorityFolderアクションを実行
                var createAction = new PuzzleAction
                {
                    Type = PuzzleAction.ActionType.CreateHiddenAuthorityFolder
                };

                _actionHandler?.HandleAction(createAction);
            }

            // AuthorityFolderOpenedトリガーのパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.AuthorityFolderOpened);
        }

        /// <summary>
        /// 背景位置変更イベント時のパズルチェック
        /// BackgroundPositionChangedEventを受け取って関連パズルを実行
        /// 位置条件を満たすPositionChangedタイプのパズルを評価
        /// </summary>
        /// <param name="event">背景位置変更イベントデータ</param>
        private void CheckPuzzlesOnBackgroundPositionChanged(BackgroundPositionChangedEvent @event)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] BackgroundPositionChanged event received: Component={@event.ComponentName}, Position=({@event.Position.X:F0}, {@event.Position.Y:F0})");

            // PositionChangedトリガーで対象コンポーネントが一致し、位置条件を満たすパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.PositionChanged &&
                             p.Trigger.MatchesComponentName(@event.ComponentName) &&
                             p.Trigger.MatchesPositionCondition(@event.Position.X, @event.Position.Y));
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
            
            // 候補パズルの詳細情報を出力
            foreach (var candidate in candidatePuzzles)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Candidate puzzle: {candidate.Id}, Priority: {candidate.Priority}");
            }

            if (candidatePuzzles.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Executing highest priority puzzle: {candidatePuzzles[0].Id} (Priority: {candidatePuzzles[0].Priority})");
            }

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
                
                // Buttonヒント関連の特別処理
                HandleButtonHintCancellation(puzzle);
                
                // Authority関連の特別処理
                HandleAuthorityState(puzzle);
                
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
        /// Buttonヒント関連のキャンセル処理
        /// </summary>
        private void HandleButtonHintCancellation(PuzzleDefinition puzzle)
        {
            try
            {
                // Button操作検出時のキャンセル
                bool isButtonOperation = puzzle.Id.Contains("Button_Delete") || 
                                       puzzle.Id.Contains("Button_Create") ||
                                       puzzle.Id.Contains("Button_Rename");

                // message取得検出時のキャンセル
                bool isMessageOperation = puzzle.Id.Contains("Key_") && 
                                        (puzzle.Trigger?.ComponentNames?.Any(name => 
                                            name.Equals("message", StringComparison.OrdinalIgnoreCase) ||
                                            name.Equals("Message", StringComparison.OrdinalIgnoreCase) ||
                                            name.Equals("MESSAGE", StringComparison.OrdinalIgnoreCase)) ?? false);

                if (isButtonOperation || isMessageOperation)
                {
                    // GameStateを更新
                    if (isMessageOperation && puzzle.Trigger.Type == PuzzleTrigger.TriggerType.Exists)
                    {
                        _gameState.IsMessageRevealed = true;
                        System.Diagnostics.Debug.WriteLine("[PuzzleService] Message revealed, updating GameState");
                    }

                    // ヒントタイマーキャンセルアクションを実行
                    var cancelAction = new PuzzleAction
                    {
                        Type = PuzzleAction.ActionType.CancelButtonHint
                    };
                    _actionHandler.HandleAction(cancelAction);
                    
                    System.Diagnostics.Debug.WriteLine($"[PuzzleService] Button hint cancelled due to: {puzzle.Id}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Error in HandleButtonHintCancellation: {ex.Message}");
            }
        }

        /// <summary>
        /// Authority関連の状態管理処理
        /// </summary>
        private void HandleAuthorityState(PuzzleDefinition puzzle)
        {
            try
            {
                // Background削除検出時の状態更新
                if (puzzle.Id == "Background_Deleted")
                {
                    _gameState.IsAuthorityFolderRevealed = true;
                    System.Diagnostics.Debug.WriteLine("[PuzzleService] Authority folder revealed, updating GameState");
                }

                // GameWindow移動検出時の状態更新とヒントキャンセル
                bool isGameWindowMoved = puzzle.Id.Contains("GameWindow") && 
                                       (puzzle.Trigger?.Type == PuzzleTrigger.TriggerType.Created ||
                                        puzzle.Trigger?.Type == PuzzleTrigger.TriggerType.Renamed);

                if (isGameWindowMoved)
                {
                    _gameState.IsGameWindowMoved = true;
                    
                    // Authorityヒントをキャンセル
                    var cancelAction = new PuzzleAction
                    {
                        Type = PuzzleAction.ActionType.CancelAuthorityHints
                    };
                    _actionHandler.HandleAction(cancelAction);
                    
                    System.Diagnostics.Debug.WriteLine($"[PuzzleService] GameWindow moved, Authority hints cancelled: {puzzle.Id}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PuzzleService] Error in HandleAuthorityState: {ex.Message}");
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