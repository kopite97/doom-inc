using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MobileGame.Foundation
{
    /// <summary>Review stamps are explicit author decisions, never a side effect of importing tables.</summary>
    public static class TranslationReview
    {
        public static string SourceRevision(string source, string context, string argumentContract, bool smart)
            => Hash(source, context, argumentContract, smart ? "smart" : "plain");

        // Include the translated value: changing the translation also requires a fresh review.
        public static string ReviewStamp(string sourceRevision, string language, string translation)
            => Hash(sourceRevision, language, translation);

        public static bool IsReviewed(string sourceRevision, string language, string translation, string recordedStamp)
            => !string.IsNullOrWhiteSpace(translation) && !string.IsNullOrWhiteSpace(recordedStamp)
                && string.Equals(ReviewStamp(sourceRevision, language, translation), recordedStamp, StringComparison.Ordinal);

        private static string Hash(params string[] fields)
        {
            var canonical = new StringBuilder();
            foreach (var field in fields)
            {
                var value = field ?? "";
                canonical.Append(value.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(value);
            }
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())))
                    .Replace("-", "").ToLowerInvariant();
        }
    }
}
