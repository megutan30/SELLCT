using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// 画面キャプチャ機能を提供するサービス
    /// 教育目的でPC画面の縮小演出を実現するため
    /// </summary>
    public static class ScreenCaptureService
    {
        // Win32 API定義
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
                                         IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const uint SRCCOPY = 0x00CC0020;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        /// <summary>
        /// 画面全体のスクリーンショットを取得
        /// </summary>
        /// <returns>WPF用のBitmapSource</returns>
        public static BitmapSource CaptureScreen()
        {
            try
            {
                // より正確な画面サイズを取得
                var screenWidth = GetSystemMetrics(SM_CXSCREEN);
                var screenHeight = GetSystemMetrics(SM_CYSCREEN);

                System.Diagnostics.Debug.WriteLine($"Capturing screen: {screenWidth}x{screenHeight} (using GetSystemMetrics)");

                // デスクトップのデバイスコンテキストを取得
                IntPtr desktopDC = GetDC(IntPtr.Zero);
                
                // 互換性のあるデバイスコンテキストを作成
                IntPtr memoryDC = CreateCompatibleDC(desktopDC);
                
                // 互換性のあるビットマップを作成
                IntPtr bitmap = CreateCompatibleBitmap(desktopDC, screenWidth, screenHeight);
                
                // ビットマップを選択
                IntPtr oldBitmap = SelectObject(memoryDC, bitmap);
                
                // 画面をビットマップにコピー
                bool success = BitBlt(memoryDC, 0, 0, screenWidth, screenHeight,
                                    desktopDC, 0, 0, SRCCOPY);

                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine("BitBlt failed");
                    return null;
                }

                // GDI+ Bitmapを作成
                using (var gdiBitmap = System.Drawing.Image.FromHbitmap(bitmap))
                {
                    // WPF BitmapSourceに変換
                    var bitmapSource = ConvertToBitmapSource(gdiBitmap);
                    
                    System.Diagnostics.Debug.WriteLine("Screen capture completed successfully");
                    
                    // リソースクリーンアップ
                    SelectObject(memoryDC, oldBitmap);
                    DeleteObject(bitmap);
                    DeleteDC(memoryDC);
                    ReleaseDC(IntPtr.Zero, desktopDC);
                    
                    return bitmapSource;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Screen capture failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// System.Drawing.ImageをWPF BitmapSourceに変換
        /// </summary>
        /// <param name="image">変換元の画像</param>
        /// <returns>WPF用BitmapSource</returns>
        private static BitmapSource ConvertToBitmapSource(System.Drawing.Image image)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    // PNG形式でメモリストリームに保存
                    image.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0;
                    
                    // BitmapImageを作成
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // パフォーマンス向上のため
                    
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bitmap conversion failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 指定した領域のスクリーンショットを取得
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>WPF用のBitmapSource</returns>
        public static BitmapSource CaptureRegion(int x, int y, int width, int height)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Capturing region: ({x}, {y}) {width}x{height}");

                IntPtr desktopDC = GetDC(IntPtr.Zero);
                IntPtr memoryDC = CreateCompatibleDC(desktopDC);
                IntPtr bitmap = CreateCompatibleBitmap(desktopDC, width, height);
                IntPtr oldBitmap = SelectObject(memoryDC, bitmap);
                
                bool success = BitBlt(memoryDC, 0, 0, width, height,
                                    desktopDC, x, y, SRCCOPY);

                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine("Region BitBlt failed");
                    return null;
                }

                using (var gdiBitmap = System.Drawing.Image.FromHbitmap(bitmap))
                {
                    var bitmapSource = ConvertToBitmapSource(gdiBitmap);
                    
                    // リソースクリーンアップ
                    SelectObject(memoryDC, oldBitmap);
                    DeleteObject(bitmap);
                    DeleteDC(memoryDC);
                    ReleaseDC(IntPtr.Zero, desktopDC);
                    
                    return bitmapSource;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Region capture failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 画面サイズを取得（正確な値）
        /// </summary>
        /// <returns>画面のサイズ</returns>
        public static System.Windows.Size GetScreenSize()
        {
            return new System.Windows.Size(
                GetSystemMetrics(SM_CXSCREEN),
                GetSystemMetrics(SM_CYSCREEN)
            );
        }
    }
}