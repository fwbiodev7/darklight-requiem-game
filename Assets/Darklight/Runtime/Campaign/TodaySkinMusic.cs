using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public AudioSource EventMusicSource => music;
        public bool EventMusicHasPriority => firstAwakeningMusicPending || HasFirstAwakeningMusic && music.isPlaying;
        public TodaySkinMusic SkinMusic { get; private set; }
        public TodayTransformationMusic TransformationMusic { get; private set; }
        void FinishMusicEvent() { if (SkinMusic != null) SkinMusic.FinishEvent(); else music.Stop(); }
    }
    // Skin themes persist across map loads; the campaign keeps its original resonance source.
    public sealed class TodaySkinMusic : MonoBehaviour
    {
        public const float TransitionSeconds = .85f;
        sealed class Track
        {
            public AudioSource main, tail;
            public float weight, overlap;
            public bool paused, overlapping;
            public void Pause() { main.Pause(); tail.Pause(); paused = true; }
            public void Stop() { main.Stop(); tail.Stop(); weight = overlap = 0; overlapping = paused = false; }
            public void Resume()
            {
                if (paused) { main.UnPause(); if (overlapping) tail.UnPause(); paused = false; }
                if (!main.isPlaying && main.clip != null) main.Play();
            }
            public void Tick(float dt, float gain)
            {
                Resume(); if (main.clip == null) return;
                float seam = Mathf.Min(.7f, main.clip.length * .1f);
                if (!overlapping && main.time >= main.clip.length - seam)
                { tail.time = 0; tail.volume = 0; tail.Play(); overlapping = true; overlap = 0; }
                if (overlapping)
                {
                    overlap = Mathf.Min(1, overlap + dt / seam);
                    main.volume = gain * (1 - overlap); tail.volume = gain * overlap;
                    if (overlap >= 1)
                    {
                        main.Stop(); var previous = main; main = tail; tail = previous;
                        overlapping = false; overlap = 0; main.volume = gain;
                    }
                }
                else { main.volume = gain; tail.volume = 0; }
            }
        }
        DarklightCampaign game;
        Track rock, requiem;
        float eventWeight;
        bool eventEnding;
        AudioClip lastEvent;
        public AudioSource RockSource => rock?.main;
        public AudioSource RequiemSource => requiem?.main;
        public float RockWeight => rock?.weight ?? 0;
        public float RequiemWeight => requiem?.weight ?? 0;
        public float EventWeight => eventWeight;
        public static bool ShouldSuspend(bool active, bool resonance, bool phones, bool story) => !active || resonance || phones || story;
        public static float Fade(float weight, float target, float dt) => Mathf.MoveTowards(weight, target, Mathf.Max(0, dt) / TransitionSeconds);
        void Awake()
        {
            game = GetComponent<DarklightCampaign>();
            rock = CreateTrack("Rock", "RockTheme"); requiem = CreateTrack("Requiem", "RequiemTheme");
        }
        Track CreateTrack(string name, string resource)
        {
            var clip = Resources.Load<AudioClip>("Darklight/Music/" + resource);
            return new Track { main = Create(name, clip), tail = Create(name + "LoopSeam", clip) };
        }
        AudioSource Create(string name, AudioClip clip)
        {
            var source = new GameObject("Trilha" + name).AddComponent<AudioSource>();
            source.transform.SetParent(transform, false); source.clip = clip; source.playOnAwake = false;
            source.loop = true; source.spatialBlend = 0; source.volume = 0; source.priority = 40; return source;
        }
        public void BeginEvent() { eventEnding = false; eventWeight = 0; lastEvent = game.EventMusicSource.clip; }
        public void FinishEvent() { eventEnding = true; }
        void LateUpdate() => EvaluateMix(Time.unscaledDeltaTime);
        public void EvaluateMix(float dt)
        {
            if (game == null || rock == null || requiem == null) return;
            var source = game.EventMusicSource;
            
            // Menus freeze all playheads and weights; closing resumes the same sample.
            if (!game.Active) { rock.Pause(); requiem.Pause(); source.volume = game.MusicVolume * eventWeight; return; }
            dt = Mathf.Max(0, dt);
            if (source.clip != lastEvent) { lastEvent = source.clip; eventEnding = false; eventWeight = 0; }
            bool priority = !eventEnding && (game.Resonance.Active >= 0 || game.EventMusicHasPriority);
            eventWeight = Fade(eventWeight, priority && (source.isPlaying || DarklightAutomation.Silent) ? 1 : 0, dt);
            source.volume = DarklightAutomation.Silent ? 0 : game.MusicVolume * eventWeight;
            if (eventEnding && eventWeight <= 0) { source.Stop(); eventEnding = false; }
            bool suspend = priority || game.WearingHeadphones || game.TransformationMusic != null && game.TransformationMusic.HasTheme;
            AdvanceSkinFade(game.SelectedSkin, suspend, dt);
            if (DarklightAutomation.Silent) { rock.Pause(); requiem.Pause(); source.Pause(); rock.main.volume = rock.tail.volume = requiem.main.volume = requiem.tail.volume = 0; return; }
            float gain = game.MusicVolume * .7f * (game.AudioDirector?.MusicDucking ?? 1);
            Tick(rock, dt, gain); Tick(requiem, dt, gain);
        }
        public void AdvanceSkinFade(int skin, bool suspend, float dt)
        {
            rock.weight = Fade(rock.weight, !suspend && skin == 1 ? 1 : 0, dt);
            requiem.weight = Fade(requiem.weight, !suspend && skin == 0 ? 1 : 0, dt);
        }
        static void Tick(Track track, float dt, float gain)
        {
            if (track.weight > .0001f) track.Tick(dt, gain * track.weight);
            else { track.main.volume = track.tail.volume = 0; track.Pause(); }
        }
        void OnDisable() { rock?.Stop(); requiem?.Stop(); }
    }
}
