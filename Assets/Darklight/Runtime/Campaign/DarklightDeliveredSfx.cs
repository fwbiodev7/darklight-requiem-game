using UnityEngine;
namespace Darklight
{
    // User supplied effects only. Commercial boss music and the four long sources
    // awaiting selection are deliberately absent from this runtime catalogue.
    public sealed partial class DarklightSoundDirector
    {
        public static readonly string[] DeliveredIds = {
            "WM01","WM03","WM04","WM05","WM08", "PC01","PC02","PC03","PC04","PC05","PC06",
            "PW03","PW04","PW05","PW06","PW07","PW08","PW10","PW11"
        };
        public static string DeliveredPrefix(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var known in DeliveredIds) if (known == id) return "dl-user-" + id.ToLowerInvariant();
            return null;
        }
        public static string AbilityDelivery(int ability) => ability switch { 2 => "PW03", 3 => "PW04", 4 => "PW05", 5 => "PW06", _ => null };
        public static string SwingDelivery(AttackDefinition attack, int weapon)
        {
            if (attack == null) return null;
            if (attack.ruinStrike == DarklightRuinStrike.SideKick || attack.ruinStrike == DarklightRuinStrike.SpinKick) return "WM05";
            return weapon == 1 ? "WM03" : null;
        }
        bool Preferred(string id, string fallback, DarklightAudioCategory category, float gain = .3f,
            Vector2? position = null, float pitch = 1, float interval = .08f)
        {
            string prefix = DeliveredPrefix(id);
            if (prefix != null && Samples(prefix).Length > 0) return Play(prefix, category, gain, position, pitch, interval);
            return fallback != null && Play(fallback, category, gain, position, pitch, interval);
        }
        public bool Delivered(string id, DarklightAudioCategory category = DarklightAudioCategory.Character,
            float gain = .25f, float interval = .15f, bool finishWhenInactive = false)
        {
            string prefix = DeliveredPrefix(id);
            if (prefix == null) return false;
            return Play(prefix, category, gain, null, 1, interval, finishWhenInactive);
        }
        public void PlayerDodge() => Preferred("PC01", "dl-power-dodge", DarklightAudioCategory.Character, .18f);
        public void PlayerPain() => Preferred("PC02", "impact-impactPunch_heavy", DarklightAudioCategory.Character, .27f, null, 1, .25f);
        public void PlayerDeath() => Delivered("PC03", DarklightAudioCategory.Character, .3f, 1.2f, true);
        public void LowHealthBreath() => Preferred("PC04", "dl-heartbeat", DarklightAudioCategory.Character, .13f, null, 1, 7.2f);
        public void EquipWeapon(int weapon)
        {
            // A physical draw replaces the generic equip cue where one was supplied.
            if (weapon == 0) Preferred("WM01", "rpg-drawKnife", DarklightAudioCategory.Character, .24f);
            else if (weapon == 1) Preferred("WM03", "rpg-drawKnife", DarklightAudioCategory.Character, .24f);
            else Delivered("WM08", DarklightAudioCategory.Character, .24f);
        }
        public void ActionDenied() => Delivered("PW11", DarklightAudioCategory.Interface, .16f, .7f);
        // Existing foley/UI banks are provisional feedback for newly wired events.
        // They do not imply that a dedicated supplied sheathe/end/ready take was heard.
        public const string WeaponStowFallback = "rpg-metalLatch";
        public const string HeadphonesRemoveFallback = "rpg-cloth";
        public const string PowerEndFallback = "interface-close";
        public const string CooldownReadyFallback = "interface-confirmation";
        public void StowWeapon() => Play(WeaponStowFallback, DarklightAudioCategory.Character, .12f, null, .9f, .12f);
        public void RemoveHeadphones() => Play(HeadphonesRemoveFallback, DarklightAudioCategory.Character, .12f, null, 1, .3f);
        public void PowerEnded() => Play(PowerEndFallback, DarklightAudioCategory.Character, .13f, null, .85f, .25f);
        public void CooldownReady() => Play(CooldownReadyFallback, DarklightAudioCategory.Character, .12f, null, 1, .5f);
        public void StopDelivered(string id)
        {
            string prefix = DeliveredPrefix(id);
            if (prefix == null) return;
            foreach (var voice in voices)
                if (voice?.source?.clip != null && voice.source.clip.name == prefix) voice.source.Stop();
        }
    }
}
