using System;
using System.IO;
using System.Windows;
using SELLCT.Services;
using SELLCT.Views;

namespace SELLCT
{
    public partial class App : Application
    {
        private ComponentManager _componentManager;

        public App()
        {
            // グローバルな例外ハンドラーを設定
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            base.OnStartup(e);

            // メインウィンドウの表示
            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
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