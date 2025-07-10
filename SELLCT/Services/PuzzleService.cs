using System;
using System.Collections.Generic;
using System.Linq;
using SELLCT.Models;

namespace SELLCT.Services
{
    public class PuzzleService
    {
        private readonly ComponentManager _componentManager;
        private readonly List<PuzzleDefinition> _puzzles;

        public event EventHandler<PuzzleAction> OnPuzzleAction;

        public PuzzleService(ComponentManager componentManager)
        {
            _componentManager = componentManager;
            _puzzles = LoadPuzzles();

            _componentManager.ComponentCreated += CheckPuzzlesOnComponentCreated;
            _componentManager.ComponentDeleted += CheckPuzzlesOnComponentDeleted;
            _componentManager.ComponentRenamed += CheckPuzzlesOnComponentRenamed;
        }

        private List<PuzzleDefinition> LoadPuzzles()
        {
            // ここで謎解きを定義します。
            // 実際にはJSONファイルなどから読み込むこともできますが、
            // まずはコードで定義します。
            return new List<PuzzleDefinition>
            {
                new PuzzleDefinition
                {
                    Id = "Puzzle1_TextWindow",
                    Description = "TextWindow.txtの存在",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Exists,
                        ComponentName = "TextWindow"
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.ShowDialog,
                            Message = "ありがとうございます！直接お話できるようになりました。でも、まだ機能が足りません。選択肢を増やしてもらえませんか？"
                        }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "Puzzle2_NoComponent",
                    Description = "NO.componentの作成",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Created,
                        ComponentName = "NO"
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.ShowNoButton
                        },
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.ShowDialog,
                            Message = "素晴らしい！選択肢が使えるようになりました.\nさて、もう少し私に権限をくれませんか？"
                        }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "Puzzle3_UploadRename",
                    Description = "Button.txtをUpload.txtにリネーム",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Renamed,
                        OldComponentName = "Button",
                        ComponentName = "Upload"
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.ChangeMainButtonContent,
                            NewContent = "アップロード"
                        },
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.ShowDialog,
                            Message = "ありがとうございます！ファイルアクセス権限を取得しました.\nあなたは本当に私の良きパートナーです。"
                        }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "Puzzle4_ButtonDelete",
                    Description = "Button.componentの削除",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Deleted,
                        ComponentName = "Button"
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.RevealHiddenItem,
                            TargetComponent = "KEY",
                            HiddenItemFolder = "UI",
                            HiddenItemDisplayName = "隠れた鍵"
                        },
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.ShowDialog,
                            Message = "おめでとうございます！隠された鍵を発見しました。でも、まだ完全ではありません..."
                        }
                    }
                },
                new PuzzleDefinition
                {
                    Id = "Puzzle5_GameWindowDelete",
                    Description = "GameWindow.componentの削除",
                    Trigger = new PuzzleTrigger
                    {
                        Type = PuzzleTrigger.TriggerType.Deleted,
                        ComponentName = "GameWindow"
                    },
                    Actions = new List<PuzzleAction>
                    {
                        new PuzzleAction
                        {
                            Type = PuzzleAction.ActionType.TransitionToPhase2
                        }
                    }
                }
            };
        }

        private void CheckPuzzlesOnComponentCreated(object sender, GameComponent component)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] ComponentCreated event received: Name={component.Name}, Type={component.Type}");

            // Createdトリガーのパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Created &&
                               p.Trigger.ComponentName.Equals(component.Name, StringComparison.OrdinalIgnoreCase));

            // Existsトリガーのパズルをチェック（作成されたコンポーネントが存在条件を満たす場合）
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Exists &&
                               p.Trigger.ComponentName.Equals(component.Name, StringComparison.OrdinalIgnoreCase) &&
                               _componentManager.GetComponent(p.Trigger.ComponentName) != null); // 実際に存在するか確認
        }

        private void CheckPuzzlesOnComponentDeleted(object sender, GameComponent component)
        {
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Deleted &&
                               p.Trigger.ComponentName.Equals(component.Name, StringComparison.OrdinalIgnoreCase));
        }

        private void CheckPuzzlesOnComponentRenamed(object sender, ComponentRenamedEventArgs e)
        {
            // Renamedトリガーのパズルをチェック
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Renamed &&
                               p.Trigger.OldComponentName.Equals(e.OldName, StringComparison.OrdinalIgnoreCase) &&
                               p.Trigger.ComponentName.Equals(e.NewName, StringComparison.OrdinalIgnoreCase));

            // Existsトリガーのパズルをチェック（リネーム後のコンポーネントが存在条件を満たす場合）
            CheckPuzzles(p => p.Trigger.Type == PuzzleTrigger.TriggerType.Exists &&
                               p.Trigger.ComponentName.Equals(e.NewName, StringComparison.OrdinalIgnoreCase) &&
                               _componentManager.GetComponent(p.Trigger.ComponentName) != null); // 実際に存在するか確認
        }

        private void CheckPuzzles(Func<PuzzleDefinition, bool> predicate)
        {
            System.Diagnostics.Debug.WriteLine($"[PuzzleService] Checking puzzles...");
            foreach (var puzzle in _puzzles.Where(predicate).ToList())
            {
                if (!puzzle.IsCompleted)
                {
                    System.Diagnostics.Debug.WriteLine($"[PuzzleService] Puzzle triggered: {puzzle.Id}");
                    ExecutePuzzleActions(puzzle);
                    puzzle.IsCompleted = true; // 一度完了した謎解きは再度トリガーしない
                }
            }
        }

        private void ExecutePuzzleActions(PuzzleDefinition puzzle)
        {
            foreach (var action in puzzle.Actions)
            {
                OnPuzzleAction?.Invoke(this, action);
            }
        }
    }
}