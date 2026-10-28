using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    // Temporary vector artwork: no image asset, no game-state ownership.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LaboratoryUiWorld : MaskableGraphic
    {
        private float progress;
        public LaboratoryUiWorld() { useLegacyMeshGeneration = false; }
        public float Progress { get => progress; set { if (Mathf.Approximately(progress, value)) return; progress = value; SetVerticesDirty(); } }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var rect = rectTransform.rect; var cell = Mathf.Min(rect.width, rect.height) / 23f;
            var center = rect.center;
            for (var y = 0; y < 19; y++)
            for (var x = 0; x < 19; x++)
            {
                var dx = x - 9; var dy = y - 9;
                if (dx * dx + dy * dy > 80) continue;
                var pattern = ((x * 31 + y * 17) % 29) / 29f;
                var burning = progress > pattern * 0.7f;
                var tint = burning ? Color.Lerp(new Color(1f, 0.42f, 0.12f), new Color(0.12f, 0.07f, 0.08f), progress)
                    : pattern < 0.5f ? new Color(0.2f, 0.55f, 0.42f) : new Color(0.13f, 0.3f, 0.55f);
                var scatter = Mathf.Max(0, progress - 0.35f) * 0.22f;
                var origin = center + new Vector2((dx + dx * scatter) * cell, (dy + dy * scatter) * cell);
                var extent = cell * (1 - progress * 0.4f) * 0.5f; var first = mesh.currentVertCount;
                mesh.AddVert(origin + new Vector2(-extent, -extent), tint, Vector2.zero);
                mesh.AddVert(origin + new Vector2(-extent, extent), tint, Vector2.up);
                mesh.AddVert(origin + new Vector2(extent, extent), tint, Vector2.one);
                mesh.AddVert(origin + new Vector2(extent, -extent), tint, Vector2.right);
                mesh.AddTriangle(first, first + 1, first + 2); mesh.AddTriangle(first, first + 2, first + 3);
            }
        }
    }
}
