#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class AppSceneFlowTests
    {
        [UnityTest] public IEnumerator ActualSceneCompletionFillsBarAndSlowPreparationAddsNoSecondDelay()
        {
            Assert.That(configuration.minimumLoadingSeconds, Is.EqualTo(1f));
            var gate = new TaskCompletionSource<GameDataCatalog>();
            var pending = GameDataStore.GetOrCreate().InitializeAsync(token => gate.Task);
            flow.EnterCurrent(AppScene.Bootstrap);
            var percent = flow.Loading.GetComponentsInChildren<TextMeshProUGUI>(true).Single(t => t.name == "ScenePercent");
            var fill = flow.Loading.GetComponentsInChildren<RectTransform>(true).Single(t => t.name == "SceneProgress").Find("Fill") as RectTransform;
            Assert.That(percent.gameObject.activeSelf, Is.False);
            Assert.That(fill.parent.gameObject.activeSelf, Is.False);

            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(1.1f);
            Assert.That(flow.Loading.Visible && flow.IsTransitioning, Is.True);
            Assert.That(percent.gameObject.activeSelf, Is.False, "Time spent preparing data must not fabricate scene progress.");

            var loadedTitle = default(TitleView);
            var readyFrame = -1;
            void TrackScene(Scene scene, LoadSceneMode mode)
            {
                if (scene.path == configuration.titleScene)
                    loadedTitle = UnityEngine.Object.FindFirstObjectByType<TitleView>();
            }
            void TrackOverlayFrame()
            {
                if (readyFrame < 0 && loadedTitle != null && loadedTitle.GetComponentInChildren<UiButton>() != null
                    && flow.Loading.Visible && percent.gameObject.activeInHierarchy && percent.text == "100%" && fill.anchorMax.x == 1f)
                    readyFrame = Time.frameCount;
            }
            SceneManager.sceneLoaded += TrackScene;
            Canvas.willRenderCanvases += TrackOverlayFrame;
            try
            {
                var released = Time.realtimeSinceStartupAsDouble;
                gate.SetResult(GameDataAssets.LoadCatalog());
                var deadline = released + 20;
                while (flow.IsTransitioning && Time.realtimeSinceStartupAsDouble < deadline)
                {
                    if (percent.gameObject.activeInHierarchy)
                        Assert.That(percent.text, Is.EqualTo(Mathf.FloorToInt(fill.anchorMax.x * 100f) + "%"));
                    yield return null;
                }
                Assert.That(pending.IsCompletedSuccessfully, Is.True);
                Assert.That(flow.IsTransitioning, Is.False);
                Assert.That(flow.Error, Is.Null);
                Assert.That(readyFrame, Is.GreaterThanOrEqualTo(0), "Render the 100% scene bar over the prepared title.");
                Assert.That(Time.frameCount - readyFrame, Is.InRange(1, 2), "A slow load must not start another hold after arrival.");
                TestContext.WriteLine($"Slow load completed. First ready frame {readyFrame}, closed by frame {Time.frameCount}.");
            }
            finally
            {
                SceneManager.sceneLoaded -= TrackScene;
                Canvas.willRenderCanvases -= TrackOverlayFrame;
            }
        }

        [UnityTest] public IEnumerator DirectEntryCountsOnlyVisibleUnscaledTimeAcrossOverlappingFocusAndPause()
        {
            configuration.minimumLoadingSeconds = 0f;
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            flow.StopCoroutine(keepFocused);
            configuration.minimumLoadingSeconds = 1f;
            var previousScale = Time.timeScale;
            try
            {
                Time.timeScale = 0;
                var began = Time.realtimeSinceStartupAsDouble;
                Assert.That(flow.EnterCurrent(AppScene.Title), Is.True);
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(.4f);
                Assert.That(flow.Loading.Visible && flow.IsTransitioning, Is.True);
                Assert.That(flow.Loading.GetComponentsInChildren<TextMeshProUGUI>().Any(t => t.name == "ScenePercent"), Is.False);
                flow.SendMessage("OnApplicationFocus", false);
                flow.SendMessage("OnApplicationPause", true);
                var pausedAt = Time.realtimeSinceStartupAsDouble;
                yield return new WaitForSecondsRealtime(1.05f);
                flow.SendMessage("OnApplicationPause", false);
                yield return null;
                Assert.That(flow.Loading.Visible && flow.BlocksScreenInput, Is.True);
                var resumed = Time.realtimeSinceStartupAsDouble;
                flow.SendMessage("OnApplicationFocus", true);
                Assert.That(flow.IsTransitioning, Is.True);
                var deadline = resumed + 8;
                while (flow.IsTransitioning && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                var foreground = pausedAt - began + Time.realtimeSinceStartupAsDouble - resumed;
                TestContext.WriteLine($"Direct entry shown for {foreground:F3}s in the foreground with timeScale=0.");
                Assert.That(flow.IsTransitioning, Is.False);
                Assert.That(flow.Error, Is.Null);
                Assert.That(flow.Loading.Visible, Is.False);
                Assert.That(foreground, Is.GreaterThanOrEqualTo(1d));
            }
            finally
            {
                Time.timeScale = previousScale;
            }
        }
    }
}
#endif
