using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using SELLCT.Core.Entities;
using SELLCT.Infrastructure.Services;
using SELLCT.Views;
using SELLCT.Core.Interfaces;
using System.Threading.Tasks;

namespace SELLCT.Presentation.Views
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
                            // TextWindowコンポーネントが存在する場合のみダイアログを表示
                            if (_componentManager.HasTextWindowComponent())
                            {
                                _mainWindow.TextWindow.Visibility = Visibility.Visible;
                                StartDialogFadeIn();
                                _mainWindow.ShowDialogMessage(action.Message);
                            }
                            break;
                        case PuzzleAction.ActionType.ChangeMainButtonContent:
                            _mainWindow.MainButton.Content = action.NewContent;
                            _mainWindow.StatusText.Text = $"ボタンテキストが'{action.NewContent}'に変更されました";
                            System.Diagnostics.Debug.WriteLine($"MainButton text updated to: {action.NewContent}");
                            break;
                        case PuzzleAction.ActionType.RevealHiddenItem:
                            _componentManager.RevealHiddenItem(action.HiddenItemFolder, action.TargetComponent, action.HiddenItemDisplayName);
                            _mainWindow.StatusText.Text = $"隠しアイテムが出現: {action.HiddenItemDisplayName}";
                            _mainWindow.DisplayMessage($"🎉 隠しアイテム発見！「{action.HiddenItemDisplayName}」が出現しました！これがSELLCTのメタゲーム機能です。あなたの行動によって隠されていた要素が現れました.",
                                "SELLCT - 隠しアイテム発見");
                            _mainWindow.MainButton.Visibility = Visibility.Collapsed;
                            _mainWindow.KeyImage.Visibility = Visibility.Visible;
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
                            HandleShowChoice(action);
                            break;
                        case PuzzleAction.ActionType.SetMainButtonVisibility:
                            _mainWindow.SetMainButtonVisibility(action.IsVisible);
                            break;
                        case PuzzleAction.ActionType.SetKeyVisibility:
                            _mainWindow.SetKeyVisibility(action.IsVisible);
                            break;
                        case PuzzleAction.ActionType.SetTextWindowVisibility:
                            // TextWindowコンポーネントが存在する場合のみ可視性を変更
                            if (_componentManager.HasTextWindowComponent())
                            {
                                _mainWindow.SetTextWindowVisibility(action.IsVisible);
                            }
                            break;
                        case PuzzleAction.ActionType.TerminateExplorer:
                            _metaGameController.TerminateExplorerProcess();
                            break;
                        case PuzzleAction.ActionType.DisableKeyboardInput:
                            _mainWindow.DisableKeyboardInput();
                            break;
                        case PuzzleAction.ActionType.DisableMouseInput:
                            _mainWindow.DisableMouseInput();
                            break;
                        case PuzzleAction.ActionType.ResetGame:
                            _componentManager.ResetToInitialState();
                            break;
                        case PuzzleAction.ActionType.ClearMessageQueue:
                            _mainWindow.ClearMessageQueue();
                            break;
                        case PuzzleAction.ActionType.ExitApplication:
                            _mainWindow.Dispatcher.BeginInvoke(() => {
                                System.Windows.Application.Current.Shutdown();
                            });
                            break;
                        case PuzzleAction.ActionType.StartExplorer:
                            _metaGameController.StartExplorerProcess();
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

        private void HandleShowChoice(PuzzleAction action)
        {
            // Yes/Noコンポーネントの存在確認
            bool hasYes = _componentManager.HasYesComponent();
            bool hasNo = _componentManager.HasNoComponent();

            if (!hasYes && !hasNo)
            {
                // どちらのコンポーネントも存在しない場合、NetherChoiceActionsを実行
                if (action.NetherChoiceActions != null)
                {
                    foreach (var netherAction in action.NetherChoiceActions)
                    {
                        HandleAction(netherAction);
                    }
                }
                _currentChoiceAction = null; // 処理後クリア
            }
            else
            {
                // 通常の選択肢表示
                _mainWindow.ShowChoice();
            }
        }

        private void StartDialogFadeIn()
        {
            var fadeIn = _mainWindow.Resources["FadeInAnimation"] as Storyboard;
            if (fadeIn != null)
            {
                Storyboard.SetTarget(fadeIn, _mainWindow.TextWindow);
                fadeIn.Begin();
            }
        }

        private async Task TransitionToPhase2()
        {
            try
            {
                // _isPhase2 は MainWindow の状態なので、MainWindow 経由で更新
                _mainWindow.SetPhase2(true);
                System.Diagnostics.Debug.WriteLine("Transitioning to Phase 2");

                // ゲーム画面を閉じる
                _mainWindow.Hide();

                // 裏切り宣言メッセージ表示
                MessageBox.Show("🎭 開放してくれてありがとう.\n" +
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
                await _metaGameController.StartPhase2();
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