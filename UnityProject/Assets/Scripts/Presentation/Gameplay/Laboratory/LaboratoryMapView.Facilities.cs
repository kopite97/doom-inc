using System;
using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryMapView
    {
        private void DrawFacility(Thing thing, ThingArt item, FacilityArtSet set)
        {
            if (item.Facility == null) item.Facility = new FacilityAnimation();
            var status = view.StatusFor(thing);
            var working = status.Kind == FacilityStatusKind.ProductionWorking || status.Kind == FacilityStatusKind.ProductionAutomatic;
            var progress = thing.TryGetComponent<ProductionComponent>(out var production) ? production.Progress : 0;
            item.Frame = item.Facility.Evaluate(artClock, set, working, progress, artSimulation.IsComplete(thing));
            item.Body.sprite = set.Frame(item.Frame);
            item.Body.transform.localPosition = Vector3.zero;
            item.Body.transform.localScale = Vector3.one;
            item.Body.transform.localRotation = Quaternion.Euler(0, 0, -90 * (int)thing.Placement.Rotation);
            item.Cargo.gameObject.SetActive(false);
        }

        private void OnProductionCompleted(Guid id) => ObserveFacility(id, FacilityArtMotion.Complete);
        private void OnResourceTransferred(Guid source, Guid destination, int amount)
        {
            if (amount <= 0 || source == destination) return;
            ObserveStorage(source, FacilityArtMotion.Send);
            ObserveStorage(destination, FacilityArtMotion.Receive);
        }
        private void ObserveStorage(Guid id, FacilityArtMotion motion)
        {
            if (artSimulation.Registry.TryGet(id, out var thing) && thing.Definition is FacilityData data && data.Role == FacilityRole.Storage)
                ObserveFacility(id, motion);
        }
        private void ObserveFacility(Guid id, FacilityArtMotion motion)
        {
            if (!artSimulation.Registry.TryGet(id, out var thing) || !thing.IsPlaced || !artSimulation.IsComplete(thing)
                || !thingArt.TryGetValue(id, out var item) || view.UiAssets?.Art == null
                || !view.UiAssets.Art.TryFacility(thing.Definition, out var set)) return;
            if (item.Facility == null) item.Facility = new FacilityAnimation();
            item.Facility.Observe(motion, artSimulation.ElapsedSeconds, set.Duration(motion));
        }
    }
}
