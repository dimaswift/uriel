using System.Collections.Generic;
using UnityEngine;
using Uriel.Behaviours;

namespace Uriel.Commands
{
    public class MoveCommand : ICommand
    {
        private readonly List<IMovable> list = new();
        private readonly List<Vector3> oldPositions = new();
        private readonly List<Vector3> newPositions = new();
        
        public MoveCommand(IEnumerable<IMovable> movables)
        {
            list.AddRange(movables);
            foreach (var movable in list)
            {
                oldPositions.Add(movable.position);
            }
        }
        
        public void Execute()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].position = newPositions[i];
            }
        }

        public void Undo()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].position = oldPositions[i];
            }
        }

        public void SaveModified()
        {
            newPositions.Clear();
            foreach (var movable in list)
            {
                newPositions.Add(movable.position);
            }
        }
    }
}