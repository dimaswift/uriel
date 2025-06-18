using Uriel.Behaviours;
using Uriel.Domain;

namespace Uriel.Commands
{
    public class CreateVolumeCommand : StudioCommand
    {
        private readonly VolumeSnapshot[] sources;

        public CreateVolumeCommand(VolumeStudio studio, params VolumeSnapshot[] sources) : base(studio)
        {
            this.sources = sources;
        }

        public override void Execute()
        {
            foreach (var snapshot in sources)
            { 
                Studio.AddVolume(snapshot);
            }
        }
        
        public override void Undo()
        {
            foreach (var snapshot in sources)
            { 
                Studio.RemoveVolume(snapshot.id);
            }
        }
    }
}