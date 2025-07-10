using System.Collections.Generic;

namespace SELLCT.Models
{
    public class PuzzleDefinition
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public PuzzleTrigger Trigger { get; set; }
        public List<PuzzleAction> Actions { get; set; }
        public bool IsCompleted { get; set; }
    }
}