using System;
namespace Darklight
{
    [Flags]
    public enum TodayPowerAudioEvent
    {
        None = 0, FormEnded = 1, FrenzyEnded = 2, ResonanceEnded = 4, HeadphonesRemoved = 8
    }

    // Observes presentation edges only. Loading, death and paused/menu edits
    // establish a new baseline instead of replaying stale completion sounds.
    public sealed class TodayPowerAudioTransitions
    {
        bool initialized, frenzy, headphones;
        int form, resonance;
        public void Reset() => initialized = false;
        public TodayPowerAudioEvent Tick(bool advancing, int nextForm, bool nextFrenzy, int nextResonance, bool nextHeadphones)
        {
            var events = TodayPowerAudioEvent.None;
            if (initialized && advancing)
            {
                if (form > 0 && nextForm == 0) events |= TodayPowerAudioEvent.FormEnded;
                if (frenzy && !nextFrenzy) events |= TodayPowerAudioEvent.FrenzyEnded;
                if (resonance >= 0 && nextResonance < 0)
                    events |= TodayPowerAudioEvent.ResonanceEnded | TodayPowerAudioEvent.HeadphonesRemoved;
                // Finishing the equip animation enters Resonance and is not a removal.
                else if (headphones && !nextHeadphones && nextResonance < 0)
                    events |= TodayPowerAudioEvent.HeadphonesRemoved;
            }
            initialized = true; form = nextForm; frenzy = nextFrenzy;
            resonance = nextResonance; headphones = nextHeadphones;
            return events;
        }
    }

    // Track each recovery independently so cycling the selected ability cannot
    // manufacture a completion edge. Resource recovery may delay the selected
    // cue until the ability is actually available, once per completed recovery.
    public sealed class TodayCooldownAudio
    {
        readonly float[] blood = new float[6], resonance = new float[3];
        bool initialized;
        int bloodPending, resonancePending;
        public void Reset() { initialized = false; bloodPending = resonancePending = 0; }
        static int SelectedBit(int selected, int count) => selected >= 0 && selected < count ? 1 << selected : 0;
        static void Capture(float[] previous, float[] current, bool advancing, ref int pending)
        {
            for (int i = 0; i < previous.Length; i++)
            {
                float next = current != null && i < current.Length ? current[i] : 0;
                if (advancing && previous[i] > 0 && next <= 0) pending |= 1 << i;
                if (next > 0) pending &= ~(1 << i);
                previous[i] = next;
            }
        }
        public bool Tick(bool advancing, float[] bloodRecovery, int selectedBlood, bool bloodReady,
            float[] resonanceRecovery, int selectedResonance, bool resonanceReady)
        {
            bool observe = initialized && advancing;
            Capture(blood, bloodRecovery, observe, ref bloodPending);
            Capture(resonance, resonanceRecovery, observe, ref resonancePending);
            initialized = true;
            if (!advancing) { bloodPending = resonancePending = 0; return false; }
            // Only the selected modes notify; old unselected completions stay quiet.
            bloodPending &= SelectedBit(selectedBlood, blood.Length);
            resonancePending &= SelectedBit(selectedResonance, resonance.Length);
            bool notifyBlood = bloodReady && bloodPending != 0;
            bool notifyResonance = resonanceReady && resonancePending != 0;
            if (notifyBlood) bloodPending = 0;
            if (notifyResonance) resonancePending = 0;
            return notifyBlood || notifyResonance;
        }
    }
}
