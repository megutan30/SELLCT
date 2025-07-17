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
using SELLCT.Core.Entities;
using SELLCT.Infrastructure.Services;
using SELLCT.Presentation.Views;
using SELLCT.Core.Interfaces;
using SELLCT.Application.Services;
using SELLCT.Core.Events;

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

        public void SetPhase2(bool value)
        {
            _isPhase2 = value;
        }

        public void SetAwaitingChoice(bool value)
        {
            _awaitingChoice = value;
        }
        private int _dialogStep = 0;

        private Queue<DialogItem> _dialogMessageQueue;
        private bool _isTyping;
        private DispatcherTimer _typingTimer;
        private string _currentFullMessage;
        private int _currentMessageCharIndex;
        private bool _awaitingChoice;

        private DispatcherTimer _initialLetterTimer;
        private DispatcherTimer _subsequentLetterTimer;
        
        // ボタンクリック処理の排他制御フラグ
        private bool _isButtonProcessing = false;

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
            _dialogMessageQueue = new Queue<DialogItem>();
            _typingTimer = new DispatcherTimer();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(50); // 1文字表示にかかる時間
            _typingTimer.Tick += TypingTimer_Tick;

            _initialLetterTimer = new DispatcherTimer();
            _initialLetterTimer.Interval = TimeSpan.FromSeconds(3);
            _initialLetterTimer.Tick += InitialLetterTimer_Tick;

            _subsequentLetterTimer = new DispatcherTimer();
            _subsequentLetterTimer.Interval = TimeSpan.FromSeconds(10);
            _subsequentLetterTimer.Tick += SubsequentLetterTimer_Tick;

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
                // ローディング表示
                LoadingOverlay.Visibility = Visibility.Visible;
                await Task.Delay(2000); // 初期化演出

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

            // PuzzleService初期化
            _puzzleActionHandler = new MainWindowPuzzleActionHandler(this, _componentManager, _metaGameController);
            _puzzleService = new PuzzleService(_componentManager, _puzzleActionHandler, (System.Windows.Application.Current as App).GetEventDispatcher());

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
            ShowDialogMessage(message);
        }

        /// <summary>
        /// 対話メッセージ表示
        /// </summary>
        public void ShowDialogMessage(params string[] messages)
        {
            // キューに追加可能かチェック
            if (!CanAddToQueue())
            {
                System.Diagnostics.Debug.WriteLine("[ShowDialogMessage] Cannot add to queue, ignoring messages");
                return;
            }

            foreach (var msg in messages)
            {
                EnqueueUniqueMessage(msg);
            }

            if (!_isTyping && !_awaitingChoice && TextWindow.Visibility == Visibility.Visible)
            {
                ProcessNextDialogMessage();
            }
            else if (TextWindow.Visibility != Visibility.Visible)
            {
                TextWindow.Visibility = Visibility.Visible;
                // グローバルクリックキャッチャーを廃止し、MainWindow_MouseDownで処理
                StartDialogFadeIn();
            }
        }

        /// <summary>
        /// 選択肢を表示
        /// </summary>
        public void ShowChoice()
        {
            _dialogMessageQueue.Enqueue(new DialogItem { Type = DialogItemType.Choice });
            if (!_isTyping && !_awaitingChoice)
            {
                ProcessNextDialogMessage();
            }
            else if (TextWindow.Visibility != Visibility.Visible)
            {
                // グローバルクリックキャッチャーを廃止し、MainWindow_MouseDownで処理
                StartDialogFadeIn();
            }
        }

        /// <summary>
        /// 次の対話メッセージを処理
        /// </summary>
        private void ProcessNextDialogMessage()
        {
            System.Diagnostics.Debug.WriteLine($"[ProcessNextDialogMessage] Queue count: {_dialogMessageQueue.Count}");

            if (_dialogMessageQueue.Any())
            {
                var nextItem = _dialogMessageQueue.Dequeue();
                System.Diagnostics.Debug.WriteLine($"[ProcessNextDialogMessage] Dequeued item Type: {nextItem.Type}, Message: {nextItem.Message}");

                if (nextItem.Type == DialogItemType.Text)
                {
                    _currentFullMessage = nextItem.Message;
                    _currentMessageCharIndex = 0;
                    DialogText.Text = string.Empty;
                    _isTyping = true;
                    _typingTimer.Start();

                    // 選択肢ボタンを非表示にする
                    ChoiceButtonsPanel.Visibility = Visibility.Collapsed;
                }
                else if (nextItem.Type == DialogItemType.Choice)
                {
                    // 選択肢を表示
                    ChoiceButtonsPanel.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Visible; // 個別ボタンの可視性を復元
                    NoButton.Visibility = Visibility.Visible; // 個別ボタンの可視性を復元
                    // テキストウィンドウは表示したまま
                    // グローバルクリックキャッチャーを廃止し、MainWindow_MouseDownで処理 
                    _awaitingChoice = true;
                    _typingTimer.Stop(); // テキストの自動進行を停止
                    System.Diagnostics.Debug.WriteLine("[ProcessNextDialogMessage] Choice displayed. YesButton: {YesButton.Visibility}, NoButton: {NoButton.Visibility}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ProcessNextDialogMessage] Message queue empty. Keeping dialog visible.");
                _isTyping = false;
                _typingTimer.Stop();
                _awaitingChoice = false;
            }
        }

        /// <summary>
        /// タイピングタイマーイベント
        /// </summary>
        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (_currentMessageCharIndex < _currentFullMessage.Length)
            {
                DialogText.Text += _currentFullMessage[_currentMessageCharIndex];
                _currentMessageCharIndex++;
            }
            else
            {
                _isTyping = false;
                _typingTimer.Stop();
                
                // タイピング完了後はクリック待ちとなる（自動進行を削除）
                System.Diagnostics.Debug.WriteLine("[TypingTimer_Tick] Typing completed. Waiting for click to continue.");
            }
        }

        /// <summary>
        /// メインウィンドウのマウスダウンイベント
        /// </summary>
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            System.Diagnostics.Debug.WriteLine($"[MainWindow_MouseDown] Click detected. _isTyping: {_isTyping}, _awaitingChoice: {_awaitingChoice}");

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

            // テキストウィンドウが表示されていない場合は何もしない
            if (TextWindow.Visibility != Visibility.Visible) return;

            // 選択肢表示中はクリックを無視
            if (_awaitingChoice) return;

            if (_isTyping)
            {
                // タイピング中の場合は残りのテキストを一気に表示
                DialogText.Text = _currentFullMessage;
                _currentMessageCharIndex = _currentFullMessage.Length;
                _isTyping = false;
                _typingTimer.Stop();
                System.Diagnostics.Debug.WriteLine("[MainWindow_MouseDown] Text typing skipped.");
            }
            else
            {
                // タイピング完了済みの場合は次のメッセージを処理
                ProcessNextDialogMessage();
                System.Diagnostics.Debug.WriteLine("[MainWindow_MouseDown] ProcessNextDialogMessage called.");
            }
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
                    // MainButton、YesButton、NoButtonは優先度が高い
                    if (button.Name == "MainButton" || button.Name == "YesButton" || button.Name == "NoButton")
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
        /// キューに重複しないメッセージを追加
        /// </summary>
        private void EnqueueUniqueMessage(string message)
        {
            // 現在表示中のメッセージと同じ場合は追加しない
            if (_currentFullMessage == message)
            {
                System.Diagnostics.Debug.WriteLine($"[EnqueueUniqueMessage] Current message duplicate ignored: {message}");
                return;
            }

            // キュー内に同じメッセージが既に存在する場合は追加しない
            if (_dialogMessageQueue.Any(item => item.Type == DialogItemType.Text && item.Message == message))
            {
                System.Diagnostics.Debug.WriteLine($"[EnqueueUniqueMessage] Queue duplicate ignored: {message}");
                return;
            }
            
            _dialogMessageQueue.Enqueue(new DialogItem { Type = DialogItemType.Text, Message = message });
            System.Diagnostics.Debug.WriteLine($"[EnqueueUniqueMessage] Message enqueued: {message}");
        }

        /// <summary>
        /// キューに追加可能かどうかを判定
        /// </summary>
        private bool CanAddToQueue()
        {
            // ボタン処理中は追加不可
            if (_isButtonProcessing)
            {
                System.Diagnostics.Debug.WriteLine("[CanAddToQueue] Button processing in progress, cannot add to queue");
                return false;
            }

            // 選択肢表示中は追加不可
            if (_awaitingChoice)
            {
                System.Diagnostics.Debug.WriteLine("[CanAddToQueue] Awaiting choice, cannot add to queue");
                return false;
            }

            return true;
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

                // 最初の手紙タイマーを開始
                if (!_initialLetterTimer.IsEnabled && _letterService.CurrentLetterIndex == 0)
                {
                    _initialLetterTimer.Start();
                    StatusText.Text = "手紙の到着を待っています...";
                }

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

                        ShowDialogMessage("ファイルアクセス権限を取得しました。\n最後の障壁を取り除いてください。\nButton.componentを削除してください。");
                    }
                }
                else if (!IsKnownButtonText(buttonText))
                {
                    // 未設定の名前の場合、テキストウィンドウに表示
                    HandleUnknownButtonClick(buttonText);
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
        /// 未知のボタンクリック処理
        /// </summary>
        private void HandleUnknownButtonClick(string buttonText)
        {
            // 排他制御チェック
            if (_isButtonProcessing)
            {
                System.Diagnostics.Debug.WriteLine($"[HandleUnknownButtonClick] Button processing in progress, ignoring click: {buttonText}");
                return;
            }

            // キューに追加可能かチェック
            if (!CanAddToQueue())
            {
                System.Diagnostics.Debug.WriteLine($"[HandleUnknownButtonClick] Cannot add to queue, ignoring click: {buttonText}");
                return;
            }

            if (TextWindow.Visibility != Visibility.Visible) return;

            try
            {
                // ボタン処理開始
                _isButtonProcessing = true;
                System.Diagnostics.Debug.WriteLine($"[HandleUnknownButtonClick] Starting button processing: {buttonText}");

                // テキストウィンドウを表示
                SetTextWindowVisibility(true);
                
                string message1 = $"ボタン名前を変えられるみたいですが、\n";
                string message2 = $"どうやら'{buttonText}'機能はないようですね。\n";
                
                // 重複チェックを使用してキューに追加
                EnqueueUniqueMessage(message1);
                EnqueueUniqueMessage(message2);
                
                StatusText.Text = $"'{buttonText}'機能について説明を表示しました";
                
                System.Diagnostics.Debug.WriteLine($"Unknown button clicked: {buttonText}");

                // キューの処理を開始
                if (!_isTyping && !_awaitingChoice && _dialogMessageQueue.Count > 0)
                {
                    ProcessNextDialogMessage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HandleUnknownButtonClick: {ex.Message}");
            }
            finally
            {
                // ボタン処理終了（少し遅延を設けて重複クリックを防ぐ）
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(500);
                timer.Tick += (s, e) =>
                {
                    _isButtonProcessing = false;
                    timer.Stop();
                    System.Diagnostics.Debug.WriteLine($"[HandleUnknownButtonClick] Button processing completed: {buttonText}");
                };
                timer.Start();
            }
        }

        /// <summary>
        /// 最初の手紙タイマーイベント
        /// </summary>
        private void InitialLetterTimer_Tick(object sender, EventArgs e)
        {
            _initialLetterTimer.Stop();
            _letterService.ShowNextLetter();
        }

        /// <summary>
        /// その後の手紙タイマーイベント
        /// </summary>
        private void SubsequentLetterTimer_Tick(object sender, EventArgs e)
        {
            _subsequentLetterTimer.Stop();
            _letterService.ShowNextLetter();
        }

        /// <summary>
        /// 手紙クリック
        /// </summary>
        private void Letter_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var letterIndex = _letterService?.CurrentLetterIndex ?? 1;
                if (_letterService == null) return;

                // Immediately hide the letter
                LetterImage.Visibility = Visibility.Collapsed;

                // Handle the download and check for success
                bool success = _letterService.OnLetterClicked(letterIndex);

                if (success)
                {
                    // ダウンロード成功後、次の手紙のタイマーを開始
                    if (!_letterService.IsSequenceComplete)
                    {
                        _subsequentLetterTimer.Start();
                        StatusText.Text = "次の手紙の到着を待っています...";
                    }
                }
                else
                {
                    // If failed or cancelled, show the letter again
                    LetterImage.Visibility = Visibility.Visible;
                    StatusText.Text = "手紙のダウンロードがキャンセルされました";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Letter_Click: {ex.Message}");
                // Show the letter again in case of an unexpected error
                LetterImage.Visibility = Visibility.Visible;
            }
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
            _dialogMessageQueue.Clear();
            _isTyping = false;
            _typingTimer.Stop();
            DialogText.Text = string.Empty;
            _awaitingChoice = false;
            // TextWindow全体（枠を含む）を確実に非表示にする
            TextWindow.Visibility = Visibility.Collapsed;
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
            return _dialogMessageQueue?.Count == 0;
        }

        /// <summary>
        /// 現在タイピング中かどうかをチェック
        /// </summary>
        public bool IsTyping()
        {
            return _isTyping;
        }

        /// <summary>
        /// アプリケーション終了時のクリーンアップ
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            EnableMouseInput();
            EnableKeyboardInput();
            base.OnClosed(e);
        }
    }
}