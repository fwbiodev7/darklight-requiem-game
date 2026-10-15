using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightFinalCaptureTests
    {
        [Serializable] sealed class Proof {public string sheet,sprite,state;public int skin,weapon,age,form,direction,pose;public bool today,whole;public float ppu,pivotX,pivotY,worldHeight;}
        [Serializable] sealed class Results {public string status;public int sheets,renders,todayRenders;public double milliseconds;public List<Proof> captures=new();}
        const int Width=240,Height=240;
        static readonly string[] Poses={"idle","walk","windup","contact","parry","dodge","hurt","dead"};
        [UnityTest] public IEnumerator WholeCharacterMatrixRendersBothSkinsAllWeaponsAgesDirectionsAndForms()
        {
            string folder=PlayerPrefs.GetString("Darklight.Tests.CaptureOutput","");Assert.That(folder,Is.Not.Empty,"Use the protected Capture runner");Directory.CreateDirectory(folder);
            DarklightAutomation.Silent=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(4);yield return null;
            game.Progress.weaponUnlocks=7;lab.SetPaused(true);game.enabled=false;lab.Player.enabled=false;
            var view=lab.Player.GetComponent<DarklightCharacterView>();var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var originals=Resources.Load<DarklightOriginalVampireCatalog>("Darklight/OriginalVampireCatalog");
            var obj=new GameObject("ProtectedFinalProofCamera");var camera=obj.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4.5f;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<30;camera.allowHDR=false;camera.allowMSAA=false;
            var target=new RenderTexture(Width,Height,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();camera.targetTexture=target;
            var image=new Texture2D(Width,Height,TextureFormat.RGBA32,false);var result=new Results{status="RUNNING"};var watch=Stopwatch.StartNew();
            var block=new MaterialPropertyBlock();var previous=RenderTexture.active;
            try
            {
                for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int age=0;age<1;age++)
                {
                    for(int form=0;form<4;form++)
                    {
                        string sheet=$"{(skin==0?"Requiem":"Rock")}-jovem-arma-{weapon}-forma-{form}.png";
                        var contact=new Texture2D(Width*8,Height*8,TextureFormat.RGBA32,false);
                        try
                        {
                            for(int pose=0;pose<8;pose++)for(int direction=0;direction<8;direction++)
                            {
                                var definition=game.weaponDefinitions[weapon];var model=new CombatModel(definition.loadout);
                                Vector2 facing=Quaternion.Euler(0,0,-direction*45)*Vector2.down,move=pose==1?facing:Vector2.zero;
                                model.Tick(.001f,new CombatInput{aim=facing,move=move,lightPressed=pose==2||pose==3,parryPressed=pose==4,dodgePressed=pose==5});
                                model.Tick(pose==3?model.Attack.startup+.02f:pose==5?.06f:.04f,new CombatInput{aim=facing,move=move});
                                if(pose==6)model.Hurt();if(pose==7)model.Hurt(true);
                                var state=model.State;var clock=model.Clock;
                                view.PresentPreview(model,move,skin,age,form,definition);
                                var layers=view.GetComponent<DarklightOriginalVampireView>();art.GetPropertyBlock(block);
                                bool today=TodayCharacterBank.Mask(art.sprite)!=null;
                                bool whole=today||DarklightUserSpriteBank.Mask(art.sprite)!=null||DarklightWholeCharacterBank.Mask(art.sprite)!=null||originals.Find(art.sprite)!=null;
                                Assert.That(whole,Is.True,"Unknown whole frame: "+art.sprite.name);Assert.That(game.Appearance.Stage,Is.EqualTo(age));
                                Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                                Assert.That(layers.HasAgeHead,Is.False);Assert.That(layers.PresentedHead,Is.Null);Assert.That(block.GetFloat("_EraseHead"),Is.Zero);
                                Assert.That(model.State,Is.EqualTo(state));Assert.That(model.Clock,Is.EqualTo(clock));
                                Assert.That(block.GetFloat("_VampireForm"),Is.EqualTo(form));
                                if(form>0&&pose!=7)Assert.That(layers.PresentedAura,Is.Not.Null);else Assert.That(layers.PresentedAura,Is.Null);
                                var renderers=lab.Player.GetComponentsInChildren<Renderer>(true);var oldLayers=renderers.Select(r=>r.gameObject.layer).ToArray();
                                camera.transform.position=lab.Player.transform.position+new Vector3(0,1.5f,-10);
                                try
                                {
                                    foreach(var r in renderers)r.gameObject.layer=30;
                                    camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,Width,Height),0,0);image.Apply();
                                    var pixels=image.GetPixels32();int visible=0;
                                    for(int y=0;y<Height;y++)for(int x=0;x<Width;x++)if(pixels[y*Width+x].a>25)
                                    {visible++;Assert.That(x,Is.InRange(2,Width-3),"Render cut at horizontal edge: "+sheet+"/"+direction+"/"+Poses[pose]);Assert.That(y,Is.InRange(2,Height-3),"Render cut at vertical edge: "+sheet+"/"+direction+"/"+Poses[pose]);}
                                    Assert.That(visible,Is.GreaterThan(100),"Empty real render");
                                    contact.SetPixels32(direction*Width,(7-pose)*Height,Width,Height,pixels);
                                }
                                finally{for(int i=0;i<renderers.Length;i++)renderers[i].gameObject.layer=oldLayers[i];RenderTexture.active=previous;}
                                result.captures.Add(new Proof{sheet=sheet,sprite=art.sprite.name,state=model.State.ToString(),skin=skin,weapon=weapon,age=age,form=form,direction=direction,pose=pose,today=today,whole=whole,ppu=art.sprite.pixelsPerUnit,pivotX=art.sprite.pivot.x,pivotY=art.sprite.pivot.y,worldHeight=art.bounds.size.y});
                                result.renders++;if(today)result.todayRenders++;
                            }
                            contact.Apply();File.WriteAllBytes(Path.Combine(folder,sheet),contact.EncodeToPNG());result.sheets++;
                        }
                        finally{UnityEngine.Object.Destroy(contact);}
                        yield return null;
                    }
                    DarklightSpriteExpectations.ClearTodayCache();yield return Resources.UnloadUnusedAssets();
                }
                result.status="SUCCESS";Assert.That(result.renders,Is.EqualTo(1536));Assert.That(result.sheets,Is.EqualTo(24));
                foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
            }
            finally
            {
                watch.Stop();result.milliseconds=watch.Elapsed.TotalMilliseconds;if(result.status!="SUCCESS")result.status="FAILED";
                File.WriteAllText(Path.Combine(folder,"verificacao.json"),JsonUtility.ToJson(result,true));
                File.WriteAllText(Path.Combine(folder,"LEIA-ME.txt"),"Real Unity renders. Columns: directions0..7. Rows(top to bottom): "+string.Join(", ",Poses)+". Files identify skin, fixed young appearance, weapon0..2 and form0..3. Today=true is newly authored art; other recognized complete sprites are preserved fallbacks. Appearance preview does not advance narrative age. No manual campaign or physical controller trial implied.");
                RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(obj);
                DarklightAutomation.Silent=true;AudioListener.volume=0;
            }
        }
    }
}
