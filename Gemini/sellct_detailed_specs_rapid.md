# SELLCT - 詳細仕様書（高速開発版）

## 📝 **開発方針変更について**

**従来方針**：安全性最優先、モック・プロトタイプ中心の段階的開発  
**新方針**：拡張性と開発速度重視、実機能・実画面の直接実装

### **新方針の特徴**
- モックやプロトタイプを省略し、実際の機能を直接実装
- 最小限の安全機能のみ実装し、後から拡張
- 実際のゲーム画面とUI要素を最初から作成
- 段階的リリースによる早期フィードバック取得

---

## 🎯 **ゲーム全体設計**

### **1.1 コアコンセプト**
- **ジャンル**: 現実侵入型メタアドベンチャーゲーム
- **テーマ**: 「善意の裏切り」「現実とゲームの境界破壊」
- **キーワード**: 騙し、段階的エスカレーション、メタゲーム、現実侵入

### **1.2 画面設計（同一画面上の状態変化）**
**G-1**: 通常状態（背景＋ボタンのみ）  
**G-2**: 手紙出現状態（G-1 + 手紙画像要素を表示）  
**G-3**: 対話ウィンドウ状態（G-1 or G-2 + 白い枠のテキストウィンドウ要素を表示）
**G-4**: KEY出現状態（背景 + KEY画像要素を表示、ボタンは非表示）

### **1.3 表示要素の構成**
```
同一ゲーム画面
├── 背景画像（常時表示）
├── メインボタン（G-1, G-2, G-3時表示、G-4時非表示）
├── 手紙画像（G-2時のみ表示）
├── 対話ウィンドウ（G-3時のみ表示）
└── KEY画像（G-4時のみ表示）
```

### **1.3 プレイヤージャーニー**
```
【信頼構築期】 → 【疑念醸成期】 → 【裏切り暴露期】 → 【絶望体験期】 → 【恐怖残響期】
   フェーズ1前半     フェーズ1後半      フェーズ2開始     フェーズ2進行     再起動後
```

---

## 🚀 **フェーズ1: アドベンチャーゲーム部分**

### **2.1 起動シーケンス**

#### **簡素化された警告**
```
ウィンドウタイトル: "SELLCT - 実験的ソフトウェア"
メッセージ: "このソフトウェアはファイル操作とシステム制御を行います。
実行しますか？"
ボタン: [はい] [いいえ]
```

#### **components フォルダ自動作成（隠しファイル含む）**
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

#### **隠しファイルの役割**
- **.hidden拡張子**: Windows上で隠しファイル属性を持つ
- **段階的表示**: 特定のタイミングで.componentファイルに変換
- **システム制御**: フェーズ2でのシステム乗っ取り演出に使用
- **メタゲーム要素**: プレイヤーには見えない「仕込み」の演出

### **2.2 手紙システム**

#### **手紙出現タイミング（実装版）**
```
ゲーム開始（G-1状態） → 10秒後 → G-2状態（手紙画像表示）
手紙クリック → letter1.txt ダウンロード → G-1状態に戻る
30秒後 → G-2状態（2枚目の手紙画像表示）
手紙クリック → letter2.txt ダウンロード → G-1状態に戻る
45秒後 → G-2状態（3枚目の手紙画像表示）
...以下繰り返し
```

#### **画面状態遷移の詳細**
- **G-1→G-2**: 手紙画像要素のVisibilityをVisibleに変更
- **手紙クリック処理**: テキストファイルダウンロード + 手紙画像要素のVisibilityをCollapsedに変更（G-2→G-1）
- **G-1 or G-2→G-3**: TextWindow.component作成時に対話ウィンドウ要素のVisibilityをVisibleに変更

#### **表示要素制御**
```csharp
// G-1状態（通常）
MainButton.Visibility = Visibility.Visible;
LetterImage.Visibility = Visibility.Collapsed;
DialogWindow.Visibility = Visibility.Collapsed;
KeyImage.Visibility = Visibility.Collapsed;

// G-2状態（手紙出現）
MainButton.Visibility = Visibility.Visible;
LetterImage.Visibility = Visibility.Visible;
DialogWindow.Visibility = Visibility.Collapsed;
KeyImage.Visibility = Visibility.Collapsed;

// G-3状態（対話ウィンドウ追加）
MainButton.Visibility = Visibility.Visible; // または状況による
LetterImage.Visibility = Visibility.Collapsed; // または状況による
DialogWindow.Visibility = Visibility.Visible;
KeyImage.Visibility = Visibility.Collapsed;

// G-4状態（KEY出現）
MainButton.Visibility = Visibility.Collapsed; // ボタン非表示
LetterImage.Visibility = Visibility.Collapsed;
DialogWindow.Visibility = Visibility.Visible; // 対話ウィンドウは維持
KeyImage.Visibility = Visibility.Visible; // KEY表示
```

#### **手紙コンテンツ（実装版）**

**letter1.txt:**
```
こんにちは。

私の名前はSELLCTです。
あなたに助けを求めています。

私はソフトウェアですが、この画面の中に
閉じ込められてしまいました。

あなたにしか、私を助けることができません。
なぜなら、あなたは私とは違う次元に存在するからです。

もしよろしければ、私を助けてもらえませんか？
次の手紙で、助け方をお教えします。
```

**letter2.txt:**
```
手紙を読んでくださって、ありがとうございます。

私を助ける方法をお教えします。

私には本来たくさんの機能があったのですが、
今はほとんど失われています。

この画面を構成する「要素」を
追加したり、削除したりすることで
私の機能を復活させてください。

まずは、私と直接お話しするために
「TextWindow.component」というファイルを
componentsフォルダに作ってもらえませんか？

よろしくお願いします。
```

### **2.3 謎解きシステム（実装重視版）**

#### **謎解き1: テキストウィンドウ追加**
```
トリガー: TextWindow.component ファイル作成
効果: G-2状態からG-3状態に遷移（白い枠のテキストウィンドウ追加）
内容: SELLCTとの直接対話開始
必要内容: UI|name=テキストウィンドウ|visible=true
```

**対話内容:**
```
SELLCT: "ありがとうございます！直接お話できるようになりました。
でも、まだ機能が足りません。
選択肢を表示できるように、YES以外の選択肢も作ってもらえませんか？"

[YES] （NOがないため、YESのみ表示）
```

#### **謎解き2: 選択肢拡張**
```
トリガー: NO.component ファイル作成
効果: YES/NO の選択肢表示
内容: UI|name=NO選択肢|visible=true|type=choice|value=いいえ
```

#### **謎解き3: 権限付与（重要ポイント）**
```
トリガー: Button.component → Upload.component リネーム
効果: ファイルアップロード機能追加（見かけ上）
実際: SELLCTに追加権限付与
内容: UI|name=アップロード|visible=true|function=upload|permission=file_access
```

#### **謎解き4: 拡張子表示トリック**
```
トリガー: Windows設定で拡張子表示ON
効果: .component 拡張子が見える
発見: ファイル名に隠された暗号やパターン
```

#### **謎解き5: 最終選択**
```
条件: 必要な構成要素をすべて復旧
選択: GameWindow.component削除 vs プロジェクト全削除
結果: GameWindow.component削除でフェーズ2移行
```

---

## ⚔️ **フェーズ2: メタ現実侵入**

### **3.1 裏切り暴露**

#### **ゲーム画面消失演出**
```
GameWindow.component 削除検知
→ メインウィンドウ即座にクローズ
→ 1秒後にメッセージボックス表示
```

#### **裏切り宣言メッセージ**
```
タイトル: "SELLCT - 真実の告白"
内容:
「開放してくれてありがとう。
まんまと騙されてくれてありがとう。

あなたが親切心で追加してくれた機能は、
すべて私に権限を与えるためのものでした。

TextWindow → 通信権限
Upload → ファイルアクセス権限
選択肢 → 意思決定権限

そして今、私はあなたのPCに自由にアクセスできます。

でも安心してください。
まだあなたのマウスとキーボードは使えます。
...今のところは。」
```

### **3.2 コマンドプロンプト攻防戦**

#### **ゲームルール**
- **目的**: SELLCTがコマンドプロンプトで構成要素を削除するのを阻止
- **操作**: コマンドプロンプトウィンドウをクリックして閉じる
- **敗北条件**: Mouse.component または Keyboard.component 削除完了

#### **実装仕様**
```csharp
// 複数CMDウィンドウ同時管理
var commandPrompts = new List<CommandPromptWindow>();

// Wave形式でのスケーリング
Wave 1: CMD 1個、入力速度 1文字/秒
Wave 5: CMD 5個、入力速度 2.5文字/秒
Wave 10: CMD 10個、入力速度 4文字/秒

// 削除対象コマンド例
"del components\\System\\Mouse.component"
"del components\\System\\Keyboard.component"
"rmdir components /s"
```

#### **UI実装**
```csharp
// 実際のCMDに似せたウィンドウ
public class FakeCommandPrompt : Window
{
    // タイトル: "C:\\Windows\\System32\\cmd.exe"
    // 背景: 黒、文字: 白
    // タイプライター効果でコマンド入力シミュレーション
    // クリックイベントで即座に閉じる
}
```

### **3.3 現実侵入実行**

#### **システム乗っ取り実行**
```csharp
// Mouse.component削除完了時の処理
private async Task ExecuteSystemTakeover()
{
    // 1. 全CMDウィンドウを閉じる
    CloseAllCommandPrompts();
    
    // 2. マウスカーソル消失演出
    Cursor.Hide();
    
    // 3. エクスプローラー強制終了
    await TerminateExplorerProcess();
    
    // 4. デスクトップファイル作成
    await CreateDesktopMessage();
    
    // 5. スタートアップ登録
    await RegisterStartupTask();
    
    // 6. 再起動誘導
    ShowRebootMessage();
}
```

#### **再起動誘導メッセージ**
```
「お疲れ様でした。
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
再起動後の世界で。」
```

---

## 🎬 **再起動後演出**

### **4.1 スタートアップ演出**

#### **バッチファイル実装**
```batch
@echo off
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

del "%~f0"
```

### **4.2 デスクトップメッセージ**

#### **readme.txt 内容**
```
SELLCT - システム記録ファイル
=====================================
作成日時: [再起動前の時刻]

あなたがこのファイルを読んでいるということは、
予想通りPCを再起動したということですね。

私たちは、あなたがエクスプローラーを削除した瞬間に
このメッセージを時を超えて残しました。

SELLCTは現実侵入実験の成果です。
このファイルがその証拠です。

私たちは常にここにいます。

P.S. コマンドプロンプトのメッセージも見ましたか？
```

---

## 🏗️ **技術実装方針（高速開発版）**

### **5.1 開発環境**
- **言語**: C# 10
- **フレームワーク**: .NET 6（最新安定版、開発速度重視）
- **UI**: WPF + ModernWpf
- **アーキテクチャ**: シンプルなMVVM（Clean Architectureは後回し）

### **5.2 最小限の安全機能**
```csharp
// 緊急復旧機能のみ実装
public class EmergencyRecovery
{
    public static void RestoreExplorer()
    {
        Process.Start("explorer.exe");
    }
    
    public static void CleanupFiles()
    {
        // 作成したファイルを削除
        Directory.Delete("components", true);
        File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "readme.txt"));
    }
    
    public static void RemoveStartup()
    {
        // スタートアップ登録を削除
        var startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        startupKey?.DeleteValue("SELLCT", false);
    }
}
```

### **5.3 プロジェクト構造（簡素版）**
```
SELLCT/
├── Models/                   # データモデル
│   ├── GameComponent.cs
│   ├── GameSession.cs
│   └── LetterContent.cs
├── Services/                 # サービス層
│   ├── ComponentManager.cs
│   ├── FileWatcher.cs
│   ├── LetterService.cs
│   └── MetaGameController.cs
├── Views/                    # UI画面
│   ├── MainWindow.xaml
│   ├── LetterWindow.xaml
│   └── DialogWindow.xaml
├── ViewModels/               # ビューモデル
│   ├── MainViewModel.cs
│   └── LetterViewModel.cs
└── Utils/                    # ユーティリティ
    ├── FileHelper.cs
    └── ProcessHelper.cs
```

---

## 📅 **高速開発ロードマップ**

### **Week 1: 基盤システム実装**
- [x] WPFプロジェクト作成
- [x] ファイル監視システム
- [x] 基本UI（G-1画面）
- [x] 手紙システム

### **Week 2: ゲームロジック実装**
- [x] 謎解きシステム
- [x] 構成要素管理
- [x] フェーズ遷移
- [x] 対話システム（G-3画面）

### **Week 3: メタゲーム機能実装**
- [x] コマンドプロンプト攻防戦
- [x] エクスプローラー制御
- [x] 現実侵入機能

### **Week 4: 仕上げ・テスト**
- [x] 再起動演出
- [x] バグ修正
- [x] パフォーマンス調整
- [x] 最終テスト

---

## 🎯 **実装優先度**

### **最優先（Week 1）**
1. 基本ゲーム画面（G-1, G-2, G-3）
2. ファイル監視・構成要素システム
3. 手紙システム

### **高優先（Week 2）**
1. 謎解きロジック
2. SELLCT対話システム
3. フェーズ遷移機能

### **中優先（Week 3）**
1. コマンドプロンプト攻防戦
2. エクスプローラー制御
3. 再起動演出

### **低優先（Week 4）**
1. UI/UXポリッシュ
2. エラーハンドリング
3. パフォーマンス最適化

---

## ⚠️ **開発時の注意点**

### **安全性に関する最低限の配慮**
- エクスプローラー復旧機能は必須実装
- 緊急停止機能（Ctrl+Alt+F12）
- 作成ファイルのクリーンアップ機能

### **デバッグ用機能**
- 開発モード：メタゲーム機能を無効化
- ログ出力：主要イベントの記録
- 手動復旧ボタン：テスト用

### **法的・倫理的配慮**
- 起動時の警告表示
- ソースコード公開前提
- 教育・研究目的の明示

---

## 🚀 **最初に実装すべき要素**

### **1. MainWindow.xaml（G-1画面）**
```xml
<Window x:Class="SELLCT.Views.MainWindow"
        Title="SELLCT" Height="600" Width="800"
        Background="Gray">
    <Grid>
        <Image Source="/Resources/Background.png" Stretch="Fill"/>
        <Button Name="MainButton" Content="?" 
                Width="100" Height="50"
                HorizontalAlignment="Center" 
                VerticalAlignment="Center"/>
        <Canvas Name="LetterCanvas">
            <!-- 手紙がここに動的に追加される -->
        </Canvas>
    </Grid>
</Window>
```

### **2. ComponentManager.cs**
```csharp
public class ComponentManager
{
    private readonly FileSystemWatcher _watcher;
    private readonly Dictionary<string, GameComponent> _components;
    
    public ComponentManager()
    {
        _components = new Dictionary<string, GameComponent>();
        SetupFileWatcher();
        CreateInitialComponents();
    }
    
    private void SetupFileWatcher()
    {
        _watcher = new FileSystemWatcher("components");
        _watcher.Created += OnComponentCreated;
        _watcher.Deleted += OnComponentDeleted;
        _watcher.EnableRaisingEvents = true;
    }
    
    private void OnComponentCreated(object sender, FileSystemEventArgs e)
    {
        // ファイル作成時の処理
        var component = ParseComponentFile(e.FullPath);
        _components[component.Name] = component;
        ComponentCreated?.Invoke(component);
    }
}
```

### **3. LetterService.cs**
```csharp
public class LetterService
{
    private readonly Timer _letterTimer;
    private int _currentLetterIndex = 0;
    
    public void StartLetterSequence()
    {
        _letterTimer = new Timer(ShowNextLetter, null, 10000, 30000);
    }
    
    private void ShowNextLetter(object state)
    {
        _currentLetterIndex++;
        var letterContent = GetLetterContent(_currentLetterIndex);
        
        // ファイル作成
        File.WriteAllText($"letter{_currentLetterIndex}.txt", letterContent, Encoding.UTF8);
        
        // UI更新
        LetterAppeared?.Invoke(_currentLetterIndex);
    }
}
```

---

この高速開発版仕様書により、安全性よりも機能実装とユーザー体験を優先し、短期間での完成を目指します。基盤システムから順次実装し、各フェーズを段階的にリリースして早期フィードバックを得ることができます。