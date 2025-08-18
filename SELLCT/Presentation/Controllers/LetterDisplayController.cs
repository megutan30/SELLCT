using System;
using System.Windows;
using System.Windows.Threading;
using SELLCT.Infrastructure.Services;
using SELLCT.Core.Events;
using SELLCT.Core.Interfaces;
using SELLCT.Core.Entities;

namespace SELLCT.Presentation.Controllers
{
    public class LetterDisplayController : IDisposable
    {
        private readonly LetterService _letterService;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly DispatcherTimer _initialLetterTimer;
        private readonly DispatcherTimer _subsequentLetterTimer;

        private FrameworkElement _letterImage;
        private FrameworkElement _statusText;

        private bool _disposed = false;

        public LetterDisplayController(LetterService letterService, IEventDispatcher eventDispatcher)
        {
            _letterService = letterService;
            _eventDispatcher = eventDispatcher;

            _initialLetterTimer = new DispatcherTimer();
            _initialLetterTimer.Interval = TimeSpan.FromSeconds(3);
            _initialLetterTimer.Tick += InitialLetterTimer_Tick;

            _subsequentLetterTimer = new DispatcherTimer();
            _subsequentLetterTimer.Interval = TimeSpan.FromSeconds(10);
            _subsequentLetterTimer.Tick += SubsequentLetterTimer_Tick;

            _eventDispatcher.Subscribe<LetterAppearedEvent>(OnLetterAppeared);
            _eventDispatcher.Subscribe<LetterClickedEvent>(OnLetterClicked);
        }

        public void InjectUIElements(FrameworkElement letterImage, FrameworkElement statusText)
        {
            _letterImage = letterImage;
            _statusText = statusText;
        }

        public void HandleMainButtonClick()
        {
            try
            {
                if (!_initialLetterTimer.IsEnabled && _letterService.CurrentLetterIndex == 0)
                {
                    var firstCondition = _letterService.GetNextLetterCondition();
                    if (firstCondition != null)
                    {
                        _initialLetterTimer.Interval = TimeSpan.FromSeconds(firstCondition.TimeIntervalSeconds);
                    }
                    _initialLetterTimer.Start();
                    UpdateStatusText("手紙の到着を待っています...");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HandleMainButtonClick: {ex.Message}");
            }
        }

        public void HandleLetterClick()
        {
            try
            {
                var letterIndex = _letterService?.CurrentLetterIndex ?? 1;
                if (_letterService == null) return;

                SetLetterVisibility(false);

                bool success = _letterService.OnLetterClicked(letterIndex);

                if (success)
                {
                    if (!_letterService.IsSequenceComplete)
                    {
                        SetupNextLetterTimer();
                        UpdateStatusText("次の手紙の到着を待っています...");
                    }
                }
                else
                {
                    SetLetterVisibility(true);
                    UpdateStatusText("手紙のダウンロードがキャンセルされました");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HandleLetterClick: {ex.Message}");
                SetLetterVisibility(true);
            }
        }

        private void OnLetterAppeared(LetterAppearedEvent @event)
        {
            try
            {
                SetLetterVisibility(true);
                UpdateStatusText($"SELLCTからの手紙 {@event.LetterIndex} が到着しました");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnLetterAppeared: {ex.Message}");
            }
        }

        private void OnLetterClicked(LetterClickedEvent @event)
        {
            try
            {
                UpdateStatusText($"手紙 {@event.LetterIndex} をダウンロードしました");
                ResetNextLetterTimer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnLetterClicked: {ex.Message}");
            }
        }

        private void InitialLetterTimer_Tick(object sender, EventArgs e)
        {
            _initialLetterTimer.Stop();
            TryShowNextLetterWithCondition();
        }

        private void SubsequentLetterTimer_Tick(object sender, EventArgs e)
        {
            _subsequentLetterTimer.Stop();
            TryShowNextLetterWithCondition();
        }

        private void TryShowNextLetterWithCondition()
        {
            try
            {
                var condition = _letterService.GetNextLetterCondition();
                if (condition == null)
                {
                    _letterService.ShowNextLetter();
                    return;
                }

                var componentsPath = "components";
                bool conditionMet = _letterService.CheckCondition(condition, componentsPath);

                if (conditionMet)
                {
                    _letterService.ShowNextLetter();
                    SetupNextLetterTimer();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TryShowNextLetterWithCondition: {ex.Message}");
            }
        }

        private void SetupNextLetterTimer()
        {
            try
            {
                if (_subsequentLetterTimer.IsEnabled)
                {
                    _subsequentLetterTimer.Stop();
                }

                var nextCondition = _letterService.GetNextLetterCondition();
                if (nextCondition != null && HasTimeComponent(nextCondition.TriggerType))
                {
                    _subsequentLetterTimer.Interval = TimeSpan.FromSeconds(nextCondition.TimeIntervalSeconds);
                    _subsequentLetterTimer.Start();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up next letter timer: {ex.Message}");
            }
        }

        private void ResetNextLetterTimer()
        {
            try
            {
                if (_subsequentLetterTimer.IsEnabled)
                {
                    _subsequentLetterTimer.Stop();
                }

                var nextCondition = _letterService.GetNextLetterCondition();
                if (nextCondition != null && HasTimeComponent(nextCondition.TriggerType))
                {
                    _subsequentLetterTimer.Interval = TimeSpan.FromSeconds(nextCondition.TimeIntervalSeconds);
                    _subsequentLetterTimer.Start();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting next letter timer: {ex.Message}");
            }
        }

        private bool HasTimeComponent(LetterTriggerType triggerType)
        {
            return triggerType == LetterTriggerType.TimeOnly ||
                   triggerType == LetterTriggerType.TimeAndFileExistence ||
                   triggerType == LetterTriggerType.TimeAndFileNotExistence ||
                   triggerType == LetterTriggerType.TimeAndAllFilesExist ||
                   triggerType == LetterTriggerType.TimeAndAnyFileExists ||
                   triggerType == LetterTriggerType.TimeAndAllFilesNotExist ||
                   triggerType == LetterTriggerType.TimeAndAnyFileNotExists;
        }

        private void SetLetterVisibility(bool isVisible)
        {
            if (_letterImage != null)
            {
                _letterImage.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateStatusText(string text)
        {
            if (_statusText is System.Windows.Controls.TextBlock statusTextBlock)
            {
                statusTextBlock.Text = text;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                
                _initialLetterTimer?.Stop();
                _subsequentLetterTimer?.Stop();
            }
        }
    }
}