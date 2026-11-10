using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace MobileGame.Presentation
{
    // Owned by one laboratory screen. No model state, static cache, colliders or individual Update loops.
    public sealed class LaboratoryWorldMap : IDisposable
    {
        public const string LayerName = "LaboratoryWorld";
        public const float UnitsPerCell = 2;
        public Transform Root { get; }
        public Camera Camera { get; }
        public Tilemap Floor { get; }
        public Sprite Solid { get; }
        private readonly Material material;
        private readonly Tile[] tiles = new Tile[4];
        private readonly Vector3[] corners = new Vector3[4];
        private readonly Camera background;
        private readonly int backgroundMask;
        private readonly Color backgroundColor;
        private readonly bool backgroundEnabled;
        private readonly RectTransform outsideFrame;
        private readonly RectTransform[] frameEdges = new RectTransform[4];
        private readonly Canvas canvas;
        private int width, height;

        public LaboratoryWorldMap(Transform owner, Material sharedMaterial, Canvas uiCanvas)
        {
            var layer = LayerMask.NameToLayer(LayerName);
            if (layer < 0 || sharedMaterial == null) throw new InvalidOperationException("Laboratory world assets are not configured.");
            material = sharedMaterial; canvas = uiCanvas;
            outsideFrame = UiWidgets.Rect("LaboratoryViewportFrame", canvas.transform);
            outsideFrame.SetAsFirstSibling();
            outsideFrame.anchorMin = Vector2.zero; outsideFrame.anchorMax = Vector2.one;
            outsideFrame.offsetMin = outsideFrame.offsetMax = Vector2.zero;
            for (var i = 0; i < frameEdges.Length; i++)
            {
                var edge = UiWidgets.Panel("OutsideViewport_" + i, outsideFrame, UiWidgets.Background);
                edge.raycastTarget = false; frameEdges[i] = edge.rectTransform;
                frameEdges[i].anchorMin = frameEdges[i].anchorMax = frameEdges[i].pivot = Vector2.zero;
            }
            Root = new GameObject("LaboratoryWorld").transform;
            Root.SetParent(owner, false);
            Root.gameObject.layer = layer;
            background = UnityEngine.Camera.main;
            if (background != null)
            {
                backgroundMask = background.cullingMask;
                backgroundEnabled = background.enabled;
                backgroundColor = background.backgroundColor;
                background.backgroundColor = UiWidgets.Background;
                background.cullingMask &= ~(1 << layer);
            }
            Camera = new GameObject("LaboratoryWorldCamera", typeof(Camera)).GetComponent<Camera>();
            Camera.transform.SetParent(Root, false);
            Camera.tag = "MainCamera";
            Camera.orthographic = true;
            Camera.nearClipPlane = .1f; Camera.farClipPlane = 50;
            Camera.cullingMask = 1 << layer;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = new Color(.025f, .04f, .065f);
            Camera.depth = background != null ? background.depth + 1 : 1;
            Camera.allowHDR = false; Camera.allowMSAA = false;
            Camera.gameObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(0);
            var grid = new GameObject("LaboratoryGrid", typeof(Grid)).GetComponent<Grid>();
            grid.transform.SetParent(Root, false);
            grid.cellSize = new Vector3(UnitsPerCell, UnitsPerCell, 1);
            Floor = new GameObject("LaboratoryFloor", typeof(Tilemap), typeof(TilemapRenderer)).GetComponent<Tilemap>();
            Floor.transform.SetParent(grid.transform, false); Floor.gameObject.layer = layer;
            Floor.tileAnchor = new Vector3(.5f, .5f, 0);
            var renderer = Floor.GetComponent<TilemapRenderer>();
            renderer.sharedMaterial = material; renderer.sortingOrder = -30000;
            Solid = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
            Solid.name = "Temp_LaboratorySolid";
            for (var i = 0; i < tiles.Length; i++)
            {
                tiles[i] = ScriptableObject.CreateInstance<Tile>();
                tiles[i].name = "LaboratoryFloor_" + i;
                tiles[i].colliderType = Tile.ColliderType.None;
            }
        }

        public SpriteRenderer CreateSprite(string name, Transform parent, int order = 0)
        {
            var sprite = new GameObject(name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            sprite.transform.SetParent(parent, false); sprite.gameObject.layer = Root.gameObject.layer;
            sprite.sharedMaterial = material; sprite.sortingOrder = order;
            return sprite;
        }

        public void UpdateFloor(int columns, int rows, LaboratoryUiAssets assets)
        {
            var changed = width != columns || height != rows;
            for (var i = 0; i < tiles.Length; i++)
            {
                var sprite = assets?.GetFloorTile(i) ?? Solid;
                if (tiles[i].sprite == sprite) continue;
                tiles[i].sprite = sprite;
                tiles[i].color = sprite == Solid ? new Color(.12f, .16f, .21f) : Color.white;
                tiles[i].transform = Matrix4x4.Scale(sprite == Solid ? new Vector3(2, 2, 1) : Vector3.one);
                changed = true;
            }
            if (!changed) return;
            width = columns; height = rows;
            Floor.ClearAllTiles();
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    Floor.SetTile(new Vector3Int(x, y, 0), tiles[FloorVariant(new Vector2Int(x, y))]);
            Floor.RefreshAllTiles();
        }

        public void SyncCamera(RectTransform viewport, Vector2 cellCenter, float cellSize)
        {
            viewport.GetWorldCorners(corners);
            var min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
            var original = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            // Camera projection uses integer pixel dimensions. Quantize the clip, then compensate
            // its center/size so subpixel UI layout still maps to exactly the same world point.
            var clipped = Rect.MinMaxRect(Mathf.Ceil(Mathf.Max(0, min.x)), Mathf.Ceil(Mathf.Max(0, min.y)),
                Mathf.Floor(Mathf.Min(Screen.width, max.x)), Mathf.Floor(Mathf.Min(Screen.height, max.y)));
            var visible = viewport.gameObject.activeInHierarchy && clipped.width > 0 && clipped.height > 0 && viewport.rect.height > 0;
            Camera.enabled = visible;
            outsideFrame.gameObject.SetActive(visible);
            if (background != null) background.enabled = !visible && backgroundEnabled;
            if (!visible) return;
            UpdateOutsideFrame(clipped);
            var pixelsPerUnit = original.height / viewport.rect.height * cellSize / UnitsPerCell;
            Camera.pixelRect = clipped;
            Camera.aspect = clipped.width / clipped.height;
            Camera.orthographicSize = clipped.height / (2 * pixelsPerUnit);
            var worldCenter = CellToWorld(cellCenter) + (clipped.center - original.center) / pixelsPerUnit;
            Camera.transform.position = new Vector3(worldCenter.x, worldCenter.y, -10);
        }

        private void UpdateOutsideFrame(Rect clip)
        {
            SetFrameEdge(0, new Rect(0, 0, clip.xMin, Screen.height));
            SetFrameEdge(1, new Rect(clip.xMax, 0, Screen.width - clip.xMax, Screen.height));
            SetFrameEdge(2, new Rect(clip.xMin, 0, clip.width, clip.yMin));
            SetFrameEdge(3, new Rect(clip.xMin, clip.yMax, clip.width, Screen.height - clip.yMax));
        }
        private void SetFrameEdge(int index, Rect pixels)
        {
            frameEdges[index].anchoredPosition = pixels.position / canvas.scaleFactor;
            frameEdges[index].sizeDelta = pixels.size / canvas.scaleFactor;
        }
        public void SetVisible(bool visible)
        {
            if (Root != null) Root.gameObject.SetActive(visible);
            if (outsideFrame != null) outsideFrame.gameObject.SetActive(visible);
            if (!visible && background != null) background.enabled = backgroundEnabled;
        }

        public static Vector2 CellToWorld(Vector2 cell) => cell * UnitsPerCell;
        public static Vector2 WorldToLocal(Vector2 world, Vector2 origin, float cellSize)
            => origin + world * (cellSize / UnitsPerCell);

        public static int FloorVariant(Vector2Int cell)
        {
            uint hash;
            unchecked
            {
                hash = (uint)cell.x * 0x9E3779B9u ^ (uint)cell.y * 0x85EBCA6Bu ^ 0xC2B2AE35u;
                hash ^= hash >> 16; hash *= 0x7FEB352Du;
                hash ^= hash >> 15; hash *= 0x846CA68Bu; hash ^= hash >> 16;
            }
            var bucket = hash % 100;
            return bucket < 70 ? 0 : bucket < 80 ? 1 : bucket < 90 ? 2 : 3;
        }

        public void Dispose()
        {
            if (background != null) { background.cullingMask = backgroundMask; background.backgroundColor = backgroundColor; background.enabled = backgroundEnabled; }
            if (Root != null) { Root.gameObject.SetActive(false); UnityEngine.Object.Destroy(Root.gameObject); }
            if (outsideFrame != null) UnityEngine.Object.Destroy(outsideFrame.gameObject);
            foreach (var tile in tiles) UnityEngine.Object.Destroy(tile);
            UnityEngine.Object.Destroy(Solid);
        }
    }
}
