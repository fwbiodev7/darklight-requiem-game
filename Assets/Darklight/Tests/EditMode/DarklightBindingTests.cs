using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight.Tests
{
    public sealed class DarklightBindingTests:InputTestFixture
    {
        string saved;bool exists;
        public override void Setup(){base.Setup();exists=PlayerPrefs.HasKey("Darklight.Bindings");saved=PlayerPrefs.GetString("Darklight.Bindings");PlayerPrefs.DeleteKey("Darklight.Bindings");}
        public override void TearDown(){if(exists)PlayerPrefs.SetString("Darklight.Bindings",saved);else PlayerPrefs.DeleteKey("Darklight.Bindings");PlayerPrefs.Save();base.TearDown();}
        static DarklightInput.BindingSlot Slot(DarklightInput input,string name,bool pad)=>input.Slots(name,pad)[0];
        [Test] public void NewCampaignHasAllThreeFormsWithoutCompletingStory()
        {
            var state=new DarklightProgress();Assert.That(state.FormUnlocked,Is.EqualTo(3));Assert.That(state.formsUnlocked,Is.Zero);Assert.That(state.chapter,Is.Zero);Assert.That(state.completed,Is.False);Assert.That(state.mp3Unlocked,Is.False);
        }
        [Test] public void KeyboardRemapSurvivesNewInputInstanceAndProducesActualAction()
        {
            var keyboard=InputSystem.AddDevice<Keyboard>();
            using(var input=new DarklightInput())Assert.That(input.Assign(Slot(input,"Light",false),"<Keyboard>/l"),Is.True);
            using(var input=new DarklightInput()){Press(keyboard.lKey);Assert.That(input.Action("Light").WasPressedThisFrame(),Is.True);Assert.That(input.Action("Heavy").WasPressedThisFrame(),Is.False);}
        }
        [Test] public void MovementCompositePartCanBeChangedAndSaved()
        {
            var keyboard=InputSystem.AddDevice<Keyboard>();
            using(var input=new DarklightInput())Assert.That(input.Assign(input.Slots("Move",false).First(s=>s.binding.path=="<Keyboard>/w"),"<Keyboard>/i"),Is.True);
            using(var input=new DarklightInput()){Press(keyboard.iKey);Assert.That(input.Action("Move").ReadValue<Vector2>(),Is.EqualTo(Vector2.up));}
        }
        [Test] public void ConflictingTriggerIsRejectedAndExplicitSwapPreservesBothActions()
        {
            var pad=InputSystem.AddDevice<Gamepad>();
            using(var input=new DarklightInput())
            {
                var transform=Slot(input,"Transform",true);Assert.That(input.Assign(transform,"<Gamepad>/rightTrigger"),Is.False);Assert.That(transform.binding.effectivePath,Is.EqualTo("<Gamepad>/leftTrigger"));
                Assert.That(input.Swap(transform,"<Gamepad>/rightTrigger"),Is.True);
            }
            using(var input=new DarklightInput())
            {
                Set(pad.rightTrigger,1);Assert.That(input.Action("Transform").WasPressedThisFrame(),Is.True);Assert.That(input.Action("Potion").WasPressedThisFrame(),Is.False);
                Set(pad.rightTrigger,0);Set(pad.leftTrigger,1);Assert.That(input.Action("Potion").WasPressedThisFrame(),Is.True);Assert.That(input.Action("Transform").WasPressedThisFrame(),Is.False);
            }
        }
        [Test] public void DeviceRestorePreservesOtherDeviceAndMapContextDoesNotCauseFalseConflict()
        {
            using(var input=new DarklightInput())
            {
                Assert.That(input.Conflicts(Slot(input,"Potion",true),"<Gamepad>/rightTrigger"),Is.Empty);
                Assert.That(input.Assign(Slot(input,"Transform",false),"<Keyboard>/p"),Is.True);Assert.That(input.Swap(Slot(input,"Transform",true),"<Gamepad>/rightTrigger"),Is.True);
                input.RestoreBindings(true);
            }
            using(var input=new DarklightInput()){Assert.That(Slot(input,"Transform",false).binding.effectivePath,Is.EqualTo("<Keyboard>/p"));Assert.That(Slot(input,"Transform",true).binding.effectivePath,Is.EqualTo("<Gamepad>/leftTrigger"));}
        }
        [Test] public void LegacyBindingsMigrateDespiteRandomOldIdentifiers()
        {
            PlayerPrefs.SetString("Darklight.Bindings","{\"bindings\":[{\"action\":\"Gameplay/Light\",\"id\":\"173a5f99-7f7d-48ef-aa4f-8fba8c8a1de1\",\"path\":\"<Keyboard>/p\"}]}");
            var keyboard=InputSystem.AddDevice<Keyboard>();using(var input=new DarklightInput()){Press(keyboard.pKey);Assert.That(input.Action("Light").WasPressedThisFrame(),Is.True);input.SaveBindings();}
            using(var input=new DarklightInput())Assert.That(Slot(input,"Light",false).binding.effectivePath,Is.EqualTo("<Keyboard>/p"));
        }
        [Test] public void UnusedOptionalGamepadSlotCanBeAssignedAndEmergencyMenuCannotBeStolen()
        {
            using(var input=new DarklightInput())
            {
                Assert.That(input.Assign(Slot(input,"MP3",true),"<Gamepad>/start"),Is.False);
                Assert.That(input.Assign(Slot(input,"MP3",true),"<Gamepad>/leftTrigger"),Is.False);
                Assert.That(input.Assign(Slot(input,"MP3",true),"<Gamepad>/buttonSouth"),Is.False);
                Assert.That(input.Assign(Slot(input,"Light",false),"<Keyboard>/escape"),Is.False);
                input.Assign(Slot(input,"Transform",true),"");Assert.That(input.Assign(Slot(input,"MP3",true),"<Gamepad>/leftTrigger"),Is.True);
            }
        }
    }
}
