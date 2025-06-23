using System;
using System.Linq;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Commands;
using Uriel.Domain;

namespace Uriel.UI
{
    public class SolidInspector : Inspector<SculptSolidSnapshot>
    {
        private readonly Slider scaleField;
        private readonly Slider lerpField;
        private readonly Slider featherField;
        private readonly Slider expField;
        private readonly DropdownField typeField;
        private readonly DropdownField operationField;
        
        public SolidInspector(UIDocument ui, Studio studio) : base("Solid", studio, ui)
        {
            typeField = RegisterField<DropdownField, string>("Type", true);
            scaleField = RegisterField<Slider, float>("Scale", true);
            lerpField = RegisterField<Slider, float>("Lerp", true);
            expField = RegisterField<Slider, float>("Exp", true);
            featherField = RegisterField<Slider, float>("Feather", true);
            operationField = RegisterField<DropdownField, string>("Operation", true);
            operationField.choices = Enum.GetNames(typeof(SculptOperation)).ToList();
            typeField.choices = Enum.GetNames(typeof(SculptSolidType)).ToList();
        }
        
        protected override void UpdateUI(ISnapshot snapshot)
        {
            var solid = snapshot as SculptSolidSnapshot;
            if (solid == null) return;

            if (IsModified)
            {
                return;
            }
            
            scaleField.SetValueWithoutNotify(solid.solid.scale);
            typeField.SetValueWithoutNotify(solid.solid.type.ToString());
            operationField.SetValueWithoutNotify(solid.solid.op.ToString());
            featherField.SetValueWithoutNotify(solid.solid.feather);
            lerpField.SetValueWithoutNotify(solid.solid.lerp);
            expField.SetValueWithoutNotify(solid.solid.exp);
        }

        protected override void OnApplyChanges()
        {
            foreach (var sculpt in GetInspected<SculptSolidBehaviour>())
            {
                var snapshot = sculpt.Current as SculptSolidSnapshot;
                if (snapshot == null) continue;

                var solid = snapshot.solid;
                solid.scale = scaleField.value;
                solid.type = Enum.Parse<SculptSolidType>(typeField.value);
                solid.op = Enum.Parse<SculptOperation>(operationField.value);
                solid.feather = featherField.value;
                solid.lerp = lerpField.value;
                solid.exp = expField.value;
                snapshot.solid = solid;
            }
        }
    }
}