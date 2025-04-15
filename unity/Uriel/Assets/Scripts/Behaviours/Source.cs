using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class SourceHolder : MonoBehaviour
    {
        public int frequency = 1;
        public float amplitude = 1;

        public Source GetSource()
        {
            var pos = transform.position;
            return new Source()
            {

                position = new Vec2()
                {
                    x = pos.x,
                    y = pos.y
                },
                frequency = frequency,
                amplitude = amplitude
            };
        }
    }
}