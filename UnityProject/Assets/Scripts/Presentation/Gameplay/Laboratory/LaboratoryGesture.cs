using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobileGame.Presentation
{
    // Screen-space intent only. The view owns hit testing and all game commands.
    public sealed class LaboratoryGesture
    {
        private sealed class Pointer
        {
            public Vector2 Origin;
            public Vector2 Position;
        }

        private readonly Dictionary<int, Pointer> pointers = new Dictionary<int, Pointer>();
        private bool blocked;
        private bool dragging;
        private float dragThreshold = 12;

        public event Action<Vector2> Tapped;
        public event Action<Vector2> Panned;
        public event Action<float, Vector2> Zoomed;
        public bool HasPointers => pointers.Count != 0;

        public float DragThreshold
        {
            get => dragThreshold;
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value) || value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                dragThreshold = value;
            }
        }

        public void Begin(int id, Vector2 position, bool overMap)
        {
            ValidatePosition(position);
            // A second source reporting the same pointer cannot restart a gesture.
            if (pointers.ContainsKey(id)) return;
            if (pointers.Count == 0) { blocked = false; dragging = false; }
            pointers.Add(id, new Pointer { Origin = position, Position = position });
            if (!overMap || pointers.Count > 2) blocked = true;
            if (pointers.Count > 1) dragging = true;
        }

        public void Move(int id, Vector2 position)
        {
            ValidatePosition(position);
            if (!pointers.TryGetValue(id, out var pointer)) return;
            var previous = pointer.Position;
            pointer.Position = position;
            if (blocked || previous == position) return;
            if (pointers.Count == 1)
            {
                var offset = position - pointer.Origin;
                if (!dragging)
                {
                    if (offset.sqrMagnitude < dragThreshold * dragThreshold) return;
                    dragging = true;
                    // Include the distance accumulated while distinguishing a tap.
                    Panned?.Invoke(offset);
                }
                else Panned?.Invoke(position - previous);
                return;
            }

            Pointer other = null;
            foreach (var entry in pointers)
                if (entry.Key != id) { other = entry.Value; break; }
            if (other == null) return;
            var oldCenter = (previous + other.Position) * 0.5f;
            var center = (position + other.Position) * 0.5f;
            var oldDistance = Vector2.Distance(previous, other.Position);
            var distance = Vector2.Distance(position, other.Position);
            if (center != oldCenter) Panned?.Invoke(center - oldCenter);
            // Coincident fingers do not have a meaningful scale. Rebase until separated.
            // A callback may cancel/release the gesture when the screen changes.
            if (!blocked && pointers.Count == 2 && oldDistance >= 1 && distance >= 1 && oldDistance != distance)
                Zoomed?.Invoke(distance / oldDistance, center);
        }

        public void End(int id, Vector2 position, bool canceled = false)
        {
            ValidatePosition(position);
            if (!pointers.ContainsKey(id)) return;
            if (canceled) Cancel();
            else Move(id, position);
            var tap = !blocked && !dragging && pointers.Count == 1;
            var multiple = pointers.Count > 1;
            pointers.Remove(id);
            // After a pinch or cancellation the remaining finger must not become a tap.
            if (multiple) blocked = true;
            if (pointers.Count == 0) { blocked = false; dragging = false; }
            if (tap) Tapped?.Invoke(position);
        }

        public void Cancel()
        {
            // Retain IDs until release: stale Up events cannot act on a new layout.
            // The device adapter must End(..., canceled: true) for disappeared contacts.
            blocked = true;
            dragging = true;
        }

        private static void ValidatePosition(Vector2 position)
        {
            if (float.IsNaN(position.x) || float.IsNaN(position.y)
                || float.IsInfinity(position.x) || float.IsInfinity(position.y))
                throw new ArgumentException("Pointer position must be finite.", nameof(position));
        }
    }
}
