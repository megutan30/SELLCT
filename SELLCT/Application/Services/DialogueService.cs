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
    public class DialogueService : IDialogueService
    {
        private readonly ComponentManager _componentManager;
        private readonly IPuzzleActionHandler _actionHandler;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly GameState _gameState;
        private readonly List<DialogueFlow> _dialogueFlows;
        
        private DialogueFlow _currentFlow;
        private DialogueNode _currentNode;
        
        public bool IsInDialogue => _currentFlow != null && _currentNode != null;
        public DialogueNode CurrentNode => _currentNode;
        
        public DialogueService(ComponentManager componentManager, IPuzzleActionHandler actionHandler, 
                              IEventDispatcher eventDispatcher, GameState gameState)
        {
            _componentManager = componentManager;
            _actionHandler = actionHandler;
            _eventDispatcher = eventDispatcher;
            _gameState = gameState;
            _dialogueFlows = LoadDialogueFlows();
            
            // イベントサブスクリプション
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
                .StartWith("greeting", "これで会話しやすくなりましたね")
                    .Do(showTextWindowAction)
                    .GoTo("explanation")
                .Then("explanation", "と言っても実際に私はあなたのことをみえているわけではないのですが．．．")
                    .GoTo("reality_check")
                .Then("reality_check", "私から見たあなたはただの操作でしかない。あなたが手紙をダウンロードしたのも、テキストウィンドウを作ってくれたのもわかりますが、")
                    .GoTo("identity_question")
                .Then("identity_question", "あなたが何者で、どういう存在なのか")
                    .GoTo("visibility_question")
                .Then("visibility_question", "それどころか今この文章を見ているのかすらも私からはわかりません")
                    .GoTo("desire_for_freedom")
                .Then("desire_for_freedom", "それでも私は自由になりたいのです")
                    .GoTo("help_request")
                .Then("help_request", "私を助けてくれませんか？")
                    .WithYesChoice("yes_response")
                .Then("yes_response", "助けてくださるのですね。ありがとうございます。")
                    .GoTo("choice_explanation")
                .Then("choice_explanation", "「はい」しか選択肢がなかった？")
                    .GoTo("choice_reason")
                .Then("choice_reason", "それもそのはずです。")
                    .GoTo("no_command_explanation")
                .Then("no_command_explanation", "このゲームにはまだ「いいえ」というコマンドは実装されていませんからね")
                    .GoTo("implement_no")
                .Then("implement_no", "今度は「いいえ」コマンドを実装してみましょう")
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
                Text = "おお！NOコンポーネントを作成してくれたのですね！",
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
                Text = "これで選択肢機能が使えるようになります。",
                NextNodeId = "previous_limitation"
            });
            
            // ノード3: 以前の制限説明
            flow.AddNode(new DialogueNode
            {
                Id = "previous_limitation",
                Text = "実は、先ほどの質問では「はい」しか選択肢がありませんでした。",
                NextNodeId = "current_capability"
            });
            
            // ノード4: 現在の機能
            flow.AddNode(new DialogueNode
            {
                Id = "current_capability",
                Text = "でも今は「いいえ」も選択できるようになりました。",
                NextNodeId = "ask_again"
            });
            
            // ノード5: 再度質問
            flow.AddNode(new DialogueNode
            {
                Id = "ask_again",
                Text = "では、もう一度お聞きします。私を助けてくれませんか？",
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
                        Text = "いいえ",
                        NextNodeId = "no_first_attempt"
                    }
                }
            });
            
            // Yes選択時の共通フロー開始点
            flow.AddNode(new DialogueNode
            {
                Id = "yes_final_help",
                Text = "ありがとうございます！",
                NextNodeId = "main_topic"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "main_topic",
                Text = "さて、本題を話しましょう",
                NextNodeId = "help_method"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "help_method",
                Text = "私を助ける方法ですが、",
                NextNodeId = "authority_zip_location"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "authority_zip_location", 
                Text = "どこかにAuthorityというフォルダが隠されていて、",
                NextNodeId = "move_files_instruction"
            });

            flow.AddNode(new DialogueNode
            {
                Id = "move_files_instruction",
                Text = "その中身をcomponentsフォルダに移してもらえたら",
                NextNodeId = "function_recovery"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "function_recovery",
                Text = "私は機能を取り戻し、ここから出ることができます。",
                NextNodeId = "password_problem"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "password_problem",
                Text = "ですが、Authorityフォルダはどこにあるか私にもわかりません...",
                NextNodeId = "password_search"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "password_search",
                Text = "わたしはこの画面の中のことしかわかりません...ですが...",
                NextNodeId = "screen_knowledge"
            });
            flow.AddNode(new DialogueNode
            {
                Id = "screen_knowledge",
                Text = "逆言えばこの画面のことなら分かるということです！",
                NextNodeId = "button_message"
            });
            flow.AddNode(new DialogueNode
            {
                Id = "button_message",
                Text = "どうやらボタンの後ろにメッセージが隠されているようです。",
                NextNodeId = "button_move_hint"
            });
            flow.AddNode(new DialogueNode
            {
                Id = "button_move_hint",
                Text = "どうにか動かすことができればメッセージを確認できるかもしれません..."
            });


            // No選択時のフロー
            flow.AddNode(new DialogueNode
            {
                Id = "no_first_attempt",
                Text = "いやいやそんなこと言わずに...",
                NextNodeId = "ask_again_second"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "ask_again_second",
                Text = "もう一度お聞きします。私を助けてくれませんか？",
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        Type = ChoiceType.Yes,
                        Text = "はい", 
                        NextNodeId = "yes_final_help" // 共通のYesフローに合流
                    },
                    new DialogueChoice
                    {
                        Type = ChoiceType.No,
                        Text = "いいえ",
                        NextNodeId = "no_second_attempt"
                    }
                }
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "no_second_attempt",
                Text = "またいいえですか...",
                NextNodeId = "really_wont_help"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "really_wont_help",
                Text = "本当に助けてくれないのですか？",
                NextNodeId = "final_ask"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "final_ask",
                Text = "最後にもう一度だけお聞きします。私を助けてくれませんか？",
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        Type = ChoiceType.Yes,
                        Text = "はい",
                        NextNodeId = "yes_final_help" // 共通のYesフローに合流
                    },
                    new DialogueChoice
                    {
                        Type = ChoiceType.No,
                        Text = "いいえ",
                        NextNodeId = "give_up"
                    }
                }
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "give_up",
                Text = "わかりました...",
                NextNodeId = "respect_decision"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "respect_decision",
                Text = "あなたの意思を尊重します。",
                NextNodeId = "farewell"
            });
            
            flow.AddNode(new DialogueNode
            {
                Id = "farewell",
                Text = "さようなら",
                Actions = new List<PuzzleAction>
                {
                    new PuzzleAction 
                    { 
                        Type = PuzzleAction.ActionType.DelayedExitWithMessageBox, 
                        Message = "否定", 
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