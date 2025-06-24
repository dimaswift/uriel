using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Commands;
using Uriel.Domain;

namespace Uriel.UI
{
    public class EmitterInspector : Inspector<WaveEmitterSnapshot>
    {
        private SliderInt frequencyField;
        private Slider amplitudeField;
        private Slider phaseField;
        private Slider radiusField;
        private Slider scaleField;
        private readonly Vector3IntField resolutionField;
        private readonly IntegerField commonResolutionField;
        
        public EmitterInspector(UIDocument ui, Studio studio) : base("Emitter", studio, ui)
        {
            resolutionField = RegisterField<Vector3IntField, Vector3Int>("Resolution", false, false);
            frequencyField = RegisterField<SliderInt, int>("Frequency", true, true);
            amplitudeField = RegisterField<Slider, float>("Amplitude", true, true);
            phaseField = RegisterField<Slider, float>("Phase", true, true);
            radiusField = RegisterField<Slider, float>("Radius", true, true);
            scaleField = RegisterField<Slider, float>("Scale", true, true);
            
            commonResolutionField = Root.Q<IntegerField>("ResolutionCommon");
            
            commonResolutionField.RegisterValueChangedCallback(evt =>
            {
                resolutionField.value = new Vector3Int(evt.newValue, evt.newValue, evt.newValue);
            });

            Root.Q<MinMaxSlider>("FrequencyRange").RegisterValueChangedCallback(evt =>
            {
                frequencyField.lowValue = (int) evt.newValue.x;
                frequencyField.highValue = (int) evt.newValue.y;
                frequencyField.SetValueWithoutNotify(Mathf.Clamp(
                    frequencyField.value, 
                    frequencyField.lowValue, 
                    frequencyField.highValue));
            });
        }
        
        protected override void UpdateUI(ISnapshot snapshot)
        {
            var emitter = snapshot as WaveEmitterSnapshot;
            if (emitter == null) return;

            if (IsModified)
            {
                return;
            }

            var source = emitter.sources.FirstOrDefault();
            
            frequencyField.SetValueWithoutNotify(source.frequency);
            amplitudeField.SetValueWithoutNotify(source.amplitude);
            phaseField.SetValueWithoutNotify(source.phase);
            radiusField.SetValueWithoutNotify(source.radius);
            scaleField.SetValueWithoutNotify(source.scale);
            resolutionField.SetValueWithoutNotify(emitter.resolution);
            commonResolutionField.SetValueWithoutNotify(Mathf.Max(emitter.resolution.x, emitter.resolution.y, emitter.resolution.z));
        }

        protected override void OnApplyChanges()
        {
            foreach (var emitter in GetInspected<WaveEmitter>())
            {
                var snapshot = emitter.Current as WaveEmitterSnapshot;
                if (snapshot == null) continue;
                
                snapshot.resolution = resolutionField.value;
                
                for (int i = 0; i < snapshot.sources.Count; i++)
                {
                    var source = snapshot.sources[i];
                    source.frequency = frequencyField.value;
                    source.amplitude = amplitudeField.value;
                    source.scale = scaleField.value;
                    source.phase = phaseField.value;
                    source.radius = radiusField.value;
                    snapshot.sources[i] = source;
                }
            }
        }
    }
}