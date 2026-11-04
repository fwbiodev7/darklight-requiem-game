// DARKLIGHT: REQUIEM — Cutscene Manager
// Executes cutscene sequences using the existing DarklightCampaign architecture.
// Does NOT create Timelines, animations, or visual sequences.

using System;
using System.Collections.Generic;

namespace Darklight.Cutscenes
{
    /// <summary>
    /// States of the cutscene execution lifecycle.
    /// </summary>
    public enum CutsceneState
    {
        Idle,
        Starting,
        Playing,
        Paused,
        Skipping,
        Ending,
        Error
    }

    /// <summary>
    /// Manages cutscene lifecycle: start, tick, pause, skip, end, and error recovery.
    /// Designed to integrate with DarklightCampaign without modifying it.
    /// </summary>
    public class CutsceneManager
    {
        public CutsceneState State { get; private set; } = CutsceneState.Idle;
        public ICutsceneSequence Current { get; private set; }
        public float ElapsedTime { get; private set; }
        public string LastError { get; private set; }

        readonly CutsceneContext context;
        readonly CutsceneEventBridge eventBridge;
        readonly DarklightInputGate inputGate;
        readonly HashSet<string> completedSequences = new();
        readonly HashSet<string> activeSequenceIds = new();

        bool skipRequested;

        public CutsceneManager()
        {
            eventBridge = new CutsceneEventBridge();
            inputGate = new DarklightInputGate();
            context = new CutsceneContext
            {
                EventBridge = eventBridge,
                InputGate = inputGate
            };
        }

        /// <summary>
        /// Start a cutscene sequence. Returns false if already playing or validation fails.
        /// </summary>
        public bool Play(ICutsceneSequence sequence)
        {
            if (sequence == null) return false;
            if (State != CutsceneState.Idle && State != CutsceneState.Error) return false;
            if (activeSequenceIds.Contains(sequence.Id)) return false; // Prevent reentrance

            // Validate before starting
            var validation = sequence.Validate();
            if (!validation.Valid)
            {
                LastError = string.Join("; ", validation.Errors);
                State = CutsceneState.Error;
                return false;
            }

            Current = sequence;
            ElapsedTime = 0;
            skipRequested = false;
            activeSequenceIds.Add(sequence.Id);

            State = CutsceneState.Starting;
            inputGate.BlockGameplayInput();

            try
            {
                context.ElapsedTime = 0;
                sequence.Begin(context);
                State = CutsceneState.Playing;
                eventBridge.Emit(new CutsceneEvent(CutsceneEventType.SequenceStarted, sequence.Id));
            }
            catch (Exception e)
            {
                LastError = e.Message;
                State = CutsceneState.Error;
                ForceEnd();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tick the active cutscene. Call from Update().
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (State != CutsceneState.Playing) return;
            if (Current == null) { ForceEnd(); return; }

            ElapsedTime += deltaTime;
            context.ElapsedTime = ElapsedTime;

            if (skipRequested)
            {
                State = CutsceneState.Skipping;
                try
                {
                    Current.Skip(context);
                    eventBridge.Emit(new CutsceneEvent(CutsceneEventType.SequenceSkipped, Current.Id));
                }
                catch (Exception e)
                {
                    LastError = e.Message;
                }
                EndCurrent();
                return;
            }

            try
            {
                bool running = Current.Tick(deltaTime, context);
                if (!running) EndCurrent();
            }
            catch (Exception e)
            {
                LastError = e.Message;
                State = CutsceneState.Error;
                ForceEnd();
            }
        }

        /// <summary>
        /// Request skip. If the sequence is skippable, it will skip on next tick.
        /// </summary>
        public bool RequestSkip()
        {
            if (State != CutsceneState.Playing || Current == null) return false;
            if (!Current.Skippable) return false;
            skipRequested = true;
            return true;
        }

        /// <summary>
        /// Pause the cutscene (e.g., pause menu).
        /// </summary>
        public void Pause()
        {
            if (State == CutsceneState.Playing)
                State = CutsceneState.Paused;
        }

        /// <summary>
        /// Resume from pause.
        /// </summary>
        public void Resume()
        {
            if (State == CutsceneState.Paused)
                State = CutsceneState.Playing;
        }

        /// <summary>
        /// Cancel the cutscene (e.g., player death, scene change).
        /// Forces immediate cleanup without completing mandatory events.
        /// </summary>
        public void Cancel()
        {
            if (State == CutsceneState.Idle) return;
            if (Current != null)
                eventBridge.Emit(new CutsceneEvent(CutsceneEventType.SequenceCancelled, Current.Id));
            ForceEnd();
        }

        /// <summary>
        /// Whether a specific sequence has been completed in this session.
        /// </summary>
        public bool HasCompleted(string sequenceId) => completedSequences.Contains(sequenceId);

        /// <summary>
        /// Whether a cutscene is currently active (any non-idle state).
        /// </summary>
        public bool IsActive => State != CutsceneState.Idle;

        // --- Internal ---

        void EndCurrent()
        {
            if (Current == null) return;
            State = CutsceneState.Ending;

            try { Current.End(context); }
            catch (Exception) { /* Log but don't propagate during cleanup */ }

            completedSequences.Add(Current.Id);
            activeSequenceIds.Remove(Current.Id);
            eventBridge.Emit(new CutsceneEvent(CutsceneEventType.SequenceCompleted, Current.Id));
            eventBridge.ClearAll();

            inputGate.RestoreGameplayInput();
            Current = null;
            skipRequested = false;
            State = CutsceneState.Idle;
        }

        void ForceEnd()
        {
            try { Current?.End(context); } catch { }
            if (Current != null) activeSequenceIds.Remove(Current.Id);
            eventBridge.ClearAll();
            inputGate.ForceRestore();
            Current = null;
            skipRequested = false;
            State = CutsceneState.Idle;
        }
    }
}

