using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class BreathBuffer : SerializableBufferHandler<Breath>
    {
        [SerializeField] private float rate = 1f;

        protected override void OnBeforeUpdate()
        {
            Buffer.Tick(Time.deltaTime * rate);
        }
    }
}