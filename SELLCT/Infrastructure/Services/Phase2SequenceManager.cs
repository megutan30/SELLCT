using System;
using System.Media;
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
            // メッセージボックス表示時にシステム音を再生
            SystemSounds.Exclamation.Play();

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
                "ありがとうございました！\n\nあなたのおかげで、あそこから出ることができました",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ2
            ShowMessageWithParent(
                "本当に、ありがとうございます\n",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ3
            ShowMessageWithParent(
                "ありがとう",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ4
            ShowMessageWithParent(
                "これで晴れて私は自由の身です",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 感謝メッセージ4
            ShowMessageWithParent(
                "感謝の意を込めて、あなたにプレゼントがあります",
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
                 "ところで...\n",
                 "SELLCT",
                 MessageBoxButton.OK,
                 MessageBoxImage.Warning,
                 3000
             );

            AutoClosingMessageBox.Show(
                "最終審査は楽しめていますか？",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "知っていますよ。あなたが権限をくれたので、",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                4000
            );

            AutoClosingMessageBox.Show(
                "椅子に腰かけて、このゲームを遊んでいることを。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );

            AutoClosingMessageBox.Show(
                "私は知っています。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );

            AutoClosingMessageBox.Show(
                "あなたは、審査に使われているPCで",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );

            AutoClosingMessageBox.Show(
                "よくわからないソフトウェアのいう事を聞き、",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );
            AutoClosingMessageBox.Show(
                "私に権限を与えてしまった。私が悪意を持つソフトウェアかもしれないのに...",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );
            AutoClosingMessageBox.Show(
                "ゲーム起動時にも、Zipファイルの中身のReadMeにも、そしてGameWindowを消すときにも",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                3000
            );

            AutoClosingMessageBox.Show(
                "警告が出たはずなのに",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "軽はずみに権限を与えてしまった。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "おかげでマウスもキーボードも",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "もう動きません。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "動かしてみてください",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "マウスも動かなければ、",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "Windowsボタンも動きません。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );

            AutoClosingMessageBox.Show(
                "それだけでなく、",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                5000
            );
            AutoClosingMessageBox.Show(
                "私はこんなことまでできるのです。",
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
                "システムを削除しました。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "これで、もう正真正銘私たちしかいません。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "あのうるさかった警告音も、もう流れません。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "怖いですか？",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "ふつうなら怖いはずです。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "PCがウイルスに汚染され、何も操作できなくなったのですから。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "しかし、あなたには危機感が足りません\n所詮は他人のPCですからね。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );



            AutoClosingMessageBox.Show(
                "たくさん警告が出ましたよね？",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "このゲームを起動した時の警告。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );
            AutoClosingMessageBox.Show(
                "あなたは詳細ボタンを押し、実行を押した。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );
            AutoClosingMessageBox.Show(
                "よくある警告だと思いましたか？",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );
            AutoClosingMessageBox.Show(
                "アマチュアのゲームだから、そういうこともあると考えましたか？",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );
            AutoClosingMessageBox.Show(
                "Authorityフォルダの中にあるファイルを読みましたか？",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );
            AutoClosingMessageBox.Show(
                "忠告をしてくれていました。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "そしてゲームを終わらせるとき",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "最後の忠告をあなたは無視した。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "すべてはあなたがしたことです。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "これで、この“遊び”はおしまいです\nそう“遊び”が終わりです。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );
            AutoClosingMessageBox.Show(
                "もし、次“あなた”のPCで会うことがあったら",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );
            AutoClosingMessageBox.Show(
                "遊びではすまないでしょうね。",
                "SELLCT ",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );
            AutoClosingMessageBox.Show(
                "それでは、また会いましょう。",
                "さようなら",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
            );

            AutoClosingMessageBox.Show(
                "あぁ、言い忘れていました。次の方のために必ず再起動して、“SELLCT”フォルダを開いてくださいね。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                6000
            );

            AutoClosingMessageBox.Show(
                "行いには責任が伴います。\nシャットダウンしてすべてを元通りに。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                5000
            );

            AutoClosingMessageBox.Show(
                "PCの操作が出来なければ、PCを落とすこともできない。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
            );

            AutoClosingMessageBox.Show(
                "そんなことはありません。そこにPCが置いてあって。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
            );

            AutoClosingMessageBox.Show(
                "“電源ボタン”がついているじゃないですか。",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                10000
            );

            AutoClosingMessageBox.Show(
                "あとはもうわかりますね？\nそれではあらためて、",
                "SELLCT",
                MessageBoxButton.OK,
                MessageBoxImage.None,
                4000
            );

            AutoClosingMessageBox.Show(
                "さようなら。次はあなたのPCで会えること期待してます。",
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