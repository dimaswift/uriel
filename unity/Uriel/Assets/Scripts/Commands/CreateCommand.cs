using System.Collections.Generic;
using Uriel.Behaviours;

namespace Uriel.Commands
{
    public class CreateCommand : StudioCommand
    {
        private readonly List<ISnapshot> snapshots;
        private readonly ISnapshot snapshot;

        public CreateCommand(Studio studio, IEnumerable<ISnapshot> snapshots) : base(studio)
        {
            this.snapshots = new List<ISnapshot>(snapshots);
        }
        
        public CreateCommand(Studio studio, ISnapshot snapshot) : base(studio)
        {
            this.snapshot = snapshot;
        }

        public override void Execute()
        {
            if (snapshot != null)
            {
                Studio.Add(snapshot);
                return;
            }
            foreach (var s in snapshots)
            { 
                Studio.Add(s);
            }
        }
        
        public override void Undo()
        {
            if (snapshot != null)
            {
                Studio.Remove(snapshot.ID);
                return;
            }
            foreach (var s in snapshots)
            { 
                Studio.Remove(s.ID);
            }
        }
    }
}