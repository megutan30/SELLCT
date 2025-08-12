using System.Threading.Tasks;

namespace SELLCT.Core.Interfaces
{
    /// <summary>
    /// ゲーム内アクションを実行するコマンドのインターフェース
    /// </summary>
    public interface IGameCommand
    {
        /// <summary>
        /// コマンドを実行
        /// </summary>
        /// <param name="context">実行コンテキスト</param>
        Task ExecuteAsync(IGameContext context);
        
        /// <summary>
        /// コマンドが実行可能かどうかを判定
        /// </summary>
        /// <param name="context">実行コンテキスト</param>
        /// <returns>実行可能な場合はtrue</returns>
        bool CanExecute(IGameContext context);
        
        /// <summary>
        /// コマンドの説明（デバッグ用）
        /// </summary>
        string Description { get; }
    }
}