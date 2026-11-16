using UnityEngine;
namespace MobileGame.Foundation
{
    // Immutable art references shared by screen-owned UI contexts.
    public sealed class UiSkinAssets : ScriptableObject
    {
        public Sprite panel, button, selection, scrollThumb, hud, destructionTrack;
    }
}
