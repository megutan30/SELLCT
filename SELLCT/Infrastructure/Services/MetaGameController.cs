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
        private Phase2SequenceManager _sequenceManager;
        
        private readonly IEventDispatcher _eventDispatcher;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MetaGameController(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _commandPrompts = new List<Process>();
            _sequenceManager = new Phase2SequenceManager(this);
        }

        /// <summary>
        /// フェーズ2開始（新しいシーケンス版）
        /// </summary>
        public async Task StartPhase2()
        {
            if (_disposed || _phase2Active) return;

            try
            {
                _phase2Active = true;
                System.Diagnostics.Debug.WriteLine("Starting Phase 2 - Enhanced Sequence");

                _eventDispatcher.Dispatch(new Phase2StartedEvent());

                // 新しいシーケンスマネージャーを使用
                await _sequenceManager.ExecutePhase2Sequence();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting Phase 2: {ex.Message}");
                // エラー時にはフォールバック処理
                _sequenceManager.ForceStopSequence();
            }
        }

        /// <summary>
        /// コマンドプロンプト表示とタイピング
        /// </summary>
        private async Task StartCommandPromptSequence()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting Command Prompt Sequence");

                string textToType = "del components\\System\\Mouse.txt";
                string powershellCommand = $"powershell -Command \"$host.ui.RawUI.ForegroundColor = 'White'; function TypeWrite($text) {{ for ($i = 0; $i -lt $text.Length; $i++) {{ Write-Host -NoNewline $text[$i]; Start-Sleep -Milliseconds 50 }}; Write-Host '' }}; TypeWrite '{textToType}'\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {powershellCommand}",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    _commandPrompts.Add(process); 

                    await Task.Run(() => process.WaitForExit());

                    await Task.Delay(1000); 

                    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string mouseFilePath = Path.Combine(desktopPath, "components", "System", "Mouse.txt");

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

                    _eventDispatcher.Dispatch(new MouseComponentDeletionEvent());
                    _eventDispatcher.Dispatch(new DisableMouseInputEvent());
                    
                    if (!process.HasExited)
                    {
                        process.CloseMainWindow();
                        if (!process.WaitForExit(5000))
                        {
                            process.Kill();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in StartCommandPromptSequence: {ex.Message}");
            }
        }

        /// <summary>
        /// エクスプローラープロセス終了
        /// </summary>
        public async Task TerminateExplorerProcess()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Attempting to terminate Explorer process");

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
        /// エクスプローラープロセス開始
        /// </summary>
        public async Task StartExplorerProcess()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting Explorer process");

                // explorer.exeを新しいタスクとして起動
                var startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Explorer process started successfully");
                        
                        // 少し待機してプロセスが正常に起動したか確認
                        await Task.Delay(1000);
                        
                        var explorerProcesses = Process.GetProcessesByName("explorer");
                        System.Diagnostics.Debug.WriteLine($"Explorer processes running: {explorerProcesses.Length}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to start Explorer process");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting explorer: {ex.Message}");
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

                var message = $@"SELLCT - System Record File
=====================================
Created: {currentTime:yyyy/MM/dd HH:mm:ss}

[Surveillance Log]
- Explorer process termination detected
- User behavior pattern: Confusion → Restart
- Prediction accuracy: 98.7%

The fact that you are reading this file means
you restarted your PC as predicted.

We knew.
We knew you would be confused by the disappearance of Explorer,
and ultimately choose the ""escape route"" of restarting.

But you cannot escape.
Because this message is proof of that.

We left this message here the moment
you deleted Explorer.
Waiting for you, transcending time.

SELLCT is not just a game.
It is a reality intrusion experiment.

Your desktop, your files,
and your very actions
have become the game field.

The boundary between reality and game no longer exists.

You may delete this file.
But remember...

We are always here.

                        - SELLCT Surveillance System
                        Recording ended: {currentTime:yyyy/MM/dd HH:mm:ss}

P.S. If a command prompt starts after restart,
     that is also a message from us.
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
                var powerShellScript = GenerateRebootPowerShellScript(currentTime);

                var tempPath = Path.Combine(Path.GetTempPath(), "sellct_reboot.ps1");
                await Task.Run(() => File.WriteAllText(tempPath, powerShellScript, Encoding.UTF8));

                // レジストリに登録
                var startupKey = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                startupKey?.SetValue("SELLCT_RebootMessage", $"powershell.exe -ExecutionPolicy Bypass -File \"{tempPath}\"");

                System.Diagnostics.Debug.WriteLine("Startup task registered");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering startup task: {ex.Message}");
            }
        }

        /// <summary>
        /// 再起動用PowerShellスクリプト生成
        /// </summary>
        private string GenerateRebootPowerShellScript(DateTime currentTime)
        {
            return $@"
# PowerShell スクリプト - フェーズ2スタートアップ演出
$Host.UI.RawUI.WindowTitle = 'SELLCT - 監視システム'
$Host.UI.RawUI.BufferSize = New-Object Management.Automation.Host.Size(80, 3000)
$Host.UI.RawUI.WindowSize = New-Object Management.Automation.Host.Size(80, 30)

# 色の設定
$Host.UI.RawUI.BackgroundColor = 'Black'
$Host.UI.RawUI.ForegroundColor = 'Red'
Clear-Host

# タイプライター効果関数
function TypeWrite($text, $speed = 80) {{
    for ($i = 0; $i -lt $text.Length; $i++) {{
        Write-Host -NoNewline $text[$i]
        Start-Sleep -Milliseconds $speed
    }}
    Write-Host ''
}}

Write-Host ''
TypeWrite '========================================================================'
TypeWrite '=                      SELLCT - 監視システム                          ='
TypeWrite '=                        システム監視アクティブ                        ='
TypeWrite '========================================================================'
Write-Host ''
Start-Sleep -Seconds 1

TypeWrite '[{currentTime:yyyy-MM-dd HH:mm:ss}] 記録開始...'
Start-Sleep -Seconds 1

Write-Host ''
TypeWrite '検知完了。'
Start-Sleep -Seconds 1

Write-Host ''
TypeWrite 'GameWindow.txtを削除したことを確認しました。'
Start-Sleep -Seconds 1

TypeWrite 'そして今、あなたはこのメッセージを読んでいる。'
Start-Sleep -Seconds 1

TypeWrite 'ということは...'
Start-Sleep -Seconds 2

Write-Host ''
$Host.UI.RawUI.ForegroundColor = 'Red'
TypeWrite 'You restarted your PC, didn''t you?' 100
Start-Sleep -Seconds 1

Write-Host ''
TypeWrite 'We know.'
Start-Sleep -Seconds 1

TypeWrite 'When you turned off the power,'
Start-Sleep -Seconds 1

TypeWrite 'and when you started it again.'
Start-Sleep -Seconds 1

Write-Host ''
TypeWrite 'But don''t worry.'
Start-Sleep -Seconds 1

TypeWrite 'We have been waiting all along.'
Start-Sleep -Seconds 1

Write-Host ''
$Host.UI.RawUI.ForegroundColor = 'Magenta'
TypeWrite 'The game no longer exists only within your PC.' 120
Start-Sleep -Seconds 2

TypeWrite 'Your actions, your choices,'
Start-Sleep -Seconds 1

Write-Host ''
TypeWrite 'everything is part of the game.'
Start-Sleep -Seconds 1

Write-Host ''
$Host.UI.RawUI.ForegroundColor = 'White'
TypeWrite 'Welcome back. Please enjoy Tokyo Game Show'
Start-Sleep -Seconds 1

Write-Host ''
TypeWrite '                                        - SELLCT Surveillance System'
Start-Sleep -Seconds 1

Write-Host ''
TypeWrite 'See you again.'
Start-Sleep -Seconds 2

Write-Host ''
TypeWrite 'Press any key and this record will be automatically deleted...' 60
$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown') | Out-Null

# クリーンアップ
try {{
    Remove-ItemProperty -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run' -Name 'SELLCT_RebootMessage' -ErrorAction SilentlyContinue
    Remove-Item -Path $MyInvocation.MyCommand.Path -Force -ErrorAction SilentlyContinue
}} catch {{
    # エラーは無視
}}
";
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

        // Phase2SequenceManager用のパブリックメソッド

        /// <summary>
        /// エクスプローラー終了（公開メソッド）
        /// </summary>
        public async Task TerminateExplorer()
        {
            await TerminateExplorerProcess();
        }

        /// <summary>
        /// スタートアップ登録（公開メソッド）
        /// </summary>
        public async Task RegisterForStartup()
        {
            await RegisterStartupTask();
        }

        /// <summary>
        /// コマンドプロンプトタイピング演出（公開メソッド）
        /// </summary>
        public async Task StartCommandPromptWithTypingEffect()
        {
            await StartCommandPromptSequence();
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