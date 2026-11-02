using UnityEngine;

namespace MobileGame.Foundation
{
    [CreateAssetMenu(menuName = "Mobile Game/UI/Popup Resource")]
    public sealed class PopupResource : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private GameObject prefab;
        public string Id => id;
        public GameObject Prefab => prefab;
        public void Configure(string value, GameObject asset) { id = value; prefab = asset; }
    }
}
