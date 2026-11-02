using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MobileGame.Foundation.Editor
{
    public sealed class TranslationReviewWindow : EditorWindow
    {
        [MenuItem("Tools/Mobile Game/Localization/Review Translations")]
        public static void Open() => GetWindow<TranslationReviewWindow>("Translation review");
        public void CreateGUI()
        {
            rootVisualElement.Clear();
            var topic = new TextField("CSV topic"); var key = new TextField("Key"); var language = new TextField("Target locale") { value = "en" };
            var details = new Label { style = { whiteSpace = WhiteSpace.Normal } };
            var scroll = new ScrollView(); scroll.Add(details); scroll.style.flexGrow = 1;
            string reviewedTopic = null, reviewedKey = null, reviewedLanguage = null, revision = null, translation = null;
            var confirm = new Button { text = "Confirm this translation was reviewed" }; confirm.SetEnabled(false);
            var inspect = new Button(() =>
            {
                confirm.SetEnabled(false);
                try
                {
                    var document = LocalizationCsvTools.ReadAll(false).Single(d => d.Topic == topic.value);
                    var row = document.Rows.Single(r => r.Key == key.value);
                    if (language.value == "ko" || !document.Languages.Contains(language.value)) throw new ArgumentException("Choose a target locale.");
                    reviewedTopic = topic.value; reviewedKey = key.value; reviewedLanguage = language.value;
                    revision = row.Revision; translation = row.Values[language.value];
                    details.text = row.Context + "\nArguments: " + row.Contract + "\n\nKorean source:\n" + row.Values["ko"]
                        + "\n\nTranslation:\n" + translation + "\n\nReview: " + (TranslationReview.IsReviewed(revision, reviewedLanguage, translation, row.Reviews[reviewedLanguage]) ? "Current" : "Required");
                    confirm.SetEnabled(true);
                }
                catch (Exception e) { details.text = e.Message; }
            }) { text = "Inspect current source and translation" };
            confirm.clicked += () =>
            {
                try
                {
                    var current = LocalizationCsvTools.ReadAll(false).Single(d => d.Topic == reviewedTopic).Rows.Single(r => r.Key == reviewedKey);
                    if (current.Revision != revision || current.Values[reviewedLanguage] != translation)
                        throw new InvalidOperationException("CSV changed since inspection. Inspect it again before confirming.");
                    LocalizationCsvTools.ConfirmReview(reviewedTopic, reviewedKey, reviewedLanguage);
                    details.text = "Review recorded for " + reviewedTopic + "/" + reviewedKey + " (" + reviewedLanguage + "). Import is a separate action.";
                }
                catch (Exception e) { details.text = e.Message; }
                confirm.SetEnabled(false);
            };
            rootVisualElement.Add(topic); rootVisualElement.Add(key); rootVisualElement.Add(language);
            rootVisualElement.Add(inspect); rootVisualElement.Add(scroll); rootVisualElement.Add(confirm);
        }
    }
}
