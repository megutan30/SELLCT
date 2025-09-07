using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    public class PuzzleAction
    {
        public enum ActionType
        {
            ShowDialog,
            ChangeMainButtonContent,
            RevealHiddenItem,
            TransitionToPhase2,
            ShowMessageBox,
            ChangeNoButtonContent,
            EnableNoFunction,
            ShowChoice,

            // 指示書からの新しいアクション
            SetMainButtonVisibility,
            SetKeyVisibility,
            SetDoorVisibility,
            SetTextWindowVisibility,
            SetBackgroundVisibility,
            TerminateExplorer,
            DisableKeyboardInput,
            DisableMouseInput,
            EnableKeyboardInput,
            EnableMouseInput,
            ResetGame,
            ClearMessageQueue,
            ExitApplication,
            ExitWithMessageBox,
            DelayedExitWithMessageBox,
            StartExplorer,
            BetrayalEnding,
            ShowPseudoDesktopIcon,
            HidePseudoDesktopIcon,
            UpdatePseudoIconPosition,
            CreateHiddenAuthorityFolder
        }

        public ActionType Type { get; set; }
        public string Message { get; set; }
        public string NewContent { get; set; }
        public string TargetComponent { get; set; }
        public string HiddenItemFolder { get; set; }
        public string HiddenItemDisplayName { get; set; }
        public string IconName { get; set; }
        public string FolderPath { get; set; }
        public double IconX { get; set; }
        public double IconY { get; set; }
        public List<PuzzleAction> YesActions { get; set; }
        public List<PuzzleAction> NoActions { get; set; }
        public List<PuzzleAction> NetherChoiceActions { get; set; }
        
        // 新しい可視性アクションのパラメータ
        public bool IsVisible { get; set; }
        
        // 遅延時間（ミリ秒）
        public int DelayMilliseconds { get; set; } = 3000; // デフォルト3秒
    }
}
