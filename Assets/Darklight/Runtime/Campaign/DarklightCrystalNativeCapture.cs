using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightDeliveryCapture
    {
        [Serializable] sealed class CrystalNativeResult
        {
            public bool success,savesPreserved,silent,returnedToNormal;
            public string version,exception,gpu;public int maps,forms,combatFrames,attacks;
            public float combatMedianMs,combatP95Ms;public string environment="Standalone Windows, offscreen batch graphics, not interactive display FPS";
        }
        IEnumerator CaptureCrystalsNative(string folder)
        {
            Directory.CreateDirectory(folder);DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            var result=new CrystalNativeResult{version=Application.version,gpu=SystemInfo.graphicsDeviceName};
            var saves=new Dictionary<string,byte[]>();
            if(Directory.Exists(Application.persistentDataPath))foreach(string path in Directory.GetFiles(Application.persistentDataPath,"*.json*",SearchOption.AllDirectories))saves[path]=File.ReadAllBytes(path);
            Application.LogCallback log=(message,stack,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)result.exception=(result.exception??"")+message+"\n";};
            Application.logMessageReceived+=log;
            try
            {
                // An offscreen batch player may never present the splash frame.
                // Manual capture must not wait indefinitely for a visible swap.
                float splashDeadline=Time.realtimeSinceStartup+3;
                while(!UnityEngine.Rendering.SplashScreen.isFinished&&Time.realtimeSinceStartup<splashDeadline)yield return null;
                yield return null;yield return null;Capture(Path.Combine(folder,"menu.png"));
                var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
                for(int chapter=0;chapter<6;chapter++)
                {
                    game.PreviewChapter(chapter);yield return new WaitForSecondsRealtime(.35f);
                    Capture(Path.Combine(folder,"capitulo-"+(chapter+1)+".png"));result.maps++;
                }
                game.PreviewChapter(0);yield return new WaitForSecondsRealtime(.25f);
                for(int skin=0;skin<2;skin++)for(int form=0;form<4;form++)
                {
                    game.SelectSkin(skin,false);
                    if(game.Form>0){game.TryTransformPlayer();while(game.Transforming)yield return null;}
                    if(form>0)
                    {
                        game.Progress.preferredForm=form;game.Blood.Set(100);yield return null;
                        if(!game.TryTransformPlayer())throw new InvalidOperationException("Native transformation failed: "+skin+":"+form);
                        float deadline=Time.realtimeSinceStartup+5;while(game.Transforming&&Time.realtimeSinceStartup<deadline)yield return null;
                        if(game.Form!=form||game.Transforming)throw new InvalidOperationException("Incomplete native transformation");
                    }
                    yield return null;Capture(Path.Combine(folder,"skin-"+skin+"-forma-"+form+".png"));result.forms++;
                }
                // Real defeat while transformed, followed by the production return transition.
                lab.Player.GetComponent<HealthSystem>().SetInvincibilityDuration(0);lab.Player.GetComponent<HealthSystem>().TakeDamage(100000);
                yield return new WaitForSecondsRealtime(DarklightDeathReversion.Duration+.25f);Capture(Path.Combine(folder,"game-over.png"));
                game.ResumeLastReturnPoint();yield return new WaitForSecondsRealtime(1.1f);
                result.returnedToNormal=game.Form==0&&!lab.Player.GetComponent<HealthSystem>().IsDead&&!lab.Paused;
                Capture(Path.Combine(folder,"retorno-normal.png"));
                var spawn=typeof(DarklightCampaign).GetMethod("Spawn",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                for(int i=0;i<8;i++)spawn.Invoke(game,new object[]{(RequiemEnemyKind)(i%3),game.Map.SafeSpawn((Vector2)lab.Player.transform.position+new Vector2(Mathf.Cos(i),Mathf.Sin(i))),false});
                lab.Player.GetComponent<HealthSystem>().SetMaxHealth(20000,false);
                var times=new List<float>();float started=Time.realtimeSinceStartup,nextAttack=started;
                while(Time.realtimeSinceStartup-started<6)
                {
                    if(Time.realtimeSinceStartup>=nextAttack){nextAttack+=.4f;lab.Player.Model.Tick(.001f,new CombatInput{lightPressed=true,aim=Vector2.down});}
                    yield return null;if(Time.realtimeSinceStartup-started>.75f)times.Add(Time.unscaledDeltaTime*1000);
                }
                times.Sort();result.combatFrames=times.Count;result.attacks=lab.Player.Model.AttackSerial;
                if(times.Count>0){result.combatMedianMs=times[times.Count/2];result.combatP95Ms=times[Math.Min(times.Count-1,(int)(times.Count*.95f))];}
                Capture(Path.Combine(folder,"combate.png"));
                result.silent=!FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Any(source=>source.isPlaying);
                result.savesPreserved=saves.All(pair=>File.Exists(pair.Key)&&File.ReadAllBytes(pair.Key).SequenceEqual(pair.Value))&&(!Directory.Exists(Application.persistentDataPath)||Directory.GetFiles(Application.persistentDataPath,"*.json*",SearchOption.AllDirectories).Length==saves.Count);
                result.success=result.maps==6&&result.forms==8&&result.returnedToNormal&&result.silent&&result.savesPreserved&&result.combatFrames>10&&result.attacks>0&&string.IsNullOrEmpty(result.exception);
            }
            finally
            {
                Application.logMessageReceived-=log;File.WriteAllText(Path.Combine(folder,"verificacao.json"),JsonUtility.ToJson(result,true));Application.Quit(result.success?0:3);
            }
        }
    }
}
