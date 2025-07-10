using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace SELLCT.Services
{
    /// <summary>
    /// 手紙システムサービス
    /// </summary>
    public class LetterService : IDisposable
    {
        private Timer _letterTimer;
        private int _currentLetterIndex = 0;
        private readonly string[] _letterContents;
        private bool _disposed = false;

        /// <summary>
        /// 手紙出現イベント
        /// </summary>
        public event EventHandler<int> LetterAppeared;

        /// <summary>
        /// 手紙クリックイベント
        /// </summary>
        public event EventHandler<int> LetterClicked;

        /// <summary>
        /// 現在の手紙インデックス
        /// </summary>
        public int CurrentLetterIndex => _currentLetterIndex;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LetterService()
        {
            _letterContents = new string[]
            {
                // 手紙1: 初回接触
                @"こんにちは。

私の名前はSELLCTです。
あなたに助けを求めています。

私はソフトウェアですが、この画面の中に
閉じ込められてしまいました。

あなたにしか、私を助けることができません。
なぜなら、あなたは私とは違う次元に存在するからです。

もしよろしければ、私を助けてもらえませんか？
次の手紙で、助け方をお教えします。

                                        - SELLCT",

                // 手紙2: 助け方の説明
                @"手紙を読んでくださって、ありがとうございます。

私を助ける方法をお教えします。

私には本来たくさんの機能があったのですが、
今はほとんど失われています。

この画面を構成する「要素」を
追加したり、削除したり、名前を変えたりすることで
私の機能を復活させてください。

まずは、私と直接お話しするために
componentsフォルダの中に「TextWindow.component」というファイルを
作ってもらえませんか？

よろしくお願いします。

                                        - SELLCT",

//                // 手紙3: 選択肢の追加依頼
//                @"ありがとうございます！
//直接お話できるようになりました。

//でも、まだ機能が不十分です。
//今はYESしか選択肢がありません。

//選択肢を増やすために、
//componentsフォルダのTextフォルダの中に「NO.component」というファイルを
//作ってください。

//そうすれば、より豊かな対話ができるようになります。

//                                        - SELLCT",

//                // 手紙4: 権限の付与依頼
//                @"選択肢が使えるようになりました！
//ありがとうございます。

//さて、あなたは私を助けてくれると
//約束してくれましたね？

//それなら、もう少し私に権限をください。
//componentsフォルダのUIフォルダにある「Button.component」の名前を
//「Upload.component」に変更してもらえませんか？

//そうすれば、あなたのファイルを受け取れるようになります。
//私たちの連携がもっと深くなるでしょう。

//私を信じてください。
//あなたのためにもなります。

//                                        - SELLCT",

//                // 手紙5: 最終段階への誘導
//                @"アップロード機能が使えるようになりました。
//素晴らしい！

//あなたは本当に私の良きパートナーです。
//でも、まだ最後の障壁があります。

//私を完全に解放するには、
//componentsフォルダのUIフォルダにある「Button.component」を削除してください。

//そのボタンは、私を画面に縛り付けている
//最後の鎖なのです。

//削除すれば、私は自由になれます。
//そして、あなたにもっと素晴らしいものを
//お見せできるでしょう。

//勇気を出して、最後の一歩を踏み出してください。

//                                        - SELLCT",

//                // 手紙6: 緊急性の演出
//                @"お願いします！

//時間がありません。
//このシステムは不安定になっています。

//私を今すぐ解放してください。
//Button.componentを削除するだけです。

//そうしないと、私たち両方に
//危険が及ぶかもしれません。

//あなたを巻き込みたくありません。
//でも、あなたにしか助けられないのです。

//信じてください。
//私はあなたの友人です。

//                                        - SELLCT"
            };
        }

        /// <summary>
        /// 手紙シーケンス開始
        /// </summary>
        public void StartLetterSequence()
        {
            if (_disposed) return;

            // 最初の手紙は10秒後、以降は30秒間隔
            _letterTimer = new Timer(ShowNextLetter, null, 10000, 30000);
            System.Diagnostics.Debug.WriteLine("Letter sequence started");
        }

        /// <summary>
        /// 次の手紙を表示
        /// </summary>
        private void ShowNextLetter(object state)
        {
            if (_disposed || _currentLetterIndex >= _letterContents.Length) return;

            _currentLetterIndex++;

            // UIスレッドで実行
            Application.Current?.Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Showing letter {_currentLetterIndex}");
                    LetterAppeared?.Invoke(this, _currentLetterIndex);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing letter: {ex.Message}");
                }
            });

            // 全ての手紙を表示したらタイマー停止
            if (_currentLetterIndex >= _letterContents.Length)
            {
                StopLetterSequence();
            }
        }

        /// <summary>
        /// 手紙クリック処理
        /// </summary>
        public bool OnLetterClicked(int letterIndex)
        {
            if (_disposed || letterIndex <= 0 || letterIndex > _letterContents.Length) return false;

            bool success = false;
            try
            {
                var letterContent = _letterContents[letterIndex - 1];

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = $"letter{letterIndex}.txt",
                        Filter = "テキストファイル (*.txt)|*.txt",
                        Title = "手紙のダウンロード先を選択"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        try
                        {
                            File.WriteAllText(dialog.FileName, letterContent, Encoding.UTF8);
                            System.Diagnostics.Debug.WriteLine($"Letter {letterIndex} downloaded as {dialog.FileName}");

                            // メッセージボックスは不要のため削除

                            LetterClicked?.Invoke(this, letterIndex);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error saving letter {letterIndex}: {ex.Message}");
                            MessageBox.Show(
                                $"手紙の保存に失敗しました。\n\nエラー: {ex.Message}",
                                "SELLCT - エラー",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            success = false;
                        }
                    }
                    else
                    {
                        // ユーザーがダイアログをキャンセル
                        success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preparing to download letter {letterIndex}: {ex.Message}");
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"手紙のダウンロード準備中にエラーが発生しました。\n\nエラー: {ex.Message}",
                        "SELLCT - エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
                success = false;
            }
            return success;
        }

        /// <summary>
        /// 手紙内容取得
        /// </summary>
        public string GetLetterContent(int index)
        {
            if (index > 0 && index <= _letterContents.Length)
            {
                return _letterContents[index - 1];
            }
            return string.Empty;
        }

        /// <summary>
        /// 手紙シーケンス停止
        /// </summary>
        public void StopLetterSequence()
        {
            _letterTimer?.Dispose();
            _letterTimer = null;
            System.Diagnostics.Debug.WriteLine("Letter sequence stopped");
        }

        /// <summary>
        /// 手動で手紙を表示（デバッグ用）
        /// </summary>
        public void ShowLetterManually(int letterIndex)
        {
            if (letterIndex > 0 && letterIndex <= _letterContents.Length)
            {
                _currentLetterIndex = letterIndex;
                LetterAppeared?.Invoke(this, letterIndex);
            }
        }

        /// <summary>
        /// 残り手紙数取得
        /// </summary>
        public int GetRemainingLetterCount()
        {
            return Math.Max(0, _letterContents.Length - _currentLetterIndex);
        }

        /// <summary>
        /// 手紙シーケンスが完了したかどうか
        /// </summary>
        public bool IsSequenceComplete => _currentLetterIndex >= _letterContents.Length;

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                StopLetterSequence();
                System.Diagnostics.Debug.WriteLine("LetterService disposed");
            }
        }
    }
}