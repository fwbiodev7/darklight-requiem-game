using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightSpritePlaybackTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        [UnitySetUp]public IEnumerator Setup(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;}
        [UnityTest]public IEnumerator BothSkinsAndThreeWeaponsKeepRequestedSizeThroughEveryFacingAndCombatState()
        {
            var view=lab.Player.GetComponent<DarklightCharacterView>();var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var appearance=lab.Player.GetComponent<CharacterAppearanceSystem>();game.Progress.weaponUnlocks=7;
            int checkedPoses=0;
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int d=0;d<8;d++)
            {
                game.SelectSkin(skin,false);game.Progress.equippedWeapon=weapon;game.Weapons.Restore();
                Vector2 facing=Quaternion.Euler(0,0,-d*45)*Vector2.down;
                for(int stage=0;stage<5;stage++)for(int state=0;state<6;state++)
                {
                    appearance.PreviewStage(stage);var model=new CombatModel(game.Weapons.Equipped.loadout);model.Tick(.01f,new CombatInput{aim=facing});
                    var input=new CombatInput{aim=facing,move=state==1||state==3?facing:Vector2.zero,lightPressed=state==2,dodgePressed=state==3,parryPressed=state==4};model.Tick(.04f,input);if(state==5)model.Hurt();
                    view.Present(model,input.move);Assert.That(art.sprite,Is.Not.Null);Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f),$"skin {skin}, weapon {weapon}, facing {d}, state {state}");
                    foreach(var renderer in lab.Player.GetComponentsInChildren<SpriteRenderer>())if(renderer.enabled)Assert.That(renderer.sprite,Is.Not.Null,renderer.name);
                    Assert.That(art.transform.Find("Aparencia_Cabelo"),Is.Null);Assert.That(art.transform.Find("Aparencia_Ombros"),Is.Null);Assert.That(art.GetComponentsInChildren<SpriteRenderer>(true).Length,Is.EqualTo(2),"Only the complete body and approved aura may exist");
                    Assert.That(view.GetComponent<DarklightOriginalVampireView>().HasAgeHead,Is.False,"No age, weapon or facing may render a separate head");Assert.That(art.transform.Find("IdadeOriginal"),Is.Null);
                    var ageBlock=new MaterialPropertyBlock();art.GetPropertyBlock(ageBlock);Assert.That(ageBlock.GetFloat("_Physique"),Is.Zero,"Procedural physique must not deform authored bodies");
                    checkedPoses++;
                }
            }
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
            Assert.That(checkedPoses,Is.EqualTo(1440));yield return null;
        }
        [UnityTest]public IEnumerator MenuOwnsRequestedLoopAndSilentPreviewNeverPlaysIt()
        {
            game.PreviewPanel("Main");Assert.That(game.MenuMusicSource.clip,Is.EqualTo(game.menuMusic));Assert.That(game.menuMusic.name,Is.EqualTo("metal-in-the-night-def-lev"));Assert.That(game.MenuMusicSource.loop,Is.True);Assert.That(game.MenuMusicSource.playOnAwake,Is.False);Assert.That(game.MenuMusicSource.isPlaying,Is.False);
            game.PreviewPanel("Options");Assert.That(game.MenuMusicSource.clip,Is.EqualTo(game.menuMusic));game.PreviewCampaign();Assert.That(game.MenuMusicSource.isPlaying,Is.False);yield return null;
        }
        [UnityTest]public IEnumerator AllFormsUseRepairedFramesAcrossBothSkinsAndThreeWeapons()
        {
            game.PreviewCampaign();yield return null;game.Progress.weaponUnlocks=7;game.Progress.formsUnlocked=3;
            var transformMethod=typeof(DarklightCampaign).GetMethod("TransformPlayer",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            var view=lab.Player.GetComponent<DarklightCharacterView>();var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int form=1;form<=3;form++)
            {
                game.SelectSkin(skin,false);game.Progress.equippedWeapon=weapon;game.Weapons.Restore();game.Progress.preferredForm=form;
                game.Blood.Set(100);transformMethod.Invoke(game,null);Assert.That(game.Form,Is.EqualTo(form));
                typeof(DarklightCampaign).GetMethod("TickFormPresentation",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(game,new object[]{.7f});
                for(int d=0;d<8;d++)
                {
                    var model=new CombatModel(game.Weapons.Equipped.loadout);Vector2 facing=Quaternion.Euler(0,0,-d*45)*Vector2.down;model.Tick(.01f,new CombatInput{aim=facing});view.Present(model,Vector2.zero);
                    Assert.That(art.sprite,Is.Not.Null);Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f));
                    model.Tick(.05f,new CombatInput{aim=facing,lightPressed=true});view.Present(model,Vector2.zero);Assert.That(art.sprite,Is.Not.Null);Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f));
                }
                transformMethod.Invoke(game,null);Assert.That(game.Form,Is.EqualTo(0));
                typeof(DarklightCampaign).GetMethod("TickFormPresentation",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(game,new object[]{.4f});
            }
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);yield return null;
        }
        [UnityTest]public IEnumerator CaptureNorthWalkAfterRepairedSpriteSelection()
        {
            game.PreviewCampaign();game.SelectSkin(1,false);var view=lab.Player.GetComponent<DarklightCharacterView>();var model=new CombatModel(lab.Player.loadout);model.Tick(.1f,new CombatInput{move=Vector2.up,aim=Vector2.up});view.Present(model,Vector2.up);
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/correcoes-sprites"));Directory.CreateDirectory(output);
            var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();var obj=new GameObject("SilentSpriteProofCamera");var camera=obj.AddComponent<Camera>();
            var target=new RenderTexture(192,192,24){filterMode=FilterMode.Point};target.Create();camera.targetTexture=target;camera.orthographic=true;camera.orthographicSize=1.7f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.09f,.12f);camera.cullingMask=1<<30;
            var renderers=lab.Player.GetComponentsInChildren<SpriteRenderer>();var oldLayers=new int[renderers.Length];for(int i=0;i<renderers.Length;i++){oldLayers[i]=renderers[i].gameObject.layer;renderers[i].gameObject.layer=30;}
            camera.transform.position=art.transform.position+new Vector3(0,1,-10);camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(192,192,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,"fabio-costas-corrigido.png"),image.EncodeToPNG());RenderTexture.active=old;
            for(int i=0;i<renderers.Length;i++)renderers[i].gameObject.layer=oldLayers[i];camera.targetTexture=null;target.Release();Object.Destroy(target);Object.Destroy(image);Object.Destroy(obj);yield return null;
        }
    }
}



