using System.Collections.Generic;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Utils
{
    public static class Lattices
    {
        public static IEnumerable<WaveSource> Tetrahedron(WaveSource source)
        {
            source.position = new Vector3(0.35355339f, 0.35355339f, 0.35355339f);
            yield return source;
            source.position = new Vector3(0.35355339f, -0.35355339f, -0.35355339f);
            yield return source;
            source.position = new Vector3(-0.35355339f, 0.35355339f, -0.35355339f);
            yield return source;
            source.position = new Vector3(-0.35355339f, -0.35355339f, 0.35355339f);
            yield return source;
        }
    }
}