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
                var passwordContent = @"PassWord : SELLCT_Literacy";

                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = "PassWord.txt",
                        Filter = "テキストファイル (*.txt)|*.txt",
                        Title = "パスワードファイルのダウンロード先を選択"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        try
                        {
                            File.WriteAllText(dialog.FileName, passwordContent, Encoding.UTF8);
                            System.Diagnostics.Debug.WriteLine($"PassWord.txt downloaded as {dialog.FileName}");

                            _eventDispatcher.Dispatch(new KeyClickedEvent());
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error saving PassWord.txt: {ex.Message}");
                            MessageBox.Show(
                                $"パスワードファイルの保存に失敗しました。\n\nエラー: {ex.Message}",
                                "SELLCT - エラー",
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