using System.Windows;
using Application = System.Windows.Application;
using System;
using System.IO;
using SELLCT.Infrastructure.Services;
using SELLCT.Views;
using SELLCT.Core.Interfaces;

namespace SELLCT
{
    public partial class App
    {
        private ComponentManager _componentManager;
        private IEventDispatcher _eventDispatcher;

        public App()
        {
            // グローバルな例外ハンドラーを設定
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // EventDispatcherを初期化
            _eventDispatcher = new EventDispatcher();
        }

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

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"UIスレッドで予期せぬエラーが発生しました: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "SELLCT - エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            // 例外を処理済みとしてマークし、アプリケーションがクラッシュしないようにする
            e.Handled = true;
        }

        public ComponentManager GetComponentManager()
        {
            return _componentManager;
        }

        public IEventDispatcher GetEventDispatcher()
        {
            return _eventDispatcher;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 起動時警告メッセージを表示
            var warningResult = MessageBox.Show(
                "認識されないアプリの実行を確認しました。\n" +
                "このソフトウェアを実行すると、PCが危険にさらされる可能性があります。\n" +
                "実行しますか？",
                "SELLCT",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
            );

            // ユーザーが「いいえ」を選択した場合、アプリケーションを完全に終了
            if (warningResult != MessageBoxResult.Yes)
            {
                // アプリケーションを完全に終了
                Environment.Exit(0);
                return;
            }

            // ComponentManagerを初期化
            _componentManager = new ComponentManager(_eventDispatcher);

            // メインウィンドウの表示
            var mainWindow = new MainWindow();
            System.Windows.Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _componentManager?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }

            base.OnExit(e);
        }
    }
}