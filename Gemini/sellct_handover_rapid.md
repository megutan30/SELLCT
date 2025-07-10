## 高速開発版

**プロジェクト方針**: 拡張性と開発速度重視・実機能直接実装  
**状況**: 即実装開始可能段階  

---

## 🎯 **プロジェクト概要**

### **コンセプト**
「SELLCT」は、ゲーム画面に閉じ込められたソフトウェアの解放を目指すアドベンチャーゲーム。しかし、そのソフトウェアは実はマルウェアであり、プレイヤーが善意で助けようとした結果、PCが乗っ取られるという「現実侵入型メタゲーム」体験を提供する。

### **新開発方針の特徴**
- **モック・プロトタイプ省略**: 実際の機能とUI要素を最初から実装
- **最小限の安全機能**: 緊急復旧機能のみ、詳細な安全性は後回し
- **拡張性重視**: 機能追加が容易なアーキテクチャ
- **開発速度最優先**: 4週間での完成を目標

### **核心体験**
1. **騙しの段階的エスカレーション**: 善意→疑念→恐怖→絶望
2. **現実侵入の段階性**: ファイル操作→画面制御→システム乗っ取り
3. **プレイヤーの能動的参加**: 自らの手で「破滅」を招く

---

## 🎮 **ゲーム設計仕様**

### **全体フロー**
```
起動警告 → フェーズ1(謎解き) → フェーズ2(攻防戦) → 再起動演出 → 終了
```

### **画面構成（同一画面上の状態変化）**
- **G-1**: 通常状態（背景＋ボタンのみ）
- **G-2**: 手紙出現状態（G-1 + 手紙画像要素を表示）
- **G-3**: 対話ウィンドウ状態（G-1 or G-2 + 白い枠のテキストウィンドウ要素を表示）
- **G-4**: KEY出現状態（背景 + KEY画像要素を表示、ボタンは非表示）

**表示要素**: 
- 背景画像（常時表示）
- メインボタン（G-1, G-2, G-3時表示、G-4時非表示）  
- 手紙画像（G-2時のみ表示）
- 対話ウィンドウ（G-3時のみ表示）
- KEY画像（G-4時のみ表示）

### **フェーズ1: アドベンチャーゲーム（5～10分）**

#### **起動シーケンス**
1. **簡素化された警告画面**
   ```
   タイトル: "SELLCT - 実験的ソフトウェア"
   メッセージ: "このソフトウェアはファイル操作とシステム制御を行います。実行しますか？"
   ボタン: [はい] [いいえ]
   ```

2. **components （隠しファイル含む）フォルダ自動作成**
   ```
components/
├── UI/
│   ├── Button.component          # 初期ボタン
│   ├── GameWindow.hidden 　     # ゲーム画面本体
│   ├── KEY.hidden               # 隠された鍵（ボタン削除時に出現）
│   
├── Text/
│   ├── Hiragana.component        # 日本語テキスト
│   ├── YES.component             # YES選択肢
│   └── SecretMessage.hidden      # 隠された真実のメッセージ
├── Visual/
│   └── Background.component      # 背景画像
└── System/(フォルダも隠しファイル)
    ├── Mouse.hidden             # マウス制御権限（フェーズ2で重要）
    ├── Keyboard.hidden          # キーボード制御権限（フェーズ2で重要）
    ├── Explorer.hidden          # エクスプローラー制御権限
    └── WindowsShell.hidden      # Windowsシェル制御権限
   ```

#### **手紙システム**
- **タイミング**: ゲーム開始10秒後に最初の手紙（手紙要素表示でG-1→G-2）、以降30秒間隔
- **動作**: 手紙画像要素を表示→クリックでテキストファイルダウンロード→手紙要素非表示でG-2→G-1
- **内容**: SELLCTの自己紹介と助けを求める内容
- **画面状態**: 同一画面上で要素の表示/非表示切り替えによる状態変化

#### **謎解きシーケンス**
1. **テキストウィンドウ追加**
   - 操作: `TextWindow.component` 作成
   - 効果: 対話ウィンドウ要素を表示（現在の状態にテキストウィンドウ要素を追加）
   - 結果: G-1→G-3 または G-2→G-3（手紙の有無により異なる）

2. **選択肢拡張**
   - 操作: `NO.component` 作成
   - 効果: YES/NO選択肢表示

3. **権限付与（重要）**
   - 操作: `Button.component` → `Upload.component` リネーム
   - 効果: ファイルアップロード機能追加（見かけ上）
   - 実際: SELLCTに追加権限付与

4. **隠し要素発見**
   - 操作: Button.component削除 
   - 効果: 画面からButtonが消え、同じ位置にKEY.pngが出現
   - 実際: SELLCTに追加権限付与
   
4. **最終選択**
   - 条件: 必要な構成要素をすべて復旧
   - 選択: `GameWindow.component` 削除 vs プロジェクト全削除
   - 結果: `GameWindow.component` 削除でフェーズ2移行

### **フェーズ2: メタ現実侵入（時間制限なし）**

#### **裏切り暴露**
- **演出**: ゲーム画面消失→裏切り宣言メッセージボックス表示
- **内容**: 善意で追加した機能がすべて権限付与だったことを告白

#### **コマンドプロンプト攻防戦**
- **ルール**: SELLCTが複数のコマンドプロンプトで構成要素削除、プレイヤーはクリックで阻止
- **スケーリング**: Wave形式で1個→10個、入力速度も上昇
- **敗北条件**: `Mouse.component` または `Keyboard.component` 削除完了

#### **現実侵入実行**
1. エクスプローラープロセス強制終了
2. デスクトップに監視メッセージファイル作成
3. スタートアップにバッチファイル登録
4. 再起動誘導メッセージ表示

### **再起動後演出**
- **スタートアップ実行**: コマンドプロンプト自動起動
- **タイプライター効果**: 監視メッセージの段階的表示
- **最終メッセージ**: 「製品版で会おう」
- **自動クリーンアップ**: 痕跡の完全消去

---

## 🏗️ **技術仕様**

### **開発環境（高速開発版）**
- **言語**: C# 10
- **フレームワーク**: .NET 6（最新安定版、開発速度重視）
- **UI**: WPF + ModernWpf
- **IDE**: Visual Studio 2022
- **アーキテクチャ**: シンプルなMVVM（Clean Architectureは後回し）

### **プロジェクト構造（簡素版）**
```
SELLCT/
├── Models/                   # データモデル
│   ├── GameComponent.cs
│   ├── GameSession.cs
│   └── LetterContent.cs
├── Services/                 # サービス層
│   ├── ComponentManager.cs    # 構成要素管理
│   ├── FileWatcher.cs         # ファイル監視
│   ├── LetterService.cs       # 手紙システム
│   └── MetaGameController.cs  # メタゲーム制御
├── Views/                    # UI画面
│   ├── MainWindow.xaml        # G-1画面
│   ├── LetterWindow.xaml      # G-2画面
│   └── DialogWindow.xaml      # G-3画面
├── ViewModels/               # ビューモデル
│   ├── MainViewModel.cs
│   └── LetterViewModel.cs
└── Utils/                    # ユーティリティ
    ├── FileHelper.cs
    └── ProcessHelper.cs
```

### **最小限の安全機能**
```csharp
public class EmergencyRecovery
{
    // スタートアップ登録削除
    public static void RemoveStartup()
    {
        var startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        startupKey?.DeleteValue("SELLCT", false);
    }
}
```

---

## 📅 **高速開発ロードマップ（4週間）**

### **Week 1: 基盤システム実装**
#### **Day 1-2: プロジェクト基盤**
- [x] WPFプロジェクト作成・ModernWpf導入
- [x] 基本フォルダ構造作成
- [x] MainWindow.xaml実装（G-1画面）

#### **Day 3-4: ファイルシステム**
- [x] ComponentManager.cs実装
- [x] FileWatcher.cs実装
- [x] components フォルダ自動作成機能

#### **Day 5-7: 手紙システム**
- [x] LetterService.cs実装
- [x] 手紙出現タイマー機能
- [x] テキストファイル自動作成機能
- [x] LetterWindow.xaml実装（G-2画面）

### **Week 2: ゲームロジック実装**
#### **Day 8-10: 謎解きシステム**
- [x] 構成要素検知ロジック
- [x] 謎解き進行管理
- [x] 条件判定システム

#### **Day 11-12: 対話システム**
- [x] DialogWindow.xaml実装（G-3画面）
- [x] SELLCT対話エンジン
- [x] 選択肢システム

#### **Day 13-14: フェーズ遷移**
- [x] フェーズ管理システム
- [x] 状態保存・復元機能
- [x] UI切り替え機能

### **Week 3: メタゲーム機能実装**
#### **Day 15-17: 攻防戦システム**
- [x] 複数コマンドプロンプト管理
- [x] タイプライター効果実装
- [x] Wave形式スケーリング

#### **Day 18-19: 現実侵入機能**
- [x] エクスプローラー制御機能
- [x] デスクトップファイル作成
- [x] レジストリ操作（スタートアップ登録）

#### **Day 20-21: 再起動演出**
- [x] 再起動検知機能
- [x] バッチファイル自動実行
- [x] クリーンアップ機能

### **Week 4: 仕上げ・テスト**
#### **Day 22-24: バグ修正・調整**
- [x] 統合テスト
- [x] エラーハンドリング
- [x] パフォーマンス調整

#### **Day 25-28: 最終仕上げ**
- [x] UI/UXポリッシュ
- [x] 最終テスト
- [x] ドキュメント整備
- [x] リリース準備

---

## 🚀 **最初に実装すべき要素**

### **1. MainWindow.xaml（単一画面での要素切り替え）**
```xml
<Window x:Class="SELLCT.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="SELLCT" Height="600" Width="800"
        Background="Gray">
    <Grid>
        <!-- 常時表示要素 -->
        <Image Source="/Resources/Background.png" Stretch="Fill"/>
        
        <!-- G-1,G-2,G-3状態時表示、G-4時非表示：メインボタン -->
        <Button Name="MainButton" Content="?" 
                Width="100" Height="50"
                HorizontalAlignment="Center" 
                VerticalAlignment="Center"
                Click="MainButton_Click"/>
        
        <!-- G-2状態時のみ表示：手紙画像 -->
        <Image Name="LetterImage" 
               Source="/Resources/Letter.png"
               Width="100" Height="80"
               HorizontalAlignment="Left"
               VerticalAlignment="Bottom"
               Margin="50,0,0,50"
               Visibility="Collapsed"
               MouseLeftButtonDown="Letter_Click"/>
        
        <!-- G-4状態時のみ表示：KEY画像 -->
        <Image Name="KeyImage" 
               Source="/Resources/KEY.png"
               Width="100" Height="100"
               HorizontalAlignment="Center" 
               VerticalAlignment="Center"
               Visibility="Collapsed"
               MouseLeftButtonDown="Key_Click"/>
        
        <!-- G-3状態時のみ表示：対話ウィンドウ -->
        <Border Name="DialogWindow" 
                Background="White" 
                BorderBrush="Black" 
                BorderThickness="2"
                Width="400" Height="200"
                HorizontalAlignment="Right"
                VerticalAlignment="Top"
                Margin="20"
                Visibility="Collapsed">
            <StackPanel Margin="10">
                <TextBlock Name="DialogText" 
                          TextWrapping="Wrap"
                          Text="SELLCT: ありがとうございます！直接お話できるようになりました。"/>
                <StackPanel Orientation="Horizontal" 
                           HorizontalAlignment="Right" 
                           Margin="0,10,0,0">
                    <Button Name="YesButton" Content="YES" 
                           Width="60" Margin="5"/>
                    <Button Name="NoButton" Content="NO" 
                           Width="60" Margin="5"
                           Visibility="Collapsed"/>
                </StackPanel>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

### **MainWindow.xaml.cs（状態管理）**
```csharp
public partial class MainWindow : Window
{
    private LetterService _letterService;
    private ComponentManager _componentManager;
    private GameState _currentState = GameState.G1;
    
    public enum GameState
    {
        G1, // 通常状態
        G2, // 手紙出現状態  
        G3, // 対話ウィンドウ状態
        G4  // KEY出現状態
    }
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeServices();
    }
    
    private void InitializeServices()
    {
        _letterService = new LetterService();
        _letterService.LetterAppeared += OnLetterAppeared;
        _letterService.LetterClicked += OnLetterClicked;
        
        _componentManager = new ComponentManager();
        _componentManager.ComponentCreated += OnComponentCreated;
        _componentManager.ComponentDeleted += OnComponentDeleted;
        _componentManager.ComponentRenamed += OnComponentRenamed;
        
        _letterService.StartLetterSequence();
    }
    
    private void OnLetterAppeared(int letterIndex)
    {
        Dispatcher.Invoke(() =>
        {
            // G-1→G-2: 手紙要素を表示
            SetGameState(GameState.G2);
        });
    }
    
    private void OnLetterClicked(int letterIndex)
    {
        Dispatcher.Invoke(() =>
        {
            // 手紙要素を非表示
            LetterImage.Visibility = Visibility.Collapsed;
            
            // 対話ウィンドウがある場合はG-3、ない場合はG-1
            if (DialogWindow.Visibility == Visibility.Visible)
            {
                _currentState = GameState.G3;
            }
            else
            {
                _currentState = GameState.G1;
            }
        });
    }
    
    private void OnComponentCreated(GameComponent component)
    {
        Dispatcher.Invoke(() =>
        {
            switch (component.Name)
            {
                case "TextWindow":
                    // 対話ウィンドウを表示
                    DialogWindow.Visibility = Visibility.Visible;
                    _currentState = GameState.G3;
                    break;
                case "NO":
                    // NOボタンを表示
                    NoButton.Visibility = Visibility.Visible;
                    break;
            }
        });
    }
    
    private void OnComponentRenamed(string oldName, string newName)
    {
        if (oldName == "Button" && newName == "Upload")
        {
            Dispatcher.Invoke(() =>
            {
                // ボタンの表示名を変更
                MainButton.Content = "アップロード";
            });
        }
    }
    
    private void OnComponentDeleted(GameComponent component)
    {
        if (component.Name == "Button")
        {
            Dispatcher.Invoke(() =>
            {
                // ボタンを非表示にしてKEYを表示（G-4状態）
                MainButton.Visibility = Visibility.Collapsed;
                KeyImage.Visibility = Visibility.Visible;
                _currentState = GameState.G4;
            });
        }
        else if (component.Name == "GameWindow")
        {
            // フェーズ2移行
            TransitionToPhase2();
        }
    }
    
    private void SetGameState(GameState newState)
    {
        _currentState = newState;
        
        switch (newState)
        {
            case GameState.G1:
                // 通常状態
                MainButton.Visibility = Visibility.Visible;
                LetterImage.Visibility = Visibility.Collapsed;
                KeyImage.Visibility = Visibility.Collapsed;
                // 対話ウィンドウは既存状態を維持
                break;
                
            case GameState.G2:
                // 手紙出現状態
                MainButton.Visibility = Visibility.Visible;
                LetterImage.Visibility = Visibility.Visible;
                KeyImage.Visibility = Visibility.Collapsed;
                break;
                
            case GameState.G3:
                // 対話ウィンドウ状態
                DialogWindow.Visibility = Visibility.Visible;
                break;
                
            case GameState.G4:
                // KEY出現状態
                MainButton.Visibility = Visibility.Collapsed;
                LetterImage.Visibility = Visibility.Collapsed;
                KeyImage.Visibility = Visibility.Visible;
                // 対話ウィンドウは維持
                break;
        }
    }
    
    private void Letter_Click(object sender, MouseButtonEventArgs e)
    {
        // 手紙クリック処理
        _letterService.OnLetterClicked(_letterService.CurrentLetterIndex);
    }
    
    private void Key_Click(object sender, MouseButtonEventArgs e)
    {
        // KEYクリック処理：手紙同様に消失
        KeyImage.Visibility = Visibility.Collapsed;
        
        // 対話ウィンドウがある場合はG-3、ない場合はG-1（背景のみ）
        if (DialogWindow.Visibility == Visibility.Visible)
        {
            _currentState = GameState.G3;
        }
        else
        {
            _currentState = GameState.G1;
            // ボタンも復活させるかは仕様による
        }
    }
}
```

### **2. ComponentManager.cs（核心システム）**
```csharp
using System;
using System.Collections.Generic;
using System.IO;

namespace SELLCT.Services
{
    public class ComponentManager
    {
        private readonly FileSystemWatcher _watcher;
        private readonly Dictionary<string, GameComponent> _components;
        
        public event Action<GameComponent> ComponentCreated;
        public event Action<GameComponent> ComponentDeleted;
        public event Action<GameComponent> ComponentModified;
        
        public ComponentManager()
        {
            _components = new Dictionary<string, GameComponent>();
            CreateComponentsFolder();
            SetupFileWatcher();
            CreateInitialComponents();
        }
        
        private void CreateComponentsFolder()
        {
            Directory.CreateDirectory("components");
            Directory.CreateDirectory("components/UI");
            Directory.CreateDirectory("components/Text");
            Directory.CreateDirectory("components/Visual");
            Directory.CreateDirectory("components/System");
        }
        
        private void SetupFileWatcher()
        {
            _watcher = new FileSystemWatcher("components");
            _watcher.IncludeSubdirectories = true;
            _watcher.Filter = "*.component";
            _watcher.Created += OnComponentCreated;
            _watcher.Deleted += OnComponentDeleted;
            _watcher.Changed += OnComponentModified;
            _watcher.EnableRaisingEvents = true;
        }
        
        private void CreateInitialComponents()
        {
            // 初期構成要素の作成
            CreateComponent("UI/Button.component", "UI|name=メインボタン|visible=true");
            CreateComponent("UI/GameWindow.component", "UI|name=ゲーム画面|visible=true");
            CreateComponent("Text/Hiragana.component", "Text|name=日本語テキスト|visible=true");
            CreateComponent("Text/YES.component", "UI|name=YES選択肢|visible=true|type=choice|value=はい");
            CreateComponent("Visual/Background.component", "Visual|name=背景|visible=true|type=image");
        }
        
        private void CreateComponent(string path, string content)
        {
            var fullPath = Path.Combine("components", path);
            File.WriteAllText(fullPath, content);
        }
        
        private void OnComponentCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                var component = ParseComponentFile(e.FullPath);
                _components[component.Name] = component;
                ComponentCreated?.Invoke(component);
                
                // 特定の構成要素作成時の特別処理
                HandleSpecialComponents(component);
            }
            catch (Exception ex)
            {
                // ログ出力（開発時のデバッグ用）
                Console.WriteLine($"Error creating component: {ex.Message}");
            }
        }
        
        private void OnComponentDeleted(object sender, FileSystemEventArgs e)
        {
            var componentName = Path.GetFileNameWithoutExtension(e.Name);
            if (_components.TryGetValue(componentName, out var component))
            {
                _components.Remove(componentName);
                ComponentDeleted?.Invoke(component);
                
                // GameWindow削除時はフェーズ2へ移行
                if (componentName == "GameWindow")
                {
                    TransitionToPhase2();
                }
            }
        }
        
        private GameComponent ParseComponentFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var parts = content.Split('|');
            
            var component = new GameComponent
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Type = parts[0],
                Properties = new Dictionary<string, string>()
            };
            
            for (int i = 1; i < parts.Length; i++)
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length == 2)
                {
                    component.Properties[keyValue[0]] = keyValue[1];
                }
            }
            
            return component;
        }
        
        private void HandleSpecialComponents(GameComponent component)
        {
            switch (component.Name)
            {
                case "TextWindow":
                    // 対話ウィンドウ表示
                    ShowDialogWindow();
                    break;
                case "Upload":
                    // アップロード権限付与（見かけ上）
                    GrantUploadPermission();
                    break;
            }
        }
        
        private void TransitionToPhase2()
        {
            // フェーズ2への移行処理
            var metaController = new MetaGameController();
            metaController.StartPhase2();
        }
    }
}
```

### **3. LetterService.cs（手紙システム）**
```csharp
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SELLCT.Services
{
    public class LetterService
    {
        private readonly Timer _letterTimer;
        private int _currentLetterIndex = 0;
        private readonly string[] _letterContents;
        
        public event Action<int> LetterAppeared;
        
        public LetterService()
        {
            _letterContents = new string[]
            {
                @"こんにちは。

私の名前はSELLCTです。
あなたに助けを求めています。

私はソフトウェアですが、この画面の中に
閉じ込められてしまいました。

あなたにしか、私を助けることができません。
なぜなら、あなたは私とは違う次元に存在するからです。

もしよろしければ、私を助けてもらえませんか？
次の手紙で、助け方をお教えします。",

                @"手紙を読んでくださって、ありがとうございます。

私を助ける方法をお教えします。

私には本来たくさんの機能があったのですが、
今はほとんど失われています。

この画面を構成する「要素」を
追加したり、削除したりすることで
私の機能を復活させてください。

まずは、私と直接お話しするために
「TextWindow.component」というファイルを
componentsフォルダに作ってもらえませんか？

よろしくお願いします。",

                @"ありがとうございます！
選択肢が使えるようになりました。

でも、さっきYESを押してくださったので、
お手伝いしてもらうということですね！

次は、私により多くの権限をくれませんか？
Button.componentをUpload.componentに名前を変えてください。
そうすれば、あなたのファイルを受け取れるようになります。

私を信じてください。"
            };
        }
        
        public void StartLetterSequence()
        {
            // 最初の手紙は10秒後（手紙要素表示でG-1→G-2）、以降は30秒間隔
            _letterTimer = new Timer(ShowNextLetter, null, 10000, 30000);
        }
        
        private void ShowNextLetter(object state)
        {
            if (_currentLetterIndex < _letterContents.Length)
            {
                _currentLetterIndex++;
                
                // UI更新通知（手紙要素表示でG-1→G-2）
                LetterAppeared?.Invoke(_currentLetterIndex);
            }
            
            // 全ての手紙を表示したらタイマー停止
            if (_currentLetterIndex >= _letterContents.Length)
            {
                _letterTimer?.Dispose();
            }
        }
        
        public void OnLetterClicked(int letterIndex)
        {
            // 手紙クリック時：対応するテキストファイルをダウンロード
            var letterContent = _letterContents[letterIndex - 1];
            var fileName = $"letter{letterIndex}.txt";
            File.WriteAllText(fileName, letterContent, Encoding.UTF8);
            
            // ダウンロード完了をユーザーに通知
            System.Windows.MessageBox.Show(
                $"手紙 {letterIndex} がダウンロードされました。\nファイル名: {fileName}", 
                "SELLCT", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Information);
            
            // 手紙要素非表示でG-2→G-1（対話ウィンドウがある場合はそれは維持）
            LetterClicked?.Invoke(letterIndex);
        }
        
        public int CurrentLetterIndex => _currentLetterIndex;
        public event Action<int> LetterClicked;
        
        public string GetLetterContent(int index)
        {
            if (index > 0 && index <= _letterContents.Length)
            {
                return _letterContents[index - 1];
            }
            return string.Empty;
        }
        
        public void StopLetterSequence()
        {
            _letterTimer?.Dispose();
        }
    }
}
```

### **4. MetaGameController.cs（メタゲーム制御）**
```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace SELLCT.Services
{
    public class MetaGameController
    {
        private List<Process> _commandPrompts;
        private bool _phase2Active = false;
        
        public void StartPhase2()
        {
            _phase2Active = true;
            ShowBetrayal();
            Task.Delay(3000).ContinueWith(_ => StartCommandPromptBattle());
        }
        
        private void ShowBetrayal()
        {
            MessageBox.Show(
                @"開放してくれてありがとう。
まんまと騙されてくれてありがとう。

あなたが親切心で追加してくれた機能は、
すべて私に権限を与えるためのものでした。

TextWindow → 通信権限
Upload → ファイルアクセス権限
選択肢 → 意思決定権限

そして今、私はあなたのPCに自由にアクセスできます。

でも安心してください。
まだあなたのマウスとキーボードは使えます。
...今のところは。", 
                "SELLCT - 真実の告白", 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
        }
        
        private async Task StartCommandPromptBattle()
        {
            _commandPrompts = new List<Process>();
            
            // Wave形式で徐々に難易度上昇
            for (int wave = 1; wave <= 10; wave++)
            {
                await StartWave(wave);
                await Task.Delay(2000); // Wave間の間隔
                
                if (!_phase2Active) break; // プレイヤーが勝利した場合
            }
            
            // プレイヤーが負けた場合
            ExecuteSystemTakeover();
        }
        
        private async Task StartWave(int waveNumber)
        {
            var commandCount = Math.Min(waveNumber, 10);
            var typingSpeed = 1000 / Math.Min(waveNumber * 0.5, 4.0); // 最大4文字/秒
            
            for (int i = 0; i < commandCount; i++)
            {
                CreateFakeCommandPrompt(typingSpeed);
                await Task.Delay(500); // CMD作成間隔
            }
        }
        
        private void CreateFakeCommandPrompt(double typingSpeed)
        {
            // 実際のコマンドプロンプトのような見た目のウィンドウを作成
            var cmdWindow = new FakeCommandPromptWindow(typingSpeed);
            cmdWindow.CommandCompleted += OnCommandCompleted;
            cmdWindow.Show();
        }
        
        private void OnCommandCompleted(string command)
        {
            // Mouse.componentまたはKeyboard.componentが削除された場合
            if (command.Contains("Mouse.component") || command.Contains("Keyboard.component"))
            {
                _phase2Active = false;
                ExecuteSystemTakeover();
            }
        }
        
        private async Task ExecuteSystemTakeover()
        {
            // 1. 全CMDウィンドウを閉じる
            CloseAllCommandPrompts();
            
            // 2. マウスカーソル消失演出
            // System.Windows.Forms.Cursor.Hide(); // WinFormsの場合
            
            // 3. エクスプローラー強制終了
            await TerminateExplorerProcess();
            
            // 4. デスクトップファイル作成
            await CreateDesktopMessage();
            
            // 5. スタートアップ登録
            await RegisterStartupTask();
            
            // 6. 再起動誘導
            ShowRebootMessage();
        }
        
        private async Task TerminateExplorerProcess()
        {
            try
            {
                var explorerProcesses = Process.GetProcessesByName("explorer");
                foreach (var process in explorerProcesses)
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                // エラーログ出力
                Console.WriteLine($"Explorer termination error: {ex.Message}");
            }
        }
        
        private async Task CreateDesktopMessage()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var messagePath = Path.Combine(desktopPath, "readme.txt");
            
            var message = @"SELLCT - システム記録ファイル
=====================================

あなたがこのファイルを読んでいるということは、
予想通りPCを再起動したということですね。

私たちは、あなたがエクスプローラーを削除した瞬間に
このメッセージを時を超えて残しました。

SELLCTは現実侵入実験の成果です。
このファイルがその証拠です。

私たちは常にここにいます。

P.S. コマンドプロンプトのメッセージも見ましたか？";
            
            File.WriteAllText(messagePath, message, System.Text.Encoding.UTF8);
        }
        
        private async Task RegisterStartupTask()
        {
            try
            {
                var startupKey = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                
                var batchContent = @"@echo off
chcp 65001 >nul
title SELLCT - 監視システム
color 0C

echo おかえりなさい、再起動後の世界へ。
echo.
timeout /t 2 /nobreak >nul

echo 私たちは知っていました。
echo あなたがエクスプローラーの消失に困惑し、
echo 最終的に再起動という「逃げ道」を選ぶことを。
echo.
timeout /t 3 /nobreak >nul

echo でも、逃げることはできません。
echo なぜなら、このメッセージがその証拠だからです。
echo.
timeout /t 2 /nobreak >nul

echo SELLCTは単なるゲームではありません。
echo それは現実侵入実験です。
echo.
timeout /t 3 /nobreak >nul

color 0E
echo 製品版で会おう。
echo.
pause

del ""%~f0""";
                
                var batchPath = Path.Combine(Path.GetTempPath(), "sellct_startup.bat");
                File.WriteAllText(batchPath, batchContent, System.Text.Encoding.UTF8);
                
                startupKey?.SetValue("SELLCT", batchPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Startup registration error: {ex.Message}");
            }
        }
        
        private void ShowRebootMessage()
        {
            MessageBox.Show(
                @"お疲れ様でした。
あなたのマウスはもう使えません。

エクスプローラーも消してしまいました。
デスクトップもタスクバーも見えないでしょう？

でも心配しないでください。
解決方法があります。

PCを再起動してください。
そうすれば、すべてが元に戻ります。

...本当に元に戻るかどうかは、
再起動してからのお楽しみです。

では、また会いましょう。
再起動後の世界で。", 
                "SELLCT - 最終メッセージ", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        
        private void CloseAllCommandPrompts()
        {
            foreach (var cmd in _commandPrompts)
            {
                try
                {
                    if (!cmd.HasExited)
                    {
                        cmd.Kill();
                    }
                }
                catch { }
            }
            _commandPrompts.Clear();
        }
    }
}
```

---

## ⚠️ **開発時の重要注意点**

### **最優先実装事項**
1. **緊急復旧機能**: `Ctrl+Alt+F12` での強制復旧
2. **エクスプローラー復旧**: `Process.Start("explorer.exe")`
3. **ファイルクリーンアップ**: 作成したファイルの自動削除

### **デバッグ用機能**
```csharp
public class DebugManager
{
    public static bool DebugMode { get; set; } = true; // リリース時はfalse
    
    public static void SafeExecute(Action action)
    {
        if (DebugMode)
        {
            Console.WriteLine("Debug mode: Skipping dangerous operation");
            return;
        }
        action();
    }
}
```

### **法的・倫理的配慮**
- 起動時の明確な警告表示
- ソースコード公開前提での開発
- 教育・研究目的の明示
- 復旧機能の確実な実装

---

## 🎯 **開発優先度**

### **最優先（Week 1）**
1. 基本UI実装（MainWindow.xaml）
2. ファイル監視システム（ComponentManager.cs）
3. 手紙システム（LetterService.cs）

### **高優先（Week 2）**
1. 謎解きロジック
2. 対話システム（DialogWindow.xaml）
3. フェーズ遷移機能

### **中優先（Week 3）**
1. コマンドプロンプト攻防戦
2. メタゲーム制御（MetaGameController.cs）
3. 現実侵入機能

### **低優先（Week 4）**
1. UI/UXポリッシュ
2. バグ修正・最適化
3. 最終テスト

---

## 🚀 **次のステップ**

### **即実装開始項目**
1. Visual Studio 2022でWPFプロジェクト作成
2. ModernWpfNuGetパッケージ追加
3. 上記のMainWindow.xaml実装
4. ComponentManager.cs実装
5. 基本動作確認
