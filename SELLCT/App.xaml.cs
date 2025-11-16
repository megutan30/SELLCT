using System.Windows;
using Application = System.Windows.Application;
using System;
using System.IO;
using System.Media;
using System.Threading;
using SELLCT.Infrastructure.Services;
using SELLCT.Views;
using SELLCT.Core.Interfaces;

namespace SELLCT
{
    /// <summary>
    /// SELLCTアプリケーションのメインエントリーポイント
    /// WPFアプリケーションの初期化、例外処理、起動・終了処理を担当
    /// Clean Architectureのコンポーネント間の依存性注入も管理
    /// </summary>
    public partial class App
    {
        /// <summary>コンポーネント管理（ファイル監視・イベント処理）のインスタンス</summary>
        private ComponentManager _componentManager;

        /// <summary>ドメインイベントの配信を管理するディスパッチャー</summary>
        private IEventDispatcher _eventDispatcher;

        /// <summary>二重起動防止用Mutex</summary>
        private Mutex _mutex;

        /// <summary>
        /// アプリケーションのコンストラクタ
        /// グローバル例外ハンドラーとEventDispatcherを初期化
        /// </summary>
        public App()
        {
            // グローバルな例外ハンドラーを設定
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // EventDispatcherを初期化
            _eventDispatcher = new EventDispatcher();
        }

        /// <summary>
        /// アプリケーション全体での未処理例外ハンドラー
        /// 予期しないエラーが発生した際にユーザーに通知してアプリケーションを安全に終了
        /// </summary>
        /// <param name="sender">イベント送信者</param>
        /// <param name="e">未処理例外イベント引数</param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                MessageBox.Show($"予期せぬエラーが発生しました: {ex.Message}\n\n{ex.StackTrace}", "SELLCT - エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("予期せぬエラーが発生しました。", "SELLCT - エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // アプリケーションをシャットダウン
            this.Shutdown();
        }

        /// <summary>
        /// UIスレッド（Dispatcher）での未処理例外ハンドラー
        /// WPFのUI処理中に発生した例外をキャッチしてアプリケーションのクラッシュを防ぐ
        /// </summary>
        /// <param name="sender">イベント送信者</param>
        /// <param name="e">Dispatcher未処理例外イベント引数</param>
        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"UIスレッドで予期せぬエラーが発生しました: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "SELLCT - エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            // 例外を処理済みとしてマークし、アプリケーションがクラッシュしないようにする
            e.Handled = true;
        }

        /// <summary>
        /// ComponentManagerインスタンスを取得
        /// ファイル監視・イベント処理システムへのアクセスポイント
        /// </summary>
        /// <returns>初期化済みComponentManagerインスタンス</returns>
        public ComponentManager GetComponentManager()
        {
            return _componentManager;
        }

        /// <summary>
        /// EventDispatcherインスタンスを取得
        /// ドメインイベント配信システムへのアクセスポイント
        /// </summary>
        /// <returns>初期化済みEventDispatcherインスタンス</returns>
        public IEventDispatcher GetEventDispatcher()
        {
            return _eventDispatcher;
        }

        /// <summary>
        /// アプリケーション起動時の処理
        /// コンポーネントの初期化、クリーンアップ、メインウィンドウの作成を実行
        /// </summary>
        /// <param name="e">起動イベント引数</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 二重起動チェック
            bool createdNew;
            _mutex = new Mutex(true, "SELLCT_SingleInstance_Mutex_8F3A9B2C", out createdNew);

            if (!createdNew)
            {
                // 既に起動している場合
                SystemSounds.Asterisk.Play();

                // アプリケーションを終了
                this.Shutdown();
                return;
            }

            // 起動時クリーンアップを実行（前回の実行時の残留ファイル等を削除）
            CleanupService.PerformStartupCleanup();

            //// 起動時警告メッセージを表示（教育目的）
            //// Windowsシステム警告音を再生
            //SystemSounds.Exclamation.Play();

            //var warningResult = MessageBox.Show(
            //    "認識されないアプリの実行を確認しました。\n" +
            //    "このソフトウェアを実行すると、PCが危険にさらされる可能性があります。\n" +
            //    "実行しますか？",
            //    "SELLCT",
            //    MessageBoxButton.YesNo,
            //    MessageBoxImage.Warning,
            //    MessageBoxResult.No
            //);

            //// ユーザーが「いいえ」を選択した場合、アプリケーションを完全に終了
            //if (warningResult != MessageBoxResult.Yes)
            //{
            //    // Mutexを解放
            //    _mutex?.ReleaseMutex();
            //    _mutex?.Dispose();

            //    // アプリケーションを完全に終了
            //    Environment.Exit(0);
            //    return;
            //}

            // ComponentManagerを初期化（ファイル監視・パズルシステム開始）
            _componentManager = new ComponentManager(_eventDispatcher);

            // メインウィンドウの作成（表示タイミングはMainWindow側で制御）
            var mainWindow = new MainWindow();
            System.Windows.Application.Current.MainWindow = mainWindow;
        }

        /// <summary>
        /// アプリケーション終了時の処理
        /// リソースの解放とクリーンアップを実行してシステムを元の状態に復元
        /// </summary>
        /// <param name="e">終了イベント引数</param>
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // リソースを初期化したインスタンスのみクリーンアップを実行
                // 二重起動時、2つ目のインスタンスは_componentManagerがnullのためクリーンアップをスキップ
                if (_componentManager != null)
                {
                    // 終了時クリーンアップを実行（レジストリエントリ削除、一時ファイル削除等）
                    CleanupService.PerformExitCleanup();

                    // ComponentManagerリソースの解放（ファイル監視停止、イベントハンドラー解除）
                    _componentManager.Dispose();
                }

                // Mutexは常にクリーンアップ
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                    _mutex.Dispose();
                }
            }
            catch (Exception ex)
            {
                // クリーンアップ中のエラーはログ出力のみ（アプリ終了を妨げない）
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }

            base.OnExit(e);
        }
    }
}