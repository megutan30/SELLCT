using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// カスタム警告ダイアログ
    /// 教育目的でマルウェアの警告スパム攻撃を模擬
    /// </summary>
    internal class WarningDialog : Window
    {
        private readonly bool _isFinalWarning;
        
        /// <summary>
        /// 最終警告の「はい」ボタンがクリックされた時のイベント
        /// </summary>
        public event EventHandler YesButtonClicked;
        
        /// <summary>
        /// ウィンドウが閉じられた時のイベント
        /// </summary>
        public event EventHandler WindowClosed;
        
        public WarningDialog(bool isFinalWarning = false)
        {
            _isFinalWarning = isFinalWarning;
            InitializeDialog();
        }
        
        private void InitializeDialog()
        {
            // 基本設定
            Width = 400;
            Height = 200;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            ShowInTaskbar = false;
            Background = Brushes.White;
            
            // 最終警告の場合はタイトルとTopmost設定を変更
            if (_isFinalWarning)
            {
                Title = "重要な警告 - SELLCT";
                Topmost = true;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                Title = "セキュリティ警告";
                Topmost = false;
            }
            
            CreateDialogContent();
        }
        
        private void CreateDialogContent()
        {
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            mainGrid.Margin = new Thickness(15);
            
            // メッセージ部分
            var messagePanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            // 警告アイコン
            var warningIcon = CreateWarningIcon();
            messagePanel.Children.Add(warningIcon);
            
            // メッセージテキスト
            var messageText = new TextBlock
            {
                Text = GetWarningMessage(),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 320
            };
            
            if (_isFinalWarning)
            {
                messageText.FontWeight = FontWeights.Bold;
                messageText.FontSize = 13;
            }
            
            messagePanel.Children.Add(messageText);
            Grid.SetRow(messagePanel, 0);
            mainGrid.Children.Add(messagePanel);
            
            // ボタン部分
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            if (_isFinalWarning)
            {
                // 最終警告の場合は「はい」ボタンのみ
                var yesButton = new Button
                {
                    Content = "はい",
                    Width = 75,
                    Height = 40,
                    IsDefault = true
                };
                yesButton.Click += YesButton_Click;
                
                buttonPanel.Children.Add(yesButton);
            }
            else
            {
                // 通常の警告の場合はOKボタン（無効化）
                var okButton = new Button
                {
                    Content = "待機中...",
                    Width = 75,
                    Height = 40,
                    IsEnabled = false
                };
                buttonPanel.Children.Add(okButton);
            }
            Grid.SetRow(buttonPanel, 1);
            mainGrid.Children.Add(buttonPanel);
            
            Content = mainGrid;
        }
        
        private UIElement CreateWarningIcon()
        {
            var canvas = new Canvas { Width = 32, Height = 32 };
            
            // 黄色の三角形背景
            var triangle = new Polygon
            {
                Points = new PointCollection(new Point[]
                {
                    new Point(16, 2), new Point(30, 26), new Point(2, 26)
                }),
                Fill = new SolidColorBrush(Color.FromRgb(255, 193, 7)), // 警告黄色
                Stroke = new SolidColorBrush(Color.FromRgb(255, 143, 0)),
                StrokeThickness = 1
            };
            canvas.Children.Add(triangle);
            
            // 感嘆符
            var exclamationBody = new Rectangle
            {
                Width = 3,
                Height = 12,
                Fill = Brushes.Black
            };
            Canvas.SetLeft(exclamationBody, 14.5);
            Canvas.SetTop(exclamationBody, 8);
            canvas.Children.Add(exclamationBody);
            
            var exclamationDot = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = Brushes.Black
            };
            Canvas.SetLeft(exclamationDot, 14.5);
            Canvas.SetTop(exclamationDot, 22);
            canvas.Children.Add(exclamationDot);
            
            return canvas;
        }
        
        private string GetWarningMessage()
        {
            if (_isFinalWarning)
            {
                return "システムの整合性を確認するため、最後の認証が必要です。\n\n" +
                       "この操作により、SELLCT教育プログラムが開始されます。\n\n" +
                       "続行してもよろしいですか？";
            }
            else
            {
                var messages = new[]
                {
                    "システムファイルの異常を検出しました。",
                    "不正なプロセスが実行されています。",
                    "ウイルススキャンが必要です。",
                    "セキュリティポリシー違反が発生しました。",
                    "システムの整合性チェックに失敗しました。",
                    "認証されていないソフトウェアが検出されました。",
                    "レジストリの不整合が発見されました。",
                    "不審なネットワーク活動を監視中です。"
                };
                
                var random = new Random();
                return messages[random.Next(messages.Length)] + "\n\n対処が必要です。";
            }
        }
        
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            
            // 「はい」ボタンがクリックされた場合、ゲーム開始イベントを発火
            YesButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // 最終警告以外の場合は、マウスやキーボード操作を制限
            if (!_isFinalWarning)
            {
                // ウィンドウの移動やサイズ変更を無効化
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    // WS_SYSMENU スタイルを削除してシステムメニューを無効化
                    var style = GetWindowLong(hwnd, GWL_STYLE);
                    style &= ~WS_SYSMENU;
                    SetWindowLong(hwnd, GWL_STYLE, style);
                }
            }
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            
            // 最終警告ダイアログが閉じボタンで閉じられた場合、プログラム終了イベントを発火
            if (_isFinalWarning && DialogResult != true)
            {
                WindowClosed?.Invoke(this, EventArgs.Empty);
            }
        }
        
        // Win32 API定義
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}