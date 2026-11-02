#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Foundation;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MobileGame.SpatialTests
{
    public sealed class LocalizationRuntimeTests
    {
        [UnityTest]
        public IEnumerator OpenConfirmationRetranslatesCapturedNestedMessageWithoutInvokingOrReplacingAction()
        {
            var assets = AssetDatabase.LoadAssetAtPath<LocalizationAssets>("Assets/SO/Localization/LocalizationAssets.asset");
            var directory = Path.Combine(Path.GetTempPath(), "MobileGame-PLAN018-Prompt-" + Guid.NewGuid().ToString("N"));
            var owner = new GameObject("Localization prompt fixture");
            using (var app = new AppLocalization(assets, Path.Combine(directory, "language.json")))
            {
                try
                {
                    yield return Wait(app.InitializeAsync()); yield return Wait(app.SelectLanguageAsync("ko"));
                    var loading = owner.AddComponent<LoadingScreen>(); loading.Configure(null, () => false, app);
                    var message = new LocalizedMessage("Laboratory", "stock", new TextArgument("resource", new LocalizedMessage("Definitions", "resource.culture-medium")), new TextArgument("count", 12));
                    var calls = 0; loading.Show(LoadingScreenKind.Confirmation, message, () => calls++);
                    yield return null; yield return null; yield return null;
                    var retry = owner.GetComponentsInChildren<UiButton>().Single(b => b.name == "Retry");
                    var pointer = new PointerEventData(EventSystem.current) { pointerId = 91 }; retry.OnPointerDown(pointer);
                    yield return Wait(app.SelectLanguageAsync("en")); yield return null; yield return null;
                    retry.OnPointerClick(pointer);
                    Assert.That(calls, Is.Zero); Assert.That(loading.Visible, Is.True); Assert.That(loading.Kind, Is.EqualTo(LoadingScreenKind.Confirmation));
                    Assert.That(owner.GetComponentsInChildren<Text>().Single(t => t.name == "Status").text, Is.EqualTo("Stock: 12 Culture medium"));
                    Assert.That(message.Arguments["count"], Is.EqualTo(12));
                    Assert.That(owner.GetComponentsInChildren<UiButton>().Single(b => b.name == "Retry"), Is.SameAs(retry));
                    yield return null;
                    retry.OnPointerDown(pointer); retry.OnPointerClick(pointer); Assert.That(calls, Is.EqualTo(1));
                }
                finally
                {
                    UnityEngine.Object.Destroy(owner);
                    if (Directory.Exists(directory)) Directory.Delete(directory, true);
                }
            }
        }

        [UnityTest]
        public IEnumerator PackageLoadsCsvTablesAndSwitchesLanguageWithoutContentBuild()
        {
            var assets = AssetDatabase.LoadAssetAtPath<LocalizationAssets>("Assets/SO/Localization/LocalizationAssets.asset");
            Assert.That(assets, Is.Not.Null);
            using (var service = new UnityLocalizationService(assets, "ko", () => "Unavailable"))
            using (var texts = new TextManager(service))
            {
                yield return Wait(service.InitializeAsync(CancellationToken.None));
                Assert.That(texts.Format(new LocalizedMessage("Title", "new_game")), Is.EqualTo("새 게임"));
                yield return Wait(service.ChangeLanguageAsync("en", CancellationToken.None));
                Assert.That(texts.Format(new LocalizedMessage("Title", "new_game")), Is.EqualTo("New Game"));
                var nested = new LocalizedMessage("Common", "text_setting", new TextArgument("value", new LocalizedMessage("Common", "large")));
                Assert.That(texts.Format(nested), Is.EqualTo("Text size: Large"));
                yield return Wait(service.ChangeLanguageAsync("ko", CancellationToken.None));
                Assert.That(texts.Format(nested), Is.EqualTo("글자 크기: 크게"));
            }
            LogAssert.NoUnexpectedReceived();
        }

        private static IEnumerator Wait(Task task)
        {
            var until = Time.realtimeSinceStartup + 45;
            while (!task.IsCompleted && Time.realtimeSinceStartup < until) yield return null;
            Assert.That(task.IsCompleted, Is.True, "Localization operation timed out.");
            if (task.IsFaulted) throw task.Exception.GetBaseException();
            Assert.That(task.IsCanceled, Is.False);
        }
    }
}
#endif
