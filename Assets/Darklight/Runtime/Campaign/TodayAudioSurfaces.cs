using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public enum TodayFootstepSurface { Stone, Wood, Dirt, Grass, Sand, Water, Metal, Building, Mud }
    [RequireComponent(typeof(Collider2D))]
    public sealed class TodaySurfaceArea : MonoBehaviour
    {
        public TodayFootstepSurface surface; public int priority;
        internal static readonly List<TodaySurfaceArea> Areas = new();
        Collider2D shape;
        void Awake() { shape = GetComponent<Collider2D>(); }
        void OnEnable() { if (!Areas.Contains(this)) Areas.Add(this); }
        void OnDisable() { Areas.Remove(this); }
        public bool Contains(Vector2 p) => (shape != null ? shape : GetComponent<Collider2D>()).OverlapPoint(p);
    }
    public static class TodaySurfaces
    {
        static readonly TodayFootstepSurface[] Regions = { TodayFootstepSurface.Stone, TodayFootstepSurface.Building,
            TodayFootstepSurface.Metal, TodayFootstepSurface.Stone, TodayFootstepSurface.Grass,
            TodayFootstepSurface.Building, TodayFootstepSurface.Stone, TodayFootstepSurface.Stone,
            TodayFootstepSurface.Stone, TodayFootstepSurface.Building };
        public static string Prefix(TodayFootstepSurface surface) => surface switch {
            TodayFootstepSurface.Stone => "impact-footstep_concrete", TodayFootstepSurface.Wood => "impact-footstep_wood",
            TodayFootstepSurface.Grass => "impact-footstep_grass", TodayFootstepSurface.Dirt => "dl-step-dirt",
            TodayFootstepSurface.Sand => "dl-step-sand", TodayFootstepSurface.Water => "dl-step-water",
            TodayFootstepSurface.Metal => "dl-step-metal", TodayFootstepSurface.Building => "dl-step-building", _ => "dl-step-mud" };
        public static TodayFootstepSurface Resolve(int region, Vector2 position, Vector2 spawn, DarklightMapDefinition map = null)
        {
            TodaySurfaceArea found = null;
            foreach (var area in TodaySurfaceArea.Areas)
                if (area != null && area.Contains(position) && (found == null || area.priority > found.priority)) found = area;
            if (found != null) return found.surface;
            if (region == 4 && map?.Art != null)
            {
                // Pixel rectangles follow the three complete bridges and shallow pool in map5.png.
                // They classify terrain only; navigation and visual geometry remain authored by the map.
                Vector2 p = map.Art.Pixel(position);
                if (new Rect(522, 226, 103, 56).Contains(p) || new Rect(621, 462, 103, 51).Contains(p) || new Rect(1090, 632, 146, 63).Contains(p)) return TodayFootstepSurface.Wood;
                if (new Rect(135, 418, 218, 100).Contains(p)) return TodayFootstepSurface.Water;
                if (new Rect(353, 450, 76, 68).Contains(p)) return TodayFootstepSurface.Mud;
                if (new Rect(440, 540, 160, 80).Contains(p)) return TodayFootstepSurface.Dirt;
            }
            return Regions[Mathf.Clamp(region, 0, Regions.Length - 1)];
        }
    }
    // Contacts match walking frames 0 and 2 in the current four-frame, 10fps catalog.
    // Actual displacement is required: input against walls and attack lunges are silent.
    public sealed class TodayStepContacts
    {
        int bucket = -1; Vector2 previous; bool ready;
        public bool Tick(float clock, bool walking, Vector2 position)
        {
            int now = Mathf.FloorToInt(clock * 5f);
            float distance = ready ? (position - previous).sqrMagnitude : 0;
            previous = position;
            bool emit = ready && walking && now != bucket && distance > .000025f && distance < 2.25f;
            bucket = now; ready = walking; return emit;
        }
        public void Reset() { ready = false; bucket = -1; }
    }
    public sealed class TodayEnemyAudio : MonoBehaviour
    {
        DarklightEnemy enemy; Rigidbody2D body; float nextBreath; int lastFrame=-1, phase;
        bool secondPhase; string state; Vector2 lastPosition; HealthSystem health;
        void Start()
        {
            enemy = GetComponent<DarklightEnemy>(); body = GetComponent<Rigidbody2D>(); health = GetComponent<HealthSystem>();
            if (health != null) { health.OnDamageReceived += Hurt; health.OnDeath += Death; }
            lastPosition = transform.position; phase = enemy?.Phase ?? 1;
        }
        void Hurt(float amount) { if (enemy != null && !health.IsDead) Emit("hurt"); }
        void Death() { Emit("death"); }
        void Emit(string cue) { if (enemy != null) DarklightCampaign.Instance?.AudioDirector?.EnemyIdentity(enemy.kind, cue, transform.position); }
        void LateUpdate()
        {
            var game = DarklightCampaign.Instance;
            if (enemy == null || body == null || game == null || !game.Active || health.IsDead || enemy.Resolved || DarklightAutomation.Silent) { lastPosition = transform.position; return; }
            string current = enemy.StateName;
            if (current != state) { if (current == "Preparando") Emit("windup"); state = current; }
            if (enemy.Phase != phase || enemy.Boss && !secondPhase && health.HealthPercent < .5f)
            { phase = enemy.Phase; secondPhase = true; Emit("special"); }
            int frame = Mathf.FloorToInt(Time.time * 6);
            bool moving = body.linearVelocity.sqrMagnitude > .1f && ((Vector2)transform.position - lastPosition).sqrMagnitude > .000025f;
            if (current == "Vigiando" && moving && frame != lastFrame) { Emit("step"); lastFrame = frame; }
            if (Time.time >= nextBreath) { nextBreath = Time.time + Random.Range(3.5f, 6.5f); Emit("breath"); }
            lastPosition = transform.position;
        }
        void OnDestroy() { if (health != null) { health.OnDamageReceived -= Hurt; health.OnDeath -= Death; } }
    }
}
