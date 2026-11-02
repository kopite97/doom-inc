#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class AppSceneFlowTests
    {
        [UnityTest] public IEnumerator RepeatedVisibleLoadingRequestsInvalidatePressEvenWithIdenticalText()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var oldCalls = 0; var newCalls = 0;
            flow.Loading.Show(LoadingScreenKind.Confirmation, "same message", () => oldCalls++, null, "custom label");
            for (var i = 0; i < 4; i++) yield return null;
            var button = flow.Loading.GetComponentsInChildren<UiButton>().Single(b => b.name == "Retry");
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            var press = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerId = 17 };
            button.OnPointerDown(press);
            flow.Loading.Show(LoadingScreenKind.Error, "same message", () => newCalls++, null, "새 게임 시작");
            for (var i = 0; i < 4; i++) yield return null;
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            Assert.That(flow.Loading.GetComponentsInChildren<Text>().Single(t => t.name == "LoadingTitle").text,
                Is.EqualTo("준비 중 문제가 발생했습니다"), "A button caption cannot choose the screen kind.");
            button.OnPointerClick(press);
            Assert.That(oldCalls + newCalls, Is.Zero);
            for (var i = 0; i < 4; i++) yield return null;
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            button.OnPointerDown(press);
            flow.Loading.Show(LoadingScreenKind.Error, "same message", () => newCalls++, null, "새 게임 시작");
            for (var i = 0; i < 4; i++) yield return null;
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            button.OnPointerClick(press);
            Assert.That(oldCalls + newCalls, Is.Zero, "Even an identical error is a new input lifetime.");
            for (var i = 0; i < 4; i++) yield return null;
            Assert.That(button.interactable && AppUiLifetime.AcceptInput, Is.True);
            button.OnPointerDown(press); button.OnPointerClick(press); button.OnPointerClick(press);
            Assert.That(newCalls, Is.EqualTo(1)); Assert.That(oldCalls, Is.Zero);
            flow.Loading.Show(LoadingScreenKind.Loading, "same message");
            yield return null;
            Assert.That(flow.Loading.GetComponentsInChildren<UiButton>(), Is.Empty);
            Assert.Throws<ArgumentOutOfRangeException>(() => flow.Loading.Show((LoadingScreenKind)99, "invalid"));
            Assert.That(flow.Loading.Kind, Is.EqualTo(LoadingScreenKind.Loading));
            flow.Loading.Show(LoadingScreenKind.Error, "same message", () => newCalls++);
            yield return ClickLoading("Retry");
            Assert.That(newCalls, Is.EqualTo(2));
            flow.Loading.Hide(); LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator SharedTextStyleKeepsBaseSizeForReusedNewAndHiddenRowsWithoutAView()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            using (var canvas = new UiCanvas(flow.transform, "Temp_SharedStyleTest", 20))
            {
                var content = UiWidgets.Rect("Rows", canvas.SafeRoot);
                var rows = new UiList(content, () => canvas.LayoutEpoch);
                var staticLabel = UiWidgets.Label("Static", canvas.SafeRoot, 24);
                var style = staticLabel.GetComponent<UiText>();
                foreach (var scale in new[] { 1f, 1.25f, 1.25f, 1f, 1.25f, 1f })
                {
                    style.Apply(null, scale);
                    rows.Style(null, scale, 64);
                    rows.Begin(400); rows.Text("Existing", "Test", 20); rows.Action("Action", "Action", () => { }, true, "test"); rows.End();
                    canvas.SetVisible(false); canvas.SetVisible(true);
                    yield return null;
                    Assert.That(staticLabel.fontSize, Is.EqualTo(Mathf.RoundToInt(24 * scale)));
                    Assert.That(staticLabel.font, Is.Not.Null);
                    Assert.That(content.Find("Existing").GetComponent<Text>().fontSize, Is.EqualTo(Mathf.RoundToInt(20 * scale)));
                    Assert.That(((RectTransform)content.Find("Action")).rect.height, Is.GreaterThanOrEqualTo(64));
                }
                rows.Style(configuration.uiAssets.Font, 1.25f, 70);
                rows.Begin(400); rows.Text("Existing", "Existing", 20); rows.Text("New", "New", 20); rows.End();
                yield return null;
                foreach (var text in content.GetComponentsInChildren<Text>())
                {
                    Assert.That(text.fontSize, Is.EqualTo(25));
                    Assert.That(text.font, Is.EqualTo(configuration.uiAssets.Font));
                }
            }
            flow.Loading.Configure(null, () => true);
            flow.Loading.Show(LoadingScreenKind.Error, "Fallback text", () => { });
            yield return null;
            Assert.That(flow.Loading.GetComponentsInChildren<Text>().Single(t => t.name == "LoadingTitle").fontSize, Is.EqualTo(30));
            Assert.That(flow.Loading.GetComponentsInChildren<Text>().Single(t => t.name == "Status").fontSize, Is.EqualTo(Mathf.RoundToInt(18 * 1.25f)));
            flow.Loading.Hide(); LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator UnreadableSaveRemainsAnErrorAndCannotSilentlyStartANewRun()
        {
            Directory.CreateDirectory(path);
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var title = UnityEngine.Object.FindFirstObjectByType<TitleView>();
            Assert.That(title.HasSave, Is.True);
            Assert.That(title.SaveError, Is.Not.Null);
            Assert.That(flow.StartNewGame(), Is.False);
            Assert.That(flow.Loading.Kind, Is.EqualTo(LoadingScreenKind.Error));
            Assert.That(Directory.Exists(path), Is.True);
            Directory.Delete(path); title.RefreshSave();
            Assert.That(title.HasSave, Is.False); Assert.That(title.SaveError, Is.Null);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
