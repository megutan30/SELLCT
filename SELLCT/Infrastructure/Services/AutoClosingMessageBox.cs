using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SELLCT.Infrastructure.Services
{
    public static class AutoClosingMessageBox
    {
        public static MessageBoxResult Show(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, int autoCloseTimeoutMs = 0)
        {
            if (autoCloseTimeoutMs <= 0)
            {
                return MessageBox.Show(messageBoxText, caption, button, icon);
            }

            return ShowAutoClosing(messageBoxText, caption, button, icon, autoCloseTimeoutMs);
        }

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

        public static async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, int autoCloseTimeoutMs = 0)
        {
            return await Task.Run(() => Show(messageBoxText, caption, button, icon, autoCloseTimeoutMs));
        }
    }
}