using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// 警告メッセージボックスが画面を埋め尽くす演出を制御するサービス
    /// 教育目的でマルウェアの「ダイアログボム」攻撃を模擬
    /// </summary>
    public class WarningFloodService
    {
        private List<WarningDialog> _activeDialogs = new List<WarningDialog>();
        private DispatcherTimer _spawnTimer;
        private Random _random = new Random();
        
        // 演出の段階制御
        private int _currentPhase = 0;
        private int _dialogsSpawned = 0;
        
        // 演出設定
        private readonly int[] _phaseDurations = { 1000, 500, 200, 100 }; // ミリ秒
        private readonly int[] _phaseTargetCounts = { 3, 9, 24, 50 }; // 各段階での累積目標数
        
        private TaskCompletionSource<bool> _floodCompletionSource;
        
        /// <summary>
        /// 警告の氾濫演出を開始
        /// </summary>
        /// <returns>演出完了を示すTask</returns>
        public Task StartWarningFlood()
        {
            _floodCompletionSource = new TaskCompletionSource<bool>();
            
            // 初期化
            _currentPhase = 0;
            _dialogsSpawned = 0;
            
            // タイマー開始
            StartPhase();
            
            return _floodCompletionSource.Task;
        }
        
        /// <summary>
        /// 現在の段階を開始
        /// </summary>
        private void StartPhase()
        {
            if (_currentPhase >= _phaseDurations.Length)
            {
                // 全段階完了 - 最終演出に進む
                FinishFloodAndShowFinalWarning();
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"Starting phase {_currentPhase + 1} with interval {_phaseDurations[_currentPhase]}ms");
            
            _spawnTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_phaseDurations[_currentPhase])
            };
            _spawnTimer.Tick += SpawnTimerTick;
            _spawnTimer.Start();
        }
        
        /// <summary>
        /// タイマーティック処理 - 警告ダイアログを生成
        /// </summary>
        private void SpawnTimerTick(object sender, EventArgs e)
        {
            SpawnWarningDialog();
            _dialogsSpawned++;
            
            // 現在の段階の目標に達したか確認
            if (_dialogsSpawned >= _phaseTargetCounts[_currentPhase])
            {
                _spawnTimer?.Stop();
                _spawnTimer = null;
                
                _currentPhase++;
                
                // 次の段階に進むか、最終演出に移るかを決定
                if (_currentPhase < _phaseDurations.Length)
                {
                    StartPhase();
                }
                else
                {
                    // 全段階完了
                    FinishFloodAndShowFinalWarning();
                }
            }
        }
        
        /// <summary>
        /// 警告ダイアログを生成して表示
        /// </summary>
        private void SpawnWarningDialog()
        {
            try
            {
                var dialog = new WarningDialog();
                
                // ランダムな位置に配置
                var position = GetRandomScreenPosition();
                dialog.Left = position.X;
                dialog.Top = position.Y;
                
                // ダイアログを表示
                dialog.Show();
                
                // アクティブなダイアログリストに追加
                _activeDialogs.Add(dialog);
                
                System.Diagnostics.Debug.WriteLine($"Spawned dialog {_dialogsSpawned + 1} at ({position.X}, {position.Y})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error spawning warning dialog: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ランダムな画面位置を取得
        /// </summary>
        private Point GetRandomScreenPosition()
        {
            // 主画面のサイズを取得
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            
            // ダイアログのサイズを考慮（おおよそ400x200）
            var dialogWidth = 400;
            var dialogHeight = 200;
            
            // 配置可能な範囲を計算
            var maxX = Math.Max(0, screenWidth - dialogWidth);
            var maxY = Math.Max(0, screenHeight - dialogHeight);
            
            // ランダムな位置を生成
            var x = _random.NextDouble() * maxX;
            var y = _random.NextDouble() * maxY;
            
            return new Point(x, y);
        }
        
        /// <summary>
        /// 氾濫演出を完了し、最終警告を表示
        /// </summary>
        private async void FinishFloodAndShowFinalWarning()
        {
            System.Diagnostics.Debug.WriteLine($"Flood completed with {_activeDialogs.Count} active dialogs. Waiting 1 second...");
            
            // 1秒間待機
            await Task.Delay(1000);
            
            // 画面中央に最終警告を表示
            ShowFinalWarning();
        }
        
        /// <summary>
        /// 画面中央に最終警告を表示
        /// </summary>
        private void ShowFinalWarning()
        {
            try
            {
                var finalDialog = new WarningDialog(isFinalWarning: true);
                
                // 画面中央に配置
                finalDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                
                // 最前面に表示
                finalDialog.Topmost = true;
                
                // ダイアログが閉じられたときの処理
                finalDialog.Closed += (sender, e) =>
                {
                    // 全ての警告ダイアログを閉じる
                    CloseAllDialogs();
                    
                    // 演出完了を通知
                    _floodCompletionSource?.SetResult(true);
                };
                
                System.Diagnostics.Debug.WriteLine("Showing final warning dialog");
                finalDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing final warning: {ex.Message}");
                
                // エラー時も演出完了として処理
                CloseAllDialogs();
                _floodCompletionSource?.SetResult(true);
            }
        }
        
        /// <summary>
        /// 全てのアクティブな警告ダイアログを閉じる
        /// </summary>
        private void CloseAllDialogs()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Closing {_activeDialogs.Count} active dialogs");
                
                foreach (var dialog in _activeDialogs)
                {
                    try
                    {
                        if (dialog.IsLoaded)
                        {
                            dialog.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error closing dialog: {ex.Message}");
                    }
                }
                
                _activeDialogs.Clear();
                
                // タイマーがまだ動いていれば停止
                _spawnTimer?.Stop();
                _spawnTimer = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CloseAllDialogs: {ex.Message}");
            }
        }
        
        /// <summary>
        /// リソースのクリーンアップ
        /// </summary>
        public void Cleanup()
        {
            CloseAllDialogs();
        }
    }
}