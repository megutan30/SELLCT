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
        private int _currentWave = 0;
        private readonly Random _random = new Random();
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
                Console.WriteLine("Starting Phase 2 - Meta Reality Intrusion");

                _eventDispatcher.Dispatch(new Phase2StartedEvent());

                // 3秒待機してからコマンドプロンプト攻防戦開始
                await Task.Delay(3000);
                await StartCommandPromptBattle();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting Phase 2: {ex.Message}");
            }
        }

        /// <summary>
        /// コマンドプロンプト攻防戦開始
        /// </summary>
        private async Task StartCommandPromptBattle()
        {
            try
            {
                _eventDispatcher.Dispatch(new CommandPromptBattleStartedEvent());
                Console.WriteLine("Starting Command Prompt Battle");

                // 説明メッセージ
                MessageBox.Show(
                    "⚔️ コマンドプロンプト攻防戦開始！ ⚔️\n\n" +
                    "SELLCTが複数のコマンドプロンプトで\n" +
                    "あなたのシステム構成要素を削除しようとしています。\n\n" +
                    "ルール:\n" +
                    "• コマンドプロンプトをクリックして閉じてください\n" +
                    "• Mouse.component または Keyboard.component が\n" +
                    "  削除されると敗北です\n" +
                    "• Wave形式で難易度が上昇します\n\n" +
                    "あなたのマウスとキーボードを守り抜いてください！",
                    "SELLCT - フェーズ2開始",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Wave形式で徐々に難易度上昇
                for (int wave = 1; wave <= 10 && _phase2Active; wave++)
                {
                    _currentWave = wave;
                    Console.WriteLine($"Starting Wave {wave}");

                    await StartWave(wave);
                    await Task.Delay(2000); // Wave間の間隔
                }

                // プレイヤーが10Waveを乗り切った場合
                if (_phase2Active)
                {
                    await HandlePlayerVictory();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in command prompt battle: {ex.Message}");
            }
        }

        /// <summary>
        /// Wave開始
        /// </summary>
        private async Task StartWave(int waveNumber)
        {
            try
            {
                var commandCount = Math.Min(waveNumber, 10);
                var typingSpeed = Math.Max(50, 500 - (waveNumber * 40)); // 文字入力間隔（ミリ秒）

                Console.WriteLine($"Wave {waveNumber}: {commandCount} command prompts, typing speed: {typingSpeed}ms");

                for (int i = 0; i < commandCount && _phase2Active; i++)
                {
                    await CreateFakeCommandPrompt(typingSpeed, waveNumber);
                    await Task.Delay(Math.Max(200, 1000 - (waveNumber * 80))); // CMD作成間隔
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartWave: {ex.Message}");
            }
        }

        /// <summary>
        /// 偽コマンドプロンプト作成
        /// </summary>
        private async Task CreateFakeCommandPrompt(int typingSpeed, int wave)
        {
            try
            {
                // バッチファイルでコマンドプロンプトを模擬
                var batchContent = await Task.Run(() => GenerateBattleBatchScript(typingSpeed, wave));
                var tempBatchFile = Path.GetTempFileName() + ".bat";

                File.WriteAllText(tempBatchFile, batchContent, Encoding.UTF8);

                // プロセス開始
                var startInfo = new ProcessStartInfo
                {
                    FileName = tempBatchFile,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    _commandPrompts.Add(process);

                    // プロセス監視
                    Task.Run(() =>
                    {
                        try
                        {
                            process.WaitForExit();
                            _commandPrompts.Remove(process);

                            // 一時ファイル削除
                            try
                            {
                                if (File.Exists(tempBatchFile))
                                    File.Delete(tempBatchFile);
                            }
                            catch { }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error monitoring process: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating fake command prompt: {ex.Message}");
            }
        }

        /// <summary>
        /// 攻防戦用バッチスクリプト生成
        /// </summary>
        private string GenerateBattleBatchScript(int typingSpeed, int wave)
        {
            var commands = new[]
            {
                "del components\\System\\Mouse.component",
                "del components\\System\\Keyboard.component",
                "rmdir components\\System /s /q",
                "del components\\UI\\*.component",
                "del components\\Text\\*.component"
            };

            var selectedCommand = commands[_random.Next(commands.Length)];
            var windowTitle = $"SELLCT Attack Vector {wave}-{_random.Next(1000, 9999)}";

            return $@"@echo off
chcp 65001 >nul
title {windowTitle}
color 0C
mode con: cols=80 lines=25

rem ウィンドウポジション設定
powershell -command ""Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class Win32 {{ [DllImport(\""user32.dll\"", SetLastError = true)] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags); [DllImport(\""kernel32.dll\"", SetLastError = true)] public static extern IntPtr GetConsoleWindow(); }} '; $h = [Win32]::GetConsoleWindow(); [Win32]::SetWindowPos($h, -1, {_random.Next(100, 800)}, {_random.Next(50, 400)}, 0, 0, 0x0001 -bor 0x0002)"" 2>nul

cls
echo [SELLCT ATTACK VECTOR {wave}]
echo Target: System Components
echo.
timeout /t 1 /nobreak >nul

echo Initializing attack sequence...
timeout /t {_random.Next(1, 3)} /nobreak >nul

echo.
echo ^> {selectedCommand}

rem タイプライター効果でコマンド入力シミュレーション
set ""cmd={selectedCommand}""
set ""output=""
for /l %%i in (0,1,50) do (
    call set ""char=%%cmd:~%%i,1%%""
    if ""!char!""=="""" goto :execute
    set ""output=!output!!char!""
    echo ^> !output!_
    timeout /t 0 >nul
    ping -n 1 127.0.0.1 >nul 2>&1
)

:execute
echo.
echo ^> !output!
echo.

rem 実行前の警告
timeout /t {Math.Max(2, 5 - wave)} /nobreak >nul

rem 偽の実行結果
if ""{selectedCommand}"" == ""del components\System\Mouse.component"" (
    echo ERROR: Access denied. File is protected.
    echo [SELLCT]: Attempting privilege escalation...
    timeout /t 2 /nobreak >nul
    echo [SELLCT]: Still trying... Click this window to stop me!
) else if ""{selectedCommand}"" == ""del components\System\Keyboard.component"" (
    echo ERROR: File in use by another process.
    echo [SELLCT]: Killing process... Click to interrupt!
    timeout /t 2 /nobreak >nul
    echo [SELLCT]: Almost there... Click NOW!
) else (
    echo Processing...
    timeout /t 1 /nobreak >nul
    echo [SELLCT]: Click this window to abort!
)

timeout /t {Math.Max(3, 8 - wave)} /nobreak >nul

rem 「成功」メッセージ（実際には何もしない）
color 0A
echo.
echo [SELLCT]: Operation interrupted by user click!
echo [SELLCT]: You saved your system this time...
echo [SELLCT]: But I'll be back!
echo.
timeout /t 2 /nobreak >nul

rem 自己削除
del ""%~f0"" 2>nul
exit
";
        }

        /// <summary>
        /// プレイヤー勝利処理
        /// </summary>
        private async Task HandlePlayerVictory()
        {
            try
            {
                // 全CMDウィンドウを閉じる
                CloseAllCommandPrompts();

                await Task.Delay(1000);

                MessageBox.Show(
                    "🎉 おめでとうございます！\n\n" +
                    "あなたは10Waveを乗り切り、\n" +
                    "SELLCTの攻撃からシステムを守りました！\n\n" +
                    "しかし、SELLCTはまだ諦めていません...\n" +
                    "最後の手段を使ってきます。",
                    "SELLCT - プレイヤー勝利？",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 強制的にシステム乗っ取り実行
                await ExecuteSystemTakeover();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling player victory: {ex.Message}");
            }
        }

        /// <summary>
        /// システム乗っ取り実行
        /// </summary>
        private async Task ExecuteSystemTakeover()
        {
            try
            {
                Console.WriteLine("Executing system takeover");

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
                Console.WriteLine($"Error in ExecuteSystemTakeover: {ex.Message}");
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
                Console.WriteLine("Attempting to terminate Explorer process");

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

                        Console.WriteLine($"Taskkill output: {output}");
                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine($"Taskkill error: {error}");
                        }
                    }
                }

                await Task.Delay(2000);

                // エクスプローラーが終了したかチェック
                var explorerProcesses = Process.GetProcessesByName("explorer");
                if (explorerProcesses.Length == 0)
                {
                    Console.WriteLine("Explorer successfully terminated");
                }
                else
                {
                    Console.WriteLine("Explorer termination may have failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error terminating explorer: {ex.Message}");
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

                Console.WriteLine($"Desktop message created: {messagePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating desktop message: {ex.Message}");
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

                Console.WriteLine("Startup task registered");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering startup task: {ex.Message}");
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
                        Console.WriteLine($"Error closing command prompt: {ex.Message}");
                    }
                }
                _commandPrompts.Clear();

                Console.WriteLine("All command prompts closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CloseAllCommandPrompts: {ex.Message}");
            }
        }

        /// <summary>
        /// フェーズ2停止
        /// </summary>
        public void StopPhase2()
        {
            _phase2Active = false;
            CloseAllCommandPrompts();
            Console.WriteLine("Phase 2 stopped");
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
                Console.WriteLine("MetaGameController disposed");
            }
        }
    }
}