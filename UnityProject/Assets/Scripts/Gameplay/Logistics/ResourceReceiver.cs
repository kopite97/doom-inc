using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Foundation;

namespace MobileGame.Gameplay
{
    public enum ReceiptResult { Admitted, Queued, SkippedWhilePending, AlreadyProcessed }

    // One ordered request stream per source. Retain the watermark even after delivery.
    public sealed class ReceiptState
    {
        public Guid Source { get; internal set; }
        public long LastRound { get; internal set; }
        public long PendingRound { get; internal set; }
        public string ResourceId { get; internal set; }
        public int Amount { get; internal set; }
        public long Delivered { get; internal set; }
    }

    public sealed class ResourceReceiver
    {
        private readonly LaboratoryManager laboratory;
        private long lastCheckedVersion = -1;
        private readonly Dictionary<Guid, ReceiptState> sources = new Dictionary<Guid, ReceiptState>();
        public IEnumerable<ReceiptState> Sources => sources.Values;
        public int PendingCount { get; private set; }
        internal ResourceReceiver(LaboratoryManager laboratory) { this.laboratory = laboratory; }

        internal void Restore(ReceiptState state)
        {
            sources.Add(state.Source, state);
            if (state.PendingRound > 0) PendingCount++;
        }

        public ReceiptResult Receive(Guid source, long round, string resource, int amount)
        {
            if (source == Guid.Empty || round <= 0 || amount <= 0 || ResolveResource(resource) == null)
                throw new ArgumentException("Invalid supply request.");
            if (!sources.TryGetValue(source, out var state))
            {
                state = new ReceiptState { Source = source };
                sources.Add(source, state);
            }
            if (round <= state.LastRound) return ReceiptResult.AlreadyProcessed;
            Retry();
            state.LastRound = round;
            if (state.PendingRound != 0) return ReceiptResult.SkippedWhilePending;
            state.PendingRound = round;
            state.Amount = amount;
            state.ResourceId = resource;
            PendingCount++;
            lastCheckedVersion = -1;
            return TryAdmit(state) ? ReceiptResult.Admitted : ReceiptResult.Queued;
        }

        internal void Retry()
        {
            if (PendingCount == 0 || laboratory.IsEditing || laboratory.Map.Commands.IsEditing ||
                lastCheckedVersion == laboratory.WorkVersion) return;
            foreach (var state in Sources.Where(s => s.PendingRound != 0).OrderBy(s => s.Source))
                TryAdmit(state);
            lastCheckedVersion = laboratory.WorkVersion;
        }

        private ResourceData ResolveResource(string id) => id == laboratory.Organic.DefinitionId
            ? laboratory.Organic : id == laboratory.Culture?.DefinitionId ? laboratory.Culture : null;

        private bool TryAdmit(ReceiptState state)
        {
            if (laboratory.IsEditing || laboratory.Map.Commands.IsEditing ||
                !laboratory.Logistics.TryReceive(ResolveResource(state.ResourceId), state.Amount)) return false;
            state.Delivered = checked(state.Delivered + state.Amount);
            state.Amount = 0;
            state.PendingRound = 0;
            state.ResourceId = null;
            PendingCount--;
            return true;
        }
    }
}
