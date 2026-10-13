using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public DarklightSoundDirector AudioDirector {get;private set;}
        void InitializeSoundDesign(){AudioDirector=GetComponent<DarklightSoundDirector>()??gameObject.AddComponent<DarklightSoundDirector>();AudioDirector.Initialize(this);SkinMusic=GetComponent<TodaySkinMusic>()??gameObject.AddComponent<TodaySkinMusic>();if(GetComponent<TodayCampaignSoundDesign>()==null)gameObject.AddComponent<TodayCampaignSoundDesign>();}
        public void WeaponSwing(AttackDefinition attack,int serial)=>AudioDirector?.Swing(attack,serial,(int)(Weapons?.Equipped.id??DarklightWeaponId.Katana));
        public void EnemyAudioWarning(DarklightEnemy enemy)=>AudioDirector?.EnemyIdentity(enemy.kind,"alert",enemy.transform.position);
        void AudioFootstep()
        {
            Vector2 p=lab.Player.transform.position;AudioDirector?.Footstep(TodaySurfaces.Resolve(MapRegion,p,Map.Spawn,Map),p);
        }
    }
}
