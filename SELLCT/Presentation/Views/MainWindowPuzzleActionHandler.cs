using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using SELLCT.Core.Entities;
using SELLCT.Infrastructure.Services;
using SELLCT.Views;
using SELLCT.Core.Interfaces;
using System.Threading.Tasks;

namespace SELLCT.Presentation.Views
{
    /// <summary>
    /// MainWindow用のパズルアクション実行ハンドラー（Presentation層）
    /// IPuzzleActionHandlerの実装として、UI操作、システム制御、フェーズ遷移を管理
    /// パズルゲームのアクション実行とUIレスポンスの中核的な処理を担当
    /// </summary>
    public class MainWindowPuzzleActionHandler : IPuzzleActionHandler
    {
        private readonly MainWindow _mainWindow;
        private readonly ComponentManager _componentManager;
        private readonly MetaGameController _metaGameController;
        private readonly AudioService _audioService;
        private PuzzleAction _currentChoiceAction; // 現在の選択肢アクションを保持
        private IDialogueService _dialogueService; // 対話サービス（後で設定）

        public MainWindowPuzzleActionHandler(MainWindow mainWindow, ComponentManager componentManager, MetaGameController metaGameController, AudioService audioService)
        {
            _mainWindow = mainWindow;
            _componentManager = componentManager;
            _metaGameController = metaGameController;
            _audioService = audioService;
        }
        
        /// <summary>
        /// 対話サービスを設定（循環依存回避のため後で設定）
        /// </summary>
        public void SetDialogueService(IDialogueService dialogueService)
        {
            _dialogueService = dialogueService;
        }

        public void HandleAction(PuzzleAction action)
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Executing puzzle action: {action.Type}");

                    switch (action.Type)
                    {
                        case PuzzleAction.ActionType.ShowDialog:
                            // TextWindowコンポーネントが存在する場合のみダイアログを表示
                            if (_componentManager.HasTextWindowComponent())
                            {
                                _mainWindow.TextWindow.Visibility = Visibility.Visible;
                                StartDialogFadeIn();
                                _mainWindow.ShowDialogMessage(action.Message);
                            }
                            break;
                        case PuzzleAction.ActionType.ChangeMainButtonContent:
                            _mainWindow.MainButton.Content = action.NewContent;
                            _mainWindow.StatusText.Text = $"ボタンテキストが'{action.NewContent}'に変更されました";
                            System.Diagnostics.Debug.WriteLine($"MainButton text updated to: {action.NewContent}");
                            break;
                        case PuzzleAction.ActionType.RevealHiddenItem:
                            _componentManager.RevealHiddenItem(action.HiddenItemFolder, action.TargetComponent, action.HiddenItemDisplayName);
                            _mainWindow.StatusText.Text = $"隠しアイテムが出現: {action.HiddenItemDisplayName}";
                            _mainWindow.MainButton.Visibility = Visibility.Collapsed;
                            _mainWindow.KeyImage.Visibility = Visibility.Visible;
                            break;
                        case PuzzleAction.ActionType.TransitionToPhase2:
                            TransitionToPhase2();
                            break;
                        case PuzzleAction.ActionType.ShowMessageBox:
                            MessageBox.Show(action.Message, "SELLCT", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case PuzzleAction.ActionType.ChangeNoButtonContent:
                            _mainWindow.NoButton.Content = action.NewContent;
                            _mainWindow.StatusText.Text = $"NOボタンのテキストが'{action.NewContent}'に変更されました";
                            break;
                        case PuzzleAction.ActionType.EnableNoFunction:
                            _mainWindow.IsNoFunctionEnabled = true;
                            _mainWindow.StatusText.Text = "NO機能が有効になりました";
                            break;
                        case PuzzleAction.ActionType.ShowChoice:
                            System.Diagnostics.Debug.WriteLine($"[HandleAction] ShowChoice action received. YesActions count: {action.YesActions?.Count ?? 0}, NoActions count: {action.NoActions?.Count ?? 0}");
                            _currentChoiceAction = action; // 選択肢アクションを保持
                            HandleShowChoice(action);
                            break;
                        case PuzzleAction.ActionType.SetMainButtonVisibility:
                            _mainWindow.SetMainButtonVisibility(action.IsVisible);
                            break;
                        case PuzzleAction.ActionType.SetKeyVisibility:
                            _mainWindow.SetKeyVisibility(action.IsVisible);
                            break;
                        case PuzzleAction.ActionType.SetDoorVisibility:
                            _mainWindow.SetDoorVisibility(action.IsVisible);
                            break;
                        case PuzzleAction.ActionType.SetTextWindowVisibility:
                            _mainWindow.SetTextWindowVisibility(action.IsVisible);
                            break;
                        case PuzzleAction.ActionType.SetBackgroundVisibility:
                            _mainWindow.SetBackgroundVisibility(action.IsVisible);
                            break;
                        case PuzzleAction.ActionType.TerminateExplorer:
                            _metaGameController.TerminateExplorerProcess();
                            break;
                        case PuzzleAction.ActionType.DisableKeyboardInput:
                            _mainWindow.DisableKeyboardInput();
                            break;
                        case PuzzleAction.ActionType.DisableMouseInput:
                            _mainWindow.DisableMouseInput();
                            break;
                        case PuzzleAction.ActionType.EnableKeyboardInput:
                            _mainWindow.EnableKeyboardInput();
                            break;
                        case PuzzleAction.ActionType.EnableMouseInput:
                            _mainWindow.EnableMouseInput();
                            break;
                        case PuzzleAction.ActionType.ResetGame:
                            _componentManager.ResetToInitialState();
                            break;
                        case PuzzleAction.ActionType.ClearMessageQueue:
                            _mainWindow.ClearMessageQueue();
                            break;
                        case PuzzleAction.ActionType.ExitApplication:
                            _mainWindow.Dispatcher.BeginInvoke(() => {
                                System.Windows.Application.Current.Shutdown();
                            });
                            break;
                        case PuzzleAction.ActionType.ExitWithMessageBox:
                            _mainWindow.Dispatcher.BeginInvoke(() => {
                                _mainWindow.Hide(); // メインウィンドウを非表示
                                MessageBox.Show(action.Message, "SELLCT", MessageBoxButton.OK, MessageBoxImage.Information);
                                System.Windows.Application.Current.Shutdown();
                            });
                            break;
                        case PuzzleAction.ActionType.DelayedExitWithMessageBox:
                            HandleDelayedExit(action);
                            break;
                        case PuzzleAction.ActionType.StartExplorer:
                            _metaGameController.StartExplorerProcess();
                            break;
                        case PuzzleAction.ActionType.BetrayalEnding:
                            HandleBetrayalEnding(action);
                            break;
                        case PuzzleAction.ActionType.ShowPseudoDesktopIcon:
                            HandleShowPseudoDesktopIcon(action);
                            break;
                        case PuzzleAction.ActionType.HidePseudoDesktopIcon:
                            HandleHidePseudoDesktopIcon(action);
                            break;
                        case PuzzleAction.ActionType.UpdatePseudoIconPosition:
                            HandleUpdatePseudoIconPosition(action);
                            break;
                        case PuzzleAction.ActionType.CreateHiddenAuthorityFolder:
                            HandleCreateHiddenAuthorityFolder(action);
                            break;

                        // 位置制御アクション
                        case PuzzleAction.ActionType.SetButtonPosition:
                            _mainWindow.SetButtonPosition(action.IconX, action.IconY);
                            break;

                        case PuzzleAction.ActionType.SetYESPosition:
                            _mainWindow.SetYESPosition(action.IconX, action.IconY);
                            break;

                        case PuzzleAction.ActionType.SetKeyPosition:
                            _mainWindow.SetKeyPosition(action.IconX, action.IconY);
                            break;

                        case PuzzleAction.ActionType.SetDoorPosition:
                            _mainWindow.SetDoorPosition(action.IconX, action.IconY);
                            break;

                        case PuzzleAction.ActionType.SetBackgroundPosition:
                            _mainWindow.SetBackgroundPosition(action.IconX, action.IconY);
                            break;

                        case PuzzleAction.ActionType.RecreateComponentWithPosition:
                            if (!string.IsNullOrEmpty(action.TargetComponent))
                            {
                                _componentManager.RecreateComponent(action.TargetComponent);
                                System.Diagnostics.Debug.WriteLine($"Component {action.TargetComponent} recreated with Position");
                            }
                            break;
                            
                        case PuzzleAction.ActionType.StartButtonHint:
                            _mainWindow.DialogController?.StartButtonHint(action.DelayMilliseconds / 1000);
                            System.Diagnostics.Debug.WriteLine($"Button hint started with {action.DelayMilliseconds}ms delay");
                            break;
                            
                        case PuzzleAction.ActionType.CancelButtonHint:
                            _mainWindow.DialogController?.CancelButtonHint();
                            System.Diagnostics.Debug.WriteLine("Button hint cancelled");
                            break;
                            
                        case PuzzleAction.ActionType.StartAuthorityHints:
                            _mainWindow.DialogController?.StartAuthorityHints(action.DelayMilliseconds / 1000);
                            System.Diagnostics.Debug.WriteLine($"Authority hints started with {action.DelayMilliseconds}ms delay");
                            break;
                            
                        case PuzzleAction.ActionType.CancelAuthorityHints:
                            _mainWindow.DialogController?.CancelAuthorityHints();
                            System.Diagnostics.Debug.WriteLine("Authority hints cancelled");
                            break;

                        // === オーディオ制御アクション ===
                        case PuzzleAction.ActionType.SetBgmVolume:
                            HandleSetBgmVolume();
                            break;

                        case PuzzleAction.ActionType.SetSeVolume:
                            HandleSetSeVolume();
                            break;

                        case PuzzleAction.ActionType.DisableBGM:
                            _audioService.DisableBGM();
                            System.Diagnostics.Debug.WriteLine("[AudioAction] BGM disabled");
                            break;

                        case PuzzleAction.ActionType.EnableBGM:
                            _audioService.EnableBGM();
                            System.Diagnostics.Debug.WriteLine("[AudioAction] BGM enabled");
                            break;

                        case PuzzleAction.ActionType.DisableSE:
                            _audioService.DisableSE();
                            System.Diagnostics.Debug.WriteLine("[AudioAction] SE disabled");
                            break;

                        case PuzzleAction.ActionType.EnableSE:
                            _audioService.EnableSE();
                            System.Diagnostics.Debug.WriteLine("[AudioAction] SE enabled");
                            break;

                        case PuzzleAction.ActionType.PlayClickSound:
                            _audioService.PlayClickSound();
                            break;

                        case PuzzleAction.ActionType.PlayDoorSound:
                            _audioService.PlayDoorSound();
                            break;

                        case PuzzleAction.ActionType.PlayComponentSound:
                            _audioService.PlayComponentSound();
                            break;
                    }

                    _mainWindow.UpdateComponentCount();
                    _mainWindow.UpdateDebugInfo();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnPuzzleAction: {ex.Message}");
                }
            });
        }

        public void HandleYesClick()
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"[HandleYesClick] Yes button clicked");
                
                _mainWindow.SetAwaitingChoice(false);
                _mainWindow.YesButton.Visibility = Visibility.Collapsed;
                _mainWindow.NoButton.Visibility = Visibility.Collapsed;
                _mainWindow.ChoiceButtonsPanel.Visibility = Visibility.Collapsed;

                // 対話サービスがある場合はそちらを優先
                if (_dialogueService?.IsInDialogue == true)
                {
                    System.Diagnostics.Debug.WriteLine("[HandleYesClick] Using dialogue service");
                    _dialogueService.ChooseYes();
                }
                // 従来のパズルアクションベース処理
                else if (_currentChoiceAction != null)
                {
                    var currentChoice = _currentChoiceAction; // ローカルコピーを作成
                    _currentChoiceAction = null; // 先にクリアして再帰処理に備える

                    if (currentChoice?.YesActions != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[HandleYesClick] Executing {currentChoice.YesActions.Count} Yes actions");
                        foreach (var action in currentChoice.YesActions)
                        {
                            System.Diagnostics.Debug.WriteLine($"[HandleYesClick] Executing action: {action.Type}");
                            HandleAction(action);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[HandleYesClick] No Yes actions to execute");
                    }
                }
                
                _mainWindow.StatusText.Text = "YESが選択されました";
            });
        }

        public void HandleNoClick()
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"[HandleNoClick] No button clicked");
                
                _mainWindow.SetAwaitingChoice(false);
                _mainWindow.YesButton.Visibility = Visibility.Collapsed;
                _mainWindow.NoButton.Visibility = Visibility.Collapsed;
                _mainWindow.ChoiceButtonsPanel.Visibility = Visibility.Collapsed;

                // 対話サービスがある場合はそちらを優先
                if (_dialogueService?.IsInDialogue == true)
                {
                    System.Diagnostics.Debug.WriteLine("[HandleNoClick] Using dialogue service");
                    _dialogueService.ChooseNo();
                }
                // 従来のパズルアクションベース処理
                else if (_currentChoiceAction != null)
                {
                    var currentChoice = _currentChoiceAction; // ローカルコピーを作成
                    _currentChoiceAction = null; // 先にクリアして再帰処理に備える

                    if (currentChoice?.NoActions != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[HandleNoClick] Executing {currentChoice.NoActions.Count} No actions");
                        foreach (var action in currentChoice.NoActions)
                        {
                            System.Diagnostics.Debug.WriteLine($"[HandleNoClick] Executing action: {action.Type}");
                            HandleAction(action);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[HandleNoClick] No No actions to execute");
                    }
                }
                
                _mainWindow.StatusText.Text = "NOが選択されました";
            });
        }

        private void HandleShowChoice(PuzzleAction action)
        {
            // Yes/Noコンポーネントの存在確認
            bool hasYes = _componentManager.HasYesComponent();
            bool hasNo = _componentManager.HasNoComponent();

            if (!hasYes && !hasNo)
            {
                // どちらのコンポーネントも存在しない場合、NetherChoiceActionsを実行
                if (action.NetherChoiceActions != null)
                {
                    foreach (var netherAction in action.NetherChoiceActions)
                    {
                        HandleAction(netherAction);
                    }
                }
                _currentChoiceAction = null; // 処理後クリア
            }
            else
            {
                // 通常の選択肢表示
                _mainWindow.ShowChoice();
            }
        }

        private void StartDialogFadeIn()
        {
            var fadeIn = _mainWindow.Resources["FadeInAnimation"] as Storyboard;
            if (fadeIn != null)
            {
                Storyboard.SetTarget(fadeIn, _mainWindow.TextWindow);
                fadeIn.Begin();
            }
        }

        private void HandleDelayedExit(PuzzleAction action)
        {
            // メッセージキューが空になるまで待機してから終了処理を実行
            Task.Run(async () =>
            {
                var delayMs = action.DelayMilliseconds;
                System.Diagnostics.Debug.WriteLine($"[HandleDelayedExit] Starting delayed exit process with {delayMs}ms delay");
                
                // メッセージキューの処理完了を待機
                await WaitForMessageQueueToEmpty();
                
                // 指定された時間だけ待機
                await Task.Delay(delayMs);
                
                // UI スレッドで終了処理を実行
                _mainWindow.Dispatcher.BeginInvoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine("[HandleDelayedExit] Executing exit sequence");
                    _mainWindow.Hide(); // メインウィンドウを非表示
                    MessageBox.Show(action.Message, "SELLCT", MessageBoxButton.OK, MessageBoxImage.Information);
                    System.Windows.Application.Current.Shutdown();
                });
            });
        }

        private async Task WaitForMessageQueueToEmpty()
        {
            // メッセージキューが空になるまで待機
            while (true)
            {
                bool isQueueEmpty = false;
                bool isTyping = false;
                
                _mainWindow.Dispatcher.Invoke(() =>
                {
                    isQueueEmpty = _mainWindow.IsMessageQueueEmpty();
                    isTyping = _mainWindow.IsTyping();
                });
                
                if (isQueueEmpty && !isTyping)
                {
                    System.Diagnostics.Debug.WriteLine("[WaitForMessageQueueToEmpty] Message queue empty and typing finished");
                    break;
                }
                
                System.Diagnostics.Debug.WriteLine($"[WaitForMessageQueueToEmpty] Waiting... Queue empty: {isQueueEmpty}, Typing: {isTyping}");
                await Task.Delay(100); // 100ms間隔でチェック
            }
        }

        private async Task TransitionToPhase2()
        {
            try
            {
                // _isPhase2 は MainWindow の状態なので、MainWindow 経由で更新
                _mainWindow.SetPhase2(true);
                System.Diagnostics.Debug.WriteLine("Transitioning to Phase 2");

                // ゲーム画面を閉じる
                _mainWindow.Hide();

                // フェーズ2開始
                await _metaGameController.StartPhase2();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TransitionToPhase2: {ex.Message}");
                MessageBox.Show(
                    $"フェーズ2移行中にエラーが発生しました: {ex.Message}",
                    "SELLCT - エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void HandleBetrayalEnding(PuzzleAction action)
        {
            // 裏切りエンディングの段階的実行
            Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[HandleBetrayalEnding] Starting betrayal ending sequence");
                    
                    // 1. "ああ、どうして..."メッセージを表示
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        if (_componentManager.HasTextWindowComponent())
                        {
                            _mainWindow.TextWindow.Visibility = Visibility.Visible;
                            _mainWindow.ShowDialogMessage("ああ、どうして...");
                        }
                    });
                    
                    // メッセージキューが空になるまで待機
                    await WaitForMessageQueueToEmpty();
                    
                    // 2. ウィンドウを最前面に表示
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        BringWindowToForeground();
                    });
                    
                    // 3. 指定時間待機
                    await Task.Delay(action.DelayMilliseconds);
                    
                    // 4. 画面を閉じてメッセージボックス表示後、アプリケーション終了
                    _mainWindow.Dispatcher.BeginInvoke(() =>
                    {
                        System.Diagnostics.Debug.WriteLine("[HandleBetrayalEnding] Executing final betrayal sequence");
                        _mainWindow.Hide(); // メインウィンドウを非表示
                        MessageBox.Show(action.Message, "SELLCT", MessageBoxButton.OK, MessageBoxImage.Information);
                        System.Windows.Application.Current.Shutdown();
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[HandleBetrayalEnding] Error in betrayal ending: {ex.Message}");
                }
            });
        }

        private void BringWindowToForeground()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[BringWindowToForeground] Bringing window to foreground");
                
                // ウィンドウハンドルを取得
                var windowHelper = new System.Windows.Interop.WindowInteropHelper(_mainWindow);
                var hWnd = windowHelper.Handle;
                
                if (hWnd != IntPtr.Zero)
                {
                    // Win32 API を直接呼び出し
                    ShowWindow(hWnd, 9); // SW_RESTORE
                    SetForegroundWindow(hWnd);
                    
                    System.Diagnostics.Debug.WriteLine("[BringWindowToForeground] Window brought to foreground successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[BringWindowToForeground] Failed to get window handle");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BringWindowToForeground] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 疑似デスクトップアイコンを表示
        /// </summary>
        private void HandleShowPseudoDesktopIcon(PuzzleAction action)
        {
            try
            {
                var iconManager = _mainWindow.GetPseudoDesktopIconManager();
                if (iconManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ PseudoDesktopIconManager is null");
                    return;
                }

                var iconName = action.IconName ?? "Authority";
                var folderPath = action.FolderPath;

                // アイコン位置を計算（指定されていない場合は画面中央）
                System.Windows.Point position;
                if (action.IconX != 0 || action.IconY != 0)
                {
                    position = new System.Windows.Point(action.IconX, action.IconY);
                }
                else
                {
                    // メインウィンドウの位置とサイズを取得してアイコン位置を計算
                    var windowPosition = new System.Windows.Point(_mainWindow.Left, _mainWindow.Top);
                    var windowSize = new System.Windows.Size(_mainWindow.Width, _mainWindow.Height);
                    position = iconManager.CalculateIconPosition(windowPosition, windowSize, 
                        new System.Windows.Point(-100, -100)); // 少しオフセット
                }

                var success = iconManager.CreatePseudoIcon(iconName, position, folderPath);
                
                System.Diagnostics.Debug.WriteLine($"ShowPseudoDesktopIcon: {iconName} at ({position.X:F0},{position.Y:F0}) - Success: {success}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in HandleShowPseudoDesktopIcon: {ex.Message}");
            }
        }

        /// <summary>
        /// 疑似デスクトップアイコンを非表示
        /// </summary>
        private void HandleHidePseudoDesktopIcon(PuzzleAction action)
        {
            try
            {
                var iconManager = _mainWindow.GetPseudoDesktopIconManager();
                if (iconManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ PseudoDesktopIconManager is null");
                    return;
                }

                var iconName = action.IconName ?? "Authority";
                
                if (string.IsNullOrEmpty(iconName))
                {
                    // アイコン名が指定されていない場合は全て非表示
                    iconManager.SetAllIconsVisibility(false);
                    System.Diagnostics.Debug.WriteLine("HidePseudoDesktopIcon: All icons hidden");
                }
                else
                {
                    var success = iconManager.SetIconVisibility(iconName, false);
                    System.Diagnostics.Debug.WriteLine($"HidePseudoDesktopIcon: {iconName} - Success: {success}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in HandleHidePseudoDesktopIcon: {ex.Message}");
            }
        }

        /// <summary>
        /// 疑似デスクトップアイコンの位置を更新
        /// </summary>
        private void HandleUpdatePseudoIconPosition(PuzzleAction action)
        {
            try
            {
                var iconManager = _mainWindow.GetPseudoDesktopIconManager();
                if (iconManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ PseudoDesktopIconManager is null");
                    return;
                }

                var iconName = action.IconName ?? "Authority";
                var newPosition = new System.Windows.Point(action.IconX, action.IconY);

                var success = iconManager.UpdateIconPosition(iconName, newPosition);
                System.Diagnostics.Debug.WriteLine($"UpdatePseudoIconPosition: {iconName} to ({newPosition.X:F0},{newPosition.Y:F0}) - Success: {success}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in HandleUpdatePseudoIconPosition: {ex.Message}");
            }
        }


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 非表示Authorityフォルダを作成
        /// </summary>
        private void HandleCreateHiddenAuthorityFolder(PuzzleAction action)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== HandleCreateHiddenAuthorityFolder ===");
                
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var authorityFolderPath = System.IO.Path.Combine(desktopPath, "Authority");
                
                System.Diagnostics.Debug.WriteLine($"Creating Authority folder at: {authorityFolderPath}");

                // フォルダが既に存在する場合は削除
                if (System.IO.Directory.Exists(authorityFolderPath))
                {
                    System.IO.Directory.Delete(authorityFolderPath, true);
                    System.Diagnostics.Debug.WriteLine("Deleted existing Authority folder");
                }

                // Authorityフォルダを作成
                System.IO.Directory.CreateDirectory(authorityFolderPath);
                System.Diagnostics.Debug.WriteLine("Authority folder created");

                // 5つのtxtファイルを作成
                CreateAuthorityFiles(authorityFolderPath);

                // フォルダをスーパー隠しファイル化（System + Hidden属性）
                MakeFolderSuperHidden(authorityFolderPath);

                System.Diagnostics.Debug.WriteLine("✅ Hidden Authority folder created successfully");
                System.Diagnostics.Debug.WriteLine("=== End HandleCreateHiddenAuthorityFolder ===\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error creating hidden Authority folder: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Authorityフォルダ内にテキストファイルを作成
        /// </summary>
        private void CreateAuthorityFiles(string folderPath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Creating Authority files...");

                var files = new Dictionary<string, string>
                {
                    ["AdminRights.txt"] = 
                        "管理者権限 (Administrator Rights)\n" +
                        "===================================\n\n" +
                        "この権限により、システムの重要な設定を変更できます。\n" +
                        "- システム設定の変更\n" +
                        "- 重要なファイルへのアクセス\n" +
                        "- 他のユーザーアカウントの管理\n\n" +
                        "⚠️ 注意: 管理者権限を不正なソフトウェアに与えることは\n" +
                        "システム全体を危険にさらす可能性があります。",

                    ["FileAccess.txt"] = 
                        "ファイルアクセス権限 (File Access Rights)\n" +
                        "========================================\n\n" +
                        "この権限により、ファイルシステムにアクセスできます。\n" +
                        "- ファイルの作成・読み取り・変更・削除\n" +
                        "- フォルダの作成・削除\n" +
                        "- ファイル属性の変更\n\n" +
                        "⚠️ 注意: ファイルアクセス権限は個人情報や\n" +
                        "重要な文書への不正アクセスを可能にします。",

                    ["SystemControl.txt"] = 
                        "システム制御権限 (System Control Rights)\n" +
                        "==========================================\n\n" +
                        "この権限により、システムプロセスを制御できます。\n" +
                        "- プロセスの開始・停止\n" +
                        "- サービスの制御\n" +
                        "- システム設定の変更\n\n" +
                        "⚠️ 注意: システム制御権限により、悪意のあるソフトウェアは\n" +
                        "セキュリティソフトを無効化したり、システムを乗っ取ったり\n" +
                        "することができます。",

                    ["NetworkAccess.txt"] = 
                        "ネットワークアクセス権限 (Network Access Rights)\n" +
                        "=================================================\n\n" +
                        "この権限により、ネットワーク通信を行えます。\n" +
                        "- インターネット接続\n" +
                        "- 外部サーバーとの通信\n" +
                        "- データの送受信\n\n" +
                        "⚠️ 注意: ネットワーク権限により、悪意のあるソフトウェアは\n" +
                        "個人情報を外部に送信したり、追加のマルウェアを\n" +
                        "ダウンロードしたりする可能性があります。",

                    ["ReadMe.txt"] =
                        "[CLASSIFIED DOCUMENT]\n" +
                        "機密レベル：極秘\n\n" +
                        "警告 - これより先は危険区域\n\n\n\n" +
                        "このファイルを閲覧している者へ\n" +
                        "あなたは既に危険な領域に足を踏み入れている。\n\n" +
                        "このフォルダには、あなたが知らず知らずのうちに\n" +
                        "与えようとしている権限の詳細が記録されている。\n\n\n\n" +
                        "これらの権限は以下の危険を伴う：\n\n" +
                        "・AdminRights.txt：システムの完全制御\n" +
                        "・FileAccess.txt：個人情報の完全暴露\n" +
                        "・SystemControl.txt：セキュリティの完全無効化\n" +
                        "・NetworkAccess.txt：外部への情報流出\n\n\n\n\n\n" +
                        "これ以上の探索は推奨されない。\n\n\n\n\n\n" +
                        "しかし、それでも続ける覚悟があるなら......\n\n\n\n\n\n" +
                        "まだ引き返すことができる。\n" +
                        "今すぐこのソフトウェアを終了せよ。\n\n" +
                        "これは最後の警告だ。\n" +
                        "この先には取り返しのつかない選択が待っている。"
                };

                foreach (var file in files)
                {
                    var filePath = System.IO.Path.Combine(folderPath, file.Key);
                    System.IO.File.WriteAllText(filePath, file.Value);
                    System.Diagnostics.Debug.WriteLine($"Created: {file.Key}");
                }

                System.Diagnostics.Debug.WriteLine($"✅ All 5 files created in Authority folder");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error creating Authority files: {ex.Message}");
            }
        }

        /// <summary>
        /// フォルダをスーパー隠しファイル化 (System + Hidden属性)
        /// </summary>
        private void MakeFolderSuperHidden(string folderPath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Making folder super hidden: {folderPath}");

                // attrib +s +h コマンドを実行してスーパー隠しファイル化
                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c attrib +s +h \"{folderPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = System.Diagnostics.Process.Start(processStartInfo))
                {
                    process.WaitForExit();
                    
                    if (process.ExitCode == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Folder successfully made super hidden (+s +h)");
                    }
                    else
                    {
                        var error = process.StandardError.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine($"⚠️ attrib command failed with exit code {process.ExitCode}: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error making folder super hidden: {ex.Message}");
            }
        }

        /// <summary>
        /// BGM.txtファイルから音量をパースしてAudioServiceに設定
        /// </summary>
        private void HandleSetBgmVolume()
        {
            try
            {
                var component = _componentManager.GetComponent("BGM");
                if (component != null && System.IO.File.Exists(component.FilePath))
                {
                    var content = System.IO.File.ReadAllText(component.FilePath);
                    var volume = ParseVolumeFromContent(content);

                    if (volume.HasValue)
                    {
                        _audioService.SetBgmVolume(volume.Value);
                        System.Diagnostics.Debug.WriteLine($"[AudioAction] BGM volume set to {volume.Value}%");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[AudioAction] Failed to parse BGM volume from content: '{content}'");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioAction] Error setting BGM volume: {ex.Message}");
            }
        }

        /// <summary>
        /// SE.txtファイルから音量をパースしてAudioServiceに設定
        /// </summary>
        private void HandleSetSeVolume()
        {
            try
            {
                var component = _componentManager.GetComponent("SE");
                if (component != null && System.IO.File.Exists(component.FilePath))
                {
                    var content = System.IO.File.ReadAllText(component.FilePath);
                    var volume = ParseVolumeFromContent(content);

                    if (volume.HasValue)
                    {
                        _audioService.SetSeVolume(volume.Value);
                        System.Diagnostics.Debug.WriteLine($"[AudioAction] SE volume set to {volume.Value}%");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[AudioAction] Failed to parse SE volume from content: '{content}'");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioAction] Error setting SE volume: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイル内容から音量値をパース
        /// "Volume = 100" や "Volume=100%" などの形式に対応
        /// </summary>
        /// <param name="content">ファイル内容</param>
        /// <returns>音量パーセント値（0-200、パース失敗時はnull）</returns>
        private int? ParseVolumeFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            // 正規表現: "Volume" の後に "=" があり、その後に数字（オプションで%記号）
            var match = System.Text.RegularExpressions.Regex.Match(
                content,
                @"Volume\s*=\s*(\d+)%?",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            if (match.Success && int.TryParse(match.Groups[1].Value, out int volume))
            {
                // 0-200%の範囲チェック（AudioServiceでもクランプされるが、ここでも確認）
                if (volume < 0)
                {
                    return 0;
                }
                if (volume > 200)
                {
                    System.Diagnostics.Debug.WriteLine($"[AudioAction] Volume {volume}% exceeds max, will be clamped to 200%");
                }

                return volume;
            }

            return null;
        }
    }
}