using System.Collections.Generic;
using UnityEngine;
using Uriel.Behaviours;

namespace Uriel.Commands
{
    public class ScaleCommand : ICommand
    {
        private readonly List<IScalable> list = new();
        private readonly List<Vector3> oldScales = new();
        private readonly List<Vector3> newScales = new();
        
        public ScaleCommand(IEnumerable<IScalable> movables)
        {
            list.AddRange(movables);
            foreach (var movable in list)
            {
                oldScales.Add(movable.Scale);
            }
        }
        
        public void Execute()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Scale = newScales[i];
            }
        }

        public void Undo()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Scale = oldScales[i];
            }
        }

        public void SaveModified()
        {
            newScales.Clear();
            foreach (var movable in list)
            {
                newScales.Add(movable.Scale);
            }
        }
    }
}