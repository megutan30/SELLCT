using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// パズルアクションクラス
    /// パズルがトリガーされた時に実行される具体的な処理を定義
    /// UI操作、システム制御、ゲーム進行など様々な種類のアクションを統一的に管理
    /// </summary>
    public class PuzzleAction
    {
        /// <summary>
        /// アクションタイプ列挙型
        /// 実行可能なアクションの種類を定義
        /// 各アクションはMainWindowPuzzleActionHandlerで実装される
        /// </summary>
        public enum ActionType
        {
            // === 基本UI操作 ===
            /// <summary>ダイアログメッセージを表示</summary>
            ShowDialog,
            /// <summary>メインボタンのテキストを変更</summary>
            ChangeMainButtonContent,
            /// <summary>隠しアイテムを公開・表示</summary>
            RevealHiddenItem,
            /// <summary>ゲームフェーズ2に移行</summary>
            TransitionToPhase2,
            /// <summary>システムメッセージボックスを表示</summary>
            ShowMessageBox,
            /// <summary>NOボタンのテキストを変更</summary>
            ChangeNoButtonContent,
            /// <summary>NO機能を有効化</summary>
            EnableNoFunction,
            /// <summary>選択肢ダイアログを表示</summary>
            ShowChoice,

            // === 可視性制御アクション ===
            /// <summary>メインボタンの表示/非表示を切り替え</summary>
            SetMainButtonVisibility,
            /// <summary>キーオブジェクトの表示/非表示を切り替え</summary>
            SetKeyVisibility,
            /// <summary>ドアオブジェクトの表示/非表示を切り替え</summary>
            SetDoorVisibility,
            /// <summary>テキストウィンドウの表示/非表示を切り替え</summary>
            SetTextWindowVisibility,
            /// <summary>背景要素の表示/非表示を切り替え</summary>
            SetBackgroundVisibility,
            
            // === システム制御アクション ===
            /// <summary>Windowsエクスプローラーを強制終了</summary>
            TerminateExplorer,
            /// <summary>キーボード入力を無効化</summary>
            DisableKeyboardInput,
            /// <summary>マウス入力を無効化</summary>
            DisableMouseInput,
            /// <summary>キーボード入力を有効化</summary>
            EnableKeyboardInput,
            /// <summary>マウス入力を有効化</summary>
            EnableMouseInput,
            
            // === ゲーム制御アクション ===
            /// <summary>ゲームを初期状態にリセット</summary>
            ResetGame,
            /// <summary>メッセージキューをクリア</summary>
            ClearMessageQueue,
            /// <summary>アプリケーションを終了</summary>
            ExitApplication,
            /// <summary>メッセージ表示後にアプリケーション終了</summary>
            ExitWithMessageBox,
            /// <summary>遅延後にメッセージ表示してアプリケーション終了</summary>
            DelayedExitWithMessageBox,
            /// <summary>Windowsエクスプローラーを再起動</summary>
            StartExplorer,
            /// <summary>裏切りエンディング実行</summary>
            BetrayalEnding,
            
            // === 偽デスクトップアイコン制御 ===
            /// <summary>偽デスクトップアイコンを表示</summary>
            ShowPseudoDesktopIcon,
            /// <summary>偽デスクトップアイコンを隠す</summary>
            HidePseudoDesktopIcon,
            /// <summary>偽デスクトップアイコンの位置を更新</summary>
            UpdatePseudoIconPosition,
            
            // === 特殊ファイル操作 ===
            /// <summary>隠しAuthorityフォルダを作成</summary>
            CreateHiddenAuthorityFolder,

            // === 位置制御アクション ===
            /// <summary>ボタンの位置を設定</summary>
            SetButtonPosition,
            /// <summary>YESボタンの位置を設定</summary>
            SetYESPosition,
            /// <summary>キーの位置を設定</summary>
            SetKeyPosition,
            /// <summary>ドアの位置を設定</summary>
            SetDoorPosition,
            /// <summary>背景の位置を設定</summary>
            SetBackgroundPosition,

            // === コンポーネント操作 ===
            /// <summary>位置指定でコンポーネントを再作成</summary>
            RecreateComponentWithPosition,
            
            // === ヒントシステム ===
            /// <summary>ボタン操作ヒントを開始</summary>
            StartButtonHint,
            /// <summary>ボタン操作ヒントをキャンセル</summary>
            CancelButtonHint,
            /// <summary>Authorityフォルダ探索ヒントを開始</summary>
            StartAuthorityHints,
            /// <summary>Authorityフォルダ探索ヒントをキャンセル</summary>
            CancelAuthorityHints,

            // === ウィンドウ制御 ===
            /// <summary>ゲームウィンドウを非表示（透明化、枠線・タイトルバー削除）</summary>
            HideGameWindow,
            /// <summary>ゲームウィンドウを表示（元の状態に復元）</summary>
            ShowGameWindow,

            // === オーディオ制御 ===
            /// <summary>BGM音量を設定（ファイル内容から自動パース）</summary>
            SetBgmVolume,
            /// <summary>SE音量を設定（ファイル内容から自動パース）</summary>
            SetSeVolume,
            /// <summary>BGMを無効化（停止）</summary>
            DisableBGM,
            /// <summary>BGMを有効化（再開）</summary>
            EnableBGM,
            /// <summary>SEを無効化</summary>
            DisableSE,
            /// <summary>SEを有効化</summary>
            EnableSE,
            /// <summary>クリックSEを再生</summary>
            PlayClickSound,
            /// <summary>ドアSEを再生</summary>
            PlayDoorSound,
            /// <summary>コンポーネント操作SEを再生</summary>
            PlayComponentSound
        }

        /// <summary>
        /// アクションタイプ
        /// このアクションが実行する処理の種類を指定
        /// ActionType列挙型の値を使用
        /// </summary>
        public ActionType Type { get; set; }
        
        /// <summary>
        /// メッセージ内容
        /// ShowDialog、ShowMessageBox等で表示されるテキスト
        /// null/空文字の場合はメッセージ表示をスキップ
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 新しいコンテンツ
        /// ボタンテキスト変更時などの新しい表示内容
        /// ChangeMainButtonContent、ChangeNoButtonContent等で使用
        /// </summary>
        public string NewContent { get; set; }
        
        /// <summary>
        /// ターゲットコンポーネント名
        /// アクションの対象となるコンポーネントを指定
        /// コンポーネント操作系アクションで使用
        /// </summary>
        public string TargetComponent { get; set; }
        
        /// <summary>
        /// 隠しアイテムフォルダパス
        /// RevealHiddenItem実行時の隠しアイテムが格納されているフォルダ
        /// 通常は"Authority"等の隠しフォルダを指定
        /// </summary>
        public string HiddenItemFolder { get; set; }
        
        /// <summary>
        /// 隠しアイテム表示名
        /// RevealHiddenItem実行時にUIに表示される名前
        /// 実際のファイル名と異なる表示名を設定可能
        /// </summary>
        public string HiddenItemDisplayName { get; set; }
        
        /// <summary>
        /// アイコン名
        /// 偽デスクトップアイコン操作で使用するアイコンの識別名
        /// ShowPseudoDesktopIcon、HidePseudoDesktopIcon等で使用
        /// </summary>
        public string IconName { get; set; }
        
        /// <summary>
        /// フォルダパス
        /// ファイル・フォルダ操作系アクションで使用するパス
        /// CreateHiddenAuthorityFolder等で使用
        /// </summary>
        public string FolderPath { get; set; }
        
        /// <summary>
        /// アイコンX座標
        /// 偽デスクトップアイコンの水平位置
        /// UpdatePseudoIconPosition等で使用
        /// </summary>
        public double IconX { get; set; }
        
        /// <summary>
        /// アイコンY座標
        /// 偽デスクトップアイコンの垂直位置
        /// UpdatePseudoIconPosition等で使用
        /// </summary>
        public double IconY { get; set; }
        
        /// <summary>
        /// YES選択時実行アクションリスト
        /// ShowChoice実行時にYESが選択された場合に実行されるアクション群
        /// 対話システムの分岐処理で使用
        /// </summary>
        public List<PuzzleAction> YesActions { get; set; }
        
        /// <summary>
        /// NO選択時実行アクションリスト
        /// ShowChoice実行時にNOが選択された場合に実行されるアクション群
        /// 対話システムの分岐処理で使用
        /// </summary>
        public List<PuzzleAction> NoActions { get; set; }
        
        /// <summary>
        /// 選択肢なし時実行アクションリスト
        /// YES/NO両方のコンポーネントが存在しない場合に実行されるアクション群
        /// フォールバック処理として使用
        /// </summary>
        public List<PuzzleAction> NetherChoiceActions { get; set; }
        
        /// <summary>
        /// 可視性フラグ
        /// 可視性制御アクション（SetMainButtonVisibility等）で使用
        /// true: 表示、false: 非表示
        /// </summary>
        public bool IsVisible { get; set; }
        
        /// <summary>
        /// 遅延時間（ミリ秒）
        /// DelayedExitWithMessageBox等の遅延系アクションで使用
        /// アクション実行前の待機時間を指定
        /// デフォルト値: 3000ミリ秒（3秒）
        /// </summary>
        public int DelayMilliseconds { get; set; } = 3000;
    }
}
