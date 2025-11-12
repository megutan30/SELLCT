using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// 自動閉鎖メッセージボックスクラス
    /// 標準のMessageBoxに加えて、指定時間後に自動的に閉じる機能を提供
    /// 社会工学デモンストレーションで強制的な通知表示に使用
    /// Clean ArchitectureのInfrastructure層に配置されたユーティリティサービス
    /// 非同期処理にも対応し、UI応答性を維持
    /// </summary>
    public static class AutoClosingMessageBox
    {
        /// <summary>
        /// メッセージボックスを表示する（自動閉鎖オプション付き）
        /// タイムアウト時間が指定されている場合は自動閉鎖機能を有効にする
        /// タイムアウト時間が0以下の場合は標準のMessageBoxと同じ動作
        /// </summary>
        /// <param name="messageBoxText">表示するメッセージテキスト</param>
        /// <param name="caption">ウィンドウのタイトル</param>
        /// <param name="button">表示するボタンの種類</param>
        /// <param name="icon">表示するアイコンの種類</param>
        /// <param name="autoCloseTimeoutMs">自動閉鎖までの時間（ミリ秒）、0以下の場合は無効</param>
        /// <returns>ユーザーの選択またはタイムアウト時のデフォルト結果</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, int autoCloseTimeoutMs = 0)
        {
            // タイムアウトが設定されていない場合は標準のMessageBoxを使用
            if (autoCloseTimeoutMs <= 0)
            {
                return MessageBox.Show(messageBoxText, caption, button, icon);
            }

            // 自動閉鎖機能付きのメッセージボックスを表示
            return ShowAutoClosing(messageBoxText, caption, button, icon, autoCloseTimeoutMs);
        }

        /// <summary>
        /// 自動閉鎖機能付きメッセージボックスを表示する内部メソッド
        /// DispatcherTimerを使用して指定時間後に自動的にウィンドウを閉じる
        /// ユーザーが手動で閉じた場合とタイムアウトの両方に対応
        /// </summary>
        /// <param name="messageBoxText">表示するメッセージ</param>
        /// <param name="caption">ウィンドウタイトル</param>
        /// <param name="button">ボタンの種類</param>
        /// <param name="icon">アイコンの種類</param>
        /// <param name="timeoutMs">タイムアウト時間（ミリ秒）</param>
        /// <returns>ユーザーの選択またはデフォルト結果</returns>
        private static MessageBoxResult ShowAutoClosing(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, int timeoutMs)
        {
            var result = MessageBoxResult.None;
            var messageBoxWindow = CreateMessageBoxWindow(messageBoxText, caption, button, icon);
            
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(timeoutMs)
            };

            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                messageBoxWindow.DialogResult = true;
                result = GetDefaultResult(button);
            };

            timer.Start();

            try
            {
                var dialogResult = messageBoxWindow.ShowDialog();
                timer.Stop();

                if (dialogResult.HasValue && dialogResult.Value)
                {
                    return result == MessageBoxResult.None ? GetDefaultResult(button) : result;
                }
                else
                {
                    return MessageBoxResult.Cancel;
                }
            }
            catch
            {
                timer.Stop();
                return GetDefaultResult(button);
            }
        }

        private static Window CreateMessageBoxWindow(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            var window = new Window
            {
                Title = caption,
                Content = messageBoxText,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                SizeToContent = SizeToContent.WidthAndHeight,
                MinWidth = 300,
                MinHeight = 150,
                MaxWidth = 600,
                MaxHeight = 400,
                Topmost = true
            };

            var stackPanel = new System.Windows.Controls.StackPanel
            {
                Margin = new Thickness(20),
                Orientation = System.Windows.Controls.Orientation.Vertical
            };

            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = messageBoxText,
                Margin = new Thickness(0, 0, 0, 20),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stackPanel.Children.Add(textBlock);

            if (button != MessageBoxButton.OK || button == MessageBoxButton.OKCancel)
            {
                var buttonPanel = new System.Windows.Controls.StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                if (button == MessageBoxButton.OK || button == MessageBoxButton.OKCancel)
                {
                    var okButton = new System.Windows.Controls.Button
                    {
                        Content = "OK",
                        Margin = new Thickness(5),
                        Padding = new Thickness(15, 5, 15, 5),
                        IsDefault = true
                    };
                    okButton.Click += (s, e) => { window.DialogResult = true; };
                    buttonPanel.Children.Add(okButton);
                }

                if (button == MessageBoxButton.OKCancel || button == MessageBoxButton.YesNoCancel)
                {
                    var cancelButton = new System.Windows.Controls.Button
                    {
                        Content = "Cancel",
                        Margin = new Thickness(5),
                        Padding = new Thickness(15, 5, 15, 5),
                        IsCancel = true
                    };
                    cancelButton.Click += (s, e) => { window.DialogResult = false; };
                    buttonPanel.Children.Add(cancelButton);
                }

                stackPanel.Children.Add(buttonPanel);
            }

            window.Content = stackPanel;
            return window;
        }

        private static MessageBoxResult GetDefaultResult(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    return MessageBoxResult.OK;
                case MessageBoxButton.OKCancel:
                    return MessageBoxResult.OK;
                case MessageBoxButton.YesNo:
                    return MessageBoxResult.Yes;
                case MessageBoxButton.YesNoCancel:
                    return MessageBoxResult.Yes;
                default:
                    return MessageBoxResult.OK;
            }
        }

        /// <summary>
        /// メッセージボックスを非同期で表示する
        /// UIスレッドをブロックせずにメッセージボックスを表示可能
        /// 長時間の処理中やタイムアウト待機中のUI応答性を維持
        /// </summary>
        /// <param name="messageBoxText">表示するメッセージテキスト</param>
        /// <param name="caption">ウィンドウのタイトル</param>
        /// <param name="button">表示するボタンの種類</param>
        /// <param name="icon">表示するアイコンの種類</param>
        /// <param name="autoCloseTimeoutMs">自動閉鎖までの時間（ミリ秒）</param>
        /// <returns>ユーザーの選択またはタイムアウト時のデフォルト結果を返すTask</returns>
        public static async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, int autoCloseTimeoutMs = 0)
        {
            return await Task.Run(() => Show(messageBoxText, caption, button, icon, autoCloseTimeoutMs));
        }
    }
}