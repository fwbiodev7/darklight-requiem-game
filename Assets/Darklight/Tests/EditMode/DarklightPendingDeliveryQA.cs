using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    [InitializeOnLoad] public static class DarklightPendingDeliveryQA
    {
        const string Key="Darklight.PendingDelivery.Result";
        static DarklightPendingDeliveryQA(){EditorApplication.update+=Finish;}
        public static void TechnicalFinal()=>Run("technical-final-20261018","^Darklight.Tests.(DarklightBridgeDepthTests|DarklightGameOverFlowTests|DarklightSustainedCombatTests|DarklightAudioSettingsTests|DarklightConsistencyGameplayTests|DarklightLitPaletteTests|DarklightTechnicalDeliveryGameplayTests)\\.|^Darklight.Tests.DarklightAbyssPrototypeTests.(ImpactPauseRetainsBikeAndMountedDriftWithoutDoublePayment|BufferedBikeWinsOverHeldSawAndPaysOnceAfterContact|ReleasedSustainQueuesBikeWithoutCuttingPaidContactOrRecovery|DriftCannotRemountOnceVaultHasStarted|MountedSlamLiftsBikeWithoutFirstSplittingAndPaysOnce)$|^Darklight.Tests.(DarklightCampaignTests.(MovementPauseAndDodgeUseActualPhysics|AllPowersApplyGameplayAndKeepFormsIndependent)|DarklightGameplayTests.(CrowdedStrikesHitEveryTargetAndEnemyAreasCannotCrossWalls|AreaThreatRequiresEvadingAndWeaponsKeepTheirDifferentCounters)|DarklightAgeWeaponRegressionTests.*)$");
        public static void GameOver()=>Run("gameover-flow","^Darklight.Tests.DarklightGameOverFlowTests\\.");
        public static void Bridges()=>Run("bridge-depth","^Darklight.Tests.DarklightBridgeDepthTests\\.");
        public static void Delivery()=>Run("delivery-playmode","^Darklight.Tests.(DarklightBridgeDepthTests|DarklightGameOverFlowTests|DarklightSustainedCombatTests)\\.");
        public static void Palette()=>Run("lit-palette","^Darklight.Tests.DarklightLitPaletteTests\\.");
        public static void Consistency()=>Run("consistency-playmode","^Darklight.Tests.(DarklightConsistencyGameplayTests|DarklightLitPaletteTests|DarklightGameOverFlowTests)\\.");
        public static void Gameplay()=>Run("gameplay-regression","^Darklight.Tests.(DarklightCampaignTests.(MovementPauseAndDodgeUseActualPhysics|AllPowersApplyGameplayAndKeepFormsIndependent)|DarklightGameplayTests.(CrowdedStrikesHitEveryTargetAndEnemyAreasCannotCrossWalls|AreaThreatRequiresEvadingAndWeaponsKeepTheirDifferentCounters)|DarklightAgeWeaponRegressionTests.*)$");
        static void Run(string name,string filter)
        {
            string output=Path.GetFullPath("outputs/validacao-pendencias-20261017/"+name+".xml");Directory.CreateDirectory(Path.GetDirectoryName(output));
            if(File.Exists(output+".summary.json"))File.Delete(output+".summary.json");
            SessionState.SetString(Key,output);DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            DarklightDeliveryTestRunner.Focused(output,filter);
        }
        [Serializable] sealed class Summary{public int passed,failed,skipped;public bool savesPreserved,prefsRestored;}
        static void Finish()
        {
            string path=SessionState.GetString(Key,"");if(path==""||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(path+".summary.json"))return;
            var result=JsonUtility.FromJson<Summary>(File.ReadAllText(path+".summary.json"));SessionState.EraseString(Key);
            EditorApplication.Exit(result.failed==0&&result.skipped==0&&result.passed>0&&result.savesPreserved&&result.prefsRestored?0:1);
        }
    }
}
