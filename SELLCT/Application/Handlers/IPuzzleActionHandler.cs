using SELLCT.Models;

namespace SELLCT.Application.Handlers
{
    public interface IPuzzleActionHandler
    {
        void HandleAction(PuzzleAction action);
    }
}
