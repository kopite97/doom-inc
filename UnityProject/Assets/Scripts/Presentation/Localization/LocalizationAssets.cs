using UnityEngine;
using UnityEngine.Localization.Tables;
using System;

namespace MobileGame.Presentation
{
    public sealed class LocalizationAssets : ScriptableObject
    {
        public SharedTableData[] tables;
        public TextAsset bootstrapText;
    }
    [Serializable] public sealed class BootstrapTextDocument { public BootstrapTextEntry[] entries; }
    [Serializable] public sealed class BootstrapTextEntry { public string key; public BootstrapTranslation[] translations; }
    [Serializable] public sealed class BootstrapTranslation { public string language; public string text; }
}
