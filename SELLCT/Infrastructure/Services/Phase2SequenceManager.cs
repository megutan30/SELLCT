using System;
using System.Threading.Tasks;
using System.Windows;
using SELLCT.Views;

namespace SELLCT.Infrastructure.Services
{
    public class Phase2SequenceManager
    {
        private readonly MetaGameController _metaGameController;
        private bool _mouseKeyboardDisabled = false;

        public Phase2SequenceManager(MetaGameController metaGameController)
        {
            _metaGameController = metaGameController ?? throw new ArgumentNullException(nameof(metaGameController));
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
            MessageBox.Show(
                "おつかれさまでした！\n\nSELLCTをプレイしていただき、ありがとうございました。\nあなたのおかげで、素晴らしい体験ができました。",
                "SELLCT - 感謝",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ2
            MessageBox.Show(
                "このゲームは、プレイヤーの皆さんの創意工夫により成り立っています。\n\nファイルを操作し、謎を解いていく過程で、\n新しい発見があったことでしょう。",
                "SELLCT - 感謝",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ3
            MessageBox.Show(
                "最後に、特別なプレゼントを用意しました。\n\nあなたのコンピューターをより便利にする\n新しい機能をインストールします。",
                "SELLCT - 特別なプレゼント",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ4
            MessageBox.Show(
                "ご安心ください。\n\nこの機能は完全に安全で、\nあなたのシステムを最適化するものです。",
                "SELLCT - 安心してください",
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
            await Task.Delay(3000);
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

            // 騙されたことの発覚メッセージ（自動で閉じる）
            AutoClosingMessageBox.Show(
                "あ、そうそう...\n\n言い忘れていましたが、これは罠でした。",
                "SELLCT - 真実",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                4000
            );

            AutoClosingMessageBox.Show(
                "あなたは今、私たちの「実験」の一部となりました。\n\nマウスとキーボードは、もう使用できません。",
                "SELLCT - システム制御",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                5000
            );

            AutoClosingMessageBox.Show(
                "心配しないでください。\n\nあなたのコンピューターは、今や私たちの管理下にあります。",
                "SELLCT - 新しい管理者",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                4000
            );

            AutoClosingMessageBox.Show(
                "これから、本当の「ゲーム」が始まります。\n\nルールは私たちが決めます。",
                "SELLCT - 本当のゲーム開始",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );
        }

        private async Task ExecutePhase2_4_SystemTakeoverStage()
        {
            // スタートアップ登録の説明
            AutoClosingMessageBox.Show(
                "次に、システムの起動設定を変更します。\n\n今後、コンピューターを起動するたびに\n私たちのプログラムが実行されます。",
                "SELLCT - システム変更",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                6000
            );

            // コマンドプロンプトでエクスプローラー終了演出とスタートアップ登録
            await _metaGameController.TerminateExplorer();
            await _metaGameController.RegisterForStartup();

            await Task.Delay(3000);

            AutoClosingMessageBox.Show(
                "登録完了しました。\n\nあなたのシステムは、永続的に私たちの制御下に置かれます。",
                "SELLCT - 登録完了",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                5000
            );

            AutoClosingMessageBox.Show(
                "もちろん、この状況から逃れる方法はあります。\n\nコンピューターを再起動してください。",
                "SELLCT - 解決方法",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "ただし...\n\n再起動後に何が起こるかは、お楽しみに。",
                "SELLCT - 最後の警告",
                MessageBoxButton.OK,
                MessageBoxImage.Question,
                4000
            );

            AutoClosingMessageBox.Show(
                "ゲームはまだ終わっていません。\n\n真のエンディングは、再起動の先にあります。",
                "SELLCT - ゲーム継続",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                6000
            );

            AutoClosingMessageBox.Show(
                "それでは、また会いましょう。\n\n再起動をお忘れなく...",
                "SELLCT - さようなら",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
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