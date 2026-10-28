using MobileGame.TestSupport;
using System.IO;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public sealed class LaboratoryCompatibilityTests : FoundationTestFixture
    {
        [Test]
        public void UpgradedSharedDefinitionStillLoadsLegacyDiagnosticButNotActualPlaySlot()
        {
            var organic = CreateResourceDefinition("compat.organic");
            var storage = CreateDefinition<FacilityData>("compat.storage", ThingComponentKind.ResourceContainer);
            var registry = new ThingRegistry();
            var map = new GridMap(registry, 8, 8);
            var thing = registry.Create(storage);
            Assert.That(map.Commands.Place(thing.Id, Vector2Int.one).Succeeded, Is.True);
            var definitions = new ThingData[] { organic, storage };
            var codec = new SessionSnapshotCodec(definitions);
            var oldJson = codec.Serialize(codec.Capture(new SessionState(registry, map), SessionPhase.Playing));
            // The same SO/GUID receives gameplay fields after the legacy diagnostic was saved.
            using (var serialized = new DataFixtureEditor(storage))
            {
                serialized.FindProperty("role").intValue = (int)FacilityRole.Storage;
                serialized.FindProperty("resource").objectReferenceValue = organic;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            var restored = codec.Restore(codec.Deserialize(oldJson));
            Assert.That(restored.Runtime, Is.Null);
            Assert.That(restored.Registry.TryGet(thing.Id, out var loaded), Is.True);
            Assert.That(loaded.Definition, Is.SameAs(storage));
            Assert.That(loaded.TryGetComponent<ConstructionComponent>(out _), Is.False);
            var playCodec = new SessionSnapshotCodec(definitions, requireLaboratory: true);
            Assert.That(() => playCodec.Restore(playCodec.Deserialize(oldJson)), Throws.TypeOf<InvalidDataException>());
        }
    }
}
