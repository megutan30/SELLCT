namespace SELLCT.Core.Events
{
    public class LetterAppearedEvent
    {
        public int LetterIndex { get; }

        public LetterAppearedEvent(int letterIndex)
        {
            LetterIndex = letterIndex;
        }
    }
}