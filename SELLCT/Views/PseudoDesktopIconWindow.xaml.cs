using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SELLCT.Views
{
    /// <summary>
    /// 疑似デスクトップアイコンウィンドウ
    /// 本物のデスクトップアイコンのように見える独立したウィンドウ
    /// </summary>
    public partial class PseudoDesktopIconWindow : Window, IDisposable
    {
        private bool _disposed = false;
        private string _associatedFolderPath;
        
        public PseudoDesktopIconWindow()
        {
            InitializeComponent();
            
            // ウィンドウの初期設定
            InitializeWindow();
            
            System.Diagnostics.Debug.WriteLine("PseudoDesktopIconWindow initialized");
        }

        /// <summary>
        /// ウィンドウの初期設定
        /// </summary>
        private void InitializeWindow()
        {
            // ウィンドウ位置を画面外に設定（初期状態では非表示）
            this.Left = -1000;
            this.Top = -1000;
            
            // ウィンドウイベントのハンドリング
            this.Loaded += PseudoDesktopIconWindow_Loaded;
            this.Closed += PseudoDesktopIconWindow_Closed;
        }

        /// <summary>
        /// アイコン画像とサイズを設定
        /// </summary>
        /// <param name="iconImage">アイコンのBitmapSource</param>
        /// <param name="iconSize">アイコンのサイズ</param>
        public void SetIcon(BitmapSource iconImage, Size iconSize)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Setting icon: {iconSize.Width}x{iconSize.Height}");
                
                if (iconImage != null)
                {
                    IconImage.Source = iconImage;
                    System.Diagnostics.Debug.WriteLine("✅ Icon image set successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Icon image is null, keeping default");
                }

                // アイコンサイズを適用
                IconImage.Width = iconSize.Width;
                IconImage.Height = iconSize.Height;
                
                System.Diagnostics.Debug.WriteLine($"Icon size applied: {IconImage.Width}x{IconImage.Height}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error setting icon: {ex.Message}");
            }
        }

        /// <summary>
        /// アイコンの名前（ラベル）を設定
        /// </summary>
        /// <param name="iconName">表示する名前</param>
        public void SetName(string iconName)
        {
            try
            {
                if (!string.IsNullOrEmpty(iconName))
                {
                    IconLabel.Text = iconName;
                    System.Diagnostics.Debug.WriteLine($"Icon name set to: '{iconName}'");
                }
                else
                {
                    IconLabel.Text = "Authority"; // デフォルト名
                    System.Diagnostics.Debug.WriteLine("Using default icon name: 'Authority'");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error setting icon name: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウの位置を設定
        /// </summary>
        /// <param name="position">新しい位置</param>
        public void SetPosition(Point position)
        {
            try
            {
                this.Left = position.X;
                this.Top = position.Y;
                
                System.Diagnostics.Debug.WriteLine($"Icon position set to: ({position.X:F0}, {position.Y:F0})");
                
                // ウィンドウを表示状態にする
                if (!this.IsVisible)
                {
                    this.Show();
                    System.Diagnostics.Debug.WriteLine("Icon window shown");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error setting icon position: {ex.Message}");
            }
        }

        /// <summary>
        /// 関連付けられたフォルダパスを設定
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        public void SetAssociatedFolder(string folderPath)
        {
            _associatedFolderPath = folderPath;
            System.Diagnostics.Debug.WriteLine($"Associated folder set: {folderPath}");
        }

        /// <summary>
        /// ウィンドウの表示/非表示を切り替え
        /// </summary>
        /// <param name="visible">表示するかどうか</param>
        public void SetVisible(bool visible)
        {
            try
            {
                if (visible)
                {
                    this.Show();
                    System.Diagnostics.Debug.WriteLine("PseudoDesktopIcon shown");
                }
                else
                {
                    this.Hide();
                    System.Diagnostics.Debug.WriteLine("PseudoDesktopIcon hidden");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error setting visibility: {ex.Message}");
            }
        }

        /// <summary>
        /// アイコンクリックイベントハンドラー
        /// </summary>
        private void IconImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Icon clicked: Button={e.ChangedButton}, ClickCount={e.ClickCount}");
                
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (e.ClickCount == 2) // ダブルクリック
                    {
                        System.Diagnostics.Debug.WriteLine("Double-click detected - opening folder");
                        OpenAssociatedFolder();
                    }
                    else if (e.ClickCount == 1) // シングルクリック
                    {
                        System.Diagnostics.Debug.WriteLine("Single-click detected - selecting icon");
                        // シングルクリックの処理（必要に応じて実装）
                        SelectIcon();
                    }
                }
                else if (e.ChangedButton == MouseButton.Right) // 右クリック
                {
                    System.Diagnostics.Debug.WriteLine("Right-click detected");
                    // 右クリックメニューの処理（必要に応じて実装）
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error handling icon click: {ex.Message}");
            }
        }

        /// <summary>
        /// アイコンの選択状態を表示
        /// </summary>
        private void SelectIcon()
        {
            try
            {
                // 選択エフェクトを追加（簡単な実装）
                // 実際のデスクトップアイコンのような選択表示も可能
                System.Diagnostics.Debug.WriteLine("Icon selected");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error selecting icon: {ex.Message}");
            }
        }

        /// <summary>
        /// 関連付けられたフォルダを開く
        /// </summary>
        private void OpenAssociatedFolder()
        {
            try
            {
                if (!string.IsNullOrEmpty(_associatedFolderPath) && Directory.Exists(_associatedFolderPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Opening folder: {_associatedFolderPath}");
                    
                    // エクスプローラーでフォルダを開く
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = _associatedFolderPath,
                        UseShellExecute = true
                    });
                    
                    System.Diagnostics.Debug.WriteLine("✅ Folder opened successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Cannot open folder: path='{_associatedFolderPath}', exists={Directory.Exists(_associatedFolderPath ?? "")}");
                    
                    // フォルダが存在しない場合は作成する
                    if (!string.IsNullOrEmpty(_associatedFolderPath))
                    {
                        Directory.CreateDirectory(_associatedFolderPath);
                        System.Diagnostics.Debug.WriteLine($"✅ Created folder: {_associatedFolderPath}");
                        OpenAssociatedFolder(); // 再帰的に呼び出し
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error opening associated folder: {ex.Message}");
                
                // フォールバック: デスクトップを開く
                try
                {
                    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = desktopPath,
                        UseShellExecute = true
                    });
                    System.Diagnostics.Debug.WriteLine("Opened desktop as fallback");
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Fallback also failed: {fallbackEx.Message}");
                }
            }
        }

        /// <summary>
        /// ウィンドウロードイベント
        /// </summary>
        private void PseudoDesktopIconWindow_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PseudoDesktopIconWindow loaded");
        }

        /// <summary>
        /// ウィンドウクローズイベント
        /// </summary>
        private void PseudoDesktopIconWindow_Closed(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PseudoDesktopIconWindow closed");
            Dispose();
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
                    // イベントハンドラーの解除
                    this.Loaded -= PseudoDesktopIconWindow_Loaded;
                    this.Closed -= PseudoDesktopIconWindow_Closed;
                    
                    System.Diagnostics.Debug.WriteLine("PseudoDesktopIconWindow disposed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during disposal: {ex.Message}");
                }
            }
        }
    }
}