using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// ゲーム終了時・起動時のクリーンアップサービス
    /// ダウンロードした手紙、componentsフォルダ、Authorityフォルダなどを削除
    /// </summary>
    public class CleanupService
    {
        /// <summary>
        /// 起動時クリーンアップを実行
        /// </summary>
        public static void PerformStartupCleanup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Starting startup cleanup ===");
                
                // componentsフォルダの削除
                CleanupComponentsFolder();
                
                // Authorityフォルダの削除
                CleanupAuthorityFolder();
                
                // ダウンロードした手紙ファイルの削除
                CleanupLetterFiles();
                
                // 一時ファイルの削除
                CleanupTempFiles();
                
                // デスクトップのreadme.txtファイルの削除
                CleanupDesktopReadmeFile();
                
                // レジストリエントリの削除
                CleanupRegistryEntries();
                
                System.Diagnostics.Debug.WriteLine("=== Startup cleanup completed ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during startup cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// 終了時クリーンアップを実行
        /// </summary>
        public static void PerformExitCleanup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Starting exit cleanup ===");
                
                // componentsフォルダの削除
                CleanupComponentsFolder();
                
                // Authorityフォルダの削除
                CleanupAuthorityFolder();
                
                // ダウンロードした手紙ファイルの削除
                CleanupLetterFiles();
                
                // 一時ファイルの削除
                CleanupTempFiles();
                
                // デスクトップのreadme.txtファイルの削除
                CleanupDesktopReadmeFile();
                
                System.Diagnostics.Debug.WriteLine("=== Exit cleanup completed ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during exit cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// 手動クリーンアップを実行（デバッグ用）
        /// </summary>
        public static void PerformManualCleanup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Starting manual cleanup ===");
                
                // componentsフォルダの削除
                CleanupComponentsFolder();
                
                // Authorityフォルダの削除
                CleanupAuthorityFolder();
                
                // ダウンロードした手紙ファイルの削除
                CleanupLetterFiles();
                
                // 一時ファイルの削除
                CleanupTempFiles();
                
                // デスクトップのreadme.txtファイルの削除
                CleanupDesktopReadmeFile();
                
                // レジストリエントリの削除
                CleanupRegistryEntries();
                
                System.Diagnostics.Debug.WriteLine("=== Manual cleanup completed ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during manual cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// componentsフォルダを削除
        /// </summary>
        private static void CleanupComponentsFolder()
        {
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var componentsPath = Path.Combine(desktopPath, "components");
                
                if (Directory.Exists(componentsPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Deleting components folder: {componentsPath}");
                    Directory.Delete(componentsPath, true);
                    System.Diagnostics.Debug.WriteLine("✅ Components folder deleted successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Components folder not found - no cleanup needed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error deleting components folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Authorityフォルダを削除
        /// </summary>
        private static void CleanupAuthorityFolder()
        {
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var authorityPath = Path.Combine(desktopPath, "Authority");
                
                if (Directory.Exists(authorityPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Deleting Authority folder: {authorityPath}");
                    
                    // 隠し属性とシステム属性を解除してから削除
                    var dirInfo = new DirectoryInfo(authorityPath);
                    dirInfo.Attributes = FileAttributes.Normal;
                    
                    // 中のファイルの属性も解除
                    foreach (var file in Directory.GetFiles(authorityPath))
                    {
                        var fileInfo = new FileInfo(file);
                        fileInfo.Attributes = FileAttributes.Normal;
                    }
                    
                    Directory.Delete(authorityPath, true);
                    System.Diagnostics.Debug.WriteLine("✅ Authority folder deleted successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Authority folder not found - no cleanup needed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error deleting Authority folder: {ex.Message}");
            }
        }

        /// <summary>
        /// ダウンロードした手紙・パスワードファイルを削除
        /// </summary>
        private static void CleanupLetterFiles()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Cleaning up downloaded files...");
                
                int trackedDeletedCount = 0;
                int searchDeletedCount = 0;
                
                // 1. DownloadTrackerで記録されたファイルを削除
                try
                {
                    trackedDeletedCount = DownloadTracker.DeleteTrackedFilesAndClearRecords();
                    System.Diagnostics.Debug.WriteLine($"✅ Deleted {trackedDeletedCount} tracked files");
                }
                catch (Exception trackerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error deleting tracked files: {trackerEx.Message}");
                }
                
                // 2. フォールバック：従来の検索方式も併用
                var searchFolders = new List<string>
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads", // 修正: ダウンロード → Downloads
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };
                
                foreach (var folder in searchFolders)
                {
                    if (!Directory.Exists(folder)) continue;
                    
                    try
                    {
                        // letter*.txtパターンとmessage.txtを検索
                        var letterFiles = Directory.GetFiles(folder, "letter*.txt", SearchOption.TopDirectoryOnly);
                        var messageFiles = Directory.GetFiles(folder, "message.txt", SearchOption.TopDirectoryOnly);
                        var allFiles = letterFiles.Concat(messageFiles).ToArray();
                        
                        foreach (var file in allFiles)
                        {
                            try
                            {
                                File.Delete(file);
                                System.Diagnostics.Debug.WriteLine($"✅ Deleted file via search: {file}");
                                searchDeletedCount++;
                            }
                            catch (Exception fileEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ Error deleting file {file}: {fileEx.Message}");
                            }
                        }
                    }
                    catch (Exception folderEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error searching folder {folder}: {folderEx.Message}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Downloaded files cleanup completed - Tracked: {trackedDeletedCount}, Search: {searchDeletedCount} files deleted");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error during downloaded files cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// 一時ファイルを削除
        /// </summary>
        private static void CleanupTempFiles()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Cleaning up temporary files...");
                
                var tempPath = Path.GetTempPath();
                var tempFiles = new[]
                {
                    Path.Combine(tempPath, "sellct_reboot.ps1"),
                    // 他の一時ファイルがある場合はここに追加
                };
                
                int deletedCount = 0;
                
                foreach (var tempFile in tempFiles)
                {
                    try
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                            System.Diagnostics.Debug.WriteLine($"✅ Deleted temp file: {tempFile}");
                            deletedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error deleting temp file {tempFile}: {ex.Message}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Temporary files cleanup completed - {deletedCount} files deleted");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error during temporary files cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// レジストリエントリを削除
        /// </summary>
        private static void CleanupRegistryEntries()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Cleaning up registry entries...");
                
                var startupKey = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                
                if (startupKey != null)
                {
                    var valueNames = new[] { "SELLCT_RebootMessage" };
                    
                    foreach (var valueName in valueNames)
                    {
                        try
                        {
                            if (startupKey.GetValue(valueName) != null)
                            {
                                startupKey.DeleteValue(valueName);
                                System.Diagnostics.Debug.WriteLine($"✅ Deleted registry value: {valueName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Error deleting registry value {valueName}: {ex.Message}");
                        }
                    }
                    
                    startupKey.Close();
                }
                
                System.Diagnostics.Debug.WriteLine("Registry cleanup completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error during registry cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// デスクトップ上のreadme.txtファイルを削除（MetaGameControllerで作成される）
        /// </summary>
        private static void CleanupDesktopReadmeFile()
        {
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var readmePath = Path.Combine(desktopPath, "readme.txt");
                
                if (File.Exists(readmePath))
                {
                    File.Delete(readmePath);
                    System.Diagnostics.Debug.WriteLine($"✅ Deleted desktop readme.txt: {readmePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error deleting desktop readme.txt: {ex.Message}");
            }
        }
    }
}