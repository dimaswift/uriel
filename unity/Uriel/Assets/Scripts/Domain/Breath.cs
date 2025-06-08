using System.Collections.Generic;
using UnityEngine;

namespace Uriel.Domain
{
    [CreateAssetMenu(menuName = "Uriel/Breath")]
    public class Breath : SerializableBuffer<Modulation>
    {
        [SerializeField] private float rate = 1f;
        [SerializeField] private List<Modulation> mods = new();

        protected override List<Modulation> GetData()
        {
            return mods;
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < mods.Count; i++)
            {
                Modulation mod = mods[i];
                mod.time += dt * rate;
                mods[i] = mod;
            }
        }

        public override int GetBufferHash()
        {
            var h = 0;
            foreach (var mod in mods)
            {
                h += mod.GetHashCode();
            }

            return h;
        }
    }
}