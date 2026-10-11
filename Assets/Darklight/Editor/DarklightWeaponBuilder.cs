using System.IO;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightWeaponBuilder
    {
        const string Root="Assets/Darklight/Data/Weapons/";
        static T Asset<T>(string name) where T:ScriptableObject
        {
            string path=Root+name+".asset";var value=AssetDatabase.LoadAssetAtPath<T>(path);
            if(value==null){value=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(value,path);}return value;
        }
        static AttackDefinition Attack(string name,StrikeKind kind,float damage,float startup,float active,float recovery,float range,float angle,int pulses=1,float cooldown=0,float lunge=0,bool pull=false,bool blink=false)
        {
            var a=Asset<AttackDefinition>(name);a.displayName=name;a.kind=kind;a.damage=damage;a.startup=startup;a.active=active;a.recovery=recovery;a.range=range;a.angle=angle;a.hitPulses=pulses;a.cooldown=cooldown;a.lungeSpeed=lunge;a.pullTarget=pull;a.blink=blink;a.knockback=kind==StrikeKind.Light?.15f:.35f;EditorUtility.SetDirty(a);return a;
        }
        public static void Apply(DarklightCampaign game,DarklightFighter player)
        {
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();var katana=player.loadout;
            var icons=DarklightUserArtImporter.Slice("weaponProps",false);
            katana.aerial=Attack("KatanaAerial",StrikeKind.Aerial,19,.065f,.09f,.14f,1.65f,145,1,1.1f);
            katana.weaponSpecial=null; // Onda remains the katana's existing special.
            var daggers=new CombatLoadout{
                lights=new[]{Attack("Presas1",StrikeKind.Light,8,.035f,.045f,.095f,1.25f,105),Attack("Presas2",StrikeKind.Light,9,.035f,.05f,.10f,1.25f,110),Attack("Presas3",StrikeKind.Light,11,.045f,.055f,.115f,1.3f,125)},
                heavy=Attack("PresasInvestida",StrikeKind.Heavy,7,.055f,.20f,.15f,1.5f,90,3,0,1.4f),
                charged=Attack("PresasCarregadas",StrikeKind.Charged,11,.075f,.23f,.16f,1.65f,110,3,0,1.5f),
                aerial=Attack("PresasAereas",StrikeKind.Aerial,7,.035f,.21f,.10f,1.5f,150,3,1.1f),
                weaponSpecial=Attack("PresasSombrias",StrikeKind.WeaponSpecial,9,.055f,.30f,.15f,1.9f,240,4,10,0,false,true),counter=katana.counter,skill=katana.skill};
            var chain=new CombatLoadout{
                lights=new[]{Attack("Laco1",StrikeKind.Light,12,.055f,.08f,.17f,3.1f,130),Attack("Laco2",StrikeKind.Light,14,.065f,.08f,.18f,3.1f,145),Attack("Laco3",StrikeKind.Light,17,.075f,.095f,.19f,3.2f,160)},
                heavy=Attack("LacoPuxao",StrikeKind.Heavy,24,.095f,.12f,.22f,3.7f,55,1,0,0,true),
                charged=Attack("LacoCarregado",StrikeKind.Charged,30,.12f,.14f,.24f,3.7f,65,1,0,0,true),
                aerial=Attack("LacoGiro",StrikeKind.Aerial,10,.065f,.22f,.14f,2.8f,360,2,1.5f),
                weaponSpecial=Attack("LacoRedemoinho",StrikeKind.WeaponSpecial,9,.08f,.40f,.19f,3.2f,360,4,12),counter=katana.counter,skill=katana.skill};
            game.weaponDefinitions=new WeaponDefinition[3];
            for(int i=0;i<3;i++)
            {
                var w=Asset<WeaponDefinition>(new[]{"Requiem","PresasDoVeu","LacoDaRuptura"}[i]);w.id=(DarklightWeaponId)i;w.displayName=new[]{"REQUIEM","PRESAS DO VEU","LACO DA RUPTURA"}[i];w.unlockAfterChapter=i==0?-1:i==1?0:2;w.loadout=i==0?katana:i==1?daggers:chain;w.icon=icons[i];
                if(i>0){string suffix=i==1?"daggers":"chain";w.requiemPoses=DarklightUserArtImporter.Slice("requiem_"+suffix,true);w.rockPoses=DarklightUserArtImporter.Slice("rock_"+suffix,true);}
                EditorUtility.SetDirty(w);game.weaponDefinitions[i]=w;
            }
            if(player.GetComponent<DarklightWeaponController>()==null)player.gameObject.AddComponent<DarklightWeaponController>();
            var view=player.GetComponent<DarklightChainView>()??player.gameObject.AddComponent<DarklightChainView>();view.link=icons[3];view.blade=icons[4];view.material=game.pixelMaterial;
        }
    }
}
