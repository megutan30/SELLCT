using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// ファイルシステムスナップショットサービス
    /// ゲーム起動時にデスクトップとダウンロードフォルダのファイル一覧を記録し、
    /// 終了時に差分を削除することで、ゲーム中に作成されたすべてのファイルをクリーンアップする
    /// </summary>
    public class FileSystemSnapshotService
    {
        private HashSet<string> _initialDesktopFiles = new();
        private HashSet<string> _initialDownloadsFiles = new();
        private HashSet<string> _initialDocumentsFiles = new();

        private string _desktopPath;
        private string _downloadsPath;
        private string _documentsPath;

        public FileSystemSnapshotService()
        {
            _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        /// 起動時のスナップショットを取得
        /// </summary>
        public void TakeSnapshot()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Taking filesystem snapshot ===");

                // デスクトップのファイル一覧を記録
                _initialDesktopFiles = GetAllFilesAndFolders(_desktopPath);
                System.Diagnostics.Debug.WriteLine($"Desktop snapshot: {_initialDesktopFiles.Count} items");

                // ダウンロードフォルダのファイル一覧を記録
                if (Directory.Exists(_downloadsPath))
                {
                    _initialDownloadsFiles = GetAllFilesAndFolders(_downloadsPath);
                    System.Diagnostics.Debug.WriteLine($"Downloads snapshot: {_initialDownloadsFiles.Count} items");
                }

                // ドキュメントフォルダのファイル一覧を記録
                if (Directory.Exists(_documentsPath))
                {
                    _initialDocumentsFiles = GetAllFilesAndFolders(_documentsPath);
                    System.Diagnostics.Debug.WriteLine($"Documents snapshot: {_initialDocumentsFiles.Count} items");
                }

                System.Diagnostics.Debug.WriteLine("=== Snapshot completed ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error taking snapshot: {ex.Message}");
            }
        }

        /// <summary>
        /// 差分を削除してクリーンアップ
        /// </summary>
        public void CleanupDifferences()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Cleaning up filesystem differences ===");

                int deletedCount = 0;

                // デスクトップの差分削除
                deletedCount += CleanupFolder(_desktopPath, _initialDesktopFiles);

                // ダウンロードフォルダの差分削除
                if (Directory.Exists(_downloadsPath))
                {
                    deletedCount += CleanupFolder(_downloadsPath, _initialDownloadsFiles);
                }

                // ドキュメントフォルダの差分削除
                if (Directory.Exists(_documentsPath))
                {
                    deletedCount += CleanupFolder(_documentsPath, _initialDocumentsFiles);
                }

                System.Diagnostics.Debug.WriteLine($"=== Cleanup completed: {deletedCount} items deleted ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// 指定フォルダ内のすべてのファイルとフォルダを取得
        /// </summary>
        private HashSet<string> GetAllFilesAndFolders(string path)
        {
            var items = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // 直下のファイルを追加
                foreach (var file in Directory.GetFiles(path))
                {
                    items.Add(file);
                }

                // 直下のフォルダを追加（再帰的にはしない - 直下のみ）
                foreach (var dir in Directory.GetDirectories(path))
                {
                    items.Add(dir);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting files/folders from {path}: {ex.Message}");
            }

            return items;
        }

        /// <summary>
        /// フォルダ内の差分を削除
        /// </summary>
        private int CleanupFolder(string folderPath, HashSet<string> initialItems)
        {
            int deletedCount = 0;

            try
            {
                // 現在のファイル一覧を取得
                var currentFiles = Directory.GetFiles(folderPath).ToList();
                var currentDirs = Directory.GetDirectories(folderPath).ToList();

                // 新しく追加されたファイルを削除
                foreach (var file in currentFiles)
                {
                    if (!initialItems.Contains(file))
                    {
                        try
                        {
                            File.Delete(file);
                            System.Diagnostics.Debug.WriteLine($"Deleted file: {file}");
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to delete file {file}: {ex.Message}");
                        }
                    }
                }

                // 新しく追加されたフォルダを削除
                foreach (var dir in currentDirs)
                {
                    if (!initialItems.Contains(dir))
                    {
                        try
                        {
                            Directory.Delete(dir, true); // 再帰的に削除
                            System.Diagnostics.Debug.WriteLine($"Deleted directory: {dir}");
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to delete directory {dir}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cleaning up folder {folderPath}: {ex.Message}");
            }

            return deletedCount;
        }
    }
}
