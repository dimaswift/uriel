using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    public class Adam : MonoBehaviour
    {
        [SerializeField] private Lumen lumen;
        [SerializeField] private float speed = 5;
        [SerializeField] private float acc = 5;
        [SerializeField] private float inertia = 1;
        private Vector3 vel;
        
        
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                vel += Vector3.left * acc * Time.deltaTime;

            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                vel += Vector3.right * acc * Time.deltaTime;

            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                vel += Vector3.forward * acc * Time.deltaTime;

            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                vel += Vector3.back * acc * Time.deltaTime;

            }


            if (Input.GetKey(KeyCode.R))
            {
                transform.position = Vector3.zero;
                vel = Vector3.zero;
            }

          
            if (Input.GetKey(KeyCode.P))
            {
                var p = lumen.photons[0];
                p.phase += Time.deltaTime;
                lumen.photons[0] = p;
            }

            if (Input.GetKey(KeyCode.O))
            {
                var p = lumen.photons[0];
                p.phase -= Time.deltaTime;
                lumen.photons[0] = p;
            }
            
            transform.position += vel * speed * Time.deltaTime;

            vel = Vector3.Lerp(vel, Vector3.zero, Time.deltaTime * inertia);
        }
    }

}
