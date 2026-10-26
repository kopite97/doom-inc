using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace MobileGame.Foundation.Tests
{
    public class DefinitionFoundationTests : FoundationTestFixture
    {
        [Test]
        public void DifferentAssetsCannotUseOneDefinitionIdButTheSameAssetCanBeShared()
        {
            var registry = new ThingRegistry();
            var definition = CreateStorageDefinition();
            var duplicate = CreateStorageDefinition(definition.DefinitionId);
            var first = registry.Create(definition);

            AssertRejectedWithoutRegistryChange(registry, () => registry.Create(duplicate));
            var second = registry.Create(definition);

            Assert.That(first.Definition, Is.SameAs(second.Definition));
            Assert.That(registry.Things, Is.EquivalentTo(new[] { first, second }));
        }

        [Test]
        public void InvalidComponentDefinitionsAreRejectedBeforeRegisteringThings()
        {
            var invalidDefinitions = new ThingDefinition[]
            {
                CreateDefinition<ResourceDefinition>("fixture.missing-stack"),
                CreateDefinition<ResourceDefinition>("fixture.extra-container",
                    ThingComponentKind.ResourceStack, ThingComponentKind.ResourceContainer),
                CreateDefinition<CreatureDefinition>("fixture.creature-stack", ThingComponentKind.ResourceStack),
                CreateDefinition<FacilityDefinition>("fixture.duplicate-component",
                    ThingComponentKind.ResourceContainer, ThingComponentKind.ResourceContainer)
            };
            var registry = new ThingRegistry();
            var holder = CreateHolder(registry, CreateStorageDefinition());

            foreach (var definition in invalidDefinitions)
            {
                if (definition is ResourceDefinition resourceDefinition)
                {
                    AssertRejectedWithoutRegistryChange(registry,
                        () => registry.CreateResource(resourceDefinition, 1, holder));
                }
                else
                {
                    AssertRejectedWithoutRegistryChange(registry, () => registry.Create(definition));
                }
            }

            Assert.That(holder.Contents, Is.Empty);
        }

        [Test]
        public void RepresentativeAssetsImportAndCreateLogicalThingsWithoutChangingDefinitions()
        {
            var resource = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(
                "Assets/SO/Resource/OrganicMatter.asset");
            var storage = AssetDatabase.LoadAssetAtPath<FacilityDefinition>(
                "Assets/SO/Facility/Storage.asset");
            var prototype = AssetDatabase.LoadAssetAtPath<CreatureDefinition>(
                "Assets/SO/Creature/PrototypeSupport.asset");
            Assert.That(resource, Is.Not.Null, "Import the representative organic matter definition.");
            Assert.That(storage, Is.Not.Null, "Import the representative storage definition.");
            Assert.That(prototype, Is.Not.Null, "Import the representative support prototype definition.");
            var definitions = new ThingDefinition[] { resource, storage, prototype };
            var before = definitions.Select(definition => EditorJsonUtility.ToJson(definition)).ToArray();
            Assert.That(definitions.Select(definition => definition.DefinitionId).Distinct().Count(),
                Is.EqualTo(definitions.Length));
            foreach (var definition in definitions)
            {
                Assert.That(string.IsNullOrWhiteSpace(definition.DefinitionId), Is.False);
                Assert.That(string.IsNullOrWhiteSpace(definition.DisplayName), Is.False);
                Assert.DoesNotThrow(() => definition.Validate());
            }

            var registry = new ThingRegistry();
            var storageHolder = CreateHolder(registry, storage);
            var prototypeHolder = CreateHolder(registry, prototype);
            var stack = registry.CreateResource(resource, 5, storageHolder);
            registry.TransferResource(stack, prototypeHolder);

            Assert.That(resource.Kind, Is.EqualTo(ThingKind.Resource));
            Assert.That(storage.Kind, Is.EqualTo(ThingKind.Facility));
            Assert.That(prototype.Kind, Is.EqualTo(ThingKind.Creature));
            Assert.That(storageHolder.Contents, Is.Empty);
            Assert.That(prototypeHolder.Contents, Is.EquivalentTo(new[] { stack }));
            Assert.That(definitions.Select(definition => EditorJsonUtility.ToJson(definition)).ToArray(),
                Is.EqualTo(before), "Creating or transferring runtime Things must not modify the SO assets.");
        }
    }
}
