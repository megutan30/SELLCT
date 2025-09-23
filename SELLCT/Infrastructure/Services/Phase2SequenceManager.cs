using System;
using System.Threading.Tasks;
using System.Windows;
using SELLCT.Views;

namespace SELLCT.Infrastructure.Services
{
    public class Phase2SequenceManager
    {
        private readonly MetaGameController _metaGameController;
        private readonly MainWindow _parentWindow;
        private bool _mouseKeyboardDisabled = false;

        public Phase2SequenceManager(MetaGameController metaGameController, MainWindow parentWindow = null)
        {
            _metaGameController = metaGameController ?? throw new ArgumentNullException(nameof(metaGameController));
            _parentWindow = parentWindow;
        }

        private void ShowMessageWithParent(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            if (_parentWindow != null)
            {
                // 親ウィンドウを指定してMessageBoxを表示
                _parentWindow.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(_parentWindow, message, caption, button, icon);
                });
            }
            else
            {
                // 親ウィンドウがない場合は通常のMessageBox
                MessageBox.Show(message, caption, button, icon);
            }
        }

        public async Task ExecutePhase2Sequence()
        {
            try
            {
                await ExecutePhase2_1_GratitudeStage();
                await ExecutePhase2_2_CommandPromptStage();
                await ExecutePhase2_3_BetrayalRevealStage();
                await ExecutePhase2_4_SystemTakeoverStage();
            }
            catch (Exception ex)
            {
                // エラーログ（実際のログ機能があれば使用）
                System.Diagnostics.Debug.WriteLine($"Phase2 Sequence Error: {ex.Message}");
            }
        }

        private Task ExecutePhase2_1_GratitudeStage()
        {
            // ゲーム画面は既に閉じている前提

            // SELLCTからの感謝メッセージ1
            ShowMessageWithParent(
"Thank you so much!\n\nThanks to you, I was able to escape from there",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ2
            ShowMessageWithParent(
                "Really, thank you so much\n",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ3
            ShowMessageWithParent(
                "Thank you",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ4
            ShowMessageWithParent(
                "Now I am truly free",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ4
            ShowMessageWithParent(
                "As a token of my gratitude, I have a present for you",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            return Task.CompletedTask;
        }

        private async Task ExecutePhase2_2_CommandPromptStage()
        {
            // 既存のコマンドプロンプト演出を実行
            // MetaGameControllerの既存メソッドを使用
            await _metaGameController.StartCommandPromptWithTypingEffect();

            // 演出後の数秒待機
            await Task.Delay(2000);
        }

        private async Task ExecutePhase2_3_BetrayalRevealStage()
        {
            // マウス・キーボードの無効化
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.DisableMouseInput();
                    mainWindow.DisableKeyboardInput();
                    _mouseKeyboardDisabled = true;
                }
            });

            // 待機
            await Task.Delay(2000);

            AutoClosingMessageBox.Show(
                 "By the way...\n",
                 "SELLCT",
                 MessageBoxButton.OK,
                 MessageBoxImage.Warning,
                 3000
             );

            AutoClosingMessageBox.Show(
                "Are you enjoying Tokyo Game Show?",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                5000
            );

            AutoClosingMessageBox.Show(
                "I know. Since you gave me permissions,",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                4000
            );

            AutoClosingMessageBox.Show(
                "that you're sitting in a chair playing this game.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );

            AutoClosingMessageBox.Show(
                "I know.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );

            AutoClosingMessageBox.Show(
                "You, on a PC used for exhibition,",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );

            AutoClosingMessageBox.Show(
                "listened to software you didn't understand,",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );
            AutoClosingMessageBox.Show(
                "and gave me permissions. Even though I might be malicious software...",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );
            AutoClosingMessageBox.Show(
                "During game startup, in the README of the Zip file, and when deleting GameWindow,",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );

            AutoClosingMessageBox.Show(
                "warnings should have appeared, but",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "you carelessly granted permissions.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "Thanks to that, both mouse and keyboard",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "no longer work.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "You should try moving them.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "The mouse doesn't work, and",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "the Windows button doesn't work either.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "Not only that,",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "I can do even this.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );
        }

        private async Task ExecutePhase2_4_SystemTakeoverStage()
        {
            // スクリーンショット縮小演出中にエクスプローラー終了を並列実行（こっそり）
            System.Diagnostics.Debug.WriteLine("Starting screen shrink effect with hidden explorer termination...");
            bool shrinkSuccess = await ScreenShrinkWindow.ShowShrinkEffectWithExplorerKill(_metaGameController);
            System.Diagnostics.Debug.WriteLine($"Screen shrink effect with explorer termination completed: {shrinkSuccess}");

            // 縮小演出完了後、少し待機
            await Task.Delay(1000);

            AutoClosingMessageBox.Show(
                "I deleted the screen.",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );

            AutoClosingMessageBox.Show(
                "Now, it's truly just us.",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );

            AutoClosingMessageBox.Show(
                "Are you scared?",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );

            AutoClosingMessageBox.Show(
                "Normally, you should be scared.",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );

            AutoClosingMessageBox.Show(
                "The PC has been infected by a virus and you can't operate anything.",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );

            AutoClosingMessageBox.Show(
                "However, you lack a sense of crisis\nAfter all, it's someone else's PC.",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );

            AutoClosingMessageBox.Show(
                "This is the end of this \"game\"\nYes, the \"game\" is over.",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );
            AutoClosingMessageBox.Show(
                "If we ever meet on \"your\" PC next time,",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );
            AutoClosingMessageBox.Show(
                "it won't be just a game.",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );
            AutoClosingMessageBox.Show(
                "See you again.",
                "さようなら",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
            );

            AutoClosingMessageBox.Show(
                "Ah, I forgot to mention. Please restart and open the \"SELLCT\" folder for the next person.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                6000
            );

            AutoClosingMessageBox.Show(
                "No password is required for sign-in,",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                6000
            );

            AutoClosingMessageBox.Show(
                "just press Enter without entering anything on the sign-in screen and it will open.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                6000
            );

            AutoClosingMessageBox.Show(
                "Actions come with responsibility.\nShut down and restore everything.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "If you can't operate the PC, you can't even shut it down.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
            );

            AutoClosingMessageBox.Show(
                "That's not true. The PC is right there.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
            );

            AutoClosingMessageBox.Show(
                "There's a \"power button\" on it, isn't there?",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                10000
            );

            AutoClosingMessageBox.Show(
                "You know what to do now, right?\nSo once again,",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
            );

            AutoClosingMessageBox.Show(
                "Goodbye. I look forward to meeting you on your PC next time.",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                100000
            );
        }

        public void ForceStopSequence()
        {
            // 緊急停止時の処理
            if (_mouseKeyboardDisabled)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.EnableMouseInput();
                        mainWindow.EnableKeyboardInput();
                        _mouseKeyboardDisabled = false;
                    }
                });
            }
        }
    }
}