using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MobileGame.Presentation;
using NUnit.Framework;

namespace MobileGame.Foundation.Tests
{
    public sealed class TextManagementTests
    {
        internal sealed class Service : ILocalizationService
        {
            public bool IsReady { get; set; } = true;
            public string CurrentLanguage { get; private set; } = "ko";
            public IReadOnlyList<LanguageOption> Languages { get; } = new[]
                { new LanguageOption("ko", "한국어"), new LanguageOption("en", "English") };
            public event Action LanguageChanged;
            public string MissingEnglishKey;
            public bool HasEntry(string table, string key, string language) => language != "en" || key != MissingEnglishKey;
            public Task InitializeAsync(CancellationToken token) => Task.CompletedTask;
            public Task ChangeLanguageAsync(string code, CancellationToken token)
            { token.ThrowIfCancellationRequested(); CurrentLanguage = code; LanguageChanged?.Invoke(); return Task.CompletedTask; }
            public string Format(string table, string key, string language, IReadOnlyDictionary<string, object> args)
            {
                if (table == "Definitions" && key == "culture") return language == "ko" ? "배양액" : "Culture medium";
                if (key == "stock") return language == "ko" ? $"보유 {args["resource"]} {args["count"]}개"
                    : $"Owned {args["resource"]}: {args["count"]}";
                return table + "/" + key;
            }
            public void Dispose() { LanguageChanged = null; }
        }

        [Test]
        public async Task OpenMessageTranslatesNestedNameAndRetainsCapturedCount()
        {
            using (var service = new Service())
            using (var texts = new TextManager(service))
            {
                var args = new[] { new TextArgument("resource", new LocalizedMessage("Definitions", "culture")),
                    new TextArgument("count", 10) };
                var notice = new LocalizedMessage("Laboratory", "stock", args);
                args[1] = new TextArgument("count", 500); // External mutation must not change the event snapshot.
                string label = null; var updates = 0;
                using (texts.Observe(notice, value => { label = value; updates++; }))
                {
                    Assert.That(label, Is.EqualTo("보유 배양액 10개"));
                    await service.ChangeLanguageAsync("en", CancellationToken.None);
                    Assert.That(label, Is.EqualTo("Owned Culture medium: 10"));
                    Assert.That(updates, Is.EqualTo(2));
                    Assert.That(notice.Arguments["count"], Is.EqualTo(10));
                }
                await service.ChangeLanguageAsync("ko", CancellationToken.None);
                Assert.That(updates, Is.EqualTo(2), "Closed/reused views must not receive old subscriptions.");
            }
        }

        [Test]
        public async Task MissingNestedTranslationFallsBackAsOneWholeMessage()
        {
            using (var service = new Service { MissingEnglishKey = "culture" })
            using (var texts = new TextManager(service))
            {
                await service.ChangeLanguageAsync("en", CancellationToken.None);
                Assert.That(texts.Format(new LocalizedMessage("Laboratory", "stock",
                    new TextArgument("resource", new LocalizedMessage("Definitions", "culture")), new TextArgument("count", 7))),
                    Is.EqualTo("보유 배양액 7개"));
            }
        }

        [Test]
        public void MessagesRejectMutableArgumentsAndDuplicateNames()
        {
            Assert.Throws<ArgumentException>(() => new LocalizedMessage("T", "K", new TextArgument("value", new List<int>())));
            Assert.Throws<ArgumentException>(() => new LocalizedMessage("T", "K", new TextArgument("x", 1), new TextArgument("x", 2)));
        }

        [Test]
        public async Task DisposingManagerRemovesLanguageSubscriptions()
        {
            using (var service = new Service())
            {
                var texts = new TextManager(service); var updates = 0;
                using (texts.Observe(new LocalizedMessage("Definitions", "culture"), _ => updates++))
                {
                    texts.Dispose();
                    await service.ChangeLanguageAsync("en", CancellationToken.None);
                    Assert.That(updates, Is.EqualTo(1));
                    Assert.Throws<ObjectDisposedException>(() => texts.Format(new LocalizedMessage("T", "K")));
                }
            }
        }

        [Test]
        public void UnreadyServiceDoesNotPresentPartialData()
        {
            using (var service = new Service { IsReady = false })
            using (var texts = new TextManager(service))
                Assert.Throws<InvalidOperationException>(() => texts.Format(new LocalizedMessage("Definitions", "culture")));
        }

        [TestCase("생산 일시정지", "button", "", false)]
        [TestCase("생산 중지", "confirmation", "", false)]
        [TestCase("생산 중지", "button", "count:number", false)]
        [TestCase("생산 중지", "button", "", true)]
        public void SourceContextContractOrSmartChangesRequireExplicitReview(string source, string context, string contract, bool smart)
        {
            var previous = TranslationReview.SourceRevision("생산 중지", "button", "", false);
            var approved = TranslationReview.ReviewStamp(previous, "en", "Stop production");
            var changed = TranslationReview.SourceRevision(source, context, contract, smart);
            Assert.That(TranslationReview.IsReviewed(changed, "en", "Stop production", approved), Is.False);
            var confirmed = TranslationReview.ReviewStamp(changed, "en", "Stop production");
            Assert.That(TranslationReview.IsReviewed(changed, "en", "Stop production", confirmed), Is.True,
                "A reviewer can confirm that an unchanged translation is still appropriate.");
        }

        [Test]
        public void TranslationEditsAndMissingValuesCannotReuseReviewStamp()
        {
            var revision = TranslationReview.SourceRevision("확인", "button", "", false);
            var stamp = TranslationReview.ReviewStamp(revision, "en", "Confirm");
            Assert.That(TranslationReview.IsReviewed(revision, "en", "Accept", stamp), Is.False);
            Assert.That(TranslationReview.IsReviewed(revision, "en", "", stamp), Is.False);
            Assert.That(TranslationReview.IsReviewed(revision, "en", "Confirm", null), Is.False);
            Assert.That(TranslationReview.IsReviewed(revision, "en", "Confirm", stamp), Is.True);
        }
    }
}
