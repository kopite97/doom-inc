using System;
using System.IO;
using MobileGame.Foundation;
using MobileGame.Persistence;
using UnityEngine;

namespace MobileGame.GameFlow
{
    // UI and harness enter through this same coordinator. Systems keep their own state.
    public sealed class SessionController
    {
        private readonly SessionSnapshotCodec codec;
        private readonly ISessionStore store;
        private readonly Func<SessionState> initialFactory;
        private readonly double autoSaveSeconds;
        private double timeSinceSave;
        private bool applicationPaused;

        public SessionController(SessionSnapshotCodec codec, ISessionStore store,
            Func<SessionState> initialFactory, double autoSaveSeconds = 5)
        {
            this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.initialFactory = initialFactory ?? throw new ArgumentNullException(nameof(initialFactory));
            if (!IsFinite(autoSaveSeconds) || autoSaveSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(autoSaveSeconds));
            this.autoSaveSeconds = autoSaveSeconds;
        }

        public SessionState Current { get; private set; }
        public SessionPhase Phase { get; private set; } = SessionPhase.Preparing;
        public bool CanAdvanceLaboratory => CanAdvanceExternal && !Current.Map.Commands.IsEditing &&
            (Current.Runtime == null || !Current.Runtime.IsEditing);
        public bool CanAdvanceExternal => Current != null && Phase == SessionPhase.Playing && !applicationPaused;
        public string LastError { get; private set; }
        public string LastSaveError { get; private set; }
        public int SuccessfulSaves { get; private set; }
        public int Generation { get; private set; }
        public event Action<double> LaboratoryAdvanced;
        public event Action<double> ExternalAdvanced;
        public event Action SessionChanged;

        public bool StartOrContinue()
        {
            bool exists;
            try { exists = store.Exists; }
            catch (Exception error) when (IsOperationFailure(error)) { return Reject(error.Message); }
            return exists ? Reload() : Restart();
        }

        public bool Reload()
        {
            SessionState candidate;
            SessionPhase restoredPhase;
            try
            {
                if (!store.Exists)
                    return Reject("No saved session exists.");
                var snapshot = codec.Deserialize(store.Read());
                candidate = codec.Restore(snapshot);
                restoredPhase = snapshot.phase;
            }
            catch (Exception error) when (IsOperationFailure(error)) { return Reject(error.Message); }
            Publish(candidate, restoredPhase);
            return true;
        }

        public bool Restart()
        {
            SessionState candidate;
            try
            {
                candidate = initialFactory();
                if (candidate == null || (Current != null &&
                    (ReferenceEquals(candidate.Registry, Current.Registry) || ReferenceEquals(candidate.Map, Current.Map))))
                    return Reject("Initial state must be a new, independent session.");
                var snapshot = codec.Capture(candidate, SessionPhase.Playing);
                // Publish only after durable replacement succeeds; failed restart preserves the old session.
                WriteSnapshot(snapshot);
            }
            catch (Exception error) when (IsOperationFailure(error))
            {
                LastSaveError = error.Message;
                return Reject(error.Message);
            }
            // Observer bugs after commit must not be reported as failed IO or a rolled-back restart.
            Publish(candidate, SessionPhase.Playing);
            return true;
        }

        public bool Save()
        {
            if (Current == null || Phase == SessionPhase.Preparing)
                return Reject("The session is not ready to save.");
            try
            {
                WriteSnapshot(codec.Capture(Current, Phase));
                LastError = null;
                return true;
            }
            catch (Exception error) when (IsOperationFailure(error))
            {
                LastSaveError = error.Message;
                return Reject(error.Message);
            }
        }

        public bool ReachGoal()
        {
            if (Current == null || Phase != SessionPhase.Playing)
                return Reject("Only a playing session can reach its goal.");
            Phase = SessionPhase.Destruction;
            // Stop simulation even if the disk is unavailable. Keep save failure visible and retry later.
            Save();
            return true;
        }

        public bool FinishDestruction()
        {
            if (Current == null || Phase != SessionPhase.Destruction)
                return Reject("The destruction presentation is not active.");
            Phase = SessionPhase.Results;
            Save();
            return true;
        }

        public void Advance(double seconds)
        {
            if (!IsFinite(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (Current == null || applicationPaused || seconds == 0)
                return;
            var generation = Generation;
            if (CanAdvanceLaboratory)
            {
                Current.Runtime?.Advance(seconds);
                // Runtime callbacks can finish or replace a session. Do not notify an old
                // frame's observers or advance a newly published world with that frame.
                if (generation != Generation)
                    return;
                if (CanAdvanceLaboratory)
                    LaboratoryAdvanced?.Invoke(seconds);
            }
            if (generation != Generation)
                return;
            if (CanAdvanceExternal)
                ExternalAdvanced?.Invoke(seconds);
            if (generation != Generation)
                return;

            // Explicit elapsed input only. No saved wall clock and no offline catch-up.
            timeSinceSave += seconds;
            if (timeSinceSave >= autoSaveSeconds)
            {
                timeSinceSave = 0;
                Save();
            }
        }

        public void NotifyApplicationPause(bool paused)
        {
            applicationPaused = paused;
            if (paused && Current != null)
                Save();
        }

        public void NotifyApplicationQuit()
        {
            applicationPaused = true;
            if (Current != null)
                Save();
        }

        public bool BeginPlacement(Guid id)
        {
            if (!RequirePlaying()) return false;
            if (Current.Runtime != null && Current.Runtime.IsEditing)
                return Reject("Finish or cancel the current edit first.");
            return ApplyPlacementResult(Current.Map.Commands.BeginEdit(id));
        }

        public bool PreviewPlacement(Vector2Int origin, QuarterTurn rotation) => RequirePlaying() &&
            ApplyPlacementResult(Current.Map.Commands.Preview(origin, rotation));

        public bool ConfirmPlacement()
        {
            if (!RequirePlaying() || !ApplyPlacementResult(Current.Map.Commands.Confirm()))
                return false;
            Current.Runtime?.OnMapChanged();
            return true;
        }

        // Cancelling a transient edit is safe after a goal and must not change the phase.
        public bool CancelPlacement() => Current != null
            ? ApplyPlacementResult(Current.Map.Commands.Cancel()) : Reject("No session is ready.");

        private void Publish(SessionState state, SessionPhase phase)
        {
            Current = state;
            Phase = phase;
            Generation++;
            timeSinceSave = 0;
            LastError = null;
            LastSaveError = null;
            SessionChanged?.Invoke();
        }

        private void WriteSnapshot(SessionSnapshot snapshot)
        {
            store.Write(codec.Serialize(snapshot));
            SuccessfulSaves++;
            timeSinceSave = 0;
            LastSaveError = null;
        }

        private bool RequirePlaying() => Current != null && Phase == SessionPhase.Playing
            ? true : Reject("Placement is only available while playing.");

        private bool ApplyPlacementResult(PlacementResult result)
        {
            LastError = result.Succeeded ? null : result.ToString();
            return result.Succeeded;
        }

        private bool Reject(string error) { LastError = error; return false; }
        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static bool IsOperationFailure(Exception error) => error is IOException || error is InvalidDataException
            || error is UnauthorizedAccessException || error is ArgumentException
            || error is InvalidOperationException || error is FormatException || error is NotSupportedException;
    }
}
