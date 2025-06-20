using Uriel.Behaviours;

namespace Uriel.Commands
{
    public abstract class StudioCommand : ICommand
    {
        protected Studio Studio;
        
        public StudioCommand(Studio studio)
        {
            Studio = studio;
        }
        public abstract void Execute();

        public abstract void Undo();
    }
}