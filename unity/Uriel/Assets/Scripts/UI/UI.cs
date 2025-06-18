using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Uriel.Behaviours;
using Uriel.Utils;

namespace Uriel.UI
{
    public class UI : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField]  private VolumeStudio studio;
        
        private static UI instance;

        private ProgressBar progressBar;
        private Button exportButton;
        private SettingsPanel settings;
        private StateManagePanel stateManager;
        
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
            
            stateManager.Hide();
            settings.Hide();
            
            studio.OnExportFinished += OnExportFinished;
            studio.OnExportStarted += OnExportStarted;
            studio.OnExportProgressChanged += OnExportProgressChanged;
            
            document.rootVisualElement.RegisterCallback<PointerEnterEvent>(evt => studio.Selector.Enabled = false);
            document.rootVisualElement.RegisterCallback<PointerLeaveEvent>(evt => studio.Selector.Enabled = true);
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
                studio.CreateDefaultEmitter();
            });
            
            buttons.Q<Button>("CreateVolume").RegisterCallback<ClickEvent>(_ =>
            {
                studio.CreateDefaultVolume();
            });
            
            buttons.Q<Button>("Export").RegisterCallback<ClickEvent>(_ =>
            {
                studio.ExportSelectedMesh();
            });
        }
    }
}