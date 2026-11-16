#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest, Category("BatchArtIntegration")]
        public IEnumerator BatchArtNonWorkersBirthReleasePauseRestoreAndBoundary()
        {
            using(var input = new PointerScene())
            {
                yield return input.Load(); var view=input.View;var map=input.Map;
                Assert.That(map.World.Root.Find("LaboratoryBoundary"),Is.Not.Null);
                Assert.That(map.World.Root.Find("LaboratoryBoundary").childCount,Is.EqualTo(2*view.Simulation.Map.Width+2*view.Simulation.Map.Height+4));
                foreach(var id in new[]{"creature.invader","creature.raider"})
                {
                    var creature=view.Simulation.Registry.Create(GameDataStore.Instance.Catalog.Get<CreatureData>(id));
                    Assert.That(view.Simulation.Map.Commands.Place(creature.Id,new Vector2Int(2,2)).Succeeded,Is.True);
                }
                yield return null;yield return null;
                var bodies=map.World.Root.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name=="Body"&&r.sprite!=null&&
                    (r.sprite.name.StartsWith("Invader_")||r.sprite.name.StartsWith("Raider_"))).ToArray();
                Assert.That(bodies.Length,Is.EqualTo(2));
                foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(720,1280)})
                using(new LaboratoryGameViewSize(size.x,size.y))
                {
                    yield return WaitFor(()=>Screen.width==size.x&&Screen.height==size.y,"Resize failed");
                    yield return null;yield return null;
                    yield return AppSceneFlowTests.Capture("batch-creatures-"+size.x);
                }
                yield return null; yield return null;
                view.SendMessage("OnApplicationFocus",true);
                var beforeTime=view.Simulation.ElapsedSeconds;
                var first=bodies.Select(r=>r.sprite).ToArray();view.AdvanceFrame(.3,0);yield return null;
                Assert.That(view.Simulation.ElapsedSeconds,Is.GreaterThan(beforeTime));
                Assert.That(bodies.Select(r=>r.sprite),Is.Not.EqualTo(first));
                Assert.That(view.ReleaseInvaders(),Is.True);yield return null;
                Assert.That(map.ActiveCreatureEffects,Is.EqualTo(2));
                yield return AppSceneFlowTests.Capture("release-effect");
                Assert.That(view.ReleaseInvaders(),Is.False);Assert.That(map.ActiveCreatureEffects,Is.EqualTo(2));
                var effects=map.World.Root.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name=="CreatureEventEffect").ToArray();
                var before=effects.Select(r=>r.transform.position).ToArray();
                view.SetTransitionPaused(true);view.AdvanceFrame(1,1);yield return null;
                Assert.That(effects.Select(r=>r.transform.position),Is.EqualTo(before));
                view.SetTransitionPaused(false);view.AdvanceFrame(.4,0);yield return null;
                Assert.That(map.ActiveCreatureEffects,Is.Zero);
                // Released invaders would reach the game goal during the long cultivation wait.
                // Start a fresh paid run for the independent birth scenario.
                Assert.That(view.Restart(),Is.True);yield return null;yield return null;
                var facility=CreateStatusLayoutFacility(view,"cultivationId",new Vector2Int(6,5),QuarterTurn.None,true);
                Assert.That(view.SelectCultivation(facility.Id,"creature.invader"),Is.True);
                view.Simulation.Registry.CreateResource(view.Simulation.Culture,2,facility.GetComponent<ResourceContainerComponent>());
                var born=0;view.Simulation.CreatureBorn+=(id,cell)=>born++;
                for(var i=0;i<1100&&born==0;i++){view.AdvanceFrame(.1,0);yield return null;}
                Assert.That(born,Is.EqualTo(1));Assert.That(map.ActiveCreatureEffects,Is.EqualTo(1));
                view.AdvanceFrame(.1,0);yield return null;yield return AppSceneFlowTests.Capture("birth-effect");
                view.SelectThing(facility.Id);view.ExpandSelection();yield return null;yield return null;
                foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(720,1280)})
                using(new LaboratoryGameViewSize(size.x,size.y))
                {
                    yield return WaitFor(()=>Screen.width==size.x&&Screen.height==size.y,"Panel resize failed");
                    yield return null;yield return null;
                    foreach(var indicator in view.UiCanvas.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i=>i.name=="ScrollPosition"&&i.enabled))
                    {
                        var parent=(RectTransform)indicator.transform.parent;
                        Assert.That(indicator.rectTransform.anchoredPosition.x,Is.EqualTo(parent.rect.width-7).Within(.01),"Passive scroll indicator must stay at the panel edge.");
                    }
                    yield return AppSceneFlowTests.Capture("laboratory-panel-final-"+size.x);
                }
                Assert.That(view.Save()&&view.Reload(),Is.True);yield return null;yield return null;
                Assert.That(map.ActiveCreatureEffects,Is.Zero,"Restoring a creature must not replay its birth.");
                Assert.That(view.Restart(),Is.True);yield return null;Assert.That(map.ActiveCreatureEffects,Is.Zero);
                LogAssert.NoUnexpectedReceived();
            }
        }
        [UnityTest, Category("BatchArtIntegration")]
        public IEnumerator BatchArtEffectSlotsHaveBoundedLifetimeAndNoCompletionSideEffects()
        {
            using(var input=new PointerScene())
            {
                yield return input.Load();
                using(var effects=new LaboratoryArtEffects(input.Map.World,input.View.UiAssets.Art))
                {
                    for(var i=0;i<8;i++) Assert.That(effects.Spawn(i%2==0,Vector2.zero,10),Is.True);
                    Assert.That(effects.Spawn(true,Vector2.zero,10),Is.False);
                    effects.Sample(10.1);Assert.That(effects.ActiveCount,Is.EqualTo(8));
                    effects.Sample(10.1);Assert.That(effects.ActiveCount,Is.EqualTo(8));
                    effects.Sample(10.36);Assert.That(effects.ActiveCount,Is.EqualTo(4));
                    effects.Sample(10.46);Assert.That(effects.ActiveCount,Is.Zero);
                    Assert.That(effects.Spawn(true,Vector2.zero,11),Is.True);
                }
                yield return null;LogAssert.NoUnexpectedReceived();
            }
        }
    }
}
#endif
