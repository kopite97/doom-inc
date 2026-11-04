#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator KoreanEnglishLargeTextFitsBothOrientationsAndKeepsSelectedDetails()
        {
            var background = Application.runInBackground; Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN018-Layout-" + Guid.NewGuid().ToString("N"));
            LaboratoryView view = null;
            try
            {
                yield return LoadIsolatedScene(Path.Combine(directory, "session.json"), 999, loaded => { view = loaded; view.SimulationRate = 0; });
                var facility = CreateStatusLayoutFacility(view, "processingId", new Vector2Int(6, 4), QuarterTurn.None, true);
                foreach (var language in new[] { "ko", "en" })
                {
                    view.OpenPanel(LaboratoryPanel.Game); yield return SelectLanguage(view, language);
                    view.Popups.Top.Cancel(); yield return PopupSettled(view);
                    view.SetUiPreferences(true, true);
                    foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(720, 1280) })
                    using (new LaboratoryGameViewSize(size.x, size.y))
                    {
                        yield return WaitFor(() => Screen.width == size.x && Screen.height == size.y, "Game View did not resize.");
                        view.SelectThing(facility.Id); view.ExpandSelection();
                        Assert.That(view.ReleaseInvaders(), Is.False); // Long localized notice without changing game state.
                        yield return null; yield return null; Canvas.ForceUpdateCanvases();
                        Assert.That(view.SelectedId, Is.EqualTo(facility.Id)); Assert.That(view.DetailsExpanded, Is.True);
                        var closeLabel = view.FindUiButton("ClosePanel").GetComponentInChildren<TextMeshProUGUI>();
                        Assert.That(closeLabel.preferredWidth, Is.LessThanOrEqualTo(closeLabel.rectTransform.rect.width + 1), "Close must not wrap in the middle of a word.");
                        foreach (var label in view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Where(t => t.enabled && !string.IsNullOrEmpty(t.text)))
                        {
                            if (label.transform.IsChildOf(view.MapViewport)) continue; // World-space markers are separately measured in status tests.
                            Assert.That(label.preferredHeight, Is.LessThanOrEqualTo(label.rectTransform.rect.height + 2), label.name + ": " + label.text);
                        }
                        var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/plan018-" + language + "-" + size.x + "x" + size.y + ".png"));
                        yield return AppSceneFlowTests.Capture("localization", path);
                        Assert.That(Screen.width, Is.EqualTo(size.x)); Assert.That(Screen.height, Is.EqualTo(size.y));
                    }
                }
            }
            finally
            {
                if (view != null) UnityEngine.Object.Destroy(view.gameObject);
                Application.runInBackground = background;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [UnityTest]
        public IEnumerator LanguageChangePreservesPreviewSessionNoticeAndSavedLanguage()
        {
            var background = Application.runInBackground; Application.runInBackground = true;
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN018-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(directory, "session.json"); LaboratoryView view = null;
            try
            {
                yield return LoadIsolatedScene(path, 999, loaded => { view = loaded; view.SimulationRate = 0; });
                Assert.That(view.BeginGatheringConstruction(), Is.True);
                Assert.That(view.PreviewAt(new Vector2Int(3, 3)), Is.True);
                var session = view.Simulation; var generation = view.Controller.Generation;
                var origin = session.ConstructionOrigin; var ids = session.Registry.Things.Select(t => t.Id).ToArray();
                var hash = session.Registry.Things.First().Definition.Settings.CompatibilityHash;
                view.SetUiPreferences(false, true);
                view.OpenPanel(LaboratoryPanel.Game); yield return null;
                view.ShowSettings(); yield return PopupSettled(view);
                var stale = FindActionButton(view, "StatusPreference");
                var pointer = new PointerEventData(EventSystem.current) { pointerId = 77 };
                stale.OnPointerDown(pointer);
                yield return SelectLanguage(view, "en");
                stale.OnPointerClick(pointer);
                Assert.That(view.ShowFacilityStatus, Is.False, "Changing language consumes a held old-language button.");
                Assert.That(view.Simulation, Is.SameAs(session)); Assert.That(view.Controller.Generation, Is.EqualTo(generation));
                Assert.That(view.IsEditing, Is.True); Assert.That(session.ConstructionOrigin, Is.EqualTo(origin));
                Assert.That(session.Registry.Things.Select(t => t.Id), Is.EquivalentTo(ids));
                Assert.That(session.Registry.Things.First().Definition.Settings.CompatibilityHash, Is.EqualTo(hash));
                Assert.That(view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.name == "Feedback").text, Is.EqualTo("Facility status: OFF"));
                Assert.That(view.FindUiButton("Confirm").GetComponentInChildren<TextMeshProUGUI>().text, Is.EqualTo("Confirm"));
                Assert.That(File.ReadAllText(Path.Combine(directory, "language.json")), Does.Contain("en"));
                view.Popups.Top.Cancel(); yield return PopupSettled(view);
                view.CancelPlacement(); view.OpenPanel(LaboratoryPanel.Game);
                yield return SelectLanguage(view, "ko");
                Assert.That(view.FindUiButton("Save").GetComponentInChildren<TextMeshProUGUI>().text, Is.EqualTo("지금 저장"));
                yield return SelectLanguage(view, "en");
                Assert.That(view.Save(), Is.True);
                UnityEngine.Object.Destroy(view.gameObject); view = null; yield return null;
                yield return LoadIsolatedScene(path, 999, loaded => { view = loaded; view.SimulationRate = 0; });
                view.OpenPanel(LaboratoryPanel.Game); yield return null;
                Assert.That(view.FindUiButton("Save").GetComponentInChildren<TextMeshProUGUI>().text, Is.EqualTo("Save now"));
                Assert.That(view.Simulation.Registry.Things.Select(t => t.Id), Is.EquivalentTo(ids));
            }
            finally
            {
                if (view != null) UnityEngine.Object.Destroy(view.gameObject);
                Application.runInBackground = background;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
        private static IEnumerator SelectLanguage(LaboratoryView view, string code)
        {
            // A Game View resize/capture can deliver focus callbacks after the coroutine resumes.
            // Wait for the real menu to be usable before opening and pressing its language action.
            if (view.Popups.Top == null) view.ShowSettings();
            yield return PopupSettled(view);
            yield return WaitFor(() => view.Popups.Top?.State == PopupState.Open && AppUiLifetime.AcceptInput, "Language settings are not interactive.");
            yield return WaitFor(() => view.GetComponentsInChildren<SettingsPopup>().Single().GetComponentsInChildren<UiButton>().Any(b => b.name == "Language_" + code),
                "The language menu did not render: " + code);
            var button = view.GetComponentsInChildren<SettingsPopup>().Single().GetComponentsInChildren<UiButton>().Single(b => b.name == "Language_" + code); Assert.That(button, Is.Not.Null);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = 78 };
            button.OnPointerDown(pointer); button.OnPointerClick(pointer);
            yield return WaitFor(() => button.GetComponentInChildren<TextMeshProUGUI>().text.StartsWith("●", StringComparison.Ordinal), "Language did not change: " + code);
            yield return null;
        }
    }
}
#endif
