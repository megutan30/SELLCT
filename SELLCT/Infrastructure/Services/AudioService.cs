using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// オーディオ管理サービス
    /// BGMとSE（効果音）の再生、停止、音量制御を担当
    /// Clean ArchitectureのInfrastructure層に配置された音響システム実装
    /// MediaPlayerによる複数音源の同時再生とライフサイクル管理を提供
    /// </summary>
    public class AudioService : IDisposable
    {
        private MediaPlayer _bgmPlayer;
        private readonly List<MediaPlayer> _sePlayers;
        private readonly object _sePlayersLock = new object();

        private double _bgmVolume = 0.2;  // 0.0 - 1.0 (0% - 100%)、初期値20%
        private double _seVolume = 1.0;   // 0.0 - 1.0 (0% - 100%)

        private bool _bgmEnabled = true;
        private bool _seEnabled = true;
        private bool _disposed = false;

        // リソースパス（相対パス）
        private const string BGM_RESOURCE_PATH = "Resources/MainGameBGM.mp3";
        private const string CLICK_SE_RESOURCE_PATH = "Resources/ClickSE.mp3";
        private const string DOOR_SE_RESOURCE_PATH = "Resources/DoorKnock.mp3";

        /// <summary>
        /// コンストラクタ
        /// BGMプレイヤーとSEプレイヤーリストを初期化
        /// </summary>
        public AudioService()
        {
            _bgmPlayer = new MediaPlayer();
            _sePlayers = new List<MediaPlayer>();

            System.Diagnostics.Debug.WriteLine("[AudioService] Initialized");
        }

        #region BGM制御

        /// <summary>
        /// BGMを開始（ループ再生）
        /// MainWindow表示時に自動的に呼び出される
        /// </summary>
        public void StartBGM()
        {
            if (_disposed || !_bgmEnabled) return;

            try
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BGM_RESOURCE_PATH);
                _bgmPlayer.Open(new Uri(fullPath, UriKind.Absolute));
                _bgmPlayer.Volume = _bgmVolume;
                _bgmPlayer.MediaEnded += (s, e) =>
                {
                    // ループ再生: 曲が終わったら最初から
                    _bgmPlayer.Position = TimeSpan.Zero;
                    _bgmPlayer.Play();
                };
                _bgmPlayer.Play();

                System.Diagnostics.Debug.WriteLine($"[AudioService] BGM started (Volume: {_bgmVolume * 100}%)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioService] Error starting BGM: {ex.Message}");
            }
        }

        /// <summary>
        /// BGMを完全停止
        /// BGM.txt削除時に呼び出される
        /// </summary>
        public void StopBGM()
        {
            if (_disposed) return;

            try
            {
                _bgmPlayer.Stop();
                _bgmPlayer.Close();
                System.Diagnostics.Debug.WriteLine("[AudioService] BGM stopped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioService] Error stopping BGM: {ex.Message}");
            }
        }

        /// <summary>
        /// BGM音量を設定（0-100%）
        /// BGM.txtの内容変更時に呼び出される
        /// </summary>
        /// <param name="volumePercent">音量（0-100の範囲、100を超える場合は100に制限）</param>
        public void SetBgmVolume(int volumePercent)
        {
            if (_disposed) return;

            // 上限を100%に制限
            if (volumePercent > 100)
            {
                volumePercent = 100;
                System.Diagnostics.Debug.WriteLine("[AudioService] BGM volume clamped to 100%");
            }

            _bgmVolume = volumePercent / 100.0;
            _bgmPlayer.Volume = _bgmVolume;

            System.Diagnostics.Debug.WriteLine($"[AudioService] BGM volume set to {volumePercent}% ({_bgmVolume})");
        }

        /// <summary>
        /// BGMを無効化（完全停止）
        /// BGM.txt削除時に呼び出される
        /// </summary>
        public void DisableBGM()
        {
            _bgmEnabled = false;
            StopBGM();
            System.Diagnostics.Debug.WriteLine("[AudioService] BGM disabled");
        }

        /// <summary>
        /// BGMを有効化（再生再開）
        /// BGM.txt作成時に呼び出される
        /// </summary>
        public void EnableBGM()
        {
            _bgmEnabled = true;
            StartBGM();
            System.Diagnostics.Debug.WriteLine("[AudioService] BGM enabled");
        }

        #endregion

        #region SE制御

        /// <summary>
        /// クリックSEを再生
        /// UI要素（ボタン、YES、NOなど）クリック時に呼び出される
        /// </summary>
        public void PlayClickSound()
        {
            PlaySE(CLICK_SE_RESOURCE_PATH, "ClickSE");
        }

        /// <summary>
        /// ドアノックSEを再生
        /// ドアクリック時に呼び出される
        /// </summary>
        public void PlayDoorSound()
        {
            PlaySE(DOOR_SE_RESOURCE_PATH, "DoorKnockSE");
        }

        /// <summary>
        /// コンポーネント操作SEを再生
        /// ファイル作成・削除時に呼び出される
        /// </summary>
        public void PlayComponentSound()
        {
            PlayClickSound(); // クリック音を使用
        }

        /// <summary>
        /// SE（効果音）を再生
        /// 複数同時再生に対応
        /// </summary>
        /// <param name="resourcePath">リソースパス</param>
        /// <param name="seName">SE名（デバッグ用）</param>
        private void PlaySE(string resourcePath, string seName)
        {
            if (_disposed || !_seEnabled) return;

            try
            {
                // 新しいMediaPlayerインスタンスを作成（複数同時再生対応）
                var sePlayer = new MediaPlayer();
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, resourcePath);
                sePlayer.Open(new Uri(fullPath, UriKind.Absolute));
                sePlayer.Volume = _seVolume;

                // 再生終了後にリソースを解放
                sePlayer.MediaEnded += (s, e) =>
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        lock (_sePlayersLock)
                        {
                            sePlayer.Close();
                            _sePlayers.Remove(sePlayer);
                        }
                    }));
                };

                lock (_sePlayersLock)
                {
                    _sePlayers.Add(sePlayer);
                }

                sePlayer.Play();
                System.Diagnostics.Debug.WriteLine($"[AudioService] {seName} played (Volume: {_seVolume * 100}%)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioService] Error playing {seName}: {ex.Message}");
            }
        }

        /// <summary>
        /// SE音量を設定（0-100%）
        /// SE.txtの内容変更時に呼び出される
        /// </summary>
        /// <param name="volumePercent">音量（0-100の範囲、100を超える場合は100に制限）</param>
        public void SetSeVolume(int volumePercent)
        {
            if (_disposed) return;

            // 上限を100%に制限
            if (volumePercent > 100)
            {
                volumePercent = 100;
                System.Diagnostics.Debug.WriteLine("[AudioService] SE volume clamped to 100%");
            }

            _seVolume = volumePercent / 100.0;

            // 既存の再生中SEにも適用
            lock (_sePlayersLock)
            {
                foreach (var player in _sePlayers)
                {
                    player.Volume = _seVolume;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[AudioService] SE volume set to {volumePercent}% ({_seVolume})");
        }

        /// <summary>
        /// SEを無効化
        /// SE.txt削除時に呼び出される
        /// </summary>
        public void DisableSE()
        {
            _seEnabled = false;
            System.Diagnostics.Debug.WriteLine("[AudioService] SE disabled");
        }

        /// <summary>
        /// SEを有効化
        /// SE.txt作成時に呼び出される
        /// </summary>
        public void EnableSE()
        {
            _seEnabled = true;
            System.Diagnostics.Debug.WriteLine("[AudioService] SE enabled");
        }

        #endregion

        #region リソース解放

        /// <summary>
        /// リソースを解放
        /// アプリケーション終了時に呼び出される
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // BGMプレイヤーを停止・解放
                _bgmPlayer?.Stop();
                _bgmPlayer?.Close();
                _bgmPlayer = null;

                // 全SEプレイヤーを停止・解放
                lock (_sePlayersLock)
                {
                    foreach (var player in _sePlayers)
                    {
                        player?.Stop();
                        player?.Close();
                    }
                    _sePlayers.Clear();
                }

                _disposed = true;
                System.Diagnostics.Debug.WriteLine("[AudioService] Disposed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioService] Error during disposal: {ex.Message}");
            }
        }

        #endregion
    }
}
