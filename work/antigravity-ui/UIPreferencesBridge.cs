// DARKLIGHT: REQUIEM — UI Preferences & Decoupled Navigation Bridge
// Antigravity Support Engineering — 2026-11-04
// Reusable contracts for settings persistence, pause rules, and navigation state.

using System;
using UnityEngine;

namespace Darklight.UI
{
    [Serializable]
    public struct GamePreferences
    {
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public bool reducedFlashes;
        public bool showMinimap;
        public int displayResolutionIndex;
        public bool fullscreen;

        public static GamePreferences Default()
        {
            return new GamePreferences
            {
                masterVolume = 1.0f,
                musicVolume = 0.5f,
                sfxVolume = 0.7f,
                reducedFlashes = false,
                showMinimap = true,
                displayResolutionIndex = 0,
                fullscreen = true
            };
        }
    }

    public static class PreferencesManager
    {
        private const string PrefsPrefix = "Darklight.Settings.";

        public static void Save(GamePreferences prefs)
        {
            PlayerPrefs.SetFloat(PrefsPrefix + "MasterVolume", prefs.masterVolume);
            PlayerPrefs.SetFloat(PrefsPrefix + "MusicVolume", prefs.musicVolume);
            PlayerPrefs.SetFloat(PrefsPrefix + "SfxVolume", prefs.sfxVolume);
            PlayerPrefs.SetInt(PrefsPrefix + "ReducedFlashes", prefs.reducedFlashes ? 1 : 0);
            PlayerPrefs.SetInt(PrefsPrefix + "ShowMinimap", prefs.showMinimap ? 1 : 0);
            PlayerPrefs.SetInt(PrefsPrefix + "Fullscreen", prefs.fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static GamePreferences Load()
        {
            var p = GamePreferences.Default();
            p.masterVolume = PlayerPrefs.GetFloat(PrefsPrefix + "MasterVolume", p.masterVolume);
            p.musicVolume = PlayerPrefs.GetFloat(PrefsPrefix + "MusicVolume", p.musicVolume);
            p.sfxVolume = PlayerPrefs.GetFloat(PrefsPrefix + "SfxVolume", p.sfxVolume);
            p.reducedFlashes = PlayerPrefs.GetInt(PrefsPrefix + "ReducedFlashes", p.reducedFlashes ? 1 : 0) == 1;
            p.showMinimap = PlayerPrefs.GetInt(PrefsPrefix + "ShowMinimap", p.showMinimap ? 1 : 0) == 1;
            p.fullscreen = PlayerPrefs.GetInt(PrefsPrefix + "Fullscreen", p.fullscreen ? 1 : 0) == 1;
            return p;
        }
    }

    public enum MenuState
    {
        Closed,
        MainMenu,
        PauseMenu,
        SettingsMenu,
        SkinSelectMenu,
        JournalMenu,
        DeathMenu,
        VictoryMenu
    }

    public interface IMenuNavigationController
    {
        MenuState CurrentState { get; }
        bool IsPaused { get; }
        bool OpenMenu(MenuState target);
        void CloseMenu();
        void TogglePause();
    }
}

