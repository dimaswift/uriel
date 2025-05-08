using System;
using UnityEngine;
using Uriel.Domain;

namespace Uriel.Behaviours
{
    [ExecuteInEditMode]
    public class PhotonMaterialLinker : MonoBehaviour
    {
        [SerializeField] private bool useTransform;
        [SerializeField] private Sky sky;
        [SerializeField] private Material mat;
        
        private void OnEnable()
        {
            var b = gameObject.GetComponent<PhotonBuffer>();
            if (b == null)
            {
                b = gameObject.AddComponent<PhotonBuffer>();
            }

            b.UseTransform = useTransform;
            b.Init(sky).LinkMaterial(mat);
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            OnEnable();
        }
#endif
    }
} 