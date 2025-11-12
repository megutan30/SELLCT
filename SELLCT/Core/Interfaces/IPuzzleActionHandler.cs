using SELLCT.Core.Entities;

namespace SELLCT.Core.Interfaces
{
    /// <summary>
    /// パズルアクションハンドラーインターフェース
    /// パズルシステムで発生するアクションを処理するための抽象化
    /// Clean Architectureの依存性逆転原則に従った設計
    /// ドメイン層がインフラストラクチャ層の実装に依存しないようにするための契約
    /// UI操作、システム制御、ゲーム状態変更等の具体的なアクション実行を抽象化
    /// </summary>
    public interface IPuzzleActionHandler
    {
        /// <summary>
        /// パズルアクションを処理する
        /// 指定されたPuzzleActionインスタンスに基づいて具体的な処理を実行
        /// アクションの種類に応じて適切な処理ルートに分岐する
        /// UI更新、システム操作、ファイル操作、メッセージ表示等を実行
        /// 実装クラスはMainWindowPuzzleActionHandler等が想定される
        /// </summary>
        /// <param name="action">実行するパズルアクション（ActionType、パラメータ等を含む）</param>
        void HandleAction(PuzzleAction action);
    }
}
