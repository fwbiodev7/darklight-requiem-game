using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightSpriteRootStabilityTests
    {
        readonly List<Object> owned=new();
        bool priorSilent,priorPause;float priorVolume;
        SimulationMode2D priorSimulation;

        [SetUp] public void SetUp()
        {
            priorSilent=DarklightAutomation.Silent;priorPause=AudioListener.pause;priorVolume=AudioListener.volume;
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            priorSimulation=Physics2D.simulationMode;Physics2D.simulationMode=SimulationMode2D.Script;
            Assert.That(DarklightCampaign.Instance,Is.Null,"Isolated checks must not load campaign or saves");
        }
        [TearDown] public void TearDown()
        {
            foreach(var obj in owned)if(obj!=null)Object.DestroyImmediate(obj);
            owned.Clear();DarklightLuciusSpriteBank.ClearCache();Physics2D.simulationMode=priorSimulation;
            DarklightAutomation.Silent=priorSilent;AudioListener.pause=priorPause;AudioListener.volume=priorVolume;
        }
        GameObject Actor(string name)
        {var obj=new GameObject(name);owned.Add(obj);return obj;}
        static void AssertRoot(SpriteRenderer art,Vector3 root)
        {
            Assert.That(Vector3.Distance(art.transform.position,DarklightCharacterView.SnapRenderPosition(root)),Is.LessThan(.00001f));
            Assert.That(AudioListener.pause,Is.True);Assert.That(AudioListener.volume,Is.Zero);
            Assert.That(Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Any(source=>source.isPlaying),Is.False);
        }

        [UnityTest] public IEnumerator FabiusConfirmedContactsDoNotMoveTheRenderedFigure()
        {
            var actor=Actor("FabiusStableRoot");actor.transform.position=new Vector3(1.123f,-.237f,0);
            var figure=new GameObject("Figure");figure.transform.SetParent(actor.transform,false);
            var art=figure.AddComponent<SpriteRenderer>();
            var view=actor.AddComponent<DarklightCharacterView>();view.enabled=false;
            Vector3 root=actor.transform.position;
            for(int i=0;i<12;i++)
            {
                view.ConfirmContactPose(i%2==0?Vector2.right:Vector2.left);
                view.SendMessage("LateUpdate");AssertRoot(art,root);
                Assert.That(actor.transform.position,Is.EqualTo(root));
                yield return null;
            }
            Assert.That(view.ConfirmedContactPresentations,Is.EqualTo(12));
        }

        [UnityTest] public IEnumerator LuciusWalkCyclesWithPhysicsTravelAndNeverMovesItsOwnRoot()
        {
            var actor=Actor("LuciusStableWalk");var body=actor.AddComponent<Rigidbody2D>();body.gravityScale=0;
            var view=actor.AddComponent<DarklightLuciusView>();view.enabled=false;
            var model=new CombatModel(new CombatLoadout());var frames=new HashSet<Sprite>();
            var text=Resources.Load<TextAsset>(DarklightLuciusSpriteBank.Root+"catalog");
            var page=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text).pages.First(p=>p.action=="Walk");
            float angle=page.direction*45*Mathf.Deg2Rad;
            Vector2 velocity=new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle))*2.3f;
            for(int i=0;i<40;i++)
            {
                body.linearVelocity=velocity;Physics2D.Simulate(.02f);model.Tick(.02f,default);
                Vector3 root=actor.transform.position;
                Assert.That(view.Present(model,velocity),Is.True);view.SendMessage("LateUpdate");
                Assert.That(view.UsesIdleFallback,Is.False);frames.Add(view.Renderer.sprite);
                AssertRoot(view.Renderer,root);Assert.That(actor.transform.position,Is.EqualTo(root));
                yield return null;
            }
            Assert.That(Vector2.Dot(body.position,velocity.normalized),Is.EqualTo(40*.02f*2.3f).Within(.001f));
            Assert.That(frames.Count,Is.GreaterThanOrEqualTo(4));
        }

        [UnityTest] public IEnumerator IntegratedSwordPhasesKeepTheGroundRootStationary()
        {
            var text=Resources.Load<TextAsset>(DarklightLuciusSpriteBank.Root+"catalog");
            var pages=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text).pages.Where(p=>p.action=="Attack").ToArray();
            Assert.That(pages.Length,Is.GreaterThan(0),"At least one reviewed technique must be imported");
            var actor=Actor("LuciusStableSword");actor.transform.position=new Vector3(.413f,-.187f,0);
            var view=actor.AddComponent<DarklightLuciusView>();view.enabled=false;
            foreach(var page in pages)
            {
                var attack=ScriptableObject.CreateInstance<AttackDefinition>();owned.Add(attack);
                attack.animationKey=page.attackKey;attack.startup=.2f;attack.active=.1f;attack.recovery=.3f;
                var model=new CombatModel(new CombatLoadout());
                float angle=page.direction*45*Mathf.Deg2Rad;
                model.TriggerAttack(attack,new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle)));
                Vector3 root=actor.transform.position;var frames=new HashSet<Sprite>();
                foreach(float dt in new[]{.1f,.15f,.2f})
                {
                    model.Tick(dt,default);Assert.That(view.Present(model),Is.True);view.SendMessage("LateUpdate");
                    Assert.That(view.UsesIdleFallback,Is.False);frames.Add(view.Renderer.sprite);
                    AssertRoot(view.Renderer,root);Assert.That(actor.transform.position,Is.EqualTo(root));
                    yield return null;
                }
                Assert.That(frames.Count,Is.EqualTo(3),page.id);
            }
        }
    }
}
