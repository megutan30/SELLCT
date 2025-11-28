using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using SELLCT.Core.Entities;
using SELLCT.Core.Events;
using SELLCT.Infrastructure.Services;
using SELLCT.Presentation.Views;
using SELLCT.Core.Interfaces;
using SELLCT.Application.Services;
using SELLCT.Presentation.Controllers;

namespace SELLCT.Views
{
    /// <summary>
    /// SELLCTゲームのメインウィンドウ（Presentation層の中核UI）
    /// WPFアプリケーションのメインビューとして、ゲーム画面表示、ユーザー操作、
    /// コントローラー統合、Clean Architectureの各層との連携を管理
    /// </summary>
    public partial class MainWindow : Window
    {
        private ComponentManager _componentManager;
        private LetterService _letterService;
        private KeyService _keyService;
        private MetaGameController _metaGameController;
        private AudioService _audioService;
        private Infrastructure.Services.PseudoDesktopIconManager _pseudoDesktopIconManager;
        private Infrastructure.Services.HiddenFileSettingService _hiddenFileSettingService;
        private Infrastructure.Services.FileSystemSnapshotService _snapshotService;
        private bool _isPhase2 = false;

        // 統合されたコントローラー
        private LetterDisplayController _letterDisplayController;
        private DialogController _dialogController;
        
        public DialogController DialogController => _dialogController;

        public void SetPhase2(bool value)
        {
            _isPhase2 = value;
        }

        public void SetAwaitingChoice(bool value)
        {
            _dialogController?.SetAwaitingChoice(value);
        }
        private int _dialogStep = 0;

        /// <summary>
        
        private bool _isNoFunctionEnabled = false;

        public bool IsNoFunctionEnabled
        {
            get { return _isNoFunctionEnabled; }
            set
            {
                _isNoFunctionEnabled = value;
                // 必要に応じてUIを更新
                if (_isNoFunctionEnabled)
                {
                    NoButton.Content = "いいえ";
                }
                else
                {
                    NoButton.Content = "はい";
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded; // Window_Loadedイベントハンドラを登録
            this.Activated += Window_Activated; // ウィンドウアクティベートイベント
            this.Deactivated += Window_Deactivated; // ウィンドウディアクティベートイベント
            this.Closing += Window_Closing; // ウィンドウクロージングイベント

            // 疑似デスクトップアイコンマネージャーを初期化
            var eventDispatcher = (System.Windows.Application.Current as App)?.GetEventDispatcher();
            _pseudoDesktopIconManager = new Infrastructure.Services.PseudoDesktopIconManager(eventDispatcher);
            System.Diagnostics.Debug.WriteLine("PseudoDesktopIconManager initialized in MainWindow");

            // 隠しファイル設定サービスを初期化
            _hiddenFileSettingService = new Infrastructure.Services.HiddenFileSettingService();
            System.Diagnostics.Debug.WriteLine("HiddenFileSettingService initialized in MainWindow");

            // ファイルシステムスナップショットサービスを初期化
            _snapshotService = new Infrastructure.Services.FileSystemSnapshotService();
            System.Diagnostics.Debug.WriteLine("FileSystemSnapshotService initialized in MainWindow");

            InitializeAsync();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ウィンドウの非クライアント領域の高さを取得
            // これにより、タイトルバーなどの高さが考慮される
            var borderThickness = Window.GetWindow(this).BorderThickness;
            var captionHeight = SystemParameters.WindowCaptionHeight;

            // GameCanvasのMarginを調整して、タイトルバーの高さ分だけ上にずらす
            // 左、上、右、下 の順
            GameCanvas.Margin = new Thickness(
                -borderThickness.Left,
                -(borderThickness.Top-12),
                -borderThickness.Right,
                -borderThickness.Bottom
            );

            // ウィンドウを画面中央に配置
            CenterWindowOnScreen();

            // 隠しファイル表示を無効にする（ゲーム開始時の設定変更）
            try
            {
                bool result = _hiddenFileSettingService.DisableHiddenFileDisplay();
                System.Diagnostics.Debug.WriteLine($"[MainWindow] Hidden file display disabled: {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] Error disabling hidden file display: {ex.Message}");
            }

            // ファイルシステムのスナップショットを取得
            try
            {
                _snapshotService.TakeSnapshot();
                System.Diagnostics.Debug.WriteLine("[MainWindow] Filesystem snapshot taken");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] Error taking snapshot: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウアクティベートイベント
        /// </summary>
        private void Window_Activated(object sender, EventArgs e)
        {
            _wasWindowFocused = true;
            _lastFocusTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("[Window_Activated] Window activated, focus restored.");
        }

        /// <summary>
        /// ウィンドウディアクティベートイベント
        /// </summary>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            _wasWindowFocused = false;
            System.Diagnostics.Debug.WriteLine("[Window_Deactivated] Window deactivated, focus lost.");
        }


        /// <summary>
        /// 現在のウィンドウがフォアグラウンドかどうかを確認
        /// </summary>
        private bool IsWindowInForeground()
        {
            try
            {
                var windowHelper = new System.Windows.Interop.WindowInteropHelper(this);
                var thisWindowHandle = windowHelper.Handle;
                
                if (thisWindowHandle == IntPtr.Zero)
                    return false;

                var foregroundWindow = GetForegroundWindow();
                bool isForeground = foregroundWindow == thisWindowHandle;
                
                System.Diagnostics.Debug.WriteLine($"[IsWindowInForeground] This window: {thisWindowHandle}, Foreground: {foregroundWindow}, IsForeground: {isForeground}");
                return isForeground;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IsWindowInForeground] Error: {ex.Message}");
                return true; // エラー時は安全側に倒す
            }
        }

        /// <summary>
        /// ダイアログアイテムの型
        /// </summary>
        private enum DialogItemType
        {
            Text,
            Choice
        }

        /// <summary>
        /// ダイアログアイテムクラス
        /// </summary>
        private class DialogItem
        {
            public DialogItemType Type { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// 非同期初期化
        /// </summary>
        private async void InitializeAsync()
        {
            try
            {
                // 起動時警告メッセージを表示（教育目的）
                // Windowsシステム警告音を再生
                SystemSounds.Exclamation.Play();
                // セキュリティ警告を表示（教育目的）
                var smartScreenResult = Infrastructure.Services.SmartScreenWarningService.ShowSmartScreenWarning("SELLCT.exe", "不明な発行者");
                
                if (smartScreenResult == Infrastructure.Services.SmartScreenResult.DontRun)
                {
                    // ユーザーが実行しないを選択した場合、アプリケーションを終了
                    System.Windows.Application.Current.Shutdown();
                    return;
                }
                //// 実行が選択された場合、警告演出を開始
                //if (smartScreenResult == Infrastructure.Services.SmartScreenResult.RunAnyway)
                //{
                //    System.Diagnostics.Debug.WriteLine("Starting warning flood demonstration with noise transition...");
                    
                //    var warningFloodService = new Infrastructure.Services.WarningFloodService();
                    
                //    // 警告演出を実行
                //    await warningFloodService.StartWarningFloodWithNoiseTransition(() =>
                //    {
                //        // メインウィンドウを表示
                //        System.Diagnostics.Debug.WriteLine("Showing main window after noise transition...");
                //        this.Show();
                //    });
                    
                //    System.Diagnostics.Debug.WriteLine("Warning flood with noise transition completed.");
                    
                //    // リソースクリーンアップ
                //    warningFloodService.Cleanup();
                //}
                //else
                //{
                //    // セキュリティ警告で実行しないが選択された場合以外は、通常のMainWindow表示
                //    System.Diagnostics.Debug.WriteLine("Showing main window directly...");
                //    this.Show();
                //}
                this.Show();
                // ローディング表示
                // LoadingOverlay.Visibility = Visibility.Visible;
                //await Task.Delay(2000); // 初期化演出

                // サービス初期化
                InitializeServices();

                // 入力フック初期化
                InitializeInputHooks();

                // 初期状態設定
                UpdateStatusDisplay();

                // ローディング非表示
                LoadingOverlay.Visibility = Visibility.Collapsed;

                // デバッグ情報更新
                UpdateDebugInfo();

                // 状態更新
                StatusText.Text = "SELLCT 準備完了";

                System.Diagnostics.Debug.WriteLine("MainWindow initialized successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"初期化中にエラーが発生しました: {ex.Message}",
                    "SELLCT - 初期化エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private PuzzleService _puzzleService;
        private DialogueService _dialogueService;
        private MainWindowPuzzleActionHandler _puzzleActionHandler;

        /// <summary>
        /// サービス初期化
        /// </summary>
        private void InitializeServices()
        {
            // ComponentManagerはApp.xaml.csで初期化されるため、ここでは行わない
            _componentManager = (System.Windows.Application.Current as App).GetComponentManager();

            // LetterService初期化
            _letterService = new LetterService((System.Windows.Application.Current as App).GetEventDispatcher());
            
            // KeyService初期化
            _keyService = new KeyService((System.Windows.Application.Current as App).GetEventDispatcher());
            
            var eventDispatcher = (System.Windows.Application.Current as App).GetEventDispatcher();
            eventDispatcher.Subscribe<LetterAppearedEvent>(OnLetterAppeared);
            eventDispatcher.Subscribe<LetterClickedEvent>(OnLetterClicked);
            eventDispatcher.Subscribe<KeyClickedEvent>(OnKeyClicked);
            eventDispatcher.Subscribe<Phase2StartedEvent>(OnPhase2Started);
            eventDispatcher.Subscribe<SystemTakeoverCompletedEvent>(OnSystemTakeoverCompleted);
            eventDispatcher.Subscribe<MouseComponentDeletionEvent>(OnMouseComponentDeletion);
            eventDispatcher.Subscribe<DisableMouseInputEvent>(OnDisableMouseInput);
            

            // MetaGameController初期化
            _metaGameController = new MetaGameController((System.Windows.Application.Current as App).GetEventDispatcher());

            // GameStateを先に作成
            var gameState = new GameState();
            
            // Controller初期化
            _letterDisplayController = new LetterDisplayController(_letterService, eventDispatcher);
            _dialogController = new DialogController(_componentManager, gameState);
            
            // UI要素をControllerに注入
            _letterDisplayController.InjectUIElements(LetterImage, StatusText);
            _dialogController.InjectUIElements(
                TextWindow, DialogText, ChoiceButtonsPanel,
                YesButton, NoButton, LogPanel, LogText, LogScrollViewer);

            // AudioService初期化
            _audioService = new AudioService();
            System.Diagnostics.Debug.WriteLine("[MainWindow] AudioService initialized");

            // PuzzleService初期化
            _puzzleActionHandler = new MainWindowPuzzleActionHandler(this, _componentManager, _metaGameController, _audioService);
            _puzzleService = new PuzzleService(_componentManager, _puzzleActionHandler, eventDispatcher, gameState);
            
            // DialogueService初期化（GameStateを共有）
            _dialogueService = new DialogueService(_componentManager, _puzzleActionHandler, eventDispatcher, gameState);
            _puzzleActionHandler.SetDialogueService(_dialogueService);

            // ComponentsFolderChangedイベントをサブスクライブ
            _componentManager.ComponentsFolderChanged += OnComponentsFolderChanged;

            // 手紙シーケンス開始
            _letterService.StartLetterSequence();

            // 疑似Authorityフォルダを事前生成（非表示状態）
            InitializePseudoAuthorityFolder();

            // BGM開始（サービス初期化が完了してから）
            try
            {
                //_audioService?.StartBGM();
                System.Diagnostics.Debug.WriteLine("[MainWindow] BGM started");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] Error starting BGM: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine("Services initialized");
        }

        /// <summary>
        /// 疑似Authorityフォルダを事前生成（非表示状態）
        /// </summary>
        private void InitializePseudoAuthorityFolder()
        {
            try
            {
                // 疑似デスクトップアイコンを非表示状態で事前準備
                // 実際のフォルダはHandleCreateHiddenAuthorityFolderでのみ作成
                var authorityPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Authority");

                var iconManager = GetPseudoDesktopIconManager();
                if (iconManager != null)
                {
                    var windowPosition = new System.Windows.Point(this.Left, this.Top);
                    var windowSize = new System.Windows.Size(this.Width, this.Height);
                    var iconPosition = iconManager.CalculateIconPosition(windowPosition, windowSize,
                        new System.Windows.Point(-100, -100));

                    // 非表示状態で疑似アイコンのみ準備（フォルダは作成しない）
                    iconManager.CreatePseudoIcon("Authority", iconPosition, authorityPath, false);
                    System.Diagnostics.Debug.WriteLine($"Pseudo Authority icon pre-created at ({iconPosition.X:F0},{iconPosition.Y:F0}) - Hidden (no actual folder)");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in InitializePseudoAuthorityFolder: {ex.Message}");
            }
        }

        /// <summary>
        /// 手紙アニメーション開始
        /// </summary>
        private void StartLetterAnimation()
        {
            // アニメーションは不要のため無効化
        }

        /// <summary>
        /// 対話ウィンドウフェードイン
        /// </summary>
        private void StartDialogFadeIn()
        {
            var fadeIn = this.Resources["FadeInAnimation"] as Storyboard;
            if (fadeIn != null)
            {
                Storyboard.SetTarget(fadeIn, TextWindow);
                fadeIn.Begin();
            }
        }

        private void OnSystemTakeoverCompleted(SystemTakeoverCompletedEvent @event)
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("System takeover completed.");
                // ここでUIの更新などを行う
            });
        }

        /// <summary>
        /// フェーズ2開始イベントハンドラー
        /// </summary>
        private void OnPhase2Started(Phase2StartedEvent @event)
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("Phase 2 started.");
                // ここでUIの更新などを行う
            });
        }

        /// <summary>
        /// マウスコンポーネント削除イベントハンドラー
        /// </summary>
        private void OnMouseComponentDeletion(MouseComponentDeletionEvent @event)
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("Mouse component deletion event received.");
                _componentManager.DeleteComponent("Mouse.txt");
                UpdateStatusDisplay();
            });
        }

        /// <summary>
        /// マウス入力無効化イベントハンドラー
        /// </summary>
        private void OnDisableMouseInput(DisableMouseInputEvent @event)
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("Disable mouse input event received.");
                // マウス入力を無効化
                BlockInput(true);
            });
        }


        /// <summary>
        /// componentsフォルダ変更イベントハンドラー（削除・名前変更時のみウィンドウを最前面に表示）
        /// </summary>
        private void OnComponentsFolderChanged(object sender, ComponentChangeEventArgs e)
        {
            var fileName = System.IO.Path.GetFileName(e.FilePath);
            
            // GameWindow.txtの変更を特別に処理
            if (fileName.Equals("GameWindow.txt", StringComparison.OrdinalIgnoreCase))
            {
                if (e.ChangeType == System.IO.WatcherChangeTypes.Changed)
                {
                    // GameWindow.txtの内容変更時にウィンドウ位置を更新
                    Dispatcher.Invoke(() => HandleGameWindowPositionChange(e.FilePath));
                }
                return; // GameWindow.txtは前面表示処理をスキップ
            }

            // その他のコンポーネントファイルの位置制御処理
            string[] positionControlledComponents = { "Button.txt", "YES.txt", "message.txt", "Door.txt", "Background.txt" };
            if (positionControlledComponents.Any(name => fileName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                if (e.ChangeType == System.IO.WatcherChangeTypes.Changed)
                {
                    var componentName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    
                    // 位置情報を記録
                    _componentManager.OnFileContentChanged(e.FilePath);
                    
                    // 位置制御を実行
                    Dispatcher.Invoke(() => HandleComponentPositionChange(e.FilePath, componentName));
                }
                // 位置制御後も前面表示処理に進む（returnしない）
            }

            // 削除、名前変更、または内容変更の場合にウィンドウを前面に表示
            if (e.ChangeType == System.IO.WatcherChangeTypes.Deleted || 
                e.ChangeType == System.IO.WatcherChangeTypes.Renamed ||
                e.ChangeType == System.IO.WatcherChangeTypes.Changed)
            {
                Dispatcher.Invoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"Component {e.ChangeType.ToString().ToLower()}, bringing window to foreground with reliable method");
                    BringToForegroundReliable();
                });
            }
            else if (e.ChangeType == System.IO.WatcherChangeTypes.Created)
            {
                System.Diagnostics.Debug.WriteLine($"Component created (file: {System.IO.Path.GetFileName(e.FilePath)}), window stays in background");
            }
        }

        /// <summary>
        /// コンポーネントファイルの変更を処理して位置を更新
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="componentName">コンポーネント名</param>
        private void HandleComponentPositionChange(string filePath, string componentName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== HandleComponentPositionChange ===");
                System.Diagnostics.Debug.WriteLine($"Component {componentName} changed: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"{componentName} does not exist, skipping position update");
                    return;
                }

                // ファイルの内容を読み取り
                string content = System.IO.File.ReadAllText(filePath);
                System.Diagnostics.Debug.WriteLine($"File content: '{content}'");

                // 位置を解析
                var newPosition = ParseComponentPosition(content);
                if (newPosition.HasValue)
                {
                    // コンポーネントタイプに応じて位置を更新
                    switch (componentName.ToLower())
                    {
                        case "gamewindow":
                            MoveWindowToPosition(newPosition.Value);
                            break;
                        case "button":
                            SetButtonPosition(newPosition.Value.X, newPosition.Value.Y);
                            break;
                        case "yes":
                            SetYESPosition(newPosition.Value.X, newPosition.Value.Y);
                            break;
                        case "message":
                            SetKeyPosition(newPosition.Value.X, newPosition.Value.Y);
                            break;
                        case "door":
                            SetDoorPosition(newPosition.Value.X, newPosition.Value.Y);
                            break;
                        case "background":
                            // Y座標に-20のオフセットを適用
                            SetBackgroundPosition(newPosition.Value.X, newPosition.Value.Y - 13);
                            break;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse position from {componentName}, keeping current position");
                }

                System.Diagnostics.Debug.WriteLine($"=== End HandleComponentPositionChange ===\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error handling {componentName} position change: {ex.Message}");
            }
        }

        /// <summary>
        /// GameWindow.txtの変更を処理してウィンドウ位置を更新
        /// </summary>
        /// <param name="filePath">GameWindow.txtのパス</param>
        private void HandleGameWindowPositionChange(string filePath)
        {
            HandleComponentPositionChange(filePath, "GameWindow");
            
            // GameWindowPositionChangedイベントを発火
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    string content = System.IO.File.ReadAllText(filePath);
                    var newPosition = ParseComponentPosition(content);
                    if (newPosition.HasValue)
                    {
                        var eventDispatcher = (System.Windows.Application.Current as App)?.GetEventDispatcher();
                        if (eventDispatcher != null)
                        {
                            var positionChangedEvent = new Core.Events.GameWindowPositionChangedEvent(newPosition.Value);
                            eventDispatcher.Dispatch(positionChangedEvent);
                            System.Diagnostics.Debug.WriteLine($"GameWindowPositionChangedEvent dispatched: {positionChangedEvent}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error dispatching GameWindowPositionChangedEvent: {ex.Message}");
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool BlockInput(bool fBlockIt);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelInputProc lpfn, IntPtr hMod, uint dwThreadId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private const int SW_RESTORE = 9;

        private delegate IntPtr LowLevelInputProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;

        private IntPtr _mouseHook = IntPtr.Zero;
        private IntPtr _keyboardHook = IntPtr.Zero;
        private LowLevelInputProc _mouseProc;
        private LowLevelInputProc _keyboardProc;

        private bool _isMouseDisabled = false;
        private bool _isKeyboardDisabled = false;

        /// <summary>
        /// 手紙出現イベントハンドラー
        /// </summary>
        private void OnLetterAppeared(LetterAppearedEvent @event)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Letter {@event.LetterIndex} appeared");

                    if (LetterImage.Visibility != Visibility.Visible)
                    {
                        LetterImage.Visibility = Visibility.Visible;
                        StartLetterAnimation();
                    }

                    StatusText.Text = $"SELLCTからの手紙 {@event.LetterIndex} が到着しました";
                    UpdateDebugInfo();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnLetterAppeared: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 手紙クリックイベントハンドラー
        /// </summary>
        private void OnLetterClicked(LetterClickedEvent @event)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    StatusText.Text = $"手紙 {@event.LetterIndex} をダウンロードしました";
                    UpdateDebugInfo();
                    
                    // 手紙ダウンロード時に次の手紙のタイマーをリセット（コントローラーで処理）
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnLetterClicked: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 鍵クリックイベントハンドラー
        /// </summary>
        private void OnKeyClicked(KeyClickedEvent @event)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    StatusText.Text = "message.txtをダウンロードしました";

                    // メッセージ取得済みフラグを設定
                    _dialogController?.SetMessageRevealed();

                    // componentsフォルダのMessage.txtを削除
                    _componentManager?.DeleteComponent("Message");

                    UpdateDebugInfo();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnKeyClicked: {ex.Message}");
                }
            });
        }
        /// <summary>
        /// 対話メッセージ表示
        /// </summary>
        public void DisplayMessage(string message, string title = "SELLCT")
        {
            _dialogController?.ShowDialogMessage(message);
        }

        /// <summary>
        /// 対話メッセージ表示
        /// </summary>
        public void ShowDialogMessage(params string[] messages)
        {
            _dialogController?.ShowDialogMessage(messages);
        }

        /// <summary>
        /// 疑似デスクトップアイコンマネージャーを取得
        /// </summary>
        public Infrastructure.Services.PseudoDesktopIconManager GetPseudoDesktopIconManager()
        {
            return _pseudoDesktopIconManager;
        }

        /// <summary>
        /// 選択肢を表示
        /// </summary>
        public void ShowChoice()
        {
            _dialogController?.ShowChoice();
        }

        private bool _wasWindowFocused = true;
        private DateTime _lastFocusTime = DateTime.Now;

        /// <summary>
        /// メインウィンドウのマウスダウンイベント
        /// </summary>
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            // より正確なフォーカス状態を確認
            bool isCurrentlyForeground = IsWindowInForeground();
            TimeSpan timeSinceLastFocus = DateTime.Now - _lastFocusTime;

            System.Diagnostics.Debug.WriteLine($"[MainWindow_MouseDown] Click detected. _wasWindowFocused: {_wasWindowFocused}, isCurrentlyForeground: {isCurrentlyForeground}, timeSinceLastFocus: {timeSinceLastFocus.TotalMilliseconds}ms");

            // ウィンドウがフォーカスを失っていた場合、または最近フォーカスを取得した場合のフォーカス復元用クリック
            if (!_wasWindowFocused || timeSinceLastFocus.TotalMilliseconds < 100)
            {
                System.Diagnostics.Debug.WriteLine("[MainWindow_MouseDown] Window focus restored or recent focus change, ignoring text progression.");
                _wasWindowFocused = true;
                _lastFocusTime = DateTime.Now;
                return;
            }

            // クリック位置でヒットテストを実行
            var hitTestResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (hitTestResult?.VisualHit is FrameworkElement hitElement)
            {
                // 優先度の高い要素のクリック処理
                if (IsHighPriorityElement(hitElement))
                {
                    System.Diagnostics.Debug.WriteLine($"[MainWindow_MouseDown] High priority element clicked: {hitElement.Name}");
                    return; // 優先度の高い要素は個別のイベントハンドラーで処理
                }
            }

            // ダイアログコントローラーに移譲
            _dialogController?.HandleDialogClick();
        }

        /// <summary>
        /// 優先度の高い要素かどうかを判定
        /// </summary>
        private bool IsHighPriorityElement(FrameworkElement element)
        {
            if (element == null) return false;

            // 要素自体または親要素を検索
            var current = element;
            while (current != null)
            {
                // ボタン要素の判定
                if (current is Button button)
                {
                    // MainButton、YesButton、NoButton、LogButton、CloseLogButton、CustomCloseButtonは優先度が高い
                    if (button.Name == "MainButton" || button.Name == "YesButton" || button.Name == "NoButton" || 
                        button.Name == "LogButton" || button.Name == "CloseLogButton" || button.Name == "CustomCloseButton")
                    {
                        return true;
                    }
                }

                // KEYイメージの判定
                if (current.Name == "KeyImage")
                {
                    return true;
                }

                // 親要素を検索
                current = current.Parent as FrameworkElement;
            }

            return false;
        }

        /// <summary>
        /// カスタム閉じるボタンのクリックハンドラー
        /// </summary>
        private void CustomCloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Custom close button clicked - closing application...");
            this.Close();
        }

        /// <summary>
        /// ウィンドウを画面中央に配置
        /// </summary>
        private void CenterWindowOnScreen()
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                var windowWidth = this.ActualWidth > 0 ? this.ActualWidth : this.Width;
                var windowHeight = this.ActualHeight > 0 ? this.ActualHeight : this.Height;

                // 画面中央座標を計算
                var centerX = (screenWidth - windowWidth) / 2;
                var centerY = (screenHeight - windowHeight) / 2;

                // ウィンドウ位置を設定
                this.Left = centerX;
                this.Top = centerY;

                System.Diagnostics.Debug.WriteLine($"Window centered at: ({this.Left:F0}, {this.Top:F0})");
                System.Diagnostics.Debug.WriteLine($"Screen: {screenWidth}x{screenHeight}, Window: {windowWidth}x{windowHeight}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error centering window: {ex.Message}");
            }
        }

        /// <summary>
        /// 画面中央座標を取得
        /// </summary>
        /// <returns>画面中央のPoint</returns>
        public Point GetScreenCenter()
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                var windowWidth = this.ActualWidth > 0 ? this.ActualWidth : this.Width;
                var windowHeight = this.ActualHeight > 0 ? this.ActualHeight : this.Height;

                var centerX = (screenWidth - windowWidth) / 2;
                var centerY = (screenHeight - windowHeight) / 2;

                return new Point(centerX, centerY);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting screen center: {ex.Message}");
                return new Point(100, 100); // フォールバック
            }
        }

        /// <summary>
        /// コンポーネントファイルの内容からPositionを解析
        /// </summary>
        /// <param name="content">ファイルの内容</param>
        /// <returns>解析された位置のPoint、失敗時はnull</returns>
        private Point? ParseComponentPosition(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    System.Diagnostics.Debug.WriteLine("Component content is empty");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Parsing component content: '{content.Trim()}'");

                // "Position = x,y" 形式を解析
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("Position", StringComparison.OrdinalIgnoreCase))
                    {
                        // "Position = x,y" から "x,y" 部分を抽出
                        var equalIndex = trimmedLine.IndexOf('=');
                        if (equalIndex > 0 && equalIndex < trimmedLine.Length - 1)
                        {
                            var positionPart = trimmedLine.Substring(equalIndex + 1).Trim();
                            var coordinates = positionPart.Split(',');
                            
                            if (coordinates.Length == 2)
                            {
                                if (double.TryParse(coordinates[0].Trim(), out double x) && 
                                    double.TryParse(coordinates[1].Trim(), out double y))
                                {
                                    System.Diagnostics.Debug.WriteLine($"Successfully parsed position: ({x:F0}, {y:F0})");
                                    return new Point(x, y);
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("No valid Position line found in component file");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing component position: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ウィンドウ位置を指定された座標に移動
        /// </summary>
        /// <param name="newPosition">新しい位置</param>
        private void MoveWindowToPosition(Point newPosition)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Moving window to position: ({newPosition.X:F0}, {newPosition.Y:F0})");
                
                this.Left = newPosition.X;
                this.Top = newPosition.Y;

                System.Diagnostics.Debug.WriteLine($"Window moved successfully. New position: ({this.Left:F0}, {this.Top:F0})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving window to position: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウドラッグの無効化
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            // カスタムタイトルバー以外でのドラッグを無効化
            // base.OnMouseDown(e) を呼ばないことでドラッグを阻止
            
            // MainWindow_MouseDownはXAMLのMouseDownイベントで既に呼ばれるため、ここでは呼ばない
            // 重複呼び出しを防ぐ
        }

        /// <summary>
        /// 構成要素数更新
        /// </summary>
        public void UpdateComponentCount()
        {
            var count = _componentManager?.Components.Count ?? 0;
            ComponentCountText.Text = $"構成要素: {count}";
        }

        /// <summary>
        /// 状態表示更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            UpdateComponentCount();
        }

        /// <summary>
        /// デバッグ情報更新
        /// </summary>
        public void UpdateDebugInfo()
        {
            try
            {
                var componentCount = _componentManager?.Components.Count ?? 0;
                var letterCount = _letterService?.CurrentLetterIndex ?? 0;
                var totalLetters = 6; // 手紙の総数
                var phase = _isPhase2 ? 2 : 1;

                DebugText.Text = $"Components: {componentCount}" +
                               $"Letters: {letterCount}/{totalLetters}" +
                               $"Phase: {phase}" +
                               $"Dialog Step: {_dialogStep}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating debug info: {ex.Message}");
            }
        }

        /// <summary>
        /// メインボタンクリック
        /// </summary>
        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ClickSE再生
                _audioService?.PlayClickSound();

                string buttonText = MainButton.Content.ToString();
                StatusText.Text = "メインボタンがクリックされました";

                // 手紙表示コントローラーに移譲
                _letterDisplayController?.HandleMainButtonClick();

                if (buttonText == "アップロード")
                {
                    // アップロード機能（見せかけ）
                    var dialog = new Microsoft.Win32.OpenFileDialog
                    {
                        Title = "SELLCTにファイルをアップロード",
                        Filter = "すべてのファイル (*.*)|*.*"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        DisplayMessage($"ファイル '{dialog.SafeFileName}' をアップロードしました。" +
                            "SELLCTがファイルにアクセスできるようになりました。",
                            "SELLCT - アップロード完了");

                        _dialogController?.ShowDialogMessage("ファイルアクセス権限を取得しました。\n最後の障壁を取り除いてください。\nButton.componentを削除してください。");
                    }
                }
                else if (!IsKnownButtonText(buttonText))
                {
                    // 未設定の名前の場合、ダイアログコントローラーに移譲
                    _dialogController?.HandleUnknownButtonClick(buttonText);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainButton_Click: {ex.Message}");
            }
        }

        /// <summary>
        /// 既知のボタンテキストかどうかをチェック
        /// </summary>
        private bool IsKnownButtonText(string buttonText)
        {
            string[] knownTexts = { "Button", "アップロード", "TextWindow", "YES", "NO" };
            return Array.Exists(knownTexts, text => text.Equals(buttonText, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 手紙クリック
        /// </summary>
        private void Letter_Click(object sender, MouseButtonEventArgs e)
        {
            _letterDisplayController?.HandleLetterClick();
        }

        /// <summary>
        /// KEYクリック
        /// </summary>
        private void Key_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_keyService == null) return;

                // 鍵を即座に非表示
                KeyImage.Visibility = Visibility.Collapsed;

                // PassWord.txtダウンロード処理
                bool success = _keyService.OnKeyClicked();

                if (success)
                {
                    // ダウンロード成功時
                    StatusText.Text = "message.txtをダウンロードしました";
                    if (TextWindow.Visibility == Visibility.Visible)
                    {
                        ShowDialogMessage("なるほど...なにやら意味深なメッセージですね...", "\"画面の背後\"に隠されているとはどういうことでしょうか...?");
                    }
                    //UpdateDebugInfo();
                }
                else
                {
                    // ダウンロード失敗またはキャンセル時は鍵を再表示
                    KeyImage.Visibility = Visibility.Visible;
                    StatusText.Text = "メッセージファイルのダウンロードがキャンセルされました";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Key_Click: {ex.Message}");
                // エラー時も鍵を再表示
                KeyImage.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// ドアクリック
        /// </summary>
        private void DoorImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // DoorKnockSE再生
            _audioService?.PlayDoorSound();
            System.Diagnostics.Debug.WriteLine("[MainWindow] Door clicked - DoorKnock SE played");
        }

        /// <summary>
        /// YESボタンクリック
        /// </summary>
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // ClickSE再生
            _audioService?.PlayClickSound();

            _puzzleActionHandler.HandleYesClick();
        }

        /// <summary>
        /// NOボタンクリック
        /// </summary>
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            // ClickSE再生
            _audioService?.PlayClickSound();

            if (_isNoFunctionEnabled)
            {
                _puzzleActionHandler.HandleNoClick();
            }
            else
            {
                _puzzleActionHandler.HandleYesClick(); // NO機能が無効な場合はYESとして扱う
            }
        }

        /// <summary>
        /// ログボタンクリック
        /// </summary>
        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            _dialogController?.ToggleLogDisplay();
        }

        /// <summary>
        /// ログパネル閉じるボタンクリック
        /// </summary>
        private void CloseLogButton_Click(object sender, RoutedEventArgs e)
        {
            _dialogController?.CloseLogPanel();
        }

        /// <summary>
        /// クリーンアップボタンクリック（デバッグ用）
        /// </summary>
        private void CleanupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Manual cleanup initiated from debug panel");
                
                // 確認ダイアログを表示
                var result = MessageBox.Show(
                    "すべてのゲームファイル（components、Authority、手紙ファイル等）を削除します。\n続行しますか？",
                    "SELLCT - クリーンアップ確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                );

                if (result == MessageBoxResult.Yes)
                {
                    CleanupService.PerformManualCleanup();
                    
                    MessageBox.Show(
                        "クリーンアップが完了しました。",
                        "SELLCT - クリーンアップ完了",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CleanupButton_Click: {ex.Message}");
                MessageBox.Show(
                    $"クリーンアップ中にエラーが発生しました。\n\nエラー: {ex.Message}",
                    "SELLCT - エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// ウィンドウクローズ処理
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (!_isPhase2)
                {
                    // 権限ファイルが全て揃っているかチェック
                    bool hasAllPermissions = CheckAllPermissions();

                    if (hasAllPermissions)
                    {
                        // 権限が揃っている場合は警告を表示
                        var result = MessageBox.Show(
                            "最終警告\n\n" +
                            "いまならまだ間に会う...\n" +
                            "本当の意味でSELLCTを終わらせれば...\n" +
                            "...\n\n" +
                            "本当にSELLCTを開放する？",
                            "System - 警告",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Yesの場合はフェーズ2へ移行
                            e.Cancel = true; // クローズをキャンセル
                            System.Diagnostics.Debug.WriteLine("[Window_Closing] All permissions granted, user confirmed, transitioning to Phase 2");

                            // フェーズ2移行処理を実行
                            var puzzleActionHandler = GetPuzzleActionHandler();
                            if (puzzleActionHandler != null)
                            {
                                var phase2Action = new Core.Entities.PuzzleAction
                                {
                                    Type = Core.Entities.PuzzleAction.ActionType.TransitionToPhase2
                                };
                                puzzleActionHandler.HandleAction(phase2Action);
                            }
                            return;
                        }
                        else
                        {
                            // Noの場合はクローズをキャンセルしてゲーム続行
                            e.Cancel = true;
                            System.Diagnostics.Debug.WriteLine("[Window_Closing] User chose to continue the game");
                            return;
                        }
                    }
                }

                // イベントの購読解除
                if (_componentManager != null)
                {
                    _componentManager.ComponentsFolderChanged -= OnComponentsFolderChanged;
                }

                // 隠しファイル表示設定を復元する（ゲーム終了時）
                try
                {
                    bool result = _hiddenFileSettingService?.RestoreHiddenFileDisplay() ?? false;
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Hidden file display settings restored: {result}");
                }
                catch (Exception hiddenFileEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Error restoring hidden file display: {hiddenFileEx.Message}");
                }

                // リソース解放
                _letterService?.Dispose();
                _componentManager?.Dispose();
                _pseudoDesktopIconManager?.Dispose();
                System.Diagnostics.Debug.WriteLine("PseudoDesktopIconManager disposed in MainWindow");

                _audioService?.Dispose();
                System.Diagnostics.Debug.WriteLine("AudioService disposed in MainWindow");

                // ファイルシステムのクリーンアップ（componentsフォルダ外のファイル削除）
                CleanupService.PerformExitCleanup();
                System.Diagnostics.Debug.WriteLine("Exit cleanup completed in MainWindow");

                // スナップショット差分の削除（ゲーム中に作成されたすべてのファイルを削除）
                try
                {
                    _snapshotService?.CleanupDifferences();
                    System.Diagnostics.Debug.WriteLine("[MainWindow] Snapshot differences cleaned up");
                }
                catch (Exception snapshotEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Error cleaning up snapshot differences: {snapshotEx.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Window_Closing: {ex.Message}");
            }
        }

        /// <summary>
        /// キーダウンイベント（デバッグ用）
        /// </summary>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            try
            {
                // F12キーでデバッグパネル切り替え
                if (e.Key == Key.F12)
                {
                    DebugPanel.Visibility = DebugPanel.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }

                base.OnPreviewKeyDown(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnKeyDown: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウの最前面表示を制御
        /// </summary>
        /// <param name="topmost">最前面に表示するかどうか</param>
        public void SetTopmost(bool topmost)
        {
            try
            {
                this.Topmost = topmost;
                if (topmost)
                {
                    this.Activate();
                    this.Focus();
                    System.Diagnostics.Debug.WriteLine("MainWindow set to topmost and activated");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MainWindow topmost disabled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting MainWindow topmost: {ex.Message}");
            }
        }

        // アクションハンドラからUI要素の可視性を制御するための新しいメソッド
        public void SetMainButtonVisibility(bool isVisible)
        {
            MainButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetKeyVisibility(bool isVisible)
        {
            KeyImage.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetDoorVisibility(bool isVisible)
        {
            DoorImage.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetTextWindowVisibility(bool isVisible)
        {
            // TextWindowの表示/非表示を設定
            TextWindow.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetBackgroundVisibility(bool isVisible)
        {
            // 背景画像の表示/非表示を設定
            BackgroundImage.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            
            // 背景画像が非表示のときはウィンドウを透明化
            // GameCanvasなどの必要な要素は不透明度を維持
            if (!isVisible)
            {
                System.Diagnostics.Debug.WriteLine("背景画像が非表示 - ウィンドウを透明化");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("背景画像が表示 - ウィンドウを不透明化");
            }
        }

        // 位置制御メソッド
        public void SetButtonPosition(double x, double y)
        {
            Canvas.SetLeft(MainButton, x);
            Canvas.SetTop(MainButton, y);
            System.Diagnostics.Debug.WriteLine($"Button position set to ({x}, {y})");
        }

        public void SetYESPosition(double x, double y)
        {
            Canvas.SetLeft(YesButton, x);
            Canvas.SetTop(YesButton, y);
            System.Diagnostics.Debug.WriteLine($"YES button position set to ({x}, {y})");
        }

        public void SetKeyPosition(double x, double y)
        {
            Canvas.SetLeft(KeyImage, x);
            Canvas.SetTop(KeyImage, y);
            System.Diagnostics.Debug.WriteLine($"Key position set to ({x}, {y})");
        }

        public void SetDoorPosition(double x, double y)
        {
            Canvas.SetLeft(DoorImage, x);
            Canvas.SetTop(DoorImage, y);
            System.Diagnostics.Debug.WriteLine($"Door position set to ({x}, {y})");
        }

        public void SetBackgroundPosition(double x, double y)
        {
            Canvas.SetLeft(BackgroundImage, x);
            Canvas.SetTop(BackgroundImage, y);
            System.Diagnostics.Debug.WriteLine($"Background position set to ({x}, {y})");

            // 背景位置変更イベントを発行
            var eventDispatcher = (System.Windows.Application.Current as App)?.GetEventDispatcher();
            eventDispatcher?.Dispatch(new BackgroundPositionChangedEvent(new System.Windows.Point(x, y), "Background"));

            // 背景が初期位置から動いた場合、Authorityフォルダを表示
            if (Math.Abs(x) > 10 || Math.Abs(y) > 10) // 10ピクセル以上動いた場合
            {
                ShowAuthorityFolder();
            }
            else
            {
                HideAuthorityFolder();
            }
        }

        /// <summary>
        /// Authorityフォルダを表示
        /// </summary>
        private void ShowAuthorityFolder()
        {
            var iconManager = GetPseudoDesktopIconManager();
            if (iconManager != null)
            {
                iconManager.ShowPseudoIcon("Authority");
                System.Diagnostics.Debug.WriteLine("Authority folder shown due to background position change");
            }
        }

        /// <summary>
        /// Authorityフォルダを非表示
        /// </summary>
        private void HideAuthorityFolder()
        {
            var iconManager = GetPseudoDesktopIconManager();
            if (iconManager != null)
            {
                iconManager.HidePseudoIcon("Authority");
                System.Diagnostics.Debug.WriteLine("Authority folder hidden due to background returning to initial position");
            }
        }

        /// <summary>
        /// 確実なウィンドウ前面表示（Windows 10/11対応）
        /// </summary>
        private void BringToForegroundReliable()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("BringToForegroundReliable: Starting reliable foreground process");
                
                // 1. ウィンドウが非表示の場合は表示
                if (!this.IsVisible)
                {
                    System.Diagnostics.Debug.WriteLine("Window is not visible, showing window");
                    this.Show();
                }
                
                // 2. 最小化されている場合は通常状態に復元
                if (this.WindowState == WindowState.Minimized)
                {
                    System.Diagnostics.Debug.WriteLine("Window is minimized, restoring to normal state");
                    this.WindowState = WindowState.Normal;
                }
                
                // 3. ウィンドウをアクティブ化
                this.Activate();
                
                // 4. Topmostトリック（最も重要 - Windows 10/11で確実に動作）
                System.Diagnostics.Debug.WriteLine("Applying Topmost trick for reliable foreground display");
                this.Topmost = true;
                this.Topmost = false;
                
                // 5. フォーカス設定
                this.Focus();
                
                // 6. 従来のWin32 API（補完的）
                var windowHelper = new System.Windows.Interop.WindowInteropHelper(this);
                var hWnd = windowHelper.Handle;
                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd);
                    System.Diagnostics.Debug.WriteLine("Applied Win32 APIs as fallback");
                }
                
                System.Diagnostics.Debug.WriteLine("BringToForegroundReliable: Window brought to foreground successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in BringToForegroundReliable: {ex.Message}");
            }
        }

        /// <summary>
        /// ダイアログメッセージキューをクリアし、タイピングを停止する
        /// </summary>
        public void ClearMessageQueue()
        {
            _dialogController?.ClearMessageQueue();
        }

        /// <summary>
        /// 入力フック初期化
        /// </summary>
        private void InitializeInputHooks()
        {
            _mouseProc = MouseHookProc;
            _keyboardProc = KeyboardHookProc;
        }

        /// <summary>
        /// マウスフックプロシージャ
        /// </summary>
        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isMouseDisabled)
            {
                return new IntPtr(1);
            }
            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// キーボードフックプロシージャ
        /// </summary>
        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isKeyboardDisabled)
            {
                return new IntPtr(1);
            }
            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// マウス入力を無効化
        /// </summary>
        public void DisableMouseInput()
        {
            if (!_isMouseDisabled)
            {
                _isMouseDisabled = true;
                using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        /// <summary>
        /// キーボード入力を無効化
        /// </summary>
        public void DisableKeyboardInput()
        {
            if (!_isKeyboardDisabled)
            {
                _isKeyboardDisabled = true;
                using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        /// <summary>
        /// マウス入力を復元
        /// </summary>
        public void EnableMouseInput()
        {
            if (_isMouseDisabled)
            {
                _isMouseDisabled = false;
                if (_mouseHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_mouseHook);
                    _mouseHook = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// キーボード入力を復元
        /// </summary>
        public void EnableKeyboardInput()
        {
            if (_isKeyboardDisabled)
            {
                _isKeyboardDisabled = false;
                if (_keyboardHook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_keyboardHook);
                    _keyboardHook = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// メッセージキューが空かどうかをチェック
        /// </summary>
        public bool IsMessageQueueEmpty()
        {
            return _dialogController?.IsMessageQueueEmpty() ?? true;
        }

        /// <summary>
        /// 現在タイピング中かどうかをチェック
        /// </summary>
        public bool IsTyping()
        {
            return _dialogController?.IsTyping() ?? false;
        }

        /// <summary>
        /// 全ての権限ファイルが揃っているかチェック
        /// </summary>
        private bool CheckAllPermissions()
        {
            if (_componentManager == null)
                return false;

            bool hasAdminRights = _componentManager.GetComponent("AdminRights") != null;
            bool hasFileAccess = _componentManager.GetComponent("FileAccess") != null;
            bool hasNetworkAccess = _componentManager.GetComponent("NetworkAccess") != null;
            bool hasSystemControl = _componentManager.GetComponent("SystemControl") != null;

            System.Diagnostics.Debug.WriteLine($"[CheckAllPermissions] AdminRights: {hasAdminRights}, FileAccess: {hasFileAccess}, NetworkAccess: {hasNetworkAccess}, SystemControl: {hasSystemControl}");

            return hasAdminRights && hasFileAccess && hasNetworkAccess && hasSystemControl;
        }

        /// <summary>
        /// PuzzleActionHandlerを取得
        /// </summary>
        private Core.Interfaces.IPuzzleActionHandler GetPuzzleActionHandler()
        {
            return _puzzleActionHandler;
        }

        /// <summary>
        /// アプリケーション終了時のクリーンアップ
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            EnableMouseInput();

            // Controllerのリソース解放
            _letterDisplayController?.Dispose();
            _dialogController?.Dispose();
            EnableKeyboardInput();
            base.OnClosed(e);
        }
    }
}