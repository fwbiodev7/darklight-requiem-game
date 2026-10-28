using UnityEngine;
namespace Darklight
{
    public sealed class DarklightWeaponController : MonoBehaviour
    {
        DarklightCampaign game;DarklightFighter fighter;Rigidbody2D body;
        int pending=-1,lastSerial=-1,lastCompatible;float pendingUntil,swapClock;
        public WeaponDefinition Equipped {get;private set;}
        public bool Swapping=>swapClock>0;
        public void Initialize(DarklightCampaign campaign)
        {game=campaign;fighter=GetComponent<DarklightFighter>();body=GetComponent<Rigidbody2D>();Equipped=game.weaponDefinitions[0];}
        public bool Unlocked(int index)=>index>=0&&index<game.weaponDefinitions.Length&&DarklightPlayableScope.WeaponActive(index)&&((game.Progress.weaponUnlocks&(1<<index))!=0||index==3&&game.Training);
        public void Restore()
        {
            pending=-1;swapClock=0;lastSerial=-1;game.Progress.weaponUnlocks|=1;DarklightPlayableScope.Migrate(game.Progress);
            if(game.Progress.chapter>0||(game.Progress.chapter==0&&game.Progress.checkpoint==4))game.Progress.weaponUnlocks|=2;
            if(game.Progress.chapter>2||(game.Progress.chapter==2&&game.Progress.checkpoint==4))game.Progress.weaponUnlocks|=4;
            // The final three-weapon trial must remain completable if the optional motor was missed.
            if(game.Progress.chapter>=9)game.Progress.weaponUnlocks|=8;
            int index=Unlocked(game.Progress.equippedWeapon)?game.Progress.equippedWeapon:0;
            game.Progress.equippedWeapon=index;if(index<3)lastCompatible=index;
            Equipped=game.weaponDefinitions[index];fighter.loadout=Equipped.loadout;fighter.ResetModel();game.AbyssRider?.ResetMotion();
        }
        public void Request(int index)
        {
            if(!Unlocked(index)){game.Notice(index==3?"CAVALEIRO: MOTOR SELADO NA CRIPTA":index==1?"ARMA DE LEGADO INDISPONIVEL":(DarklightRuinFistsBank.Ready?"PUNHOS DA RUÍNA: DERROTE A MATRIARCA":"CORRENTE: DERROTE A MATRIARCA"));return;}
            if(Equipped==game.weaponDefinitions[index])return;
            pending=index;pendingUntil=fighter.Model.Clock+.8f;
        }
        public void BeforeTick(float dt,bool active)
        {
            if(game==null)return;EnforceSkin();if(!active||dt<=0)return;swapClock=Mathf.Max(0,swapClock-dt);
            if(game.GetComponent<DarklightCombatLab>().Input.Action("WeaponNext").WasPressedThisFrame())
            {
                RequestNext();
            }
            if(pending<0)return;
            if(!Unlocked(pending)){pending=-1;return;}
            if(game.AbyssRider?.BlocksSwap==true)return;
            if(fighter.Model.Clock>pendingUntil){pending=-1;return;}
            if(!fighter.Model.Equip(game.weaponDefinitions[pending].loadout))return;
            if(pending<3)lastCompatible=pending;
            Equipped=game.weaponDefinitions[pending];fighter.loadout=Equipped.loadout;game.Progress.equippedWeapon=pending;pending=-1;swapClock=.12f;
            var art=GetComponentInChildren<SpriteRenderer>();DarklightCombatFX.Echo(art,.08f);bool help=game.MarkAbyssEquipped();game.RecordWeaponSelection();if(help)game.ShowAbyssHelp();
        }
        public void RequestNext()
        {
            int start=pending>=0?pending:(int)Equipped.id;
            for(int step=1;step<=game.weaponDefinitions.Length;step++){int index=(start+step)%game.weaponDefinitions.Length;if(Unlocked(index)){Request(index);break;}}
        }
        public void EnforceSkin(){if(Equipped?.id!=DarklightWeaponId.AbyssRider||Unlocked(3))return;pending=-1;int index=Unlocked(lastCompatible)?lastCompatible:0;game.Progress.equippedWeapon=index;Equipped=game.weaponDefinitions[index];fighter.loadout=Equipped.loadout;fighter.ResetModel();game.AbyssRider?.ResetMotion();}
        public void AfterTick()
        {
            if(fighter.Model.AttackSerial==lastSerial)return;lastSerial=fighter.Model.AttackSerial;
            if(fighter.Model.Attack?.blink!=true)return;
            var art=GetComponentInChildren<SpriteRenderer>();DarklightCombatFX.Echo(art,.13f);
            DarklightNavigation.SafeStep(body,fighter.Model.Facing*1.6f,.32f);Physics2D.SyncTransforms();
        }
    }
}
