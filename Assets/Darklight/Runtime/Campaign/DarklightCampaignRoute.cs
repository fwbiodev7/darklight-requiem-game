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
            "GUARDAS DO PORTAL", "PATRULHA DO VINIL", "MATRIARCA RUBRA",
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
        void StartChapterGuardWave()
        {
            int chapter = Progress.chapter;
            if (chapter == DarklightCampaignRoute.FinalChapter && TryStartNeroTruce(StartChapterGuardWave)) return;
            encounterStarted = true;
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
            if (NeroTruceActive) StartCampaignAlly(false);
            Notice(Objective);
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
