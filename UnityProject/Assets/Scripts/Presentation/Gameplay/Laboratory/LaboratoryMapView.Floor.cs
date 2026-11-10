using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryMapView
    {
        // Owned by this map view, below the existing selection/placement/status graphics.
        private RectTransform floorLayer;
        private readonly List<Image> floorCells = new List<Image>();

        private bool DrawFloor(float size, Vector2 origin)
        {
            var assets = view.UiAssets;
            var available = assets?.GetFloorTile(0) != null;
            if (floorLayer != null) floorLayer.gameObject.SetActive(available);
            if (!available) return false;

            if (floorLayer == null)
            {
                floorLayer = new GameObject("LaboratoryFloor", typeof(RectTransform)).GetComponent<RectTransform>();
                floorLayer.SetParent(transform, false);
                floorLayer.SetAsFirstSibling();
                floorLayer.anchorMin = Vector2.zero;
                floorLayer.anchorMax = Vector2.one;
                floorLayer.offsetMin = floorLayer.offsetMax = Vector2.zero;
            }
            while (floorCells.Count < width * height)
            {
                var image = new GameObject("FloorCell", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                image.transform.SetParent(floorLayer, false);
                image.raycastTarget = false;
                image.color = Color.white;
                floorCells.Add(image);
            }
            for (var index = 0; index < floorCells.Count; index++)
            {
                var image = floorCells[index];
                var active = index < width * height;
                image.gameObject.SetActive(active);
                if (!active) continue;
                var cell = new Vector2Int(index % width, index / width);
                image.sprite = assets.GetFloorTile(FloorVariant(cell));
                var rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                rect.anchoredPosition = origin - viewport.rect.min + (Vector2)cell * size;
                rect.sizeDelta = Vector2.one * size; // No gutter between adjacent floor tiles.
            }
            return true;
        }

        private static int FloorVariant(Vector2Int cell)
        {
            // Stable coordinate hash: no session seed, Unity random state, or saved state.
            uint hash;
            unchecked
            {
                hash = (uint)cell.x * 0x9E3779B9u ^ (uint)cell.y * 0x85EBCA6Bu ^ 0xC2B2AE35u;
                hash ^= hash >> 16;
                hash *= 0x7FEB352Du;
                hash ^= hash >> 15;
                hash *= 0x846CA68Bu;
                hash ^= hash >> 16;
            }
            var bucket = hash % 100;
            return bucket < 70 ? 0 : bucket < 80 ? 1 : bucket < 90 ? 2 : 3;
        }
    }
}
