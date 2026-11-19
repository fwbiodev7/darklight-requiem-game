using UnityEngine;

namespace Darklight
{
    public static class DarklightCampaignRoute
    {
        public const int MatriarchChapter = 2;
        public const int NeroChapter = 8;
        public const int FinalChapter = 9;

        static readonly string[] GateTitles =
        {
            "RITUAL DOS TRÊS SELOS", "PATRULHA DO VINIL", "MATRIARCA RUBRA",
            "VIGÍLIA DO LABORATÓRIO", "EMBOSCADA DOS ECOS", "GUARDIÕES DO OBSERVATÓRIO",
            "JURAMENTO DOS GUARDIÕES", "ÚLTIMA DEFESA DO ABRIGO", "NERO FRATURADO",
            "CAÇADA DAS FENDAS"
        };

        public static bool TryBoss(int chapter, out RequiemEnemyKind kind)
        {
            switch (chapter)
            {
                case MatriarchChapter: kind = RequiemEnemyKind.Matriarch; return true;
                case NeroChapter: kind = RequiemEnemyKind.KaelFractured; return true;
                default: kind = default; return false;
            }
        }

        public static string GateTitle(int chapter) => GateTitles[chapter];
    }

    public sealed partial class DarklightCampaign
    {
        static readonly string[] RitualSeals = {"LUA", "ROSA", "CHAVE"};
        static readonly Vector2[] RitualOffsets = {new(-1.9f, .65f), new(1.9f, .65f), new(0, 2.15f)};
        const string RitualSealPrefix = "ritual_seal_";
        const string RitualClue = "ritual_clue";
        int ritualStep;

        bool FinalRitualReady => Progress.chapter == 0 && encounter == 3 && !chapterCleared && !encounterStarted;

        Vector2 RitualSealPoint(int index) => Map.Arenas[3] + RitualOffsets[index];
        Vector2 RitualCluePoint => Map.Arenas[3] + Vector2.down * 1.6f;

        void PrepareFinalRitual()
        {
            ritualStep = 0;
            if (!FinalRitualReady) return;
            Sprite seal = Resources.Load<Sprite>("Darklight/Props/CheckpointShrine") ?? extras[3];
            for (int i = 0; i < RitualSeals.Length; i++)
            {
                string id = RitualSealPrefix + i;
                AddProp(id, seal, RitualSealPoint(i), .92f);
                var marker = levelRoot.Find(id);
                var label = new GameObject("Inscrição").AddComponent<TextMesh>();
                label.transform.SetParent(marker, false);
                label.transform.localPosition = Vector3.up * seal.bounds.size.y * 1.05f;
                label.transform.localScale = Vector3.one / marker.localScale.x;
                label.anchor = TextAnchor.MiddleCenter;
                label.characterSize = .12f;
                label.fontSize = 42;
                label.text = RitualSeals[i];
                label.color = new Color(1f, .81f, .67f);
                label.GetComponent<MeshRenderer>().sortingOrder = marker.GetComponent<SpriteRenderer>().sortingOrder + 1;
            }
            AddProp(RitualClue, extras[5], RitualCluePoint, .72f);
        }

        void MarkRitualStep()
        {
            for (int i = 0; i < RitualSeals.Length; i++)
            {
                var marker = levelRoot.Find(RitualSealPrefix + i);
                if (marker != null)
                    marker.GetComponent<SpriteRenderer>().color = i < ritualStep ? new Color(.65f, 1f, .75f) : Color.white;
            }
        }

        bool InteractFinalRitual(string id)
        {
            if (!FinalRitualReady) return false;
            if (id == RitualClue)
            {
                Speak(new[] {"INSCRIÇÃO|Quando a lua nascer, a rosa se abre. Da rosa desperta a chave."});
                return true;
            }
            if (!id.StartsWith(RitualSealPrefix, System.StringComparison.Ordinal) ||
                !int.TryParse(id.Substring(RitualSealPrefix.Length), out int seal) ||
                seal < 0 || seal >= RitualSeals.Length) return false;
            if (seal != ritualStep)
            {
                ritualStep = 0;
                MarkRitualStep();
                Objective = Goal();
                Notice("OS SELOS SE APAGARAM — CONSULTE A INSCRIÇÃO");
                return true;
            }
            ritualStep++;
            MarkRitualStep();
            if (ritualStep < RitualSeals.Length)
            {
                Objective = $"ATIVAR OS SELOS — {ritualStep} / {RitualSeals.Length}";
                return true;
            }
            foreach (string marker in new[] {"ritual_seal_0", "ritual_seal_1", "ritual_seal_2", RitualClue})
            {
                var prop = levelRoot.Find(marker);
                if (prop != null) prop.gameObject.SetActive(false);
            }
            StartChapterGuardWave();
            return true;
        }

        void StartChapterGuardWave()
        {
            encounterStarted = true;
            int chapter = Progress.chapter;
            int wave = chapter == DarklightCampaignRoute.FinalChapter ? Progress.missionWave : 0;
            var pack = (chapter, wave) switch
            {
                (0, _) => new[] {RequiemEnemyKind.Crawler, RequiemEnemyKind.Hunter},
                (1, _) => new[] {RequiemEnemyKind.Shooter, RequiemEnemyKind.Hunter, RequiemEnemyKind.Crawler},
                (3, _) => new[] {RequiemEnemyKind.Armored, RequiemEnemyKind.Shooter, RequiemEnemyKind.Shooter},
                (4, _) => new[] {RequiemEnemyKind.Spectral, RequiemEnemyKind.Crawler, RequiemEnemyKind.Hunter},
                (5, _) => new[] {RequiemEnemyKind.Hunter, RequiemEnemyKind.Shooter, RequiemEnemyKind.Spectral},
                (6, _) => new[] {RequiemEnemyKind.Armored, RequiemEnemyKind.Spectral, RequiemEnemyKind.Crawler},
                (7, _) => new[] {RequiemEnemyKind.Hunter, RequiemEnemyKind.Hunter, RequiemEnemyKind.Armored},
                (DarklightCampaignRoute.FinalChapter, 0) => new[] {RequiemEnemyKind.Crawler, RequiemEnemyKind.Hunter, RequiemEnemyKind.Spectral},
                _ => new[] {RequiemEnemyKind.Armored, RequiemEnemyKind.Shooter, RequiemEnemyKind.Spectral}
            };
            string title = DarklightCampaignRoute.GateTitle(chapter);
            Objective = chapter == DarklightCampaignRoute.FinalChapter ? $"{title} — GRUPO {wave + 1} / 2" : $"LIMPE A ÁREA — {title}";
            float side = wave == 0 ? 1f : -1f;
            for (int i = 0; i < pack.Length; i++)
            {
                var offset = new Vector2((i - (pack.Length - 1) * .5f) * 1.6f, i == 1 ? 1.5f * side : -.85f * side);
                Spawn(pack[i], Map.Arenas[3] + offset, chapter >= 4 && i == 0);
            }
            Notice(chapter == 0 ? "RITUAL CONCLUÍDO — DERROTE OS GUARDIÕES" : Objective);
        }

        bool AdvanceFinalGuardWave()
        {
            if (Progress.chapter != DarklightCampaignRoute.FinalChapter || encounter != 3 || Progress.missionWave != 0)
                return false;
            Progress.missionWave = 1;
            SaveCheckpoint();
            StartChapterGuardWave();
            return true;
        }
    }
}
