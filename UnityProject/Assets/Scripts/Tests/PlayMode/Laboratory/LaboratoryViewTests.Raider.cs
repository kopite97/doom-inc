#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator RaiderCultivationAndMixedReleaseDeliverOnceAcrossContinue()
        {
            var background = Application.runInBackground;
            Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN022-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                yield return LoadIsolatedScene(Path.Combine(directory, "session.json"), 999,
                    loaded => { view = loaded; view.SimulationRate = 0; });
                var facility = CreateStatusLayoutFacility(view, "cultivationId", new Vector2Int(6, 5), QuarterTurn.None, true);
                var store = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(10, 5), QuarterTurn.None, true);
                var lab = view.Simulation;
                var input = facility.GetComponent<ResourceContainerComponent>();
                const string raider = "creature.raider";
                Assert.That(view.SelectCultivation(facility.Id, raider), Is.True);
                lab.Registry.CreateResource(lab.Culture, 2, input);
                view.Controller.Advance(0.1);
                Assert.That(view.SelectCultivation(facility.Id, CultivationTarget.Invader), Is.True);
                Assert.That(view.Save() && view.Reload(), Is.True);
                lab = view.Simulation;
                yield return AdvanceUntil(view, () => lab.Invasion.WaitingCount == 1, 110);
                Assert.That(lab.Registry.Things.Count(t => t.Definition.DefinitionId == raider), Is.EqualTo(1));
                lab.Registry.TryGet(facility.Id, out facility);
                lab.Registry.CreateResource(lab.Culture, 2, facility.GetComponent<ResourceContainerComponent>());
                yield return AdvanceUntil(view, () => lab.Invasion.WaitingCount == 2, 110);
                Assert.That(view.SetAllowed(store.Id, lab.Organic, false), Is.True);
                Assert.That(view.ReleaseInvaders(), Is.True);
                view.Controller.Advance(15);
                Assert.That(view.Invasion.Score, Is.EqualTo(9));
                Assert.That(lab.Receiving.PendingCount, Is.EqualTo(1));
                foreach (var language in new[] { "ko", "en" })
                {
                    yield return SelectLanguage(view, language);
                    view.Popups.Top.Cancel(); yield return PopupSettled(view);
                    foreach (var size in new[] { new Vector2Int(720, 1280), new Vector2Int(1280, 720) })
                    using (new LaboratoryGameViewSize(size.x, size.y))
                    {
                        yield return WaitFor(() => Screen.width == size.x && Screen.height == size.y, "Resize failed");
                        view.OpenPanel(LaboratoryPanel.Invasion);
                        yield return WaitFor(() => VisibleUiText(view).Contains(language == "ko" ? "대기 1건" : "Pending shipments 1"), "Missing receipt state");
                        view.SelectThing(facility.Id); view.ExpandSelection();
                        yield return WaitFor(() => view.FindUiButton("Cultivate_" + raider) != null, "Missing raider cultivation button");
                    }
                }
                Assert.That(view.Save() && view.Reload(), Is.True);
                Assert.That(view.SetAllowed(store.Id, view.Simulation.Organic, true), Is.True);
                view.Controller.Advance(0.1);
                Assert.That(view.Simulation.Receiving.Sources.Single().Delivered, Is.EqualTo(1));
                view.SimulationRate = 1;
                var elapsed = view.Invasion.Elapsed;
                yield return WaitFor(() => view.Invasion.Elapsed > elapsed + 0.1, "Game did not advance from Update");
                view.SimulationRate = 0;
                view.Controller.Advance(500);
                yield return WaitFor(() => view.Controller.Phase == SessionPhase.Results, "Missing result screen");
                Assert.That(view.Reload(), Is.True);
                Assert.That(view.Controller.Phase, Is.EqualTo(SessionPhase.Results));
                Assert.That(view.Restart(), Is.True);
                Assert.That(view.Simulation.Receiving.Sources, Is.Empty);
            }
            finally
            {
                if (view != null) UnityEngine.Object.DestroyImmediate(view.gameObject);
                Application.runInBackground = background;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }
}
#endif
