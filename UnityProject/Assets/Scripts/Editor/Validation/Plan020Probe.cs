using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using MobileGame.GameFlow;
using MobileGame.Gameplay;
using MobileGame.Persistence;
using MobileGame.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using Unity.Profiling;

namespace MobileGame.Foundation.Editor
{
    // Explicitly invoked Editor-only diagnostic. Never changes product assets or rules.
    public static partial class Plan020Probe
    {
        public static string Root => Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/plans/evidence/PLAN-020"));
        public static string Status { get; private set; } = "idle";
        private static LaboratoryGameViewSize size;
        private static readonly List<object> events = new List<object>();
        private static LaboratoryView View => UnityEngine.Object.FindFirstObjectByType<LaboratoryView>();
        public static void Write(string name, object value) { Directory.CreateDirectory(Root); File.WriteAllText(Path.Combine(Root, name + ".json"), (string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json", true).GetMethod("SerializeObject", new[]{typeof(object)}).Invoke(null,new[]{value})); }
        public static object Snapshot() => new {
            utc=DateTime.UtcNow, frame=Time.frameCount, time=Time.realtimeSinceStartupAsDouble,
            scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
            phase=View?.Controller?.Phase.ToString(), paused=View?.Controller?.IsApplicationPaused,
            gameTime=View?.Invasion?.Elapsed, score=View?.Invasion?.Score, reports=View?.Invasion?.Reports.Count,
            things=View?.Simulation?.Registry.Things.Count(), facilities=View?.Simulation?.Registry.Things.Count(t=>t.Definition.Kind==ThingKind.Facility),
            creatures=View?.Simulation?.Registry.Things.Count(t=>t.Definition.Kind==ThingKind.Creature),
            external=View?.Simulation?.Invasion.ExternalCount, saves=View?.Controller?.SuccessfulSaves,
            flow=UnityEngine.Object.FindObjectsByType<SceneFlowManager>(FindObjectsSortMode.None).Length,
            data=UnityEngine.Object.FindObjectsByType<GameDataStore>(FindObjectsSortMode.None).Length,
            eventSystems=UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length,
            hosts=UnityEngine.Object.FindObjectsByType<PopupHost>(FindObjectsSortMode.None).Length,
            popupViews=UnityEngine.Object.FindObjectsByType<PopupBase>(FindObjectsSortMode.None).Length,
            popupCount=SceneFlowManager.Instance?.Popups?.Count, canInteract=View?.CanInteract,
            objects=Resources.FindObjectsOfTypeAll<GameObject>().Length,
            managed=GC.GetTotalMemory(false), unityUsed=Profiler.GetTotalAllocatedMemoryLong(), unityReserved=Profiler.GetTotalReservedMemoryLong(),
            width=Screen.width,height=Screen.height, vSync=QualitySettings.vSyncCount, fps=Application.targetFrameRate,
            error=SceneFlowManager.Instance?.Error, saveError=View?.Controller?.LastSaveError
        };
        public static void Resize(int w, int h) { size?.Dispose(); size = new LaboratoryGameViewSize(w,h); }
        public static void RestoreSize() { size?.Dispose(); size=null; }
        private static void Focus()
        { SceneFlowManager.Instance?.SendMessage("OnApplicationFocus",true); View?.SendMessage("OnApplicationFocus",true); }
        private static void Require(bool value, string detail) { if(!value) throw new InvalidOperationException(detail); }
        private static void Set(object target,string key,object value) => target.GetType().GetProperty(key).GetSetMethod(true).Invoke(target,new[]{value});
        private static Thing Facility(LaboratoryManager lab,FacilityData def,int x,int y)
        {
            var t=lab.Registry.Create(def); Require(lab.Map.Commands.Place(t.Id,new Vector2Int(x,y)).Succeeded,"fixture placement");
            var c=t.GetComponent<ConstructionComponent>(); Set(c,"Delivered",def.BuildCost); Set(c,"Progress",(double)def.BuildSeconds); Set(c,"IsComplete",true); return t;
        }
        // Preparation is intentionally separate from paid-run/UI validation.
        public static string Prepare(int level)
        {
            var d=GameDataStore.Instance.Catalog;
            var o=d.Selected<ResourceData>("organicId"); var c=d.Selected<ResourceData>("cultureId");
            var inv=d.Settings.Invasion;
            var settings=new InvasionSettings(inv.ActivitySeconds,inv.SmallScore,inv.MajorScore,inv.MajorEvery,100000000,inv.DetailedUntilScore,inv.ReportLimit);
            var state=LaboratorySessionFactory.Create(o,d.Selected<FacilityData>("gatheringId"),d.Selected<FacilityData>("storageId"),d.Selected<CreatureData>("prototypeId"),c,settings);
            var lab=(LaboratoryManager)state.Runtime;
            if(level>0)
            {
                foreach(var position in new[]{new Vector2Int(3,3),new Vector2Int(3,7)}) Facility(lab,d.Selected<FacilityData>("gatheringId"),position.x,position.y);
                var s=Facility(lab,d.Selected<FacilityData>("storageId"),8,3); lab.Registry.CreateResource(o,10,s.GetComponent<ResourceContainerComponent>());
                Facility(lab,d.Selected<FacilityData>("processingId"),8,7);
                var cult=Facility(lab,d.Selected<FacilityData>("cultivationId"),12,7); lab.SelectCultivation(cult.Id,CultivationTarget.Support);
                var s2=Facility(lab,d.Selected<FacilityData>("storageId"),12,3); lab.Registry.CreateResource(c,10,s2.GetComponent<ResourceContainerComponent>());
                for(int i=0;i<(level==1?3:23);i++){var t=lab.Registry.Create(d.Selected<CreatureData>("supportId"));Require(lab.Map.Commands.Place(t.Id,new Vector2Int(1,1)).Succeeded,"worker fixture");}
            }
            if(level>1)
            {
                Facility(lab,d.Selected<FacilityData>("processingId"),0,7);
                Facility(lab,d.Selected<FacilityData>("gatheringId"),0,3);
                for(int i=0;i<16;i++){var t=lab.Registry.Create(d.Selected<CreatureData>("invaderId"));Require(lab.Map.Commands.Place(t.Id,new Vector2Int(1,1)).Succeeded,"invader fixture");}
                Require(lab.Invasion.ReleaseAll(),"release fixture");
            }
            lab.ValidateState();
            var codec=new SessionSnapshotCodec(d.Definitions.Values,true);
            return codec.Serialize(codec.Capture(state,SessionPhase.Playing));
        }
        private static void RequireDiagnosticSlot()
        {
            var expected=Path.GetFullPath(Path.Combine(Application.temporaryCachePath,"EditorSessions","SlotB","session.json"));
            Require(EditorApplication.isPlaying && View!=null && string.Equals(Path.GetFullPath(View.SavePath),expected,StringComparison.OrdinalIgnoreCase),
                "Use the alternate Editor test slot and back up its files before running diagnostics.");
        }
        private static void Load(string snapshot)
        {
            RequireDiagnosticSlot(); var v=View; v.SimulationRate=0; File.WriteAllText(v.SavePath,snapshot); Require(v.Reload(),"fixture reload"); Focus(); v.SimulationRate=1;
        }
        private struct Sample { public int frame; public double elapsed,mainMs,gcBytes,deltaMs,gameTime; }
        private static readonly List<Sample> samples=new List<Sample>(100000);
        private static ProfilerRecorder main,gc;
        private static double begin,warm,duration,startedGame;
        private static int previousFrame;
        private static string sampleName;
        private static bool measuring;
        private static object startSnapshot;
        public static void Measure(string name,double warmSeconds=30,double sampleSeconds=60)
        {
            Require(!measuring,"sample already running"); Focus(); samples.Clear();
            main=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);
            gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
            sampleName=name;begin=Time.realtimeSinceStartupAsDouble;warm=warmSeconds;duration=sampleSeconds;
            startedGame=View.Invasion.Elapsed;previousFrame=-1;startSnapshot=Snapshot();measuring=true;
            EditorApplication.update+=Collect; Status="measuring "+name;
        }
        private static void Collect()
        {
            if(!EditorApplication.isPlaying || View==null){FinishSample("interrupted");return;}
            Focus(); var now=Time.realtimeSinceStartupAsDouble; var elapsed=now-begin;
            if(Time.frameCount!=previousFrame && elapsed>=warm && elapsed<warm+duration)
            { previousFrame=Time.frameCount;samples.Add(new Sample{frame=Time.frameCount,elapsed=elapsed,mainMs=main.Valid?main.LastValue/1000000d:-1,gcBytes=gc.Valid?gc.LastValue:-1,deltaMs=Time.unscaledDeltaTime*1000d,gameTime=View.Invasion.Elapsed}); }
            if(elapsed>=warm+duration) FinishSample("complete");
        }
        private static void FinishSample(string result)
        {
            EditorApplication.update-=Collect;measuring=false;
            var mv=main.Valid;var gv=gc.Valid;main.Dispose();gc.Dispose();
            var end=Snapshot(); var rows=samples.ToArray();
            double P(Func<Sample,double> f,double p){var a=rows.Select(f).OrderBy(x=>x).ToArray();return a.Length==0?0:a[Math.Min(a.Length-1,(int)(a.Length*p))];}
            Write(sampleName,new{result,start=startSnapshot,end,sampleCount=rows.Length,warmupSeconds=warm,sampleSeconds=duration,mainValid=mv,gcValid=gv,
                mainMedianMs=P(s=>s.mainMs,.5),mainP95Ms=P(s=>s.mainMs,.95),mainMaxMs=P(s=>s.mainMs,1),gcMedianBytes=P(s=>s.gcBytes,.5),gcP95Bytes=P(s=>s.gcBytes,.95),
                gcMeanBytes=rows.Length==0?0:rows.Average(s=>s.gcBytes),gameAdvanced=View==null?0:View.Invasion.Elapsed-startedGame,
                note="EditorApplication.update samples distinct rendered frames; recorder Main Thread includes waits and Editor work. Not Android CPU/FPS evidence."});
            using(var w=new StreamWriter(Path.Combine(Root,sampleName+".csv"))){w.WriteLine("frame,elapsed,mainMs,gcBytes,deltaMs,gameTime");foreach(var s in rows)w.WriteLine(string.Join(",",s.frame,s.elapsed.ToString("R",CultureInfo.InvariantCulture),s.mainMs.ToString("R",CultureInfo.InvariantCulture),s.gcBytes,s.deltaMs.ToString("R",CultureInfo.InvariantCulture),s.gameTime.ToString("R",CultureInfo.InvariantCulture)));}
            Status="finished "+sampleName;
        }
        public static void StartMeasurements() { RequireDiagnosticSlot(); View.StartCoroutine(MeasurementRoutine()); }
        private static IEnumerator MeasurementRoutine()
        {
            Resize(1280,720);
            for(int level=0;level<3;level++)
            {
                var fixture=Prepare(level);File.WriteAllText(Path.Combine(Root,"fixture-"+level+".json"),fixture);
                for(int run=1;run<=3;run++)
                {
                    Load(fixture);View.SetUiPreferences(true,false);yield return new WaitForSecondsRealtime(1);
                    Measure("performance-level"+level+"-run"+run);while(measuring)yield return null;
                }
            }
            Status="measurements complete";Write("measurements-complete",Snapshot());
        }
        public static void StartCycles() { RequireDiagnosticSlot(); View.StartCoroutine(Cycles()); }
        public static void StartUiMeasurements() { RequireDiagnosticSlot(); View.StartCoroutine(UiMeasurements()); }
        private static IEnumerator UiMeasurements()
        {
            var fixture=Prepare(2);
            foreach(var mode in new[]{"status-off","reports-expanded","portrait-large-en"})
            {
                Load(fixture); Resize(mode=="portrait-large-en"?720:1280,mode=="portrait-large-en"?1280:720);
                View.SetUiPreferences(mode!="status-off",mode=="portrait-large-en");
                var task=SceneFlowManager.Instance.Localization.SelectLanguageAsync(mode=="portrait-large-en"?"en":"ko");
                while(!task.IsCompleted){Focus();yield return null;}
                if(task.IsFaulted)throw task.Exception;
                if(mode!="status-off"){View.OpenPanel(LaboratoryPanel.Invasion);View.ToggleReports();}
                yield return Button("FitMap");
                Measure("ui-"+mode);while(measuring)yield return null;
                ScreenCapture.CaptureScreenshot(Path.Combine(Root,"ui-"+mode+".png"));yield return new WaitForSecondsRealtime(1);
            }
            Status="ui measurements complete";
        }
        public static void StartSoak() { RequireDiagnosticSlot(); SceneFlowManager.Instance.StartCoroutine(Soak()); }
        private static IEnumerator Soak()
        {
            var fixture=Prepare(2);Load(fixture);Resize(1280,720);events.Clear();
            var start=Time.realtimeSinceStartupAsDouble;var last=start;double active=0;int minute=0;var next=start+60;
            Status="soak running";events.Add(new{stage="start",active,state=Snapshot()});
            while(Time.realtimeSinceStartupAsDouble-start<1200)
            {
                Focus();var now=Time.realtimeSinceStartupAsDouble;
                if(View?.Controller?.CanAdvanceLaboratory==true)active+=now-last;
                last=now;
                if(now>=next)
                {
                    minute++;next=start+(minute+1)*60;events.Add(new{minute,active,state=Snapshot()});Write("soak-progress",events);
                    if(View!=null)
                    {
                        if(minute==5||minute==10)
                        {
                            var flow=SceneFlowManager.Instance;Require(flow.RequestReturnToTitle(),"soak return prompt");
                            yield return new WaitForSecondsRealtime(.5f);Focus();Require(flow.Popups.Choose(flow.Popups.Top,PopupResult.Confirmed),"soak confirm");
                            var deadline=Time.realtimeSinceStartupAsDouble+30;
                            while((flow.IsTransitioning||flow.Laboratory!=null)&&Time.realtimeSinceStartupAsDouble<deadline){Focus();yield return null;}
                            Require(!flow.IsTransitioning&&flow.Laboratory==null,"soak title timeout");events.Add(new{stage="title",minute,state=Snapshot()});
                            yield return new WaitForSecondsRealtime(3);Require(flow.Continue(),"soak continue");
                            while(flow.IsTransitioning&&Time.realtimeSinceStartupAsDouble<deadline){Focus();yield return null;}
                            Require(View!=null&&!flow.IsTransitioning,"soak continue timeout");events.Add(new{stage="continued",minute,state=Snapshot()});
                        }
                        else if(minute%3==0)
                        { View.ShowSettings();yield return new WaitForSecondsRealtime(.5f);Focus();View.Popups.Back();yield return new WaitForSecondsRealtime(.5f); }
                        else
                        {
                            View.SetUiPreferences(minute%2==0,minute%4==0);var task=SceneFlowManager.Instance.Localization.SelectLanguageAsync(minute%2==0?"ko":"en");
                            while(!task.IsCompleted){Focus();yield return null;}
                            View.OpenPanel(LaboratoryPanel.Invasion);if(!View.ReportsExpanded)View.ToggleReports();
                        }
                    }
                    last=Time.realtimeSinceStartupAsDouble;
                }
                yield return null;
            }
            events.Add(new{stage="complete",active,total=Time.realtimeSinceStartupAsDouble-start,state=Snapshot()});Write("soak-final",events);
            Status="soak complete";
        }
        private static IEnumerator Until(Func<bool> done,int maximum)
        { for(int i=0;i<maximum*2&&!done();i++){Focus();View.Controller.Advance(.5);if(i%20==0)yield return null;}Require(done(),"paid cycle timeout"); }
        private static IEnumerator Button(string key)
        {
            Focus();yield return new WaitForSecondsRealtime(.2f);for(int i=0;i<4;i++){Focus();yield return null;}
            var b=View.FindUiButton(key);Require(b!=null&&b.interactable,"button unavailable "+key);
            b.OnSubmit(new BaseEventData(EventSystem.current));yield return null;
        }
        private static IEnumerator Build(string key,int x,int y)
        {
            Focus();yield return new WaitForSecondsRealtime(.2f);View.OpenPanel(LaboratoryPanel.Build);yield return Button(key);Require(View.IsEditing,"build UI not applied");
            Require(View.PreviewAt(new Vector2Int(x,y)),"preview");yield return Button("Confirm");Require(!View.IsEditing,"confirm UI not applied");
        }
        private static IEnumerator Cycles()
        {
            events.Clear(); Resize(1280,720);View.SimulationRate=0;
            var d=GameDataStore.Instance.Catalog;var organic=d.Selected<ResourceData>("organicId");var culture=d.Selected<ResourceData>("cultureId");
            var store=d.Selected<FacilityData>("storageId");var processor=d.Selected<FacilityData>("processingId");var cultivator=d.Selected<FacilityData>("cultivationId");
            for(int run=1;run<=3;run++)
            {
                Status="paid cycle "+run;Require(View.Controller.Phase==SessionPhase.Playing,"start phase");
                events.Add(new{stage="start",run,state=Snapshot()});
                yield return Build("BuildGathering",3,3);yield return Build("BuildStorage",8,3);
                yield return Until(()=>View.Simulation.Registry.Things.Any(t=>t.Definition==store&&View.Simulation.TotalAt(t.Id)>=5),150);
                yield return Build("BuildProcessing",3,7);yield return Build("BuildCultivation",8,7);yield return Build("BuildStorage",12,3);
                var stores=View.Simulation.Registry.Things.Where(t=>t.Definition==store).OrderBy(t=>t.CreationOrder).ToArray();
                Require(View.SetAllowed(stores[0].Id,culture,false),"organic store");Require(View.SetAllowed(stores[1].Id,organic,false),"culture store");
                yield return Until(()=>View.Simulation.Registry.Things.Where(t=>t.Definition.Kind==ThingKind.Facility).All(t=>t.GetComponent<ConstructionComponent>().IsComplete),200);
                var p=View.Simulation.Registry.Things.Single(t=>t.Definition==processor);var c=View.Simulation.Registry.Things.Single(t=>t.Definition==cultivator);
                Require(View.SelectCultivation(c.Id,CultivationTarget.Support),"support target");
                yield return Until(()=>View.Simulation.Registry.Things.Any(t=>t.Definition==d.Selected<CreatureData>("supportId")),900);
                var worker=View.Simulation.Registry.Things.First(t=>t.Definition==d.Selected<CreatureData>("supportId"));
                Require(View.AssignWorker(worker.Id,p.Id),"assignment");Require(View.SetWorkerPriorities(worker.Id,new[]{2,3,4,1}),"priority");
                Require(View.SelectCultivation(c.Id,CultivationTarget.Invader),"invader target");
                yield return Until(()=>View.Simulation.Invasion.WaitingCount>0,900);
                View.Simulation.ValidateState();events.Add(new{stage="paid production",run,state=Snapshot()});
                View.OpenPanel(LaboratoryPanel.Invasion);yield return Button("Release");Require(View.Simulation.Invasion.ExternalCount>0,"release UI");
                yield return Until(()=>View.Controller.Phase==SessionPhase.Destruction,300);
                var deadline=Time.realtimeSinceStartupAsDouble+15;while(View.Controller.Phase!=SessionPhase.Results&&Time.realtimeSinceStartupAsDouble<deadline){Focus();yield return null;}
                Require(View.Controller.Phase==SessionPhase.Results,"results timeout");
                events.Add(new{stage="results",run,state=Snapshot()});Write("paid-cycles",events);
                yield return Button("RestartFromResults");Require(View.Controller.Phase==SessionPhase.Playing,"restart UI");
                Require(View.Simulation.Registry.Things.Count(t=>t.Definition.Kind==ThingKind.Facility)==0,"stale facilities");
                Require(View.Invasion.Score==0&&View.Invasion.Reports.Count==0,"stale reports");
            }
            events.Add(new{stage="three runs complete",state=Snapshot()});Write("paid-cycles",events);Status="cycles complete";View.SimulationRate=1;
        }
    }
}
