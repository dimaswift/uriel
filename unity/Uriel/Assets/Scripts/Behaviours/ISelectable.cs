using UnityEngine;

namespace Uriel.Behaviours
{
    public interface ISelectable
    {
        string ID { get; }
        bool Selected { get; set; }
        Bounds Bounds { get; }
        void SetState(SelectableState state);
    }
}