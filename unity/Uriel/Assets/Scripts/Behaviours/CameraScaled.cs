using UnityEngine;

namespace Uriel.Behaviours
{
    public class CameraScaled : MonoBehaviour
    {
        [SerializeField] private float size = 1f;
        private Camera cam;
        private void Update()
        {
            var parentScale = transform.parent.localScale;
            var scale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f / parentScale.z);
            if (cam.orthographic)
            {
                transform.localScale = scale * size * cam.orthographicSize;
            }
            else
            {
                transform.localScale =
                    Vector3.Distance(transform.position, cam.transform.position) * scale * size;
            }
        }
        
        private void Awake()
        {
            cam = Camera.main;
        }

    }
}