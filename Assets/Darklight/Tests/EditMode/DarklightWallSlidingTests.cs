using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightWallSlidingTests
    {
        GameObject root;Rigidbody2D body;
        [SetUp] public void Prepare(){root=new GameObject("SlidingFixture");var actor=new GameObject("Actor");actor.transform.SetParent(root.transform);body=actor.AddComponent<Rigidbody2D>();body.gravityScale=0;body.constraints=RigidbodyConstraints2D.FreezeRotation;actor.AddComponent<CircleCollider2D>().radius=.3f;}
        [TearDown] public void Finish(){Object.DestroyImmediate(root);Physics2D.SyncTransforms();}
        void Wall(Vector2 position,Vector2 size){var go=new GameObject("Wall");go.transform.SetParent(root.transform);go.transform.position=position;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();}
        [Test] public void DiagonalStepSlidesAlongWallWithoutAddingDistanceOrCrossingIt(){Wall(new Vector2(1.5f,0),new Vector2(1,10));DarklightNavigation.SafeStep(body,new Vector2(2,1));Assert.That(body.position.x,Is.InRange(.69f,.701f));Assert.That(body.position.y,Is.EqualTo(1).Within(.01f));Assert.That(body.position.magnitude,Is.LessThan(new Vector2(2,1).magnitude));}
        [Test] public void ResidualCastStopsAtSecondWallAndCanMoveAwayFromCorner(){Wall(new Vector2(1.5f,0),new Vector2(1,10));Wall(new Vector2(0,1.8f),new Vector2(10,1));DarklightNavigation.SafeStep(body,new Vector2(2,2));Assert.That(body.position.x,Is.LessThan(.701f));Assert.That(body.position.y,Is.LessThan(1.001f));var before=body.position;Physics2D.SyncTransforms();DarklightNavigation.SafeStep(body,new Vector2(-.2f,-.2f));Assert.That(body.position.x,Is.LessThan(before.x-.19f));Assert.That(body.position.y,Is.LessThan(before.y-.19f));}
        [Test] public void ActualCapsuleKeepsOriginalSizeAndSlidesThroughNarrowCorridor(){Object.DestroyImmediate(body.GetComponent<CircleCollider2D>());var feet=body.gameObject.AddComponent<CapsuleCollider2D>();feet.size=new Vector2(.42f,.28f);feet.offset=new Vector2(0,.14f);Wall(new Vector2(.73f,0),new Vector2(1,10));Wall(new Vector2(-.73f,0),new Vector2(1,10));var velocity=DarklightNavigation.SlidingVelocity(body,new Vector2(4,4),.02f);Assert.That(velocity.y,Is.EqualTo(4).Within(.01f));Assert.That(velocity.x,Is.InRange(0,1.1f));Assert.That(feet.size,Is.EqualTo(new Vector2(.42f,.28f)));Assert.That(feet.offset,Is.EqualTo(new Vector2(0,.14f)));Assert.That(DarklightNavigation.Frictionless.friction,Is.Zero);}
    }
}
