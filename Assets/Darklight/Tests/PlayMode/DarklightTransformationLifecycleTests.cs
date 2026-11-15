using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightTransformationLifecycleTests
    {
        [UnityTest]
        public IEnumerator DisablingPresentationDuringLiftRestoresOnlyTheVisual()
        {
            var actor=new GameObject("PlayMode presentation lifecycle actor");
            try
            {
                actor.transform.position=new Vector3(3,4,0);
                var visual=new GameObject("Complete figure");visual.transform.SetParent(actor.transform,false);
                var renderer=visual.AddComponent<SpriteRenderer>();
                var transition=actor.AddComponent<DarklightTransformationPresentation>();
                transition.Present(renderer,.5f,true);
                yield return null;
                Assert.That(transition.VisualHeight,Is.GreaterThan(0));
                transition.enabled=false;yield return null;
                Assert.That(transition.VisualHeight,Is.Zero);
                Assert.That(visual.transform.position,Is.EqualTo(DarklightCharacterView.SnapRenderPosition(actor.transform.position)));
                Assert.That(actor.transform.position,Is.EqualTo(new Vector3(3,4,0)));
            }
            finally{Object.Destroy(actor);}
        }
        [UnityTest]
        public IEnumerator DisablingLuciusDuringEntryClearsLiftAuraAndDamageGate()
        {
            var actor=new GameObject("PlayMode Arconte lifecycle actor");
            try
            {
                actor.AddComponent<Rigidbody2D>().gravityScale=0;
                actor.AddComponent<CircleCollider2D>();
                var health=actor.AddComponent<HealthSystem>();health.SetMaxHealth(260,false);
                var view=actor.AddComponent<DarklightLuciusView>();
                var enemy=actor.AddComponent<DarklightEnemy>();enemy.InitializeLucius(new CombatLoadout(),view);
                health.TakeDamage(health.MaxHealth*.5f);
                enemy.TickLucius(null,null,DarklightEnemy.LuciusTransformationDuration*.6f);
                var transition=actor.GetComponent<DarklightTransformationPresentation>();
                Assert.That(DarklightOfficialTransitionBank.IsAuthored(view.Renderer.sprite),Is.True);
                if(transition!=null)Assert.That(transition.VisualHeight,Is.Zero);
                Assert.That(enemy.LuciusTransforming,Is.True);Assert.That(enemy.AcceptDamage,Is.False);
                enemy.enabled=false;yield return null;
                Assert.That(enemy.LuciusTransforming,Is.False);Assert.That(enemy.AcceptDamage,Is.True);
                if(transition!=null)Assert.That(transition.VisualHeight,Is.Zero);
                Assert.That(DarklightOfficialTransitionBank.IsAuthored(view.Renderer.sprite),Is.False);
                Assert.That(view.Renderer.transform.position,Is.EqualTo(DarklightCharacterView.SnapRenderPosition(actor.transform.position)));
                foreach(var effect in actor.GetComponentsInChildren<MeshRenderer>())
                    Assert.That(effect.enabled,Is.False,effect.name);
            }
            finally{Object.Destroy(actor);}
        }
    }
}
