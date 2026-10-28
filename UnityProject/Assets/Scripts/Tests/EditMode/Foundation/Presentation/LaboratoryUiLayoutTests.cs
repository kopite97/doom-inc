using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MobileGame.Gameplay;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MobileGame.Foundation.Tests
{
    public sealed class LaboratoryUiLayoutTests
    {
        [TestCase(0, 80, 1080, 2240, 1080, 2400)]
        [TestCase(100, 0, 2200, 1080, 2400, 1080)]
        [TestCase(0, 0, 2300, 1080, 2400, 1080)]
        [TestCase(30, 20, 740, 560, 800, 600)]
        public void SafeAreaNotchesMapBackToTheExactPhysicalBounds(float x, float y, float w, float h, float screenW, float screenH)
        {
            var safe = new Rect(x, y, w, h);
            var logical = LaboratoryUiCanvas.CalculateSafeRect(safe, new Vector2(screenW, screenH), out var scale);
            Assert.That(logical.x * scale, Is.EqualTo(safe.xMin).Within(.001f));
            Assert.That(logical.y * scale, Is.EqualTo(screenH - safe.yMax).Within(.001f));
            Assert.That(logical.width * scale, Is.EqualTo(safe.width).Within(.001f));
            Assert.That(logical.height * scale, Is.EqualTo(safe.height).Within(.001f));
            Assert.That(scale, Is.InRange(.5f, 3f));
        }

        [TestCase(0, 0)]
        [TestCase(-1, 500)]
        [TestCase(500, -1)]
        public void EmptyOrInvalidSafeAreaFallsBackToTheWholeScreen(float width, float height)
        {
            var logical = LaboratoryUiCanvas.CalculateSafeRect(new Rect(90, 20, width, height), new Vector2(1200, 800), out var scale);
            Assert.That(logical.position, Is.EqualTo(Vector2.zero));
            Assert.That(logical.width * scale, Is.EqualTo(1200).Within(.001f));
            Assert.That(logical.height * scale, Is.EqualTo(800).Within(.001f));
        }

        [Test]
        public void ReleaseFromAnOldLayoutOrBindingCannotInvokeAnAction()
        {
            var host = new GameObject("Button epoch test");
            var events = new GameObject("Button epoch events", typeof(EventSystem));
            try
            {
                var epoch = 1; var clicks = 0;
                var button = LaboratoryUiWidgets.Button("Action", host.transform, "Action", () => epoch);
                button.Bind(() => clicks++, "first");
                var pointer = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId = 1 };
                button.OnPointerDown(pointer); epoch++; button.OnPointerClick(pointer);
                Assert.That(clicks, Is.Zero, "A rotation/layout change must consume the previous press.");
                button.OnPointerDown(pointer); button.Bind(() => clicks += 10, "second"); button.OnPointerClick(pointer);
                Assert.That(clicks, Is.Zero, "Changing the selected binding must consume the previous press.");
                button.OnPointerDown(pointer); button.OnPointerClick(pointer); button.OnPointerClick(pointer);
                Assert.That(clicks, Is.EqualTo(10), "One accepted press may only invoke once.");
            }
            finally { UnityEngine.Object.DestroyImmediate(host); UnityEngine.Object.DestroyImmediate(events); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TwoHeldPriorityButtonsApplyEachChangeToTheLatestValues(bool individual)
        {
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN013-Ui-" + Guid.NewGuid().ToString("N"));
            var host = new GameObject("Priority test");
            var events = new GameObject("Priority events", typeof(EventSystem));
            try
            {
                var view = host.AddComponent<LaboratoryView>();
                view.Initialize(DataTestAssets.Catalog, Path.Combine(directory, "session.json"));
                var workerThing = view.Simulation.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                var worker = workerThing.GetComponent<WorkerComponent>();
                if (individual) Assert.That(view.SelectThing(workerThing.Id), Is.True);
                var rows = new LaboratoryUiList(LaboratoryUiWidgets.Rect("Rows", host.transform), () => 0);
                typeof(LaboratoryView).GetField("controls", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, rows);
                rows.Begin(500);
                var before = view.Simulation.Preset.ToArray();
                Action<int[]> apply = individual ? values => view.SetWorkerPriorities(workerThing.Id, values) : values => view.SetPreset(values);
                typeof(LaboratoryView).GetMethod("AddPriorities", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view,
                    new object[] { "Priority", before, apply, true, individual ? (Guid?)workerThing.Id : null });
                rows.End();
                var buttons = host.GetComponentsInChildren<LaboratoryUiButton>();
                var first = buttons.Single(b => b.name == "Priority_0"); var second = buttons.Single(b => b.name == "Priority_1");
                var firstPointer = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId = 1 };
                var secondPointer = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId = 2 };
                first.OnPointerDown(firstPointer); second.OnPointerDown(secondPointer);
                first.OnPointerClick(firstPointer); second.OnPointerClick(secondPointer);
                var after = individual ? worker.Priorities.ToArray() : view.Simulation.Preset.ToArray();
                Assert.That(after[0], Is.EqualTo((before[0] + 1) % 5));
                Assert.That(after[1], Is.EqualTo((before[1] + 1) % 5));
                Assert.That(after.Skip(2), Is.EqualTo(before.Skip(2)));
                if (individual) Assert.That(worker.UsesPreset, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host); UnityEngine.Object.DestroyImmediate(events);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }
}
