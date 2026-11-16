#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Foundation.Editor;
using MobileGame.Gameplay;
using MobileGame.Presentation;
using MobileGame.TestSupport;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MobileGame.SpatialTests
{
    public sealed partial class LaboratoryViewTests
    {
        [UnityTest]
        public IEnumerator CultureArtProductionHaulingAndUi()
        {
            using var input = new PointerScene();
            yield return input.Load(); var view = input.View; var lab = view.Simulation;
            var worker = lab.Registry.Things.Single(t => t.Definition.Kind == ThingKind.Creature);
            foreach (var initial in worker.GetComponent<ResourceContainerComponent>().Contents.ToArray())
                lab.Registry.ConsumeResource(initial, initial.GetComponent<ResourceStackComponent>().Quantity);
            var processor = CreateStatusLayoutFacility(view,"processingId",new Vector2Int(3,3),QuarterTurn.None,true);
            var producer=lab.Registry.Create(GameDataStore.Instance.Catalog.Selected<CreatureData>("supportId"));
            Assert.That(lab.Map.Commands.Place(producer.Id,processor.Placement.InteractionCell.Value).Succeeded,Is.True);
            Assert.That(view.SetWorkerPriorities(worker.Id,new int[4]),Is.True);
            Assert.That(view.AssignWorker(producer.Id,processor.Id), Is.True);
            Assert.That(view.SetWorkerPriorities(producer.Id,new[]{0,0,0,1}), Is.True);
            lab.Registry.CreateResource(lab.Organic,5,processor.GetComponent<ResourceContainerComponent>());
            lab.OnMapChanged();
            yield return AdvanceArtUntil(view,()=>processor.GetComponent<ProductionComponent>().Outstanding>0,40);
            var medium = GameDataStore.Instance.Catalog.Selected<ResourceData>("cultureId");
            Assert.That(view.SetWorkerPriorities(producer.Id,new int[4]),Is.True);
            var store = CreateStatusLayoutFacility(view,"storageId",new Vector2Int(8,3),QuarterTurn.None,true);
            Assert.That(view.SetWorkerPriorities(worker.Id,new[]{0,0,1,0}), Is.True);
            yield return AdvanceArtUntil(view,()=>worker.GetComponent<WorkerComponent>().Job?.Kind==WorkKind.Hauling
                && worker.GetComponent<WorkerComponent>().Job.Stage==JobStage.ToDestination,40);
            var cargoId=worker.GetComponent<WorkerComponent>().Job.ResourceId.Value;
            Assert.That(lab.Registry.TryGet(cargoId,out var cargo), Is.True);
            Assert.That(cargo.Definition,Is.SameAs(medium));
            Assert.That(cargo.GetComponent<ResourceStackComponent>().Quantity,Is.EqualTo(((FacilityData)processor.Definition).OutputAmount));
            yield return null; yield return null;
            Assert.That(ArtSprite(input.Map,worker,"Cargo").sprite,Is.SameAs(medium.Assets.Icon));
            Assert.That(ArtSprite(input.Map,worker,"Cargo").gameObject.activeSelf,Is.True);
            Assert.That(view.Save(),Is.True); Assert.That(view.Reload(),Is.True); yield return null; yield return null;
            lab=view.Simulation; Assert.That(lab.Registry.TryGet(worker.Id,out worker),Is.True);
            Assert.That(ArtSprite(input.Map,worker,"Cargo").sprite,Is.SameAs(medium.Assets.Icon));
            view.AdvanceFrame(.5,0); yield return null; yield return null;
            foreach(var language in new[]{"ko","en"})
            {
                view.OpenPanel(LaboratoryPanel.Game); yield return SelectLanguage(view,language);
                view.Popups.Top.Cancel(); yield return PopupSettled(view);
                foreach(var large in new[]{false,true})
                {
                    view.SetUiPreferences(true,large);
                    foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(720,1280)})
                    using(new LaboratoryGameViewSize(size.x,size.y))
                    {
                        yield return SettleScreenStateSize(size);
                        Assert.That(view.SelectThing(worker.Id),Is.True); view.ExpandSelection();
                        input.Map.Fit(); input.Map.ZoomBy(1.3f); yield return null; yield return null; Canvas.ForceUpdateCanvases();
                        var icon=view.UiCanvas.GetComponentsInChildren<Image>().Single(i=>i.name=="CultureMediumQuantityIcon");
                        Assert.That(icon.sprite,Is.SameAs(medium.Assets.Icon)); Assert.That(icon.raycastTarget,Is.False);
                        var row=view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Stored_"+medium.DefinitionId);
                        Assert.That(row.transform.Find("DefinitionIcon").GetComponent<Image>().sprite,Is.SameAs(icon.sprite));
                        foreach(var label in view.UiCanvas.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.enabled&&!string.IsNullOrEmpty(t.text)))
                        {
                            if(label.transform.IsChildOf(view.MapViewport))continue;
                            Assert.That(label.preferredHeight,Is.LessThanOrEqualTo(label.rectTransform.rect.height+2),label.name+": "+label.text);
                        }
                        var path=Path.Combine(RegressionEvidenceRoot,"culture-r001",language+"-"+(large?"large":"normal")+"-"+(size.x>size.y?"landscape":"portrait")+".png");
                        yield return AppSceneFlowTests.Capture("culture-r001",path);
                    }
                }
            }
            yield return AdvanceArtUntil(view,()=>!lab.Registry.TryGet(cargoId,out var remaining)||remaining.HolderId!=worker.Id,40);
            yield return null;yield return null;
            Assert.That(ArtSprite(input.Map,worker,"Cargo").gameObject.activeSelf,Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator CultureArtPartialAdmissionKeepsRemainderUntilEmptyHands()
        {
            using var input=new PointerScene();yield return input.Load();var view=input.View;var lab=view.Simulation;
            var worker=lab.Registry.Things.Single(t=>t.Definition.Kind==ThingKind.Creature);
            foreach(var initial in worker.GetComponent<ResourceContainerComponent>().Contents.ToArray())
                lab.Registry.ConsumeResource(initial,initial.GetComponent<ResourceStackComponent>().Quantity);
            var source=CreateStatusLayoutFacility(view,"storageId",new Vector2Int(3,3),QuarterTurn.None,true);
            var destination=CreateStatusLayoutFacility(view,"storageId",new Vector2Int(9,3),QuarterTurn.None,true);
            var medium=GameDataStore.Instance.Catalog.Selected<ResourceData>("cultureId");
            lab.Registry.CreateResource(medium,5,source.GetComponent<ResourceContainerComponent>());
            Assert.That(view.SetAllowed(source.Id,medium,false),Is.True);
            Assert.That(view.SetWorkerPriorities(worker.Id,new[]{0,0,1,0}),Is.True);
            lab.OnMapChanged();
            yield return AdvanceArtUntil(view,()=>worker.GetComponent<WorkerComponent>().Job?.Stage==JobStage.ToDestination,40);
            var cargoId=worker.GetComponent<WorkerComponent>().Job.ResourceId.Value;
            var fill=lab.Registry.CreateResource(lab.Organic,((FacilityData)destination.Definition).StorageCapacity-2,destination.GetComponent<ResourceContainerComponent>());
            lab.OnMapChanged();
            yield return AdvanceArtUntil(view,()=>worker.GetComponent<WorkerComponent>().Job?.Stage==JobStage.Waiting
                && lab.Registry.TryGet(cargoId,out var remaining)&&remaining.GetComponent<ResourceStackComponent>().Quantity==3,40);
            yield return null;yield return null;
            Assert.That(ArtSprite(input.Map,worker,"Cargo").gameObject.activeSelf,Is.True);
            Assert.That(ArtSprite(input.Map,worker,"Cargo").sprite,Is.SameAs(medium.Assets.Icon));
            lab.Registry.ConsumeResource(fill,3);lab.OnMapChanged();
            yield return AdvanceArtUntil(view,()=>!lab.Registry.TryGet(cargoId,out var remaining)||remaining.HolderId!=worker.Id,40);
            yield return null;yield return null;
            Assert.That(ArtSprite(input.Map,worker,"Cargo").gameObject.activeSelf,Is.False);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
