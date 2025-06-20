using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Domain;
using Uriel.Utils;

namespace Uriel.UI
{
    public class UI : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField]  private Studio studio;
        
        private static UI instance;

        private ProgressBar progressBar;
        private Button exportButton;
        private SettingsPanel settings;
        private StateManagePanel stateManager;
        private VolumeInspector volumeInspector;
        private EmitterInspector emitterInspector;
        private CameraControls cameraControls;
        private VolumeStudio volumeStudio;
        
        public static UI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<UI>();
                    if (instance == null)
                    {
                        instance = new GameObject("UI").AddComponent<UI>();
                    }
                }
                return instance;
            }
        }
        
        private void Update()
        {
           
        }

        private void Start()
        {
            BindVolumeStudio();
            progressBar = document.rootVisualElement.Q<ProgressBar>("ProgressBar");
            progressBar.visible = false;
            
            settings = new SettingsPanel(document, studio);
            stateManager = new StateManagePanel(document, studio);
            volumeInspector = new VolumeInspector(document, studio);
            cameraControls = new CameraControls(document);
            emitterInspector = new EmitterInspector(document, studio);
            
            stateManager.Hide();
            settings.Hide();
            volumeInspector.Close();
            emitterInspector.Close();

            volumeStudio = studio.GetComponent<VolumeStudio>();
            volumeStudio.OnExportFinished += OnExportFinished;
            volumeStudio.OnExportStarted += OnExportStarted;
            volumeStudio.OnExportProgressChanged += OnExportProgressChanged;
            
            document.rootVisualElement.RegisterCallback<PointerEnterEvent>(evt => studio.Selector.Enabled = false);
            document.rootVisualElement.RegisterCallback<PointerLeaveEvent>(evt => studio.Selector.Enabled = true);

            studio.Selector.OnSelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            volumeInspector.HandleSelection<Volume>();
            emitterInspector.HandleSelection<WaveEmitter>();
        }
        
        private void OnExportProgressChanged(int progress)
        {
            progressBar.title = $"Exporting... {progress}%";
            progressBar.value = progress;
        }

        private async void OnExportFinished()
        {
            progressBar.title = "Export completed";
            await Task.Delay(3000);
            progressBar.visible = false;
        }

        private void OnExportStarted()
        {
            progressBar.visible = true;
        }

        private void BindVolumeStudio()
        {
            var buttons = document.rootVisualElement.Q<VisualElement>("ToolBar");

            buttons.Q<Button>("CreateEmitter").RegisterCallback<ClickEvent>(_ =>
            {
                var source = new WaveEmitterSnapshot();
                source.resolution = new Vector3Int(128, 128, 128);
                source.saturate = true;
                source.sources = Lattices.Tetrahedron(new WaveSource() {frequency = 10, amplitude = 1, scale = 1, radius = 1}).ToList();
                studio.Create(source);
            });
            
            buttons.Q<Button>("CreateVolume").RegisterCallback<ClickEvent>(_ =>
            {
                studio.Selector.Select(studio.CreateDefault<Volume>().ID);
            });
            
            buttons.Q<Button>("Export").RegisterCallback<ClickEvent>(_ =>
            {
                volumeStudio.ExportSelectedMesh();
            });
        }
    }
}