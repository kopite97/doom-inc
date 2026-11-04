using Febucci.TextAnimatorCore.Time;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using TMPro;
using UnityEngine;

namespace MobileGame.Presentation
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class LoadingTextWave : MonoBehaviour
    {
        private TextAnimator_TMP animator;

        private void OnEnable()
        {
            if (animator == null)
            {
                animator = gameObject.AddComponent<TextAnimator_TMP>();
                animator.localSettings.timeScale = TimeScale.Unscaled;
                animator.localSettings.defaultBehaviorTags = new[] { "wave" };
                animator.localSettings.isAnimatingAppearances = false;
                animator.localSettings.isAnimatingDisappearances = false;
            }
            animator.enabled = true;
        }

        private void OnDisable()
        {
            if (animator == null) return;
            animator.enabled = false;
            // Text Animator takes over TMP rendering while active. Return the label to TMP for errors.
            animator.TMProComponent.renderMode = TextRenderFlags.Render;
            animator.TMProComponent.havePropertiesChanged = true;
            animator.TMProComponent.SetVerticesDirty();
        }
    }
}
