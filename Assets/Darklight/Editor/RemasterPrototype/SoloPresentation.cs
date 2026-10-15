using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Darklight.Editor
{
    public static partial class RemasterPrototype
    {
        static bool soloMode;
        public static void SoloSkin(int skin)
        {
            soloMode=true;
            var a=actors.First(x=>x.skin==0||x.skin==1);
            var player=DarklightCombatLab.Instance.Player;var game=DarklightCampaign.Instance;
            var model=new CombatModel(game.weaponDefinitions[0].loadout);model.Tick(.01f,new CombatInput{aim=Vector2.down});
            player.GetComponent<DarklightCharacterView>().PresentPreview(model,Vector2.zero,skin,0,0,game.weaponDefinitions[0]);
            a.skin=skin;a.sprite=a.actor.sprite;a.scale=a.actor.transform.localScale;a.actor.GetPropertyBlock(a.block);a.actor.sortingOrder=10000;
            foreach(var other in actors.Where(x=>x!=a)){other.actor.enabled=false;other.shadow.gameObject.SetActive(false);}
            camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.045f,.065f,1);
            camera.orthographicSize=2.6f;camera.transform.position=player.transform.position+new Vector3(0,1.4f,-10);
            camera.aspect=(float)camera.targetTexture.width/camera.targetTexture.height;camera.ResetProjectionMatrix();
            addedCamera.volumeLayerMask=1<<31;
            foreach(var light in root.GetComponentsInChildren<Light2D>())
            {
                if(light.name=="AmbientBlue"){light.color=new Color(.94f,.96f,1);light.intensity=.78f;}
                else if(light.name=="WarmKey"){light.color=new Color(1,.83f,.68f);light.intensity=.35f;light.transform.position=player.transform.position+new Vector3(-2,2);}
                else if(light.name=="ColdMoon"){light.color=new Color(.53f,.7f,1);light.intensity=.4f;light.transform.position=player.transform.position+new Vector3(2,2);}
                else if(light.name=="LanternWarm"||light.name=="MemoryTeal")light.intensity=0;
            }
            root.transform.Find("MovingLampMarker")?.gameObject.SetActive(false);root.transform.Find("Embers")?.gameObject.SetActive(false);
            SoloLayers();
        }
        public static void SoloLayers()
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true)){originalLayers.TryAdd(t.gameObject,t.gameObject.layer);t.gameObject.layer=31;}
            foreach(var a in actors.Where(a=>a.actor.enabled))foreach(var t in a.actor.GetComponentsInChildren<Transform>(true)){originalLayers.TryAdd(t.gameObject,t.gameObject.layer);t.gameObject.layer=31;}
        }
        public static float SoloBodyHeight()=>actors.First(a=>a.actor.enabled).actor.bounds.size.y;
        static void PresentSoloIdle(int form,float clock)
        {
            foreach(var a in actors.Where(a=>a.skin>=0&&a.actor.enabled))
            {
                var normal=TodayCharacterBank.YoungIdle(a.skin,0,0,clock);var first=TodayCharacterBank.YoungIdle(a.skin,0,0,0);
                if(normal==null||first==null)throw new InvalidOperationException("Missing complete young idle");
                Sprite sprite=normal;Texture2D mask=TodayCharacterBank.Mask(normal);int owner=TodayCharacterBank.WholeFrameId(normal);float scale=a.scale.y;
                if(form>0)
                {
                    if(!DarklightTransformedBank.TryResolve(normal,form,out sprite,out mask,out owner)||!DarklightTransformedBank.TryResolve(first,form,out var firstForm,out _,out _))throw new InvalidOperationException("Missing animated full-body transformation");
                    scale=a.sprite.bounds.size.y*a.scale.y/firstForm.bounds.size.y;
                }
                a.actor.sprite=sprite;a.actor.transform.localScale=Vector3.one*scale;
                var block=new MaterialPropertyBlock();a.actor.GetPropertyBlock(block);block.SetFloat("_VampireForm",0);block.SetFloat("_WholeSprite",1);block.SetFloat("_WholeFrameId",owner);block.SetTexture("_VampireMask",mask);a.actor.SetPropertyBlock(block);
            }
        }
    }
}
