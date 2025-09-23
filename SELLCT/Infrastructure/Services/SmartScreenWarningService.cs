using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// Windows SmartScreen警告を模擬する教育用サービス
    /// 教育目的でマルウェアの実際の動作を理解するため
    /// </summary>
    public class SmartScreenWarningService
    {
        /// <summary>
        /// SmartScreen警告ダイアログを表示
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="publisher">発行者</param>
        /// <returns>ユーザーの選択結果</returns>
        public static SmartScreenResult ShowSmartScreenWarning(string fileName = "SELLCT.exe", string publisher = "Unknown Publisher")
        {
            var dialog = new SmartScreenDialog(fileName, publisher);
            dialog.ShowDialog();
            return dialog.Result;
        }
    }

    /// <summary>
    /// ユーザーの選択結果
    /// </summary>
    public enum SmartScreenResult
    {
        RunAnyway,
        DontRun,
        MoreInfo
    }

    /// <summary>
    /// SmartScreen警告ダイアログ
    /// </summary>
    internal class SmartScreenDialog : Window
    {
        public SmartScreenResult Result { get; private set; } = SmartScreenResult.DontRun;
        private bool _showingMoreInfo = false;

        public SmartScreenDialog(string fileName, string publisher)
        {
            InitializeDialog(fileName, publisher);
        }

        private void InitializeDialog(string fileName, string publisher)
        {
            Title = "Windows protected your PC";
            Width = 480;
            Height = 320;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            ShowInTaskbar = false;
            Topmost = true;
            Background = Brushes.White;

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });

            // 上部のWindows Defenderアイコンとタイトル
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 15, 20, 10) };
            
            // Windowsシールドアイコン（簡易版）
            var shieldIcon = CreateShieldIcon();
            headerPanel.Children.Add(shieldIcon);
            
            var titleText = new TextBlock
            {
                Text = "Windows protected your PC",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(15, 5, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
            };
            headerPanel.Children.Add(titleText);
            
            Grid.SetRow(headerPanel, 0);
            mainGrid.Children.Add(headerPanel);

            // 中央のメッセージ部分
            var messagePanel = new StackPanel { Margin = new Thickness(20, 10, 20, 10) };
            
            var warningText = new TextBlock
            {
                Text = "Not recognized by Windows Defender SmartScreen",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };
            messagePanel.Children.Add(warningText);
            
            var appNameText = new TextBlock
            {
                Text = $"App launch was blocked. Running this app may put your PC at risk.",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 15),
                TextWrapping = TextWrapping.Wrap
            };
            messagePanel.Children.Add(appNameText);

            // アプリ情報
            var appInfoPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            
            var appLabel = new TextBlock { Text = "App:", FontWeight = FontWeights.SemiBold, FontSize = 12 };
            var appValue = new TextBlock { Text = fileName, FontSize = 12, Margin = new Thickness(0, 2, 0, 5) };
            var publisherLabel = new TextBlock { Text = "Publisher:", FontWeight = FontWeights.SemiBold, FontSize = 12 };
            var publisherValue = new TextBlock { Text = publisher, FontSize = 12, Margin = new Thickness(0, 2, 0, 0) };
            
            appInfoPanel.Children.Add(appLabel);
            appInfoPanel.Children.Add(appValue);
            appInfoPanel.Children.Add(publisherLabel);
            appInfoPanel.Children.Add(publisherValue);
            
            messagePanel.Children.Add(appInfoPanel);

            Grid.SetRow(messagePanel, 1);
            mainGrid.Children.Add(messagePanel);

            // 下部のボタン
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 10, 20, 15)
            };

            var moreInfoButton = new Button
            {
                Content = "More info",
                Width = 100,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(173, 173, 173))
            };
            moreInfoButton.Click += MoreInfoButton_Click;
            
            var runAnywayButton = new Button
            {
                Content = "Run anyway",
                Width = 100,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Visibility = Visibility.Collapsed
            };
            runAnywayButton.Click += RunAnywayButton_Click;

            var dontRunButton = new Button
            {
                Content = "Don't run",
                Width = 100,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            dontRunButton.Click += DontRunButton_Click;

            buttonPanel.Children.Add(moreInfoButton);
            buttonPanel.Children.Add(runAnywayButton);
            buttonPanel.Children.Add(dontRunButton);
            
            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;

            // タグにボタン参照を保存
            Tag = new { MoreInfoButton = moreInfoButton, RunAnywayButton = runAnywayButton, MessagePanel = messagePanel };
        }

        private UIElement CreateShieldIcon()
        {
            var canvas = new Canvas { Width = 32, Height = 32 };
            
            // 背景円（青）
            var backgroundCircle = new Ellipse
            {
                Width = 32,
                Height = 32,
                Fill = new SolidColorBrush(Color.FromRgb(0, 120, 215))
            };
            Canvas.SetLeft(backgroundCircle, 0);
            Canvas.SetTop(backgroundCircle, 0);
            canvas.Children.Add(backgroundCircle);
            
            // シールド形状（白）
            var shield = new Polygon
            {
                Points = new PointCollection(new Point[]
                {
                    new Point(16, 6), new Point(26, 10), new Point(26, 20),
                    new Point(16, 26), new Point(6, 20), new Point(6, 10)
                }),
                Fill = Brushes.White
            };
            canvas.Children.Add(shield);
            
            // チェックマーク（青）
            var checkmark = new Polygon
            {
                Points = new PointCollection(new Point[]
                {
                    new Point(12, 16), new Point(15, 19), new Point(21, 13)
                }),
                Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };
            canvas.Children.Add(checkmark);
            
            return canvas;
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_showingMoreInfo)
            {
                _showingMoreInfo = true;
                var buttons = (dynamic)Tag;
                buttons.RunAnywayButton.Visibility = Visibility.Visible;
                
                // メッセージを更新
                var messagePanel = buttons.MessagePanel as StackPanel;
                if (messagePanel != null && messagePanel.Children.Count > 2)
                {
                    var additionalInfo = new TextBlock
                    {
                        Text = "\nThis app is from an unknown publisher and is not recognized by Microsoft Defender SmartScreen. Unknown apps may harm your PC.",
                        FontSize = 12,
                        Margin = new Thickness(0, 10, 0, 0),
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102))
                    };
                    messagePanel.Children.Insert(messagePanel.Children.Count - 1, additionalInfo);
                }
            }
        }

        private void RunAnywayButton_Click(object sender, RoutedEventArgs e)
        {
            Result = SmartScreenResult.RunAnyway;
            Close();
        }

        private void DontRunButton_Click(object sender, RoutedEventArgs e)
        {
            Result = SmartScreenResult.DontRun;
            Close();
        }
    }
}