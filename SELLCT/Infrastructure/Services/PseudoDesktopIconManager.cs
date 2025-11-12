using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using SELLCT.Views;
using SELLCT.Core.Interfaces;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// 疑似デスクトップアイコンの管理サービス
    /// 複数の疑似アイコンウィンドウの作成、位置制御、表示状態管理を行う
    /// </summary>
    public class PseudoDesktopIconManager : IDisposable
    {
        private readonly Dictionary<string, PseudoDesktopIconWindow> _activeIcons;
        private readonly SystemIconService _iconService;
        private readonly IEventDispatcher _eventDispatcher;
        private bool _disposed = false;

        public PseudoDesktopIconManager(IEventDispatcher eventDispatcher = null)
        {
            _activeIcons = new Dictionary<string, PseudoDesktopIconWindow>();
            _iconService = new SystemIconService();
            _eventDispatcher = eventDispatcher;
            
            System.Diagnostics.Debug.WriteLine("PseudoDesktopIconManager initialized");
        }

        /// <summary>
        /// 疑似デスクトップアイコンを作成
        /// </summary>
        /// <param name="iconName">アイコンの名前</param>
        /// <param name="position">アイコンの位置</param>
        /// <param name="folderPath">関連付けるフォルダパス（オプション）</param>
        /// <param name="visible">初期表示状態（デフォルト: true）</param>
        /// <returns>作成に成功した場合true</returns>
        public bool CreatePseudoIcon(string iconName, Point position, string folderPath = null, bool visible = true)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== CreatePseudoIcon Debug ===");
                System.Diagnostics.Debug.WriteLine($"Icon Name: '{iconName}'");
                System.Diagnostics.Debug.WriteLine($"Position: ({position.X:F2}, {position.Y:F2})");
                System.Diagnostics.Debug.WriteLine($"Folder Path: '{folderPath}'");

                // 既存のアイコンが存在する場合は削除
                if (_activeIcons.ContainsKey(iconName))
                {
                    System.Diagnostics.Debug.WriteLine($"Removing existing icon: {iconName}");
                    RemovePseudoIcon(iconName);
                }

                // システムからフォルダアイコンとサイズを取得
                var (iconImage, iconSize) = _iconService.GetFolderIconAndSize(useLargeIcon: true);
                
                System.Diagnostics.Debug.WriteLine($"Retrieved system icon: {iconSize.Width}x{iconSize.Height}");

                // 疑似アイコンウィンドウを作成（UIスレッドで実行）
                PseudoDesktopIconWindow iconWindow = null;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    iconWindow = new PseudoDesktopIconWindow(_eventDispatcher);

                    // アイコンの設定
                    iconWindow.SetIcon(iconImage, iconSize);
                    iconWindow.SetName(iconName);
                    iconWindow.SetPosition(position);

                    // 表示状態を設定
                    if (visible)
                    {
                        // Z-orderを設定してメインウィンドウの後ろに配置
                        iconWindow.EnsureBehindMainWindow();
                    }
                    else
                    {
                        // 非表示で作成
                        iconWindow.Hide();
                    }
                });
                
                // フォルダパスが指定されている場合は関連付け
                if (!string.IsNullOrEmpty(folderPath))
                {
                    iconWindow.SetAssociatedFolder(folderPath);

                    // 表示状態の場合のみフォルダを作成
                    if (visible && !Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        System.Diagnostics.Debug.WriteLine($"Created folder: {folderPath}");
                    }
                    else if (!visible)
                    {
                        System.Diagnostics.Debug.WriteLine($"Hidden icon - folder not created: {folderPath}");
                    }
                }

                // アクティブアイコン辞書に追加
                _activeIcons[iconName] = iconWindow;

                System.Diagnostics.Debug.WriteLine($"✅ Pseudo icon created successfully: {iconName}");
                System.Diagnostics.Debug.WriteLine($"=== End CreatePseudoIcon Debug ===\n");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error creating pseudo icon '{iconName}': {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 疑似デスクトップアイコンの位置を更新
        /// </summary>
        /// <param name="iconName">アイコンの名前</param>
        /// <param name="newPosition">新しい位置</param>
        /// <returns>更新に成功した場合true</returns>
        public bool UpdateIconPosition(string iconName, Point newPosition)
        {
            try
            {
                if (_activeIcons.TryGetValue(iconName, out var iconWindow))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        iconWindow.SetPosition(newPosition);
                        // 位置更新後もZ-orderを維持
                        iconWindow.EnsureBehindMainWindow();
                    });
                    System.Diagnostics.Debug.WriteLine($"✅ Icon position updated: {iconName} -> ({newPosition.X:F2}, {newPosition.Y:F2})");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Icon not found for position update: {iconName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error updating icon position '{iconName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 疑似デスクトップアイコンの表示/非表示を切り替え
        /// </summary>
        /// <param name="iconName">アイコンの名前</param>
        /// <param name="visible">表示するかどうか</param>
        /// <returns>切り替えに成功した場合true</returns>
        public bool SetIconVisibility(string iconName, bool visible)
        {
            try
            {
                if (_activeIcons.TryGetValue(iconName, out var iconWindow))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        iconWindow.SetVisible(visible);
                        // 表示切り替え後もZ-orderを維持（visible=trueの場合のみ）
                        if (visible)
                        {
                            iconWindow.EnsureBehindMainWindow();
                        }
                    });
                    System.Diagnostics.Debug.WriteLine($"✅ Icon visibility changed: {iconName} -> {(visible ? "Visible" : "Hidden")}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Icon not found for visibility change: {iconName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error changing icon visibility '{iconName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 疑似デスクトップアイコンを表示
        /// </summary>
        /// <param name="iconName">アイコンの名前</param>
        /// <returns>表示に成功した場合true</returns>
        public bool ShowPseudoIcon(string iconName)
        {
            return SetIconVisibility(iconName, true);
        }

        /// <summary>
        /// 疑似デスクトップアイコンを非表示
        /// </summary>
        /// <param name="iconName">アイコンの名前</param>
        /// <returns>非表示に成功した場合true</returns>
        public bool HidePseudoIcon(string iconName)
        {
            return SetIconVisibility(iconName, false);
        }

        /// <summary>
        /// すべての疑似デスクトップアイコンの表示/非表示を切り替え
        /// </summary>
        /// <param name="visible">表示するかどうか</param>
        public void SetAllIconsVisibility(bool visible)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Setting all icons visibility: {(visible ? "Visible" : "Hidden")}");
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var kvp in _activeIcons)
                    {
                        try
                        {
                            kvp.Value.SetVisible(visible);
                            // 表示切り替え後もZ-orderを維持（visible=trueの場合のみ）
                            if (visible)
                            {
                                kvp.Value.EnsureBehindMainWindow();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error setting visibility for icon '{kvp.Key}': {ex.Message}");
                        }
                    }
                });
                
                System.Diagnostics.Debug.WriteLine($"✅ All icons visibility changed to: {(visible ? "Visible" : "Hidden")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error setting all icons visibility: {ex.Message}");
            }
        }

        /// <summary>
        /// 疑似デスクトップアイコンを削除
        /// </summary>
        /// <param name="iconName">アイコンの名前</param>
        /// <returns>削除に成功した場合true</returns>
        public bool RemovePseudoIcon(string iconName)
        {
            try
            {
                if (_activeIcons.TryGetValue(iconName, out var iconWindow))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        iconWindow.Close();
                        iconWindow.Dispose();
                    });
                    _activeIcons.Remove(iconName);
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Pseudo icon removed: {iconName}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Icon not found for removal: {iconName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error removing pseudo icon '{iconName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// すべての疑似デスクトップアイコンを削除
        /// </summary>
        public void RemoveAllPseudoIcons()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Removing all pseudo icons ({_activeIcons.Count} icons)");
                
                var iconsToRemove = new List<string>(_activeIcons.Keys);
                
                foreach (var iconName in iconsToRemove)
                {
                    RemovePseudoIcon(iconName);
                }
                
                System.Diagnostics.Debug.WriteLine("✅ All pseudo icons removed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error removing all pseudo icons: {ex.Message}");
            }
        }

        /// <summary>
        /// 指定したアイコンが存在するかチェック
        /// </summary>
        /// <param name="iconName">アイコンの名前</param>
        /// <returns>存在する場合true</returns>
        public bool HasIcon(string iconName)
        {
            return _activeIcons.ContainsKey(iconName);
        }

        /// <summary>
        /// アクティブなアイコンの数を取得
        /// </summary>
        /// <returns>アイコンの数</returns>
        public int GetActiveIconCount()
        {
            return _activeIcons.Count;
        }

        /// <summary>
        /// アクティブなアイコンの名前一覧を取得
        /// </summary>
        /// <returns>アイコン名のリスト</returns>
        public List<string> GetActiveIconNames()
        {
            return new List<string>(_activeIcons.Keys);
        }

        /// <summary>
        /// ウィンドウ位置に基づいてアイコンの最適な配置位置を計算
        /// </summary>
        /// <param name="windowPosition">メインウィンドウの位置</param>
        /// <param name="windowSize">メインウィンドウのサイズ</param>
        /// <param name="offset">オフセット（オプション）</param>
        /// <returns>計算されたアイコン位置</returns>
        public Point CalculateIconPosition(Point windowPosition, Size windowSize, Point offset = default)
        {
            try
            {
                // デフォルトではウィンドウの中央に配置
                var centerX = windowPosition.X + (windowSize.Width / 2);
                var centerY = windowPosition.Y + (windowSize.Height / 2);
                
                // システムアイコンサイズを取得してオフセットを調整
                var iconSize = _iconService.GetSystemIconSize(useLargeIcon: true);
                centerX -= iconSize.Width / 2;
                centerY -= iconSize.Height / 2;
                
                // 指定されたオフセットを適用
                var finalX = centerX + offset.X;
                var finalY = centerY + offset.Y;
                
                // 画面境界を考慮した位置補正
                var correctedPosition = ClampToScreenBounds(new Point(finalX, finalY), iconSize);
                
                System.Diagnostics.Debug.WriteLine($"Icon position calculated: Window({windowPosition.X:F0},{windowPosition.Y:F0}) {windowSize.Width:F0}x{windowSize.Height:F0} -> Icon({correctedPosition.X:F0},{correctedPosition.Y:F0})");
                
                return correctedPosition;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error calculating icon position: {ex.Message}");
                return new Point(100, 100); // フォールバック位置
            }
        }

        /// <summary>
        /// アイコン位置を画面境界内に補正
        /// </summary>
        /// <param name="position">元の位置</param>
        /// <param name="iconSize">アイコンサイズ</param>
        /// <returns>補正後の位置</returns>
        private Point ClampToScreenBounds(Point position, Size iconSize)
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                
                var clampedX = Math.Max(0, Math.Min(position.X, screenWidth - iconSize.Width));
                var clampedY = Math.Max(0, Math.Min(position.Y, screenHeight - iconSize.Height));
                
                return new Point(clampedX, clampedY);
            }
            catch
            {
                return position; // エラー時は元の位置をそのまま返す
            }
        }

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("Disposing PseudoDesktopIconManager");
                    
                    // すべてのアイコンを削除
                    RemoveAllPseudoIcons();
                    
                    System.Diagnostics.Debug.WriteLine("✅ PseudoDesktopIconManager disposed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error during PseudoDesktopIconManager disposal: {ex.Message}");
                }
            }
        }
    }
}