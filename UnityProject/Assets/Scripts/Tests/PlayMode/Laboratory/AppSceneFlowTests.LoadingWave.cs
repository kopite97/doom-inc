#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class AppSceneFlowTests
    {
        [UnityTest] public IEnumerator TextAnimatorWaveMovesUnscaledAndRestoresTextOnErrorAndRetry()
        {
            var titleScene = configuration.titleScene;
            var oldTimeScale = Time.timeScale;
            var catalog = GameDataAssets.LoadCatalog();
            var gate = new TaskCompletionSource<GameDataCatalog>();
            var preparation = GameDataStore.GetOrCreate().InitializeAsync(token => gate.Task);
            try
            {
                Time.timeScale = 0;
                configuration.titleScene = "Assets/Tests/MissingWaveTestScene.unity";
                Assert.That(flow.EnterCurrent(AppScene.Bootstrap), Is.True);
                var wave = flow.Loading.GetComponentInChildren<LoadingTextWave>();
                var title = wave.GetComponent<TextMeshProUGUI>();
                var animator = wave.GetComponents<TextAnimator_TMP>().Single();
                var deadline = Time.realtimeSinceStartupAsDouble + 5;
                bool moved = false;
                bool distinct = false;
                float[] first = null;
                while ((!moved || !distinct) && Time.realtimeSinceStartupAsDouble < deadline)
                {
                    yield return new WaitForEndOfFrame();
                    var offsets = GlyphOffsets(title);
                    if (offsets.Length != "Loading...".Length) continue;
                    if (first == null) first = offsets;
                    moved |= offsets.Where((y, i) => Mathf.Abs(y - first[i]) > .25f).Any();
                    distinct |= offsets.Max() - offsets.Min() > 1f;
                }
                Assert.That(title.text, Is.EqualTo("Loading..."));
                Assert.That(distinct && moved, Is.True, "Individual glyphs must move while game time is stopped.");
                LogAssert.Expect(LogType.Warning, "Scene preparation failed: Scene is unavailable: " + configuration.titleScene);
                gate.SetResult(catalog);
                deadline = Time.realtimeSinceStartupAsDouble + 8;
                while (flow.IsTransitioning && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                yield return new WaitForEndOfFrame();
                Assert.That(preparation.IsCompletedSuccessfully, Is.True);
                Assert.That(flow.Loading.Kind, Is.EqualTo(LoadingScreenKind.Error));
                Assert.That(wave.enabled || animator.enabled, Is.False);
                var restored = GlyphOffsets(title);
                Assert.That(restored.Length, Is.GreaterThan(0));
                Assert.That(restored.All(y => Mathf.Abs(y) < .001f), Is.True, "The error title renders on its original vertices.");

                configuration.titleScene = titleScene;
                // Leave enough observation time independently of scene size or frame rate.
                configuration.minimumLoadingSeconds = 30f;
                yield return ClickLoading("Retry");
                Assert.That(flow.Loading.Kind, Is.EqualTo(LoadingScreenKind.Loading));
                Assert.That(wave.GetComponents<TextAnimator_TMP>().Single(), Is.SameAs(animator));
                yield return new WaitForEndOfFrame();
                Assert.That(GlyphOffsets(title).Any(y => Mathf.Abs(y) > .25f), Is.True);
                configuration.minimumLoadingSeconds = 0;
                yield return Settled();
                Assert.That(animator.enabled || wave.enabled, Is.False);
            }
            finally
            {
                Time.timeScale = oldTimeScale;
                configuration.titleScene = titleScene;
                gate.TrySetResult(catalog);
            }
        }

        private static float[] GlyphOffsets(TextMeshProUGUI label)
        {
            // Read the rendered mesh and TMP's layout vertices from the same frame.
            var info = label.textInfo;
            return info.characterInfo.Take(info.characterCount).Where(c => c.isVisible).Select(c =>
                info.meshInfo[c.materialReferenceIndex].vertices[c.vertexIndex].y - c.vertex_BL.position.y).ToArray();
        }
    }
}
#endif
