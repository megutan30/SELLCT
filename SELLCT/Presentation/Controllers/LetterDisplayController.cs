using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using SELLCT.Infrastructure.Services;
using SELLCT.Core.Events;
using SELLCT.Core.Interfaces;
using SELLCT.Core.Entities;

namespace SELLCT.Presentation.Controllers
{
    /// <summary>
    /// 手紙表示の制御を担当するPresentation層のコントローラー
    /// タイマーベースの手紙配信、UIイベント処理、条件チェックを管理
    /// MVCパターンのControllerとしてViewとServiceの仲介役を果たす
    /// </summary>
    public class LetterDisplayController : IDisposable
    {
        /// <summary>手紙配信処理を担当するサービス</summary>
        private readonly LetterService _letterService;
        
        /// <summary>ドメインイベントの配信を管理するディスパッチャー</summary>
        private readonly IEventDispatcher _eventDispatcher;
        
        /// <summary>初回の手紙表示タイミングを制御するタイマー</summary>
        private readonly DispatcherTimer _initialLetterTimer;
        
        /// <summary>2通目以降の手紙表示タイミングを制御するタイマー</summary>
        private readonly DispatcherTimer _subsequentLetterTimer;

        /// <summary>UI上の手紙画像要素への参照</summary>
        private FrameworkElement _letterImage;
        
        /// <summary>UI上のステータステキスト要素への参照</summary>
        private FrameworkElement _statusText;

        /// <summary>リソース解放フラグ（二重解放防止）</summary>
        private bool _disposed = false;

        /// <summary>
        /// LetterDisplayControllerのコンストラクタ
        /// タイマーの初期化、イベントハンドラーの登録、サービス依存性の注入を実行
        /// </summary>
        /// <param name="letterService">手紙配信処理を担当するサービス</param>
        /// <param name="eventDispatcher">ドメインイベント配信システム</param>
        public LetterDisplayController(LetterService letterService, IEventDispatcher eventDispatcher)
        {
            _letterService = letterService;
            _eventDispatcher = eventDispatcher;

            // 初回手紙表示用タイマーを設定（デフォルト3秒間隔）
            _initialLetterTimer = new DispatcherTimer();
            _initialLetterTimer.Interval = TimeSpan.FromSeconds(3);
            _initialLetterTimer.Tick += InitialLetterTimer_Tick;

            // 2通目以降の手紙表示用タイマーを設定（デフォルト10秒間隔）
            _subsequentLetterTimer = new DispatcherTimer();
            _subsequentLetterTimer.Interval = TimeSpan.FromSeconds(10);
            _subsequentLetterTimer.Tick += SubsequentLetterTimer_Tick;

            // ドメインイベントの購読登録
            _eventDispatcher.Subscribe<LetterAppearedEvent>(OnLetterAppeared);
            _eventDispatcher.Subscribe<LetterClickedEvent>(OnLetterClicked);
        }

        /// <summary>
        /// UI要素への参照を注入（Dependency Injection）
        /// MVCパターンにおけるControllerとViewの分離を実現
        /// </summary>
        /// <param name="letterImage">手紙画像を表示するUI要素</param>
        /// <param name="statusText">ステータステキストを表示するUI要素</param>
        public void InjectUIElements(FrameworkElement letterImage, FrameworkElement statusText)
        {
            _letterImage = letterImage;
            _statusText = statusText;
        }

        /// <summary>
        /// メインボタンクリック時の処理
        /// 手紙配信シーケンスの開始、初回タイマーの起動、UI状態更新を実行
        /// </summary>
        public void HandleMainButtonClick()
        {
            try
            {
                // 初回配信かつタイマーが未実行の場合のみ処理
                if (!_initialLetterTimer.IsEnabled && _letterService.CurrentLetterIndex == 0)
                {
                    // 次の手紙の条件を取得してタイマー間隔を調整
                    var firstCondition = _letterService.GetNextLetterCondition();
                    if (firstCondition != null)
                    {
                        _initialLetterTimer.Interval = TimeSpan.FromSeconds(firstCondition.TimeIntervalSeconds);
                    }
                    
                    // 初回手紙配信タイマーを開始
                    _initialLetterTimer.Start();
                    UpdateStatusText("Waiting for letter to arrive...");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HandleMainButtonClick: {ex.Message}");
            }
        }

        /// <summary>
        /// 手紙クリック時の処理
        /// 手紙のダウンロード処理、次の手紙配信準備、UI状態更新を実行
        /// </summary>
        public void HandleLetterClick()
        {
            try
            {
                var letterIndex = _letterService?.CurrentLetterIndex ?? 1;
                if (_letterService == null) return;

                // 手紙を一時的に非表示
                SetLetterVisibility(false);

                // LetterServiceに手紙クリックを通知してダウンロード処理を実行
                bool success = _letterService.OnLetterClicked(letterIndex);

                if (success)
                {
                    // ダウンロード成功：次の手紙の配信準備
                    if (!_letterService.IsSequenceComplete)
                    {
                        SetupNextLetterTimer();
                        UpdateStatusText("Waiting for next letter to arrive...");
                    }
                }
                else
                {
                    // ダウンロードキャンセル：手紙を再表示
                    SetLetterVisibility(true);
                    UpdateStatusText("Letter download was cancelled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in HandleLetterClick: {ex.Message}");
                SetLetterVisibility(true); // エラー時は手紙を再表示
            }
        }

        /// <summary>
        /// 手紙出現イベントのハンドラー
        /// ドメインイベント「LetterAppearedEvent」を受信してUI状態を更新
        /// </summary>
        /// <param name="event">手紙出現イベント（手紙インデックス情報を含む）</param>
        private void OnLetterAppeared(LetterAppearedEvent @event)
        {
            try
            {
                // 手紙画像を表示状態に変更
                SetLetterVisibility(true);
                
                // ステータステキストを更新（手紙到着通知）
                UpdateStatusText($"Letter {@event.LetterIndex} from SELLCT has arrived");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnLetterAppeared: {ex.Message}");
            }
        }

        /// <summary>
        /// 手紙クリックイベントのハンドラー
        /// ドメインイベント「LetterClickedEvent」を受信してUI状態と次タイマーをリセット
        /// </summary>
        /// <param name="event">手紙クリックイベント（手紙インデックス情報を含む）</param>
        private void OnLetterClicked(LetterClickedEvent @event)
        {
            try
            {
                // ステータステキストを更新（ダウンロード完了通知）
                UpdateStatusText($"Downloaded letter {@event.LetterIndex}");
                
                // 次の手紙配信タイマーをリセット（新しい条件で再設定）
                ResetNextLetterTimer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnLetterClicked: {ex.Message}");
            }
        }

        /// <summary>
        /// 初回手紙配信タイマーのTickイベント
        /// タイマーを停止して最初の手紙の表示処理を実行
        /// </summary>
        private void InitialLetterTimer_Tick(object sender, EventArgs e)
        {
            _initialLetterTimer.Stop();
            TryShowNextLetterWithCondition();
        }

        /// <summary>
        /// 2通目以降手紙配信タイマーのTickイベント
        /// タイマーを停止して次の手紙の表示処理を実行
        /// </summary>
        private void SubsequentLetterTimer_Tick(object sender, EventArgs e)
        {
            _subsequentLetterTimer.Stop();
            TryShowNextLetterWithCondition();
        }

        /// <summary>
        /// 条件付きで次の手紙を表示する試行処理
        /// 条件チェック、ファイル存在確認、手紙表示、次タイマー設定を実行
        /// </summary>
        private void TryShowNextLetterWithCondition()
        {
            try
            {
                // 次の手紙の表示条件を取得
                var condition = _letterService.GetNextLetterCondition();
                
                // 条件が設定されていない場合は無条件で表示
                if (condition == null)
                {
                    _letterService.ShowNextLetter();
                    return;
                }

                // componentsフォルダのパスを構築（デスクトップ配下）
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var componentsPath = Path.Combine(desktopPath, "components");
                
                // 条件判定を実行（ファイル存在確認等）
                bool conditionMet = _letterService.CheckCondition(condition, componentsPath);

                if (conditionMet)
                {
                    // 条件満たした場合：手紙表示と次タイマー設定
                    _letterService.ShowNextLetter();
                    SetupNextLetterTimer();
                }
                // 条件を満たしていない場合は何もしない（再度タイマーで確認される）
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TryShowNextLetterWithCondition: {ex.Message}");
            }
        }

        /// <summary>
        /// 次の手紙配信タイマーの設定
        /// 現在のタイマーを停止し、次の手紙の条件に基づいて新しいタイマーを開始
        /// </summary>
        private void SetupNextLetterTimer()
        {
            try
            {
                // 既存のタイマーが動作中の場合は停止
                if (_subsequentLetterTimer.IsEnabled)
                {
                    _subsequentLetterTimer.Stop();
                }

                // 次の手紙の表示条件を取得
                var nextCondition = _letterService.GetNextLetterCondition();
                
                // 時間要素を含む条件の場合のみタイマーを設定
                if (nextCondition != null && HasTimeComponent(nextCondition.TriggerType))
                {
                    _subsequentLetterTimer.Interval = TimeSpan.FromSeconds(nextCondition.TimeIntervalSeconds);
                    _subsequentLetterTimer.Start();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up next letter timer: {ex.Message}");
            }
        }

        /// <summary>
        /// 次の手紙配信タイマーのリセット
        /// 手紙ダウンロード完了後に次の配信タイミングを再設定
        /// </summary>
        private void ResetNextLetterTimer()
        {
            try
            {
                // 現在動作中のタイマーを停止
                if (_subsequentLetterTimer.IsEnabled)
                {
                    _subsequentLetterTimer.Stop();
                }

                // 次の手紙の条件を取得して新しいタイマー間隔を設定
                var nextCondition = _letterService.GetNextLetterCondition();
                if (nextCondition != null && HasTimeComponent(nextCondition.TriggerType))
                {
                    _subsequentLetterTimer.Interval = TimeSpan.FromSeconds(nextCondition.TimeIntervalSeconds);
                    _subsequentLetterTimer.Start();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting next letter timer: {ex.Message}");
            }
        }

        /// <summary>
        /// トリガータイプが時間要素を含むかどうかを判定
        /// タイマーベースの手紙配信が必要かどうかの判断に使用
        /// </summary>
        /// <param name="triggerType">チェック対象のトリガータイプ</param>
        /// <returns>時間要素を含む場合true、それ以外false</returns>
        private bool HasTimeComponent(LetterTriggerType triggerType)
        {
            return triggerType == LetterTriggerType.TimeOnly ||
                   triggerType == LetterTriggerType.TimeAndFileExistence ||
                   triggerType == LetterTriggerType.TimeAndFileNotExistence ||
                   triggerType == LetterTriggerType.TimeAndAllFilesExist ||
                   triggerType == LetterTriggerType.TimeAndAnyFileExists ||
                   triggerType == LetterTriggerType.TimeAndAllFilesNotExist ||
                   triggerType == LetterTriggerType.TimeAndAnyFileNotExists;
        }

        /// <summary>
        /// 手紙画像の表示・非表示を制御
        /// UI要素の参照がnullでない場合のみ操作を実行
        /// </summary>
        /// <param name="isVisible">表示する場合true、非表示にする場合false</param>
        private void SetLetterVisibility(bool isVisible)
        {
            if (_letterImage != null)
            {
                _letterImage.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// ステータステキストの更新
        /// TextBlock型のUI要素への安全なテキスト設定
        /// </summary>
        /// <param name="text">設定するテキスト内容</param>
        private void UpdateStatusText(string text)
        {
            if (_statusText is System.Windows.Controls.TextBlock statusTextBlock)
            {
                statusTextBlock.Text = text;
            }
        }

        /// <summary>
        /// リソースの解放処理
        /// タイマーの停止とイベントハンドラーのクリーンアップを実行
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                
                // 両方のタイマーを停止してリソースを解放
                _initialLetterTimer?.Stop();
                _subsequentLetterTimer?.Stop();
            }
        }
    }
}