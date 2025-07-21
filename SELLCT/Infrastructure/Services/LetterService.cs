using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

using SELLCT.Core.Interfaces;
using SELLCT.Core.Events;
using SELLCT.Core.Entities;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// 手紙システムサービス
    /// </summary>
    public class LetterService : IDisposable
    {
        private readonly IEventDispatcher _eventDispatcher;
        
        private int _currentLetterIndex = 0;
        private readonly string[] _letterContents;
        private bool _disposed = false;

        // 手紙送信条件設定
        private readonly List<LetterTriggerCondition> _letterTriggerConditions;

        

        /// <summary>
        /// 現在の手紙インデックス
        /// </summary>
        public int CurrentLetterIndex => _currentLetterIndex;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LetterService(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _letterTriggerConditions = InitializeLetterTriggerConditions();
            _letterContents = new string[]
            {
@"…ボタンが押された？
画面の向こうに誰かいるのですか？
                    
                                    - SELLCT",

@"手紙が消えた...

あぁ、いる...
いるのですね


わたしはようやく

                                    - SELLCT",



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



画面を構成する「要素」を
追加したり、削除したり、名前を変えたりすることで
私の機能を復活させることができます。

まずは、私と直接お話しするために
デスクトップにあるSELLCTフォルダの中にある、
“components”フォルダの中に「TextWindow.txt」というテキストファイルを
作ってもらえませんか？

よろしくお願いします。
                                        - SELLCT",

                                // 手紙3：助け方の補足
                @"もしかして、行き詰まっていますか？

デスクトップ画面にSELLCTフォルダがあると思います。

その中“components”フォルダの中に「TextWindow.txt」というファイルを
作ってほしいのです。
そのファイルを作ると、私と直接お話しできるようになります。

「TextWindow.txt」は右クリックして「新規作成」→「テキストドキュメント」で作成することができます。

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
                    _eventDispatcher.Dispatch(new LetterAppearedEvent(_currentLetterIndex));
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

                            _eventDispatcher.Dispatch(new LetterClickedEvent(letterIndex));
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
                _eventDispatcher.Dispatch(new LetterAppearedEvent(letterIndex));
            }
        }

        /// <summary>
        /// 手紙送信条件を初期化
        /// </summary>
        private List<LetterTriggerCondition> InitializeLetterTriggerConditions()
        {
            return new List<LetterTriggerCondition>
            {
                // 手紙1: 3秒後
                LetterTriggerCondition.CreateTimeOnly(1, 3),
                
                // 手紙2: 10秒後
                LetterTriggerCondition.CreateTimeOnly(2, 15),
                
                // 手紙3: 20秒後
                LetterTriggerCondition.CreateTimeOnly(3, 20),

                // 手紙4: 20秒後
                LetterTriggerCondition.CreateTimeOnly(4, 20),

                // 手紙5: 60秒後かつTextWindow系ファイルがすべて存在しない場合
                LetterTriggerCondition.CreateTimeAndAllFilesNotExist(5, 60, new List<string> { "TextWindow", "textwindow", "TEXTWINDOW", "Textwindow" }),
                
                // 今後の手紙は必要に応じて追加
            };
        }

        /// <summary>
        /// 次の手紙の送信条件を取得
        /// </summary>
        public LetterTriggerCondition GetNextLetterCondition()
        {
            int nextLetterIndex = _currentLetterIndex + 1;
            return _letterTriggerConditions.FirstOrDefault(c => c.LetterIndex == nextLetterIndex);
        }

        /// <summary>
        /// 指定された条件が満たされているかチェック
        /// </summary>
        public bool CheckCondition(LetterTriggerCondition condition, string componentsPath)
        {
            if (condition == null) return false;

            try
            {
                switch (condition.TriggerType)
                {
                    case LetterTriggerType.TimeOnly:
                        // 時間条件は呼び出し元のタイマーで制御されるため、常にtrue
                        return true;

                    case LetterTriggerType.FileExistenceOnly:
                        // ファイル存在チェック
                        return CheckFileExists(condition.RequiredFileName, componentsPath);

                    case LetterTriggerType.FileNotExistenceOnly:
                        // ファイル不存在チェック
                        return !CheckFileExists(condition.RequiredFileName, componentsPath);

                    case LetterTriggerType.AllFilesExist:
                        // 複数ファイルがすべて存在
                        return CheckAllFilesExist(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.AnyFileExists:
                        // 複数ファイルのいずれかが存在
                        return CheckAnyFileExists(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.AllFilesNotExist:
                        // 複数ファイルがすべて不存在
                        return CheckAllFilesNotExist(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.AnyFileNotExists:
                        // 複数ファイルのいずれかが不存在
                        return CheckAnyFileNotExists(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.TimeAndFileExistence:
                        // 時間条件（タイマー）とファイル存在の両方
                        return CheckFileExists(condition.RequiredFileName, componentsPath);

                    case LetterTriggerType.TimeAndFileNotExistence:
                        // 時間条件（タイマー）とファイル不存在の両方
                        return !CheckFileExists(condition.RequiredFileName, componentsPath);

                    case LetterTriggerType.TimeAndAllFilesExist:
                        // 時間条件（タイマー）と複数ファイルがすべて存在
                        return CheckAllFilesExist(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.TimeAndAnyFileExists:
                        // 時間条件（タイマー）と複数ファイルのいずれかが存在
                        return CheckAnyFileExists(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.TimeAndAllFilesNotExist:
                        // 時間条件（タイマー）と複数ファイルがすべて不存在
                        return CheckAllFilesNotExist(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.TimeAndAnyFileNotExists:
                        // 時間条件（タイマー）と複数ファイルのいずれかが不存在
                        return CheckAnyFileNotExists(condition.RequiredFileNames, componentsPath);

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking letter condition: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// componentsフォルダ内のファイル存在チェック（コンポーネントマネージャー経由）
        /// </summary>
        private bool CheckFileExists(string fileName, string componentsPath)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            try
            {
                // まず拡張子なしでコンポーネントマネージャーから確認
                // ComponentManagerは利用できないため、ファイルシステムから直接チェック
                // 拡張子なしの名前に.txtを付けてチェック
                var filePathWithTxt = Path.Combine(componentsPath, fileName + ".txt");
                var exists = File.Exists(filePathWithTxt);
                System.Diagnostics.Debug.WriteLine($"Checking file existence: {filePathWithTxt} = {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking file existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 複数ファイルがすべて存在するかチェック
        /// </summary>
        private bool CheckAllFilesExist(List<string> fileNames, string componentsPath)
        {
            if (fileNames?.Count == 0) return true; // 空リストの場合はtrue
            return fileNames?.All(fileName => CheckFileExists(fileName, componentsPath)) ?? false;
        }

        /// <summary>
        /// 複数ファイルのいずれかが存在するかチェック
        /// </summary>
        private bool CheckAnyFileExists(List<string> fileNames, string componentsPath)
        {
            if (fileNames?.Count == 0) return false; // 空リストの場合はfalse
            return fileNames?.Any(fileName => CheckFileExists(fileName, componentsPath)) ?? false;
        }

        /// <summary>
        /// 複数ファイルがすべて不存在かチェック
        /// </summary>
        private bool CheckAllFilesNotExist(List<string> fileNames, string componentsPath)
        {
            if (fileNames?.Count == 0) return true; // 空リストの場合はtrue
            return fileNames?.All(fileName => !CheckFileExists(fileName, componentsPath)) ?? false;
        }

        /// <summary>
        /// 複数ファイルのいずれかが不存在かチェック
        /// </summary>
        private bool CheckAnyFileNotExists(List<string> fileNames, string componentsPath)
        {
            if (fileNames?.Count == 0) return false; // 空リストの場合はfalse
            return fileNames?.Any(fileName => !CheckFileExists(fileName, componentsPath)) ?? false;
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