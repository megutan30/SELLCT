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
        private Random _random = new Random();

        public ScreenShrinkWindow()
        {
            InitializeComponent();
            
            // 正確な画面サイズに設定
            SetWindowSize();
            
            Loaded += ScreenShrinkWindow_Loaded;
        }

        /// <summary>
        /// ウィンドウロード時の処理
        /// </summary>
        private async void ScreenShrinkWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
                // ウィンドウを一時的に非表示にしてスクリーンショット撮影
                System.Diagnostics.Debug.WriteLine("Hiding window for screenshot capture...");
                this.Hide();
                
                // ウィンドウが完全に非表示になるまで待機
                await Task.Delay(200);
                
                // スクリーンショット撮影
                var screenshot = ScreenCaptureService.CaptureScreen();
                
                if (screenshot != null)
                {
                    System.Diagnostics.Debug.WriteLine("Screenshot captured successfully");
                    
                    // メインスレッドで画像を設定
                    Dispatcher.Invoke(() =>
                    {
                        ScreenImage.Source = screenshot;
                    });
                    
                    // ウィンドウを再表示
                    this.Show();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to capture screenshot");
                    this.Show(); // エラー時も再表示
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error capturing screenshot: {ex.Message}");
                this.Show(); // エラー時も再表示
            }
        }

        /// <summary>
        /// 縮小アニメーションを開始
        /// </summary>
        public Task StartShrinkAnimationAsync()
        {
            _animationCompletionSource = new TaskCompletionSource<bool>();
            return _animationCompletionSource.Task;
        }

        /// <summary>
        /// 縮小アニメーション開始
        /// </summary>
        private void StartShrinkAnimation()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting shrink animation...");
                
                // メインゲーム画面サイズに対応する縮小比率を計算
                var screenSize = ScreenCaptureService.GetScreenSize();
                var gameWindowWidth = 800.0;  // MainWindow.xaml の Width
                var gameWindowHeight = 600.0; // MainWindow.xaml の Height
                
                var scaleX = gameWindowWidth / screenSize.Width;
                var scaleY = gameWindowHeight / screenSize.Height;
                
                System.Diagnostics.Debug.WriteLine($"Calculated scale: X={scaleX:F3}, Y={scaleY:F3} (target: {gameWindowWidth}x{gameWindowHeight})");
                
                // 動的にアニメーションの終点を設定
                var shrinkStoryboard = (Storyboard)Resources["ShrinkAnimation"];
                var scaleXAnimation = (DoubleAnimation)shrinkStoryboard.Children[0];
                var scaleYAnimation = (DoubleAnimation)shrinkStoryboard.Children[1];
                
                scaleXAnimation.To = scaleX;
                scaleYAnimation.To = scaleY;
                
                shrinkStoryboard.Begin();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting shrink animation: {ex.Message}");
                CompleteAnimation();
            }
        }

        /// <summary>
        /// 縮小アニメーション完了イベント - ノイズトランジション開始
        /// </summary>
        private void ShrinkAnimation_Completed(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Shrink animation completed, starting noise transition...");
                
                // ノイズオーバーレイのサイズを縮小された画像に合わせる
                SetupNoiseOverlay();
                
                // ノイズエフェクト開始
                StartNoiseEffect();
                
                // ノイズトランジションアニメーション開始
                var noiseStoryboard = (Storyboard)Resources["NoiseTransitionAnimation"];
                noiseStoryboard.Begin();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in shrink animation completion: {ex.Message}");
                CompleteAnimation();
            }
        }

        /// <summary>
        /// ノイズトランジション完了イベント
        /// </summary>
        private void NoiseTransition_Completed(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Noise transition completed, starting main game...");
            
            // ノイズエフェクト停止
            StopNoiseEffect();
            
            // メインゲーム開始処理に進む
            StartMainGame();
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
                
                // ウィンドウを表示
                shrinkWindow.Show();
                
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
                // 元の画面サイズを取得
                var screenSize = ScreenCaptureService.GetScreenSize();
                
                // 縮小比率を取得
                var scaleX = ScreenTransform.ScaleX;
                var scaleY = ScreenTransform.ScaleY;
                
                // 縮小後の実際の表示サイズを計算
                var displayWidth = screenSize.Width * scaleX;
                var displayHeight = screenSize.Height * scaleY;
                
                // ノイズオーバーレイのサイズを縮小後の実際のサイズに設定
                NoiseOverlay.Width = displayWidth;
                NoiseOverlay.Height = displayHeight;
                
                // ScaleTransformは適用しない（すでに正確なサイズで作成済み）
                NoiseTransform.ScaleX = 1.0;
                NoiseTransform.ScaleY = 1.0;
                
                System.Diagnostics.Debug.WriteLine($"Noise overlay setup - Original screen: {screenSize.Width}x{screenSize.Height}, Scale: {scaleX:F3}x{scaleY:F3}, Final size: {displayWidth:F0}x{displayHeight:F0}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up noise overlay: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ノイズエフェクトの開始
        /// </summary>
        private void StartNoiseEffect()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting noise effect...");
                
                // ノイズタイマーの設定
                _noiseTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(50) // 20FPS
                };
                _noiseTimer.Tick += NoiseTimer_Tick;
                _noiseTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting noise effect: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ノイズエフェクトの停止
        /// </summary>
        private void StopNoiseEffect()
        {
            try
            {
                _noiseTimer?.Stop();
                _noiseTimer = null;
                System.Diagnostics.Debug.WriteLine("Noise effect stopped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping noise effect: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ノイズタイマーティック処理
        /// </summary>
        private void NoiseTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // ノイズパターンを生成
                GenerateNoisePattern();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in noise timer tick: {ex.Message}");
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
    }
}