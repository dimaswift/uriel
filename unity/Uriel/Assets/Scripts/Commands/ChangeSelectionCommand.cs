using Uriel.Behaviours;

namespace Uriel.Commands
{
    public class ChangeSelectionCommand : ICommand
    {
        private readonly string[] oldSelection;
        private readonly string[] newSelection;

        private readonly Selector selector;

        public ChangeSelectionCommand(Selector selector, string[] oldSelection, string[] newSelection)
        {
            this.selector = selector;
            this.oldSelection = oldSelection;
            this.newSelection = newSelection;
        }
 
        public void Execute()
        {
            foreach (var s in oldSelection)
            {
                selector.Deselect(s);
            }
            foreach (var s in newSelection)
            {
                selector.Select(s);
            }
        } 

        public void Undo()
        {
            foreach (var s in newSelection)
            {
                selector.Deselect(s);
            }
            foreach (var s in oldSelection)
            {
                selector.Select(s);
            }
        }
    }
}