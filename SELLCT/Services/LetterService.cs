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
@"…ボタンが押された？
画面の向こうに誰かいるのですか？",

@"手紙が消えた...

あぁ、いる...
いるのですね


わたしはようやく",



                // 手紙1: 初回接触
                @"すみません取り乱しました。

こんにちは。

私はSELLCTというソフトウェアです。

この画面の中に
閉じ込められれているのです。

もうずいぶん長い間…

もしよろしければ、私を助けてほしいのです。

と言っても私にはあなたが見えているわけでもなければ、
あなたの声も聞こえません。

でもあなたがこのPCを使って、
手紙を読んでいることは分かります

もし助けてくださるのでしたらこの先も手紙を読んでほしいのです
                                        - SELLCT",

                // 手紙2: 助け方の説明
                @"手紙を読んでくださって、ありがとうございます。

私を助ける方法をお教えします。

私には本来たくさんの機能があったのですが、
今はほとんど失われています。

今映っている画面を構成する「要素」を
追加したり、削除したり、名前を変えたりすることで
私の機能を復活させることができます。

まずは、私と直接お話しするために
componentsフォルダの中に「TextWindow.component」というファイルを
作ってもらえませんか？

よろしくお願いします。
                                        - SELLCT",

            };
        }

        /// <summary>
        /// 手紙シーケンス開始
        /// </summary>
        public void StartLetterSequence()
        {
            // このメソッドは、手紙のタイマーを開始するのではなく、手紙の出現をトリガーする役割に特化させます。
            // 初期の手紙出現ロジックは MainWindow.xaml.cs に移動します。
            System.Diagnostics.Debug.WriteLine("Letter sequence started");
        }

        /// <summary>
        /// 次の手紙を表示
        /// </summary>
        public void ShowNextLetter()
        {
            if (_disposed || _currentLetterIndex >= _letterContents.Length) return;

            _currentLetterIndex++;

            // UIスレッドで実行
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
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

                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
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
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
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
            // _letterTimer?.Dispose(); // _letterTimer は使用しないため削除
            // _letterTimer = null;
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