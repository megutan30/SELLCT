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
            SetTextWindowVisibility,
            TerminateExplorer,
            DisableKeyboardInput,
            DisableMouseInput,
            ResetGame,
            ClearMessageQueue,
            ExitApplication
        }

        public ActionType Type { get; set; }
        public string Message { get; set; }
        public string NewContent { get; set; }
        public string TargetComponent { get; set; }
        public string HiddenItemFolder { get; set; }
        public string HiddenItemDisplayName { get; set; }
        public List<PuzzleAction> YesActions { get; set; }
        public List<PuzzleAction> NoActions { get; set; }
        public List<PuzzleAction> NetherChoiceActions { get; set; }
        
        // 新しい可視性アクションのパラメータ
        public bool IsVisible { get; set; }
    }
}
