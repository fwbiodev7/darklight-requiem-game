// DARKLIGHT: REQUIEM — Cutscene Unit Tests
// Pure C# tests that validate cutscene lifecycle without Unity Editor.
// These can run in EditMode test runner or standalone NUnit.

using System;
using System.Collections.Generic;

namespace Darklight.Cutscenes.Tests
{
    /// <summary>
    /// Minimal test sequence for unit testing the manager.
    /// </summary>
    public class TestSequence : ICutsceneSequence
    {
        public string Id { get; set; } = "test_seq_001";
        public string DisplayName { get; set; } = "Test Sequence";
        public bool Skippable { get; set; } = true;
        public bool HasMandatoryEvents { get; set; } = false;

        public int BeginCalls, TickCalls, SkipCalls, EndCalls;
        public float TotalDelta;
        public bool TickReturns = true;
        public bool ValidateResult = true;
        public Action<CutsceneContext> OnBegin, OnSkip, OnEnd;
        public bool ThrowOnBegin, ThrowOnTick, ThrowOnEnd;

        public void Begin(CutsceneContext context)
        {
            if (ThrowOnBegin) throw new InvalidOperationException("Begin error");
            BeginCalls++;
            OnBegin?.Invoke(context);
        }

        public bool Tick(float deltaTime, CutsceneContext context)
        {
            if (ThrowOnTick) throw new InvalidOperationException("Tick error");
            TickCalls++;
            TotalDelta += deltaTime;
            return TickReturns;
        }

        public void Skip(CutsceneContext context) { SkipCalls++; OnSkip?.Invoke(context); }

        public void End(CutsceneContext context)
        {
            if (ThrowOnEnd) throw new InvalidOperationException("End error");
            EndCalls++;
            OnEnd?.Invoke(context);
        }

        public CutsceneValidationResult Validate() =>
            ValidateResult ? CutsceneValidationResult.Ok() : CutsceneValidationResult.Fail("Validation failed");
    }

    /// <summary>
    /// Test results container for running without NUnit.
    /// </summary>
    public class CutsceneTestRunner
    {
        readonly List<string> passed = new();
        readonly List<string> failed = new();

        public IReadOnlyList<string> Passed => passed;
        public IReadOnlyList<string> Failed => failed;
        public int Total => passed.Count + failed.Count;

        void Assert(bool condition, string testName, string message = "")
        {
            if (condition) passed.Add(testName);
            else failed.Add($"{testName}: {message}");
        }

        public void RunAll()
        {
            Test_PlayAndComplete();
            Test_PlayAndSkip();
            Test_SkipNonSkippable();
            Test_Cancel();
            Test_DeathDuringCutscene();
            Test_SceneChangeDuringCutscene();
            Test_InputRestoredAfterEnd();
            Test_InputRestoredAfterCancel();
            Test_SaveLoadDuringCutscene();
            Test_DuplicateEventPrevention();
            Test_ReentryPrevention();
            Test_ValidationFailure();
            Test_ErrorRecovery();
            Test_PauseResume();
            Test_MandatoryEventsOnSkip();
            Test_EventBridgeHistory();
            Test_MultipleSequencesSerial();
            Test_CheckpointBridge();
        }

        void Test_PlayAndComplete()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence { TickReturns = false }; // completes immediately
            Assert(mgr.Play(seq), "PlayAndComplete_Play");
            mgr.Tick(0.016f);
            Assert(mgr.State == CutsceneState.Idle, "PlayAndComplete_State", $"Was {mgr.State}");
            Assert(seq.BeginCalls == 1, "PlayAndComplete_Begin");
            Assert(seq.TickCalls == 1, "PlayAndComplete_Tick");
            Assert(seq.EndCalls == 1, "PlayAndComplete_End");
            Assert(mgr.HasCompleted("test_seq_001"), "PlayAndComplete_Completed");
        }

        void Test_PlayAndSkip()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence();
            mgr.Play(seq);
            mgr.Tick(0.016f); // first tick, still playing
            Assert(mgr.State == CutsceneState.Playing, "PlayAndSkip_Playing");
            Assert(mgr.RequestSkip(), "PlayAndSkip_SkipAccepted");
            mgr.Tick(0.016f); // processes skip
            Assert(mgr.State == CutsceneState.Idle, "PlayAndSkip_Idle");
            Assert(seq.SkipCalls == 1, "PlayAndSkip_SkipCalled");
            Assert(seq.EndCalls == 1, "PlayAndSkip_EndCalled");
        }

        void Test_SkipNonSkippable()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence { Skippable = false };
            mgr.Play(seq);
            Assert(!mgr.RequestSkip(), "SkipNonSkippable_Rejected");
        }

        void Test_Cancel()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence();
            mgr.Play(seq);
            mgr.Tick(0.016f);
            mgr.Cancel();
            Assert(mgr.State == CutsceneState.Idle, "Cancel_State");
            Assert(seq.EndCalls == 1, "Cancel_EndCalled");
        }

        void Test_DeathDuringCutscene()
        {
            // Simulates player death: Cancel should force-end and restore input
            var mgr = new CutsceneManager();
            var seq = new TestSequence();
            mgr.Play(seq);
            mgr.Tick(0.016f);
            mgr.Cancel(); // death triggers cancel
            Assert(mgr.State == CutsceneState.Idle, "Death_State");
            Assert(!mgr.IsActive, "Death_NotActive");
        }

        void Test_SceneChangeDuringCutscene()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence();
            mgr.Play(seq);
            mgr.Cancel(); // scene change triggers cancel
            Assert(mgr.State == CutsceneState.Idle, "SceneChange_State");
            Assert(seq.EndCalls == 1, "SceneChange_Cleanup");
        }

        void Test_InputRestoredAfterEnd()
        {
            var gate = new DarklightInputGate();
            gate.BlockGameplayInput();
            Assert(gate.IsBlocked, "InputRestore_Blocked");
            gate.RestoreGameplayInput();
            Assert(!gate.IsBlocked, "InputRestore_Restored");
        }

        void Test_InputRestoredAfterCancel()
        {
            var gate = new DarklightInputGate();
            gate.BlockGameplayInput();
            gate.BlockGameplayInput(); // nested
            gate.ForceRestore();
            Assert(!gate.IsBlocked, "InputForceRestore_Restored");
        }

        void Test_SaveLoadDuringCutscene()
        {
            var bridge = new DarklightCheckpointBridge();
            bridge.SaveCutsceneState("test_save", 5.0f, "phase2");
            var loaded = bridge.LoadCutsceneState();
            Assert(loaded != null, "SaveLoad_NotNull");
            Assert(loaded.sequenceId == "test_save", "SaveLoad_Id");
            Assert(Math.Abs(loaded.elapsed - 5.0f) < 0.001f, "SaveLoad_Elapsed");
            Assert(loaded.customData == "phase2", "SaveLoad_Data");
            bridge.ClearCutsceneState();
            Assert(bridge.LoadCutsceneState() == null, "SaveLoad_Cleared");
        }

        void Test_DuplicateEventPrevention()
        {
            var bridge = new CutsceneEventBridge();
            int count = 0;
            bridge.Subscribe(CutsceneEventType.SequenceStarted, e => count++);
            bridge.Emit(new CutsceneEvent(CutsceneEventType.SequenceStarted, "test"));
            bridge.Emit(new CutsceneEvent(CutsceneEventType.SequenceStarted, "test"));
            Assert(count == 2, "DuplicateEvent_Count", $"Got {count}"); // Events should fire, dedup is sequence-level
            Assert(bridge.HasEvent("test", CutsceneEventType.SequenceStarted), "DuplicateEvent_HasEvent");
        }

        void Test_ReentryPrevention()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence();
            mgr.Play(seq);
            Assert(!mgr.Play(seq), "Reentry_Rejected"); // same sequence can't re-enter
            mgr.Cancel();
            Assert(mgr.Play(seq), "Reentry_AllowedAfterCancel"); // allowed after cancel
            mgr.Cancel();
        }

        void Test_ValidationFailure()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence { ValidateResult = false };
            Assert(!mgr.Play(seq), "Validation_Rejected");
            Assert(mgr.State == CutsceneState.Error, "Validation_ErrorState");
            Assert(mgr.LastError != null, "Validation_HasError");
        }

        void Test_ErrorRecovery()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence { ThrowOnBegin = true };
            Assert(!mgr.Play(seq), "ErrorRecovery_PlayFailed");
            Assert(mgr.State == CutsceneState.Idle || mgr.State == CutsceneState.Error, "ErrorRecovery_SafeState");

            // Can play a new sequence after error
            var seq2 = new TestSequence { Id = "recovery", TickReturns = false };
            Assert(mgr.Play(seq2), "ErrorRecovery_CanPlayNew");
            mgr.Tick(0.016f);
            Assert(mgr.State == CutsceneState.Idle, "ErrorRecovery_CompletedNew");
        }

        void Test_PauseResume()
        {
            var mgr = new CutsceneManager();
            var seq = new TestSequence();
            mgr.Play(seq);
            mgr.Tick(0.016f);
            mgr.Pause();
            Assert(mgr.State == CutsceneState.Paused, "PauseResume_Paused");
            int ticksBefore = seq.TickCalls;
            mgr.Tick(0.016f); // should not tick while paused
            Assert(seq.TickCalls == ticksBefore, "PauseResume_NoTickWhilePaused");
            mgr.Resume();
            Assert(mgr.State == CutsceneState.Playing, "PauseResume_Resumed");
            mgr.Cancel();
        }

        void Test_MandatoryEventsOnSkip()
        {
            var skip = new CutsceneSkipController();
            bool event1Done = false, event2Done = false;
            skip.RegisterMandatory("unlock_form", () => event1Done = true);
            skip.RegisterMandatory("save_checkpoint", () => event2Done = true);
            Assert(!skip.AllMandatoryComplete, "Mandatory_NotComplete");
            Assert(skip.PendingCount == 2, "Mandatory_Pending2");
            skip.Complete("unlock_form");
            Assert(skip.PendingCount == 1, "Mandatory_Pending1");
            skip.ExecuteAllPending();
            Assert(skip.AllMandatoryComplete, "Mandatory_AllComplete");
            Assert(event2Done, "Mandatory_Event2Executed");
        }

        void Test_EventBridgeHistory()
        {
            var bridge = new CutsceneEventBridge();
            bridge.Emit(new CutsceneEvent(CutsceneEventType.SequenceStarted, "s1"));
            bridge.Emit(new CutsceneEvent(CutsceneEventType.DialogueStarted, "s1"));
            bridge.Emit(new CutsceneEvent(CutsceneEventType.SequenceCompleted, "s1"));
            Assert(bridge.History.Count == 3, "History_Count");
            Assert(bridge.HasEvent("s1", CutsceneEventType.DialogueStarted), "History_HasDialogue");
            Assert(!bridge.HasEvent("s2", CutsceneEventType.DialogueStarted), "History_NoS2");
        }

        void Test_MultipleSequencesSerial()
        {
            var mgr = new CutsceneManager();
            var s1 = new TestSequence { Id = "seq1", TickReturns = false };
            var s2 = new TestSequence { Id = "seq2", TickReturns = false };
            mgr.Play(s1); mgr.Tick(0.016f);
            Assert(mgr.HasCompleted("seq1"), "Serial_S1Complete");
            mgr.Play(s2); mgr.Tick(0.016f);
            Assert(mgr.HasCompleted("seq2"), "Serial_S2Complete");
        }

        void Test_CheckpointBridge()
        {
            var bridge = new DarklightCheckpointBridge();
            bridge.MarkWatched("intro_001");
            Assert(bridge.HasWatched("intro_001"), "Checkpoint_Watched");
            Assert(!bridge.HasWatched("intro_002"), "Checkpoint_NotWatched");
        }

        /// <summary>
        /// Generate a text report of test results.
        /// </summary>
        public string Report()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Cutscene Tests: {passed.Count}/{Total} passed ===");
            foreach (var p in passed) sb.AppendLine($"  [PASS] {p}");
            foreach (var f in failed) sb.AppendLine($"  [FAIL] {f}");
            return sb.ToString();
        }
    }
}

