using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Darklight
{
    [DefaultExecutionOrder(-1000)]
    public sealed class DarklightCombatLab : MonoBehaviour
    {
        public static DarklightCombatLab Instance { get; private set; }
        public Camera WorldCamera;
        public DarklightFighter Player;
        public DarklightTrainingTarget[] Targets;
        public DarklightPixelHUD PixelHUD;
        public DarklightInput Input { get; private set; }
        public bool Paused { get; private set; }
        public int RepaintCount { get; private set; }
        public bool Ready => Input != null;
        public float CombatDelta => Paused || Time.unscaledTime < hitstopUntil || Time.frameCount <= suppressThrough ? 0 : Time.deltaTime;
        public Rect Viewport
        {
            get
            {
                float scale = Mathf.Max(1, Mathf.Floor(Mathf.Min(Screen.width / 1280f, Screen.height / 720f)));
                if (Screen.width < 1280 || Screen.height < 720) scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
                return new Rect((Screen.width - 1280 * scale) * .5f, (Screen.height - 720 * scale) * .5f, 1280 * scale, 720 * scale);
            }
        }
        RenderTexture world;
        float hitstopUntil, rumbleUntil, nextNavigate;
        int menuSelection, suppressThrough;
        Gamepad rumbling;
        DarklightLabPauseMenu pauseMenu;
        Rigidbody2D pausedBody;
        RigidbodyInterpolation2D resumeInterpolation;

        void Awake()
        {
            Instance = this; Input = new DarklightInput();
            DarklightPresentation.Configure(WorldCamera);
            world = new RenderTexture(1280, 720, 24) { filterMode = FilterMode.Point, antiAliasing = 1, name = "DarklightWorld1280x720" };
            world.Create(); WorldCamera.targetTexture = world;
            WorldCamera.aspect = 1280f / 720f;WorldCamera.allowHDR=false;WorldCamera.allowMSAA=false;WorldCamera.allowDynamicResolution=false;QualitySettings.antiAliasing=0;QualitySettings.anisotropicFiltering=AnisotropicFiltering.Disable;
            InputSystem.onDeviceChange += OnDeviceChange;
        }
        public DarklightCombatCoach Coach {get;private set;}
        void Start()
        {
            Coach=gameObject.AddComponent<DarklightCombatCoach>();Coach.Initialize(this);
            if(PixelHUD!=null&&DarklightCampaign.Instance==null)
            {
                pauseMenu=gameObject.AddComponent<DarklightLabPauseMenu>();pauseMenu.Initialize(this,PixelHUD);
            }
        }
        void Update()
        {
            if (DarklightCampaign.Instance != null) { if (rumbling != null && Time.unscaledTime >= rumbleUntil) StopRumble(); return; }
            if (Input.Menu("Pause").WasPressedThisFrame()) { SetPaused(!Paused); return; }
            if (Paused)
            {
                if(pauseMenu!=null&&pauseMenu.ReadPointer(ref menuSelection,out int clicked)&&clicked>=0){SelectMenu(clicked);return;}
                float axis = Input.Menu("Navigate").ReadValue<Vector2>().y;
                if (Mathf.Abs(axis) < .4f) nextNavigate = 0;
                else if (Time.unscaledTime >= nextNavigate)
                { menuSelection = (menuSelection + (axis > 0 ? 3 : 1)) % 4; nextNavigate = Time.unscaledTime + .22f; }
                if (Input.Menu("Cancel").WasPressedThisFrame()) SetPaused(false);
                else if (Input.Menu("Submit").WasPressedThisFrame()) SelectMenu(menuSelection);
            }
            if (rumbling != null && Time.unscaledTime >= rumbleUntil) StopRumble();
        }
        public void SetPaused(bool value)
        {
            if (value != Paused)
            {
                if (value && Player != null)
                {
                    pausedBody = Player.GetComponent<Rigidbody2D>();
                    if (pausedBody != null)
                    {
                        resumeInterpolation = pausedBody.interpolation;
                        pausedBody.linearVelocity = Vector2.zero;
                        pausedBody.interpolation = RigidbodyInterpolation2D.None;
                        var p = pausedBody.transform.position;
                        pausedBody.transform.position = new Vector3(pausedBody.position.x, pausedBody.position.y, p.z);
                    }
                }
                else if (pausedBody != null)
                {
                    pausedBody.interpolation = resumeInterpolation;
                    pausedBody = null;
                }
            }
            Paused = value; Input.SetGameplay(!value);
            Player?.Model?.SuspendInput(); suppressThrough = Time.frameCount + 1;
            menuSelection = 0; nextNavigate = 0; StopRumble();
        }
        public int PauseSelection=>menuSelection;
        void SelectMenu(int index)
        {
            if (index == 0) SetPaused(false);
            else if (index == 1) ResetEncounter();
            else if(index==3){SceneManager.LoadScene("DarklightCampaign");}
            else { foreach (var target in Targets) if (target != null) target.DefensePractice = !target.DefensePractice; SetPaused(false); }
        }
        public void ResetEncounter() { StopRumble(); SceneManager.LoadScene(gameObject.scene.name); }
        public void Hitstop(float duration) { hitstopUntil = Mathf.Max(hitstopUntil, Time.unscaledTime + duration); }
        public void Feedback(Vector3 location, bool defended)
        {
            if (!Input.UsingGamepad || Gamepad.current == null) return;
            rumbling = Gamepad.current; rumbling.SetMotorSpeeds(defended ? .1f : .18f, defended ? .3f : .12f);
            rumbleUntil = Time.unscaledTime + .08f;
        }
        void StopRumble() { rumbling?.SetMotorSpeeds(0, 0); rumbling = null; }
        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        { if (device == rumbling && (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)) StopRumble(); }
        void OnApplicationFocus(bool focus) { if (!focus && !DarklightRemasterSmoke.Active) { if(DarklightCampaign.Instance!=null) DarklightCampaign.Instance.PauseForFocus(); else SetPaused(true); } }
        void OnApplicationPause(bool paused) { if (paused) { if(DarklightCampaign.Instance!=null) DarklightCampaign.Instance.PauseForFocus(); else SetPaused(true); } }
        void OnDestroy()
        {
            InputSystem.onDeviceChange -= OnDeviceChange; StopRumble(); Input?.Dispose();
            if (WorldCamera != null) WorldCamera.targetTexture = null;
            if (world != null) { world.Release(); Destroy(world); }
            if (Instance == this) Instance = null;
        }
        public void DrawHUD()
        {
            GUI.color = Color.white;
            if (PixelHUD == null)
            {
            GUI.Label(new Rect(16, 8, 500, 22), "DARKLIGHT: REQUIEM · TREINO");
            if (Player != null)
            {
                var health = Player.GetComponent<HealthSystem>();
                GUI.Label(new Rect(16, 30, 350, 22), "VIDA " + Mathf.CeilToInt(health.CurrentHealth) + " / 100   ·   RÉQUIEM");
                GUI.Label(new Rect(16, 50, 430, 22), Player.Model.SkillRemaining > 0 ? "Onda do Véu · " + Player.Model.SkillRemaining.ToString("0.0") + " s" : "Onda do Véu · pronta");
                if (health.IsDead) { GUI.Label(new Rect(245, 145, 220, 35), "O VÉU TE ALCANÇOU"); if (GUI.Button(new Rect(230, 185, 180, 30), "Recomeçar")) ResetEncounter(); }
            }
            foreach (var target in Targets)
            {
                if (target == null) continue;
                Vector3 p = WorldCamera.WorldToScreenPoint(target.transform.position + Vector3.up * 1.7f);p.x*=640f/WorldCamera.pixelWidth;p.y*=360f/WorldCamera.pixelHeight;
                GUI.Label(new Rect(p.x - 42, 360 - p.y, 100, 25), target.Health.IsDead ? "Concluído" : Mathf.CeilToInt(target.Health.CurrentHealth) + " / 150");
            }
            GUI.Label(new Rect(16, 312, 620, 20), Input.UsingGamepad ? "X leve · Y pesado · B esquiva · LB parry · RB onda · START pausa" : "Mouse: leve / pesado · Espaço esquiva · Q parry · F onda · Esc pausa");
            GUI.Label(new Rect(16, 332, 620, 20), Input.UsingGamepad ? "Analógico esquerdo: mover · A reinicia o treino" : "WASD: mover · E reinicia o treino");
            }
            if (DarklightCampaign.Instance != null || !Paused || pauseMenu!=null) return;
            GUI.Box(new Rect(202, 100, 236, 210), "PAUSA");
            string[] labels = { "Retomar", "Reiniciar treino", "Alternar treino de defesa", "Menu principal" };
            for (int i = 0; i < labels.Length; i++)
            { GUI.color = i == menuSelection ? new Color(1, .65f, .7f) : Color.white; if (GUI.Button(new Rect(220, 130 + i * 39, 200, 32), labels[i])) SelectMenu(i); }
            GUI.color = Color.white;
        }
        void OnGUI()
        {
            if (world == null || Input == null) return;
            if (Event.current.type == EventType.Repaint) RepaintCount++;
            GUI.depth = 100;
            if (PixelHUD == null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.blackTexture);
                GUI.DrawTexture(Viewport, world, ScaleMode.StretchToFill, false);
            }
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(Viewport.x, Viewport.y, 0), Quaternion.identity, new Vector3(Viewport.width / 640, Viewport.height / 360, 1));
            DrawHUD(); GUI.matrix = previous;
        }
    }
}
