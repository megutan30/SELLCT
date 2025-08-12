using SELLCT.Core.Commands;
using SELLCT.Core.Interfaces;

namespace SELLCT.Core.Factories
{
    /// <summary>
    /// ゲームコマンドを生成するファクトリー
    /// </summary>
    public static class GameCommandFactory
    {
        /// <summary>
        /// ダイアログ表示コマンドを作成
        /// </summary>
        public static IGameCommand ShowDialog(string message)
        {
            return new ShowDialogCommand(message);
        }

        /// <summary>
        /// UI可視性設定コマンドを作成
        /// </summary>
        public static IGameCommand SetVisibility(SetVisibilityCommand.VisibilityTarget target, bool isVisible)
        {
            return new SetVisibilityCommand(target, isVisible);
        }

        /// <summary>
        /// メインボタン表示/非表示コマンド
        /// </summary>
        public static IGameCommand ShowMainButton(bool visible = true)
        {
            return new SetVisibilityCommand(SetVisibilityCommand.VisibilityTarget.MainButton, visible);
        }

        /// <summary>
        /// キー表示/非表示コマンド
        /// </summary>
        public static IGameCommand ShowKey(bool visible = true)
        {
            return new SetVisibilityCommand(SetVisibilityCommand.VisibilityTarget.Key, visible);
        }

        /// <summary>
        /// テキストウィンドウ表示/非表示コマンド
        /// </summary>
        public static IGameCommand ShowTextWindow(bool visible = true)
        {
            return new SetVisibilityCommand(SetVisibilityCommand.VisibilityTarget.TextWindow, visible);
        }

        /// <summary>
        /// プロセス制御コマンドを作成
        /// </summary>
        public static IGameCommand ProcessControl(ProcessControlCommand.ProcessAction action)
        {
            return new ProcessControlCommand(action);
        }

        /// <summary>
        /// エクスプローラー起動コマンド
        /// </summary>
        public static IGameCommand StartExplorer()
        {
            return new ProcessControlCommand(ProcessControlCommand.ProcessAction.StartExplorer);
        }

        /// <summary>
        /// エクスプローラー終了コマンド
        /// </summary>
        public static IGameCommand TerminateExplorer()
        {
            return new ProcessControlCommand(ProcessControlCommand.ProcessAction.TerminateExplorer);
        }

        /// <summary>
        /// キーボード入力有効化コマンド
        /// </summary>
        public static IGameCommand EnableKeyboard()
        {
            return new ProcessControlCommand(ProcessControlCommand.ProcessAction.EnableKeyboardInput);
        }

        /// <summary>
        /// マウス入力有効化コマンド
        /// </summary>
        public static IGameCommand EnableMouse()
        {
            return new ProcessControlCommand(ProcessControlCommand.ProcessAction.EnableMouseInput);
        }

        /// <summary>
        /// ボタンコンテンツ変更コマンドを作成
        /// </summary>
        public static IGameCommand ChangeButtonContent(ChangeButtonContentCommand.ButtonTarget target, string content)
        {
            return new ChangeButtonContentCommand(target, content);
        }

        /// <summary>
        /// ゲーム状態変更コマンドを作成
        /// </summary>
        public static IGameCommand GameState(GameStateCommand.StateAction action)
        {
            return new GameStateCommand(action);
        }

        /// <summary>
        /// ゲームリセットコマンド
        /// </summary>
        public static IGameCommand ResetGame()
        {
            return new GameStateCommand(GameStateCommand.StateAction.ResetGame);
        }

        /// <summary>
        /// No機能有効化コマンド
        /// </summary>
        public static IGameCommand EnableNoFunction()
        {
            return new GameStateCommand(GameStateCommand.StateAction.EnableNoFunction);
        }

        /// <summary>
        /// フェーズ2移行コマンド
        /// </summary>
        public static IGameCommand TransitionToPhase2()
        {
            return new GameStateCommand(GameStateCommand.StateAction.TransitionToPhase2);
        }

        /// <summary>
        /// アプリケーション終了コマンドを作成
        /// </summary>
        public static IGameCommand ExitApplication(string message = null, int delayMs = 0, bool showMessageBox = false)
        {
            return new ExitApplicationCommand(message, delayMs, showMessageBox);
        }

        /// <summary>
        /// コンポジットコマンドを作成
        /// </summary>
        public static IGameCommand Composite(string description, params IGameCommand[] commands)
        {
            return new CompositeCommand(description, commands);
        }
    }
}