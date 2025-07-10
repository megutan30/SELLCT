using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SELLCT.Models;

namespace SELLCT.Services
{
    /// <summary>
    /// 構成要素管理サービス
    /// </summary>
    public class ComponentManager : IDisposable
    {
        private readonly Dictionary<string, GameComponent> _components;
        private FileSystemWatcher _watcher;
        private readonly string _componentsPath;
        private readonly Timer _debounceTimer;
        private volatile bool _disposed = false;

        /// <summary>
        /// 構成要素作成イベント
        /// </summary>
        public event EventHandler<GameComponent> ComponentCreated;

        /// <summary>
        /// 構成要素削除イベント
        /// </summary>
        public event EventHandler<GameComponent> ComponentDeleted;

        /// <summary>
        /// 構成要素変更イベント
        /// </summary>
        public event EventHandler<GameComponent> ComponentChanged;

        /// <summary>
        /// 構成要素リネームイベント
        /// </summary>
        public event EventHandler<ComponentRenamedEventArgs> ComponentRenamed;

        /// <summary>
        /// 隠しアイテム出現イベント
        /// </summary>
        public event EventHandler<HiddenItemRevealedEventArgs> HiddenItemRevealed;

        /// <summary>
        /// 管理中の構成要素
        /// </summary>
        public IReadOnlyDictionary<string, GameComponent> Components => _components;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ComponentManager()
        {
            _componentsPath = "components";
            _components = new Dictionary<string, GameComponent>();
            _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);

            CreateComponentsFolder();
            CreateInitialComponents();
            SetupFileWatcher();
            LoadExistingComponents();
        }

        /// <summary>
        /// componentsフォルダ作成
        /// </summary>
        private void CreateComponentsFolder()
        {
            try
            {
                Directory.CreateDirectory(_componentsPath);
                Directory.CreateDirectory(Path.Combine(_componentsPath, "UI"));
                Directory.CreateDirectory(Path.Combine(_componentsPath, "Text"));
                Directory.CreateDirectory(Path.Combine(_componentsPath, "Visual"));
                Directory.CreateDirectory(Path.Combine(_componentsPath, "System"));

                Console.WriteLine("Components folder structure created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating components folder: {ex.Message}");
            }
        }

        /// <summary>
        /// 初期構成要素の作成
        /// </summary>
        private void CreateInitialComponents()
        {
            try
            {
                // 表示される初期構成要素
                CreateComponentFile("UI/Button.component", "UI|name=メインボタン|visible=true|function=main");
                CreateComponentFile("UI/GameWindow.component", "UI|name=ゲーム画面|visible=true|type=window");
                CreateComponentFile("Text/Hiragana.component", "Text|name=日本語テキスト|visible=true|charset=hiragana");
                CreateComponentFile("Text/YES.component", "UI|name=YES選択肢|visible=true|type=choice|value=はい");
                CreateComponentFile("Visual/Background.component", "Visual|name=背景|visible=true|type=image");

                // 隠しファイル（.hidden拡張子）
                CreateHiddenFile("UI/KEY.hidden", "UI|name=隠された鍵|visible=false|description=リトライボタンの後ろに隠されていた謎の鍵");
                CreateHiddenFile("Text/SecretMessage.hidden", "Text|name=隠されたメッセージ|visible=false|description=真実を告げる秘密のメッセージ");
                CreateHiddenFile("System/Mouse.hidden", "System|name=マウス制御権限|visible=false|permission=mouse_control");
                CreateHiddenFile("System/Keyboard.hidden", "System|name=キーボード制御権限|visible=false|permission=keyboard_control");
                CreateHiddenFile("System/Explorer.hidden", "System|name=エクスプローラー制御権限|visible=false|permission=explorer_control");
                CreateHiddenFile("System/WindowsShell.hidden", "System|name=Windowsシェル制御権限|visible=false|permission=shell_control");

                Console.WriteLine("Initial components created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating initial components: {ex.Message}");
            }
        }

        /// <summary>
        /// 構成要素ファイル作成
        /// </summary>
        private void CreateComponentFile(string relativePath, string content)
        {
            var fullPath = Path.Combine(_componentsPath, relativePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, content);
        }

        /// <summary>
        /// 隠しファイル作成
        /// </summary>
        private void CreateHiddenFile(string relativePath, string content)
        {
            var fullPath = Path.Combine(_componentsPath, relativePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, content);

            // 隠しファイル属性設定
            try
            {
                File.SetAttributes(fullPath, FileAttributes.Hidden);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set hidden attribute: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイル監視設定
        /// </summary>
        private void SetupFileWatcher()
        {
            try
            {
                _watcher = new FileSystemWatcher(_componentsPath)
                {
                    IncludeSubdirectories = true,
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _watcher.Created += OnFileCreated;
                _watcher.Deleted += OnFileDeleted;
                _watcher.Changed += OnFileChanged;
                _watcher.Renamed += OnFileRenamed;
                _watcher.Error += OnWatcherError;

                _watcher.EnableRaisingEvents = true;

                Console.WriteLine("File watcher initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up file watcher: {ex.Message}");
            }
        }

        /// <summary>
        /// 既存構成要素の読み込み
        /// </summary>
        private void LoadExistingComponents()
        {
            try
            {
                var componentFiles = Directory.GetFiles(_componentsPath, "*.component", SearchOption.AllDirectories);

                foreach (var filePath in componentFiles)
                {
                    var component = ParseComponentFile(filePath);
                    if (component != null)
                    {
                        _components[component.Name] = component;
                        Console.WriteLine($"Loaded existing component: {component.Name}");
                    }
                }

                Console.WriteLine($"Loaded {_components.Count} existing components");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading existing components: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイル作成イベントハンドラー
        /// </summary>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (_disposed) return;

            try
            {
                Console.WriteLine($"File created: {e.FullPath}");

                if (e.Name.EndsWith(".component"))
                {
                    var component = ParseComponentFile(e.FullPath);
                    if (component != null)
                    {
                        _components[component.Name] = component;
                        ComponentCreated?.Invoke(this, component);

                        // 特定構成要素作成時の特別処理
                        HandleSpecialComponentCreation(component);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnFileCreated: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイル削除イベントハンドラー
        /// </summary>
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (_disposed) return;

            try
            {
                Console.WriteLine($"File deleted: {e.FullPath}");

                if (e.Name.EndsWith(".component"))
                {
                    var componentName = Path.GetFileNameWithoutExtension(e.Name);
                    if (_components.TryGetValue(componentName, out var component))
                    {
                        _components.Remove(componentName);
                        ComponentDeleted?.Invoke(this, component);

                        // 特定構成要素削除時の処理
                        HandleSpecialComponentDeletion(component);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnFileDeleted: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイル変更イベントハンドラー
        /// </summary>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_disposed) return;

            try
            {
                // デバウンス処理
                _debounceTimer.Change(500, Timeout.Infinite);

                if (e.Name.EndsWith(".component"))
                {
                    var componentName = Path.GetFileNameWithoutExtension(e.Name);
                    if (_components.TryGetValue(componentName, out var component))
                    {
                        UpdateComponentFromFile(component, e.FullPath);
                        ComponentChanged?.Invoke(this, component);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnFileChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイル名変更イベントハンドラー
        /// </summary>
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (_disposed) return;

            try
            {
                Console.WriteLine($"File renamed: {e.OldFullPath} -> {e.FullPath}");

                if (e.OldName.EndsWith(".component") && e.Name.EndsWith(".component"))
                {
                    var oldName = Path.GetFileNameWithoutExtension(e.OldName);
                    var newName = Path.GetFileNameWithoutExtension(e.Name);

                    if (_components.TryGetValue(oldName, out var component))
                    {
                        _components.Remove(oldName);
                        component.Name = newName;
                        component.FilePath = e.FullPath;
                        _components[newName] = component;

                        ComponentRenamed?.Invoke(this, new ComponentRenamedEventArgs(oldName, newName, component));

                        // 特別なリネーム処理
                        HandleSpecialComponentRename(oldName, newName, component);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnFileRenamed: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイル監視エラーハンドラー
        /// </summary>
        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"File watcher error: {e.GetException()?.Message}");
        }

        /// <summary>
        /// デバウンスタイマーイベント
        /// </summary>
        private void OnDebounceElapsed(object state)
        {
            // デバウンス処理完了
        }

        /// <summary>
        /// ファイルから構成要素を解析
        /// </summary>
        private GameComponent ParseComponentFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var content = File.ReadAllText(filePath);
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0)
                    return null;

                var parts = lines[0].Split('|');
                if (parts.Length == 0)
                    return null;

                var component = new GameComponent
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath,
                    LastModified = File.GetLastWriteTime(filePath),
                    Content = content,
                    IsVisible = true
                };

                // タイプ解析
                if (Enum.TryParse<ComponentType>(parts[0], true, out var type))
                {
                    component.Type = type;
                }

                // プロパティ解析
                for (int i = 1; i < parts.Length; i++)
                {
                    ParseProperty(component, parts[i]);
                }

                return component;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing component file {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// プロパティ解析
        /// </summary>
        private void ParseProperty(GameComponent component, string propertyString)
        {
            var equalIndex = propertyString.IndexOf('=');
            if (equalIndex == -1) return;

            var key = propertyString.Substring(0, equalIndex).Trim();
            var value = propertyString.Substring(equalIndex + 1).Trim();

            switch (key.ToLower())
            {
                case "name":
                    // 名前は既にファイル名から設定済み
                    break;
                case "visible":
                    component.IsVisible = bool.TryParse(value, out var visible) && visible;
                    break;
                default:
                    component.SetProperty(key, value);
                    break;
            }
        }

        /// <summary>
        /// ファイルから構成要素を更新
        /// </summary>
        private void UpdateComponentFromFile(GameComponent component, string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;

                var content = File.ReadAllText(filePath);
                component.Content = content;
                component.LastModified = File.GetLastWriteTime(filePath);

                // 内容の再解析
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    var parts = lines[0].Split('|');
                    for (int i = 1; i < parts.Length; i++)
                    {
                        ParseProperty(component, parts[i]);
                    }
                }

                Console.WriteLine($"Updated component: {component.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating component from file {filePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// 特別な構成要素作成処理
        /// </summary>
        private void HandleSpecialComponentCreation(GameComponent component)
        {
            switch (component.Name.ToLower())
            {
                case "TextWindow":
                    Console.WriteLine("TextWindow component created - enabling dialog");
                    break;
                case "no":
                    Console.WriteLine("NO component created - enabling NO choice");
                    break;
                case "upload":
                    Console.WriteLine("Upload component created - granting file access permission");
                    break;
            }
        }

        /// <summary>
        /// 特別な構成要素削除処理
        /// </summary>
        private void HandleSpecialComponentDeletion(GameComponent component)
        {
            switch (component.Name.ToLower())
            {
                case "button":
                    Console.WriteLine("Button component deleted - revealing KEY");
                    RevealHiddenItem("UI", "KEY", "隠れた鍵");
                    break;
                case "gamewindow":
                    Console.WriteLine("GameWindow component deleted - transitioning to Phase 2");
                    // フェーズ2移行は MainWindow で処理
                    break;
                case "hiragana":
                    Console.WriteLine("Hiragana component deleted - revealing secret message");
                    RevealHiddenItem("Text", "SecretMessage", "隠されたメッセージ");
                    break;
            }
        }

        /// <summary>
        /// 特別なリネーム処理
        /// </summary>
        private void HandleSpecialComponentRename(string oldName, string newName, GameComponent component)
        {
            if (oldName.ToLower() == "button" && newName.ToLower() == "upload")
            {
                Console.WriteLine("Button renamed to Upload - granting file upload permission");
                component.SetProperty("permission", "file_upload");
                component.SetProperty("function", "upload");
            }
        }

        /// <summary>
        /// 隠しアイテム表示
        /// </summary>
        private void RevealHiddenItem(string folder, string itemName, string displayName)
        {
            try
            {
                var hiddenPath = Path.Combine(_componentsPath, folder, itemName + ".hidden");
                var componentPath = Path.Combine(_componentsPath, folder, itemName + ".component");

                if (File.Exists(hiddenPath) && !File.Exists(componentPath))
                {
                    // 隠しファイルをコンポーネントファイルに変換
                    var content = File.ReadAllText(hiddenPath);
                    File.WriteAllText(componentPath, content);

                    // 隠しファイル削除
                    try
                    {
                        File.SetAttributes(hiddenPath, FileAttributes.Normal);
                        File.Delete(hiddenPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete hidden file {hiddenPath}: {ex.Message}");
                    }

                    Console.WriteLine($"Hidden item revealed: {displayName}");
                    HiddenItemRevealed?.Invoke(this, new HiddenItemRevealedEventArgs(itemName, displayName, folder));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error revealing hidden item {itemName}: {ex.Message}");
            }
        }

        /// <summary>
        /// 構成要素取得
        /// </summary>
        public GameComponent GetComponent(string name)
        {
            return _components.TryGetValue(name, out var component) ? component : null;
        }

        /// <summary>
        /// 特定タイプの構成要素を取得
        /// </summary>
        public IEnumerable<GameComponent> GetComponentsByType(ComponentType type)
        {
            return _components.Values.Where(c => c.Type == type);
        }

        /// <summary>
        /// 可視状態の構成要素を取得
        /// </summary>
        public IEnumerable<GameComponent> GetVisibleComponents()
        {
            return _components.Values.Where(c => c.IsVisible);
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _debounceTimer?.Dispose();

                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Created -= OnFileCreated;
                    _watcher.Deleted -= OnFileDeleted;
                    _watcher.Changed -= OnFileChanged;
                    _watcher.Renamed -= OnFileRenamed;
                    _watcher.Error -= OnWatcherError;
                    _watcher.Dispose();
                }

                _components.Clear();
                Console.WriteLine("ComponentManager disposed");
            }
        }
    }

    /// <summary>
    /// 構成要素リネームイベント引数
    /// </summary>
    public class ComponentRenamedEventArgs : EventArgs
    {
        public string OldName { get; }
        public string NewName { get; }
        public GameComponent Component { get; }

        public ComponentRenamedEventArgs(string oldName, string newName, GameComponent component)
        {
            OldName = oldName;
            NewName = newName;
            Component = component;
        }
    }

    /// <summary>
    /// 隠しアイテム出現イベント引数
    /// </summary>
    public class HiddenItemRevealedEventArgs : EventArgs
    {
        public string ItemName { get; }
        public string DisplayName { get; }
        public string Folder { get; }

        public HiddenItemRevealedEventArgs(string itemName, string displayName, string folder)
        {
            ItemName = itemName;
            DisplayName = displayName;
            Folder = folder;
        }
    }
}