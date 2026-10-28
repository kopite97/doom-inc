using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MobileGame.TestSupport
{
    // Deliberately invalid/mutable fixtures are confined to tests; production Data has no setters.
    public static class DataTestAssets
    {
        private static GameDataCatalog cached;
        public static GameDataCatalog Catalog => GameDataStore.Instance != null && GameDataStore.Instance.State == GameDataLoadState.Ready
            ? GameDataStore.Instance.Catalog : cached ?? (cached = GameDataAssets.LoadCatalog());
        public static T LoadAssetAtPath<T>(string path) where T : ThingData
        {
            var source = AssetDatabase.LoadAssetAtPath<ThingDefinition>(path);
            return source == null ? null : Catalog.Get<T>(source.DefinitionId);
        }
        public static T Create<T>() where T : ThingData
        {
            var data = (T)Activator.CreateInstance(typeof(T), true);
            typeof(ThingData).GetField("<Settings>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(data, Catalog.Settings);
            if (data is FacilityData)
            {
                Set(data, "footprint", Vector2Int.one); Set(data, "interactionOffset", new Vector2Int(0, -1));
                Set(data, "buildCost", 2); Set(data, "buildSeconds", 3f); Set(data, "storageCapacity", 20);
                Set(data, "gatheringAmount", 5); Set(data, "gatheringSeconds", 5f);
                Set(data, "inputAmount", 5); Set(data, "outputAmount", 1); Set(data, "productionSeconds", 10f);
            }
            if (data is CreatureData) { Set(data, "carryCapacity", 5); Set(data, "moveSecondsPerCell", 0.25f); }
            return data;
        }
        internal static FieldInfo Field(object target, string name)
        {
            for (var type = target.GetType(); type != null; type = type.BaseType)
            { var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly); if (field != null) return field; }
            throw new ArgumentException("Missing fixture field: " + name);
        }
        internal static void Set(object target, string field, object value) => Field(target, field).SetValue(target, value);
    }

    public sealed class DataFixtureEditor : IDisposable
    {
        private readonly object target;
        public DataFixtureEditor(object target) => this.target = target;
        public FixtureProperty FindProperty(string name)
        {
            var field = DataTestAssets.Field(target, name);
            return new FixtureProperty(field.FieldType, () => field.GetValue(target), v => field.SetValue(target, v));
        }
        public void ApplyModifiedPropertiesWithoutUndo() { }
        public void Dispose() { }
    }
    public sealed class FixtureProperty
    {
        private readonly Type type; private readonly Func<object> get; private readonly Action<object> set;
        internal FixtureProperty(Type type, Func<object> get, Action<object> set) { this.type = type; this.get = get; this.set = set; }
        public string stringValue { get => (string)get(); set => set(value); }
        public int intValue { get => Convert.ToInt32(get()); set => set(type.IsEnum ? Enum.ToObject(type, value) : (object)value); }
        public float floatValue { get => (float)get(); set => set(value); }
        public Vector2Int vector2IntValue { get => (Vector2Int)get(); set => set(value); }
        public object objectReferenceValue { get => get(); set => set(value); }
        public int arraySize
        {
            get => ((Array)get())?.Length ?? 0;
            set { var array = Array.CreateInstance(type.GetElementType(), value); var previous = (Array)get(); if (previous != null) Array.Copy(previous, array, Math.Min(previous.Length, value)); set(array); }
        }
        public FixtureProperty GetArrayElementAtIndex(int index) => new FixtureProperty(type.GetElementType(), () => ((Array)get()).GetValue(index), value => ((Array)get()).SetValue(value, index));
    }
    public static class DataFixtureSnapshot
    {
        public static string ToJson(object target)
        {
            if (target is UnityEngine.Object asset) return EditorJsonUtility.ToJson(asset);
            var fields = Enumerable.Empty<FieldInfo>();
            for (var type = target.GetType(); type != null; type = type.BaseType)
                fields = fields.Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
            return string.Join(";", fields.OrderBy(f => f.Name).Select(f => f.Name + "=" + Value(f.GetValue(target))));
        }
        private static string Value(object value) => value is Array array ? string.Join(",", array.Cast<object>().Select(Value))
            : value is ThingData data ? data.DefinitionId : Convert.ToString(value, CultureInfo.InvariantCulture);
    }
}
