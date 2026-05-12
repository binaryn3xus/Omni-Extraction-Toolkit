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
            static void Prefix(PhysGrabObjectImpactDetector __instance, ref float valueLost, bool _loseValue, PhotonMessageInfo _info)
            {
                // AUTHORITY FIX: Only the Host's multiplier and math should matter.
                if (PhotonNetwork.IsMasterClient && _info.Sender.IsMasterClient && _loseValue)
                {
                    ValuableObject valObj = Traverse.Create(__instance).Field<ValuableObject>("valuableObject").Value;
                    if (valObj != null)
                    {
                        float multiplier = OmniExtractionToolkitPlugin.DamageMultiplier.Value;
                        Traverse tVal = Traverse.Create(valObj);
                        float currentVal = tVal.Field<float>("dollarValueCurrent").Value;
                        float originalVal = tVal.Field<float>("dollarValueOriginal").Value;

                        if (multiplier < 0)
                        {
                            float scalingRatio = (originalVal > 0) ? (currentVal / originalVal) : 1f;
                            valueLost *= scalingRatio * multiplier;
                        }
                        else
                        {
                            valueLost *= multiplier;
                        }

                        float newVal = currentVal - valueLost;
                        if (newVal > 1000000000f)
                        {
                            valueLost = currentVal - 1000000000f;
                        }

                        if (RoundDirector.instance != null)
                        {
                            Traverse t = Traverse.Create(RoundDirector.instance);
                            long currentMax = t.Field<int>("haulGoalMax").Value;
                            long newMax = currentMax - (int)valueLost; 
                            
                            if (newMax > 2100000000) newMax = 2100000000;
                            if (newMax < 0) newMax = 0;

                            t.Field("haulGoalMax").SetValue((int)newMax);
                        }
                    }
                }
            }
        }

        // --- JUMP HEIGHT CHEAT ---
        [HarmonyPatch(typeof(PlayerController), "Update")]
        public static class JumpHeight_Patch
        {
            static void Postfix(PlayerController __instance)
            {
                // Correct field name is JumpForce (public)
                float originalJump = 20f; // Game default
                __instance.JumpForce = originalJump * OmniExtractionToolkitPlugin.JumpHeightMultiplier.Value;
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
