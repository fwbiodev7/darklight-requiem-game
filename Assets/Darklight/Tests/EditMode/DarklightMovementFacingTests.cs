using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Darklight.Tests
{
    public sealed class DarklightMovementFacingTests : InputTestFixture
    {
        [Test] public void PointerAndRightStickDoNotTurnFabioButMovementDoes()
        {
            var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();var pad=InputSystem.AddDevice<Gamepad>();
            using(var input=new DarklightInput())
            {
                Set(mouse.position,new Vector2(5000,3000));Set(pad.rightStick,Vector2.up);
                var idle=input.Read(Vector2.zero,null,new Rect(0,0,1280,720));
                Assert.That(idle.aim,Is.EqualTo(Vector2.zero));
                Press(keyboard.aKey);var left=input.Read(Vector2.zero,null,new Rect(0,0,1280,720));
                Assert.That(left.aim,Is.EqualTo(Vector2.left));Assert.That(left.move,Is.EqualTo(Vector2.left));
                Release(keyboard.aKey);Set(pad.leftStick,Vector2.down);
                var down=input.Read(Vector2.zero,null,new Rect(0,0,1280,720));Assert.That(down.aim.y,Is.LessThan(-.9f));
            }
        }
    }
}
