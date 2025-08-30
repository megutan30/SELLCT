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
using System.Runtime.InteropServices;
using System.Text;
using SELLCT.Core.Entities;
using SELLCT.Infrastructure.Services;
using SELLCT.Presentation.Views;
using SELLCT.Core.Interfaces;
using SELLCT.Application.Services;
using SELLCT.Core.Events;
using SELLCT.Presentation.Controllers;

namespace SELLCT.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private ComponentManager _componentManager;
        private LetterService _letterService;
        private KeyService _keyService;
        private MetaGameController _metaGameController;
        private bool _isPhase2 = false;

        // 統合されたコントローラー
        private LetterDisplayController _letterDisplayController;
        private DialogController _dialogController;

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
                -(borderThickness.Top + captionHeight-5),
                -borderThickness.Right,
                -borderThickness.Bottom
            );
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
                // SmartScreen警告を表示（教育目的）
                var smartScreenResult = Infrastructure.Services.SmartScreenWarningService.ShowSmartScreenWarning("SELLCT.exe", "不明な発行者");
                
                if (smartScreenResult == Infrastructure.Services.SmartScreenResult.DontRun)
                {
                    // ユーザーが実行しないを選択した場合、アプリケーションを終了
                    System.Windows.Application.Current.Shutdown();
                    return;
                }

                // 実行が選択された場合、警告演出を開始
                if (smartScreenResult == Infrastructure.Services.SmartScreenResult.RunAnyway)
                {
                    System.Diagnostics.Debug.WriteLine("Starting warning flood demonstration with noise transition...");
                    
                    var warningFloodService = new Infrastructure.Services.WarningFloodService();
                    
                    // ノイズトランジション付きの警告演出を実行
                    await warningFloodService.StartWarningFloodWithNoiseTransition(() =>
                    {
                        // ノイズトランジション完了後にメインウィンドウを表示
                        System.Diagnostics.Debug.WriteLine("Showing main window after noise transition...");
                        this.Show();
                        
                        // メインウィンドウを最前面に固定
                        this.SetTopmost(true);
                    });
                    
                    System.Diagnostics.Debug.WriteLine("Warning flood with noise transition completed.");
                    
                    // リソースクリーンアップ
                    warningFloodService.Cleanup();
                }
                else
                {
                    // SmartScreen で実行しないが選択された場合以外は、通常のMainWindow表示
                    System.Diagnostics.Debug.WriteLine("Showing main window directly...");
                    this.Show();
                }

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

            // Controller初期化
            _letterDisplayController = new LetterDisplayController(_letterService, eventDispatcher);
            _dialogController = new DialogController(_componentManager);
            
            // UI要素をControllerに注入
            _letterDisplayController.InjectUIElements(LetterImage, StatusText);
            _dialogController.InjectUIElements(
                TextWindow, DialogText, ChoiceButtonsPanel, 
                YesButton, NoButton, LogPanel, LogText, LogScrollViewer);

            // PuzzleService初期化
            _puzzleActionHandler = new MainWindowPuzzleActionHandler(this, _componentManager, _metaGameController);
            
            // GameStateを共有するために先に作成
            var gameState = new GameState();
            _puzzleService = new PuzzleService(_componentManager, _puzzleActionHandler, eventDispatcher, gameState);
            
            // DialogueService初期化（GameStateを共有）
            _dialogueService = new DialogueService(_componentManager, _puzzleActionHandler, eventDispatcher, gameState);
            _puzzleActionHandler.SetDialogueService(_dialogueService);

            // ComponentsFolderChangedイベントをサブスクライブ
            _componentManager.ComponentsFolderChanged += OnComponentsFolderChanged;

            // 手紙シーケンス開始
            _letterService.StartLetterSequence();

            System.Diagnostics.Debug.WriteLine("Services initialized");
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
        /// componentsフォルダ変更イベントハンドラー（ウィンドウを最前面に表示）
        /// </summary>
        private void OnComponentsFolderChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Components folder changed, bringing window to foreground");
                    
                    // ウィンドウハンドルを取得
                    var windowHelper = new System.Windows.Interop.WindowInteropHelper(this);
                    var hWnd = windowHelper.Handle;
                    
                    if (hWnd != IntPtr.Zero)
                    {
                        // ウィンドウが最小化されている場合は復元
                        ShowWindow(hWnd, SW_RESTORE);
                        
                        // ウィンドウを最前面に表示
                        SetForegroundWindow(hWnd);
                        
                        System.Diagnostics.Debug.WriteLine("Window brought to foreground successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to get window handle");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error bringing window to foreground: {ex.Message}");
                }
            });
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
                    StatusText.Text = "PassWord.txtをダウンロードしました";
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
                    // MainButton、YesButton、NoButton、LogButton、CloseLogButtonは優先度が高い
                    if (button.Name == "MainButton" || button.Name == "YesButton" || button.Name == "NoButton" || 
                        button.Name == "LogButton" || button.Name == "CloseLogButton")
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
                    StatusText.Text = "PassWord.txtをダウンロードしました";
                    if (TextWindow.Visibility == Visibility.Visible)
                    {
                        ShowDialogMessage("パスワードファイルをダウンロードしました！真の解放のためには...GameWindow.componentを削除してください。");
                    }
                    UpdateDebugInfo();
                }
                else
                {
                    // ダウンロード失敗またはキャンセル時は鍵を再表示
                    KeyImage.Visibility = Visibility.Visible;
                    StatusText.Text = "パスワードファイルのダウンロードがキャンセルされました";
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
        /// YESボタンクリック
        /// </summary>
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            _puzzleActionHandler.HandleYesClick();
        }

        /// <summary>
        /// NOボタンクリック
        /// </summary>
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
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
        /// ウィンドウクローズ処理
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (!_isPhase2)
                {
                    //var result = MessageBox.Show(
                    //    "SELLCTを終了しますか？\n\n" +
                    //    "進行状況は保存されません。",
                    //    "SELLCT - 終了確認",
                    //    MessageBoxButton.YesNo,
                    //    MessageBoxImage.Question);

                    //if (result == MessageBoxResult.No)
                    //{
                    //    e.Cancel = true;
                    //    return;
                    //}
                }

                // イベントの購読解除
                if (_componentManager != null)
                {
                    _componentManager.ComponentsFolderChanged -= OnComponentsFolderChanged;
                }

                // リソース解放
                _letterService?.Dispose();
                _componentManager?.Dispose();
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

        public void SetTextWindowVisibility(bool isVisible)
        {
            // TextWindowの表示/非表示を設定
            TextWindow.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetBackgroundVisibility(bool isVisible)
        {
            // 背景画像の表示/非表示を設定
            BackgroundImage.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
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