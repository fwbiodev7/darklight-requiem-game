using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightDeliveryCapture
    {
        [Serializable] class HeadProof {public string file,sprite;public int skin,age,weapon,direction,form,action;public bool whole,separateHead,fallback;public float erase;}
        [Serializable] class HeadProofs {public string status;public List<HeadProof> captures=new();}
        IEnumerator CaptureHeadFix(string folder)
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--requiem-playtest")<0){Application.Quit(2);yield break;}
            Directory.CreateDirectory(folder);DarklightAutomation.Silent=true;AudioListener.pause=true;
            while(!UnityEngine.Rendering.SplashScreen.isFinished||DarklightCampaign.Instance?.PlaytestSession!=true)yield return null;
            yield return new WaitForSecondsRealtime(.5f);
            var game=DarklightCampaign.Instance;game.PreviewChapter(4);yield return null;game.Progress.weaponUnlocks=7;
            var lab=DarklightCombatLab.Instance;var fighter=lab.Player;game.FreezeUserSheetsWorld();fighter.enabled=false;
            var view=fighter.GetComponent<DarklightCharacterView>();var body=fighter.GetComponent<Rigidbody2D>();var display=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            body.position=game.Map.SafeSpawn(game.Map.Spawn);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();var cp=lab.WorldCamera.transform.position;lab.WorldCamera.transform.position=new Vector3(body.position.x,body.position.y,cp.z);
            var result=new HeadProofs{status="RUNNING"};var block=new MaterialPropertyBlock();var catalog=Resources.Load<DarklightOriginalVampireCatalog>("Darklight/OriginalVampireCatalog");
            for(int age=1;age<=4;age++)for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int d=0;d<8;d++)for(int action=0;action<2;action++)for(int form=0;form<=(age==3?3:0);form++)
            {
                game.PrepareUserSheetsPose(skin,age,form,weapon);
                foreach(var echo in UnityEngine.Object.FindObjectsByType<DarklightCombatFX>())UnityEngine.Object.Destroy(echo.gameObject);
                Vector2 aim=Quaternion.Euler(0,0,-d*45)*Vector2.down;var m=fighter.Model;var move=action==0?aim:Vector2.zero;
                m.Tick(.001f,new CombatInput{aim=aim,move=move,lightPressed=action==1});m.Tick(action==1?m.Attack.startup+.02f:.15f,new CombatInput{aim=aim,move=move});view.Present(m,move);yield return new WaitForEndOfFrame();
                var layers=view.GetComponent<DarklightOriginalVampireView>();display.GetPropertyBlock(block);
                string file=$"{(skin==0?"Requiem":"Rock")}-idade-{age}-arma-{weapon}-direcao-{d}-{(action==0?"caminhar":"ataque")}-forma-{form}.png";
                Capture(Path.Combine(folder,file));bool whole=TodayCharacterBank.Mask(display.sprite)!=null||DarklightWholeCharacterBank.Mask(display.sprite)!=null||catalog.Find(display.sprite)!=null;
                result.captures.Add(new HeadProof{file=file,sprite=display.sprite.name,skin=skin,age=age,weapon=weapon,direction=d,form=form,action=action,whole=whole,fallback=view.UsesAgePoseFallback,separateHead=layers.HasAgeHead,erase=block.GetFloat("_EraseHead")});
                if(!whole||layers.HasAgeHead||display.transform.Find("IdadeOriginal")!=null||block.GetFloat("_EraseHead")!=0)
                {result.status="FAILED: "+file;File.WriteAllText(Path.Combine(folder,"verificacao.json"),JsonUtility.ToJson(result,true));Application.Quit(7);yield break;}
            }
            result.status="SUCCESS: "+result.captures.Count+" complete sprites across both skins, all ages, weapons and directions; no separate heads or erasure";
            File.WriteAllText(Path.Combine(folder,"verificacao.json"),JsonUtility.ToJson(result,true));Debug.Log(result.status);Application.Quit(0);
        }
    }
}

