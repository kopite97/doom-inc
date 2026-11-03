using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using UnityEngine;

namespace MobileGame.Foundation.Editor
{
    public static partial class Plan020Probe
    {
        private static SessionState EmptyFixture(GameDataCatalog d)
        {
            var r=new ThingRegistry(GameplayComponentFactory.Attach);var m=new GridMap(r,16,10);
            return new SessionState(r,m,new LaboratoryManager(r,m,d.Selected<ResourceData>("organicId"),d.Selected<ResourceData>("cultureId")));
        }
        private static Thing Worker(LaboratoryManager lab,GameDataCatalog d,bool support,int[] priorities)
        {
            var w=lab.Registry.Create(d.Selected<CreatureData>(support?"supportId":"prototypeId"));
            Require(lab.Map.Commands.Place(w.Id,new Vector2Int(1,1)).Succeeded,"place worker");Require(lab.SetWorkerPriorities(w.Id,priorities),"priorities");return w;
        }
        private static void Tick(LaboratoryManager lab,double seconds){for(double t=0;t<seconds;t+=.05)lab.Advance(Math.Min(.05,seconds-t));lab.ValidateState();}
        public static void Bottlenecks()
        {
            var d=GameDataAssets.LoadCatalog();var o=d.Selected<ResourceData>("organicId");var result=new List<object>();
            var codec=new SessionSnapshotCodec(d.Definitions.Values,true);
            // Material exists in storage but disabled transport prevents supply.
            {
                var state=EmptyFixture(d);var lab=(LaboratoryManager)state.Runtime;
                var p=Facility(lab,d.Selected<FacilityData>("processingId"),4,4);var s=Facility(lab,d.Selected<FacilityData>("storageId"),8,3);
                lab.Registry.CreateResource(o,10,s.GetComponent<ResourceContainerComponent>());var w=Worker(lab,d,false,new[]{0,0,0,0});
                Tick(lab,10);var before=new FacilityStatusQuery(lab).Read(p);
                var saved=codec.Serialize(codec.Capture(state,SessionPhase.Playing));
                state=codec.Restore(codec.Deserialize(saved));lab=(LaboratoryManager)state.Runtime;
                Require(lab.Registry.TryGet(w.Id,out w)&&lab.Registry.TryGet(p.Id,out p),"restore ids");
                Require(lab.SetWorkerPriorities(w.Id,new[]{0,0,1,0}),"enable haul");Tick(lab,20);var after=new FacilityStatusQuery(lab).Read(p);
                result.Add(new{scenario="material supply + save restore",before=before.Kind.ToString(),wait=before.Materials.WaitKind.ToString(),after=after.Kind.ToString(),progress=after.Progress,passed=before.Kind==FacilityStatusKind.MaterialWaiting&&after.Progress>0});
            }
            // The same outstanding output is blocked then admitted by the existing storage setting.
            {
                var state=EmptyFixture(d);var lab=(LaboratoryManager)state.Runtime;var g=Facility(lab,d.Selected<FacilityData>("gatheringId"),3,3);var s=Facility(lab,d.Selected<FacilityData>("storageId"),8,3);
                var w=Worker(lab,d,false,new[]{0,0,1,0});Set(g.GetComponent<GatheringComponent>(),"Outstanding",5);Set(g.GetComponent<GatheringComponent>(),"Progress",((FacilityData)g.Definition).GatheringSeconds);lab.Registry.CreateResource(o,5,g.GetComponent<ResourceContainerComponent>());
                Require(lab.SetAllowed(s.Id,o,false),"exclude");Tick(lab,20);var before=lab.TotalAt(s.Id);var outstanding=g.GetComponent<GatheringComponent>().Outstanding;
                var saved=codec.Serialize(codec.Capture(state,SessionPhase.Playing));File.WriteAllText(Path.Combine(Root,"storage-blocked-fixture.json"),saved);
                Require(lab.SetAllowed(s.Id,o,true),"allow");Tick(lab,20);var after=lab.TotalAt(s.Id);
                result.Add(new{scenario="storage exclusion",before,outstanding,after,remaining=g.GetComponent<GatheringComponent>().Outstanding,passed=before==0&&after==5});
            }
            // Replay identical saved output once at a distant store and once after normal placement commands.
            {
                var state=EmptyFixture(d);var lab=(LaboratoryManager)state.Runtime;var g=Facility(lab,d.Selected<FacilityData>("gatheringId"),3,3);var s=Facility(lab,d.Selected<FacilityData>("storageId"),12,7);
                Worker(lab,d,false,new[]{0,0,1,0});Set(g.GetComponent<GatheringComponent>(),"Outstanding",5);Set(g.GetComponent<GatheringComponent>(),"Progress",((FacilityData)g.Definition).GatheringSeconds);lab.Registry.CreateResource(o,5,g.GetComponent<ResourceContainerComponent>());
                var saved=codec.Serialize(codec.Capture(state,SessionPhase.Playing));var times=new List<double>();
                for(int pass=0;pass<2;pass++)
                {
                    var path=Path.Combine(Application.temporaryCachePath,"PLAN020-Distance-"+pass+".json");File.WriteAllText(path,saved);
                    var flow=new GameFlowManager(codec,new JsonFileSessionStore(path),()=>codec.Restore(codec.Deserialize(saved)));Require(flow.Reload(),"distance reload");lab=(LaboratoryManager)flow.Current.Runtime;
                    if(pass==1){Require(flow.BeginPlacement(s.Id),"move begin");Require(flow.PreviewPlacement(new Vector2Int(6,3),QuarterTurn.None),"move preview");Require(flow.ConfirmPlacement(),"move confirm");}
                    double seconds=0;while(lab.TotalAt(s.Id)<5&&seconds<60){flow.Advance(.05);seconds+=.05;}
                    lab.ValidateState();Require(lab.TotalAt(s.Id)==5,"distance delivery");times.Add(seconds);
                }
                result.Add(new{scenario="transport distance",farSeconds=times[0],nearSeconds=times[1],passed=times[1]<times[0]});
            }
            // Assignment changes actual working rate, then unassignment preserves consumed cycle.
            {
                var state=EmptyFixture(d);var lab=(LaboratoryManager)state.Runtime;var p=Facility(lab,d.Selected<FacilityData>("processingId"),4,4);
                var w=Worker(lab,d,true,new[]{0,0,0,1});lab.Registry.CreateResource(o,5,p.GetComponent<ResourceContainerComponent>());
                Tick(lab,1);var slow=p.GetComponent<ProductionComponent>().Progress;Require(lab.AssignWorker(w.Id,p.Id),"assign");
                double t=0;while(w.GetComponent<WorkerComponent>().Job?.Stage!=JobStage.Working&&t<30){Tick(lab,.05);t+=.05;}
                var start=p.GetComponent<ProductionComponent>().Progress;Tick(lab,1);var fast=p.GetComponent<ProductionComponent>().Progress-start;
                Require(lab.AssignWorker(w.Id,null),"unassign");start=p.GetComponent<ProductionComponent>().Progress;Tick(lab,1);var unassigned=p.GetComponent<ProductionComponent>().Progress-start;
                result.Add(new{scenario="production assignment",automatic=slow,staffed=fast,unassigned,passed=Math.Abs(slow-.2)<1e-6&&Math.Abs(fast-1)<1e-6&&Math.Abs(unassigned-.2)<1e-6});
            }
            Write("operating-bottlenecks",result);
            PairedComparisons(d, codec);
        }
        public static void CompareSavedWindows()
        {
            var data = GameDataAssets.LoadCatalog();
            PairedComparisons(data, new SessionSnapshotCodec(data.Definitions.Values, true));
        }

        private static void PairedComparisons(GameDataCatalog data, SessionSnapshotCodec codec)
        {
            var organic = data.Selected<ResourceData>("organicId");
            var rows = new List<object>();
            string Freeze(SessionState state) => codec.Serialize(codec.Capture(state, SessionPhase.Playing));
            LaboratoryManager Restore(string snapshot) => (LaboratoryManager)codec.Restore(codec.Deserialize(snapshot)).Runtime;
            // Repeat both branches from identical serialized state for the same simulated interval.
            var state = EmptyFixture(data);
            var lab = (LaboratoryManager)state.Runtime;
            var processor = Facility(lab, data.Selected<FacilityData>("processingId"), 4, 4);
            var store = Facility(lab, data.Selected<FacilityData>("storageId"), 8, 3);
            lab.Registry.CreateResource(organic, 10, store.GetComponent<ResourceContainerComponent>());
            var worker = Worker(lab, data, false, new[] { 0, 0, 0, 0 });
            var original = Freeze(state);
            for (var change = 0; change < 2; change++)
            {
                lab = Restore(original);
                if (change == 1) Require(lab.SetWorkerPriorities(worker.Id, new[] { 0, 0, 1, 0 }), "enable transport");
                Tick(lab, 20);
                lab.Registry.TryGet(processor.Id, out var target);
                var status = new FacilityStatusQuery(lab).Read(target);
                rows.Add(new { scenario = "material", change, seconds = 20, status = status.Kind.ToString(), status.Progress, reservations = lab.Reservations.Count() });
                Require(change == 0 ? status.Kind == FacilityStatusKind.MaterialWaiting : status.Progress > 0, "material paired result");
            }
            original = File.ReadAllText(Path.Combine(Root, "storage-blocked-fixture.json"));
            for (var change = 0; change < 2; change++)
            {
                lab = Restore(original);
                var destination = lab.Registry.Things.Single(t => t.Definition == data.Selected<FacilityData>("storageId"));
                if (change == 1) Require(lab.SetAllowed(destination.Id, organic, true), "allow stock");
                Tick(lab, 20);
                var stock = lab.TotalAt(destination.Id);
                rows.Add(new { scenario = "storage", change, seconds = 20, stock, reservations = lab.Reservations.Count() });
                Require(stock == (change == 0 ? 0 : 5), "storage paired result");
            }
            for (var change = 0; change < 2; change++)
            {
                var path = Path.Combine(Application.temporaryCachePath, "PLAN020-Distance-" + change + ".json");
                File.WriteAllText(path, original);
                var flow = new GameFlowManager(codec, new JsonFileSessionStore(path), () => codec.Restore(codec.Deserialize(original)));
                Require(flow.Reload(), "distance paired restore");
                lab = (LaboratoryManager)flow.Current.Runtime;
                var destination = lab.Registry.Things.Single(t => t.Definition == data.Selected<FacilityData>("storageId"));
                Require(lab.SetAllowed(destination.Id, organic, true), "distance accept");
                Require(flow.BeginPlacement(destination.Id) && flow.PreviewPlacement(change == 0 ? new Vector2Int(12, 7) : new Vector2Int(6, 3), QuarterTurn.None) && flow.ConfirmPlacement(), "paired distance placement");
                for (int i = 0; i < 40; i++) flow.Advance(.05);
                lab.ValidateState();
                var stock = lab.TotalAt(destination.Id);
                rows.Add(new { scenario = "distance", change, seconds = 2, stock, reservations = lab.Reservations.Count() });
                Require(change == 0 ? stock < 5 : stock == 5, "distance paired result");
            }
            state = EmptyFixture(data); lab = (LaboratoryManager)state.Runtime;
            processor = Facility(lab, data.Selected<FacilityData>("processingId"), 4, 3);
            store = Facility(lab, data.Selected<FacilityData>("storageId"), 8, 7);
            var source = Facility(lab, data.Selected<FacilityData>("gatheringId"), 3, 7);
            worker = Worker(lab, data, true, new[] { 0, 0, 2, 1 });
            lab.Registry.CreateResource(organic, 5, processor.GetComponent<ResourceContainerComponent>());
            Require(lab.AssignWorker(worker.Id, processor.Id), "prepare working assignment");
            for (int i = 0; i < 600 && worker.GetComponent<WorkerComponent>().Job?.Stage != JobStage.Working; i++) Tick(lab, .05);
            Require(worker.GetComponent<WorkerComponent>().Job?.Stage == JobStage.Working, "assignment arrival");
            Set(source.GetComponent<GatheringComponent>(), "Outstanding", 5);
            Set(source.GetComponent<GatheringComponent>(), "Progress", ((FacilityData)source.Definition).GatheringSeconds);
            lab.Registry.CreateResource(organic, 5, source.GetComponent<ResourceContainerComponent>());
            original = Freeze(state);
            for (var change = 0; change < 2; change++)
            {
                lab = Restore(original); lab.Registry.TryGet(processor.Id, out var target);
                var production = target.GetComponent<ProductionComponent>(); var start = production.Progress;
                if (change == 1) Require(lab.AssignWorker(worker.Id, null), "release production worker");
                Tick(lab, 4);
                var progress = production.Progress - start; var stock = lab.TotalAt(store.Id);
                rows.Add(new { scenario = "assignment vs transport", change, seconds = 4, progress, stock, reservations = lab.Reservations.Count() });
                Require(change == 0 ? Math.Abs(progress - 4) < 1e-6 && stock == 0 : Math.Abs(progress - .8) < 1e-6 && stock > 0, "assignment/transport paired result: " + progress + "," + stock);
            }
            Write("operating-paired-windows", rows);
        }


        public static void StartUiRecovery()=>View.StartCoroutine(UiRecovery());
        private static IEnumerator UiRecovery()
        {
            Load(File.ReadAllText(Path.Combine(Root,"storage-blocked-fixture.json")));View.SimulationRate=0;
            var s=View.Simulation.Registry.Things.Single(t=>t.Definition is FacilityData f&&f.Role==FacilityRole.Storage);
            var g=View.Simulation.Registry.Things.Single(t=>t.Definition is FacilityData f&&f.Role==FacilityRole.Gathering);
            Require(View.SelectThing(g.Id),"select blocked output");yield return null;
            var before=new FacilityStatusQuery(View.Simulation).Read(g);ScreenCapture.CaptureScreenshot(Path.Combine(Root,"storage-ui-blocked.png"));
            yield return new WaitForSecondsRealtime(.5f);Require(View.SelectThing(s.Id),"select store");yield return Button("Details");yield return Button("AllowOrganic");
            Require(s.GetComponent<StorageComponent>().AllowsOrganic,"UI allowance applied");
            yield return Until(()=>View.Simulation.TotalAt(s.Id)==5,30);View.Simulation.ValidateState();
            Write("storage-ui-recovery",new{before=before.Kind.ToString(),after=View.Simulation.TotalAt(s.Id),outstanding=g.GetComponent<GatheringComponent>().Outstanding,state=Snapshot()});
            ScreenCapture.CaptureScreenshot(Path.Combine(Root,"storage-ui-recovered.png"));Status="ui recovery complete";
        }
    }
}
