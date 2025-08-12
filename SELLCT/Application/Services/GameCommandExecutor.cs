using System;
using System.Threading.Tasks;
using SELLCT.Core.Interfaces;

namespace SELLCT.Application.Services
{
    /// <summary>
    /// ゲームコマンドを実行するサービス
    /// </summary>
    public class GameCommandExecutor
    {
        private readonly IGameContext _context;

        public GameCommandExecutor(IGameContext context)
        {
            _context = context;
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        /// <returns>実行結果（成功した場合はtrue）</returns>
        public async Task<bool> ExecuteAsync(IGameCommand command)
        {
            try
            {
                if (!command.CanExecute(_context))
                {
                    System.Diagnostics.Debug.WriteLine($"[GameCommandExecutor] Command cannot be executed: {command.Description}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[GameCommandExecutor] Executing command: {command.Description}");
                await command.ExecuteAsync(_context);
                System.Diagnostics.Debug.WriteLine($"[GameCommandExecutor] Command executed successfully: {command.Description}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameCommandExecutor] Error executing command '{command.Description}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 複数のコマンドを順次実行
        /// </summary>
        /// <param name="commands">実行するコマンド群</param>
        /// <returns>すべて成功した場合はtrue</returns>
        public async Task<bool> ExecuteAsync(params IGameCommand[] commands)
        {
            bool allSuccessful = true;
            
            foreach (var command in commands)
            {
                var success = await ExecuteAsync(command);
                if (!success)
                {
                    allSuccessful = false;
                    // エラーがあっても続行する（場合によってはbreakする設計も可能）
                }
            }

            return allSuccessful;
        }

        /// <summary>
        /// コマンドが実行可能かチェック
        /// </summary>
        /// <param name="command">チェックするコマンド</param>
        /// <returns>実行可能な場合はtrue</returns>
        public bool CanExecute(IGameCommand command)
        {
            return command.CanExecute(_context);
        }
    }
}