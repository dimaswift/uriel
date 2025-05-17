using UnityEngine;

namespace Uriel.Behaviours
{
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private AnimationCurve curve;
        private void Update()
        {
            transform.localEulerAngles = new Vector3(0, 0, curve.Evaluate(Time.time * speed));
        }
    }

}
