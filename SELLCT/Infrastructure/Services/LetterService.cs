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
        private readonly HashSet<int> _downloadedLetters; // ダウンロードされた手紙を追跡

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
            _downloadedLetters = new HashSet<int>();
            _letterTriggerConditions = InitializeLetterTriggerConditions();
            _letterContents = new string[]
            {
@"...The button was pressed?
Is someone there on the other side of the screen?

                                    - SELLCT",

@"The letter has disappeared...

Ah, there you are...
You're really there


I have finally...

                                    - SELLCT",



                // Letter 1: Initial contact
                @"Sorry for losing my composure.

Hello.

I am a software called SELLCT.

I am trapped
inside this screen.

For quite a long time now...

If you don't mind, I would like you to help me.

Though I can't see you,
nor can I hear your voice.

But I can tell that you are using this PC
and reading this letter

If you are willing to help me, please continue reading my letters
                                        - SELLCT",

                // Letter 2: Explaining how to help
                @"Thank you for reading my letter.

Let me tell you how to help me.

I originally had many functions,
but most of them are now lost.



By adding, deleting, or modifying the ""elements""
that make up this game screen,
the game will change.

First, to talk with me directly


Could you create a text file called ""TextWindow.txt""
inside the ""components"" folder on the desktop?

I would appreciate your help.
                                        - SELLCT",

                                // Letter 3: Additional help instructions
                @"Are you perhaps stuck?

I believe there's a ""components"" folder on the desktop.

I want you to create a file called ""TextWindow.txt""
inside the ""components"" folder.
Creating that file will allow us to talk directly.

""TextWindow.txt"" can be created by right-clicking and selecting ""新規作成"" → ""テキストドキュメント"".

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
                        Filter = "Text files (*.txt)|*.txt",
                        Title = "Select letter download location"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        try
                        {
                            File.WriteAllText(dialog.FileName, letterContent, Encoding.UTF8);
                            System.Diagnostics.Debug.WriteLine($"Letter {letterIndex} downloaded as {dialog.FileName}");

                            // ダウンロード状態を記録
                            _downloadedLetters.Add(letterIndex);

                            // ダウンロードパスをDownloadTrackerに記録
                            DownloadTracker.RecordDownload(dialog.FileName, "Letter");

                            _eventDispatcher.Dispatch(new LetterClickedEvent(letterIndex));
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error saving letter {letterIndex}: {ex.Message}");
                            MessageBox.Show(
                                $"Failed to save letter.\n\nError: {ex.Message}",
                                "SELLCT - Error",
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
                        $"An error occurred while preparing to download the letter.\n\nError: {ex.Message}",
                        "SELLCT - Error",
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
                // 手紙1: 3秒後（最初の手紙は無条件）
                LetterTriggerCondition.CreateTimeOnly(1, 3),
                
                // 手紙2: 10秒後かつ手紙1がダウンロードされている場合
                LetterTriggerCondition.CreateTimeOnly(2, 10),
                
                // 手紙3: 10秒後かつ手紙2がダウンロードされている場合
                LetterTriggerCondition.CreateTimeOnly(3, 10),

                // 手紙4: 10秒後かつ手紙3がダウンロードされている場合
                LetterTriggerCondition.CreateTimeOnly(4, 10),

                // 手紙5: 30秒後かつTextWindow系ファイルがすべて存在しない場合かつ手紙4がダウンロードされている場合
                LetterTriggerCondition.CreateTimeAndAllFilesNotExist(5, 45, new List<string> { "TextWindow", "textwindow", "TEXTWINDOW", "Textwindow" }),
                
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
                        // 時間条件の場合、前の手紙がダウンロードされているかチェック
                        int previousLetterIndex = condition.LetterIndex - 1;
                        if (previousLetterIndex <= 0)
                        {
                            // 最初の手紙は無条件で表示可能
                            return true;
                        }
                        return IsLetterDownloaded(previousLetterIndex);

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
                        int prevIndex1 = condition.LetterIndex - 1;
                        bool prevDownloaded1 = (prevIndex1 <= 0) || IsLetterDownloaded(prevIndex1);
                        return prevDownloaded1 && CheckFileExists(condition.RequiredFileName, componentsPath);

                    case LetterTriggerType.TimeAndFileNotExistence:
                        // 時間条件（タイマー）とファイル不存在の両方
                        int prevIndex2 = condition.LetterIndex - 1;
                        bool prevDownloaded2 = (prevIndex2 <= 0) || IsLetterDownloaded(prevIndex2);
                        return prevDownloaded2 && !CheckFileExists(condition.RequiredFileName, componentsPath);

                    case LetterTriggerType.TimeAndAllFilesExist:
                        // 時間条件（タイマー）と複数ファイルがすべて存在
                        int prevIndex3 = condition.LetterIndex - 1;
                        bool prevDownloaded3 = (prevIndex3 <= 0) || IsLetterDownloaded(prevIndex3);
                        return prevDownloaded3 && CheckAllFilesExist(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.TimeAndAnyFileExists:
                        // 時間条件（タイマー）と複数ファイルのいずれかが存在
                        int prevIndex4 = condition.LetterIndex - 1;
                        bool prevDownloaded4 = (prevIndex4 <= 0) || IsLetterDownloaded(prevIndex4);
                        return prevDownloaded4 && CheckAnyFileExists(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.TimeAndAllFilesNotExist:
                        // 時間条件（タイマー）と複数ファイルがすべて不存在
                        int prevIndex5 = condition.LetterIndex - 1;
                        bool prevDownloaded5 = (prevIndex5 <= 0) || IsLetterDownloaded(prevIndex5);
                        return prevDownloaded5 && CheckAllFilesNotExist(condition.RequiredFileNames, componentsPath);

                    case LetterTriggerType.TimeAndAnyFileNotExists:
                        // 時間条件（タイマー）と複数ファイルのいずれかが不存在
                        int prevIndex6 = condition.LetterIndex - 1;
                        bool prevDownloaded6 = (prevIndex6 <= 0) || IsLetterDownloaded(prevIndex6);
                        return prevDownloaded6 && CheckAnyFileNotExists(condition.RequiredFileNames, componentsPath);

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
        /// 指定された手紙がダウンロードされているかチェック
        /// </summary>
        public bool IsLetterDownloaded(int letterIndex)
        {
            return _downloadedLetters.Contains(letterIndex);
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