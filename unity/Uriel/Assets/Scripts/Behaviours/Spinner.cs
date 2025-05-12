using UnityEngine;

namespace Uriel.Behaviours
{
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        
        private void Update()
        {
            transform.Rotate(Vector3.up * Time.deltaTime * speed);
        }
    }

}
