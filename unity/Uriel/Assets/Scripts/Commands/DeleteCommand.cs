using System.Collections.Generic;
using Uriel.Behaviours;

namespace Uriel.Commands
{
    public class DeleteCommand : StudioCommand
    {
        private readonly string[] ids;
        private readonly ISnapshot[] snapshots;
        
        public DeleteCommand(Studio studio, IReadOnlyList<IModifiable> modifiables) : base(studio)
        {
            ids = new string [modifiables.Count];
            snapshots = new ISnapshot[modifiables.Count];
            for (int i = 0; i < modifiables.Count; i++)
            {
                ids[i] = modifiables[i].ID;
                snapshots[i] = modifiables[i].CreateSnapshot();
            }
        }
        public override void Execute()
        {
            foreach (var id in ids)
            {
                Studio.Remove(id);
            }
        }

        public override void Undo()
        {
            foreach (var snapshot in snapshots)
            {
                Studio.Add(snapshot);
            }
        }
    }
}