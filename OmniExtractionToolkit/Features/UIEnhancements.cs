using HarmonyLib;
using UnityEngine;
using TMPro;
using OmniExtractionToolkit;

namespace OmniExtractionToolkit.Features
{
    public static class UIPatches
    {
        // --- ITEM HOVER VALUE ---
        [HarmonyPatch(typeof(ItemInfoUI), "ItemInfoText")]
        public static class ItemInfoUI_Patch
        {
            static void Prefix(ref string message, ItemAttributes _itemAttributes)
            {
                if (_itemAttributes != null)
                {
                    // 1. Strictly limit to true Valuables (those that can be extracted)
                    ValuableObject v = _itemAttributes.GetComponentInParent<ValuableObject>();
                    
                    // Ensure it's NOT an equippable tool/weapon unless we are in the shop
                    bool isEquippable = _itemAttributes.GetComponent<ItemEquippable>() != null;
                    bool isShop = SemiFunc.RunIsShop();

                    if (v != null && (!isEquippable || isShop))
                    {
                        // WYSIWYG: Showing raw units to match vanilla HUD/Goal numbers
                        float val = Traverse.Create(v).Field<float>("dollarValueCurrent").Value;
                        message += $" <color=#00FF00>(${val:F0})</color>";
                        return;
                    }

                    // 2. Show for other items (Tools/Guns) ONLY in the Shop
                    if (isShop)
                    {
                        int val = Traverse.Create(_itemAttributes).Field<int>("value").Value;
                        if (val > 0)
                        {
                            message += $" <color=#00FF00>(${val:F0})</color>";
                        }
                    }
                }
            }
        }

        // --- VALUABLE PROP HOVER VALUE ---
        [HarmonyPatch(typeof(PhysGrabber), "RayCheck")]
        public static class PhysGrabber_RayCheck_Patch
        {
            static void Prefix(PhysGrabber __instance)
            {
                Traverse t = Traverse.Create(__instance);
                t.Field("currentlyLookingAtPhysGrabObject").SetValue(null);
                t.Field("currentlyLookingAtItemAttributes").SetValue(null);
            }

            static void Postfix(PhysGrabber __instance)
            {
                Traverse tGrabber = Traverse.Create(__instance);
                PhysGrabObject o = tGrabber.Field<PhysGrabObject>("currentlyLookingAtPhysGrabObject").Value;
                ItemAttributes attr = tGrabber.Field<ItemAttributes>("currentlyLookingAtItemAttributes").Value;

                if (o != null && attr == null)
                {
                    ValuableObject v = o.GetComponent<ValuableObject>();
                    if (v != null)
                    {
                        float val = Traverse.Create(v).Field<float>("dollarValueCurrent").Value;
                        string name = o.gameObject.name.Replace("(Clone)", "").Trim();
                        SemiFunc.UIItemInfoText(null, $"{name} <color=#00FF00>(${val:F0})</color>");
                    }
                }
            }
        }
    }

    // --- VANILLA BUG FIXES ---
    // These patches fix errors in the base game that are triggered by modded playstyles.
    public static class VanillaFixPatches
    {
        [HarmonyPatch(typeof(ShopRadio), "Update")]
        public static class ShopRadio_Update_Patch
        {
            static bool Prefix(ShopRadio __instance)
            {
                // Fix for "Do not create your own module instances" error.
                // This happens in vanilla when a radio is destroyed but the Update loop keeps running.
                
                PhysGrabObject pObj = Traverse.Create(__instance).Field<PhysGrabObject>("physGrabObject").Value;
                if (pObj != null && pObj.dead) return false; // Stop updating if dead

                ParticleSystem smoke = Traverse.Create(__instance).Field<ParticleSystem>("smoke").Value;
                if (smoke == null || smoke.gameObject == null) return false; // Stop if smoke is missing/destroyed

                return true;
            }
        }
    }
}
