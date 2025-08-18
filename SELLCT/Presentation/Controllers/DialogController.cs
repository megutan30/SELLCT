using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SELLCT.Presentation.Controllers
{
    public class DialogController : IDisposable
    {
        private enum DialogItemType
        {
            Text,
            Choice
        }

        private class DialogItem
        {
            public DialogItemType Type { get; set; }
            public string Message { get; set; }
        }

        private readonly Queue<DialogItem> _dialogMessageQueue;
        private readonly DispatcherTimer _typingTimer;
        private readonly List<string> _messageHistory;

        private bool _isTyping;
        private bool _awaitingChoice;
        private bool _isButtonProcessing = false;
        private string _currentFullMessage;
        private int _currentMessageCharIndex;

        private FrameworkElement _textWindow;
        private TextBlock _dialogText;
        private FrameworkElement _choiceButtonsPanel;
        private Button _yesButton;
        private Button _noButton;
        private FrameworkElement _logPanel;
        private TextBlock _logText;
        private ScrollViewer _logScrollViewer;

        private bool _disposed = false;

        public DialogController()
        {
            _dialogMessageQueue = new Queue<DialogItem>();
            _messageHistory = new List<string>();
            
            _typingTimer = new DispatcherTimer();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(50);
            _typingTimer.Tick += TypingTimer_Tick;
        }

        public void InjectUIElements(
            FrameworkElement textWindow,
            TextBlock dialogText,
            FrameworkElement choiceButtonsPanel,
            Button yesButton,
            Button noButton,
            FrameworkElement logPanel,
            TextBlock logText,
            ScrollViewer logScrollViewer)
        {
            _textWindow = textWindow;
            _dialogText = dialogText;
            _choiceButtonsPanel = choiceButtonsPanel;
            _yesButton = yesButton;
            _noButton = noButton;
            _logPanel = logPanel;
            _logText = logText;
            _logScrollViewer = logScrollViewer;
        }

        public void SetAwaitingChoice(bool value)
        {
            _awaitingChoice = value;
        }

        public void ShowDialogMessage(params string[] messages)
        {
            if (!CanAddToQueue()) return;

            foreach (var msg in messages)
            {
                EnqueueUniqueMessage(msg);
            }

            if (!_isTyping && !_awaitingChoice && _textWindow.Visibility == Visibility.Visible)
            {
                ProcessNextDialogMessage();
            }
            else if (_textWindow.Visibility != Visibility.Visible)
            {
                _textWindow.Visibility = Visibility.Visible;
                StartDialogFadeIn();
            }
        }

        public void ShowChoice()
        {
            _dialogMessageQueue.Enqueue(new DialogItem { Type = DialogItemType.Choice });
            if (!_isTyping && !_awaitingChoice)
            {
                ProcessNextDialogMessage();
            }
            else if (_textWindow.Visibility != Visibility.Visible)
            {
                StartDialogFadeIn();
            }
        }

        public bool HandleDialogClick()
        {
            if (_textWindow.Visibility != Visibility.Visible) return false;
            if (_awaitingChoice) return false;

            if (_isTyping)
            {
                _dialogText.Text = _currentFullMessage;
                _currentMessageCharIndex = _currentFullMessage.Length;
                _isTyping = false;
                _typingTimer.Stop();
                AddMessageToHistory(_currentFullMessage);
                return true;
            }
            else
            {
                ProcessNextDialogMessage();
                return true;
            }
        }

        public void HandleUnknownButtonClick(string buttonText)
        {
            if (_isButtonProcessing || !CanAddToQueue()) return;
            if (_textWindow.Visibility != Visibility.Visible) return;

            try
            {
                _isButtonProcessing = true;
                SetTextWindowVisibility(true);
                
                string message1 = $"ボタン名前を変えられるみたいですが、\n";
                string message2 = $"どうやら'{buttonText}'機能はないようですね。\n";
                
                EnqueueUniqueMessage(message1);
                EnqueueUniqueMessage(message2);

                if (!_isTyping && !_awaitingChoice && _dialogMessageQueue.Count > 0)
                {
                    ProcessNextDialogMessage();
                }
            }
            finally
            {
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(500);
                timer.Tick += (s, e) =>
                {
                    _isButtonProcessing = false;
                    timer.Stop();
                };
                timer.Start();
            }
        }

        public void ToggleLogDisplay()
        {
            if (_logPanel.Visibility == Visibility.Visible)
            {
                _logPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateLogDisplay();
                _logPanel.Visibility = Visibility.Visible;
            }
        }

        public void CloseLogPanel()
        {
            _logPanel.Visibility = Visibility.Collapsed;
        }

        public void SetTextWindowVisibility(bool isVisible)
        {
            if (_textWindow != null)
            {
                _textWindow.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void ClearMessageQueue()
        {
            _dialogMessageQueue.Clear();
            _isTyping = false;
            _typingTimer.Stop();
            _dialogText.Text = string.Empty;
            _awaitingChoice = false;
            _textWindow.Visibility = Visibility.Collapsed;
        }

        public bool IsMessageQueueEmpty()
        {
            return _dialogMessageQueue?.Count == 0;
        }

        public bool IsTyping()
        {
            return _isTyping;
        }

        private void ProcessNextDialogMessage()
        {
            if (_dialogMessageQueue.Any())
            {
                var nextItem = _dialogMessageQueue.Dequeue();

                if (nextItem.Type == DialogItemType.Text)
                {
                    _currentFullMessage = nextItem.Message;
                    _currentMessageCharIndex = 0;
                    _dialogText.Text = string.Empty;
                    _isTyping = true;
                    _typingTimer.Start();
                    _choiceButtonsPanel.Visibility = Visibility.Collapsed;
                }
                else if (nextItem.Type == DialogItemType.Choice)
                {
                    _choiceButtonsPanel.Visibility = Visibility.Visible;
                    _yesButton.Visibility = Visibility.Visible;
                    _noButton.Visibility = Visibility.Visible;
                    _awaitingChoice = true;
                    _typingTimer.Stop();
                }
            }
            else
            {
                _isTyping = false;
                _typingTimer.Stop();
                _awaitingChoice = false;
            }
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (_currentMessageCharIndex < _currentFullMessage.Length)
            {
                _dialogText.Text += _currentFullMessage[_currentMessageCharIndex];
                _currentMessageCharIndex++;
            }
            else
            {
                _isTyping = false;
                _typingTimer.Stop();
                AddMessageToHistory(_currentFullMessage);
            }
        }

        private void StartDialogFadeIn()
        {
            // アニメーション処理は必要に応じて実装
        }

        private void EnqueueUniqueMessage(string message)
        {
            if (_currentFullMessage == message) return;
            if (_dialogMessageQueue.Any(item => item.Type == DialogItemType.Text && item.Message == message)) return;
            
            _dialogMessageQueue.Enqueue(new DialogItem { Type = DialogItemType.Text, Message = message });
        }

        private bool CanAddToQueue()
        {
            return !_isButtonProcessing && !_awaitingChoice;
        }

        private void UpdateLogDisplay()
        {
            try
            {
                if (_messageHistory.Count == 0)
                {
                    _logText.Text = "まだメッセージはありません。";
                    return;
                }

                var logContent = new StringBuilder();
                for (int i = 0; i < _messageHistory.Count; i++)
                {
                    logContent.AppendLine($"[{i + 1:D2}] {_messageHistory[i]}");
                    if (i < _messageHistory.Count - 1)
                    {
                        logContent.AppendLine();
                    }
                }

                _logText.Text = logContent.ToString();
                _logScrollViewer.ScrollToEnd();
            }
            catch (Exception ex)
            {
                _logText.Text = "ログの表示中にエラーが発生しました。";
            }
        }

        private void AddMessageToHistory(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            try
            {
                var cleanMessage = message.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
                if (string.IsNullOrEmpty(cleanMessage)) return;

                if (_messageHistory.Count > 0 && _messageHistory[_messageHistory.Count - 1] == cleanMessage)
                {
                    return;
                }

                _messageHistory.Add(cleanMessage);
            }
            catch
            {
                // エラーは無視
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _typingTimer?.Stop();
            }
        }
    }
}