using Uriel.Behaviours;

namespace Uriel.Commands
{
    public abstract class StudioCommand : ICommand
    {
        protected VolumeStudio Studio;
        
        public StudioCommand(VolumeStudio studio)
        {
            Studio = studio;
        }
        public abstract void Execute();

        public abstract void Undo();
    }
}