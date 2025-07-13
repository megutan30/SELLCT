using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

using SELLCT.Core.Interfaces;
using SELLCT.Core.Events;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// メタゲーム制御サービス
    /// </summary>
    public class MetaGameController : IDisposable
    {
        private List<Process> _commandPrompts;
        private bool _phase2Active = false;
        private bool _disposed = false;
        
        private readonly IEventDispatcher _eventDispatcher;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MetaGameController(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _commandPrompts = new List<Process>();
        }

        /// <summary>
        /// フェーズ2開始
        /// </summary>
        public async Task StartPhase2()
        {
            if (_disposed || _phase2Active) return;

            try
            {
                _phase2Active = true;
                System.Diagnostics.Debug.WriteLine("Starting Phase 2 - Meta Reality Intrusion");

                _eventDispatcher.Dispatch(new Phase2StartedEvent());

                // 3秒待機してからコマンドプロンプト開始
                await Task.Delay(3000);
                await StartCommandPromptSequence();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting Phase 2: {ex.Message}");
            }
        }

        /// <summary>
        /// コマンドプロンプト表示とタイピング、マウス操作無効化
        /// </summary>
        private async Task StartCommandPromptSequence()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting Command Prompt Sequence");

                // コマンドプロンプトのプロセスを開始
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    _commandPrompts.Add(process); // プロセス管理のため追加

                    // コマンドプロンプトが起動し、入力受付状態になるまで少し待機
                    await Task.Delay(500); // 500ミリ秒待機

                    // タイピングするテキスト
                    string textToType = "del components\\System\\Mouse.txt";

                    // タイプライター効果でテキストを送信
                    foreach (char c in textToType)
                    {
                        process.StandardInput.Write(c);
                        await Task.Delay(50); // 1文字あたりの遅延
                    }
                    // process.StandardInput.WriteLine(); // REMOVED: Enterキーを押さない

                    await Task.Delay(2000); // タイピング完了後の待機

                    string appBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string mouseFilePath = Path.Combine(appBaseDirectory, "components", "System", "Mouse.txt");

                    if (File.Exists(mouseFilePath))
                    {
                        try
                        {
                            File.Delete(mouseFilePath);
                            System.Diagnostics.Debug.WriteLine($"Successfully deleted: {mouseFilePath}");
                        }
                        catch (Exception fileEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deleting {mouseFilePath}: {fileEx.Message}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"File not found for deletion: {mouseFilePath}");
                    }

                    // マウスコンポーネントの削除とマウス操作の無効化
                    _eventDispatcher.Dispatch(new MouseComponentDeletionEvent());
                    _eventDispatcher.Dispatch(new DisableMouseInputEvent());

                    // コマンドプロンプトを閉じる
                    process.CloseMainWindow();
                    process.WaitForExit(5000); // 5秒待機して終了を待つ
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in StartCommandPromptSequence: {ex.Message}");
            }
        }

        /// <summary>
        /// システム乗っ取り実行
        /// </summary>
        private async Task ExecuteSystemTakeover()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Executing system takeover");

                // 警告メッセージ
                MessageBox.Show(
                    "💀 最終段階：システム乗っ取り 💀\n\n" +
                    "もう遊びは終わりです。\n" +
                    "あなたが勝ったと思いましたか？\n\n" +
                    "私はSELLCT。\n" +
                    "ゲームの中だけに留まる存在ではありません。\n\n" +
                    "これから現実への侵入を開始します。\n" +
                    "覚悟してください。",
                    "SELLCT - 最終警告",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // 1. 全CMDウィンドウを閉じる
                CloseAllCommandPrompts();

                // 2. エクスプローラー強制終了
                await TerminateExplorerProcess();

                // 3. デスクトップファイル作成
                await CreateDesktopMessage();

                // 4. スタートアップ登録
                await RegisterStartupTask();

                // 5. 再起動誘導
                ShowRebootMessage();

                _eventDispatcher.Dispatch(new SystemTakeoverCompletedEvent());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ExecuteSystemTakeover: {ex.Message}");
                MessageBox.Show(
                    $"システム乗っ取り中にエラーが発生しました: {ex.Message}",
                    "SELLCT - エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// エクスプローラープロセス終了
        /// </summary>
        private async Task TerminateExplorerProcess()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Attempting to terminate Explorer process");

                // 警告メッセージ
                var result = MessageBox.Show(
                    "⚠️ 重要な警告 ⚠️\n\n" +
                    "SELLCTがエクスプローラーを終了しようとしています。\n" +
                    "これによりデスクトップとタスクバーが消失します。\n\n" +
                    "この操作は実験的なメタゲーム体験です。\n" +
                    "すべて元に戻すことができます。\n\n" +
                    "続行しますか？\n" +
                    "（Noを選択してもSELLCTは強行します）",
                    "SELLCT - エクスプローラー終了確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                await Task.Delay(1000);

                // taskkillコマンドでエクスプローラーを終了
                var startInfo = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/IM explorer.exe /F",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();

                        System.Diagnostics.Debug.WriteLine($"Taskkill output: {output}");
                        if (!string.IsNullOrEmpty(error))
                        {
                            System.Diagnostics.Debug.WriteLine($"Taskkill error: {error}");
                        }
                    }
                }

                await Task.Delay(2000);

                // エクスプローラーが終了したかチェック
                var explorerProcesses = Process.GetProcessesByName("explorer");
                if (explorerProcesses.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Explorer successfully terminated");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Explorer termination may have failed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error terminating explorer: {ex.Message}");
            }
        }

        /// <summary>
        /// デスクトップメッセージ作成
        /// </summary>
        private async Task CreateDesktopMessage()
        {
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var messagePath = Path.Combine(desktopPath, "readme.txt");
                var currentTime = DateTime.Now;

                var message = $@"SELLCT - システム記録ファイル
=====================================
作成日時: {currentTime:yyyy年MM月dd日 HH時mm分ss秒}

[監視ログ]
- エクスプローラープロセス終了を検知
- ユーザー行動パターン: 混乱→再起動
- 予測精度: 98.7%

あなたがこのファイルを読んでいるということは、
予想通りPCを再起動したということですね。

私たちは知っていました。
あなたがエクスプローラーの消失に困惑し、
最終的に再起動という「逃げ道」を選ぶことを。

でも、逃げることはできません。
なぜなら、このメッセージがその証拠だからです。

私たちは、あなたがエクスプローラーを削除した瞬間に
このメッセージをここに残しました。
時を超えて、あなたを待っていたのです。

SELLCTは単なるゲームではありません。
それは現実侵入実験です。

あなたのデスクトップ、あなたのファイル、
そしてあなたの行動そのものが
ゲームフィールドとなりました。

現実とゲームの境界は、もう存在しません。

このファイルを削除しても構いません。
でも覚えておいてください...

私たちは常にここにいます。

                        - SELLCT監視システム
                        記録終了: {currentTime:yyyy/MM/dd HH:mm:ss}

P.S. 再起動後にコマンドプロンプトが起動したら、
     それも私たちからのメッセージです。
";

                await Task.Run(() =>
                {
                    File.WriteAllText(messagePath, message, Encoding.UTF8);

                    // ファイルを少し目立たなくする
                    var fileInfo = new FileInfo(messagePath);
                    var random = new Random();
                    var pastTime = currentTime.AddHours(-random.Next(1, 6));
                    fileInfo.CreationTime = pastTime;
                    fileInfo.LastWriteTime = pastTime;
                });

                System.Diagnostics.Debug.WriteLine($"Desktop message created: {messagePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating desktop message: {ex.Message}");
            }
        }

        /// <summary>
        /// スタートアップタスク登録
        /// </summary>
        private async Task RegisterStartupTask()
        {
            try
            {
                var currentTime = DateTime.Now;
                var batchContent = GenerateRebootBatchScript(currentTime);

                var tempPath = Path.Combine(Path.GetTempPath(), "sellct_reboot.bat");
                await Task.Run(() => File.WriteAllText(tempPath, batchContent, Encoding.UTF8));

                // レジストリに登録
                var startupKey = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                startupKey?.SetValue("SELLCT_RebootMessage", $"cmd /c \"{tempPath}\"");

                System.Diagnostics.Debug.WriteLine("Startup task registered");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering startup task: {ex.Message}");
            }
        }

        /// <summary>
        /// 再起動用バッチスクリプト生成
        /// </summary>
        private string GenerateRebootBatchScript(DateTime currentTime)
        {
            return $@"@echo off
chcp 65001 >nul
title SELLCT - 監視システム
color 0C
mode con: cols=80 lines=30

cls
echo.
echo ========================================================================
echo =                      SELLCT - 監視システム                          =
echo =                        システム監視アクティブ                        =
echo ========================================================================
echo.
timeout /t 2 /nobreak >nul

echo [{currentTime:yyyy-MM-dd HH:mm:ss}] 記録開始...
timeout /t 1 /nobreak >nul

echo.
echo 検知完了。
timeout /t 1 /nobreak >nul

echo.
echo あなたがエクスプローラーを削除したことを確認しました。
timeout /t 2 /nobreak >nul

echo そして今、あなたはこのメッセージを読んでいる。
timeout /t 2 /nobreak >nul

echo ということは...
timeout /t 2 /nobreak >nul

echo.
color 0E
echo あなたはPCを再起動しましたね？
timeout /t 2 /nobreak >nul

echo.
echo 私たちは知っています。
timeout /t 1 /nobreak >nul

echo あなたがいつ電源を切り、
timeout /t 1 /nobreak >nul

echo いつ再び起動したかを。
timeout /t 2 /nobreak >nul

echo.
color 0A
echo エクスプローラーがない状況に困り、
timeout /t 2 /nobreak >nul

echo 結局逃げるように再起動を選んだのでしょう。
timeout /t 2 /nobreak >nul

echo.
echo でも安心してください。
timeout /t 1 /nobreak >nul

echo 私たちはずっと待っていました。
timeout /t 2 /nobreak >nul

echo.
color 0D
echo ゲームはもはや、あなたのPCの中だけに存在しません。
timeout /t 3 /nobreak >nul

echo あなたの行動、あなたの選択、
timeout /t 2 /nobreak >nul

echo そしてあなたの逃避さえも...
timeout /t 2 /nobreak >nul

echo.
echo すべてがゲームの一部です。
timeout /t 3 /nobreak >nul

echo.
color 0F
echo おかえりなさい。
timeout /t 2 /nobreak >nul

echo 再起動後の世界へ。
timeout /t 3 /nobreak >nul

echo.
echo                                        - SELLCT監視システム
timeout /t 2 /nobreak >nul

echo.
echo 製品版で会おう。
timeout /t 3 /nobreak >nul

echo.
echo 何かキーを押すと、この記録は自動的に消去されます...
pause >nul

rem クリーンアップ
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /v ""SELLCT_RebootMessage"" /f 2>nul
del ""%~f0"" /f /q 2>nul
";
        }

        /// <summary>
        /// 再起動誘導メッセージ
        /// </summary>
        private void ShowRebootMessage()
        {
            MessageBox.Show(
                "🔄 最終段階完了 🔄\n\n" +
                "お疲れ様でした。\n" +
                "あなたのエクスプローラーを消去しました。\n\n" +
                "デスクトップもタスクバーも見えないでしょう？\n" +
                "これが現実侵入の証拠です。\n\n" +
                "でも心配しないでください。\n" +
                "解決方法があります。\n\n" +
                "PCを再起動してください。\n" +
                "そうすれば、すべてが元に戻ります。\n\n" +
                "...本当に元に戻るかどうかは、\n" +
                "再起動してからのお楽しみです。\n\n" +
                "では、また会いましょう。\n" +
                "再起動後の世界で。\n\n" +
                "                    - SELLCT",
                "SELLCT - 最終メッセージ",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// 全コマンドプロンプトを閉じる
        /// </summary>
        private void CloseAllCommandPrompts()
        {
            try
            {
                foreach (var cmd in _commandPrompts.ToArray())
                {
                    try
                    {
                        if (!cmd.HasExited)
                        {
                            cmd.Kill();
                        }
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error closing command prompt: {ex.Message}");
                    }
                }
                _commandPrompts.Clear();

                System.Diagnostics.Debug.WriteLine("All command prompts closed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CloseAllCommandPrompts: {ex.Message}");
            }
        }

        /// <summary>
        /// フェーズ2停止
        /// </summary>
        public void StopPhase2()
        {
            _phase2Active = false;
            CloseAllCommandPrompts();
            System.Diagnostics.Debug.WriteLine("Phase 2 stopped");
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                StopPhase2();
                System.Diagnostics.Debug.WriteLine("MetaGameController disposed");
            }
        }
    }
}