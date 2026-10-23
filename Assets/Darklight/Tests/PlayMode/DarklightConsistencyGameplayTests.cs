using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightConsistencyGameplayTests
    {
        const string Output="outputs/revisao-consistencia-20261017/cena-real";
        DarklightCampaign game;DarklightCombatLab lab;DarklightCharacterView view;SpriteRenderer art;
        [UnitySetUp] public IEnumerator Setup()
        {
            if(!System.IO.File.Exists("Assets/Darklight/ArtSources/ConsistentWalk20261017/manifest.json"))Assert.Ignore("Consistency sources have not yet been imported into production.");
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.EnterTraining();lab.SetPaused(true);
            view=lab.Player.GetComponent<DarklightCharacterView>();art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            Directory.CreateDirectory(Output);lab.WorldCamera.GetComponent<DarklightLabCamera>().enabled=false;
        }
        void Capture(string name)
        {
            lab.WorldCamera.transform.position=lab.Player.transform.position+new Vector3(0,1,-10);lab.WorldCamera.orthographicSize=3.6f;
            var old=lab.WorldCamera.targetTexture;var target=new RenderTexture(1280,720,24){filterMode=FilterMode.Point};target.Create();lab.WorldCamera.targetTexture=target;
            var previous=RenderTexture.active;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{DarklightNativeCapture.Render(lab.WorldCamera,target);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Output,name+".png"),image.EncodeToPNG());}
            finally{RenderTexture.active=previous;lab.WorldCamera.targetTexture=old;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);}
        }
        [UnityTest] public IEnumerator RequiemTransitionsKeepAllSixWalkPosesEquipmentAndRendererScale()
        {
            int samples=0;
            for(int form=0;form<4;form++)for(int weapon=0;weapon<3;weapon++)for(int direction=0;direction<8;direction++)
            {
                var facing=(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);var definition=game.weaponDefinitions[weapon];
                var idle=new CombatModel(definition.loadout);idle.Tick(.001f,new CombatInput{aim=facing});
                view.PresentPreview(idle,Vector2.zero,0,0,form,definition);var idleSprite=art.sprite;
                if(direction==0)Capture("requiem-f"+form+"-w"+weapon+"-parado");
                var frames=new HashSet<string>();
                for(int phase=0;phase<6;phase++)
                {
                    var model=new CombatModel(definition.loadout);model.Tick((phase+.2f)*.4f/6,new CombatInput{aim=facing,move=facing});
                    view.PresentPreview(model,facing,0,0,form,definition);frames.Add(art.sprite.name);
                    Assert.That(art.sprite.name,Does.Contain("WalkConsistent"));
                    Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                    Assert.That(art.sprite.bounds.size.y/idleSprite.bounds.size.y,Is.InRange(.72f,1.28f));samples++;
                    if(direction==0&&phase==0)Capture("requiem-f"+form+"-w"+weapon+"-passo");
                }
                Assert.That(frames.Count,Is.EqualTo(6));
                var attack=new CombatModel(definition.loadout);attack.Tick(.001f,new CombatInput{aim=facing,lightPressed=true});attack.Tick(attack.Attack.startup+.001f,new CombatInput{aim=facing});
                view.PresentPreview(attack,Vector2.zero,0,0,form,definition);Assert.That(art.sprite,Is.Not.Null);
                if(direction==0)Capture("requiem-f"+form+"-w"+weapon+"-ataque");
                view.PresentPreview(idle,Vector2.zero,0,0,form,definition);Assert.That(art.sprite,Is.SameAs(idleSprite));
                if(direction%4==0&&weapon==0&&form==3)Capture("cabelo-branco-direcao-"+direction);
                yield return null;
            }
            Assert.That(samples,Is.EqualTo(576));
        }
        [UnityTest] public IEnumerator RealRiderAttacksUseCorrectedWholeFiguresAcrossFormsAndFacings()
        {
            game.SelectSkin(1,false);var fighter=lab.Player;var rider=game.AbyssRider;
            fighter.ResetModel();game.Weapons.Request(3);game.Weapons.BeforeTick(.01f,true);Assert.That(rider.Active,Is.True);
            var block=new MaterialPropertyBlock();
            for(int form=0;form<4;form++)
            {
                typeof(DarklightCampaign).GetMethod("SetTrainingForm",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{form});
                typeof(DarklightCampaign).GetMethod("TickFormPresentation",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{2f});
                Assert.That(game.PresentedForm,Is.EqualTo(form));
                for(int direction=0;direction<8;direction++)
                {
                    fighter.ResetModel();var facing=(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);
                    var input=new CombatInput{aim=facing,lightPressed=true};rider.BeforeCombat(.001f,ref input);fighter.Model.Tick(.001f,input,rider.Speed);rider.AfterCombat();
                    Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Attack));
                    for(int step=0;step<50&&fighter.Model.State==CombatState.Attack;step++)
                    {
                        view.Present(fighter.Model,Vector2.zero);Assert.That(art.sprite,Is.Not.Null);
                        Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                        if(direction==0||direction==1||direction==7)
                        {
                            art.GetPropertyBlock(block);Assert.That(block.GetFloat("_WholeFrameId"),Is.InRange(1,6));
                            Assert.That(art.sprite.rect.height/art.sprite.pixelsPerUnit,Is.LessThan(2.7f));
                        }
                        if(step==12&&direction<=1)Capture("moto-f"+form+"-d"+direction+"-ataque");
                        input=new CombatInput{aim=facing};rider.BeforeCombat(.02f,ref input);fighter.Model.Tick(.02f,input,rider.Speed);rider.AfterCombat();
                    }
                }
                yield return null;
            }
        }
    }
}
