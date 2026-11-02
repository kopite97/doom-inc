using System.Collections.Generic;
using UnityEngine;

namespace MobileGame.Foundation
{
    [CreateAssetMenu(menuName = "Mobile Game/UI/Popup Registry")]
    public sealed class PopupRegistry : ScriptableObject
    {
        [SerializeField] private PopupResource[] entries = new PopupResource[0];
        public IReadOnlyList<PopupResource> Entries => entries;
        public void Configure(params PopupResource[] values) { entries = (PopupResource[])values.Clone(); }
    }
}
