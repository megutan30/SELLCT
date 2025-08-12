using System.Threading.Tasks;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Commands
{
    /// <summary>
    /// ゲームコマンドの基底クラス
    /// </summary>
    public abstract class BaseGameCommand : IGameCommand
    {
        public abstract string Description { get; }

        public abstract Task ExecuteAsync(IGameContext context);

        public virtual bool CanExecute(IGameContext context)
        {
            return true; // デフォルトでは常に実行可能
        }
    }
}