using MobileGame.Foundation;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Diagnostic dependencies point to the shared bootstrap, never the reverse.
    public sealed class DiagnosticSceneEntry : MonoBehaviour, IGameSceneEntry
    {
        public string InitializationError { get; private set; }
        public bool InitializeSession(GameDataCatalog data, string path, double interval, SessionStart start)
        {
            var session = GetComponent<SessionFoundationView>();
            var map = GetComponent<MapFoundationView>();
            if (session != null)
            {
                if (session.Controller == null) session.Initialize(data.Selected<FacilityData>("storageId"),
                    data.Selected<CreatureData>("prototypeId"), data.Selected<ResourceData>("organicId"), path, interval);
                InitializationError = session.Controller.LastError;
                return session.Controller.Current != null;
            }
            if (map != null)
            {
                if (map.Session == null) map.Initialize(data.Selected<FacilityData>("storageId"),
                    data.Selected<CreatureData>("prototypeId"), data.Selected<ResourceData>("organicId"));
                return true;
            }
            InitializationError = "Missing diagnostic view."; return false;
        }
    }
}
