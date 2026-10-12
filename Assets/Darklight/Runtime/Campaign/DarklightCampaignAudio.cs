using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public DarklightSoundDirector AudioDirector {get;private set;}
        void InitializeSoundDesign(){AudioDirector=gameObject.AddComponent<DarklightSoundDirector>();AudioDirector.Initialize(this);}
        public void WeaponSwing(AttackDefinition attack,int serial)=>AudioDirector?.Swing(attack,serial,(int)(Weapons?.Equipped.id??DarklightWeaponId.Katana));
        public void EnemyAudioWarning(DarklightEnemy enemy)=>AudioDirector?.EnemyWarning(enemy.Boss,enemy.transform.position);
        void AudioFootstep()
        {
            // The authored wooden bridge is localized; outer garden paths remain grass.
            Vector2 p=lab.Player.transform.position;bool wood=MapRegion==4&&Mathf.Abs(p.x-Map.Spawn.x)<2.1f&&Mathf.Abs(p.y-Map.Spawn.y)<2.7f;
            AudioDirector?.Footstep(wood,MapRegion==4||MapRegion==7,p);
        }
    }
}
