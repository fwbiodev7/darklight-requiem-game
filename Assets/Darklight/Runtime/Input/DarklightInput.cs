using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight
{
    /// <summary>Reuses the base's Gameplay/UI separation and device-aware action pattern with independent bindings.</summary>
    public sealed partial class DarklightInput : System.IDisposable
    {
        public readonly InputActionAsset Asset;
        public readonly InputActionMap Gameplay, UI;
        public bool UsingGamepad { get; private set; }
        public InputAction Action(string name) => Gameplay.FindAction(name, true);
        public InputAction Menu(string name) => UI.FindAction(name, true);
        public DarklightInput()
        {
            Asset = ScriptableObject.CreateInstance<InputActionAsset>(); Asset.name = "DarklightInputs";
            Gameplay = new InputActionMap("Gameplay"); UI = new InputActionMap("UI");
            Asset.AddActionMap(Gameplay); Asset.AddActionMap(UI);
            var move = Gameplay.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddBinding("<Gamepad>/leftStick", processors: "stickDeadzone(min=0.2)"); 
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            Gameplay.AddAction("Aim", InputActionType.Value, "<Gamepad>/rightStick", processors: "stickDeadzone(min=0.2)", expectedControlLayout: "Vector2");
            Add(Gameplay, "Light", "<Mouse>/leftButton", "<Gamepad>/buttonWest");
            Add(Gameplay, "Heavy", "<Mouse>/rightButton", "<Gamepad>/buttonNorth");
            Add(Gameplay, "Dodge", "<Keyboard>/space", "<Gamepad>/buttonEast");
            Add(Gameplay, "Parry", "<Keyboard>/q", "<Gamepad>/leftShoulder");
            Add(Gameplay, "Skill", "<Keyboard>/f", "<Gamepad>/rightShoulder");
            Add(Gameplay, "Interact", "<Keyboard>/e", "<Gamepad>/buttonSouth");
            Add(Gameplay, "Transform", "<Keyboard>/t", "<Gamepad>/leftTrigger");
            Add(Gameplay, "MP3", "<Keyboard>/m");
            Add(Gameplay, "Potion", "<Keyboard>/r", "<Gamepad>/rightTrigger");
            Add(Gameplay, "Journal", "<Keyboard>/j");
            Add(Gameplay, "Power", "<Keyboard>/c", "<Gamepad>/rightStickPress");
            Add(Gameplay, "PowerPrevious", "<Keyboard>/1", "<Gamepad>/dpad/left");
            Add(Gameplay, "PowerNext", "<Keyboard>/2", "<Gamepad>/dpad/right");
            Add(Gameplay,"WeaponNext","<Keyboard>/v","<Gamepad>/leftStickPress");
            Add(Gameplay,"Aerial","<Keyboard>/g","<Gamepad>/dpad/up");
            Add(Gameplay,"WeaponSpecial","<Keyboard>/z","<Gamepad>/dpad/down");
            Add(Gameplay,"Vampire","<Keyboard>/h","<Gamepad>/rightStick/up");
            Add(Gameplay,"VampireNext","<Keyboard>/k","<Gamepad>/rightStick/down");
            Add(UI, "Pause", "<Keyboard>/escape", "<Gamepad>/start");
            Add(UI,"DialogueAdvance","<Keyboard>/enter","<Keyboard>/space","<Keyboard>/e","<Gamepad>/buttonSouth");
            Add(UI, "Submit", "<Keyboard>/enter", "<Gamepad>/buttonSouth");
            Add(UI, "Cancel", "<Keyboard>/escape", "<Gamepad>/buttonEast");
            var map=UI.AddAction("Map",InputActionType.Button);map.AddBinding("<Keyboard>/tab");
            map.AddBinding("<Gamepad>/select");
            Add(UI,"MapZoomIn","<Keyboard>/equals","<Keyboard>/numpadPlus","<Gamepad>/rightTrigger");
            Add(UI,"MapZoomOut","<Keyboard>/minus","<Keyboard>/numpadMinus","<Gamepad>/leftTrigger");
            Add(UI,"MapCenter","<Keyboard>/home","<Gamepad>/rightStickPress");
            Add(UI,"MapMark","<Keyboard>/space","<Gamepad>/buttonWest");Add(UI,"MapClear","<Keyboard>/delete","<Gamepad>/buttonNorth");
            var pan=UI.AddAction("MapPan",InputActionType.Value,expectedControlLayout:"Vector2");
            pan.AddBinding("<Gamepad>/leftStick",processors:"stickDeadzone(min=0.2)");
            pan.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");
            UI.AddAction("MapCursor",InputActionType.Value,"<Gamepad>/rightStick",processors:"stickDeadzone(min=0.2)",expectedControlLayout:"Vector2");
            var objective=UI.AddAction("MapObjective",InputActionType.Value);
            objective.AddCompositeBinding("1DAxis").With("Positive","<Keyboard>/upArrow").With("Negative","<Keyboard>/downArrow");
            objective.AddCompositeBinding("1DAxis").With("Positive","<Gamepad>/dpad/up").With("Negative","<Gamepad>/dpad/down");
            var nav = UI.AddAction("Navigate", InputActionType.Value, expectedControlLayout: "Vector2");
            nav.AddBinding("<Gamepad>/dpad"); nav.AddBinding("<Gamepad>/leftStick");
            nav.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            UI.AddAction("KeyboardActivity", InputActionType.Button, "<Keyboard>/anyKey");
            UI.AddAction("PointerActivity", InputActionType.Value, "<Mouse>/delta", expectedControlLayout: "Vector2");
            foreach (var action in Asset) action.performed += Track;
            InputSystem.onDeviceChange += DeviceChanged;
            AddOptionalDeviceBindings();
            LoadBindings();
            Asset.Enable();
        }
        static void Add(InputActionMap map, string name, params string[] paths)
        { var action = map.AddAction(name, InputActionType.Button); foreach (var path in paths) action.AddBinding(path); }
        void Track(InputAction.CallbackContext context)
        {
            if (context.valueType == typeof(Vector2) && context.ReadValue<Vector2>().sqrMagnitude < .01f) return;
            UsingGamepad = context.control.device is Gamepad;
        }
        void DeviceChanged(InputDevice device, InputDeviceChange change)
        { if (device is Gamepad && (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)) UsingGamepad = false; }
        public void SetGameplay(bool enabled) { if (enabled) Gameplay.Enable(); else Gameplay.Disable(); }
        public CombatInput Read(Vector2 player, Camera camera, Rect viewport)
        {
            Vector2 movement = Action("Move").ReadValue<Vector2>();
            return new CombatInput { move = movement, aim = movement,
                lightPressed = Action("Light").WasPressedThisFrame(), heavyPressed = Action("Heavy").WasPressedThisFrame(),
                heavyHeld = Action("Heavy").IsPressed(), heavyReleased = Action("Heavy").WasReleasedThisFrame(),
                dodgePressed = Action("Dodge").WasPressedThisFrame(), parryPressed = Action("Parry").WasPressedThisFrame(),
                skillPressed = Action("Skill").WasPressedThisFrame(),aerialPressed=Action("Aerial").WasPressedThisFrame(),weaponSpecialPressed=Action("WeaponSpecial").WasPressedThisFrame() };
        }
        public string BindingLabel(string name)
        {
            var action=ConfigurableAction(name);
            for(int i=0;i<action.bindings.Count;i++)
            {
                var b=action.bindings[i];if(b.isComposite||b.isPartOfComposite)continue;
                string path=b.effectivePath;if(string.IsNullOrEmpty(path)||path.StartsWith("<Gamepad>")!=UsingGamepad)continue;
                string label=action.GetBindingDisplayString(i,InputBinding.DisplayStringOptions.DontIncludeInteractions);
                if(UsingGamepad)label=path.EndsWith("/buttonSouth")?"A":path.EndsWith("/buttonWest")?"X":path.EndsWith("/buttonEast")?"B":path.EndsWith("/buttonNorth")?"Y":path.EndsWith("/leftShoulder")?"LB":path.EndsWith("/rightShoulder")?"RB":path.EndsWith("/select")?"SELECT":path.EndsWith("/start")?"START":path.EndsWith("/rightTrigger")?"RT":path.EndsWith("/leftTrigger")?"LT":path.EndsWith("/leftStickPress")?"LS":path.EndsWith("/rightStickPress")?"RS":path.EndsWith("/rightStick/up")?"RS CIMA":path.EndsWith("/rightStick/down")?"RS BAIXO":label;
                if(!UsingGamepad)label=path.EndsWith("/leftButton")?"M1":path.EndsWith("/rightButton")?"M2":path.EndsWith("/escape")?"ESC":label;
                return label.ToUpperInvariant();
            }
            return name=="MP3"&&UsingGamepad?"PAUSA > MP3":name.ToUpperInvariant();
        }
        public void Dispose()
        {
            InputSystem.onDeviceChange -= DeviceChanged; Asset.Disable();
            foreach (var action in Asset) action.performed -= Track;
            if (Application.isPlaying) Object.Destroy(Asset); else Object.DestroyImmediate(Asset);
        }
    }
}

