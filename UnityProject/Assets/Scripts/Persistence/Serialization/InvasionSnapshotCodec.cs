using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using MobileGame.Foundation;
using MobileGame.Gameplay;

namespace MobileGame.Persistence
{
    internal static class InvasionSnapshotCodec
    {
        public static InvasionSnapshot Capture(LaboratoryManager laboratory, SessionPhase phase)
        {
            laboratory.Invasion.ValidatePhase(phase);
            var state = laboratory.Invasion.State;
            return new InvasionSnapshot
            {
                schemaVersion = 1, settings = state.Settings.Copy(), score = state.Score, activitySequence = state.ActivitySequence,
                elapsed = Time(state.Elapsed), goalReached = state.GoalReached, legacyEndVersion = state.LegacyEndVersion,
                receipts = laboratory.Receiving.Sources.OrderBy(s => s.Source).Select(s => new ReceiptSnapshot
                {
                    source = s.Source.ToString("N"), lastRound = s.LastRound, pendingRound = s.PendingRound,
                    resource = s.ResourceId, amount = s.Amount, delivered = s.Delivered
                }).ToArray(),
                invaders = laboratory.Registry.Things.Where(t => t.TryGetComponent<InvaderComponent>(out _))
                    .OrderBy(t => t.CreationOrder).Select(t => new InvaderSnapshot
                    {
                        thingId = t.Id.ToString("N"), isExternal = t.GetComponent<InvaderComponent>().IsExternal,
                        progress = Time(t.GetComponent<InvaderComponent>().Progress),
                        completedActivities = t.GetComponent<InvaderComponent>().CompletedActivities
                    }).ToArray(),
                reports = state.Reports.Select(r => new InvasionReportSnapshot
                {
                    sequence = r.Sequence, invaderId = r.InvaderId.ToString("N"), activity = r.Activity,
                    score = r.Score, isMajor = r.IsMajor, time = Time(r.Time), totalScore = r.TotalScore
                }).ToArray()
            };
        }

        public static void Restore(InvasionSnapshot saved, LaboratoryManager laboratory, int version, SessionPhase phase)
        {
            var state = laboratory.Invasion.State;
            if (version < 4)
            {
                // v2/v3 had no external activity. Preserve their actual phase, without
                // inventing achievements to justify an already ended diagnostic goal.
                state.LegacyEndVersion = phase == SessionPhase.Playing ? 0 : version;
                laboratory.Invasion.ValidatePhase(phase);
                return;
            }
            ValidateStructure(saved);
            var restoredIds = new HashSet<Guid>();
            foreach (var invader in saved.invaders)
            {
                var id = ParseId(invader.thingId);
                if (!restoredIds.Add(id) || !laboratory.Registry.TryGet(id, out var thing) ||
                    !thing.TryGetComponent<InvaderComponent>(out var component))
                    throw Invalid("Missing, duplicate or wrong-model invasion Thing.");
                component.IsExternal = invader.isExternal;
                component.Progress = ParseTime(invader.progress);
                component.CompletedActivities = invader.completedActivities;
            }
            if (laboratory.Registry.Things.Count(t => t.TryGetComponent<InvaderComponent>(out _)) != restoredIds.Count)
                throw Invalid("Missing invader activity state.");
            if (version >= 6) RestoreReceipts(saved.receipts, laboratory);
            state.Score = saved.score; state.ActivitySequence = saved.activitySequence;
            state.Elapsed = ParseTime(saved.elapsed); state.LegacyEndVersion = saved.legacyEndVersion;
            foreach (var report in saved.reports)
                state.AddReport(new InvasionReport { Sequence = report.sequence, InvaderId = ParseId(report.invaderId),
                    Activity = report.activity, Score = report.score, IsMajor = report.isMajor,
                    Time = ParseTime(report.time), TotalScore = report.totalScore });
            if (state.GoalReached != saved.goalReached) throw Invalid("Goal flag and score disagree.");
            laboratory.Invasion.ValidatePhase(phase);
        }

        private static void RestoreReceipts(ReceiptSnapshot[] saved, LaboratoryManager lab)
        {
            if (saved == null) throw Invalid("Missing receipt state.");
            var seen = new HashSet<Guid>();
            foreach (var row in saved)
            {
                if (row == null) throw Invalid("Null receipt.");
                var source = ParseId(row.source);
                if (!seen.Add(source) || !lab.Registry.TryGet(source, out var thing) ||
                    !thing.TryGetComponent<InvaderComponent>(out var outside) || !outside.IsExternal ||
                    row.lastRound <= 0 || row.lastRound > outside.CompletedActivities || row.pendingRound < 0 ||
                    row.pendingRound > row.lastRound || row.delivered < 0)
                    throw Invalid("Invalid receipt source or round.");
                var model = (CreatureData)thing.Definition;
                var last = lab.Invasion.Outcome(thing, row.lastRound);
                if (last.Amount <= 0 || model.SupplyEvery <= 0 ||
                    row.lastRound != outside.CompletedActivities / model.SupplyEvery * model.SupplyEvery ||
                    row.delivered % last.Amount != 0 || row.delivered / last.Amount >
                        row.lastRound / model.SupplyEvery - (row.pendingRound == 0 ? 0 : 1))
                    throw Invalid("Receipt history disagrees with model activity.");
                if (row.pendingRound > 0)
                {
                    var outcome = lab.Invasion.Outcome(thing, row.pendingRound);
                    if (outcome.Amount <= 0 || outcome.ResourceId != row.resource || outcome.Amount != row.amount)
                        throw Invalid("Pending reward disagrees with source activity.");
                }
                else if (row.amount != 0 || !string.IsNullOrEmpty(row.resource)) throw Invalid("Unexpected pending cargo.");
                lab.Receiving.Restore(new ReceiptState { Source = source, LastRound = row.lastRound,
                    PendingRound = row.pendingRound, ResourceId = row.resource, Amount = row.amount, Delivered = row.delivered });
            }
            foreach (var thing in lab.Registry.Things)
                if (thing.Definition is CreatureData model && model.SupplyEvery > 0 &&
                    thing.TryGetComponent<InvaderComponent>(out var outside) && outside.CompletedActivities >= model.SupplyEvery &&
                    !seen.Contains(thing.Id)) throw Invalid("Missing source watermark.");
        }

        public static void ValidateStructure(InvasionSnapshot saved)
        {
            if (saved == null || saved.schemaVersion != 1 || saved.settings == null || saved.invaders == null || saved.reports == null ||
                saved.invaders.Any(i => i == null) || saved.reports.Any(r => r == null))
                throw Invalid("Invasion settings, activity and report arrays are required.");
            try { saved.settings.Validate(); }
            catch (ArgumentException error) { throw new InvalidDataException(error.Message, error); }
            if (saved.reports.Length > saved.settings.ReportLimit) throw Invalid("Report history exceeds retention.");
        }
        private static Guid ParseId(string text) => Guid.TryParse(text, out var id) && id != Guid.Empty
            ? id : throw Invalid("Invalid invasion Thing ID.");
        // JsonUtility's double parser can change the final bits. Persist new clocks
        // as round-trip invariant strings so repeated reloads do not move boundaries.
        private static string Time(double value) => value.ToString("R", CultureInfo.InvariantCulture);
        private static double ParseTime(string text) => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            && value >= 0 && !double.IsInfinity(value) ? value : throw Invalid("Invalid invasion time.");
        private static InvalidDataException Invalid(string text) => new InvalidDataException(text);
    }
}
