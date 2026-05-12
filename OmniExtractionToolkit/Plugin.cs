using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace OmniExtractionToolkit
{
    [BepInDependency("nickklmao.repoconfig")]
    [BepInPlugin("com.binaryn3xus.repo.omniextraction", "Omni-Extraction Toolkit", "1.0.3")]
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
        
        // --- TEAM UPGRADES SETTINGS (HOST ONLY) ---
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

        // --- CHEAT SETTINGS (LOCAL) ---
        public static ConfigEntry<bool> InfiniteBattery;
        public static ConfigEntry<bool> InfiniteStamina;
        public static ConfigEntry<float> JumpHeightMultiplier;

        // --- CHEAT SETTINGS (HOST ONLY) ---
        public static ConfigEntry<float> DamageMultiplier;

        // --- CART SHRINK SETTINGS (HOST ONLY) ---
        public static ConfigEntry<bool> EnableCartShrink;
        public static ConfigEntry<float> CartShrinkFactor;
        public static ConfigEntry<float> CartShrinkSpeed;
        public static ConfigEntry<float> CartShrinkFieldSize;
        public static ConfigEntry<bool> EnableWeightReduction;

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

            // Team Upgrades (Host Only)
            ShareUpgrades = Config.Bind("Team Upgrades (Host Only)", "Master Toggle", false, "When enabled, any upgrade anyone buys is shared with everyone.");
            ShareHealth = Config.Bind("Team Upgrades (Host Only)", "Health Upgrade", true);
            ShareEnergy = Config.Bind("Team Upgrades (Host Only)", "Energy Upgrade", true);
            ShareSprint = Config.Bind("Team Upgrades (Host Only)", "Sprint Speed Upgrade", true);
            ShareGrabStrength = Config.Bind("Team Upgrades (Host Only)", "Grab Strength Upgrade", true);
            ShareGrabRange = Config.Bind("Team Upgrades (Host Only)", "Grab Range Upgrade", true);
            ShareExtraJump = Config.Bind("Team Upgrades (Host Only)", "Extra Jump Upgrade", true);
            ShareTumble = Config.Bind("Team Upgrades (Host Only)", "Tumble Launch Upgrade", true);
            ShareTumbleClimb = Config.Bind("Team Upgrades (Host Only)", "Tumble Climb Upgrade", true);
            ShareTumbleWings = Config.Bind("Team Upgrades (Host Only)", "Tumble Wings Upgrade", true);
            ShareCrouchRest = Config.Bind("Team Upgrades (Host Only)", "Crouch Rest Upgrade", true);
            ShareThrowStrength = Config.Bind("Team Upgrades (Host Only)", "Throw Strength Upgrade", true);

            // Cheats (Client-Side)
            InfiniteBattery = Config.Bind("Cheats (Client-Side)", "Infinite Battery & Ammo", false, "All tools and weapons stay at 100% charge.");
            InfiniteStamina = Config.Bind("Cheats (Client-Side)", "Infinite Stamina", false, "Never run out of energy while sprinting.");
            JumpHeightMultiplier = Config.Bind("Cheats (Client-Side)", "Jump Height Multiplier", 1.0f, new ConfigDescription("Increase player jump height.", new AcceptableValueRange<float>(1f, 5f)));

            // Cheats (Host Only)
            DamageMultiplier = Config.Bind("Cheats (Host Only)", "Loot Damage Multiplier", 1.0f, new ConfigDescription("Negative = increase value! Only works if YOU are the host.", new AcceptableValueRange<float>(-5f, 10f)));

            // Cart Shrink (Host Only)
            EnableCartShrink = Config.Bind("Cart Shrink (Host Only)", "Enable Cart Shrink", false, "Shrink items when they are placed in a cart.");
            CartShrinkFactor = Config.Bind("Cart Shrink (Host Only)", "Shrink Factor", 1.0f, new ConfigDescription("How much to shrink items (0.1 = tiny, 1.0 = normal).", new AcceptableValueRange<float>(0.1f, 1f)));
            CartShrinkSpeed = Config.Bind("Cart Shrink (Host Only)", "Shrink Speed", 3.0f, new ConfigDescription("How fast items transition between sizes (1 = slow, 20 = instant).", new AcceptableValueRange<float>(1f, 20f)));
            CartShrinkFieldSize = Config.Bind("Cart Shrink (Host Only)", "Shrink Field Size", 1.25f, new ConfigDescription("Multiplier for the storage box detection area (1.0 = normal).", new AcceptableValueRange<float>(0.1f, 5f)));
            EnableWeightReduction = Config.Bind("Cart Shrink (Host Only)", "Reduce Item Weight", false, "When enabled, weight is reduced proportionally to the Shrink Factor.");
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
