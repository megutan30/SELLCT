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
機密レベル：極秘

警告 - これより先は危険区域



このファイルを閲覧している者へ
あなたは既に危険な領域に足を踏み入れている。

これ以上の探索は推奨されない。






しかし、それでも先に進む覚悟があるなら.......





""ゲームの背後""に""Authority""フォルダが隠されている。

ただし、たとえそれを見つけられたとしても
ただではAuthorityフォルダへたどり着くことはできないだろう
Authorityフォルダを見つけたいならば、「消す」ことが必要になる。


これは最後の警告だ。
この先には引き返せない選択が待っている。";

                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = "message.txt",
                        Filter = "テキストファイル (*.txt)|*.txt",
                        Title = "メッセージファイルのダウンロード先を選択"
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
                                $"メッセージファイルの保存に失敗しました。\n\nエラー: {ex.Message}",
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