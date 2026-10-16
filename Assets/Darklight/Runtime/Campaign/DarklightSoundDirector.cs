using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public enum DarklightAudioCategory { Combat, Character, Interface, Enemy, Ambient }
    public sealed class DarklightSoundDirector : MonoBehaviour
    {
        const int VoiceLimit = 16;
        sealed class Voice { public AudioSource source; public DarklightAudioCategory category; public float gain, attenuation, began; }
        readonly Voice[] voices = new Voice[VoiceLimit];
        readonly Dictionary<string, AudioClip[]> clips = new();
        readonly Dictionary<string, int> variants = new();
        readonly Dictionary<string, float> ready = new();
        AudioClip[] allClips;
        DarklightCampaign game;
        AudioSource nature, dungeon, regional, outgoing;
        float ambientBlend, ambientTarget, layerBlend = 1, duck, nextOccasion;
        int swingSerial = -1, region;
        public int AllocatedVoices => voices.Length;
        public int ActiveVoices { get { int n = 0; foreach (var v in voices) if (v?.source != null && v.source.isPlaying) n++; return n; } }
        public float MusicDucking => duck > 0 ? .58f : 1;
        public static int CategoryVoiceLimit(DarklightAudioCategory category) => category == DarklightAudioCategory.Enemy ? 4 : category == DarklightAudioCategory.Ambient ? 2 : VoiceLimit;
        public void Initialize(DarklightCampaign owner)
        {
            if (game != null) return;
            game = owner; allClips = Resources.LoadAll<AudioClip>("Darklight/Audio");
            for (int i = 0; i < voices.Length; i++)
            {
                var src = NewSource("AudioVoice" + i); src.priority = 128;
                voices[i] = new Voice { source = src };
            }
            nature = Loop("Exterior", Resources.Load<AudioClip>("Darklight/Audio/park_ambience_wind"));
            dungeon = Loop("Veu", Resources.Load<AudioClip>("Darklight/Audio/dungeon_ambient_1"));
            regional = Loop("Regional", null); outgoing = Loop("RegionalTransition", null);
        }
        AudioSource NewSource(string name)
        {
            var src = new GameObject(name).AddComponent<AudioSource>(); src.transform.SetParent(transform, false);
            src.playOnAwake = false; src.spatialBlend = 0; src.volume = 0; return src;
        }
        AudioSource Loop(string name, AudioClip clip) { var src = NewSource(name); src.clip = clip; src.loop = true; src.priority = 180; return src; }
        public int SampleCount(string prefix) => Samples(prefix).Length;
        AudioClip[] Samples(string prefix)
        {
            if (clips.TryGetValue(prefix, out var list)) return list;
            var selected = new List<AudioClip>();
            if (allClips != null) foreach (var c in allClips) if (c.name.StartsWith(prefix, System.StringComparison.Ordinal)) selected.Add(c);
            selected.Sort((a, b) => string.CompareOrdinal(a.name, b.name)); return clips[prefix] = selected.ToArray();
        }
        public bool Play(string prefix, DarklightAudioCategory category, float gain = .35f, Vector2? position = null, float pitch = 1, float interval = .04f)
        {
            if (game == null || DarklightAutomation.Silent || category != DarklightAudioCategory.Interface && !game.Active) return false;
            float now = Time.unscaledTime; string key = prefix + category;
            if (ready.TryGetValue(key, out float until) && now < until) return false;
            var samples = Samples(prefix); if (samples.Length == 0) return false;
            Voice voice = null; int categoryCount = 0;
            foreach (var v in voices)
            {
                if (v.source.isPlaying && v.category == category) categoryCount++;
                if (!v.source.isPlaying && voice == null) voice = v;
            }
            if (voice == null || categoryCount >= CategoryVoiceLimit(category)) return false;
            ready[key] = now + interval;
            int index = variants.TryGetValue(key, out int last) ? (last + 1) % samples.Length : 0; variants[key] = index;
            voice.category = category; voice.gain = Mathf.Clamp01(gain) * Random.Range(.94f, 1.04f); voice.attenuation = 1; voice.began = now;
            if (position.HasValue)
            {
                var player = DarklightCombatLab.Instance?.Player;
                Vector2 delta = position.Value - (player == null ? Vector2.zero : (Vector2)player.transform.position);
                voice.source.panStereo = Mathf.Clamp(delta.x / 12, -.65f, .65f); voice.attenuation = 1 / (1 + delta.sqrMagnitude / 36);
                if (voice.attenuation < .06f) return false;
            }
            else voice.source.panStereo = 0;
            voice.source.clip = samples[index]; voice.source.pitch = Mathf.Clamp(pitch + Random.Range(-.035f, .035f), .7f, 1.35f);
            voice.source.priority = category == DarklightAudioCategory.Enemy ? 145 : category == DarklightAudioCategory.Ambient ? 185 : 85;
            voice.source.volume = Gain(voice); voice.source.Play(); return true;
        }
        float Gain(Voice voice) => Mathf.Min(.65f, voice.gain * voice.attenuation * game.CategoryVolume(voice.category));
        public void Swing(AttackDefinition attack, int serial, int weapon)
        {
            if (swingSerial == serial) return; swingSerial = serial;
            Play(weapon == 2 ? "rpg-metalLatch" : weapon == 1 ? "rpg-drawKnife" : "rpg-knifeSlice", DarklightAudioCategory.Combat, attack.kind == StrikeKind.Light ? .28f : .4f, null, weapon == 1 ? 1.16f : .96f);
            if (attack.kind == StrikeKind.Counter) Play("dl-power-counter", DarklightAudioCategory.Combat, .3f);
        }
        public void Contact(DarklightCombatant target, AttackDefinition attack)
        {
            var enemy = target.GetComponent<DarklightEnemy>(); bool metal = enemy != null && (enemy.kind == RequiemEnemyKind.Armored || enemy.kind == RequiemEnemyKind.Sentinel || enemy.kind == RequiemEnemyKind.Valtherion);
            Play(metal ? attack.kind == StrikeKind.Light ? "impact-impactMetal_light" : "impact-impactMetal_heavy" : attack.kind == StrikeKind.Light ? "impact-impactPunch_medium" : "impact-impactPunch_heavy", DarklightAudioCategory.Combat, .42f, target.transform.position);
            if (attack.kind == StrikeKind.Charged) Play("dl-power-critical", DarklightAudioCategory.Combat, .26f, target.transform.position);
            if (target.Health.IsDead) Play("dl-power-finisher", DarklightAudioCategory.Combat, .3f, target.transform.position, .95f, .2f);
            else if (metal) EnemyIdentity(enemy.kind, "block", target.transform.position);
        }
        public void Footstep(TodayFootstepSurface surface, Vector2 p) => Play(TodaySurfaces.Prefix(surface), DarklightAudioCategory.Character, surface == TodayFootstepSurface.Metal ? .13f : .16f, p, 1, .13f);
        public void Footstep(bool wood, bool grass, Vector2 p) => Footstep(wood ? TodayFootstepSurface.Wood : grass ? TodayFootstepSurface.Grass : TodayFootstepSurface.Stone, p);
        public static string EnemyPrefix(RequiemEnemyKind kind) => "dl-enemy-" + kind.ToString().ToLowerInvariant();
        public void EnemyIdentity(RequiemEnemyKind kind, string cue, Vector2 p)
        {
            bool boss = (int)kind >= 5;
            if (cue == "step")
            { Play(kind == RequiemEnemyKind.Armored || kind == RequiemEnemyKind.Sentinel ? "dl-step-metal" : kind == RequiemEnemyKind.Crawler ? "impact-footstep_grass" : "impact-footstep_concrete", DarklightAudioCategory.Enemy, boss ? .13f : .08f, p, boss ? .8f : 1.08f, .18f); return; }
            if (cue == "special")
            { Play(boss ? "dl-boss-" + kind.ToString().ToLowerInvariant() : "dl-power-shadow", DarklightAudioCategory.Enemy, boss ? .3f : .18f, p, 1, .5f); return; }
            if (cue == "block") { Play("impact-impactMetal_light", DarklightAudioCategory.Enemy, .19f, p, .85f, .12f); return; }
            if (cue == "attack" || cue == "heavy")
            { Play(cue == "heavy" ? "impact-impactPunch_heavy" : "rpg-knifeSlice", DarklightAudioCategory.Enemy, boss ? .26f : .18f, p, boss ? .8f : 1.08f, .12f); if (boss && cue == "heavy") EnemyIdentity(kind, "special", p); return; }
            float pitch = cue == "death" ? .75f : cue == "hurt" ? 1.13f : cue == "breath" ? .8f : 1;
            float gain = cue == "breath" ? .075f : cue == "death" ? .22f : .17f;
            Play(EnemyPrefix(kind), DarklightAudioCategory.Enemy, gain * (boss ? 1.2f : 1), p, pitch, cue == "breath" ? 2f : .16f);
        }
        public static string RegionLayer(int chapter) => chapter switch { 0 => "urban", 1 => "urban", 2 => "rain", 3 => "cave", 4 => "leaves", 5 => "veil", 6 => "cave", 7 => "rain", 8 => "veil", _ => "veil" };
        public void Region(int chapter)
        {
            region = chapter; ambientTarget = chapter == 3 || chapter == 5 || chapter >= 6 && chapter != 7 ? 1 : 0; swingSerial = -1; StopCombat();
            var clip = Resources.Load<AudioClip>("Darklight/Audio/Expansion/dl-ambient-" + (game.Map?.Art?.revision>=2&&chapter>=6?DarklightRegionAtmosphere.Bed(chapter):RegionLayer(chapter)));
            if (regional != null && regional.clip != clip)
            { outgoing.Stop(); var old = outgoing; outgoing = regional; regional = old; regional.clip = clip; layerBlend = 0; }
            if(regional!=null)regional.pitch=chapter>=6&&game.Map?.Art?.revision>=2?new[]{.92f,1.12f,.78f,.88f}[Mathf.Clamp(chapter-6,0,3)]:1;
            nextOccasion = Time.unscaledTime + 4;
        }
        public void StopCombat() { foreach (var voice in voices) if (voice?.source != null && voice.category != DarklightAudioCategory.Interface) voice.source.Stop(); }
        public void Awakening() { duck = 2; Play("dl-power-rupture", DarklightAudioCategory.Character, .32f, null, 1, .3f); }
        public void Transform(int form) { duck = .8f; Play(form == 1 ? "dl-power-rupture" : form == 2 ? "dl-power-eclipse" : "dl-power-absolute", DarklightAudioCategory.Character, .32f, null, 1, .3f); }
        public void Ability(int ability) => Play(ability == 0 ? "dl-power-claws" : ability == 1 ? "dl-power-shadow" : ability == 3 ? "dl-power-embrace" : ability == 5 ? "dl-power-judgment" : "dl-heartbeat", DarklightAudioCategory.Character, .25f, null, 1, .22f);
        public void EnemyWarning(bool boss, Vector2 p) => Play(boss ? "rpg-metalLatch" : "rpg-drawKnife", DarklightAudioCategory.Enemy, boss ? .3f : .19f, p, boss ? .8f : 1, .16f);
        void Update()
        {
            if (game == null) return;
            if (DarklightAutomation.Silent) { StopAll(); return; }
            foreach (var voice in voices) if (voice?.source != null && voice.source.isPlaying)
            { if (!game.Active && voice.category != DarklightAudioCategory.Interface) voice.source.Stop(); else voice.source.volume = Gain(voice); }
            if (nature == null || dungeon == null) return;
            if (!game.Active) { nature.Pause(); dungeon.Pause(); regional.Pause(); outgoing.Pause(); return; }
            Resume(nature); Resume(dungeon); Resume(regional); if (layerBlend < 1) Resume(outgoing);
            float dt = Time.unscaledDeltaTime; duck = Mathf.Max(0, duck - dt);
            ambientBlend = Mathf.MoveTowards(ambientBlend, ambientTarget, dt * .6f); layerBlend = Mathf.MoveTowards(layerBlend, 1, dt / 1.4f);
            float volume = game.AmbientVolume * (duck > 0 ? .5f : game.Resonance.Active >= 0 ? .75f : 1);
            nature.volume = volume * .30f * (1 - ambientBlend); dungeon.volume = volume * .30f * ambientBlend;
            regional.volume = volume * .18f * layerBlend; outgoing.volume = volume * .18f * (1 - layerBlend);
            if (layerBlend >= 1) outgoing.Pause();
            if (Time.unscaledTime >= nextOccasion)
            {
                nextOccasion = Time.unscaledTime + Random.Range(8f, 17f);
                string cue = region == 2 || region == 7 ? "thunder" : region == 3 || region == 6 ? "drip" : region == 4 ? "creature" : region == 8 || region == 9 ? "whisper" : "portal";
                if(region>=6&&game.Map?.Art?.revision>=2)cue=DarklightRegionAtmosphere.Cue(region);
                Vector2 p = DarklightCombatLab.Instance.Player.transform.position;
                if (game.Map != null && Vector2.Distance(p, game.Map.Portal) < 4) cue = "portal";
                Play("dl-ambient-" + cue, DarklightAudioCategory.Ambient, .17f, p + Random.insideUnitCircle * 7, 1, 3);
                if (region == 4) Play("dl-ambient-water", DarklightAudioCategory.Ambient, .11f, p, 1, 8);
            }
        }
        static void Resume(AudioSource src) { if (src.clip == null || src.isPlaying) return; src.UnPause(); if (!src.isPlaying) src.Play(); }
        public void StopAll()
        {
            foreach (var v in voices) if (v?.source != null) v.source.Stop();
            if (nature != null) nature.Stop(); if (dungeon != null) dungeon.Stop(); if (regional != null) regional.Stop(); if (outgoing != null) outgoing.Stop();
        }
        void OnDisable() => StopAll();
    }
}
