using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;
using SELLCT.Core.Events;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// コンポーネント変更イベント引数クラス
    /// ファイルシステム変更通知のためのイベントデータを格納
    /// ファイル作成、削除、名前変更等の詳細情報を提供
    /// </summary>
    public class ComponentChangeEventArgs : EventArgs
    {
        /// <summary>変更の種類（作成、削除、名前変更等）</summary>
        public WatcherChangeTypes ChangeType { get; set; }
        
        /// <summary>変更されたファイルのパス</summary>
        public string FilePath { get; set; }
        
        /// <summary>名前変更時の旧ファイルパス</summary>
        public string OldPath { get; set; }
        
        /// <summary>
        /// コンストラクタ
        /// ファイルシステム変更イベントの詳細情報を設定
        /// </summary>
        /// <param name="changeType">変更の種類</param>
        /// <param name="filePath">変更されたファイルパス</param>
        /// <param name="oldPath">名前変更時の旧パス（省略可能）</param>
        public ComponentChangeEventArgs(WatcherChangeTypes changeType, string filePath, string oldPath = null)
        {
            ChangeType = changeType;
            FilePath = filePath;
            OldPath = oldPath;
        }
    }
    
    /// <summary>
    /// ファイルシステム監視管理クラス
    /// FileSystemWatcherのラッパーとして高度なファイル監視機能を提供
    /// Clean ArchitectureのInfrastructure層に配置されたファイルシステム抽象化
    /// デバウンス処理、イベント重複排除、スレッドセーフな操作を実現
    /// ゲームコンポーネントのファイルシステム変更を確実に検出・通知
    /// </summary>
    public class FileSystemWatcherManager : IDisposable
    {
        private readonly string _watchPath;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Timer _debounceTimer;
        private readonly Dictionary<string, DateTime> _pendingEvents;
        private readonly object _pendingEventsLock = new object();
        
        private FileSystemWatcher _watcher;
        private volatile bool _disposed = false;

        private const int DEBOUNCE_INTERVAL_MS = 100;
        private const int MAX_PENDING_EVENTS = 1000;

        public event EventHandler<ComponentChangeEventArgs> ComponentsFolderChanged;
        public event EventHandler SELLCTFolderBetrayed;

        public FileSystemWatcherManager(string watchPath, IEventDispatcher eventDispatcher)
        {
            _watchPath = watchPath;
            _eventDispatcher = eventDispatcher;
            _pendingEvents = new Dictionary<string, DateTime>();
            _debounceTimer = new Timer(ProcessPendingEvents, null, Timeout.Infinite, Timeout.Infinite);
            
            InitializeWatcher();
        }

        private void InitializeWatcher()
        {
            try
            {
                _watcher = new FileSystemWatcher(_watchPath)
                {
                    IncludeSubdirectories = true,
                    Filter = "*",
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
                    InternalBufferSize = 65536
                };

                _watcher.Created += OnFileSystemEvent;
                _watcher.Deleted += OnFileSystemEvent;
                _watcher.Changed += OnFileSystemEvent;
                _watcher.Renamed += OnFileRenamed;
                _watcher.Error += OnWatcherError;

                _watcher.EnableRaisingEvents = true;

                System.Diagnostics.Debug.WriteLine("FileSystemWatcherManager initialized");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing FileSystemWatcher: {ex.Message}");
            }
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            if (_disposed) return;

            try
            {
                CheckSELLCTFolderStatus();
                
                // 変更種別に応じたイベント引数を作成
                var changeEventArgs = new ComponentChangeEventArgs(e.ChangeType, e.FullPath);
                ComponentsFolderChanged?.Invoke(this, changeEventArgs);
                
                ScheduleDelayedEvent(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnFileSystemEvent: {ex.Message}");
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (_disposed) return;

            try
            {
                var deleteEvent = new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(e.OldFullPath), e.OldName);
                ScheduleDelayedEvent(deleteEvent);

                var createEvent = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(e.FullPath), e.Name);
                ScheduleDelayedEvent(createEvent);

                // 名前変更イベントの詳細情報を含む引数を作成
                var changeEventArgs = new ComponentChangeEventArgs(WatcherChangeTypes.Renamed, e.FullPath, e.OldFullPath);
                ComponentsFolderChanged?.Invoke(this, changeEventArgs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnFileRenamed: {ex.Message}");
            }
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"FileSystemWatcher Error: {e.GetException().Message}");
            System.Threading.Tasks.Task.Run(RestartWatcher);
        }

        private void RestartWatcher()
        {
            if (_disposed) return;

            try
            {
                _watcher?.Dispose();
                Thread.Sleep(1000);
                InitializeWatcher();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restarting FileSystemWatcher: {ex.Message}");
            }
        }

        private void ScheduleDelayedEvent(FileSystemEventArgs e)
        {
            if (!IsRelevantFile(e.FullPath)) return;

            lock (_pendingEventsLock)
            {
                if (_pendingEvents.Count >= MAX_PENDING_EVENTS)
                {
                    _pendingEvents.Clear();
                }

                var eventKey = $"{e.ChangeType}:{e.FullPath}";
                _pendingEvents[eventKey] = DateTime.UtcNow;
                
                _debounceTimer.Change(DEBOUNCE_INTERVAL_MS, Timeout.Infinite);
            }
        }

        private bool IsRelevantFile(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(filePath);
                
                if (!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    return false;

                if (fileName.StartsWith("~") || fileName.StartsWith(".tmp") || fileName.Contains("$"))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ProcessPendingEvents(object state)
        {
            if (_disposed) return;

            Dictionary<string, DateTime> eventsToProcess;

            lock (_pendingEventsLock)
            {
                if (_pendingEvents.Count == 0) return;

                eventsToProcess = new Dictionary<string, DateTime>(_pendingEvents);
                _pendingEvents.Clear();
            }

            foreach (var kvp in eventsToProcess)
            {
                ProcessDelayedEvent(kvp.Key);
            }
        }

        private void ProcessDelayedEvent(string eventKey)
        {
            try
            {
                var parts = eventKey.Split(':', 2);
                if (parts.Length != 2) return;

                var changeType = Enum.Parse<WatcherChangeTypes>(parts[0]);
                var filePath = parts[1];

                switch (changeType)
                {
                    case WatcherChangeTypes.Created:
                        ProcessFileCreated(filePath);
                        break;
                    case WatcherChangeTypes.Deleted:
                        ProcessFileDeleted(filePath);
                        break;
                    case WatcherChangeTypes.Changed:
                        ProcessFileChanged(filePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing delayed event {eventKey}: {ex.Message}");
            }
        }

        private void ProcessFileCreated(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var component = ParseComponentFile(filePath);
            if (component != null)
            {
                _eventDispatcher.Dispatch(new ComponentCreatedEvent(component));
            }
        }

        private void ProcessFileDeleted(string filePath)
        {
            var componentName = Path.GetFileNameWithoutExtension(filePath);
            if (!string.IsNullOrEmpty(componentName))
            {
                var component = GameComponent.Create(componentName, ComponentType.UI, "");
                component.FilePath = filePath;
                _eventDispatcher.Dispatch(new ComponentDeletedEvent(component));
            }
        }

        private void ProcessFileChanged(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var component = ParseComponentFile(filePath);
            if (component != null)
            {
                _eventDispatcher.Dispatch(new ComponentContentChangedEvent(component));
            }
        }

        private GameComponent ParseComponentFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;

                var componentName = Path.GetFileNameWithoutExtension(filePath);
                var content = File.ReadAllText(filePath);
                var isVisible = !IsHiddenFile(filePath);
                var componentType = DetermineComponentType(filePath);

                var component = GameComponent.Create(componentName, componentType, content);
                component.FilePath = filePath;
                component.IsVisible = isVisible;
                
                return component;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing component file {filePath}: {ex.Message}");
                return null;
            }
        }

        private bool IsHiddenFile(string filePath)
        {
            try
            {
                var attributes = File.GetAttributes(filePath);
                return (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            }
            catch
            {
                return false;
            }
        }

        private ComponentType DetermineComponentType(string filePath)
        {
            var relativePath = Path.GetRelativePath(_watchPath, filePath);
            var directory = Path.GetDirectoryName(relativePath);
            
            return directory?.ToLowerInvariant() switch
            {
                "ui" => ComponentType.UI,
                "text" => ComponentType.Text,
                "visual" => ComponentType.Visual,
                "system" => ComponentType.System,
                _ => ComponentType.UI
            };
        }

        private void CheckSELLCTFolderStatus()
        {
            try
            {
                var sellctFolderPath = Path.Combine(_watchPath, "SELLCT");
                
                if (!Directory.Exists(sellctFolderPath))
                {
                    SELLCTFolderBetrayed?.Invoke(this, EventArgs.Empty);
                    return;
                }

                var files = Directory.GetFiles(sellctFolderPath);
                if (files.Length == 0)
                {
                    SELLCTFolderBetrayed?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking SELLCT folder status: {ex.Message}");
            }
        }

        public void PauseWatching()
        {
            if (_watcher != null && !_disposed)
            {
                _watcher.EnableRaisingEvents = false;
            }
        }

        public void ResumeWatching()
        {
            if (_watcher != null && !_disposed)
            {
                _watcher.EnableRaisingEvents = true;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _debounceTimer?.Dispose();
                _watcher?.Dispose();

                lock (_pendingEventsLock)
                {
                    _pendingEvents.Clear();
                }
            }
        }
    }
}