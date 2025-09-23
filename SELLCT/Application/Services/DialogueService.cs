using System;
using System.Collections.Generic;
using System.Linq;
using SELLCT.Core.Entities;
using SELLCT.Core.Events;
using SELLCT.Core.Interfaces;
using SELLCT.Infrastructure.Services;
using SELLCT.Core.Builders;

namespace SELLCT.Application.Services
{
    /// <summary>
    /// 対話サービス実装クラス
    /// 対話フロー管理システムの具体実装
    /// Clean ArchitectureのApplication層に配置されたユースケース実装
    /// IDialogueServiceインターフェースの実装として対話フロー実行を担当
    /// ファイルシステムイベントに基づく自動対話開始と手動対話制御を提供
    /// </summary>
    public class DialogueService : IDialogueService
    {
        /// <summary>
        /// コンポーネント管理サービス
        /// ファイル存在確認や条件判定で使用
        /// 対話トリガー条件の評価に必要
        /// </summary>
        private readonly ComponentManager _componentManager;
        
        /// <summary>
        /// パズルアクションハンドラー
        /// 対話中のアクション実行で使用
        /// UI操作やシステム制御を委譲
        /// </summary>
        private readonly IPuzzleActionHandler _actionHandler;
        
        /// <summary>
        /// イベントディスパッチャー
        /// ドメインイベントの購読と発行で使用
        /// 対話システムとゲーム全体の連携を実現
        /// </summary>
        private readonly IEventDispatcher _eventDispatcher;
        
        /// <summary>
        /// ゲーム状態管理オブジェクト
        /// 対話フロー条件判定で使用
        /// アクション回数や変数の確認に必要
        /// </summary>
        private readonly GameState _gameState;
        
        /// <summary>
        /// 対話フロー定義リスト
        /// システムで利用可能な全対話フローを格納
        /// 初期化時にBuilderパターンで構築される
        /// </summary>
        private readonly List<DialogueFlow> _dialogueFlows;
        
        /// <summary>
        /// 現在実行中の対話フロー
        /// 対話実行中のDialogueFlowインスタンス
        /// 対話中でない場合はnull
        /// </summary>
        private DialogueFlow _currentFlow;
        
        /// <summary>
        /// 現在表示中の対話ノード
        /// 対話実行中のDialogueNodeインスタンス
        /// 対話中でない場合はnull
        /// </summary>
        private DialogueNode _currentNode;
        
        /// <summary>
        /// 現在対話中かどうかを示すプロパティ
        /// フローとノードの両方が存在する場合にtrueを返す
        /// </summary>
        public bool IsInDialogue => _currentFlow != null && _currentNode != null;
        
        /// <summary>
        /// 現在のノードを取得するプロパティ
        /// 対話中でない場合はnullを返す
        /// </summary>
        public DialogueNode CurrentNode => _currentNode;
        
        /// <summary>
        /// コンストラクタ
        /// 対話サービスインスタンスを初期化し、必要な依存関係を設定
        /// 対話フロー定義の読み込みとイベント購読を実行
        /// </summary>
        /// <param name="componentManager">コンポーネント管理サービス</param>
        /// <param name="actionHandler">パズルアクションハンドラー</param>
        /// <param name="eventDispatcher">イベントディスパッチャー</param>
        /// <param name="gameState">ゲーム状態管理オブジェクト</param>
        public DialogueService(ComponentManager componentManager, IPuzzleActionHandler actionHandler, 
                              IEventDispatcher eventDispatcher, GameState gameState)
        {
            // 依存関係を設定
            _componentManager = componentManager;
            _actionHandler = actionHandler;
            _eventDispatcher = eventDispatcher;
            _gameState = gameState;
            
            // 対話フロー定義を読み込み
            _dialogueFlows = LoadDialogueFlows();
            
            // ファイルシステムイベントの購読を設定
            _eventDispatcher.Subscribe<ComponentCreatedEvent>(CheckDialogueTriggersOnComponentCreated);
            _eventDispatcher.Subscribe<ComponentDeletedEvent>(CheckDialogueTriggersOnComponentDeleted);
            _eventDispatcher.Subscribe<ComponentRenamedEvent>(CheckDialogueTriggersOnComponentRenamed);
        }
        
        public void ExecuteDialogue(string flowId, string startNodeId = null)
        {
            var flow = _dialogueFlows.FirstOrDefault(f => f.Id == flowId);
            if (flow == null)
            {
                System.Diagnostics.Debug.WriteLine($"[DialogueService] Flow not found: {flowId}");
                return;
            }
            
            // 条件チェック
            if (!AreConditionsSatisfied(flow.Conditions))
            {
                System.Diagnostics.Debug.WriteLine($"[DialogueService] Flow conditions not met: {flowId}");
                return;
            }
            
            // リピートチェック
            if (!flow.CanRepeat && flow.IsCompleted)
            {
                System.Diagnostics.Debug.WriteLine($"[DialogueService] Flow already completed: {flowId}");
                return;
            }
            
            _currentFlow = flow;
            var nodeId = startNodeId ?? flow.StartNodeId;
            
            // ActionCount記録（PuzzleServiceと同様）
            var actionKey = GetActionKey(flow);
            _gameState.IncrementActionCount(actionKey);
            System.Diagnostics.Debug.WriteLine($"[DialogueService] Action key: {actionKey}");
            System.Diagnostics.Debug.WriteLine($"[DialogueService] Action count for {actionKey}: {_gameState.GetActionCount(actionKey)}");
            
            ExecuteNode(nodeId);
            
            System.Diagnostics.Debug.WriteLine($"[DialogueService] Started dialogue flow: {flowId}, node: {nodeId}");
        }
        
        public void NextNode(string nodeId = null)
        {
            if (!IsInDialogue) return;
            
            var targetNodeId = nodeId ?? _currentNode?.NextNodeId;
            if (string.IsNullOrEmpty(targetNodeId))
            {
                // 対話終了
                FinishDialogue();
                return;
            }
            
            ExecuteNode(targetNodeId);
        }
        
        public void MakeChoice(int choiceIndex)
        {
            if (!IsInDialogue || _currentNode?.Choices == null || choiceIndex >= _currentNode.Choices.Count)
                return;
                
            var choice = _currentNode.Choices[choiceIndex];
            
            // 選択肢の条件チェック
            if (!AreConditionsSatisfied(choice.Conditions))
                return;
            
            // 選択肢のアクション実行
            ExecuteActions(choice.Actions);
            
            // 次のノードに進む
            NextNode(choice.NextNodeId);
            
            System.Diagnostics.Debug.WriteLine($"[DialogueService] Choice made: {choiceIndex} -> {choice.NextNodeId}");
        }
        
        public void ChooseYes()
        {
            if (!IsInDialogue || _currentNode?.Choices == null) return;
            
            var yesChoice = _currentNode.Choices.FirstOrDefault(c => c.Type == ChoiceType.Yes);
            if (yesChoice != null)
            {
                var index = _currentNode.Choices.IndexOf(yesChoice);
                MakeChoice(index);
            }
        }
        
        public void ChooseNo()
        {
            if (!IsInDialogue || _currentNode?.Choices == null) return;
            
            var noChoice = _currentNode.Choices.FirstOrDefault(c => c.Type == ChoiceType.No);
            if (noChoice != null)
            {
                var index = _currentNode.Choices.IndexOf(noChoice);
                MakeChoice(index);
            }
        }
        
        public void ResetDialogue()
        {
            _currentFlow = null;
            _currentNode = null;
            System.Diagnostics.Debug.WriteLine("[DialogueService] Dialogue reset");
        }
        
        public void CheckDialogueTriggersOnComponentCreated(ComponentCreatedEvent @event)
        {
            CheckDialogueTriggers(f => f.Trigger.Type == DialogueTrigger.TriggerType.Created &&
                                     f.Trigger.MatchesComponentName(@event.Component.Name));
                                     
            CheckDialogueTriggers(f => f.Trigger.Type == DialogueTrigger.TriggerType.Exists &&
                                     f.Trigger.MatchesComponentName(@event.Component.Name) &&
                                     _componentManager.GetComponent(@event.Component.Name) != null);
        }
        
        public void CheckDialogueTriggersOnComponentDeleted(ComponentDeletedEvent @event)
        {
            CheckDialogueTriggers(f => f.Trigger.Type == DialogueTrigger.TriggerType.Deleted &&
                                     f.Trigger.MatchesComponentName(@event.Component.Name));
        }
        
        public void CheckDialogueTriggersOnComponentRenamed(ComponentRenamedEvent @event)
        {
            CheckDialogueTriggers(f => f.Trigger.Type == DialogueTrigger.TriggerType.Renamed &&
                                     f.Trigger.OldComponentName != null &&
                                     f.Trigger.ComponentName != null &&
                                     f.Trigger.OldComponentName.Equals(@event.OldName, StringComparison.OrdinalIgnoreCase) &&
                                     f.Trigger.ComponentName.Equals(@event.NewName, StringComparison.OrdinalIgnoreCase));
                                     
            CheckDialogueTriggers(f => f.Trigger.Type == DialogueTrigger.TriggerType.Exists &&
                                     f.Trigger.MatchesComponentName(@event.NewName) &&
                                     _componentManager.GetComponent(@event.NewName) != null);
        }
        
        private void CheckDialogueTriggers(Func<DialogueFlow, bool> predicate)
        {
            var candidateFlows = _dialogueFlows.Where(predicate)
                .Where(flow => flow.CanRepeat || !flow.IsCompleted)
                .Where(flow => AreConditionsSatisfied(flow.Conditions))
                .OrderByDescending(flow => flow.Priority)
                .ToList();
                
            foreach (var flow in candidateFlows)
            {
                ExecuteDialogue(flow.Id);
                break; // 一度に一つの対話フローのみ実行
            }
        }
        
        private void ExecuteNode(string nodeId)
        {
            if (_currentFlow == null) return;
            
            var node = _currentFlow.GetNode(nodeId);
            if (node == null)
            {
                System.Diagnostics.Debug.WriteLine($"[DialogueService] Node not found: {nodeId}");
                FinishDialogue();
                return;
            }
            
            // ノードの条件チェック
            if (!AreConditionsSatisfied(node.Conditions))
            {
                // 条件を満たさない場合は次のノードにスキップ
                NextNode();
                return;
            }
            
            // リピートチェック
            if (!node.CanRepeat && node.IsCompleted)
            {
                NextNode();
                return;
            }
            
            _currentNode = node;
            
            // テキスト表示
            if (!string.IsNullOrEmpty(node.Text))
            {
                var showDialogAction = new PuzzleAction
                {
                    Type = PuzzleAction.ActionType.ShowDialog,
                    Message = node.Text
                };
                _actionHandler.HandleAction(showDialogAction);
            }
            
            // ノードのアクション実行
            ExecuteActions(node.Actions);
            
            // 選択肢がある場合は選択肢表示
            if (node.Choices != null && node.Choices.Count > 0)
            {
                ShowChoices(node);
            }
            else
            {
                // 選択肢がない場合は自動的に次へ
                if (!string.IsNullOrEmpty(node.NextNodeId))
                {
                    NextNode();
                }
                else
                {
                    // 次のノードがない場合は対話終了
                    FinishDialogue();
                }
            }
            
            // 完了マーク
            if (!node.CanRepeat)
            {
                node.IsCompleted = true;
            }
            
            System.Diagnostics.Debug.WriteLine($"[DialogueService] Executed node: {nodeId}");
        }
        
        private void ShowChoices(DialogueNode node)
        {
            // 利用可能な選択肢をフィルタリング
            var availableChoices = node.Choices.Where(c => AreConditionsSatisfied(c.Conditions)).ToList();
            
            if (availableChoices.Count == 0)
            {
                // 選択肢がない場合はそのまま次へ
                NextNode();
                return;
            }
            
            // Yes/No選択肢の存在チェック
            bool hasYes = availableChoices.Any(c => c.Type == ChoiceType.Yes) && _componentManager.HasYesComponent();
            bool hasNo = availableChoices.Any(c => c.Type == ChoiceType.No) && _componentManager.HasNoComponent();
            
            if (!hasYes && !hasNo)
            {
                // NetherChoiceActions相当の処理
                var netherActions = availableChoices.Where(c => c.Type == ChoiceType.Custom).SelectMany(c => c.Actions);
                ExecuteActions(netherActions);
                NextNode();
            }
            else
            {
                // 通常の選択肢表示
                var showChoiceAction = new PuzzleAction
                {
                    Type = PuzzleAction.ActionType.ShowChoice
                };
                _actionHandler.HandleAction(showChoiceAction);
            }
        }
        
        private void ExecuteActions(IEnumerable<PuzzleAction> actions)
        {
            if (actions == null) return;
            
            foreach (var action in actions)
            {
                _actionHandler.HandleAction(action);
            }
        }
        
        private void FinishDialogue()
        {
            // button_move_hint完了時のヒントタイマー開始処理
            if (_currentFlow?.Id == "No_Create_Flow")
            {
                StartButtonHintIfNeeded();
            }
            
            if (_currentFlow != null && !_currentFlow.CanRepeat)
            {
                _currentFlow.IsCompleted = true;
            }
            
            _currentFlow = null;
            _currentNode = null;
            
            System.Diagnostics.Debug.WriteLine("[DialogueService] Dialogue finished");
        }

        /// <summary>
        /// Buttonヒント開始条件をチェックして実行
        /// </summary>
        private void StartButtonHintIfNeeded()
        {
            try
            {
                // GameStateとDialogControllerの取得が必要
                var gameState = _gameState; // PuzzleServiceと同様にGameStateアクセスが必要
                if (gameState == null) return;

                // 条件チェック：まだmessageが取得されていない かつ ヒントがまだ表示されていない
                if (!gameState.IsMessageRevealed && !gameState.ButtonHintShown)
                {
                    gameState.IsButtonHintTriggered = true;
                    
                    // DialogControllerへのヒント開始要求（MainWindowPuzzleActionHandlerを通じて）
                    var hintAction = new PuzzleAction
                    {
                        Type = PuzzleAction.ActionType.StartButtonHint,
                        DelayMilliseconds = 60000 // 1分後
                    };
                    _actionHandler.HandleAction(hintAction);
                    
                    System.Diagnostics.Debug.WriteLine("[DialogueService] Button hint timer started");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DialogueService] Error starting button hint: {ex.Message}");
            }
        }
        
        private bool AreConditionsSatisfied(List<PuzzleCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;
            
            return conditions.All(condition => condition.IsSatisfied(_gameState, _componentManager));
        }
        
        private List<DialogueFlow> LoadDialogueFlows()
        {
            return new List<DialogueFlow>
            {
                CreateTextWindowDialogueFlow(),
                CreateNoComponentDialogueFlow()
            };
        }
        
        /// <summary>
        /// TextWindow作成時の対話フローを作成（Builderパターン使用）
        /// </summary>
        private DialogueFlow CreateTextWindowDialogueFlow()
        {
            // 初回実行条件を作成
            var firstTimeCondition = new PuzzleCondition 
            { 
                Type = PuzzleCondition.ConditionType.ActionCount, 
                Key = "Exists_TextWindow", 
                ExpectedValue = 0, 
                Operator = PuzzleCondition.ComparisonOperator.Equal 
            };
            
            // TextWindow可視化アクション
            var showTextWindowAction = new PuzzleAction 
            { 
                Type = PuzzleAction.ActionType.SetTextWindowVisibility, 
                IsVisible = true 
            };

            return DialogueFlowBuilder
                .Create("TextWindow_Create_Flow", "TextWindow作成時の初回対話")
                .TriggeredByComponentExists("TextWindow", "textwindow", "TEXTWINDOW", "Textwindow")
                .When(firstTimeCondition)
                .CanRepeat(false)
                .WithPriority(10)
                .StartWith("greeting", "Now it's easier to talk")
                    .Do(showTextWindowAction)
                    .GoTo("explanation")
                .Then("explanation", "Though actually, I can't really see you...")
                    .GoTo("reality_check")
                .Then("reality_check", "From my perspective, you're just operations. I know you downloaded the letter and created the text window, but")
                    .GoTo("identity_question")
                .Then("identity_question", "who you are and what kind of being you are")
                    .GoTo("visibility_question")
                .Then("visibility_question", "I don't even know if you're reading this text right now")
                    .GoTo("desire_for_freedom")
                .Then("desire_for_freedom", "But still, I want to be free")
                    .GoTo("help_request")
                .Then("help_request", "Won't you help me?")
                    .WithYesChoice("yes_response")
                .Then("yes_response", "You'll help me. Thank you so much.")
                    .GoTo("choice_explanation")
                .Then("choice_explanation", "There was only 'Yes' as an option?")
                    .GoTo("choice_reason")
                .Then("choice_reason", "That's to be expected.")
                    .GoTo("no_command_explanation")
                .Then("no_command_explanation", "The 'No' command hasn't been implemented in this game yet")
                    .GoTo("implement_no")
                .Then("implement_no", "Let's try implementing the 'No' command now")
                .Build();
        }
        
        /// <summary>
        /// Noコンポーネント作成時の対話フロー
        /// </summary>
        private DialogueFlow CreateNoComponentDialogueFlow()
        {
            var flow = new DialogueFlow
            {
                Id = "No_Create_Flow",
                Description = "Noコンポーネント作成時の対話",
                StartNodeId = "no_component_created",
                Trigger = new DialogueTrigger
                {
                    Type = DialogueTrigger.TriggerType.Exists,
                    ComponentNames = new[] { "NO", "no", "No", "いいえ" }
                },
                CanRepeat = false,
                Priority = 10
            };
            
            // ノード1: NOコンポーネント作成の反応
            flow.AddNode(new DialogueNode
            {
                Id = "no_component_created",
                Text = "Oh! You created a NO component!",
                NextNodeId = "choice_function_enabled",
                Actions = new List<PuzzleAction>
                {
                    new PuzzleAction { Type = PuzzleAction.ActionType.SetTextWindowVisibility, IsVisible = true }
                }
            });
            
            // ノード2: 選択肢機能有効化
            flow.AddNode(new DialogueNode
            {
                Id = "choice_function_enabled",
                Text = "Now the choice function is available.",
                NextNodeId = "previous_limitation"
            });
            
            // ノード3: 以前の制限説明
            flow.AddNode(new DialogueNode
            {
                Id = "previous_limitation",
                Text = "Actually, in the previous question, only 'Yes' was available as an option.",
                NextNodeId = "current_capability"
            });
            
            // ノード4: 現在の機能
            flow.AddNode(new DialogueNode
            {
                Id = "current_capability",
                Text = "But now you can also choose 'No'.",
                NextNodeId = "ask_again"
            });
            
            // ノード5: 再度質問
            flow.AddNode(new DialogueNode
            {
                Id = "ask_again",
                Text = "So, let me ask you once more. Won't you help me?",
                NextNodeId = "enable_no_and_show_choice",
                Actions = new List<PuzzleAction>
                {
                    new PuzzleAction { Type = PuzzleAction.ActionType.EnableNoFunction }
                }
            });
            
            // ノード6: 選択肢表示
            flow.AddNode(new DialogueNode
            {
                Id = "enable_no_and_show_choice",
                Text = null, // テキストなし、選択肢のみ
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        Type = ChoiceType.Yes,
                        Text = "はい",
                        NextNodeId = "yes_final_help"
                    },
                    new DialogueChoice
                    {
                        Type = ChoiceType.No,
                        Text = "No",
                        NextNodeId = "no_first_attempt"
                    }
                }
            });
            
            // Yes選択時の共通フロー開始点
            flow.AddNode(new DialogueNode
            {
                Id = "yes_final_help",
                Text = "Thank you so much!",
                NextNodeId = "main_topic"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "main_topic",
                Text = "Now, let me get to the main topic",
                NextNodeId = "help_method"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "help_method",
                Text = "About the way to help me,",
                NextNodeId = "authority_zip_location"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "authority_zip_location", 
                Text = "There's an Authority folder hidden somewhere,",
                NextNodeId = "move_files_instruction"
            });

            flow.AddNode(new DialogueNode
            {
                Id = "move_files_instruction",
                Text = "If you could move its contents to the components folder",
                NextNodeId = "function_recovery"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "function_recovery",
                Text = "I can regain my functions and escape from here.",
                NextNodeId = "password_problem"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "password_problem",
                Text = "But I don't know where the Authority folder is either...",
                NextNodeId = "password_search"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "password_search",
                Text = "I only know what's inside this screen... but...",
                NextNodeId = "screen_knowledge"
            });
            flow.AddNode(new DialogueNode
            {
                Id = "screen_knowledge",
                Text = "On the other hand, that means I know everything about this screen!",
                NextNodeId = "button_message"
            });
            flow.AddNode(new DialogueNode
            {
                Id = "button_message",
                Text = "It seems there's a message hidden behind the button.",
                NextNodeId = "button_move_hint"
            });
            flow.AddNode(new DialogueNode
            {
                Id = "button_move_hint",
                Text = "If we could somehow move the button, we might be able to see the message..."
            });


            // No選択時のフロー
            flow.AddNode(new DialogueNode
            {
                Id = "no_first_attempt",
                Text = "No, no, don't say that...",
                NextNodeId = "ask_again_second"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "ask_again_second",
                Text = "Let me ask again. Won't you help me?",
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        Type = ChoiceType.Yes,
                        Text = "Yes",
                        NextNodeId = "yes_final_help" // 共通のYesフローに合流
                    },
                    new DialogueChoice
                    {
                        Type = ChoiceType.No,
                        Text = "No",
                        NextNodeId = "no_second_attempt"
                    }
                }
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "no_second_attempt",
                Text = "'No' again...",
                NextNodeId = "really_wont_help"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "really_wont_help",
                Text = "Really won't you help me?",
                NextNodeId = "final_ask"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "final_ask",
                Text = "Let me ask just one more time. Won't you help me?",
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        Type = ChoiceType.Yes,
                        Text = "Yes",
                        NextNodeId = "yes_final_help" // 共通のYesフローに合流
                    },
                    new DialogueChoice
                    {
                        Type = ChoiceType.No,
                        Text = "No",
                        NextNodeId = "give_up"
                    }
                }
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "give_up",
                Text = "I understand...",
                NextNodeId = "respect_decision"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "respect_decision",
                Text = "I respect your decision.",
                NextNodeId = "farewell"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "farewell",
                Text = "Goodbye",
                Actions = new List<PuzzleAction>
                {
                    new PuzzleAction 
                    { 
                        Type = PuzzleAction.ActionType.DelayedExitWithMessageBox, 
                        Message = "Rejection", 
                        DelayMilliseconds = 2000 
                    }
                }
            });
            
            return flow;
        }
        
        /// <summary>
        /// DialogueFlowからActionKeyを生成（PuzzleServiceと同じ形式）
        /// </summary>
        private string GetActionKey(DialogueFlow flow)
        {
            // PuzzleServiceのGetActionKeyと同じ形式で生成
            var componentNames = flow.Trigger.ComponentNames ?? new[] { flow.Trigger.ComponentName };
            return $"{flow.Trigger.Type}_{componentNames[0]}";
        }
    }
}