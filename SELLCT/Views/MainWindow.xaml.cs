using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SELLCT.Models;
using SELLCT.Services;

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
        private GameState _currentState = GameState.G1;
        private bool _isPhase2 = false;
        private int _dialogStep = 0;

        /// <summary>
        private bool _isTextWindowEnabled = false;

        /// <summary>
        /// ゲーム状態列挙型
        /// </summary>
        public enum GameState
        {
            G1, // 通常状態（背景＋ボタンのみ）
            G2, // 手紙出現状態（G-1 + 手紙画像要素を表示）  
            G3, // 対話ウィンドウ状態（G-1 or G-2 + 白い枠のテキストウィンドウ要素を表示）
            G4  // KEY出現状態（背景 + KEY画像要素を表示、ボタンは非表示）
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
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
                SetGameState(GameState.G1);
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

        /// <summary>
        /// サービス初期化
        /// </summary>
        private void InitializeServices()
        {
            // ComponentManager初期化
            _componentManager = new ComponentManager();

            // LetterService初期化
            _letterService = new LetterService();
            _letterService.LetterAppeared += OnLetterAppeared;
            _letterService.LetterClicked += OnLetterClicked;

            // MetaGameController初期化
            _metaGameController = new MetaGameController();

            // PuzzleService初期化
            _puzzleService = new PuzzleService(_componentManager);
            _puzzleService.OnPuzzleAction += OnPuzzleAction;

            // 手紙シーケンス開始
            _letterService.StartLetterSequence();

            System.Diagnostics.Debug.WriteLine("Services initialized");
        }

        /// <summary>
        /// ゲーム状態設定
        /// </summary>
        private void SetGameState(GameState newState)
        {
            _currentState = newState;

            switch (newState)
            {
                case GameState.G1:
                    // 通常状態
                    MainButton.Visibility = Visibility.Visible;
                    LetterImage.Visibility = Visibility.Collapsed;
                    KeyImage.Visibility = Visibility.Collapsed;
                    // 対話ウィンドウは既存状態を維持
                    break;

                case GameState.G2:
                    // 手紙出現状態
                    MainButton.Visibility = Visibility.Visible;
                    LetterImage.Visibility = Visibility.Visible;
                    KeyImage.Visibility = Visibility.Collapsed;
                    // 手紙アニメーション開始
                    StartLetterAnimation();
                    break;

                case GameState.G3:
                    // 対話ウィンドウ状態
                    DialogWindow.Visibility = Visibility.Visible;
                    StartDialogFadeIn();
                    break;

                case GameState.G4:
                    // KEY出現状態
                    MainButton.Visibility = Visibility.Collapsed;
                    LetterImage.Visibility = Visibility.Collapsed;
                    KeyImage.Visibility = Visibility.Visible;
                    // KEYアニメーション開始
                    StartKeyAnimation();
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"Game state changed to: {newState}");
            UpdateDebugInfo();
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

        /// <summary>
        /// パズルアクションイベントハンドラー
        /// </summary>
        private void OnPuzzleAction(object sender, PuzzleAction action)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Executing puzzle action: {action.Type}");

                    switch (action.Type)
                    {
                        case PuzzleAction.ActionType.ShowDialog:
                            ShowDialogMessage(action.Message);
                            break;
                        case PuzzleAction.ActionType.ChangeMainButtonContent:
                            MainButton.Content = action.NewContent;
                            StatusText.Text = $"{action.NewContent}機能が有効になりました";
                            break;
                        case PuzzleAction.ActionType.RevealHiddenItem:
                            _componentManager.RevealHiddenItem(action.HiddenItemFolder, action.TargetComponent, action.HiddenItemDisplayName);
                            StatusText.Text = $"隠しアイテムが出現: {action.HiddenItemDisplayName}";
                            DisplayMessage($"🎉 隠しアイテム発見！" +
                                $"「{action.HiddenItemDisplayName}」が出現しました！" +
                                "これがSELLCTのメタゲーム機能です。" +
                                "あなたの行動によって隠されていた要素が現れました。",
                                "SELLCT - 隠しアイテム発見");
                            break;
                        case PuzzleAction.ActionType.TransitionToPhase2:
                            TransitionToPhase2();
                            break;
                        case PuzzleAction.ActionType.ShowMessageBox:
                            MessageBox.Show(action.Message, "SELLCT", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case PuzzleAction.ActionType.ShowNoButton:
                            NoButton.Visibility = Visibility.Visible;
                            StatusText.Text = "NO選択肢が追加されました";
                            break;
                    }

                    UpdateComponentCount();
                    UpdateDebugInfo();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnPuzzleAction: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 手紙出現イベントハンドラー
        /// </summary>
        private void OnLetterAppeared(object sender, int letterIndex)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Letter {letterIndex} appeared");

                    // G-1→G-2: 手紙要素を表示
                    if (_currentState == GameState.G1)
                    {
                        SetGameState(GameState.G2);
                    }
                    else if (_currentState == GameState.G3)
                    {
                        // 対話ウィンドウがある状態で手紙出現
                        LetterImage.Visibility = Visibility.Visible;
                        StartLetterAnimation();
                    }

                    StatusText.Text = $"SELLCTからの手紙 {letterIndex} が到着しました";
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
        private void OnLetterClicked(object sender, int letterIndex)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // This event now only fires on successful download.
                    // The letter's visibility is managed by the Letter_Click handler.

                    // Update game state
                    if (DialogWindow.Visibility == Visibility.Visible)
                    {
                        _currentState = GameState.G3;
                    }
                    else
                    {
                        _currentState = GameState.G1;
                    }

                    StatusText.Text = $"手紙 {letterIndex} をダウンロードしました";
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
        private void TransitionToPhase2()
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
                _metaGameController.StartPhase2();
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
        private void DisplayMessage(string message, string title = "SELLCT")
        {
            if (_isTextWindowEnabled)
            {
                ShowDialogMessage(message);
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 対話メッセージ表示
        /// </summary>
        private void ShowDialogMessage(string message)
        {
            // 対話ウィンドウが表示されていない場合は表示する
            if (DialogWindow.Visibility != Visibility.Visible)
            {
                SetGameState(GameState.G3);
            }

            if (DialogWindow.Visibility == Visibility.Visible)
            {
                DialogText.Text = message;
                _dialogStep++;
            }
        }

        /// <summary>
        /// 構成要素数更新
        /// </summary>
        private void UpdateComponentCount()
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
        private void UpdateDebugInfo()
        {
            try
            {
                var componentCount = _componentManager?.Components.Count ?? 0;
                var letterCount = _letterService?.CurrentLetterIndex ?? 0;
                var totalLetters = 6; // 手紙の総数
                var phase = _isPhase2 ? 2 : 1;

                DebugText.Text = $"State: {_currentState}\n" +
                               $"Components: {componentCount}\n" +
                               $"Letters: {letterCount}/{totalLetters}\n" +
                               $"Phase: {phase}\n" +
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

                if (!success)
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

                // 対話ウィンドウがある場合はG-3、ない場合はG-1（背景のみ）
                if (DialogWindow.Visibility == Visibility.Visible)
                {
                    _currentState = GameState.G3;
                    ShowDialogMessage("鍵を手に入れました！\nでも、真の解放のためには...\nGameWindow.componentを削除してください。");
                }
                else
                {
                    _currentState = GameState.G1;
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
            try
            {
                ShowDialogMessage("ありがとうございます！\nあなたは私を助けてくれるのですね。\n次の指示をお待ちください。");
                StatusText.Text = "YESが選択されました";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in YesButton_Click: {ex.Message}");
            }
        }

        /// <summary>
        /// NOボタンクリック
        /// </summary>
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowDialogMessage("そうですか...残念です。\nでも、きっと心を変えてくれると信じています。\nいつでもお待ちしています。");
                StatusText.Text = "NOが選択されました";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NoButton_Click: {ex.Message}");
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