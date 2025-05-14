using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class BreathBuffer : SerializableBufferHandler<Breath>
    {

        protected override void OnBeforeUpdate()
        {
            Buffer.Tick(Time.deltaTime);
        }
    }
}