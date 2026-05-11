using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace OmniExtractionToolkit
{
    [BepInDependency("nickklmao.repoconfig")]
    [BepInPlugin("com.binaryn3xus.repo.omniextraction", "Omni-Extraction Toolkit", "1.0.1")]
    public class OmniExtractionToolkitPlugin : BaseUnityPlugin
    {
        // --- SCANNER SETTINGS ---
        public static ConfigEntry<KeyCode> ScanKey;
        public static ConfigEntry<float> ScanRadius;
        public static ConfigEntry<float> ScanCooldown;
        
        // --- HUD SETTINGS ---
        public static ConfigEntry<float> HUDX;
        public static ConfigEntry<float> HUDY;
        public static ConfigEntry<bool> ShowHeldItem;
        
        // --- SHARED UPGRADES SETTINGS ---
        public static ConfigEntry<bool> ShareUpgrades;
        public static ConfigEntry<bool> ShareHealth;
        public static ConfigEntry<bool> ShareEnergy;
        public static ConfigEntry<bool> ShareSprint;
        public static ConfigEntry<bool> ShareGrabStrength;
        public static ConfigEntry<bool> ShareGrabRange;
        public static ConfigEntry<bool> ShareExtraJump;
        public static ConfigEntry<bool> ShareTumble;
        public static ConfigEntry<bool> ShareTumbleClimb;
        public static ConfigEntry<bool> ShareTumbleWings;
        public static ConfigEntry<bool> ShareCrouchRest;
        public static ConfigEntry<bool> ShareThrowStrength;

        // --- CHEAT SETTINGS ---
        public static ConfigEntry<bool> InfiniteBattery;
        public static ConfigEntry<bool> InfiniteStamina;
        public static ConfigEntry<float> DamageMultiplier;

        public static ManualLogSource ModLogger;

        private void Awake()
        {
            ModLogger = Logger;
            Logger.LogInfo("================================================");
            Logger.LogInfo("      OMNI-EXTRACTION TOOLKIT LOADED!       ");
            Logger.LogInfo("================================================");

            InitConfig();

            Harmony harmony = new Harmony("com.binaryn3xus.repo.omniextraction");
            harmony.PatchAll();
        }

        private void InitConfig()
        {
            // Scanner
            ScanKey = Config.Bind("Scanner", "ScanKey", KeyCode.F, "Key to trigger a manual scan");
            ScanRadius = Config.Bind("Scanner", "ScanRadius", 30f, new ConfigDescription("Radius", new AcceptableValueRange<float>(1f, 100f)));
            ScanCooldown = Config.Bind("Scanner", "ScanCooldown", 5f, new ConfigDescription("Cooldown", new AcceptableValueRange<float>(0.1f, 30f)));

            // HUD
            HUDX = Config.Bind("HUD", "Horizontal Position", 100f, new ConfigDescription("X% (0=Left, 100=Right)", new AcceptableValueRange<float>(0f, 100f)));
            HUDY = Config.Bind("HUD", "Vertical Position", 0f, new ConfigDescription("Y% (0=Bottom, 100=Top)", new AcceptableValueRange<float>(0f, 100f)));
            ShowHeldItem = Config.Bind("HUD", "Show Held Item Value", true, "Show the name and value of the item you are currently holding (Valuables only).");

            // Shared Upgrades
            ShareUpgrades = Config.Bind("Shared Upgrades", "Master Toggle", true);
            ShareHealth = Config.Bind("Shared Upgrades", "Share Health", true);
            ShareEnergy = Config.Bind("Shared Upgrades", "Share Energy", true);
            ShareSprint = Config.Bind("Shared Upgrades", "Share Sprint Speed", true);
            ShareGrabStrength = Config.Bind("Shared Upgrades", "Share Grab Strength", true);
            ShareGrabRange = Config.Bind("Shared Upgrades", "Share Grab Range", true);
            ShareExtraJump = Config.Bind("Shared Upgrades", "Share Extra Jump", true);
            ShareTumble = Config.Bind("Shared Upgrades", "Share Tumble Launch", true);
            ShareTumbleClimb = Config.Bind("Shared Upgrades", "Share Tumble Climb", true);
            ShareTumbleWings = Config.Bind("Shared Upgrades", "Share Tumble Wings", true);
            ShareCrouchRest = Config.Bind("Shared Upgrades", "Share Crouch Rest", true);
            ShareThrowStrength = Config.Bind("Shared Upgrades", "Share Throw Strength", true);

            // Cheats
            InfiniteBattery = Config.Bind("Cheats", "Infinite Battery & Ammo", false);
            InfiniteStamina = Config.Bind("Cheats", "Infinite Stamina", false);
            DamageMultiplier = Config.Bind("Cheats", "Loot Damage Multiplier", 1.0f, new ConfigDescription("Scale impact damage. Negative = increase value!", new AcceptableValueRange<float>(-5f, 10f)));
        }

        public static string GetBindingPath(KeyCode keyCode)
        {
            if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9) return "<Keyboard>/" + keyCode.ToString().Replace("Alpha", "");
            if (keyCode == KeyCode.Mouse0) return "<Mouse>/leftButton";
            if (keyCode == KeyCode.Mouse1) return "<Mouse>/rightButton";
            if (keyCode == KeyCode.Mouse2) return "<Mouse>/middleButton";
            string text = keyCode.ToString();
            if (text.Length > 0) text = char.ToLower(text[0]) + text.Substring(1);
            return "<Keyboard>/" + text;
        }
    }

    public static class ModInput { public const InputKey ScanInputKey = (InputKey)327; }
}
