using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System.Reflection;
using OmniExtractionToolkit;

namespace OmniExtractionToolkit.Features
{
    public static class CheatPatches
    {
        // --- LOOT DAMAGE & REVERSE DAMAGE ---
        [HarmonyPatch(typeof(PhysGrabObjectImpactDetector), "BreakRPC")]
        public static class LootDamage_Patch
        {
            static void Prefix(PhysGrabObjectImpactDetector __instance, ref float valueLost, bool _loseValue)
            {
                ValuableObject valObj = Traverse.Create(__instance).Field<ValuableObject>("valuableObject").Value;
                if (_loseValue && valObj != null)
                {
                    float multiplier = OmniExtractionToolkitPlugin.DamageMultiplier.Value;
                    
                    Traverse tVal = Traverse.Create(valObj);
                    float currentVal = tVal.Field<float>("dollarValueCurrent").Value;
                    float originalVal = tVal.Field<float>("dollarValueOriginal").Value;

                    // 1. Only apply compounding scaling if we are in "Buff Mode" (Multiplier is negative)
                    // If we are doing normal damage, we don't want the items to explode instantly.
                    if (multiplier < 0)
                    {
                        float scalingRatio = (originalVal > 0) ? (currentVal / originalVal) : 1f;
                        valueLost *= scalingRatio * multiplier;
                    }
                    else
                    {
                        valueLost *= multiplier;
                    }

                    // 2. Prevent item value from exceeding $1B
                    float newVal = currentVal - valueLost;
                    if (newVal > 1000000000f)
                    {
                        valueLost = currentVal - 1000000000f;
                    }

                    // 3. Update the Map Total (haulGoalMax) - LIVE FEEDBACK
                    if (RoundDirector.instance != null)
                    {
                        Traverse t = Traverse.Create(RoundDirector.instance);
                        long currentMax = t.Field<int>("haulGoalMax").Value;
                        
                        // valueLost is positive for damage (decreases total) 
                        // and negative for buffs (increases total)
                        long newMax = currentMax - (int)valueLost; 
                        
                        // Safety Clamps: Ensure Map Total stays within 0 and 2.1B (Max Int32)
                        if (newMax > 2100000000) newMax = 2100000000;
                        if (newMax < 0) newMax = 0;

                        t.Field("haulGoalMax").SetValue((int)newMax);
                    }
                }
            }
        }

        // --- INFINITE BATTERY / AMMO ---
        [HarmonyPatch(typeof(RoundDirector), "Update")]
        public static class InfiniteBattery_Patch
        {
            static void Postfix(RoundDirector __instance)
            {
                if (OmniExtractionToolkitPlugin.InfiniteBattery.Value)
                    Traverse.Create(__instance).Field("debugInfiniteBattery").SetValue(true);
            }
        }

        // --- INFINITE STAMINA ---
        [HarmonyPatch(typeof(PlayerController), "Update")]
        public static class InfiniteStamina_Patch
        {
            static void Postfix(PlayerController __instance)
            {
                if (OmniExtractionToolkitPlugin.InfiniteStamina.Value)
                    __instance.DebugEnergy = true;
            }
        }

        // --- SHARED UPGRADES ---
        [HarmonyPatch(typeof(PunManager))]
        public static class SharedUpgrades_Patch
        {
            private static bool _syncing = false;
            private static bool ShouldSync => OmniExtractionToolkitPlugin.ShareUpgrades.Value && !_syncing && PhotonNetwork.IsMasterClient;

            private static void SyncToAll(string _steamID, int value, string rpcMethodName)
            {
                if (PunManager.instance == null) return;
                PhotonView pv = PunManager.instance.GetComponent<PhotonView>();
                if (pv == null) return;
                _syncing = true;
                foreach (PlayerAvatar player in Object.FindObjectsOfType<PlayerAvatar>())
                {
                    string targetID = SemiFunc.PlayerGetSteamID(player);
                    if (!string.IsNullOrEmpty(targetID) && targetID != _steamID)
                    {
                        MethodInfo m = typeof(PunManager).GetMethod(rpcMethodName);
                        if (m != null) m.Invoke(PunManager.instance, new object[] { targetID, value });
                        pv.RPC(rpcMethodName, RpcTarget.Others, new object[] { targetID, value });
                    }
                }
                _syncing = false;
            }

            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerHealth")] static void H_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareHealth.Value) SyncToAll(_steamID, value, "UpgradePlayerHealth"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerEnergy")] static void E_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareEnergy.Value) SyncToAll(_steamID, value, "UpgradePlayerEnergy"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerSprintSpeed")] static void S_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareSprint.Value) SyncToAll(_steamID, value, "UpgradePlayerSprintSpeed"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerGrabStrength")] static void GS_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareGrabStrength.Value) SyncToAll(_steamID, value, "UpgradePlayerGrabStrength"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerGrabRange")] static void GR_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareGrabRange.Value) SyncToAll(_steamID, value, "UpgradePlayerGrabRange"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerExtraJump")] static void J_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareExtraJump.Value) SyncToAll(_steamID, value, "UpgradePlayerExtraJump"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerTumbleLaunch")] static void T_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareTumble.Value) SyncToAll(_steamID, value, "UpgradePlayerTumbleLaunch"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerTumbleClimb")] static void TC_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareTumbleClimb.Value) SyncToAll(_steamID, value, "UpgradePlayerTumbleClimb"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerTumbleWings")] static void TW_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareTumbleWings.Value) SyncToAll(_steamID, value, "UpgradePlayerTumbleWings"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerCrouchRest")] static void CR_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareCrouchRest.Value) SyncToAll(_steamID, value, "UpgradePlayerCrouchRest"); }
            [HarmonyPostfix] [HarmonyPatch("UpgradePlayerThrowStrength")] static void TS_P(string _steamID, int value) { if (ShouldSync && OmniExtractionToolkitPlugin.ShareThrowStrength.Value) SyncToAll(_steamID, value, "UpgradePlayerThrowStrength"); }
        }
    }
}
