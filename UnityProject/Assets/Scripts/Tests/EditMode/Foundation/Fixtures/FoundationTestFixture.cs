using MobileGame.TestSupport;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MobileGame.Foundation.Tests
{
    public abstract class FoundationTestFixture
    {
        private readonly List<ThingData> definitions = new List<ThingData>();

        protected T CreateDefinition<T>(string id, params ThingComponentKind[] components)
            where T : ThingData
        {
            var definition = DataTestAssets.Create<T>();
            definitions.Add(definition);
            using (var serialized = new DataFixtureEditor(definition))
            {
                serialized.FindProperty("definitionId").stringValue = id;
                serialized.FindProperty("displayName").stringValue = id + " fixture";
                var componentProperty = serialized.FindProperty("components");
                componentProperty.arraySize = components.Length;
                for (var index = 0; index < components.Length; index++)
                {
                    componentProperty.GetArrayElementAtIndex(index).intValue = (int)components[index];
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return definition;
        }

        protected FacilityData CreateStorageDefinition(string id = "fixture.storage")
        {
            return CreateDefinition<FacilityData>(id, ThingComponentKind.ResourceContainer);
        }

        protected ResourceData CreateResourceDefinition(string id = "fixture.resource")
        {
            return CreateDefinition<ResourceData>(id, ThingComponentKind.ResourceStack);
        }

        protected static ResourceContainerComponent CreateHolder(ThingRegistry registry,
            ThingData definition)
        {
            return registry.Create(definition).GetComponent<ResourceContainerComponent>();
        }

        protected static void AssertRejectedWithoutRegistryChange(ThingRegistry registry,
            TestDelegate action)
        {
            var before = registry.Things.ToArray();
            Assert.That(action, Throws.Exception, "Invalid input must be rejected.");
            Assert.That(registry.Things.ToArray(), Is.EquivalentTo(before),
                "Rejected input must not partially add or remove active Things.");
        }

        [TearDown]
        public void DestroyFixtureDefinitions()
        {
            foreach (var definition in definitions)
            {
                // Plain fixture Data is garbage collected.
            }

            definitions.Clear();
        }
    }
}
