using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// ダウンロードファイル記録・削除管理サービス
    /// プレイヤーがダウンロードした手紙やパスワードファイルのパスを記録し、
    /// クリーンアップ時に確実に削除できるようにする
    /// </summary>
    public static class DownloadTracker
    {
        private static readonly string RecordFilePath = Path.Combine(Path.GetTempPath(), "SELLCT_downloads.json");
        private static readonly object LockObject = new object();

        /// <summary>
        /// ダウンロード記録データ構造
        /// </summary>
        public class DownloadRecord
        {
            /// <summary>
            /// ダウンロードされたファイルのフルパス
            /// </summary>
            public string FilePath { get; set; }

            /// <summary>
            /// ダウンロード日時
            /// </summary>
            public DateTime DownloadTime { get; set; }

            /// <summary>
            /// ファイルタイプ（"Letter", "Password"等）
            /// </summary>
            public string FileType { get; set; }

            /// <summary>
            /// ファイル名（削除ログ用）
            /// </summary>
            public string FileName { get; set; }
        }

        /// <summary>
        /// ダウンロードファイルを記録する
        /// </summary>
        /// <param name="filePath">ダウンロードされたファイルのフルパス</param>
        /// <param name="fileType">ファイルタイプ（"Letter", "Password"等）</param>
        public static void RecordDownload(string filePath, string fileType)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                lock (LockObject)
                {
                    var records = LoadRecords();
                    
                    var newRecord = new DownloadRecord
                    {
                        FilePath = filePath,
                        DownloadTime = DateTime.Now,
                        FileType = fileType,
                        FileName = Path.GetFileName(filePath)
                    };

                    records.Add(newRecord);
                    SaveRecords(records);

                    System.Diagnostics.Debug.WriteLine($"✅ Download recorded: {fileType} -> {filePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error recording download: {ex.Message}");
            }
        }

        /// <summary>
        /// 記録されたダウンロードファイル一覧を取得
        /// </summary>
        /// <returns>ダウンロード記録リスト</returns>
        public static List<DownloadRecord> GetDownloadedFiles()
        {
            try
            {
                lock (LockObject)
                {
                    return LoadRecords();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error getting downloaded files: {ex.Message}");
                return new List<DownloadRecord>();
            }
        }

        /// <summary>
        /// 記録されたファイルをすべて削除する
        /// </summary>
        /// <returns>削除に成功したファイル数</returns>
        public static int DeleteTrackedFiles()
        {
            try
            {
                lock (LockObject)
                {
                    var records = LoadRecords();
                    int deletedCount = 0;

                    System.Diagnostics.Debug.WriteLine($"Attempting to delete {records.Count} tracked files...");

                    foreach (var record in records)
                    {
                        try
                        {
                            if (File.Exists(record.FilePath))
                            {
                                File.Delete(record.FilePath);
                                System.Diagnostics.Debug.WriteLine($"✅ Deleted tracked file: {record.FileName} ({record.FileType})");
                                deletedCount++;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ Tracked file not found: {record.FilePath}");
                            }
                        }
                        catch (Exception fileEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Error deleting tracked file {record.FilePath}: {fileEx.Message}");
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Tracked files deletion completed: {deletedCount}/{records.Count} files deleted");
                    return deletedCount;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in DeleteTrackedFiles: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// すべての記録をクリアする
        /// </summary>
        public static void ClearAllRecords()
        {
            try
            {
                lock (LockObject)
                {
                    if (File.Exists(RecordFilePath))
                    {
                        File.Delete(RecordFilePath);
                        System.Diagnostics.Debug.WriteLine("✅ Download records cleared");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error clearing records: {ex.Message}");
            }
        }

        /// <summary>
        /// 記録されたファイルを削除し、記録もクリアする（クリーンアップ用）
        /// </summary>
        /// <returns>削除に成功したファイル数</returns>
        public static int DeleteTrackedFilesAndClearRecords()
        {
            try
            {
                int deletedCount = DeleteTrackedFiles();
                ClearAllRecords();
                return deletedCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in DeleteTrackedFilesAndClearRecords: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 指定したタイプのファイルのみ削除
        /// </summary>
        /// <param name="fileType">削除するファイルタイプ</param>
        /// <returns>削除に成功したファイル数</returns>
        public static int DeleteTrackedFilesByType(string fileType)
        {
            try
            {
                lock (LockObject)
                {
                    var records = LoadRecords();
                    var targetRecords = records.Where(r => r.FileType.Equals(fileType, StringComparison.OrdinalIgnoreCase)).ToList();
                    int deletedCount = 0;

                    foreach (var record in targetRecords)
                    {
                        try
                        {
                            if (File.Exists(record.FilePath))
                            {
                                File.Delete(record.FilePath);
                                System.Diagnostics.Debug.WriteLine($"✅ Deleted {fileType} file: {record.FileName}");
                                deletedCount++;
                            }
                        }
                        catch (Exception fileEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Error deleting {fileType} file {record.FilePath}: {fileEx.Message}");
                        }
                    }

                    return deletedCount;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in DeleteTrackedFilesByType: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 記録ファイルから記録を読み込み
        /// </summary>
        /// <returns>ダウンロード記録リスト</returns>
        private static List<DownloadRecord> LoadRecords()
        {
            if (!File.Exists(RecordFilePath))
            {
                return new List<DownloadRecord>();
            }

            try
            {
                var json = File.ReadAllText(RecordFilePath);
                var records = JsonConvert.DeserializeObject<List<DownloadRecord>>(json);
                return records ?? new List<DownloadRecord>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error loading records: {ex.Message}");
                return new List<DownloadRecord>();
            }
        }

        /// <summary>
        /// 記録リストを記録ファイルに保存
        /// </summary>
        /// <param name="records">保存する記録リスト</param>
        private static void SaveRecords(List<DownloadRecord> records)
        {
            try
            {
                var json = JsonConvert.SerializeObject(records, Formatting.Indented);
                File.WriteAllText(RecordFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error saving records: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 記録の統計情報を取得
        /// </summary>
        /// <returns>統計情報文字列</returns>
        public static string GetStatistics()
        {
            try
            {
                var records = GetDownloadedFiles();
                var letterCount = records.Count(r => r.FileType.Equals("Letter", StringComparison.OrdinalIgnoreCase));
                var passwordCount = records.Count(r => r.FileType.Equals("Password", StringComparison.OrdinalIgnoreCase));

                return $"Total: {records.Count}, Letters: {letterCount}, Passwords: {passwordCount}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error getting statistics: {ex.Message}");
                return "Statistics unavailable";
            }
        }
    }
}