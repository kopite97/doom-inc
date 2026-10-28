using UnityEditor;
namespace MobileGame.Foundation.Editor
{ public static class InvasionDefinitionAssets
{ [MenuItem("Tools/Mobile Game/Upgrade Invasion Laboratory")] public static void Upgrade() => GameDataAssets.Migrate(); } }
