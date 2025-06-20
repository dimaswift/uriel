using UnityEngine.UIElements;
using Uriel.Behaviours;

namespace Uriel.UI
{
    public class StudioPanel : Panel
    {
        protected readonly Studio Studio;
        public StudioPanel(Studio studio, UIDocument ui, string name, string openButton) : base(ui, name, openButton)
        {
            this.Studio = studio;
        }
    }
}