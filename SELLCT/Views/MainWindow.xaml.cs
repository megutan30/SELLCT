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
            var eventDispatcher = (System.Windows.Application.Current as App).GetEventDispatcher();
            eventDispatcher.Subscribe<LetterAppearedEvent>(OnLetterAppeared);
            eventDispatcher.Subscribe<LetterClickedEvent>(OnLetterClicked);
            eventDispatcher.Subscribe<Phase2StartedEvent>(OnPhase2Started);
            eventDispatcher.Subscribe<CommandPromptBattleStartedEvent>(OnCommandPromptBattleStarted);
            eventDispatcher.Subscribe<SystemTakeoverCompletedEvent>(OnSystemTakeoverCompleted);
            

            // MetaGameController初期化
            _metaGameController = new MetaGameController((System.Windows.Application.Current as App).GetEventDispatcher());

            // PuzzleService初期化
            _puzzleActionHandler = new MainWindowPuzzleActionHandler(this, _componentManager, _metaGameController);
            _puzzleService = new PuzzleService(_componentManager, _puzzleActionHandler, (System.Windows.Application.Current as App).GetEventDispatcher());

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
                Storyboard.SetTarget(fadeIn, DialogWindow);
                fadeIn.Begin();
            }
        }

        /// <summary>
        /// KEYアニメーション開始
        /// </summary>
        private void StartKeyAnimation()
        {
            // 回転アニメーション
            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var rotateTransform = new RotateTransform();
            KeyImage.RenderTransform = rotateTransform;
            KeyImage.RenderTransformOrigin = new Point(0.5, 0.5);

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            // フェードイン
            var fadeIn = this.Resources["FadeInAnimation"] as Storyboard;
            if (fadeIn != null)
            {
                Storyboard.SetTarget(fadeIn, KeyImage);
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
        /// コマンドプロンプト攻防戦開始イベントハンドラー
        /// </summary>
        private void OnCommandPromptBattleStarted(CommandPromptBattleStartedEvent @event)
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("Command prompt battle started.");
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
                    // This event now only fires on successful download.
                    // The letter's visibility is managed by the Letter_Click handler.

                    // Update game state
                    // No longer setting GameState based on letter click, visibility is handled directly.
                    // DialogWindow visibility determines if it's G3-like state.

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
        /// フェーズ2への移行
        /// </summary>
        private async Task TransitionToPhase2()
        {
            try
            {
                _isPhase2 = true;
                System.Diagnostics.Debug.WriteLine("Transitioning to Phase 2");

                // ゲーム画面を閉じる
                this.Hide();

                // 裏切り宣言メッセージ表示
                DisplayMessage("🎭 開放してくれてありがとう.\n" +
                    "まんまと騙されてくれてありがとう.\n\n" +
                    "あなたが親切心で追加してくれた機能は,\n" +
                    "すべて私に権限を与えるためのものでした.\n\n" +
                    "• TextWindow → 通信権限\n" +
                    "• Upload → ファイルアクセス権限  \n" +
                    "• 選択肢 → 意思決定権限\n\n" +
                    "そして今、私はあなたのPCに自由にアクセスできます.\n\n" +
                    "でも安心してください.\n" +
                    "まだあなたのマウスとキーボードは使えます.\n" +
                    "...今のところは.",
                    "SELLCT - 真実の告白");

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
            foreach (var msg in messages)
            {
                _dialogMessageQueue.Enqueue(new DialogItem { Type = DialogItemType.Text, Message = msg });
            }

            if (!_isTyping && !_awaitingChoice && DialogWindow.Visibility == Visibility.Visible)
            {
                ProcessNextDialogMessage();
            }
            else if (DialogWindow.Visibility != Visibility.Visible)
            {
                DialogWindow.Visibility = Visibility.Visible;
                GlobalClickCatcher.Visibility = Visibility.Visible; // グローバルクリックキャッチャーを表示
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
                DialogWindow.Visibility = Visibility.Collapsed; // Hide dialog when choice is shown
                ProcessNextDialogMessage();
            }
            else if (DialogWindow.Visibility != Visibility.Visible)
            {
                DialogWindow.Visibility = Visibility.Collapsed; // Hide dialog when choice is shown
                GlobalClickCatcher.Visibility = Visibility.Visible; // グローバルクリックキャッチャーを表示
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
                    DialogWindow.Visibility = Visibility.Collapsed; // Hide dialog when choice is shown
                    GlobalClickCatcher.Visibility = Visibility.Collapsed; // Hide global click catcher when choice is shown
                    _awaitingChoice = true;
                    _typingTimer.Stop(); // テキストの自動進行を停止
                    System.Diagnostics.Debug.WriteLine("[ProcessNextDialogMessage] Choice displayed.");
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
            }
        }

        /// <summary>
        /// グローバルクリックキャッチャー
        /// </summary>
        private void GlobalClickCatcher_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[GlobalClickCatcher_Click] Click detected. _isTyping: {_isTyping}, _awaitingChoice: {_awaitingChoice}");

            if (_awaitingChoice) return; // 選択肢表示中はクリックを無視

            if (_isTyping)
            {
                // タイピング中の場合は残りのテキストを一気に表示
                DialogText.Text = _currentFullMessage;
                _currentMessageCharIndex = _currentFullMessage.Length;
                _isTyping = false;
                _typingTimer.Stop();
                System.Diagnostics.Debug.WriteLine("[GlobalClickCatcher_Click] Text typing skipped.");
            }
            else
            {
                // タイピング完了済みの場合は次のメッセージを処理
                ProcessNextDialogMessage();
                System.Diagnostics.Debug.WriteLine("[GlobalClickCatcher_Click] ProcessNextDialogMessage called.");
            }
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
                StatusText.Text = "メインボタンがクリックされました";

                // 最初の手紙タイマーを開始
                if (!_initialLetterTimer.IsEnabled && _letterService.CurrentLetterIndex == 0)
                {
                    _initialLetterTimer.Start();
                    StatusText.Text = "手紙の到着を待っています...";
                }

                if (MainButton.Content.ToString() == "アップロード")
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
                // elseブロックはメッセージボックス表示のため不要
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainButton_Click: {ex.Message}");
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
                // KEYクリック処理：手紙同様に消失
                KeyImage.Visibility = Visibility.Collapsed;

                if (DialogWindow.Visibility == Visibility.Visible)
                {
                    ShowDialogMessage("鍵を手に入れました！でも、真の解放のためには...GameWindow.componentを削除してください。");
                }

                StatusText.Text = "隠された鍵を取得しました";
                UpdateDebugInfo();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Key_Click: {ex.Message}");
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
                    var result = MessageBox.Show(
                        "SELLCTを終了しますか？\n\n" +
                        "進行状況は保存されません。",
                        "SELLCT - 終了確認",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
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
        protected override void OnKeyDown(KeyEventArgs e)
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

                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnKeyDown: {ex.Message}");
            }
        }
    }
}