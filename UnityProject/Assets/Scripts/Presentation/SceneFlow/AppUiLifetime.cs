using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace MobileGame.Presentation
{
    // Shared input owner, independent of individual canvases and their disposal order.
    public sealed class AppUiLifetime : MonoBehaviour
    {
        private static AppUiLifetime instance;
        public static int InputEpoch { get; private set; }
        private static bool waitingForRelease;
        private int releaseFrame;
        public static bool AcceptInput => !Application.isPlaying || !waitingForRelease;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { instance = null; InputEpoch++; waitingForRelease = true; }
        public static void Ensure()
        {
            if (instance != null) return;
            instance = FindFirstObjectByType<AppUiLifetime>();
            if (instance == null) instance = new GameObject("AppUiLifetime").AddComponent<AppUiLifetime>();
            instance.Configure();
        }
        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this; DontDestroyOnLoad(gameObject); Configure();
        }
        private void Configure()
        {
            foreach (var other in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                if (other.gameObject != gameObject) other.gameObject.SetActive(false);
            if (GetComponent<EventSystem>() == null) gameObject.AddComponent<EventSystem>();
            if (GetComponent<InputSystemUIInputModule>() == null)
                gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        public static void Invalidate()
        {
            InputEpoch++; waitingForRelease = true;
            if (instance != null) instance.releaseFrame = Time.frameCount + 1;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        private void Update()
        {
            if (!waitingForRelease) return;
            var held = Mouse.current?.leftButton.isPressed == true || Keyboard.current?.enterKey.isPressed == true
                || Keyboard.current?.spaceKey.isPressed == true || Keyboard.current?.escapeKey.isPressed == true
                || Gamepad.current?.buttonSouth.isPressed == true || Gamepad.current?.buttonEast.isPressed == true;
            if (Touchscreen.current != null)
                foreach (var touch in Touchscreen.current.touches) held |= touch.press.isPressed;
            if (held) releaseFrame = Time.frameCount + 1;
            else if (Time.frameCount > releaseFrame) waitingForRelease = false;
        }
        private void OnDestroy() { if (instance == this) { instance = null; Invalidate(); } }
    }
}
