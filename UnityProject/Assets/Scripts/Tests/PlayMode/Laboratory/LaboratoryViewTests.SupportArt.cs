#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator SupportArtProductionDirectionsWaitingSelectionAndIcons()
        {
            using var input = new PointerScene();
            yield return input.Load(); var view = input.View;
            var directions = new[] { "Up", "Right", "Down", "Left" };
            for (var rotation=0; rotation<4; rotation++)
            {
                if (rotation>0) { Assert.That(view.Restart(), Is.True); yield return null; yield return null; }
                var lab = view.Simulation;
                var prototype = lab.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                Assert.That(view.SetWorkerPriorities(prototype.Id,new int[4]), Is.True);
                var facility = CreateStatusLayoutFacility(view,"processingId",new Vector2Int(6,4),(QuarterTurn)rotation,true);
                var worker = lab.Registry.Create(GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId"));
                Assert.That(lab.Map.Commands.Place(worker.Id,facility.Placement.InteractionCell.Value).Succeeded, Is.True);
                Assert.That(view.AssignWorker(worker.Id,facility.Id), Is.True);
                Assert.That(view.SetWorkerPriorities(worker.Id,new[]{0,0,0,1}), Is.True);
                lab.OnMapChanged(); view.AdvanceFrame(.1,0); yield return null; yield return null;
                Assert.That(ArtSprite(input.Map,worker,"Body").sprite.name, Does.Contain("_Idle_"), "Assignment without inputs must not animate work.");
                lab.Registry.CreateResource(lab.Organic,5,facility.GetComponent<ResourceContainerComponent>());
                yield return AdvanceArtUntil(view,()=>worker.GetComponent<WorkerComponent>().Job?.Stage==JobStage.Working,5);
                yield return null; yield return null;
                Assert.That(ArtSprite(input.Map,worker,"Body").sprite.name, Does.StartWith("Support_Work_"+directions[rotation]+"_"));
                var frames = new HashSet<Sprite>();
                for(var i=0;i<12;i++) { view.AdvanceFrame(.06,0); yield return null; frames.Add(ArtSprite(input.Map,worker,"Body").sprite); }
                Assert.That(frames.Count, Is.EqualTo(6));
                Assert.That(ArtSprite(input.Map,prototype,"Body").sprite.name, Does.StartWith("PrototypeSupport_"));
                Assert.That(view.SelectThing(worker.Id), Is.True); yield return null; yield return null;
                Assert.That(ArtSprite(input.Map,worker,"Body").sharedMaterial, Is.SameAs(view.UiAssets.SelectionMaterial));
                if(rotation==0)
                {
                    foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(720,1280)})
                    using(new LaboratoryGameViewSize(size.x,size.y))
                    {
                        yield return SettleScreenStateSize(size);
                        input.Map.Fit(); input.Map.ZoomBy(2); yield return null; yield return null;
                        var path=Path.Combine(RegressionEvidenceRoot,"support-r002",size.x>size.y?"landscape.png":"portrait.png");
                        yield return AppSceneFlowTests.Capture("support-r002",path);
                    }
                    yield return ClickVisibleButton(input,"NavCreatures"); yield return null;
                    var icon=view.FindUiButton("Select_"+worker.Id.ToString("N")).transform.Find("DefinitionIcon").GetComponent<Image>();
                    Assert.That(icon.sprite,Is.SameAs(worker.Definition.Assets.Icon));
                    Assert.That(icon.raycastTarget,Is.False);
                    var cultivation=CreateStatusLayoutFacility(view,"cultivationId",new Vector2Int(10,4),QuarterTurn.None,true);
                    Assert.That(view.SelectThing(cultivation.Id),Is.True); yield return null; yield return null;
                    view.ExpandSelection(); yield return null; yield return null;
                    var targetIcon=view.FindUiButton("CultivateSupport").transform.Find("DefinitionIcon").GetComponent<Image>();
                    Assert.That(targetIcon.sprite,Is.SameAs(worker.Definition.Assets.Icon));
                }
                yield return AdvanceArtUntil(view,()=>facility.GetComponent<ProductionComponent>().Outstanding>0,30);
                view.AdvanceFrame(.2,0); yield return null; yield return null;
                Assert.That(ArtSprite(input.Map,worker,"Body").sprite.name,Does.Contain("_Idle_"));
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
