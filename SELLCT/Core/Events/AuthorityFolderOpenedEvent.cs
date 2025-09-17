using System;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// Authorityフォルダが開封された時に発生するドメインイベント
    /// </summary>
    public class AuthorityFolderOpenedEvent
    {
        /// <summary>
        /// 開封されたAuthorityフォルダのパス
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// イベント発生日時
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// フォルダ内のファイル一覧（権限ファイル名）
        /// </summary>
        public string[] ContainedFiles { get; }

        public AuthorityFolderOpenedEvent(string folderPath, string[] containedFiles = null)
        {
            FolderPath = folderPath;
            OccurredOn = DateTime.UtcNow;
            ContainedFiles = containedFiles ?? new[]
            {
                "AdminRights.txt",
                "FileAccess.txt", 
                "SystemControl.txt",
                "NetworkAccess.txt"
            };
        }
    }
}