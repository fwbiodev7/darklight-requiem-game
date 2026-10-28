using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightTechnicalDeliveryGameplayTests
    {
        [UnityTest] public IEnumerator RiderSideAndBackAttacksNeverFallThroughToFrontOrAnotherForm()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.EnterTraining();lab.SetPaused(true);
            game.SelectSkin(1,false);game.Weapons.Request(3);game.Weapons.BeforeTick(.01f,true);
            var rider=game.AbyssRider;var fighter=lab.Player;var view=fighter.GetComponent<DarklightCharacterView>();
            var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();var block=new MaterialPropertyBlock();
            var formSetter=typeof(DarklightCampaign).GetMethod("SetTrainingForm",BindingFlags.Instance|BindingFlags.NonPublic);
            int samples=0;
            for(int form=0;form<4;form++)
            {
                formSetter.Invoke(game,new object[]{form});
                typeof(DarklightCampaign).GetMethod("TickFormPresentation",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{2f});
                Assert.That(game.PresentedForm,Is.EqualTo(form));
                for(int direction=2;direction<=6;direction++)
                {
                    var facing=(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);
                    int cameraView=direction>4?8-direction:direction;string expected="ActionF"+form+(cameraView==2?"W":cameraView==3?"NW":"N");
                    for(int mode=0;mode<3;mode++)
                    {
                        fighter.ResetModel();game.Blood.Set(100);
                        var input=new CombatInput{aim=facing,lightPressed=mode==0,weaponSpecialPressed=mode==1,skillPressed=mode==2};
                        rider.BeforeCombat(.001f,ref input);fighter.Model.Tick(.001f,input,rider.AttackClockSpeed(game.AttackSpeed));rider.AfterCombat();
                        Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Attack));
                        if(mode==1)Assert.That(rider.Dashing,Is.True);if(mode==2)Assert.That(rider.Slamming,Is.True);
                        for(int tick=0;tick<180&&fighter.Model.State==CombatState.Attack;tick++)
                        {
                            view.Present(fighter.Model,Vector2.zero);Assert.That(art.sprite.texture.name,Is.EqualTo(expected));
                            Assert.That(art.flipX,Is.EqualTo(direction>4));art.GetPropertyBlock(block);
                            Assert.That(block.GetFloat("_WholeFrameId"),Is.InRange(1,12));Assert.That(block.GetTexture("_VampireMask"),Is.Not.Null);samples++;
                            if(tick==12&&(direction==2||direction==4))Capture(lab,"moto-direcional-f"+form+"-d"+direction+"-modo"+mode);
                            input=new CombatInput{aim=facing};rider.BeforeCombat(.02f,ref input);fighter.Model.Tick(.02f,input,rider.AttackClockSpeed(game.AttackSpeed));rider.AfterCombat();
                        }
                        Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Neutral),"Action failed to finish: "+form+":"+direction+":"+mode);
                    }
                    fighter.ResetModel();fighter.Model.Tick(.001f,new CombatInput{aim=facing,parryPressed=true});
                    view.Present(fighter.Model,Vector2.zero);Assert.That(art.sprite.texture.name,Is.EqualTo(expected));
                    art.GetPropertyBlock(block);Assert.That(block.GetFloat("_WholeFrameId"),Is.EqualTo(12));
                    yield return null;
                }
            }
            Assert.That(samples,Is.GreaterThan(100));
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
        }
        static void Capture(DarklightCombatLab lab,string name)
        {
            const string output="outputs/finalizacao-20261018/cena-real";Directory.CreateDirectory(output);
            var camera=lab.WorldCamera;camera.GetComponent<DarklightLabCamera>().enabled=false;
            camera.transform.position=lab.Player.transform.position+new Vector3(0,1,-10);camera.orthographicSize=3.6f;
            var old=camera.targetTexture;var previous=RenderTexture.active;var target=new RenderTexture(1280,720,24);target.Create();camera.targetTexture=target;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());}
            finally{camera.targetTexture=old;RenderTexture.active=previous;target.Release();Object.Destroy(target);Object.Destroy(image);}
        }
    }
}
