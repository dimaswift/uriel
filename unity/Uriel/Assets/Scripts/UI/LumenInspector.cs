using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Domain;

namespace Uriel.UI
{
    [RequireComponent(typeof(UIDocument), typeof(PhotonBuffer))]
    public class LumenInspector : MonoBehaviour
    {
        [SerializeField] private List<Material> materials;
        [SerializeField] private Lumen defaultSelection;
        // UI Elements
        private DropdownField lumenDropdown;
        private UIDocument uiDocument;
        private DropdownField photonDropdown;
        private EnumField solidField;
        private SliderInt iterationsSlider;
        private Slider frequencySlider;
        private Slider amplitudeSlider;
        private Slider phaseSlider;
        private Slider radiusSlider;
        private Slider densitySlider;
        private Slider scaleSlider;
        
        // State
        private int currentPhotonIndex = 0;
        private string currentLumenName;
        private bool isUpdating = false; // Prevent recursive updates
      
        private Dictionary<string, Lumen> lumensDict;

        private Lumen Lumen => lumensDict[currentLumenName];

        private PhotonBuffer buffer;
        
        void Awake()
        {
            buffer = GetComponent<PhotonBuffer>();
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument or Lumen data is not assigned!");
                return;
            }

            var root = uiDocument.rootVisualElement;

            CacheUIElements(root);

            RegisterCallbacks();
            
            LoadLumens();

            uiDocument.rootVisualElement.visible = false;
        }

        void LoadLumens()
        {
            
            var lumens = Resources.LoadAll<Lumen>("Lumens");
            lumensDict = new Dictionary<string, Lumen>();
            lumenDropdown.choices.Clear();

            foreach (Lumen l in lumens)
            {
                if (lumensDict.TryAdd(l.name, l))
                {
                    lumenDropdown.choices.Add(l.name);
                }
            }

            if (defaultSelection && lumensDict.ContainsKey(defaultSelection.name))
            {
                lumenDropdown.value = defaultSelection.name;
            }
            else if (lumens.Length > 0)
            {
                lumenDropdown.value = lumens[0].name;
            }
            
            
            
        }
        
        void SelectLumen(string lumenName)
        {
            if (!lumensDict.ContainsKey(lumenName))
            {
                return;
            }
            currentLumenName = lumenName;
            
            RefreshPhotonDropdown();
            
            if (Lumen.photons.Count > 0)
            {
                SelectPhoton(0);
            }
            
            buffer.SetBuffer(lumensDict[lumenName]);
            
            foreach (Material material in materials)
            {
                buffer.LinkMaterial(material);
            }
        }
        
        
        void CacheUIElements(VisualElement root)
        {
            lumenDropdown = root.Q<DropdownField>("Lumen");
            photonDropdown = root.Q<DropdownField>("Photon");
            solidField = root.Q<EnumField>("Solid");
            iterationsSlider = root.Q<SliderInt>("Iterations");
            frequencySlider = root.Q<Slider>("Frequency");
            amplitudeSlider = root.Q<Slider>("Amplitude");
            phaseSlider = root.Q<Slider>("Phase");
            radiusSlider = root.Q<Slider>("Radius");
            densitySlider = root.Q<Slider>("Density");
            scaleSlider = root.Q<Slider>("Scale");
            
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                uiDocument.rootVisualElement.visible = !uiDocument.rootVisualElement.visible;
            }
        }

        void RegisterCallbacks()
        {
            lumenDropdown.RegisterValueChangedCallback(OnLumenChanged);
            photonDropdown.RegisterValueChangedCallback(OnPhotonChanged);
            
            photonDropdown.Q<Button>("Delete").RegisterCallback<ClickEvent>(evt =>
            {
                RemoveCurrentPhoton();
            });

            photonDropdown.Q<Button>("Add").RegisterCallback<ClickEvent>(evt =>
            {
                AddNewPhoton();
            });

        
            // Value change callbacks
            solidField.RegisterValueChangedCallback(evt => UpdatePhotonValue(() => {
                var photon = Lumen.photons[currentPhotonIndex];
                photon.type = (Solid)evt.newValue;
                Lumen.photons[currentPhotonIndex] = photon;
            }));
            
            iterationsSlider.RegisterValueChangedCallback(evt => UpdatePhotonValue(() => {
                var photon = Lumen.photons[currentPhotonIndex];
                photon.iterations = (uint)evt.newValue;
                Lumen.photons[currentPhotonIndex] = photon;
            }));
            
            frequencySlider.RegisterValueChangedCallback(evt => UpdatePhotonValue(() => {
                var photon = Lumen.photons[currentPhotonIndex];
                photon.frequency = Mathf.RoundToInt(evt.newValue);
                Lumen.photons[currentPhotonIndex] = photon;
            }));
            
            amplitudeSlider.RegisterValueChangedCallback(evt => UpdatePhotonValue(() => {
                var photon = Lumen.photons[currentPhotonIndex];
                photon.amplitude = evt.newValue;
                Lumen.photons[currentPhotonIndex] = photon;
            }));
            
            phaseSlider.RegisterValueChangedCallback(evt => UpdatePhotonValue(() => {
                var photon = Lumen.photons[currentPhotonIndex];
                photon.phase = evt.newValue;
                Lumen.photons[currentPhotonIndex] = photon;
            }));
            
            radiusSlider.RegisterValueChangedCallback(evt => UpdatePhotonValue(() => {
                var photon = Lumen.photons[currentPhotonIndex];
                photon.radius = evt.newValue;
                Lumen.photons[currentPhotonIndex] = photon;
            }));
            
            densitySlider.RegisterValueChangedCallback(evt => UpdatePhotonValue(() => {
                var photon = Lumen.photons[currentPhotonIndex];
                photon.density = evt.newValue;
                Lumen.photons[currentPhotonIndex] = photon;
            }));
            
            scaleSlider.RegisterValueChangedCallback(evt => UpdatePhotonValue(() => {
                var photon = Lumen.photons[currentPhotonIndex];
                photon.scale = evt.newValue;
                Lumen.photons[currentPhotonIndex] = photon;
            }));
        }

        private void OnLumenChanged(ChangeEvent<string> evt)
        {
            SelectLumen(evt.newValue);
        }

        private void OnPhotonChanged(ChangeEvent<string> evt)
        {
            if (int.TryParse(evt.newValue, out var index))
            {
                SelectPhoton(index);
            }
        }
        
        
        void RefreshPhotonDropdown()
        {
            
            if (photonDropdown == null) return;
            
            // Store current selection
            int previousSelection = currentPhotonIndex;
            
            // Clear existing tabs
            photonDropdown.choices.Clear();
            
            // Create tabs for each photon
            for (int i = 0; i < Lumen.photons.Count; i++)
            {
                photonDropdown.choices.Add($"{i}");
            }
            
            // Restore selection if valid
            if (previousSelection < Lumen.photons.Count)
            {
                photonDropdown.value = previousSelection.ToString();
            }
        }
        
        
        void SelectPhoton(int index)
        {
            if (index < 0 || index >= Lumen.photons.Count) return;
            
            currentPhotonIndex = index;
         
            UpdateUIFromPhoton(Lumen.photons[index]);
            
        }
        
        void UpdateUIFromPhoton(Photon photon)
        {
         
            isUpdating = true;
            
            // Update all UI elements with photon values
            if (solidField != null) solidField.value = photon.type;
            if (iterationsSlider != null) iterationsSlider.value = (int)photon.iterations;
            if (frequencySlider != null) frequencySlider.value = photon.frequency;
            if (amplitudeSlider != null) amplitudeSlider.value = photon.amplitude;
            if (phaseSlider != null) phaseSlider.value = photon.phase;
            if (radiusSlider != null) radiusSlider.value = photon.radius;
            if (densitySlider != null) densitySlider.value = photon.density;
            if (scaleSlider != null) scaleSlider.value = photon.scale;
            
            isUpdating = false;
        }
        
        void UpdatePhotonValue(System.Action updateAction)
        {
            if (isUpdating) return; // Prevent recursive updates
            if (currentPhotonIndex < 0 || currentPhotonIndex >= Lumen.photons.Count) return;
            
            updateAction?.Invoke();
            
            // Mark the scriptable object as dirty if in editor
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Lumen);
            #endif
        }
        
        void AddNewPhoton()
        {
            // Create a new photon with default values
            var newPhoton = Lumen.photons[currentPhotonIndex];
            
            Lumen.photons.Add(newPhoton);
            
            // Refresh tabs to show the new photon
            RefreshPhotonDropdown();
            
            
            
            // Mark dirty for editor
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Lumen);
            #endif
        }
        
        // Public methods for external control
        public void RemoveCurrentPhoton()
        {
            if (currentPhotonIndex >= 0 && currentPhotonIndex < Lumen.photons.Count)
            {
                Lumen.photons.RemoveAt(currentPhotonIndex);
                RefreshPhotonDropdown();
                
                // Select previous photon or first one
                int newIndex = Mathf.Max(0, currentPhotonIndex - 1);
                if (Lumen.photons.Count > 0)
                {
                    SelectPhoton(newIndex);
                    photonDropdown.value = newIndex.ToString();
                }
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Lumen);
                #endif
            }
        }
    }

}

