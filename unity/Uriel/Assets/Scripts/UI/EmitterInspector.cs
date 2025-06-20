using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Commands;
using Uriel.Domain;

namespace Uriel.UI
{
    public class EmitterInspector : Inspector<WaveEmitterSnapshot>
    {
        private IntegerField frequencyField;
        private Slider amplitudeField;
        private Slider phaseField;
        private Slider radiusField;
        private Slider scaleField;
        private Vector3IntField resolutionField;
        private IntegerField commonResolutionField;
        
        public EmitterInspector(UIDocument ui, Studio studio) : base("Emitter", studio, ui)
        {
            resolutionField = Root.Q<Vector3IntField>("Resolution");
            commonResolutionField = Root.Q<IntegerField>("ResolutionCommon");
            AddField(resolutionField, false);
            commonResolutionField.RegisterValueChangedCallback(evt =>
            {
                resolutionField.value = new Vector3Int(evt.newValue, evt.newValue, evt.newValue);
            });
        }
        
        protected override void UpdateUI(ISnapshot snapshot)
        {
            var emitter = snapshot as WaveEmitterSnapshot;
            if (emitter == null) return;
            resolutionField.SetValueWithoutNotify(emitter.resolution);
            commonResolutionField.SetValueWithoutNotify(Mathf.Max(emitter.resolution.x, emitter.resolution.y, emitter.resolution.z));
        }

        protected override void OnApplyChanges()
        {
            foreach (var emitter in GetInspected<WaveEmitter>())
            {
                var snapshot = emitter.Current as WaveEmitterSnapshot;
                snapshot.resolution = resolutionField.value;
            }
        }
    }
}