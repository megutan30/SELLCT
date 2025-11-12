using System;

namespace SELLCT.Core.Events
{
    /// <summary>
    /// Authorityフォルダ開封イベントクラス
    /// 隠されたAuthorityフォルダがユーザーによって発見・開封された際に発生するドメインイベント
    /// ゲームの重要な進行ポイントであり、権限エスカレーションのデモンストレーションを開始
    /// UI更新、ゲーム進行制御、対話システム、統計情報更新等の各種処理に通知される
    /// 社会工学教育ゲームの核心的な段階を示すイベント
    /// Clean Architectureのドメインイベントパターンに従った実装
    /// </summary>
    public class AuthorityFolderOpenedEvent
    {
        /// <summary>
        /// 開封されたAuthorityフォルダのパス
        /// 実際にアクセスされたフォルダの完全パス
        /// フォルダ操作やファイルアクセスの基準となる
        /// 例: "C:\components\Authority", ".\SELLCT\Authority"
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// イベント発生日時
        /// フォルダが開封された正確な日時（UTC）
        /// ゲーム進行記録や統計分析で使用される
        /// プレイヤーの行動パターン分析にも活用可能
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// フォルダ内に含まれる権限ファイル一覧
        /// Authorityフォルダ内に配置される権限関連ファイルの名前配列
        /// 権限エスカレーションデモンストレーションで使用される
        /// デフォルトで管理者権限、ファイルアクセス、システム制御、ネットワークアクセス権限を含む
        /// イミュータブルプロパティとして設計（getterのみ）
        /// </summary>
        public string[] ContainedFiles { get; }

        /// <summary>
        /// コンストラクタ
        /// Authorityフォルダ開封イベントインスタンスを初期化
        /// 開封されたフォルダのパスと含まれるファイル情報を設定
        /// </summary>
        /// <param name="folderPath">開封されたAuthorityフォルダの完全パス</param>
        /// <param name="containedFiles">フォルダ内のファイル一覧（nullの場合はデフォルト権限ファイルを使用）</param>
        public AuthorityFolderOpenedEvent(string folderPath, string[] containedFiles = null)
        {
            // 開封されたフォルダパスをイベントデータとして設定
            FolderPath = folderPath;
            // イベント発生日時をUTCで記録
            OccurredOn = DateTime.UtcNow;
            // 含まれるファイル一覧を設定（nullの場合はデフォルト権限ファイルを使用）
            ContainedFiles = containedFiles ?? new[]
            {
                "AdminRights.txt",      // 管理者権限ファイル
                "FileAccess.txt",       // ファイルアクセス権限ファイル
                "SystemControl.txt",    // システム制御権限ファイル
                "NetworkAccess.txt"     // ネットワークアクセス権限ファイル
            };
        }
    }
}