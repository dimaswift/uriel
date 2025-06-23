using System;
using System.Linq;
using UnityEngine;
using Uriel.Utils;

namespace Uriel.Behaviours
{
    [RequireComponent(typeof(Studio))]
    public class VolumeStudio : MonoBehaviour
    {
        public bool ExportInProgress { get; private set; }
        public event Action<int> OnExportProgressChanged = v => { };
        public event Action OnExportStarted = () => { };
        public event Action OnExportFinished = ()  => { };

        private Studio studio;
        
        private void Awake()
        {
            studio = GetComponent<Studio>();
        }

        public async void ExportSelectedMesh()
        {
            if (ExportInProgress)
            {
                return;
            }
            var total = studio.Selector.GetSelectedCount<Volume>();

            if (total == 0)
            {
                return;
            }
            
            OnExportStarted();
            OnExportProgressChanged(0);
            ExportInProgress = true;
            int progress = 0;
          
            foreach (var sel in studio.Selector.GetSelected<Volume>().ToArray())
            {
                try
                {
                    await STLExporter.ExportMeshToSTLAsync(
                        name: Id.Short,
                        mesh: sel.GeneratedMesh,
                        binary: true,
                        optimizeVertices: true
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Export failed: {ex.Message}");
                }
                progress++;
                OnExportProgressChanged(Mathf.RoundToInt(((float) progress / total) * 100));
            }
            OnExportFinished();
            ExportInProgress = false;
        }
    }
}