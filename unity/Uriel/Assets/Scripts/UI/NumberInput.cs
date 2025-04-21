using System;
using TMPro;
using UnityEngine;

namespace Uriel.UI
{
    public class NumberInput : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        public void SetUp(string label, float value, Action<float> onSubmit)
        {
            if (labelText != null)
            {
                labelText.text = label;
            }
            var field = GetComponentInChildren<TMP_InputField>();
            field.text = value.ToString("F");
            field.onSubmit.AddListener(v =>
            {
                if (float.TryParse(v, out var f))
                {
                    onSubmit(f);
                }
            });
        }

        private void OnValidate()
        {
            labelText = transform.Find("Label")?.GetComponent<TMP_Text>();
        }
    }
}
