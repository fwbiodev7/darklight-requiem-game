using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        GameObject ladyAlly, neroAlly;
        CircleCollider2D ladyAllyCollider, neroAllyCollider;
        Transform waitingLady;
        bool ladyFollowing;

        bool CanCallLady => Active&&!Training&&!ExploringNewRegion&&Progress.LadyAvailable&&
            Map!=null&&levelRoot!=null;

        void CallLady()
        {
            if(!CanCallLady)return;
            ladyFollowing=!ladyFollowing;
            if(ladyFollowing)StartCampaignAlly(true);
            else StopLadyAlly();
        }

        void StartCampaignAlly(bool lady)
        {
            if(Training||ExploringNewRegion||(lady?ladyAlly:neroAlly)!=null)return;

            Vector2 position = Map.SafeSpawn((Vector2)lab.Player.transform.position + new Vector2(-1.6f, .4f), .3f);
            var campaignAlly = new GameObject(lady ? "LadyApoio" : "NeroApoio");
            campaignAlly.transform.SetParent(levelRoot, false);
            campaignAlly.transform.position = position;

            var actor = AddProp(lady ? "LyraMoreau" : "KaelAliado", lady ? extras[1] : bossFrames[0], position, 1.8f);
            actor.transform.SetParent(campaignAlly.transform, true);
            actor.transform.localPosition = Vector3.zero;
            if (lady)
            {
                waitingLady = levelRoot.Find("LyraMoreau");
                if (waitingLady != null) waitingLady.gameObject.SetActive(false);
            }

            var body = campaignAlly.AddComponent<Rigidbody2D>();
            body.gravityScale = 0;
            body.freezeRotation = true;
            if(lady)body.interpolation=RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var campaignAllyCollider = campaignAlly.AddComponent<CircleCollider2D>();
            campaignAllyCollider.radius = .28f;
            campaignAllyCollider.offset = Vector2.up * .18f;
            if(lady){ladyAlly=campaignAlly;ladyAllyCollider=campaignAllyCollider;}
            else{neroAlly=campaignAlly;neroAllyCollider=campaignAllyCollider;}
            if(ladyAllyCollider!=null&&neroAllyCollider!=null)
                Physics2D.IgnoreCollision(ladyAllyCollider,neroAllyCollider);
            campaignAlly.AddComponent<DarklightCombatant>();
            foreach (var collider in lab.Player.GetComponentsInChildren<Collider2D>())
                IgnoreCompanionContact(collider);
            foreach (var enemy in enemies)
                if (enemy != null) IgnoreCompanionContact(enemy.GetComponent<Collider2D>());

            campaignAlly.AddComponent<DarklightCampaignAlly>().Initialize(this, lab, lady);
        }

        void IgnoreCompanionContact(Collider2D collider)
        {
            if(collider==null)return;
            if(ladyAllyCollider!=null)Physics2D.IgnoreCollision(ladyAllyCollider,collider);
            if(neroAllyCollider!=null)Physics2D.IgnoreCollision(neroAllyCollider,collider);
        }

        void StopCampaignAlly()
        {
            StopNeroAlly();
            StopLadyAlly();
        }

        void StopNeroAlly() => RemoveCompanion(ref neroAlly,ref neroAllyCollider);

        void StopLadyAlly()
        {
            RemoveCompanion(ref ladyAlly,ref ladyAllyCollider);
            if(waitingLady!=null)waitingLady.gameObject.SetActive(Progress.LadyAvailable);
            waitingLady=null;
        }

        static void RemoveCompanion(ref GameObject companion,ref CircleCollider2D collider)
        {
            if(companion!=null)
            {
                companion.SetActive(false);
                Destroy(companion);
            }
            companion=null;
            collider=null;
        }

        void RegroupLady()
        {
            if(!ladyFollowing||!Progress.LadyAvailable)return;
            if(ladyAlly==null)StartCampaignAlly(true);
            else ladyAlly.GetComponent<DarklightCampaignAlly>().Regroup();
        }

        void FinishAllianceDialogue()
        {
            if (!Progress.TruceEnded) { Open("Ending"); return; }
            Speak(new[]{
                "NERO|A trégua acabou, Fabius. Ainda temos contas a acertar.",
                "FABIUS|Eu estarei aqui quando você voltar."
            }, () => Open("Ending"));
        }
    }
}
