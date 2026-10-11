using UnityEngine;
namespace Darklight
{
    public enum ResonanceMode { Love, Heavy, Thrash }
    public sealed class DarklightResonance
    {
        public static readonly string[] Names = { "LOVE METAL", "HEAVY DE FORCA", "THRASH DE FOCO" };
        public static readonly float[] Durations = { 18, 12, 14 }, Cooldowns = { 32, 28, 30 }, Costs = { 35, 40, 40 };
        public static readonly Color[] Colors = { new Color(1,.2f,.4f), new Color(.95f,.65f,.25f), new Color(.35f,.7f,1) };
        public int Selected { get; private set; }
        public int Active { get; private set; } = -1;
        public float Remaining { get; private set; }
        public float Energy { get; private set; } = 100;
        public readonly float[] Recovery = new float[3];
        public bool Ready => Active < 0 && Recovery[Selected] <= 0 && Energy >= Costs[Selected];
        public float Damage => Active == 1 ? 1.45f : 1;
        public float Speed => Active == 2 ? 1.4f : 1;
        public float Agility => Active == 2 ? 1.2f : 1;
        public float Impact => Active == 1 ? 2 : 1;
        public void Select(int mode) { Selected = Mathf.Clamp(mode, 0, 2); }
        public void Cycle(int direction) { Selected = (Selected + direction + 3) % 3; }
        public bool Activate()
        {
            if (!Ready) return false;
            Active = Selected; Energy -= Costs[Active]; Remaining = Durations[Active]; return true;
        }
        public void Tick(float dt)
        {
            if (dt <= 0) return;
            for (int i=0;i<3;i++) Recovery[i] = Mathf.Max(0,Recovery[i]-dt);
            Energy = Mathf.Min(100, Energy + (Active < 0 ? 1.8f : .4f)*dt);
            if (Active >= 0 && (Remaining -= dt) <= 0) Stop();
        }
        public void Gain(float amount) { Energy = Mathf.Min(100,Energy + amount); }
        public void Stop()
        {
            if (Active >= 0) Recovery[Active] = Cooldowns[Active];
            Active = -1; Remaining = 0;
        }
        public void Restore(DarklightProgress progress)
        {
            Stop(); Energy = Mathf.Clamp(progress.resonanceEnergy,0,100); Select(progress.resonanceSelected);
            for(int i=0;i<3;i++) Recovery[i]=progress.resonanceRecoveries != null && progress.resonanceRecoveries.Length==3 ? Mathf.Max(0,progress.resonanceRecoveries[i]) : Mathf.Max(0,progress.resonanceCooldown);
        }
        public void Store(DarklightProgress progress)
        {
            progress.resonanceEnergy = Energy; progress.resonanceSelected = Selected;
            progress.resonanceRecoveries = (float[])Recovery.Clone();
            if(Active>=0) progress.resonanceRecoveries[Active]=Remaining+Cooldowns[Active];
            progress.resonanceCooldown = 0;
        }
    }
}
