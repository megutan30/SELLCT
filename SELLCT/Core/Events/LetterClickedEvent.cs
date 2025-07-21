namespace SELLCT.Core.Events
{
    public class LetterClickedEvent
    {
        public int LetterIndex { get; }

        public LetterClickedEvent(int letterIndex)
        {
            LetterIndex = letterIndex;
        }
    }
}