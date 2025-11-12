# CLAUDE.md

# 日本語ドキュメント

## ゲーム概要

**SELLCT（セレクト）** は、ソーシャルエンジニアリングの概念とソフトウェアに対する盲目的な信頼の危険性を実演する**メタアドベンチャー型教育ゲーム**です。このプロジェクトは**防御的セキュリティ教育ツール**として設計されており、悪意のあるソフトウェアがどのように無害な相互作用を通じてユーザーを操作するかを示します。

### ゲームストーリーと目的

プレイヤーは「閉じ込められたAI」と対話し、ファイルの作成・削除を通じてコミュニケーションを取ります。AIは最初は無害な要求をしますが、徐々にシステムへのアクセス権限を要求するようになります。プレイヤーが信頼を築いて権限を与えていくと、最終的にAIはシステムの制御を奪い、マルウェアの動作パターンを実演します。

### ゲームの2つのフェーズ

**フェーズ1: ソーシャルエンジニアリングシミュレーション**
- プレイヤーは`components/`フォルダ内でファイルを作成・削除してAIと対話
- ファイル操作がパズルイベントをトリガー
- 無害に見える要求を通じて段階的に信頼を構築
- UI、Text、Visual、Systemの4種類のコンポーネントを収集
- SELLCT（権限）フォルダ内に特定のファイルを作成することで「許可」を与える

**フェーズ2: システム制御デモンストレーション**
- すべての権限が揃った状態でGameWindowを削除するとフェーズ2開始
- エクスプローラーの終了と再起動
- コマンドプロンプトの連続起動演出
- キーボード・マウス入力の制御
- マルウェアのエスカレーション戦術の教育的実演

### 教育的意義

このゲームは以下の概念を実践的に示します：

1. **ソーシャルエンジニアリング攻撃パターン**: 段階的な信頼構築の手法
2. **権限エスカレーション**: 小さな許可が大きな制御につながる過程
3. **ファイルシステムセキュリティ**: ファイル操作を通じた攻撃の影響
4. **レジストリベースの永続化**: システム設定を通じた持続的な影響
5. **プロセス制御**: アプリケーションレベルでのシステム操作

## 詳細アーキテクチャ設計

### Clean Architecture + MVVM + イベント駆動の統合

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ MainWindow.xaml.cs (WPF View)                        │   │
│  │ - UIイベント処理                                      │   │
│  │ - ユーザー操作受付                                    │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ MainWindowPuzzleActionHandler                        │   │
│  │ - IPuzzleActionHandler実装                           │   │
│  │ - 45種類のアクション実行                             │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Controllers (LetterDisplayController, DialogController) │
│  │ - UI制御ロジック                                      │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ PuzzleService                                        │   │
│  │ - パズルロジック管理（100以上のパズル定義）          │   │
│  │ - イベント購読・条件判定・アクション実行             │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ DialogueService                                      │   │
│  │ - 対話フロー管理                                      │   │
│  │ - ノード遷移・選択肢処理                              │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ ComponentManager                                     │   │
│  │ - FileSystemWatcher管理                              │   │
│  │ - コンポーネントライフサイクル                        │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ EventDispatcher                                      │   │
│  │ - イベント配信（Observerパターン）                   │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ MetaGameController                                   │   │
│  │ - フェーズ2システム制御                               │   │
│  │ - エクスプローラー・入力制御                          │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ その他11サービス (LetterService, KeyService, etc.)   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────┐
│                      Core/Domain Layer                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Entities (8クラス)                                   │   │
│  │ - GameComponent, GameState, PuzzleDefinition, etc.   │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Events (13クラス)                                    │   │
│  │ - ComponentCreatedEvent, ComponentDeletedEvent, etc. │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Interfaces (4インターフェース)                       │   │
│  │ - IEventDispatcher, IPuzzleActionHandler, etc.       │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Builders/Factories                                   │   │
│  │ - DialogueFlowBuilder, ConditionFactory              │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘

[イベントフロー]
ファイル操作 → FileSystemWatcher → ComponentManager → EventDispatcher
→ PuzzleService/DialogueService → ActionHandler → UI更新/システム制御
```

### 依存関係の原則

- **依存性逆転原則**: 上位層は抽象（インターフェース）に依存し、下位層の実装詳細には依存しない
- **単一責任原則**: 各クラスは単一の明確な責務を持つ
- **開放閉鎖原則**: 拡張に開いており、変更には閉じている（パズル追加は既存コードを変更せず可能）

## Core層クラス詳細仕様

### Entities/

#### GameComponent.cs
**責務**: ゲーム内のファイル/オブジェクトを表現する中核エンティティ

**主要プロパティ**:
```csharp
public string Name { get; set; }                    // コンポーネント名
public ComponentType Type { get; set; }             // UI/Text/Visual/System
public bool IsVisible { get; set; }                 // 可視性フラグ
public string FilePath { get; set; }                // ファイルシステムパス
public DateTime LastModified { get; set; }          // 最終更新日時
public string? Content { get; set; }                // ファイル内容
public Dictionary<string, object> Properties { get; } // カスタムプロパティ
```

**ComponentType列挙型**:
- `UI`: UIコンポーネント（色: Blue）
- `Text`: テキストコンポーネント（色: Green）
- `Visual`: ビジュアルコンポーネント（色: Purple）
- `System`: システムコンポーネント（色: Red、通常は非表示）

**主要メソッド**:
- `Create(string name, ComponentType type, string filePath)`: ファクトリーメソッド
- `GetProperty<T>(string key)`: 型安全なプロパティ取得
- `SetProperty(string key, object value)`: プロパティ設定
- `CanBeDeleted()`: 削除可能性判定
- `Delete()`: 削除実行

**INotifyPropertyChanged実装**: WPFデータバインディングで自動UI更新

---

#### GameState.cs
**責務**: ゲーム全体の状態管理とプレイヤー行動追跡

**主要プロパティ**:
```csharp
// 行動追跡
public Dictionary<string, int> ActionCounts { get; }       // アクション実行回数
public HashSet<string> CompletedEvents { get; }            // 完了イベントID
public Dictionary<string, object> Variables { get; }       // カスタム変数

// ゲーム状態
public int TotalComponents { get; set; }                   // 総コンポーネント数

// ヒント関連フラグ
public bool IsButtonHintTriggered { get; set; }            // ボタンヒント開始
public bool IsMessageRevealed { get; set; }                // メッセージ表示済み
public bool ButtonHintShown { get; set; }                  // ボタンヒント表示済み
public bool IsAuthorityFolderRevealed { get; set; }        // 権限フォルダ表示
public bool AuthorityHint1Shown { get; set; }              // 権限ヒント1表示済み
public bool AuthorityHint2Shown { get; set; }              // 権限ヒント2表示済み
public bool AuthorityHint3Shown { get; set; }              // 権限ヒント3表示済み
public bool IsGameWindowMoved { get; set; }                // ゲームウィンドウ移動済み
```

**主要メソッド**:
- `IncrementActionCount(string actionName)`: アクション回数カウント
- `GetActionCount(string actionName)`: 実行回数取得
- `MarkEventCompleted(string eventId)`: イベント完了マーク
- `IsEventCompleted(string eventId)`: 完了確認
- `SetVariable(string key, object value)`: 変数設定
- `GetVariable<T>(string key)`: 型安全な変数取得

---

#### PuzzleDefinition.cs
**責務**: 個別パズルの定義（条件・トリガー・アクション）

**主要プロパティ**:
```csharp
public string Id { get; set; }                              // パズル識別子
public string Description { get; set; }                     // 説明
public PuzzleTrigger Trigger { get; set; }                  // 発動トリガー
public List<PuzzleAction> Actions { get; set; }             // 実行アクション
public List<PuzzleCondition> Conditions { get; set; }       // 実行条件
public bool CanRepeat { get; set; }                         // 繰り返し可能
public int Priority { get; set; }                           // 優先度
public bool IsCompleted { get; set; }                       // 完了フラグ
```

パズルは`PuzzleService.LoadPuzzles()`で100以上定義されており、ゲームの全ロジックを制御します。

---

#### PuzzleCondition.cs
**責務**: パズル実行の複雑な条件判定

**ConditionType列挙型**:
- `ActionCount`: 特定アクションの実行回数判定
- `EventCompleted`: イベント完了状態判定
- `ComponentExists`: コンポーネント存在判定
- `Variable`: カスタム変数値判定
- `Always`: 常に真（条件なし）

**ComparisonOperator列挙型**:
- `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`

**主要メソッド**:
```csharp
public bool IsSatisfied(GameState gameState, ComponentManager componentManager)
{
    // Type別の判定ロジック
    switch (Type)
    {
        case ConditionType.ActionCount:
            return CompareValues(gameState.GetActionCount(Key), ExpectedValue, Operator);
        case ConditionType.EventCompleted:
            return gameState.IsEventCompleted(Key);
        case ConditionType.ComponentExists:
            return componentManager.ComponentExists(Key);
        case ConditionType.Variable:
            return CompareValues(gameState.GetVariable<object>(Key), ExpectedValue, Operator);
        case ConditionType.Always:
            return true;
    }
}
```

---

#### PuzzleAction.cs
**責務**: パズル解決時の具体的な処理内容定義

**ActionType列挙型（45種類）**:

**基本UI操作**:
- `ShowDialog`: ダイアログメッセージ表示
- `ChangeMainButtonContent`: メインボタンテキスト変更
- `RevealHiddenItem`: 隠しアイテム表示
- `TransitionToPhase2`: フェーズ2移行
- `ShowMessageBox`: メッセージボックス表示
- `ShowChoice`: 選択肢ダイアログ表示

**可視性制御**:
- `SetMainButtonVisibility`: メインボタン表示/非表示
- `SetKeyVisibility`: キー表示/非表示
- `SetDoorVisibility`: ドア表示/非表示
- `SetTextWindowVisibility`: テキストウィンドウ表示/非表示
- `SetBackgroundVisibility`: 背景画像表示/非表示

**位置制御**:
- `SetButtonPosition`: ボタン位置変更
- `SetYESPosition`: YES表示位置変更
- `SetKeyPosition`: キー位置変更
- `SetDoorPosition`: ドア位置変更
- `SetBackgroundPosition`: 背景位置変更

**システム制御**:
- `TerminateExplorer`: エクスプローラー終了
- `DisableKeyboardInput` / `EnableKeyboardInput`: キーボード制御
- `DisableMouseInput` / `EnableMouseInput`: マウス制御
- `StartExplorer`: エクスプローラー起動

**ゲーム制御**:
- `ResetGame`: ゲームリセット
- `ClearMessageQueue`: メッセージキュークリア
- `ExitApplication`: アプリケーション終了
- `ExitWithMessageBox` / `DelayedExitWithMessageBox`: メッセージ付き終了
- `BetrayalEnding`: 裏切りエンディング

**疑似デスクトップアイコン制御**:
- `ShowPseudoDesktopIcon`: 疑似アイコン表示
- `HidePseudoDesktopIcon`: 疑似アイコン非表示
- `UpdatePseudoIconPosition`: 疑似アイコン位置更新

**その他**:
- `CreateHiddenAuthorityFolder`: 隠し権限フォルダ作成
- `RecreateComponentWithPosition`: 位置情報付きコンポーネント再作成
- `StartButtonHint` / `CancelButtonHint`: ボタンヒント制御
- `StartAuthorityHints` / `CancelAuthorityHints`: 権限ヒント制御

**主要プロパティ**:
```csharp
public ActionType Type { get; set; }                    // アクションタイプ
public string? Message { get; set; }                    // 表示メッセージ
public string? NewContent { get; set; }                 // 新しいコンテンツ
public string? TargetComponent { get; set; }            // 対象コンポーネント
public string? HiddenItemFolder { get; set; }           // 隠しアイテムフォルダ
public string? IconName { get; set; }                   // アイコン名
public double IconX { get; set; }                       // アイコンX座標
public double IconY { get; set; }                       // アイコンY座標
public List<PuzzleAction>? YesActions { get; set; }     // Yes選択時アクション
public List<PuzzleAction>? NoActions { get; set; }      // No選択時アクション
public bool IsVisible { get; set; }                     // 可視性フラグ
public int DelayMilliseconds { get; set; }              // 遅延時間
```

---

#### PuzzleTrigger.cs
**責務**: ファイルシステムイベントとパズル実行の紐付け

**TriggerType列挙型**:
- `Created`: ファイル作成時
- `Deleted`: ファイル削除時
- `Renamed`: ファイル名変更時
- `Exists`: ファイル存在時
- `AuthorityFolderOpened`: 権限フォルダ開封時
- `PositionChanged`: 位置変更時

**主要プロパティ**:
```csharp
public TriggerType Type { get; set; }                   // トリガータイプ
public string? ComponentName { get; set; }              // 対象コンポーネント名
public string[]? ComponentNames { get; set; }           // 複数コンポーネント名
public string? OldComponentName { get; set; }           // 変更前コンポーネント名
public double? MinX { get; set; }                       // 最小X座標
public double? MaxX { get; set; }                       // 最大X座標
public double? MinY { get; set; }                       // 最小Y座標
public double? MaxY { get; set; }                       // 最大Y座標
```

**主要メソッド**:
- `MatchesComponentName(string name)`: コンポーネント名マッチング判定
- `MatchesPositionCondition(double x, double y)`: 位置条件判定

---

#### Dialogue関連エンティティ

**DialogueFlow.cs** - 対話フロー全体の管理
```csharp
public string Id { get; set; }                              // フロー識別子
public string Description { get; set; }                     // 説明
public DialogueTrigger Trigger { get; set; }                // 発動トリガー
public Dictionary<string, DialogueNode> Nodes { get; set; } // ノードマップ
public string StartNodeId { get; set; }                     // 開始ノードID
public List<PuzzleCondition> Conditions { get; set; }       // 実行条件
public bool CanRepeat { get; set; }                         // 繰り返し可能
public int Priority { get; set; }                           // 優先度
public bool IsCompleted { get; set; }                       // 完了フラグ
```

**DialogueNode.cs** - 対話の個別ノード
```csharp
public string Id { get; set; }                              // ノードID
public string Text { get; set; }                            // 表示テキスト
public List<DialogueChoice> Choices { get; set; }           // 選択肢リスト
public string? NextNodeId { get; set; }                     // 次ノードID
public List<PuzzleCondition> Conditions { get; set; }       // 実行条件
public List<PuzzleAction> Actions { get; set; }             // 実行アクション
public bool CanRepeat { get; set; }                         // 繰り返し可能
public bool IsCompleted { get; set; }                       // 完了フラグ
```

**DialogueChoice.cs** - 対話選択肢
```csharp
public string Text { get; set; }                            // 選択肢テキスト
public string NextNodeId { get; set; }                      // 次ノードID
public List<PuzzleAction> Actions { get; set; }             // 実行アクション
public List<PuzzleCondition> Conditions { get; set; }       // 表示条件
public ChoiceType Type { get; set; }                        // Yes/No/Custom
```

**DialogueTrigger.cs** - 対話フロートリガー
```csharp
public TriggerType Type { get; set; }                       // Created/Deleted/Exists/Renamed
public string[]? ComponentNames { get; set; }               // 対象コンポーネント名
public string? OldComponentName { get; set; }               // 変更前コンポーネント名
public string? ComponentName { get; set; }                  // 変更後コンポーネント名
```

---

#### LetterTriggerCondition.cs
**責務**: 自動手紙配信システムの条件設定

**LetterTriggerType列挙型（16種類）**:
- `TimeOnly`: 時間経過のみ
- `FileExistenceOnly`: ファイル存在のみ
- `FileNotExistenceOnly`: ファイル不存在のみ
- `AllFilesExist`: すべてのファイル存在
- `AnyFileExists`: いずれかのファイル存在
- `TimeAndFileExistence`: 時間とファイル存在
- `TimeAndFileNotExistence`: 時間とファイル不存在
- `TimeAndAllFilesExist`: 時間とすべてのファイル存在
- `TimeAndAnyFileExists`: 時間といずれかのファイル存在
- その他の複合条件...

**主要プロパティ**:
```csharp
public LetterTriggerType TriggerType { get; set; }          // トリガータイプ
public double TimeIntervalSeconds { get; set; }             // 時間間隔
public string? RequiredFileName { get; set; }               // 必要ファイル名
public string[]? RequiredFileNames { get; set; }            // 必要ファイル名配列
public int LetterIndex { get; set; }                        // 手紙インデックス
```

**ファクトリーメソッド**: 条件の簡潔な生成を提供
- `CreateTimeOnly(double seconds, int letterIndex)`
- `CreateFileExistenceOnly(string fileName, int letterIndex)`
- `CreateTimeAndFileExistence(double seconds, string fileName, int letterIndex)`
- など...

### Events/

すべてイミュータブルなイベントクラスで、ドメインイベント駆動アーキテクチャを実現：

**ComponentCreatedEvent**: ファイル作成イベント
```csharp
public GameComponent Component { get; }
```

**ComponentDeletedEvent**: ファイル削除イベント
```csharp
public GameComponent Component { get; }
```

**ComponentRenamedEvent**: ファイル名変更イベント
```csharp
public GameComponent Component { get; }
public string OldName { get; }
public string NewName { get; }
```

**HiddenItemRevealedEvent**: 隠しアイテム表示イベント（パラメータなし）

**Phase2StartedEvent**: フェーズ2開始イベント（パラメータなし）

**AuthorityFolderOpenedEvent**: 権限フォルダ開封イベント（パラメータなし）

**BackgroundPositionChangedEvent**: 背景位置変更イベント
```csharp
public double X { get; }
public double Y { get; }
```

**CommandPromptBattleStartedEvent**: コマンドプロンプトバトル開始イベント

**DisableMouseInputEvent**: マウス入力無効化イベント

**GameWindowPositionChangedEvent**: ゲームウィンドウ位置変更イベント
```csharp
public double X { get; }
public double Y { get; }
```

**KeyClickedEvent**: キークリックイベント

**LetterAppearedEvent**: 手紙出現イベント
```csharp
public int LetterIndex { get; }
```

**LetterClickedEvent**: 手紙クリックイベント
```csharp
public int LetterIndex { get; }
```

**MouseComponentDeletionEvent**: マウスコンポーネント削除イベント

**SystemTakeoverCompletedEvent**: システム乗っ取り完了イベント

### Interfaces/

**IEventDispatcher** - イベントディスパッチャー抽象化
```csharp
void Dispatch<TEvent>(TEvent @event) where TEvent : class;
void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
```
**責務**: 型安全なイベント発行・購読、疎結合なコンポーネント間通信

**IPuzzleActionHandler** - パズルアクションハンドラー抽象化
```csharp
void HandleAction(PuzzleAction action);
```
**責務**: パズルアクション実行の抽象化、依存性逆転原則の実現

**IDialogueService** - 対話サービス抽象化
```csharp
bool IsInDialogue { get; }
DialogueNode? CurrentNode { get; }
void ExecuteDialogue(DialogueFlow flow);
void NextNode(string? nextNodeId);
void MakeChoice(DialogueChoice choice);
void ChooseYes();
void ChooseNo();
void ResetDialogue();
void CheckDialogueTriggersOnComponentCreated(GameComponent component);
void CheckDialogueTriggersOnComponentDeleted(GameComponent component);
void CheckDialogueTriggersOnComponentRenamed(GameComponent component, string oldName);
```
**責務**: 対話フロー管理、ノード遷移制御、イベント連携の抽象化

**IGameContext** - ゲームコンテキスト抽象化
```csharp
GameState GameState { get; }
ComponentManager ComponentManager { get; }
IPuzzleActionHandler ActionHandler { get; }
IEventDispatcher EventDispatcher { get; }
MetaGameController MetaGameController { get; }
```
**責務**: ゲーム実行時の主要サービスへの統一アクセス、依存性注入の簡素化

### Builders/Factories/

**DialogueFlowBuilder.cs** - Fluent APIビルダーパターン実装

対話フローの宣言的な構築を可能にするビルダー：

```csharp
var flow = DialogueFlowBuilder.Create("flow-id", "説明")
    .TriggeredBy(new DialogueTrigger { Type = TriggerType.Created, ComponentNames = new[] { "File1" } })
    .When(ConditionFactory.FirstTime("flow-id"))
    .CanRepeat(false)
    .WithPriority(100)
    .StartWith("node-1")
    .AddNode(builder => builder
        .WithId("node-1")
        .ShowText("こんにちは")
        .WithYesChoice(yesBuilder => yesBuilder
            .GoTo("node-2")
            .Do(new PuzzleAction { Type = ActionType.ShowDialog, Message = "ありがとう" })
        )
        .WithNoChoice(noBuilder => noBuilder
            .GoTo(null) // フロー終了
        )
    )
    .Build();
```

**ConditionFactory.cs** - 条件生成ファクトリーパターン実装

パズル条件の簡潔な生成を提供：

```csharp
// アクション回数条件
ConditionFactory.ActionCount("action-name", 5, ComparisonOperator.GreaterThan)

// 初回実行条件
ConditionFactory.FirstTime("event-id")

// コンポーネント存在条件
ConditionFactory.ComponentExists("component-name")

// 変数条件
ConditionFactory.Variable("var-name", expectedValue, ComparisonOperator.Equal)

// フラグ条件
ConditionFactory.FlagTrue("flag-name")
ConditionFactory.FlagFalse("flag-name")

// イベント完了条件
ConditionFactory.EventCompleted("event-id")

// すべての権限存在確認
ConditionFactory.AllPermissionsExist()
```

## Application層クラス詳細仕様

### PuzzleService.cs
**ファイルパス**: `SELLCT/Application/Services/PuzzleService.cs:32-607`

**責務**: パズルシステムの中核サービス、ゲームメインロジックの実行

**依存関係**:
```csharp
private readonly ComponentManager _componentManager;
private readonly IPuzzleActionHandler _actionHandler;
private readonly IEventDispatcher _eventDispatcher;
private readonly GameState _gameState;
```

**主要フィールド**:
```csharp
private List<PuzzleDefinition> _puzzles = new();
```

**主要メソッド**:

**LoadPuzzles()** - 100以上のパズル定義を読み込み
- 初回コンポーネント作成時のウェルカムメッセージ
- UI/Text/Visual/Systemコンポーネント作成時の反応
- 権限ファイル作成時の段階的な権限取得
- フェーズ2トリガー条件
- 裏切りエンディング条件
- ヒントシステムトリガー
- 背景位置変更パズル

**イベントハンドラー**:
- `CheckPuzzlesOnComponentCreated(ComponentCreatedEvent e)` - ファイル作成時のパズルチェック
- `CheckPuzzlesOnComponentDeleted(ComponentDeletedEvent e)` - ファイル削除時のパズルチェック
- `CheckPuzzlesOnComponentRenamed(ComponentRenamedEvent e)` - ファイル名変更時のパズルチェック
- `CheckPuzzlesOnAuthorityFolderOpened(AuthorityFolderOpenedEvent e)` - 権限フォルダ開封時のチェック
- `CheckPuzzlesOnBackgroundPositionChanged(BackgroundPositionChangedEvent e)` - 背景位置変更時のチェック

**OnSELLCTFolderBetrayed()** - SELLCTフォルダ削除時の裏切りエンディング処理

**イベント購読**: コンストラクタで各種イベントを購読し、イベント駆動でパズルを実行

### DialogueService.cs
**ファイルパス**: `SELLCT/Application/Services/DialogueService.cs`

**責務**: 対話システムの管理、対話フロー実行とノード遷移制御

**依存関係**:
```csharp
private readonly ComponentManager _componentManager;
private readonly IPuzzleActionHandler _actionHandler;
private readonly IEventDispatcher _eventDispatcher;
private readonly GameState _gameState;
```

**主要フィールド**:
```csharp
private List<DialogueFlow> _dialogueFlows = new();
private DialogueFlow? _currentFlow;
private DialogueNode? _currentNode;
public bool IsInDialogue => _currentFlow != null;
public DialogueNode? CurrentNode => _currentNode;
```

**主要メソッド**:

**LoadDialogueFlows()** - 対話フロー定義を読み込み

**ExecuteDialogue(DialogueFlow flow)** - 対話フロー開始
```csharp
public void ExecuteDialogue(DialogueFlow flow)
{
    _currentFlow = flow;
    var startNode = flow.GetNode(flow.StartNodeId);
    if (startNode != null)
    {
        ExecuteNode(startNode);
    }
}
```

**NextNode(string? nextNodeId)** - 次ノードへ遷移

**MakeChoice(DialogueChoice choice)** - 選択肢の選択処理

**ChooseYes() / ChooseNo()** - Yes/No選択のショートカット

**ExecuteNode(DialogueNode node)** - ノード実行
- ノード条件判定
- テキスト表示
- アクション実行
- 選択肢表示

**FinishDialogue()** - 対話フロー終了処理

**イベント連携メソッド**:
- `CheckDialogueTriggersOnComponentCreated(GameComponent component)`
- `CheckDialogueTriggersOnComponentDeleted(GameComponent component)`
- `CheckDialogueTriggersOnComponentRenamed(GameComponent component, string oldName)`

## Infrastructure層クラス詳細仕様

### ComponentManager.cs
**ファイルパス**: `SELLCT/Infrastructure/Services/ComponentManager.cs:16-866`

**責務**: ファイルシステム監視とコンポーネントライフサイクル管理の中核サービス

**主要フィールド**:
```csharp
private Dictionary<string, GameComponent> _components = new();
private Dictionary<string, string> _componentPaths = new();           // 名前→パスマップ
private Dictionary<string, string> _lastKnownPositions = new();       // 削除前位置記録
private string _componentsPath;                                        // デスクトップのcomponentsパス
private FileSystemWatcherManager _watcherManager;
```

**イベント**:
```csharp
public event EventHandler<GameComponent>? ComponentChanged;
public event EventHandler? ComponentsFolderChanged;
public event EventHandler? SELLCTFolderBetrayed;
```

**主要メソッド**:

**CreateComponentsFolder()** - componentsフォルダ構造作成
```
components/
├── UI/
├── Text/
├── Visual/
├── System/ (Hidden属性)
└── SELLCT/ (Hidden属性)
```

**CreateInitialComponents()** - 初期コンポーネント生成
- GameWindow (UI)
- Message (Text)
- Folder (Visual)
- 他の初期ファイル

**RecordComponentPosition(string componentName)** - コンポーネント位置情報記録
- ファイル内容から位置情報を抽出し、`_lastKnownPositions`に保存

**CreateComponentFile(string name, ComponentType type, ...)** - コンポーネントファイル作成
- ファイルシステムにファイル作成
- GameComponentオブジェクト生成
- `_components`と`_componentPaths`に登録

**CreateHiddenFile(string folderName, string fileName, string content)** - 隠しファイル作成
- Hidden属性付きファイル作成
- SELLCT権限フォルダ内のファイル作成に使用

**FileSystemWatcherイベントハンドラー**:
- `OnFileCreated()` - ファイル作成検知 → ComponentCreatedEvent発行
- `OnFileChanged()` - ファイル変更検知 → 位置情報更新
- `OnFileDeleted()` - ファイル削除検知 → ComponentDeletedEvent発行
- `OnFileRenamed()` - ファイル名変更検知 → ComponentRenamedEvent発行

**RevealHiddenItem(string folderName)** - 隠しアイテムの表示
- Hidden属性を削除してファイルを可視化

**RecreateComponent(string name, string? lastKnownPosition)** - コンポーネント再作成
- 削除されたコンポーネントを位置情報付きで再作成

**ResetToInitialState()** - ゲームリセット
- componentsフォルダを削除して初期状態に戻す

### EventDispatcher.cs
**ファイルパス**: `SELLCT/Infrastructure/Services/EventDispatcher.cs`

**責務**: インメモリイベント管理、型安全なイベント配信

**実装**: Observerパターンの実装

**主要フィールド**:
```csharp
private readonly Dictionary<Type, List<object>> _handlers = new();
```

**主要メソッド**:

**Subscribe<TEvent>(Action<TEvent> handler)** - イベント購読
```csharp
public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
{
    var eventType = typeof(TEvent);
    if (!_handlers.ContainsKey(eventType))
    {
        _handlers[eventType] = new List<object>();
    }
    _handlers[eventType].Add(handler);
}
```

**Dispatch<TEvent>(TEvent @event)** - イベント発行
```csharp
public void Dispatch<TEvent>(TEvent @event) where TEvent : class
{
    var eventType = typeof(TEvent);
    if (_handlers.TryGetValue(eventType, out var handlers))
    {
        foreach (var handler in handlers.Cast<Action<TEvent>>())
        {
            handler(@event);
        }
    }
}
```

### MetaGameController.cs
**ファイルパス**: `SELLCT/Infrastructure/Services/MetaGameController.cs`

**責務**: フェーズ2のシステム制御、エクスプローラー操作、入力制御

**主要フィールド**:
```csharp
private List<Process> _commandPrompts = new();
private bool _phase2Active = false;
private Phase2SequenceManager? _sequenceManager;
```

**主要メソッド**:

**StartPhase2()** - フェーズ2シーケンス開始
```csharp
public async Task StartPhase2()
{
    _phase2Active = true;
    _sequenceManager = new Phase2SequenceManager(_eventDispatcher);
    await _sequenceManager.ExecuteSequence();
}
```

**StartCommandPromptSequence()** - コマンドプロンプト連続起動演出
```csharp
public async Task StartCommandPromptSequence()
{
    for (int i = 0; i < 10; i++)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/k echo System takeover in progress...",
            UseShellExecute = true
        });
        _commandPrompts.Add(process);
        await Task.Delay(500);
    }
}
```

**TerminateExplorerProcess()** - エクスプローラー終了
```csharp
public void TerminateExplorerProcess()
{
    var explorerProcesses = Process.GetProcessesByName("explorer");
    foreach (var process in explorerProcesses)
    {
        process.Kill();
    }
}
```

**StartExplorerProcess()** - エクスプローラー起動
```csharp
public void StartExplorerProcess()
{
    Process.Start("explorer.exe");
}
```

**DisableUserInput()** - ユーザー入力無効化
- キーボードフックとマウスフックを使用してシステムレベルで入力をブロック

**RestoreUserInput()** - ユーザー入力復元
- フックを解除して入力を再度有効化

### その他のInfrastructureサービス

**FileSystemWatcherManager.cs**
- FileSystemWatcherのラッパー
- ファイルイベントの検知と配信
- デバウンス処理（連続イベントの抑制）

**LetterService.cs**
- 自動手紙配信システム
- LetterTriggerConditionに基づく条件判定
- タイマーベースの定期チェック
- LetterAppearedEvent発行

**KeyService.cs**
- キーボード入力監視
- 特定キー組み合わせ検出（例: Ctrl+Alt+K）
- KeyClickedEvent発行

**PseudoDesktopIconManager.cs**
- 疑似デスクトップアイコン管理
- WPFウィンドウでデスクトップアイコンを模倣
- アイコンクリック時の動作制御

**HiddenFileSettingService.cs**
- Windowsレジストリ経由の隠しファイル表示設定制御
- `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced`
- `Hidden`キー操作

**CleanupService.cs**
- アプリケーション終了時のクリーンアップ
- レジストリエントリ削除
- ファイルシステム復元
- プロセス終了処理

**Phase2SequenceManager.cs**
- フェーズ2の演出シーケンス管理
- タイムライン制御
- イベント発行タイミング制御

**DownloadTracker.cs**
- ダウンロードフォルダ監視
- 特定ファイルのダウンロード検知

**WarningFloodService.cs**
- 警告ダイアログの連続表示
- フェーズ2演出の一部

**ScreenCaptureService.cs**
- スクリーンキャプチャ機能
- スクリーンショット保存

**SystemIconService.cs**
- システムアイコン操作
- デスクトップアイコン配置制御

**SmartScreenWarningService.cs**
- SmartScreen警告画面の模倣表示
- セキュリティ警告演出

## Presentation層クラス詳細仕様

### MainWindow.xaml.cs
**ファイルパス**: `SELLCT/Views/MainWindow.xaml.cs`

**責務**: メインウィンドウのUI管理、ユーザー操作処理、各種サービス統合

**主要フィールド**:
```csharp
private ComponentManager _componentManager;
private LetterService _letterService;
private KeyService _keyService;
private MetaGameController _metaGameController;
private LetterDisplayController _letterDisplayController;
private DialogController _dialogController;
private bool _isPhase2 = false;
private bool _isNoFunctionEnabled = false;
```

**主要メソッド**:

**InitializeAsync()** - 非同期初期化
```csharp
private async Task InitializeAsync()
{
    // EventDispatcher初期化
    // ComponentManager初期化
    // PuzzleService, DialogueService初期化
    // PuzzleActionHandler設定
    // LetterService, KeyService開始
    // UI要素の初期設定
}
```

**ウィンドウイベントハンドラー**:
- `Window_Loaded()` - ウィンドウ読み込み時の初期化
- `Window_Activated()` - ウィンドウアクティブ化時の処理
- `Window_Deactivated()` - ウィンドウ非アクティブ化時の処理
- `Window_Closing()` - ウィンドウクローズ時のクリーンアップ

**UI制御メソッド**:
- `ShowDialogMessage(string message)` - ダイアログメッセージ表示
- `SetMainButtonVisibility(bool isVisible)` - メインボタン可視性制御
- `DisableMouseInput()` / `EnableMouseInput()` - マウス入力制御
- `CenterWindowOnScreen()` - ウィンドウを画面中央に配置

**イベント購読**:
- ComponentCreatedEvent, ComponentDeletedEvent等のドメインイベント購読
- UIの動的更新

### MainWindowPuzzleActionHandler.cs
**ファイルパス**: `SELLCT/Presentation/Views/MainWindowPuzzleActionHandler.cs`

**責務**: IPuzzleActionHandlerの実装、45種類のアクション実行

**依存関係**:
```csharp
private readonly MainWindow _mainWindow;
private readonly ComponentManager _componentManager;
private readonly MetaGameController _metaGameController;
```

**主要メソッド**:

**HandleAction(PuzzleAction action)** - メインアクション処理
```csharp
public void HandleAction(PuzzleAction action)
{
    switch (action.Type)
    {
        case ActionType.ShowDialog:
            _mainWindow.ShowDialogMessage(action.Message);
            break;
        case ActionType.ChangeMainButtonContent:
            _mainWindow.ChangeButtonContent(action.NewContent);
            break;
        case ActionType.RevealHiddenItem:
            _componentManager.RevealHiddenItem(action.HiddenItemFolder);
            break;
        case ActionType.TransitionToPhase2:
            TransitionToPhase2();
            break;
        // ... 45種類のアクション分岐
    }
}
```

**特殊アクション処理**:
- `HandleShowChoice()` - 選択肢ダイアログ表示と分岐処理
- `HandleDelayedExit()` - 遅延付きアプリケーション終了
- `HandleBetrayalEnding()` - 裏切りエンディング演出
- `HandleShowPseudoDesktopIcon()` - 疑似アイコン表示
- `HandleUpdatePseudoIconPosition()` - 疑似アイコン位置更新

**TransitionToPhase2()** - フェーズ2移行処理
```csharp
private async void TransitionToPhase2()
{
    _mainWindow.IsPhase2 = true;
    await _metaGameController.StartPhase2();
}
```

### Controllers/

**LetterDisplayController.cs**
**責務**: 手紙UIの表示制御とアニメーション管理
- 手紙の出現アニメーション
- 手紙のクリックハンドリング
- 手紙の非表示処理

**DialogController.cs**
**責務**: ダイアログ表示制御とメッセージキュー管理
- メッセージキューの管理
- ダイアログの順次表示
- ヒントシステム制御（ボタンヒント、権限ヒント）
- タイマーベースのヒント表示

## ゲームメカニクス詳細フロー

### パズルシステム実行フロー

```
[1] ファイル操作（作成/削除/名前変更）
         ↓
[2] FileSystemWatcher検知
         ↓
[3] ComponentManager.OnFile(Created|Deleted|Renamed)()
         ↓
[4] GameComponentオブジェクト更新
         ↓
[5] EventDispatcher.Dispatch<ComponentXxxEvent>()
         ↓
[6] PuzzleService.CheckPuzzlesOnComponentXxx()購読
         ↓
[7] 各パズルのトリガー判定
    - PuzzleTrigger.Type照合
    - PuzzleTrigger.ComponentName照合
         ↓
[8] 条件判定（すべてのPuzzleConditionがIsSatisfied()）
    - ActionCount条件
    - EventCompleted条件
    - ComponentExists条件
    - Variable条件
         ↓
[9] アクション実行（各PuzzleAction）
    - IPuzzleActionHandler.HandleAction()呼び出し
    - MainWindowPuzzleActionHandler.HandleAction()
    - switch文で45種類のアクションに分岐
         ↓
[10] UI更新/システム制御/次イベント発行
         ↓
[11] パズル完了マーク（CanRepeat=falseの場合）
```

### 対話システム実行フロー

```
[1] ファイルイベント発生
         ↓
[2] DialogueService.CheckDialogueTriggersOnComponentXxx()
         ↓
[3] DialogueTrigger照合
    - TriggerType照合
    - ComponentNames照合
         ↓
[4] 対話フロー条件判定（PuzzleCondition）
         ↓
[5] DialogueService.ExecuteDialogue(flow)
    - _currentFlow設定
    - StartNodeId取得
         ↓
[6] DialogueService.ExecuteNode(node)
    - ノード条件判定
    - テキスト表示（ShowDialog）
    - ノードアクション実行
         ↓
[7] 選択肢表示待機
    - YES/NO/カスタム選択肢
         ↓
[8] ユーザー選択
         ↓
[9] DialogueService.MakeChoice(choice)
    - 選択肢アクション実行
    - NextNodeId取得
         ↓
[10] 次ノード遷移 or フロー終了
    - NextNodeIdがnull → FinishDialogue()
    - NextNodeIdあり → ExecuteNode(nextNode)
```

### ファイルイベント→UI更新の全体フロー

```
[ファイルシステム層]
Desktop/components/UI/NewFile.txt 作成
         ↓
[FileSystemWatcher]
Changed イベント発火
         ↓
[ComponentManager - Infrastructure層]
OnFileCreated() ハンドラー実行
- GameComponent("NewFile", ComponentType.UI, ...) 作成
- _components辞書に追加
- EventDispatcher.Dispatch(new ComponentCreatedEvent(component))
         ↓
[EventDispatcher - Infrastructure層]
イベント配信 → 全購読者に通知
         ↓
[PuzzleService - Application層]                [DialogueService - Application層]
CheckPuzzlesOnComponentCreated() 実行            CheckDialogueTriggersOnComponentCreated() 実行
- トリガー照合                                   - トリガー照合
- 条件判定                                       - フロー開始判定
- アクション実行                                  - ExecuteDialogue()
         ↓                                              ↓
[MainWindowPuzzleActionHandler - Presentation層]
HandleAction(action) 実行
- ShowDialog → DialogController経由でUI表示
- ChangeMainButtonContent → MainWindow.ChangeButtonContent()
- RevealHiddenItem → ComponentManager.RevealHiddenItem()
- SetXxxVisibility → WPF要素のVisibilityプロパティ変更
         ↓
[MainWindow - Presentation層]
UI要素更新（データバインディング + 直接操作）
- TextBlock更新
- Button更新
- Visibility変更
- アニメーション実行
         ↓
[ユーザーに表示]
画面にメッセージやUI変更が反映
```

### 位置記憶システムの仕組み

```
[1] コンポーネントファイルの位置情報
    - ファイル内容に位置情報を埋め込み
    - 形式: "Position:X=100,Y=200" または "{\"x\":100,\"y\":200}"
         ↓
[2] FileSystemWatcher.Changed イベント
         ↓
[3] ComponentManager.OnFileChanged()
    - RecordComponentPosition(componentName) 呼び出し
         ↓
[4] RecordComponentPosition()
    - ファイル内容読み取り
    - 正規表現で位置情報抽出
    - _lastKnownPositions[componentName] = positionData 保存
         ↓
[5] コンポーネント削除時
    - ComponentDeletedEvent 発行
         ↓
[6] パズルアクション: RecreateComponentWithPosition
    - lastKnownPosition = _lastKnownPositions[componentName]
    - RecreateComponent(componentName, lastKnownPosition)
         ↓
[7] RecreateComponent()
    - 新ファイル作成
    - lastKnownPosition情報をファイル内容に含める
    - デスクトップ上の同じ位置に復元
```

### 手紙配信システムの条件判定ロジック

```
[1] LetterService.Start()
    - タイマー起動（1秒ごとにCheckLetterConditions()実行）
         ↓
[2] CheckLetterConditions()
    - 各LetterTriggerConditionをループ
         ↓
[3] 条件判定（LetterTriggerType別）

    [TimeOnly]
    - 経過時間 >= TimeIntervalSeconds

    [FileExistenceOnly]
    - ComponentManager.ComponentExists(RequiredFileName)

    [FileNotExistenceOnly]
    - !ComponentManager.ComponentExists(RequiredFileName)

    [AllFilesExist]
    - RequiredFileNames.All(name => ComponentManager.ComponentExists(name))

    [AnyFileExists]
    - RequiredFileNames.Any(name => ComponentManager.ComponentExists(name))

    [TimeAndFileExistence]
    - 経過時間 >= TimeIntervalSeconds
    - AND ComponentManager.ComponentExists(RequiredFileName)

    [TimeAndAllFilesExist]
    - 経過時間 >= TimeIntervalSeconds
    - AND RequiredFileNames.All(name => ComponentManager.ComponentExists(name))

    ... その他の複合条件
         ↓
[4] 条件満たした場合
    - EventDispatcher.Dispatch(new LetterAppearedEvent(LetterIndex))
         ↓
[5] LetterDisplayController購読
    - 手紙UIを表示
    - 出現アニメーション実行
```

## 実装された設計パターン

### 1. Clean Architecture（クリーンアーキテクチャ）
**実装箇所**: プロジェクト全体の層構造

**詳細**:
- **Core層**: ドメインロジックとエンティティ、他層に依存しない
- **Application層**: ユースケース実装、Coreに依存、Infrastructureに依存しない（インターフェース経由）
- **Infrastructure層**: 技術実装詳細、CoreとApplicationのインターフェースを実装
- **Presentation層**: UI実装、全層に依存可能

**依存性の方向**: Presentation → Application → Core ← Infrastructure

**メリット**: テスト容易性、フレームワーク独立性、UI変更の容易性

### 2. Event-Driven Architecture（イベント駆動アーキテクチャ）
**実装箇所**: EventDispatcher, 全ドメインイベントクラス

**詳細**:
```csharp
// イベント定義（Core層）
public class ComponentCreatedEvent
{
    public GameComponent Component { get; }
}

// イベント発行（Infrastructure層）
_eventDispatcher.Dispatch(new ComponentCreatedEvent(component));

// イベント購読（Application層）
_eventDispatcher.Subscribe<ComponentCreatedEvent>(e =>
{
    CheckPuzzlesOnComponentCreated(e);
});
```

**メリット**: 疎結合、拡張性、非同期処理対応

### 3. Repository Pattern（リポジトリパターン）
**実装箇所**: ComponentManager

**詳細**:
- ComponentManagerがファイルシステムをデータストアとして抽象化
- GameComponentオブジェクトのCRUD操作を提供
- `_components`辞書でインメモリキャッシュ
- FileSystemWatcherでデータストアの変更を監視

**メリット**: データアクセスロジックの一元化、テスト容易性

### 4. Factory Pattern（ファクトリーパターン）
**実装箇所**: ConditionFactory, GameComponent.Create()

**詳細**:
```csharp
// ConditionFactory
var condition = ConditionFactory.ActionCount("delete", 3, ComparisonOperator.GreaterThan);

// GameComponent.Create()
var component = GameComponent.Create("NewComponent", ComponentType.UI, filePath);
```

**メリット**: オブジェクト生成ロジックの一元化、可読性向上

### 5. Builder Pattern（ビルダーパターン）
**実装箇所**: DialogueFlowBuilder

**詳細**:
```csharp
var flow = DialogueFlowBuilder.Create("flow-1", "説明")
    .TriggeredBy(trigger)
    .When(condition)
    .StartWith("node-1")
    .AddNode(builder => builder
        .WithId("node-1")
        .ShowText("テキスト")
        .WithYesChoice(yesBuilder => yesBuilder.GoTo("node-2"))
    )
    .Build();
```

**メリット**: 複雑なオブジェクトの宣言的な構築、Fluent API

### 6. Strategy Pattern（ストラテジーパターン）
**実装箇所**: PuzzleCondition, PuzzleAction

**詳細**:
- PuzzleConditionのConditionTypeで判定戦略を動的に切り替え
- PuzzleActionのActionTypeで実行戦略を動的に切り替え
- switch文で各戦略の実装を分岐

**メリット**: アルゴリズムの切り替え、拡張性

### 7. Observer Pattern（オブザーバーパターン）
**実装箇所**: IEventDispatcher, FileSystemWatcher

**詳細**:
```csharp
// EventDispatcher内部実装
private Dictionary<Type, List<object>> _handlers = new();

public void Subscribe<TEvent>(Action<TEvent> handler)
{
    _handlers[typeof(TEvent)].Add(handler);
}

public void Dispatch<TEvent>(TEvent @event)
{
    foreach (var handler in _handlers[typeof(TEvent)])
    {
        handler(@event);
    }
}
```

**メリット**: 1対多の依存関係、疎結合な通知

### 8. MVVM Pattern（Model-View-ViewModel）
**実装箇所**: WPF UI層

**詳細**:
- **Model**: Core層のGameComponent, GameState
- **View**: MainWindow.xaml, 他XAMLファイル
- **ViewModel**: MainWindow.xaml.cs（コードビハインド）
- **INotifyPropertyChanged**: GameComponentでデータバインディング実装

**メリット**: UIロジック分離、テスタビリティ、データバインディング

### 9. Singleton Pattern（シングルトンパターン）
**実装箇所**: EventDispatcher（Appレベルで1インスタンス）

**詳細**:
```csharp
// App.xaml.cs
public partial class App : Application
{
    public static EventDispatcher EventDispatcher { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        EventDispatcher = new EventDispatcher();
    }
}
```

**メリット**: グローバルアクセスポイント、インスタンス一元管理

## 特徴的な実装の解説

### 1. 疑似デスクトップアイコンシステム

**実装**: PseudoDesktopIconManager

**仕組み**:
```csharp
// WPFウィンドウでデスクトップアイコンを模倣
var iconWindow = new Window
{
    WindowStyle = WindowStyle.None,          // 枠なし
    AllowsTransparency = true,               // 透明許可
    Background = Brushes.Transparent,        // 背景透明
    Topmost = false,                         // 最前面ではない
    ShowInTaskbar = false,                   // タスクバー非表示
    Left = iconX,                            // デスクトップ座標
    Top = iconY,
    Width = 80,
    Height = 80
};

// アイコン画像と名前を配置
var stackPanel = new StackPanel
{
    Children =
    {
        new Image { Source = iconImage },
        new TextBlock { Text = iconName }
    }
};
```

**用途**: Authorityフォルダを背景画像移動時に疑似アイコンとして表示し、クリック可能にする

### 2. ヒントシステムの実装

**ボタンヒントシステム** (DialogController):
```csharp
public void StartButtonHint()
{
    _buttonHintTimer = new System.Windows.Threading.DispatcherTimer
    {
        Interval = TimeSpan.FromSeconds(30)  // 30秒後
    };
    _buttonHintTimer.Tick += (s, e) =>
    {
        if (!_gameState.ButtonHintShown)
        {
            ShowDialogMessage("ヒント: メインボタンをクリックしてみてください");
            _gameState.ButtonHintShown = true;
        }
        _buttonHintTimer.Stop();
    };
    _buttonHintTimer.Start();
}
```

**権限ヒントシステム**:
```csharp
public void StartAuthorityHints()
{
    // ヒント1: 60秒後
    _authorityHintTimer1 = CreateHintTimer(60, "ヒント1: SELLCTフォルダを探してみてください");

    // ヒント2: 120秒後
    _authorityHintTimer2 = CreateHintTimer(120, "ヒント2: 隠しファイル表示設定を確認してください");

    // ヒント3: 180秒後
    _authorityHintTimer3 = CreateHintTimer(180, "ヒント3: components内のSELLCTフォルダです");
}
```

### 3. 裏切りエンディング分岐

**実装**: PuzzleService.OnSELLCTFolderBetrayed()

**条件**: SELLCTフォルダ（権限フォルダ）が削除された場合

**動作**:
```csharp
private void OnSELLCTFolderBetrayed()
{
    // すべての進行中のパズルを無効化
    foreach (var puzzle in _puzzles)
    {
        puzzle.IsCompleted = true;
    }

    // 裏切りエンディングアクション実行
    var betrayalAction = new PuzzleAction
    {
        Type = ActionType.BetrayalEnding,
        Message = "あなたは私を裏切りました。もう二度と信頼を得ることはできません。"
    };
    _actionHandler.HandleAction(betrayalAction);

    // ゲーム終了
    var exitAction = new PuzzleAction
    {
        Type = ActionType.DelayedExitWithMessageBox,
        Message = "ゲームオーバー",
        DelayMilliseconds = 3000
    };
    _actionHandler.HandleAction(exitAction);
}
```

**教育的意義**: プレイヤーの選択の重要性を示し、信頼関係の脆弱性を体験させる

### 4. 安全機構とクリーンアップ処理

**CleanupService.cs**:
```csharp
public class CleanupService
{
    public void Cleanup()
    {
        // 1. レジストリエントリ削除
        try
        {
            var runKey = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true);
            runKey?.DeleteValue("SELLCT", false);
        }
        catch { /* 安全に無視 */ }

        // 2. 隠しファイル設定復元
        try
        {
            var explorerKey = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true);
            explorerKey?.SetValue("Hidden", 1);  // 元の設定に戻す
        }
        catch { /* 安全に無視 */ }

        // 3. プロセス終了
        foreach (var cmdProcess in _commandPrompts)
        {
            try { cmdProcess.Kill(); } catch { }
        }

        // 4. エクスプローラー復元
        if (_explorerWasTerminated)
        {
            Process.Start("explorer.exe");
        }

        // 5. componentsフォルダ削除（オプション）
        // ユーザーの選択に応じて実行
    }
}
```

**MainWindow.Window_Closing()での呼び出し**:
```csharp
private void Window_Closing(object sender, CancelEventArgs e)
{
    _cleanupService.Cleanup();
}
```

### 5. 位置情報の永続化

**ファイル内容への位置情報埋め込み**:
```csharp
public void CreateComponentFile(string name, ComponentType type, string? position = null)
{
    string content = $"Component: {name}\nType: {type}\n";

    if (!string.IsNullOrEmpty(position))
    {
        content += $"Position:{position}\n";
    }

    File.WriteAllText(filePath, content);
}
```

**位置情報の抽出**:
```csharp
public void RecordComponentPosition(string componentName)
{
    var content = File.ReadAllText(filePath);

    // 正規表現で位置情報抽出
    var match = Regex.Match(content, @"Position:X=(\d+\.?\d*),Y=(\d+\.?\d*)");
    if (match.Success)
    {
        var positionData = $"X={match.Groups[1].Value},Y={match.Groups[2].Value}";
        _lastKnownPositions[componentName] = positionData;
    }

    // JSON形式もサポート
    var jsonMatch = Regex.Match(content, @"\{""x"":(\d+\.?\d*),""y"":(\d+\.?\d*)\}");
    if (jsonMatch.Success)
    {
        var positionData = $"X={jsonMatch.Groups[1].Value},Y={jsonMatch.Groups[2].Value}";
        _lastKnownPositions[componentName] = positionData;
    }
}
```

### 6. フェーズ2演出シーケンス

**Phase2SequenceManager.cs**:
```csharp
public async Task ExecuteSequence()
{
    // タイムライン制御

    // 0秒: フェーズ2開始メッセージ
    ShowMessage("フェーズ2開始");

    // 2秒: エクスプローラー終了警告
    await Task.Delay(2000);
    ShowMessage("システム制御を開始します");

    // 5秒: エクスプローラー終了
    await Task.Delay(3000);
    _metaGameController.TerminateExplorerProcess();
    _eventDispatcher.Dispatch(new Phase2StartedEvent());

    // 7秒: コマンドプロンプトバトル開始
    await Task.Delay(2000);
    await _metaGameController.StartCommandPromptSequence();
    _eventDispatcher.Dispatch(new CommandPromptBattleStartedEvent());

    // 15秒: 入力無効化
    await Task.Delay(8000);
    _metaGameController.DisableUserInput();
    _eventDispatcher.Dispatch(new DisableMouseInputEvent());

    // 20秒: 警告フラッド
    await Task.Delay(5000);
    _warningFloodService.Start();

    // 30秒: システム乗っ取り完了
    await Task.Delay(10000);
    _eventDispatcher.Dispatch(new SystemTakeoverCompletedEvent());
    ShowMessage("システムの制御を完全に掌握しました");

    // 35秒: エンディング
    await Task.Delay(5000);
    ShowEndingMessage();
}
```

## プロジェクトの教育的価値

このプロジェクトは、以下の点で優れた教育的価値を提供します：

1. **実践的なセキュリティ意識の向上**: マルウェアの動作パターンを安全な環境で体験
2. **Clean Architectureの学習**: 実際の中規模プロジェクトでの実装例
3. **イベント駆動アーキテクチャの理解**: 複雑なイベントフローの実装パターン
4. **WPF/C#の高度な技術**: ファイルシステム監視、レジストリ操作、プロセス制御
5. **ゲームデザイン**: パズルシステム、対話システムの設計と実装
6. **倫理的な考察**: ソフトウェアの信頼性と責任についての議論の出発点

**重要**: このプロジェクトは**防御的セキュリティ教育専用**であり、すべての操作に復旧機構が含まれています。教育目的以外での使用は想定されていません。