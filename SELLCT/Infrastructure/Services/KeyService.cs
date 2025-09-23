using System;
using System.IO;
using System.Text;
using System.Windows;

using SELLCT.Core.Interfaces;
using SELLCT.Core.Events;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// 鍵システムサービス
    /// </summary>
    public class KeyService : IDisposable
    {
        private readonly IEventDispatcher _eventDispatcher;
        private bool _disposed = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KeyService(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        /// <summary>
        /// 鍵クリック処理
        /// </summary>
        public bool OnKeyClicked()
        {
            if (_disposed) return false;

            bool success = false;
            try
            {
                var passwordContent = @"[CLASSIFIED DOCUMENT]
Classification Level: TOP SECRET

WARNING - DANGEROUS ZONE AHEAD



To whoever is viewing this file:
You have already stepped into dangerous territory.

Further exploration is not recommended.






However, if you still have the resolve to proceed.......





An ""Authority"" folder is hidden ""behind the game"".

But even if you find it,
you won't be able to reach the Authority folder easily.
If you want to find the Authority folder, you will need to ""delete"" something.


This is the final warning.
Ahead lies a choice from which there is no return.";

                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = "message.txt",
                        Filter = "Text files (*.txt)|*.txt",
                        Title = "Select message file download location"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        try
                        {
                            File.WriteAllText(dialog.FileName, passwordContent, Encoding.UTF8);
                            System.Diagnostics.Debug.WriteLine($"message.txt downloaded as {dialog.FileName}");

                            // ダウンロードパスをDownloadTrackerに記録
                            DownloadTracker.RecordDownload(dialog.FileName, "Message");

                            _eventDispatcher.Dispatch(new KeyClickedEvent());
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error saving message.txt: {ex.Message}");
                            MessageBox.Show(
                                $"Failed to save message file.\n\nError: {ex.Message}",
                                "SELLCT - Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            success = false;
                        }
                    }
                    else
                    {
                        // ユーザーがダイアログをキャンセル
                        success = false;
                    }
                });

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnKeyClicked: {ex.Message}");
                return false;
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
                System.Diagnostics.Debug.WriteLine("KeyService disposed");
            }
        }
    }
}