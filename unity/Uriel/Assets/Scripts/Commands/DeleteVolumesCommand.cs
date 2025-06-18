using Uriel.Behaviours;
using Uriel.Domain;

namespace Uriel.Commands
{
    public class DeleteVolumesCommand : StudioCommand
    {
        private readonly string[] ids;
        private readonly VolumeSnapshot[] snapshots;
        
        public DeleteVolumesCommand(VolumeStudio studio, params Volume[] volumes) : base(studio)
        {
            ids = new string [volumes.Length];
            snapshots = new VolumeSnapshot[volumes.Length];
            for (int i = 0; i < volumes.Length; i++)
            {
                ids[i] = volumes[i].ID;
                snapshots[i] = volumes[i].CreateSnapshot();
            }
        }
        public override void Execute()
        {
            foreach (var id in ids)
            {
                Studio.RemoveVolume(id);
            }
        }

        public override void Undo()
        {
            foreach (var snapshot in snapshots)
            {
                Studio.AddVolume(snapshot);
            }
        }
    }
}