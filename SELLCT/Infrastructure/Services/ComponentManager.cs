using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SELLCT.Core.Entities;
using SELLCT.Core.Interfaces;
using SELLCT.Core.Events;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// 構成要素管理サービス
    /// </summary>
    public class ComponentManager : IDisposable
    {
        private readonly Dictionary<string, GameComponent> _components;
        private readonly Dictionary<string, string> _componentPaths; // 構成要素名 -> 実際のファイルパス
        private readonly string _componentsPath;
        private volatile bool _disposed = false;
        private readonly IEventDispatcher _eventDispatcher;
        private volatile bool _betrayalEndingTriggered = false;
        private readonly FileSystemWatcherManager _watcherManager;

        /// <summary>
        /// 構成要素変更イベント
        /// </summary>
        public event EventHandler<GameComponent> ComponentChanged;

        /// <summary>
        /// componentsフォルダ変更イベント（ウィンドウ最前面表示用）
        /// </summary>
        public event EventHandler<ComponentChangeEventArgs> ComponentsFolderChanged;

        /// <summary>
        /// SELLCTフォルダ削除/空状態検出イベント（裏切りエンディング用）
        /// </summary>
        public event EventHandler SELLCTFolderBetrayed;

        /// <summary>
        /// 管理中の構成要素
        /// </summary>
        public IReadOnlyDictionary<string, GameComponent> Components => _components;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ComponentManager(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _componentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "components");
            _components = new Dictionary<string, GameComponent>();
            _componentPaths = new Dictionary<string, string>();
            
            System.Diagnostics.Debug.WriteLine($"ComponentManager initialized with path: {_componentsPath}");

            // 起動時にcomponentsフォルダを初期状態にリセット
            ResetToInitialStateOnStartup();
            
            // FileSystemWatcherManagerを初期化
            _watcherManager = new FileSystemWatcherManager(_componentsPath, _eventDispatcher);
            _watcherManager.ComponentsFolderChanged += (s, e) => ComponentsFolderChanged?.Invoke(this, e);
            _watcherManager.SELLCTFolderBetrayed += (s, e) => {
                if (!_betrayalEndingTriggered) {
                    _betrayalEndingTriggered = true;
                    SELLCTFolderBetrayed?.Invoke(this, EventArgs.Empty);
                }
            };
            
            // イベント購読でコンポーネント管理を同期
            _eventDispatcher.Subscribe<ComponentCreatedEvent>(OnComponentCreated);
            _eventDispatcher.Subscribe<ComponentDeletedEvent>(OnComponentDeleted);
            _eventDispatcher.Subscribe<ComponentRenamedEvent>(OnComponentRenamed);
        }

        /// <summary>
        /// componentsフォルダ作成
        /// </summary>
        private void CreateComponentsFolder()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Creating components folder at: {_componentsPath}");
                Directory.CreateDirectory(_componentsPath);
                //Directory.CreateDirectory(Path.Combine(_componentsPath, "UI"));
                //Directory.CreateDirectory(Path.Combine(_componentsPath, "Text"));
                //Directory.CreateDirectory(Path.Combine(_componentsPath, "Visual"));
                //Directory.CreateDirectory(Path.Combine(_componentsPath, "SELLCT"));
                
                // Systemフォルダを作成し、隠しフォルダに設定
                //var systemFolderPath = Path.Combine(_componentsPath, "System");
                //Directory.CreateDirectory(systemFolderPath);
                
                // Systemフォルダを隠しフォルダに設定
                //var systemFolderInfo = new DirectoryInfo(systemFolderPath);
                //systemFolderInfo.Attributes |= FileAttributes.Hidden;

                System.Diagnostics.Debug.WriteLine("Components folder structure created (System folder hidden, SELLCT folder added)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating components folder: {ex.Message}");
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
                CreateComponentFile("Button.txt", "");
                //CreateComponentFile("UI/GameWindow.txt", "");
                CreateComponentFile("YES.txt", "");
                CreateComponentFile("Background.txt", "");

                // 隠しファイル（.hidden拡張子）
                //CreateHiddenFile("System/Mouse.txt", "");
                //CreateHiddenFile("System/Keyboard.txt", "");
                //CreateHiddenFile("System/Explorer.txt", "");
                CreateHiddenFile("GameWindow.txt", "");

                // 基本システムファイル
                //CreateHiddenFile("SELLCT/AI.dll.txt", "");
                //CreateHiddenFile("SELLCT/CrashHandler.exe.txt", "");
                //CreateHiddenFile("SELLCT/Updater.ini.txt", "");
                //CreateHiddenFile("SELLCT/Uninstaller.dat.txt", "");
                //CreateHiddenFile("SELLCT/Input.dll.txt", "");
                //CreateHiddenFile("SELLCT/Kernel.dll", "");
                
                //// 追加のシステムコンポーネント
                //CreateHiddenFile("SELLCT/Core.dll.txt", "");
                //CreateHiddenFile("SELLCT/Engine.exe.txt", "");
                //CreateHiddenFile("SELLCT/Renderer.dll.txt", "");
                //CreateHiddenFile("SELLCT/Audio.dll.txt", "");
                //CreateHiddenFile("SELLCT/Network.dll.txt", "");
                //CreateHiddenFile("SELLCT/Security.dll.txt", "");
                //CreateHiddenFile("SELLCT/Database.dll.txt", "");
                //CreateHiddenFile("SELLCT/Logger.dll.txt", "");
                //CreateHiddenFile("SELLCT/Config.ini.txt", "");
                //CreateHiddenFile("SELLCT/Settings.cfg.txt", "");
                
                //// UI関連コンポーネント
                //CreateHiddenFile("SELLCT/UI.dll.txt", "");
                //CreateHiddenFile("SELLCT/Graphics.dll.txt", "");
                //CreateHiddenFile("SELLCT/Window.dll.txt", "");
                //CreateHiddenFile("SELLCT/Dialog.dll.txt", "");
                //CreateHiddenFile("SELLCT/Menu.dll.txt", "");
                //CreateHiddenFile("SELLCT/Font.dll.txt", "");
                
                //// ネットワーク・通信関連
                //CreateHiddenFile("SELLCT/HttpClient.dll.txt", "");
                //CreateHiddenFile("SELLCT/WebSocket.dll.txt", "");
                //CreateHiddenFile("SELLCT/Protocol.dll.txt", "");
                //CreateHiddenFile("SELLCT/Encryption.dll.txt", "");
                //CreateHiddenFile("SELLCT/Certificate.pem.txt", "");
                
                //// データ・ファイル管理
                //CreateHiddenFile("SELLCT/FileManager.dll.txt", "");
                //CreateHiddenFile("SELLCT/DataAccess.dll.txt", "");
                //CreateHiddenFile("SELLCT/Serialization.dll.txt", "");
                //CreateHiddenFile("SELLCT/Compression.dll.txt", "");
                //CreateHiddenFile("SELLCT/Cache.dll.txt", "");
                
                //// ゲーム・エンジン関連
                //CreateHiddenFile("SELLCT/Physics.dll.txt", "");
                //CreateHiddenFile("SELLCT/Animation.dll.txt", "");
                //CreateHiddenFile("SELLCT/Scripting.dll.txt", "");
                //CreateHiddenFile("SELLCT/Resources.dll.txt", "");
                //CreateHiddenFile("SELLCT/Assets.dll.txt", "");
                
                //// システム監視・デバッグ
                //CreateHiddenFile("SELLCT/Monitor.exe.txt", "");
                //CreateHiddenFile("SELLCT/Debugger.dll.txt", "");
                //CreateHiddenFile("SELLCT/Profiler.dll.txt", "");
                //CreateHiddenFile("SELLCT/Telemetry.dll.txt", "");
                //CreateHiddenFile("SELLCT/Analytics.dll.txt", "");
                
                //// プラグイン・拡張
                //CreateHiddenFile("SELLCT/PluginManager.dll.txt", "");
                //CreateHiddenFile("SELLCT/ExtensionHost.dll.txt", "");
                //CreateHiddenFile("SELLCT/ModLoader.dll.txt", "");
                //CreateHiddenFile("SELLCT/ScriptEngine.dll.txt", "");
                
                //// 設定・リソースファイル
                //CreateHiddenFile("SELLCT/Manifest.xml.txt", "");
                //CreateHiddenFile("SELLCT/Resources.resx.txt", "");
                //CreateHiddenFile("SELLCT/Localization.json.txt", "");
                //CreateHiddenFile("SELLCT/Version.txt", "");
                //CreateHiddenFile("SELLCT/License.txt", "");
                //CreateHiddenFile("SELLCT/Readme.md.txt", "");

                System.Diagnostics.Debug.WriteLine("Initial components created");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating initial components: {ex.Message}");
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

            // 既存ファイルがあれば削除してから作成
            if (File.Exists(fullPath))
            {
                try
                {
                    File.SetAttributes(fullPath, FileAttributes.Normal);
                    File.Delete(fullPath);
                    System.Diagnostics.Debug.WriteLine($"Deleted existing component file: {fullPath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete existing component file {fullPath}: {ex.Message}");
                }
            }

            File.WriteAllText(fullPath, content);
            
            // 初期構成要素のパスを記録
            var componentName = Path.GetFileNameWithoutExtension(relativePath);
            _componentPaths[componentName] = fullPath;
            
            System.Diagnostics.Debug.WriteLine($"Created component file: {fullPath}");
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

            // 既存ファイルがあれば削除してから作成
            if (File.Exists(fullPath))
            {
                try
                {
                    File.SetAttributes(fullPath, FileAttributes.Normal);
                    File.Delete(fullPath);
                    System.Diagnostics.Debug.WriteLine($"Deleted existing file: {fullPath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete existing file {fullPath}: {ex.Message}");
                }
            }

            File.WriteAllText(fullPath, content);
            
            // 隠しファイルのパスを記録
            var componentName = Path.GetFileNameWithoutExtension(relativePath);
            _componentPaths[componentName] = fullPath;
            
            System.Diagnostics.Debug.WriteLine($"Created hidden file: {fullPath}");

            // 隠しファイル属性設定
            try
            {
                File.SetAttributes(fullPath, File.GetAttributes(fullPath) | FileAttributes.Hidden);
                System.Diagnostics.Debug.WriteLine($"Set hidden attribute for: {fullPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set hidden attribute for {fullPath}: {ex.Message}");
            }
        }


        /// <summary>
        /// 既存構成要素の読み込み
        /// </summary>
        private void LoadExistingComponents()
        {
            try
            {
                var componentFiles = Directory.GetFiles(_componentsPath, "*.txt", SearchOption.AllDirectories);

                foreach (var filePath in componentFiles)
                {
                    var component = ParseComponentFile(filePath);
                    if (component != null)
                    {
                        _components[component.Name] = component;
                        _componentPaths[component.Name] = filePath; // 既存構成要素のパスを記録
                        System.Diagnostics.Debug.WriteLine($"Loaded existing component: {component.Name} at {filePath}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Loaded {_components.Count} existing components");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading existing components: {ex.Message}");
            }
        }

        /// <summary>
        /// コンポーネント作成イベントハンドラー
        /// </summary>
        private void OnComponentCreated(ComponentCreatedEvent @event)
        {
            if (_disposed) return;

            try
            {
                var component = @event.Component;
                _components[component.Name] = component;
                _componentPaths[component.Name] = component.FilePath;
                
                // 特定構成要素作成時の特別処理
                HandleSpecialComponentCreation(component);
                
                System.Diagnostics.Debug.WriteLine($"Component registered: {component.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnComponentCreated: {ex.Message}");
            }
        }

        /// <summary>
        /// コンポーネント削除イベントハンドラー
        /// </summary>
        private void OnComponentDeleted(ComponentDeletedEvent @event)
        {
            if (_disposed) return;

            try
            {
                var component = @event.Component;
                _components.Remove(component.Name);
                _componentPaths.Remove(component.Name);
                
                // 特定構成要素削除時の処理
                HandleSpecialComponentDeletion(component);
                
                System.Diagnostics.Debug.WriteLine($"Component unregistered: {component.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnComponentDeleted: {ex.Message}");
            }
        }


        /// <summary>
        /// コンポーネントリネームイベントハンドラー
        /// </summary>
        private void OnComponentRenamed(ComponentRenamedEvent @event)
        {
            if (_disposed) return;

            try
            {
                var oldName = @event.OldName;
                var newName = @event.NewName;
                var component = @event.Component;
                
                // 古いコンポーネントとパスを削除
                _components.Remove(oldName);
                _componentPaths.Remove(oldName);
                
                // 新しいコンポーネントとパスを追加
                _components[newName] = component;
                _componentPaths[newName] = component.FilePath;
                
                // 特別なリネーム処理
                HandleSpecialComponentRename(oldName, newName, component);
                
                // リネームによって特定のコンポーネントが「作成」されたと見なす
                HandleSpecialComponentCreation(component);
                
                System.Diagnostics.Debug.WriteLine($"Component renamed: {oldName} -> {newName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnComponentRenamed: {ex.Message}");
            }
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

                var component = new GameComponent
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath,
                    LastModified = File.GetLastWriteTime(filePath),
                    Content = "", // Content will no longer be read from file
                    IsVisible = !File.GetAttributes(filePath).HasFlag(FileAttributes.Hidden) // Determine visibility based on file attributes
                };

                // Determine ComponentType from directory name
                var directoryName = new DirectoryInfo(Path.GetDirectoryName(filePath)).Name;
                if (Enum.TryParse<ComponentType>(directoryName, true, out var type))
                {
                    component.Type = type;
                }
                else
                {
                    // Default type if directory name doesn't match enum
                    component.Type = ComponentType.UI; // Or some other default
                }

                System.Diagnostics.Debug.WriteLine($"[ComponentManager] Parsed component: Name={component.Name}, Type={component.Type}");
                return component;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ComponentManager] Error parsing component file {filePath}: {ex.Message}");
                return null;
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

                // Only update LastModified, as content is no longer parsed for properties
                component.LastModified = File.GetLastWriteTime(filePath);

                System.Diagnostics.Debug.WriteLine($"Updated component: {component.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating component from file {filePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// 特別な構成要素作成処理
        /// </summary>
        private void HandleSpecialComponentCreation(GameComponent component)
        {
            // 謎解きロジックは PuzzleService で処理
        }

        /// <summary>
        /// 特別な構成要素削除処理
        /// </summary>
        private void HandleSpecialComponentDeletion(GameComponent component)
        {
            // 謎解きロジックは PuzzleService で処理
        }

        /// <summary>
        /// 特別なリネーム処理
        /// </summary>
        private void HandleSpecialComponentRename(string oldName, string newName, GameComponent component)
        {
            // 謎解きロジックは PuzzleService で処理
        }

        /// <summary>
        /// 隠しアイテム表示
        /// </summary>
        public void RevealHiddenItem(string folder, string itemName, string displayName)
        {
            try
            {
                var hiddenPath = Path.Combine(_componentsPath, folder, itemName + ".txt");
                var componentPath = Path.Combine(_componentsPath, folder, itemName + ".txt");

                if (File.Exists(hiddenPath) && !File.Exists(componentPath))
                {
                    // 隠しファイルをコンポーネントファイルに変換
                    var content = File.ReadAllText(hiddenPath);
                    File.WriteAllText(componentPath, content);

                    // 隠しファイル属性設定解除
                    try
                    {
                        File.SetAttributes(componentPath, FileAttributes.Normal);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to remove hidden attribute from {componentPath}: {ex.Message}");
                    }

                    // 隠しファイル削除
                    try
                    {
                        File.SetAttributes(hiddenPath, FileAttributes.Normal);
                        File.Delete(hiddenPath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete hidden file {hiddenPath}: {ex.Message}");
                    }

                    System.Diagnostics.Debug.WriteLine($"Hidden item revealed: {displayName}");
                    _eventDispatcher.Dispatch(new HiddenItemRevealedEvent(itemName, displayName, folder));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error revealing hidden item {itemName}: {ex.Message}");
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
        /// ファイルパスで構成要素の存在をチェック
        /// </summary>
        public bool ComponentExistsByPath(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_componentsPath, filePath);
                return File.Exists(fullPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking component existence by path: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 特定タイプの構成要素を取得
        /// </summary>
        public IEnumerable<GameComponent> GetComponentsByType(ComponentType type)
        {
            return _components.Values.Where(c => c.Type == type);
        }

        /// <summary>
        /// 構成要素を削除
        /// </summary>
        public void DeleteComponent(string componentName)
        {
            if (_components.TryGetValue(componentName, out var component))
            {
                try
                {
                    // ファイルシステムからファイルを削除
                    File.Delete(component.FilePath);
                    System.Diagnostics.Debug.WriteLine($"Component file deleted: {component.FilePath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting component file {component.FilePath}: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Component not found: {componentName}");
            }
        }

        /// <summary>
        /// 可視状態の構成要素を取得
        /// </summary>
        public IEnumerable<GameComponent> GetVisibleComponents()
        {
            return _components.Values.Where(c => c.IsVisible);
        }

        /// <summary>
        /// Yesコンポーネントの存在確認
        /// </summary>
        public bool HasYesComponent()
        {
            var yesPatterns = new[] { "YES", "yes", "Yes", "はい" };
            return yesPatterns.Any(pattern => _components.ContainsKey(pattern));
        }

        /// <summary>
        /// Noコンポーネントの存在確認
        /// </summary>
        public bool HasNoComponent()
        {
            var noPatterns = new[] { "NO", "no", "No", "いいえ" };
            return noPatterns.Any(pattern => _components.ContainsKey(pattern));
        }

        /// <summary>
        /// TextWindowコンポーネントの存在確認
        /// </summary>
        public bool HasTextWindowComponent()
        {
            var textWindowPatterns = new[] { "TextWindow", "textwindow", "TEXTWINDOW", "Textwindow" };
            return textWindowPatterns.Any(pattern => _components.ContainsKey(pattern));
        }

        /// <summary>
        /// 起動時の初期状態リセット（イベント発行なし）
        /// </summary>
        private void ResetToInitialStateOnStartup()
        {
            try
            {
                // 裏切りエンディングフラグをリセット
                _betrayalEndingTriggered = false;
                
                // Clear current components
                _components.Clear();

                // Delete and recreate the components directory
                if (Directory.Exists(_componentsPath))
                {
                    Directory.Delete(_componentsPath, true);
                    System.Diagnostics.Debug.WriteLine("Deleted existing components folder");
                }
                
                CreateComponentsFolder();
                CreateInitialComponents();
                LoadExistingComponents();

                System.Diagnostics.Debug.WriteLine("Components folder reset to initial state on startup");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting components folder on startup: {ex.Message}");
            }
        }

        /// <summary>
        /// ゲームの状態を初期構成にリセット
        /// </summary>
        public void ResetToInitialState()
        {
            try
            {
                // 裷切りエンディングフラグをリセット
                _betrayalEndingTriggered = false;
                
                // 監視を一時停止
                _watcherManager?.PauseWatching();

                // 現在のコンポーネントをクリア
                _components.Clear();
                _componentPaths.Clear();

                // componentsディレクトリを削除して再作成
                if (Directory.Exists(_componentsPath))
                {
                    Directory.Delete(_componentsPath, true);
                }
                CreateComponentsFolder();

                // 初期コンポーネントを再作成
                CreateInitialComponents();

                // 既存コンポーネントを再読み込み
                LoadExistingComponents();

                // 監視を再開
                _watcherManager?.ResumeWatching();

                // UI更新用イベントを発行
                foreach (var component in _components.Values)
                {
                    _eventDispatcher.Dispatch(new ComponentCreatedEvent(component));
                }

                System.Diagnostics.Debug.WriteLine("Game reset to initial state.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting game state: {ex.Message}");
            }
        }

        /// <summary>
        /// SELLCTフォルダが空かどうかをチェック（裏切りエンディング用）
        /// </summary>
        private bool CheckIfSELLCTFolderEmpty()
        {
            try
            {
                var sellctFolderPath = Path.Combine(_componentsPath, "SELLCT");
                
                // フォルダが存在しない場合は空と見なす
                if (!Directory.Exists(sellctFolderPath))
                {
                    return true;
                }
                
                // フォルダ内のファイル数をチェック
                var files = Directory.GetFiles(sellctFolderPath, "*", SearchOption.AllDirectories);
                var isEmpty = files.Length == 0;
                
                System.Diagnostics.Debug.WriteLine($"SELLCT folder check: {files.Length} files found, isEmpty: {isEmpty}");
                return isEmpty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking SELLCT folder: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// フェーズ2移行条件をチェック（SELLCTフォルダ内の必要な権限コンポーネントがすべて存在するか）
        /// </summary>
        public bool CheckPhase2TransitionCondition()
        {
            try
            {
                var sellctFolderPath = Path.Combine(_componentsPath, "SELLCT");
                if (!Directory.Exists(sellctFolderPath))
                {
                    System.Diagnostics.Debug.WriteLine("SELLCT folder does not exist");
                    return false;
                }

                // 必要な権限コンポーネント
                var requiredComponents = new[]
                {
                    "AdminRights.txt",
                    "FileAccess.txt", 
                    "NetworkAccess.txt",
                    "SystemControl.txt"
                };

                foreach (var component in requiredComponents)
                {
                    var componentPath = Path.Combine(sellctFolderPath, component);
                    if (!File.Exists(componentPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"Required component missing: {component}");
                        return false;
                    }
                }

                System.Diagnostics.Debug.WriteLine("All required components for Phase 2 transition exist");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking Phase 2 transition condition: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ディレクトリ削除時の子要素削除処理
        /// </summary>
        private void ProcessDirectoryDeletion(string deletedDirectoryPath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ComponentManager] Processing directory deletion: {deletedDirectoryPath}");

                // 削除されたディレクトリ内の構成要素を検索
                var componentsToDelete = new List<string>();
                
                foreach (var kvp in _componentPaths)
                {
                    var componentName = kvp.Key;
                    var componentPath = kvp.Value;
                    
                    // 実際のファイルパスが削除されたディレクトリ内にあるかチェック
                    if (componentPath.StartsWith(deletedDirectoryPath, StringComparison.OrdinalIgnoreCase))
                    {
                        componentsToDelete.Add(componentName);
                        System.Diagnostics.Debug.WriteLine($"[ComponentManager] Found component in deleted directory: {componentName} at {componentPath}");
                    }
                }

                // 子要素の削除処理を実行
                foreach (var componentName in componentsToDelete)
                {
                    if (_components.TryGetValue(componentName, out var component))
                    {
                        System.Diagnostics.Debug.WriteLine($"[ComponentManager] Simulating deletion of child component: {componentName}");
                        
                        _components.Remove(componentName);
                        _componentPaths.Remove(componentName); // パスも削除
                        _eventDispatcher.Dispatch(new ComponentDeletedEvent(component));
                        
                        // 特定構成要素削除時の処理
                        HandleSpecialComponentDeletion(component);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ComponentManager] Error in ProcessDirectoryDeletion: {ex.Message}");
            }
        }


        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _watcherManager?.Dispose();

                _components.Clear();
                _componentPaths.Clear();
                System.Diagnostics.Debug.WriteLine("ComponentManager disposed");
            }
        }
    }

    
}