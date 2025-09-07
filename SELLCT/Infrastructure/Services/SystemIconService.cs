using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// システムアイコンとサイズを取得するサービス
    /// フォルダアイコンの実際の画像と大きさを取得してWPFで使用可能な形式に変換
    /// </summary>
    public class SystemIconService
    {
        // Win32 API定義
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        // 構造体
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        // 定数
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        // システムメトリクス定数
        private const int SM_CXICON = 11;  // 大アイコンの幅
        private const int SM_CYICON = 12;  // 大アイコンの高さ
        private const int SM_CXSMICON = 49; // 小アイコンの幅
        private const int SM_CYSMICON = 50; // 小アイコンの高さ

        /// <summary>
        /// フォルダアイコンとそのサイズを取得
        /// </summary>
        /// <param name="useLargeIcon">大アイコンを使用するかどうか</param>
        /// <returns>アイコンのBitmapSourceとサイズのタプル</returns>
        public (BitmapSource iconImage, System.Windows.Size iconSize) GetFolderIconAndSize(bool useLargeIcon = true)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== GetFolderIconAndSize Debug ===");
                System.Diagnostics.Debug.WriteLine($"Requesting {(useLargeIcon ? "Large" : "Small")} folder icon");

                SHFILEINFO shinfo = new SHFILEINFO();
                uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | (useLargeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);

                // フォルダアイコンを取得（仮想的なフォルダパス）
                IntPtr result = SHGetFileInfo("folder", FILE_ATTRIBUTE_DIRECTORY, ref shinfo, 
                    (uint)Marshal.SizeOf(shinfo), flags);

                if (result == IntPtr.Zero || shinfo.hIcon == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to get folder icon from Shell API");
                    return GetFallbackFolderIcon(useLargeIcon);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Successfully retrieved folder icon handle: {shinfo.hIcon}");

                // アイコンハンドルからBitmapSourceに変換
                BitmapSource iconBitmap = ConvertIconToBitmapSource(shinfo.hIcon);

                // アイコンハンドルを破棄
                DestroyIcon(shinfo.hIcon);

                // システムアイコンサイズを取得
                var iconSize = GetSystemIconSize(useLargeIcon);

                System.Diagnostics.Debug.WriteLine($"Icon size: {iconSize.Width} x {iconSize.Height}");
                System.Diagnostics.Debug.WriteLine($"Bitmap size: {iconBitmap?.PixelWidth} x {iconBitmap?.PixelHeight}");
                System.Diagnostics.Debug.WriteLine($"=== End GetFolderIconAndSize Debug ===\n");

                return (iconBitmap, iconSize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error getting folder icon: {ex.Message}");
                return GetFallbackFolderIcon(useLargeIcon);
            }
        }

        /// <summary>
        /// システムアイコンサイズを取得
        /// </summary>
        /// <param name="useLargeIcon">大アイコンのサイズを取得するか</param>
        /// <returns>アイコンサイズ</returns>
        public System.Windows.Size GetSystemIconSize(bool useLargeIcon = true)
        {
            try
            {
                int width, height;

                if (useLargeIcon)
                {
                    width = GetSystemMetrics(SM_CXICON);
                    height = GetSystemMetrics(SM_CYICON);
                }
                else
                {
                    width = GetSystemMetrics(SM_CXSMICON);
                    height = GetSystemMetrics(SM_CYSMICON);
                }

                System.Diagnostics.Debug.WriteLine($"System icon size: {width} x {height} ({(useLargeIcon ? "Large" : "Small")})");

                // 0の場合はデフォルト値を使用
                if (width <= 0 || height <= 0)
                {
                    width = useLargeIcon ? 32 : 16;
                    height = useLargeIcon ? 32 : 16;
                    System.Diagnostics.Debug.WriteLine($"Using fallback icon size: {width} x {height}");
                }

                return new System.Windows.Size(width, height);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting system icon size: {ex.Message}");
                // フォールバック
                return new System.Windows.Size(useLargeIcon ? 32 : 16, useLargeIcon ? 32 : 16);
            }
        }

        /// <summary>
        /// デスクトップアイコン間隔を取得
        /// </summary>
        /// <returns>アイコン間隔（横、縦）</returns>
        public System.Windows.Size GetDesktopIconSpacing()
        {
            try
            {
                // デフォルトのアイコン間隔（通常は75x75程度）
                var defaultSpacing = new System.Windows.Size(75, 75);

                System.Diagnostics.Debug.WriteLine($"Desktop icon spacing: {defaultSpacing.Width} x {defaultSpacing.Height}");

                return defaultSpacing;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting desktop icon spacing: {ex.Message}");
                return new System.Windows.Size(75, 75);
            }
        }

        /// <summary>
        /// アイコンハンドルをBitmapSourceに変換
        /// </summary>
        /// <param name="hIcon">アイコンハンドル</param>
        /// <returns>BitmapSource</returns>
        private BitmapSource ConvertIconToBitmapSource(IntPtr hIcon)
        {
            try
            {
                if (hIcon == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("Icon handle is zero, cannot convert");
                    return null;
                }

                // アイコンハンドルからBitmapSourceを作成
                var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                    hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // フリーズしてパフォーマンスを向上
                bitmapSource.Freeze();

                System.Diagnostics.Debug.WriteLine($"✅ Successfully converted icon to BitmapSource: {bitmapSource.PixelWidth}x{bitmapSource.PixelHeight}");

                return bitmapSource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error converting icon to BitmapSource: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// フォールバックとしてデフォルトのフォルダアイコンを返す
        /// </summary>
        /// <param name="useLargeIcon">大アイコンを使用するか</param>
        /// <returns>フォールバックアイコンとサイズ</returns>
        private (BitmapSource iconImage, System.Windows.Size iconSize) GetFallbackFolderIcon(bool useLargeIcon = true)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Using fallback folder icon approach");

                // システムアイコンサイズを取得
                var iconSize = GetSystemIconSize(useLargeIcon);

                // 単色のフォールバック画像を作成（透明度付きの黄色四角形）
                var width = (int)iconSize.Width;
                var height = (int)iconSize.Height;

                var bitmap = new System.Windows.Media.Imaging.WriteableBitmap(width, height, 96, 96, 
                    System.Windows.Media.PixelFormats.Bgra32, null);

                // 簡単な黄色のフォルダアイコン風の図形を描画（Safe code）
                bitmap.Lock();
                try
                {
                    // safeなコードでピクセルを設定
                    var backBuffer = bitmap.BackBuffer;
                    var stride = bitmap.BackBufferStride;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // フォルダの形状を模した簡単な描画
                            if (IsInFolderShape(x, y, width, height))
                            {
                                IntPtr pixelPtr = backBuffer + y * stride + x * 4;
                                System.Runtime.InteropServices.Marshal.WriteByte(pixelPtr, 0);     // Blue
                                System.Runtime.InteropServices.Marshal.WriteByte(pixelPtr + 1, 200); // Green
                                System.Runtime.InteropServices.Marshal.WriteByte(pixelPtr + 2, 255); // Red (黄色)
                                System.Runtime.InteropServices.Marshal.WriteByte(pixelPtr + 3, 255); // Alpha
                            }
                        }
                    }
                }
                finally
                {
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                    bitmap.Unlock();
                }

                bitmap.Freeze();

                System.Diagnostics.Debug.WriteLine($"✅ Created fallback folder icon: {width}x{height}");

                return (bitmap, iconSize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error creating fallback icon: {ex.Message}");
                return (null, GetSystemIconSize(useLargeIcon));
            }
        }

        /// <summary>
        /// フォルダの形状内かどうかを判定（簡易実装）
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">全体の幅</param>
        /// <param name="height">全体の高さ</param>
        /// <returns>フォルダ形状内の場合true</returns>
        private bool IsInFolderShape(int x, int y, int width, int height)
        {
            // 簡単な矩形フォルダ形状（上部にタブ付き）
            int tabHeight = height / 5;
            int tabWidth = width * 2 / 3;

            // メインボディ
            if (y >= tabHeight && x >= 2 && x < width - 2 && y < height - 2)
                return true;

            // タブ部分
            if (y < tabHeight && x >= 2 && x < tabWidth && y >= 2)
                return true;

            return false;
        }
    }
}