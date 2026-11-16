using System;
using DG.Tweening;
using UnityEngine;
namespace MobileGame.Presentation
{
    // Eight session-owned visual slots. No gameplay callbacks or global tween cancellation.
    public sealed class LaboratoryArtEffects : IDisposable
    {
        public const int Capacity = 8;
        private sealed class Slot
        {
            public SpriteRenderer Renderer;
            public Sequence Tween;
            public double Started;
            public float Duration;
        }
        private readonly LaboratoryWorldMap world;
        private readonly LaboratoryArtAssets art;
        private readonly Slot[] slots = new Slot[Capacity];
        public int ActiveCount { get; private set; }
        public LaboratoryArtEffects(LaboratoryWorldMap world, LaboratoryArtAssets art) { this.world = world; this.art = art; }
        public bool Spawn(bool birth, Vector2 position, double now)
        {
            var sprite = birth ? art?.BirthPulse : art?.ReleaseTrail;
            if (sprite == null) return false;
            Slot slot = null;
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) slots[i] = new Slot { Renderer = world.CreateSprite("CreatureEventEffect", world.Root, -20000) };
                if (slots[i].Tween == null) { slot = slots[i]; break; }
            }
            if (slot == null) return false;
            var renderer = slot.Renderer;
            renderer.sprite = sprite; renderer.transform.position = position;
            renderer.transform.localScale = birth ? new Vector3(.35f, .21f, 1) : Vector3.one;
            var tint = birth ? new Color32(162,208,184,255) : new Color32(181,200,206,255);
            renderer.color = new Color(tint.r/255f,tint.g/255f,tint.b/255f,birth?0:.7f);
            renderer.gameObject.SetActive(true);
            var sequence = DOTween.Sequence().SetAutoKill(false).Pause();
            var times = birth ? new[]{.09f,.18f,.18f} : new[]{.07f,.14f,.14f};
            var alpha = birth ? new[]{.7f,.45f,0f} : new[]{.65f,.3f,0f};
            var scales = new[]{new Vector3(.65f,.39f,1),new Vector3(1.25f,.75f,1),new Vector3(1.6f,.96f,1)};
            var offsets = new[]{new Vector2(4,2)/32,new Vector2(11,5)/32,new Vector2(16,8)/32};
            for (var i = 0; i < 3; i++)
            {
                sequence.Append(DOTween.To(()=>renderer.color.a, value=>{var c=renderer.color;c.a=value;renderer.color=c;},alpha[i],times[i]).SetEase(Ease.Linear));
                if(birth) sequence.Join(DOTween.To(()=>renderer.transform.localScale,value=>renderer.transform.localScale=value,scales[i],times[i]).SetEase(Ease.Linear));
                else sequence.Join(DOTween.To(()=>renderer.transform.position,value=>renderer.transform.position=value,(Vector3)(position+offsets[i]),times[i]).SetEase(Ease.Linear));
            }
            slot.Tween = sequence; slot.Started = now; slot.Duration = birth ? .45f : .35f; ActiveCount++;
            return true;
        }
        public void Sample(double now)
        {
            foreach(var slot in slots)
            {
                if(slot?.Tween == null) continue;
                var age = (float)Math.Max(0, now-slot.Started);
                slot.Tween.Goto(Mathf.Min(age,slot.Duration),false);
                if(age < slot.Duration) continue;
                slot.Tween.Kill();slot.Tween=null;slot.Renderer.gameObject.SetActive(false);ActiveCount--;
            }
        }
        public void Dispose()
        {
            foreach(var slot in slots)
            {
                if(slot==null) continue;slot.Tween?.Kill();slot.Tween=null;
                if(slot.Renderer!=null){slot.Renderer.gameObject.SetActive(false);UnityEngine.Object.Destroy(slot.Renderer.gameObject);}
            }
            ActiveCount=0;
        }
    }
}
