using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using SELLCT.Infrastructure.Services;

namespace SELLCT.Views
{
    /// <summary>
    /// ノイズトランジション演出の段階
    /// </summary>
    public enum NoisePhase
    {
        Initial,      // 0-6秒: 間欠的ノイズ
        Increasing,   // 6-10秒: 頻度上昇  
        Frequent,     // 10-13秒: 高頻度
        Constant,     // 13-15秒: 常時表示
        Maintain,     // 15-17秒: 維持期
        Clear         // 17-19秒: クリア
    }

    /// <summary>
    /// 画面縮小演出用ウィンドウ
    /// 教育目的で「PC画面自体が縮小する」不思議な体験を提供
    /// </summary>
    public partial class ScreenShrinkWindow : Window
    {
        private TaskCompletionSource<bool> _animationCompletionSource;
        private bool _isAnimationCompleted = false;
        private Action _cleanupAction;
        private Action _mainWindowShowAction;
        private DispatcherTimer _noiseTimer;
        private DispatcherTimer _noisePatternTimer;
        private NoisePhase _currentPhase;
        private DateTime _animationStartTime;
        private bool _isNoiseVisible = false;
        private Random _random = new Random();

        public ScreenShrinkWindow()
        {
            InitializeComponent();
            
            // 正確な画面サイズに設定
            SetWindowSize();
            
            Loaded += ScreenShrinkWindow_Loaded;
        }

        /// <summary>
        /// ウィンドウロード時の処理（新フローでは無効化 - StartShrinkAnimationAsync()から処理開始）
        /// </summary>
        private async void ScreenShrinkWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 新しいフローではStartShrinkAnimationAsync()から直接処理を開始するため
            // Loadedイベントでの処理は実行しない（二重アニメーションを防ぐ）
            System.Diagnostics.Debug.WriteLine("ScreenShrinkWindow loaded - skipping legacy flow to prevent double animation");
            return;
            
            /* 以下は旧フロー（無効化済み）
            try
            {
                System.Diagnostics.Debug.WriteLine("ScreenShrinkWindow loaded, capturing screen...");
                
                // 少し待ってからスクリーンショットを撮る（ウィンドウが完全に表示される前に）
                await Task.Delay(100);
                
                // スクリーンショットを撮影
                await CaptureAndSetScreenshot();
                
                // 縮小アニメーション開始前の待機時間
                await Task.Delay(1000); // スクリーンショット撮影完了を待つ
                
                // 待機時間中にクリーンアップ処理を実行
                if (_cleanupAction != null)
                {
                    System.Diagnostics.Debug.WriteLine("Executing cleanup action during wait period...");
                    _cleanupAction.Invoke();
                }
                
                // さらに少し待機してから縮小アニメーション開始
                await Task.Delay(1000);
                StartShrinkAnimation();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ScreenShrinkWindow_Loaded: {ex.Message}");
                // エラー時は演出をスキップ
                CompleteAnimation();
            }
            */
        }

        /// <summary>
        /// ウィンドウサイズを画面に合わせて設定（DPI対応）
        /// </summary>
        private void SetWindowSize()
        {
            try
            {
                // Win32 APIから物理ピクセルサイズを取得
                var physicalScreenSize = ScreenCaptureService.GetScreenSize();
                System.Diagnostics.Debug.WriteLine($"Physical screen size: {physicalScreenSize.Width}x{physicalScreenSize.Height}");
                
                // WPFのDPI情報を取得
                var dpiScale = VisualTreeHelper.GetDpi(this);
                System.Diagnostics.Debug.WriteLine($"DPI scale: X={dpiScale.DpiScaleX}, Y={dpiScale.DpiScaleY}");
                
                // DIPサイズに変換（Device Independent Pixels）
                var dipWidth = physicalScreenSize.Width / dpiScale.DpiScaleX;
                var dipHeight = physicalScreenSize.Height / dpiScale.DpiScaleY;
                
                System.Diagnostics.Debug.WriteLine($"Setting window DIP size: {dipWidth}x{dipHeight}");
                
                this.Width = dipWidth;
                this.Height = dipHeight;
                this.Left = 0;
                this.Top = 0;
                this.WindowState = WindowState.Normal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting window size: {ex.Message}");
                // エラー時はMaximized fallback
                this.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// スクリーンショットを撮影して画像に設定
        /// </summary>
        private async Task CaptureAndSetScreenshot()
        {
            try
            {
                // ウィンドウは最初から非表示なのでHide()は不要
                System.Diagnostics.Debug.WriteLine("Capturing screenshot (window already hidden)...");
                
                // 少し待機してから撮影（UIの安定化）
                await Task.Delay(100);
                
                // スクリーンショット撮影
                var screenshot = ScreenCaptureService.CaptureScreen();
                
                if (screenshot != null)
                {
                    System.Diagnostics.Debug.WriteLine("Screenshot captured successfully");
                    
                    // メインスレッドで画像を設定し、ウィンドウを初回表示
                    Dispatcher.Invoke(() =>
                    {
                        ScreenImage.Source = screenshot;
                        
                        // ウィンドウを初回表示（黒背景とスクリーンショット表示）
                        this.Show();
                        
                        // ウィンドウサイズを設定
                        SetWindowSize();
                    });
                    
                    // 少し待機してからクリーンアップ処理を実行（警告消去）
                    await Task.Delay(500);
                    
                    // クリーンアップ処理実行（残っている警告を消去）
                    if (_cleanupAction != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Executing cleanup action - closing remaining warning dialogs...");
                        _cleanupAction.Invoke();
                        await Task.Delay(1000); // クリーンアップ完了待機
                    }
                    
                    // 縮小アニメーション開始
                    Dispatcher.Invoke(() =>
                    {
                        StartShrinkAnimation();
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to capture screenshot");
                    // エラー時でも表示して処理を続行
                    Dispatcher.Invoke(() =>
                    {
                        this.Show();
                        SetWindowSize();
                    });
                    
                    // エラー時でもクリーンアップ処理は実行
                    if (_cleanupAction != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Executing cleanup action (error case) - closing remaining warning dialogs...");
                        _cleanupAction.Invoke();
                        await Task.Delay(1000);
                    }
                    
                    Dispatcher.Invoke(() =>
                    {
                        StartShrinkAnimation();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error capturing screenshot: {ex.Message}");
                // エラー時でも表示して処理を続行
                Dispatcher.Invoke(() =>
                {
                    this.Show();
                    SetWindowSize();
                });
                
                // エラー時でもクリーンアップ処理は実行
                if (_cleanupAction != null)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Executing cleanup action (exception case) - closing remaining warning dialogs...");
                        _cleanupAction.Invoke();
                        await Task.Delay(1000);
                    }
                    catch
                    {
                        // クリーンアップでエラーが発生しても続行
                    }
                }
                
                Dispatcher.Invoke(() =>
                {
                    StartShrinkAnimation();
                });
            }
        }

        /// <summary>
        /// 縮小アニメーションを開始
        /// </summary>
        public Task StartShrinkAnimationAsync()
        {
            _animationCompletionSource = new TaskCompletionSource<bool>();
            
            // スクリーンショット撮影を開始（非同期）
            Task.Run(async () =>
            {
                await CaptureAndSetScreenshot();
            });
            
            return _animationCompletionSource.Task;
        }

        /// <summary>
        /// 縮小アニメーションを開始（スクリーンショット済みの場合）
        /// </summary>
        public Task StartShrinkAnimationWithoutCaptureAsync()
        {
            _animationCompletionSource = new TaskCompletionSource<bool>();
            
            // スクリーンショットは既に設定済みのため、直接アニメーション開始
            Task.Run(async () =>
            {
                await Task.Delay(500); // 少し待機してからアニメーション開始
                
                Dispatcher.Invoke(() =>
                {
                    StartShrinkAnimation();
                });
            });
            
            return _animationCompletionSource.Task;
        }

        /// <summary>
        /// 縮小アニメーション開始
        /// </summary>
        private void StartShrinkAnimation()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting shrink animation with synchronized noise transition...");
                
                // アニメーション開始時刻を記録
                _animationStartTime = DateTime.Now;
                _currentPhase = NoisePhase.Initial;
                
                // メインゲーム画面サイズ（論理ピクセル）
                var screenSize = ScreenCaptureService.GetScreenSize();
                var gameWindowWidth = 800.0;  // MainWindow.xaml の Width (DIP)
                var gameWindowHeight = 600.0; // MainWindow.xaml の Height (DIP)
                
                // WPFのDPI情報を取得
                var dpiScale = VisualTreeHelper.GetDpi(this);
                System.Diagnostics.Debug.WriteLine($"DPI scale: X={dpiScale.DpiScaleX:F3}, Y={dpiScale.DpiScaleY:F3}");
                
                // 論理ピクセル（DIP）を物理ピクセルに変換
                var physicalGameWidth = gameWindowWidth * dpiScale.DpiScaleX;
                var physicalGameHeight = gameWindowHeight * dpiScale.DpiScaleY;
                
                System.Diagnostics.Debug.WriteLine($"MainWindow physical size: {physicalGameWidth:F0}x{physicalGameHeight:F0} (from {gameWindowWidth}x{gameWindowHeight} DIP)");
                System.Diagnostics.Debug.WriteLine($"Screen physical size: {screenSize.Width}x{screenSize.Height}");
                
                // 物理ピクセル同士で正確な縮小比率を計算
                var scaleX = physicalGameWidth / screenSize.Width;
                var scaleY = physicalGameHeight / screenSize.Height;
                
                System.Diagnostics.Debug.WriteLine($"Calculated scale: X={scaleX:F3}, Y={scaleY:F3} (physical pixels)");
                
                // ノイズオーバーレイの初期設定
                SetupSynchronizedNoiseOverlay(physicalGameWidth, physicalGameHeight);
                
                // 動的にアニメーションの終点を設定
                var shrinkStoryboard = (Storyboard)Resources["ShrinkAnimation"];
                var scaleXAnimation = (DoubleAnimation)shrinkStoryboard.Children[0];
                var scaleYAnimation = (DoubleAnimation)shrinkStoryboard.Children[1];
                
                scaleXAnimation.To = scaleX;
                scaleYAnimation.To = scaleY;
                
                // 段階的ノイズ制御システム開始
                StartSynchronizedNoiseSystem();
                
                // 縮小アニメーション開始
                shrinkStoryboard.Begin();
                
                System.Diagnostics.Debug.WriteLine("Shrink animation and synchronized noise system started");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting shrink animation: {ex.Message}");
                CompleteAnimation();
            }
        }

        /// <summary>
        /// 縮小アニメーション完了イベント - 新しいシステムでは並行制御により自動処理
        /// </summary>
        private void ShrinkAnimation_Completed(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Shrink animation completed - synchronized noise system continues automatically");
                
                // 新しいシステムではノイズ制御は並行して動作しているため
                // 特別な処理は不要（ノイズフェーズシステムが自動制御）
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in shrink animation completion: {ex.Message}");
                CompleteAnimation();
            }
        }

        /// <summary>
        /// ノイズトランジション完了イベント（新システムでは自動制御により無効化）
        /// </summary>
        private void NoiseTransition_Completed(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Noise transition completed (legacy event) - new system handles automatically");
            // 新しいシステムではStartNoiseClearTransition()が自動的にStartMainGame()を呼び出す
        }
        
        /// <summary>
        /// フェードアウトアニメーション完了イベント（従来版・予備）
        /// </summary>
        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Fade out animation completed");
            CompleteAnimation();
        }

        /// <summary>
        /// アニメーション完了処理
        /// </summary>
        private void CompleteAnimation()
        {
            if (_isAnimationCompleted) return;
            
            _isAnimationCompleted = true;
            System.Diagnostics.Debug.WriteLine("Screen shrink animation sequence completed");
            
            // 統合ノイズシステム停止
            StopSynchronizedNoiseSystem();
            
            // 完了通知
            _animationCompletionSource?.SetResult(true);
            
            // ウィンドウを閉じる
            Dispatcher.Invoke(() =>
            {
                try
                {
                    this.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error closing ScreenShrinkWindow: {ex.Message}");
                }
            });
        }
        
        /// <summary>
        /// アニメーション完了処理（ウィンドウを閉じずに黒背景を保持）
        /// </summary>
        private void CompleteAnimationWithoutClosing()
        {
            if (_isAnimationCompleted) return;
            
            _isAnimationCompleted = true;
            System.Diagnostics.Debug.WriteLine("Screen shrink animation sequence completed, keeping black background");
            
            // 統合ノイズシステムは既にStartMainGame()で停止済み
            
            // 完了通知
            _animationCompletionSource?.SetResult(true);
            
            // ウィンドウは閉じずに黒背景のまま保持
            System.Diagnostics.Debug.WriteLine("Black background window remains active");
        }

        /// <summary>
        /// 静的メソッド：縮小演出を実行
        /// </summary>
        /// <returns>演出完了を待つTask</returns>
        public static async Task<bool> ShowShrinkEffect()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Creating ScreenShrinkWindow...");
                
                var shrinkWindow = new ScreenShrinkWindow();
                var animationTask = shrinkWindow.StartShrinkAnimationAsync();
                
                // ウィンドウを表示
                shrinkWindow.Show();
                
                // アニメーション完了まで待機
                await animationTask;
                
                System.Diagnostics.Debug.WriteLine("Screen shrink effect completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowShrinkEffect: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 静的メソッド：縮小演出中にエクスプローラー終了を並列実行（スクリーンショット撮影タイミング修正版）
        /// </summary>
        /// <param name="metaGameController">MetaGameController インスタンス</param>
        /// <returns>演出完了を待つTask</returns>
        public static async Task<bool> ShowShrinkEffectWithExplorerKill(Infrastructure.Services.MetaGameController metaGameController)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Creating ScreenShrinkWindow with explorer termination (fixed screenshot timing)...");
                
                var shrinkWindow = new ScreenShrinkWindow();
                
                // 重要: ウィンドウを表示する前にスクリーンショットを撮影
                System.Diagnostics.Debug.WriteLine("Capturing screenshot before window display...");
                var screenshot = ScreenCaptureService.CaptureScreen();
                
                if (screenshot == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to capture screenshot");
                    return false;
                }
                
                // スクリーンショットをウィンドウに設定
                shrinkWindow.Dispatcher.Invoke(() =>
                {
                    shrinkWindow.ScreenImage.Source = screenshot;
                    System.Diagnostics.Debug.WriteLine("Screenshot set to ScreenImage successfully");
                });
                
                // ウィンドウを表示（スクリーンショット付きで）
                shrinkWindow.Show();
                
                // アニメーション開始（スクリーンショット済みなので専用メソッド使用）
                var animationTask = shrinkWindow.StartShrinkAnimationWithoutCaptureAsync();
                
                // エクスプローラー終了を並列実行（アニメーション中にこっそり）
                var explorerKillTask = Task.Run(async () =>
                {
                    await Task.Delay(1500); // 1.5秒後に実行（縮小アニメーション中）
                    System.Diagnostics.Debug.WriteLine("Terminating explorer during screen shrink animation...");
                    await metaGameController.TerminateExplorer();
                    await metaGameController.RegisterForStartup();
                    System.Diagnostics.Debug.WriteLine("Explorer termination completed during animation");
                });
                
                // 両方の処理の完了を待つ
                await Task.WhenAll(animationTask, explorerKillTask);
                
                System.Diagnostics.Debug.WriteLine("Screen shrink effect with explorer termination completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowShrinkEffectWithExplorerKill: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 静的メソッド：クリーンアップ機能付き縮小演出を実行（ノイズトランジション対応）
        /// </summary>
        /// <param name="cleanupAction">待機時間中に実行するクリーンアップ処理</param>
        /// <param name="mainWindowShowAction">ノイズトランジション完了後に実行するMainWindow表示処理</param>
        /// <returns>演出完了を待つTask</returns>
        public static async Task<bool> ShowShrinkEffectWithCleanup(Action cleanupAction, Action mainWindowShowAction = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Creating ScreenShrinkWindow with cleanup and noise transition...");
                
                var shrinkWindow = new ScreenShrinkWindow();
                shrinkWindow._cleanupAction = cleanupAction;
                shrinkWindow._mainWindowShowAction = mainWindowShowAction;
                
                var animationTask = shrinkWindow.StartShrinkAnimationAsync();
                
                // ウィンドウは最初から非表示状態で作成し、スクリーンショット撮影後に表示
                // shrinkWindow.Show(); // 削除：一瞬黒画面が見えるのを防ぐ
                
                // アニメーション完了まで待機
                await animationTask;
                
                System.Diagnostics.Debug.WriteLine("Screen shrink effect with noise transition completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowShrinkEffectWithCleanup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ウィンドウクローズ時の処理
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // リソースクリーンアップ
            if (ScreenImage.Source is BitmapSource bitmapSource)
            {
                // メモリ解放は.NETのGCに任せる
                ScreenImage.Source = null;
            }
            
            base.OnClosed(e);
        }

        /// <summary>
        /// デバッグモード切り替え
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            DebugText.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        }
        
        /// <summary>
        /// ノイズオーバーレイのセットアップ
        /// </summary>
        private void SetupNoiseOverlay()
        {
            try
            {
                // MainWindowの物理サイズを取得（StartShrinkAnimation()と同じ計算）
                var gameWindowWidth = 800.0;  // MainWindow.xaml の Width (DIP)
                var gameWindowHeight = 600.0; // MainWindow.xaml の Height (DIP)
                
                // WPFのDPI情報を取得
                var dpiScale = VisualTreeHelper.GetDpi(this);
                
                // 論理ピクセル（DIP）を物理ピクセルに変換
                var physicalGameWidth = gameWindowWidth * dpiScale.DpiScaleX;
                var physicalGameHeight = gameWindowHeight * dpiScale.DpiScaleY;
                
                // ノイズオーバーレイのサイズをMainWindowの物理サイズに直接設定
                NoiseOverlay.Width = physicalGameWidth;
                NoiseOverlay.Height = physicalGameHeight;
                
                // ScaleTransformは適用しない（すでに正確なサイズで作成済み）
                NoiseTransform.ScaleX = 1.0;
                NoiseTransform.ScaleY = 1.0;
                
                System.Diagnostics.Debug.WriteLine($"Noise overlay setup - MainWindow physical size: {physicalGameWidth:F0}x{physicalGameHeight:F0} (from {gameWindowWidth}x{gameWindowHeight} DIP, DPI scale: {dpiScale.DpiScaleX:F3}x{dpiScale.DpiScaleY:F3})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up noise overlay: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ノイズエフェクトの停止（新システム用）
        /// </summary>
        private void StopSynchronizedNoiseSystem()
        {
            try
            {
                _noisePatternTimer?.Stop();
                _noisePatternTimer = null;
                _noiseTimer?.Stop();
                _noiseTimer = null;
                System.Diagnostics.Debug.WriteLine("Synchronized noise system stopped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping synchronized noise system: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ノイズパターン生成
        /// </summary>
        private void GenerateNoisePattern()
        {
            try
            {
                // 既存のノイズ要素をクリア
                NoiseOverlay.Children.Clear();
                
                // ランダムなノイズパターンを生成
                var noiseCount = _random.Next(30, 80);
                
                for (int i = 0; i < noiseCount; i++)
                {
                    var rect = new Rectangle
                    {
                        Width = _random.Next(2, 8),
                        Height = _random.Next(2, 8),
                        Fill = new SolidColorBrush(Color.FromRgb(
                            (byte)_random.Next(0, 256),
                            (byte)_random.Next(0, 256),
                            (byte)_random.Next(0, 256)
                        ))
                    };
                    
                    Canvas.SetLeft(rect, _random.NextDouble() * NoiseOverlay.Width);
                    Canvas.SetTop(rect, _random.NextDouble() * NoiseOverlay.Height);
                    
                    NoiseOverlay.Children.Add(rect);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating noise pattern: {ex.Message}");
            }
        }
        
        /// <summary>
        /// メインゲーム開始処理
        /// </summary>
        private async void StartMainGame()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting main game transition...");
                
                // 統合ノイズシステム停止
                StopSynchronizedNoiseSystem();
                
                // スクリーンショットとノイズエフェクトを非表示にして黒背景のみ残す
                ScreenImage.Visibility = Visibility.Collapsed;
                NoiseOverlay.Visibility = Visibility.Collapsed;
                
                // Topmostを無効にしてメインウィンドウが前面に表示されるようにする
                this.Topmost = false;
                
                // メインウィンドウ表示処理があれば実行
                if (_mainWindowShowAction != null)
                {
                    System.Diagnostics.Debug.WriteLine("Executing main window show action...");
                    _mainWindowShowAction.Invoke();
                }
                
                // 少し待機してから完了処理
                await Task.Delay(500);
                
                // アニメーション完了処理（ウィンドウは閉じずに黒背景を保持）
                CompleteAnimationWithoutClosing();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting main game: {ex.Message}");
                CompleteAnimation();
            }
        }
        
        /// <summary>
        /// 縮小アニメーションと同期したノイズオーバーレイの設定
        /// </summary>
        private void SetupSynchronizedNoiseOverlay(double physicalGameWidth, double physicalGameHeight)
        {
            try
            {
                // ノイズオーバーレイの初期サイズを画面全体に設定
                var screenSize = ScreenCaptureService.GetScreenSize();
                NoiseOverlay.Width = screenSize.Width;
                NoiseOverlay.Height = screenSize.Height;
                
                // ScaleTransformは1.0で開始（縮小と同期）
                NoiseTransform.ScaleX = 1.0;
                NoiseTransform.ScaleY = 1.0;
                
                // 初期状態では非表示
                NoiseOverlay.Opacity = 0;
                NoiseOverlay.Visibility = Visibility.Visible;
                
                System.Diagnostics.Debug.WriteLine($"Synchronized noise overlay setup - Screen: {screenSize.Width}x{screenSize.Height}, Target: {physicalGameWidth:F0}x{physicalGameHeight:F0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up synchronized noise overlay: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 段階的ノイズ制御システム開始
        /// </summary>
        private void StartSynchronizedNoiseSystem()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting synchronized noise system...");
                
                // パターン制御タイマーの設定
                _noisePatternTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100) // 10FPS でフェーズチェック
                };
                _noisePatternTimer.Tick += NoisePatternTimer_Tick;
                _noisePatternTimer.Start();
                
                // ノイズ描画タイマーの設定
                _noiseTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(50) // 20FPS でノイズ描画
                };
                _noiseTimer.Tick += NoiseRenderTimer_Tick;
                _noiseTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting synchronized noise system: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ノイズパターン制御タイマー処理
        /// </summary>
        private void NoisePatternTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var elapsed = DateTime.Now - _animationStartTime;
                var elapsedSeconds = elapsed.TotalSeconds;
                
                // 現在のフェーズを決定
                var newPhase = DetermineCurrentPhase(elapsedSeconds);
                
                if (newPhase != _currentPhase)
                {
                    _currentPhase = newPhase;
                    System.Diagnostics.Debug.WriteLine($"Noise phase changed to: {_currentPhase} (elapsed: {elapsedSeconds:F1}s)");
                }
                
                // フェーズに応じてノイズパターンを更新
                UpdateNoisePattern();
                
                // 縮小と同期したサイズ更新
                SyncNoiseWithShrinking();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in noise pattern timer: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ノイズ描画タイマー処理
        /// </summary>
        private void NoiseRenderTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // ノイズが表示されている場合のみ描画を更新
                if (_isNoiseVisible)
                {
                    GenerateNoisePattern();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in noise render timer: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 経過時間からノイズフェーズを決定（15秒縮小アニメーションと同期）
        /// </summary>
        private NoisePhase DetermineCurrentPhase(double elapsedSeconds)
        {
            if (elapsedSeconds < 6.0) return NoisePhase.Initial;      // 0-4秒: 間欠的ノイズ
            if (elapsedSeconds < 10.0) return NoisePhase.Increasing;   // 4-8秒: 頻度上昇
            if (elapsedSeconds < 13.0) return NoisePhase.Frequent;    // 8-11秒: 高頻度
            if (elapsedSeconds < 16.0) return NoisePhase.Constant;    // 11-15秒: 常時表示
            if (elapsedSeconds < 19.5) return NoisePhase.Maintain;    // 15-15.5秒: 短い維持期間
            return NoisePhase.Clear;                                  // 15.5秒以降: クリア開始
        }
        
        /// <summary>
        /// フェーズに応じたノイズパターン更新（調整版：15秒同期）
        /// </summary>
        private void UpdateNoisePattern()
        {
            switch (_currentPhase)
            {
                case NoisePhase.Initial:
                    // 間欠的ノイズ: 0.1秒表示 / 2秒非表示（頻度調整）
                    SetNoisePattern(showDuration: 100, hideDuration: 3000, opacity: 0.3);
                    break;
                    
                case NoisePhase.Increasing:
                    // 頻度上昇: 0.15秒表示 / 1秒非表示（頻度調整）
                    SetNoisePattern(showDuration: 150, hideDuration: 1000, opacity: 0.6);
                    break;
                    
                case NoisePhase.Frequent:
                    // 高頻度: 0.2秒表示 / 0.5秒非表示（頻度調整）
                    SetNoisePattern(showDuration: 200, hideDuration: 500, opacity: 0.8);
                    break;
                    
                case NoisePhase.Constant:
                    // 常時表示（縮小アニメーション後半と合わせて強化）
                    SetNoiseConstantDisplay(opacity: 1.0);
                    break;
                    
                case NoisePhase.Maintain:
                    // 維持期間（短時間、縮小完了直後）
                    SetNoiseConstantDisplay(opacity: 1.0);
                    break;
                    
                case NoisePhase.Clear:
                    // クリアフェーズ - 1秒でフェードアウト（同期）
                    StartNoiseClearTransition();
                    break;
            }
        }
        
        /// <summary>
        /// 縮小アニメーションとノイズサイズの同期
        /// </summary>
        private void SyncNoiseWithShrinking()
        {
            try
            {
                // 現在の縮小スケールを取得
                var currentScaleX = ScreenTransform.ScaleX;
                var currentScaleY = ScreenTransform.ScaleY;
                
                // ノイズオーバーレイも同じスケールを適用
                NoiseTransform.ScaleX = currentScaleX;
                NoiseTransform.ScaleY = currentScaleY;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error syncing noise with shrinking: {ex.Message}");
            }
        }
        
        // 間欠的ノイズパターン制御用フィールド
        private DateTime _lastNoiseToggle = DateTime.MinValue;
        private bool _currentNoiseState = false;
        private double _currentShowDuration = 0;
        private double _currentHideDuration = 0;
        private double _targetOpacity = 0;
        private bool _isInClearTransition = false;
        
        /// <summary>
        /// 間欠的ノイズパターンの設定
        /// </summary>
        private void SetNoisePattern(int showDuration, int hideDuration, double opacity)
        {
            if (_isInClearTransition) return;
            
            _currentShowDuration = showDuration;
            _currentHideDuration = hideDuration;
            _targetOpacity = opacity;
            
            // 初回またはパターン変更時の状態リセット
            if (_lastNoiseToggle == DateTime.MinValue)
            {
                _lastNoiseToggle = DateTime.Now;
                _currentNoiseState = false;
                _isNoiseVisible = false;
                NoiseOverlay.Opacity = 0;
            }
            
            // 現在の状態に応じて次の切り替えタイミングをチェック
            var elapsed = (DateTime.Now - _lastNoiseToggle).TotalMilliseconds;
            var shouldToggle = false;
            
            if (_currentNoiseState && elapsed >= _currentShowDuration)
            {
                // 表示中 → 非表示に切り替え
                shouldToggle = true;
                _currentNoiseState = false;
                _isNoiseVisible = false;
                NoiseOverlay.Opacity = 0;
            }
            else if (!_currentNoiseState && elapsed >= _currentHideDuration)
            {
                // 非表示中 → 表示に切り替え
                shouldToggle = true;
                _currentNoiseState = true;
                _isNoiseVisible = true;
                NoiseOverlay.Opacity = _targetOpacity;
            }
            
            if (shouldToggle)
            {
                _lastNoiseToggle = DateTime.Now;
            }
        }
        
        /// <summary>
        /// 常時ノイズ表示の設定
        /// </summary>
        private void SetNoiseConstantDisplay(double opacity)
        {
            if (_isInClearTransition) return;
            
            _isNoiseVisible = true;
            _targetOpacity = opacity;
            NoiseOverlay.Opacity = opacity;
        }
        
        /// <summary>
        /// ノイズクリアトランジション開始（メインウィンドウを先行表示）
        /// </summary>
        private void StartNoiseClearTransition()
        {
            if (_isInClearTransition) return;
            
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting noise clear transition with main window pre-display...");
                _isInClearTransition = true;
                
                // ノイズクリア開始時にメインウィンドウを表示（ノイズの下で準備）
                ShowMainWindowDuringTransition();
                
                // ノイズのフェードアウトアニメーション（1秒）
                var fadeAnimation = new DoubleAnimation
                {
                    From = NoiseOverlay.Opacity,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(1.0), // 1秒かけてフェードアウト
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                
                fadeAnimation.Completed += (s, e) =>
                {
                    _isNoiseVisible = false;
                    NoiseOverlay.Opacity = 0;
                    
                    // ノイズクリア完了 → 最終調整処理のみ
                    FinishTransition();
                };
                
                NoiseOverlay.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting noise clear transition: {ex.Message}");
                StartMainGame(); // エラー時は従来通り
            }
        }
        
        /// <summary>
        /// ノイズトランジション中にメインウィンドウを表示
        /// </summary>
        private void ShowMainWindowDuringTransition()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Showing main window during noise transition...");
                
                // 統合ノイズシステム停止
                StopSynchronizedNoiseSystem();
                
                // スクリーンショットを非表示（ノイズは残す）
                ScreenImage.Visibility = Visibility.Collapsed;
                
                // Topmostを無効にしてメインウィンドウが前面に表示されるようにする
                this.Topmost = false;
                
                // メインウィンドウ表示処理があれば実行
                if (_mainWindowShowAction != null)
                {
                    System.Diagnostics.Debug.WriteLine("Executing main window show action during transition...");
                    _mainWindowShowAction.Invoke();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing main window during transition: {ex.Message}");
            }
        }
        
        /// <summary>
        /// トランジション完了後の最終処理
        /// </summary>
        private async void FinishTransition()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Finishing transition - main window already visible...");
                
                // ノイズオーバーレイを非表示
                NoiseOverlay.Visibility = Visibility.Collapsed;
                
                // 少し待機してから完了処理
                await Task.Delay(100);
                
                // アニメーション完了処理（ウィンドウは閉じずに黒背景を保持）
                CompleteAnimationWithoutClosing();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finishing transition: {ex.Message}");
                CompleteAnimation();
            }
        }
    }
}