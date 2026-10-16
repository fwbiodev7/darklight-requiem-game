using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        internal void FreezeUserSheetsWorld()
        {if(!PlaytestSession)throw new InvalidOperationException("Protected session required");Close();lab.SetPaused(true);enabled=false;}
        internal void PrepareUserSheetsPose(int skin,int age,int form,int weapon)
        {
            if(!PlaytestSession)throw new InvalidOperationException("Save-protected capture required");
            StopPower();SelectSkin(skin,false);Progress.equippedWeapon=weapon;Weapons.Restore();
            Appearance.PreviewStage(age);Form=form;FormTime=60;formPresentationTime=0;lab.Player.ResetModel();TickMinimalHUD();TickPlaytestGuide();playtestGuideRoot.gameObject.SetActive(false);noticeTime=0;messageText.text="";
        }
    }
    public sealed partial class DarklightDeliveryCapture
    {
        [Serializable] class UserSheetCaptureRecord
        {public string file,sprite,state;public int skin,weapon,age,pose,form;public bool whole,headEnabled,flipped;public float eraseHead;}
        [Serializable] class UserSheetCaptureResults {public string status;public List<UserSheetCaptureRecord> captures=new();}
        IEnumerator CaptureUserSheets(string folder)
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--requiem-playtest")<0){Application.Quit(2);yield break;}
            Directory.CreateDirectory(folder);DarklightAutomation.Silent=true;AudioListener.pause=true;
            while(!UnityEngine.Rendering.SplashScreen.isFinished||DarklightCampaign.Instance?.PlaytestSession!=true)yield return null;
            yield return new WaitForSecondsRealtime(.6f);
            var game=DarklightCampaign.Instance;game.PreviewChapter(0);yield return null;game.Progress.mp3Unlocked=true;game.Progress.weaponUnlocks=7;var lab=DarklightCombatLab.Instance;var fighter=lab.Player;
            game.FreezeUserSheetsWorld();fighter.enabled=false;
            var view=fighter.GetComponent<DarklightCharacterView>();var body=fighter.GetComponent<Rigidbody2D>();var display=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            body.position=game.Map.SafeSpawn(game.Map.Arenas[0]+Vector2.down*2.5f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
            var position=lab.WorldCamera.transform.position;lab.WorldCamera.transform.position=new Vector3(body.position.x,body.position.y,position.z);
            var results=new UserSheetCaptureResults{status="RUNNING"};var propertyBlock=new MaterialPropertyBlock();
            string[] skins={"Requiem","Rock"},weapons={"katana","adagas","corrente"},poses={"passo-1","passo-2","ataque-preparo","ataque-contato","defesa","esquiva-inicio","esquiva-final","repouso"};
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int pose=0;pose<8;pose++)
            {
                game.PrepareUserSheetsPose(skin,0,0,weapon);
                foreach(var echo in UnityEngine.Object.FindObjectsByType<DarklightCombatFX>())UnityEngine.Object.Destroy(echo.gameObject);
                var m=fighter.Model;var move=pose<2?Vector2.down:Vector2.zero;
                m.Tick(.001f,new CombatInput{aim=Vector2.right,move=move,lightPressed=pose==2||pose==3,parryPressed=pose==4,dodgePressed=pose==5||pose==6});
                float delta=pose==0?.02f:pose==1?.15f:pose==2?.01f:pose==3?m.Attack.startup+.02f:pose==5?.05f:pose==6?.17f:.04f;
                m.Tick(delta,new CombatInput{aim=Vector2.right,move=move});view.Present(m,move);yield return new WaitForEndOfFrame();
                string file=$"{skins[skin]}-{weapons[weapon]}-{poses[pose]}.png";Capture(Path.Combine(folder,file));
                display.GetPropertyBlock(propertyBlock);var layers=view.GetComponent<DarklightOriginalVampireView>();
                bool valid=view.UsesUserWholeSprite&&display.sprite==DarklightUserSpriteBank.Page(skin,weapon)[pose]&&!layers.HasAgeHead&&propertyBlock.GetFloat("_EraseHead")==0;
                results.captures.Add(new UserSheetCaptureRecord{file=file,sprite=display.sprite.name,state=m.State.ToString(),skin=skin,weapon=weapon,age=game.Appearance.Stage,pose=pose,whole=view.UsesUserWholeSprite,headEnabled=layers.HasAgeHead,flipped=display.flipX,eraseHead=propertyBlock.GetFloat("_EraseHead")});
                if(!valid){results.status="FAILED: wrong pose or separate head: "+file;File.WriteAllText(Path.Combine(folder,"verificacao.json"),JsonUtility.ToJson(results,true));Application.Quit(3);yield break;}
            }
            // Both horizontal facings use the same supplied full sprite; age data remains intact.
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)
            {
                game.PrepareUserSheetsPose(skin,0,0,weapon);fighter.Model.Tick(.02f,new CombatInput{aim=Vector2.left});view.Present(fighter.Model,Vector2.zero);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(folder,$"{skins[skin]}-{weapons[weapon]}-esquerda.png"));if(!display.flipX){Application.Quit(4);yield break;}
            }
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int form=1;form<=3;form++)for(int action=0;action<4;action++)
            {
                game.PrepareUserSheetsPose(skin,0,form,weapon);
                foreach(var echo in UnityEngine.Object.FindObjectsByType<DarklightCombatFX>())UnityEngine.Object.Destroy(echo.gameObject);
                var m=fighter.Model;var move=action==0?Vector2.down:Vector2.zero;
                m.Tick(.001f,new CombatInput{aim=Vector2.right,move=move,lightPressed=action==1,parryPressed=action==2,dodgePressed=action==3});
                m.Tick(action==1?m.Attack.startup+.02f:action==3?.06f:.04f,new CombatInput{aim=Vector2.right,move=move});view.Present(m,move);yield return new WaitForEndOfFrame();
                string file=$"{skins[skin]}-{weapons[weapon]}-forma-{form}-{new[]{"caminhar","ataque","defesa","esquiva"}[action]}.png";Capture(Path.Combine(folder,file));display.GetPropertyBlock(propertyBlock);
                var layers=view.GetComponent<DarklightOriginalVampireView>();
                results.captures.Add(new UserSheetCaptureRecord{file=file,sprite=display.sprite.name,state=m.State.ToString(),skin=skin,weapon=weapon,age=0,pose=DarklightUserSpriteBank.Pose(m,move),form=form,whole=view.UsesUserWholeSprite,headEnabled=layers.HasAgeHead,flipped=display.flipX,eraseHead=propertyBlock.GetFloat("_EraseHead")});
                if(!view.UsesUserWholeSprite||layers.HasAgeHead||layers.PresentedAura==null||propertyBlock.GetFloat("_VampireForm")!=form||propertyBlock.GetFloat("_EraseHead")!=0)
                {results.status="FAILED: whole transformation: "+file;File.WriteAllText(Path.Combine(folder,"verificacao.json"),JsonUtility.ToJson(results,true));Application.Quit(6);yield break;}
            }
            for(int age=1;age<=4;age++)
            {
                game.PrepareUserSheetsPose(age%2,age,0,0);fighter.Model.Tick(.02f,new CombatInput{aim=Vector2.down});view.Present(fighter.Model,Vector2.zero);yield return new WaitForEndOfFrame();
                Capture(Path.Combine(folder,$"idade-preservada-{age}.png"));if(view.UsesUserWholeSprite||game.Appearance.Stage!=age){Application.Quit(5);yield break;}
            }
            results.status="SUCCESS: 48 base poses, 72 whole transformation captures, 6 left facings, 4 preserved age stages; no head clipping; protected saves";
            File.WriteAllText(Path.Combine(folder,"verificacao.json"),JsonUtility.ToJson(results,true));Debug.Log(results.status);Application.Quit(0);
        }
    }
}
