#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest, Category("FacilityAnimation")]
        public IEnumerator FacilityArtObservesActualProductionPauseAndCommittedTransfers()
        {
            using (var input = new PointerScene())
            {
                yield return input.Load(); var view = input.View; var lab = view.Simulation;
                var worker = lab.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
                Assert.That(view.SetWorkerPriorities(worker.Id, new int[4]), Is.True);
                var processing = CreateStatusLayoutFacility(view, "processingId", new Vector2Int(2,3), QuarterTurn.None, true);
                var cultivation = CreateStatusLayoutFacility(view, "cultivationId", new Vector2Int(6,3), QuarterTurn.None, true);
                var store = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(10,3), QuarterTurn.None, true);
                var other = CreateStatusLayoutFacility(view, "storageId", new Vector2Int(10,7), QuarterTurn.Clockwise90, true);
                lab.OnMapChanged(); yield return null; yield return null;
                foreach (var thing in new[] { processing, cultivation, store, other })
                    Assert.That(ArtSprite(input.Map, thing, "Body").sprite.name, Does.Contain("_Idle_"));
                lab.Registry.CreateResource(lab.Organic, 5, processing.GetComponent<ResourceContainerComponent>());
                lab.Registry.CreateResource(lab.Culture, 2, cultivation.GetComponent<ResourceContainerComponent>());
                Assert.That(view.SelectCultivation(cultivation.Id, CultivationTarget.Support), Is.True);
                var completions = new List<Guid>(); lab.ProductionCompleted += completions.Add;
                view.AdvanceFrame(.75, 0); yield return null;
                Assert.That(processing.GetComponent<ProductionComponent>().Progress, Is.EqualTo(.15).Within(1e-7));
                Assert.That(ArtSprite(input.Map, processing, "Body").sprite.name, Is.EqualTo("Processing_Working_02"));
                Assert.That(ArtSprite(input.Map, cultivation, "Body").sprite.name, Is.EqualTo("Cultivation_Working_02"));
                var paused = ArtSprite(input.Map, processing, "Body").sprite;
                var time = lab.ElapsedSeconds;
                Assert.That(view.BeginStorageConstruction(), Is.True);
                for (var i=0; i<3; i++) { view.AdvanceFrame(.5,0); yield return null; }
                Assert.That(lab.ElapsedSeconds, Is.EqualTo(time));
                Assert.That(ArtSprite(input.Map, processing, "Body").sprite, Is.SameAs(paused));
                Assert.That(view.CancelPlacement(), Is.True);
                for(var frame=0;frame<8;frame++)
                {
                    view.AdvanceFrame(.625,0); yield return null; yield return null;
                    yield return CaptureFacilities("working-"+frame.ToString("00"));
                    yield return null;
                }
                view.AdvanceFrame(44.3,0); yield return null;
                Assert.That(completions.Count(id=>id==processing.Id), Is.EqualTo(1));
                Assert.That(ArtSprite(input.Map, processing, "Body").sprite.name, Does.StartWith("Processing_Complete_"));
                yield return CaptureFacilities("processing-complete"); yield return null;
                view.AdvanceFrame(.55,0); yield return null;
                Assert.That(ArtSprite(input.Map, processing, "Body").sprite.name, Does.StartWith("Processing_Idle_"));
                view.AdvanceFrame(49.45,0); yield return null;
                Assert.That(completions.Count(id=>id==cultivation.Id), Is.EqualTo(1));
                Assert.That(ArtSprite(input.Map, cultivation, "Body").sprite.name, Does.StartWith("Cultivation_Complete_"));
                yield return CaptureFacilities("cultivation-complete"); yield return null;
                Assert.That(lab.Registry.Things.Count(t=>t.Definition.DefinitionId=="creature.support"), Is.EqualTo(1));
                view.AdvanceFrame(.45,0); yield return null;
                Assert.That(ArtSprite(input.Map, cultivation, "Body").sprite.name, Does.StartWith("Cultivation_Idle_"));
                Assert.That(completions.Count(id=>id==processing.Id), Is.EqualTo(1), "Output waiting must not replay completion.");

                var stock = lab.Registry.CreateResource(lab.Organic,4,store.GetComponent<ResourceContainerComponent>());
                Assert.That(ArtSprite(input.Map,store,"Body").sprite.name, Does.StartWith("Storage_Idle_"), "Creation is not receipt.");
                var split=lab.Registry.TransferResource(stock,other.GetComponent<ResourceContainerComponent>(),2);
                view.AdvanceFrame(.15,0); yield return null;
                Assert.That(ArtSprite(input.Map,store,"Body").sprite.name, Is.EqualTo("Storage_Send_02"));
                Assert.That(ArtSprite(input.Map,other,"Body").sprite.name, Is.EqualTo("Storage_Receive_02"));
                yield return CaptureFacilities("actual-production-and-transfer");
                lab.Registry.TransferResource(split,store.GetComponent<ResourceContainerComponent>());
                view.AdvanceFrame(.5,0); yield return null; yield return null;
                Assert.That(ArtSprite(input.Map,store,"Body").sprite.name, Does.StartWith("Storage_Receive_"));
                Assert.That(ArtSprite(input.Map,other,"Body").sprite.name, Does.StartWith("Storage_Send_"));
                var codec=new SessionSnapshotCodec(GameDataStore.Instance.Catalog.Definitions.Values,true);
                var before=codec.Serialize(codec.Capture(view.Controller.Current,view.Controller.Phase));
                for(var i=0;i<5;i++) input.Map.PublishFrame();
                Assert.That(codec.Serialize(codec.Capture(view.Controller.Current,view.Controller.Phase)), Is.EqualTo(before));
                Assert.That(view.Save(), Is.True); var json=File.ReadAllText(view.SavePath);
                yield return input.Reopen(); view=input.View;
                Assert.That(File.ReadAllText(view.SavePath), Is.EqualTo(json));
                Assert.That(view.Simulation.Registry.TryGet(store.Id,out store), Is.True);
                Assert.That(ArtSprite(input.Map,store,"Body").sprite.name, Does.StartWith("Storage_Idle_"), "Restore must not replay transfers.");
                view.ReleaseSession(); yield return null; yield return null;
                lab.Registry.TransferResource(stock,other.GetComponent<ResourceContainerComponent>());
                Assert.That(input.Map.World.Root.gameObject.activeSelf, Is.False);
                LogAssert.NoUnexpectedReceived();
            }
        }

        [UnityTest, Category("FacilityAnimation")]
        public IEnumerator FacilityArtKeepsFourRotationsPreviewSelectionAndBothLayouts()
        {
            using(var input=new PointerScene())
            {
                yield return input.Load(); var view=input.View;
                var definitions=new[]{"processingId","cultivationId","storageId"};
                var names=new[]{"Processing","Cultivation","Storage"};
                var created=new List<Thing>();
                for(var r=0;r<4;r++)
                {
                    if(r>0) { Assert.That(view.Restart(),Is.True); yield return null; yield return null; }
                    created.Clear();
                    for(var k=0;k<3;k++)
                        created.Add(CreateStatusLayoutFacility(view,definitions[k],new Vector2Int(1+k*5,4),(QuarterTurn)r,true));
                    view.Simulation.OnMapChanged(); yield return null; yield return null;
                    for(var i=0;i<created.Count;i++)
                    {
                        var thing=created[i]; var body=ArtSprite(input.Map,thing,"Body");
                        Assert.That(body.sprite.name, Does.StartWith(names[i]+"_Idle_"));
                        Assert.That(Quaternion.Angle(body.transform.localRotation,Quaternion.Euler(0,0,-90*r)), Is.LessThan(.001f));
                        Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one));
                        Assert.That(body.sprite.rect.size, Is.EqualTo(new Vector2(i==2?192:128,128)));
                    }
                    input.Map.Fit(); yield return null; yield return null;
                    yield return CaptureFacilities("rotation-"+r+"-landscape");
                }
                Assert.That(view.BeginStorageConstruction(), Is.True); Assert.That(view.PreviewAt(new Vector2Int(6,7)), Is.True);
                Assert.That(view.RotatePreview(), Is.True); yield return null; yield return null;
                var preview=input.Map.World.Root.Find("PlacementPreview/Body").GetComponent<SpriteRenderer>();
                Assert.That(preview.sprite.name, Is.EqualTo("Storage_Idle_01"));
                Assert.That(Quaternion.Angle(preview.transform.localRotation,Quaternion.Euler(0,0,-90)), Is.LessThan(.001f));
                Assert.That(view.CancelPlacement(), Is.True);
                input.Map.Fit(); input.Map.ZoomBy(2); yield return null; yield return null;
                var selected=created[1];
                Assert.That(input.Map.TryThingScreenPoint(selected.Id,out var point), Is.True);
                yield return input.Tap(429,point);
                Assert.That(view.SelectedId, Is.EqualTo(selected.Id));
                yield return CaptureFacilities("selected-landscape");
                using(var portrait=new LaboratoryGameViewSize(720,1280))
                {
                    yield return SettleStatusLayoutSize(new Vector2Int(720,1280)); input.Map.Fit();
                    yield return null; yield return null; yield return CaptureFacilities("all-rotations-portrait");
                }
                Assert.That(view.Restart(), Is.True); yield return null; yield return null;
                Assert.That(input.Map.World.Root.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.sprite!=null && names.Any(n=>s.sprite.name.StartsWith(n+"_"))), Is.False);
                LogAssert.NoUnexpectedReceived();
            }
        }
        [UnityTest, Category("FacilityAnimation")]
        public IEnumerator FacilityArtKeepsConstructionMoveAndReadableOverlays()
        {
            using(var input=new PointerScene())
            {
                yield return input.Load(); var view=input.View;
                var names=new[]{"Processing","Cultivation","Storage"};
                var keys=new[]{"processingId","cultivationId","storageId"};
                var created=new List<Thing>();
                for(var k=0;k<3;k++)
                {
                    Assert.That(view.BeginConstruction(GameDataStore.Instance.Catalog.Selected<FacilityData>(keys[k])), Is.True);
                    Assert.That(view.PreviewAt(new Vector2Int(-1,0)), Is.False);
                    Assert.That(view.ConfirmPlacement(), Is.False);
                    Assert.That(view.PreviewAt(new Vector2Int(2+k*5,4)), Is.True);
                    yield return null; yield return null;
                    Assert.That(input.Map.World.Root.Find("PlacementPreview/Body").GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo(names[k]+"_Idle_01"));
                    Assert.That(view.ConfirmPlacement(), Is.True);
                    created.Add(view.Simulation.Registry.Things.Single(t=>t.Definition.DefinitionId=="facility."+names[k].ToLowerInvariant()));
                }
                yield return null; yield return null;
                Assert.That(created.All(t=>!t.GetComponent<ConstructionComponent>().IsComplete), Is.True);
                Assert.That(created.All(t=>ArtSprite(input.Map,t,"Body").sprite.name.Contains("_Idle_")), Is.True);
                yield return CaptureFacilities("construction-sites"); yield return null;
                var worker=view.Simulation.Registry.Things.Single(t=>t.Definition.Kind==ThingKind.Creature);
                view.Simulation.Registry.CreateResource(view.Simulation.Organic,2,worker.GetComponent<ResourceContainerComponent>());
                yield return AdvanceUntil(view,()=>created.All(t=>t.GetComponent<ConstructionComponent>().IsComplete),120);
                yield return null; yield return null;
                var store=created[2]; Assert.That(view.SelectThing(store.Id), Is.True);
                Assert.That(view.BeginPlacement(), Is.True); Assert.That(view.PreviewAt(new Vector2Int(10,7)), Is.True);
                Assert.That(view.RotatePreview(), Is.True); Assert.That(view.ConfirmPlacement(), Is.True);
                Assert.That(store.Placement.Origin, Is.EqualTo(new Vector2Int(10,7)));
                Assert.That(store.Placement.Rotation, Is.EqualTo(QuarterTurn.Clockwise90));
                Assert.That(view.BeginPlacement(), Is.True); Assert.That(view.PreviewAt(new Vector2Int(11,1)), Is.True);
                Assert.That(view.CancelPlacement(), Is.True);
                Assert.That(store.Placement.Origin, Is.EqualTo(new Vector2Int(10,7)));
                Assert.That(view.SetUiPreferences(false,false), Is.True);
                foreach(var zoom in new[]{1f,1.5f,3f,8f})
                {
                    input.Map.Fit(); input.Map.ZoomBy(zoom); yield return null; yield return null;
                    Assert.That(input.Map.VisibleStatusCount, Is.Zero);
                    yield return CaptureFacilities("status-off-zoom-"+zoom.ToString(System.Globalization.CultureInfo.InvariantCulture)); yield return null;
                }
                view.OpenPanel(LaboratoryPanel.Game); yield return SelectLanguage(view,"en");
                view.Popups.Top.Cancel(); yield return PopupSettled(view);
                Assert.That(view.SetUiPreferences(true,true), Is.True);
                using(var portrait=new LaboratoryGameViewSize(720,1280))
                {
                    yield return SettleStatusLayoutSize(new Vector2Int(720,1280)); input.Map.Fit(); input.Map.ZoomBy(1.5f);
                    yield return null; yield return null; yield return CaptureFacilities("english-large-portrait"); yield return null;
                }
                LogAssert.NoUnexpectedReceived();
            }
        }
        private static IEnumerator CaptureFacilities(string name) => AppSceneFlowTests.Capture("facility-art",Path.GetFullPath(Path.Combine(Application.dataPath,"../../docs/plans/evidence/PLAN-029/screens/"+name+".png")));
    }
}
#endif
