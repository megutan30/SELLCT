using System.Collections.Generic;

namespace SELLCT.Models
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
            ShowChoice,
            ChangeNoButtonContent,
            EnableNoFunction
        }

        public ActionType Type { get; set; }
        public string Message { get; set; }
        public string TargetComponent { get; set; } // For ChangeMainButtonContent, RevealHiddenItem
        public string NewContent { get; set; } // For ChangeMainButtonContent
        public string HiddenItemFolder { get; set; } // For RevealHiddenItem
        public string HiddenItemDisplayName { get; set; } // For RevealHiddenItem

        public List<PuzzleAction> YesActions { get; set; } // For ShowChoice
        public List<PuzzleAction> NoActions { get; set; } // For ShowChoice
    }
}