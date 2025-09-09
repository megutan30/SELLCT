using System;
using Microsoft.Win32;
using System.Diagnostics;

namespace SELLCT.Infrastructure.Services
{
    /// <summary>
    /// Windows隠しファイル表示設定を管理するサービス
    /// </summary>
    public class HiddenFileSettingService
    {
        private const string REGISTRY_KEY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private const string HIDDEN_FILES_VALUE = "Hidden";
        private const string SHOW_SYSTEM_FILES_VALUE = "ShowSuperHidden";
        
        private int? _originalHiddenValue = null;
        private int? _originalShowSystemValue = null;
        private bool _settingsModified = false;

        /// <summary>
        /// 現在の隠しファイル表示設定を取得
        /// </summary>
        public bool GetCurrentHiddenFilesSetting()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(HIDDEN_FILES_VALUE);
                        return value != null && (int)value == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HiddenFileSettingService] Error getting current setting: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 隠しファイル表示を無効にする（ゲーム開始時に実行）
        /// </summary>
        public bool DisableHiddenFileDisplay()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    if (key != null)
                    {
                        // 現在の設定を保存（復元用）
                        if (!_settingsModified)
                        {
                            var hiddenValue = key.GetValue(HIDDEN_FILES_VALUE);
                            var systemValue = key.GetValue(SHOW_SYSTEM_FILES_VALUE);
                            
                            _originalHiddenValue = hiddenValue as int?;
                            _originalShowSystemValue = systemValue as int?;
                            
                            Debug.WriteLine($"[HiddenFileSettingService] Original Hidden: {_originalHiddenValue}, System: {_originalShowSystemValue}");
                        }

                        // 隠しファイルを非表示に設定
                        key.SetValue(HIDDEN_FILES_VALUE, 2, RegistryValueKind.DWord); // 2 = 隠しファイルを表示しない
                        key.SetValue(SHOW_SYSTEM_FILES_VALUE, 0, RegistryValueKind.DWord); // 0 = システムファイルを表示しない

                        _settingsModified = true;
                        
                        // Explorerをリフレッシュして設定を反映
                        RefreshExplorer();
                        
                        Debug.WriteLine("[HiddenFileSettingService] Hidden file display disabled");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HiddenFileSettingService] Error disabling hidden file display: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 隠しファイル表示設定を元に戻す（ゲーム終了時に実行）
        /// </summary>
        public bool RestoreHiddenFileDisplay()
        {
            if (!_settingsModified)
            {
                Debug.WriteLine("[HiddenFileSettingService] No settings to restore");
                return true;
            }

            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    if (key != null)
                    {
                        // 元の設定に戻す
                        if (_originalHiddenValue.HasValue)
                        {
                            key.SetValue(HIDDEN_FILES_VALUE, _originalHiddenValue.Value, RegistryValueKind.DWord);
                        }
                        else
                        {
                            // 元々値が存在しなかった場合はデフォルト値を設定
                            key.SetValue(HIDDEN_FILES_VALUE, 2, RegistryValueKind.DWord);
                        }

                        if (_originalShowSystemValue.HasValue)
                        {
                            key.SetValue(SHOW_SYSTEM_FILES_VALUE, _originalShowSystemValue.Value, RegistryValueKind.DWord);
                        }
                        else
                        {
                            key.SetValue(SHOW_SYSTEM_FILES_VALUE, 0, RegistryValueKind.DWord);
                        }

                        _settingsModified = false;
                        
                        // Explorerをリフレッシュして設定を反映
                        RefreshExplorer();
                        
                        Debug.WriteLine("[HiddenFileSettingService] Hidden file display settings restored");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HiddenFileSettingService] Error restoring hidden file display: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Explorerをリフレッシュして設定変更を反映
        /// </summary>
        private void RefreshExplorer()
        {
            try
            {
                // Explorer設定の変更を通知
                const int HWND_BROADCAST = 0xffff;
                const int WM_SETTINGCHANGE = 0x001a;
                
                // P/Invoke宣言
                [System.Runtime.InteropServices.DllImport("user32.dll")]
                static extern int SendMessage(int hWnd, int wMsg, int wParam, string lParam);
                
                SendMessage(HWND_BROADCAST, WM_SETTINGCHANGE, 0, "ShellState");
                
                Debug.WriteLine("[HiddenFileSettingService] Explorer refreshed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HiddenFileSettingService] Error refreshing Explorer: {ex.Message}");
            }
        }

        /// <summary>
        /// 緊急復旧用：強制的に隠しファイル表示を有効にする
        /// </summary>
        public bool ForceEnableHiddenFileDisplay()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    if (key != null)
                    {
                        key.SetValue(HIDDEN_FILES_VALUE, 1, RegistryValueKind.DWord); // 1 = 隠しファイルを表示する
                        key.SetValue(SHOW_SYSTEM_FILES_VALUE, 1, RegistryValueKind.DWord); // 1 = システムファイルを表示する

                        RefreshExplorer();
                        
                        Debug.WriteLine("[HiddenFileSettingService] Hidden file display force enabled");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HiddenFileSettingService] Error force enabling hidden file display: {ex.Message}");
            }
            return false;
        }
    }
}