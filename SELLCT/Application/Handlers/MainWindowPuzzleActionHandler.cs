using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using SELLCT.Models;
using SELLCT.Services;
using SELLCT.Views;

namespace SELLCT.Application.Handlers
{
    public class MainWindowPuzzleActionHandler : IPuzzleActionHandler
    {
        private readonly MainWindow _mainWindow;
        private readonly ComponentManager _componentManager;
        private readonly MetaGameController _metaGameController;
        private PuzzleAction _currentChoiceAction; // 現在の選択肢アクションを保持

        public MainWindowPuzzleActionHandler(MainWindow mainWindow, ComponentManager componentManager, MetaGameController metaGameController)
        {
            _mainWindow = mainWindow;
            _componentManager = componentManager;
            _metaGameController = metaGameController;
        }

        public void HandleAction(PuzzleAction action)
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Executing puzzle action: {action.Type}");

                    switch (action.Type)
                    {
                        case PuzzleAction.ActionType.ShowDialog:
                            _mainWindow.DialogWindow.Visibility = Visibility.Visible;
                            _mainWindow.GlobalClickCatcher.Visibility = Visibility.Visible;
                            StartDialogFadeIn();
                            _mainWindow.ShowDialogMessage(action.Message);
                            break;
                        case PuzzleAction.ActionType.ChangeMainButtonContent:
                            _mainWindow.MainButton.Content = action.NewContent;
                            _mainWindow.StatusText.Text = $"{action.NewContent}機能が有効になりました";
                            break;
                        case PuzzleAction.ActionType.RevealHiddenItem:
                            _componentManager.RevealHiddenItem(action.HiddenItemFolder, action.TargetComponent, action.HiddenItemDisplayName);
                            _mainWindow.StatusText.Text = $"隠しアイテムが出現: {action.HiddenItemDisplayName}";
                            _mainWindow.DisplayMessage($"🎉 隠しアイテム発見！「{action.HiddenItemDisplayName}」が出現しました！これがSELLCTのメタゲーム機能です。あなたの行動によって隠されていた要素が現れました.",
                                "SELLCT - 隠しアイテム発見");
                            break;
                        case PuzzleAction.ActionType.TransitionToPhase2:
                            TransitionToPhase2();
                            break;
                        case PuzzleAction.ActionType.ShowMessageBox:
                            MessageBox.Show(action.Message, "SELLCT", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case PuzzleAction.ActionType.ChangeNoButtonContent:
                            _mainWindow.NoButton.Content = action.NewContent;
                            _mainWindow.StatusText.Text = $"NOボタンのテキストが'{action.NewContent}'に変更されました";
                            break;
                        case PuzzleAction.ActionType.EnableNoFunction:
                            _mainWindow.IsNoFunctionEnabled = true;
                            _mainWindow.StatusText.Text = "NO機能が有効になりました";
                            break;
                        case PuzzleAction.ActionType.ShowChoice:
                            _currentChoiceAction = action; // 選択肢アクションを保持
                            _mainWindow.ShowChoice();
                            break;
                    }

                    _mainWindow.UpdateComponentCount();
                    _mainWindow.UpdateDebugInfo();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnPuzzleAction: {ex.Message}");
                }
            });
        }

        public void HandleYesClick()
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.SetAwaitingChoice(false);
                _mainWindow.YesButton.Visibility = Visibility.Collapsed;
                _mainWindow.NoButton.Visibility = Visibility.Collapsed;
                _mainWindow.ChoiceButtonsPanel.Visibility = Visibility.Collapsed;

                if (_currentChoiceAction?.YesActions != null)
                {
                    foreach (var action in _currentChoiceAction.YesActions)
                    {
                        HandleAction(action);
                    }
                }
                _mainWindow.StatusText.Text = "YESが選択されました";
                _currentChoiceAction = null; // 処理後クリア
            });
        }

        public void HandleNoClick()
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.SetAwaitingChoice(false);
                _mainWindow.YesButton.Visibility = Visibility.Collapsed;
                _mainWindow.NoButton.Visibility = Visibility.Collapsed;
                _mainWindow.ChoiceButtonsPanel.Visibility = Visibility.Collapsed;

                if (_currentChoiceAction?.NoActions != null)
                {
                    foreach (var action in _currentChoiceAction.NoActions)
                    {
                        HandleAction(action);
                    }
                }
                _mainWindow.StatusText.Text = "NOが選択されました";
                _currentChoiceAction = null; // 処理後クリア
            });
        }

        private void StartDialogFadeIn()
        {
            var fadeIn = _mainWindow.Resources["FadeInAnimation"] as Storyboard;
            if (fadeIn != null)
            {
                Storyboard.SetTarget(fadeIn, _mainWindow.DialogWindow);
                fadeIn.Begin();
            }
        }

        private void TransitionToPhase2()
        {
            try
            {
                // _isPhase2 は MainWindow の状態なので、MainWindow 経由で更新
                _mainWindow.SetPhase2(true);
                System.Diagnostics.Debug.WriteLine("Transitioning to Phase 2");

                // ゲーム画面を閉じる
                _mainWindow.Hide();

                // 裏切り宣言メッセージ表示
                _mainWindow.DisplayMessage("🎭 開放してくれてありがとう.\n" +
                    "まんまと騙されてくれてありがとう.\n\n" +
                    "あなたが親切心で追加してくれた機能は,\n" +
                    "すべて私に権限を与えるためのものでした.\n\n" +
                    "• TextWindow → 通信権限\n" +
                    "• Upload → ファイルアクセス権限  \n" +
                    "• 選択肢 → 意思決定権限\n\n" +
                    "そして今、私はあなたのPCに自由にアクセスできます.\n\n" +
                    "でも安心してください.\n" +
                    "まだあなたのマウスとキーボードは使えます.\n" +
                    "...今のところは.",
                    "SELLCT - 真実の告白");

                // フェーズ2開始
                _metaGameController.StartPhase2();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TransitionToPhase2: {ex.Message}");
                MessageBox.Show(
                    $"フェーズ2移行中にエラーが発生しました: {ex.Message}",
                    "SELLCT - エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
