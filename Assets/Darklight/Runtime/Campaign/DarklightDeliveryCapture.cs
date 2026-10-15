using System.Collections;
using System.IO;
using UnityEngine;
namespace Darklight
{
    // Explicit delivery preview; absent this switch the player's campaign is untouched.
    public sealed partial class DarklightDeliveryCapture : MonoBehaviour
    {
        // Capture the actual world and Canvas without depending on a visible Windows swap chain.
        void Capture(string path)=>DarklightNativeCapture.Screen(path);
        IEnumerator Start()
        {
            string[] args=System.Environment.GetCommandLineArgs();
            int headFix=System.Array.IndexOf(args,"--requiem-head-fix-capture");
            if(headFix>=0&&headFix+1<args.Length){yield return CaptureHeadFix(args[headFix+1]);yield break;}
            int userSheets=System.Array.IndexOf(args,"--requiem-user-sheets-capture");
            if(userSheets>=0&&userSheets+1<args.Length){yield return CaptureUserSheets(args[userSheets+1]);yield break;}
            bool upgrade=System.Array.IndexOf(args,"--requiem-upgrade-capture")>=0;
            bool maps=upgrade||System.Array.IndexOf(args,"--requiem-map-capture")>=0;
            int index=System.Array.IndexOf(args,upgrade?"--requiem-upgrade-capture":maps?"--requiem-map-capture":"--requiem-capture");
            if(index<0||index+1>=args.Length)yield break;string path=args[index+1];Directory.CreateDirectory(path);
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
            yield return new WaitForSecondsRealtime(.6f);yield return new WaitForEndOfFrame();
            Capture(Path.Combine(path,"menu.png"));yield return new WaitForSecondsRealtime(.4f);
            // Preview starts a fresh in-memory game without writing to the user's save slot.
            for(int chapter=0;chapter<(maps?6:1);chapter++)
            {
                DarklightCampaign.Instance.PreviewChapter(chapter);
                yield return new WaitForSecondsRealtime(.8f);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(path,maps?"capitulo-"+(chapter+1)+".png":"vespera.png"));yield return new WaitForSecondsRealtime(.4f);
            }
            if(upgrade)
            {
                var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
                game.PreviewChapter(0);yield return new WaitForSecondsRealtime(.4f);
                var body=lab.Player.GetComponent<Rigidbody2D>();body.position=game.Map.Arenas[0]+Vector2.left*2.5f;Physics2D.SyncTransforms();
                yield return new WaitForSecondsRealtime(1.2f);
                var enemy=Object.FindAnyObjectByType<DarklightEnemy>();
                if(enemy!=null){body.position=(Vector2)enemy.transform.position-Vector2.right;Physics2D.SyncTransforms();yield return new WaitForSecondsRealtime(.6f);lab.Player.Model.Tick(.01f,new CombatInput{lightPressed=true,aim=Vector2.right});}
                yield return new WaitForSecondsRealtime(.075f);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(path,"combate.png"));yield return new WaitForSecondsRealtime(.4f);
                for(int mode=0;mode<3;mode++)
                {
                    game.PreviewChapter(0);yield return null;game.Progress.mp3Unlocked=true;game.Resonance.Select(mode);lab.Player.GetComponent<HealthSystem>().TakeDamage(25);game.TryActivatePower();
                    yield return new WaitForSecondsRealtime(.45f);yield return new WaitForEndOfFrame();
                    Capture(Path.Combine(path,"poder-"+mode+".png"));yield return new WaitForSecondsRealtime(.35f);
                }
                game.PreviewChapter(0);yield return null;game.Progress.mp3Unlocked=true;game.PreviewPanel("MP3");
                yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(path,"mp3.png"));yield return new WaitForSecondsRealtime(.35f);
                game.PreviewPanel("Options");yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(path,"opcoes.png"));yield return new WaitForSecondsRealtime(.35f);
                // Export each existing screen for the artist, using only a disposable in-memory campaign.
                game.NewGame();yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(path,"menu-Dialogue.png"));
                game.Progress.mp3Unlocked=true;game.Progress.points=3;
                foreach(string screen in new[]{"Main","ConfirmNew","Pause","Skills","Inventory","Journal","MP3","Options","Controls","BindingDevice","Death","Ending","ChapterSelect","Credits"})
                {
                    game.PreviewPanel(screen);yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                    Capture(Path.Combine(path,"menu-"+screen+".png"));
                }
                game.PreviewPanel("Controls",10);yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(path,"menu-Controls-rolagem.png"));
                for(int ending=1;ending<=2;ending++)
                {
                    game.Progress.ending=ending;game.PreviewPanel("Victory");yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                    Capture(Path.Combine(path,"menu-Victory-"+ending+".png"));
                }
                game.Progress.mp3Unlocked=false;game.PreviewPanel("MP3");yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(path,"menu-MP3-bloqueado.png"));
            }
            File.WriteAllText(Path.Combine(path,"abertura.txt"),"Product="+Application.productName+"\nScene="+gameObject.scene.name+"\nHUD="+(DarklightCombatLab.Instance.PixelHUD!=null));Application.Quit();
        }
    }
}

