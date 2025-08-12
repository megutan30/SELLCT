using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Commands
{
    /// <summary>
    /// 複数のコマンドをまとめて実行するコンポジットコマンド
    /// </summary>
    public class CompositeCommand : BaseGameCommand
    {
        private readonly List<IGameCommand> _commands;
        private readonly string _description;

        public CompositeCommand(string description, params IGameCommand[] commands)
        {
            _description = description;
            _commands = new List<IGameCommand>(commands);
        }

        public CompositeCommand(string description, IEnumerable<IGameCommand> commands)
        {
            _description = description;
            _commands = new List<IGameCommand>(commands);
        }

        public override string Description => _description;

        public override async Task ExecuteAsync(IGameContext context)
        {
            foreach (var command in _commands)
            {
                if (command.CanExecute(context))
                {
                    await command.ExecuteAsync(context);
                }
            }
        }

        public override bool CanExecute(IGameContext context)
        {
            // 少なくとも1つのコマンドが実行可能であれば実行可能
            return _commands.Any(cmd => cmd.CanExecute(context));
        }

        /// <summary>
        /// コマンドを追加
        /// </summary>
        public void AddCommand(IGameCommand command)
        {
            _commands.Add(command);
        }

        /// <summary>
        /// 含まれるコマンドの一覧（読み取り専用）
        /// </summary>
        public IReadOnlyList<IGameCommand> Commands => _commands.AsReadOnly();
    }
}