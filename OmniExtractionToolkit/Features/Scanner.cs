using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using OmniExtractionToolkit;

namespace OmniExtractionToolkit.Features
{
    public static class ScannerPatches
    {
        public static float lastScanTime = -100f;

        // --- MANUAL SCAN TRIGGER ---
        [HarmonyPatch(typeof(PlayerController), "Update")]
        public static class PlayerScan_Patch
        {
            static void Postfix()
            {
                bool keyPressed = SemiFunc.InputDown(ModInput.ScanInputKey) || Input.GetKeyDown(OmniExtractionToolkitPlugin.ScanKey.Value);

                if (keyPressed)
                {
                    if (Time.time - lastScanTime < OmniExtractionToolkitPlugin.ScanCooldown.Value) return;
                    DoLocalScan();
                    lastScanTime = Time.time;
                }
            }
        }

        public static void DoLocalScan()
        {
            if (PlayerController.instance == null) return;
            Vector3 pos = PlayerController.instance.transform.position;
            float radius = OmniExtractionToolkitPlugin.ScanRadius.Value;
            int count = 0;

            Collider[] colliders = Physics.OverlapSphere(pos, radius, SemiFunc.LayerMaskGetPhysGrabObject());
            foreach (Collider col in colliders)
            {
                ValuableObject valObj = col.GetComponentInParent<ValuableObject>();
                if (valObj != null)
                {
                    PhysGrabObject pObj = Traverse.Create(valObj).Field<PhysGrabObject>("physGrabObject").Value;
                    if (pObj != null)
                    {
                        ValuableDiscover.instance.New(pObj, ValuableDiscoverGraphic.State.Discover, null);
                        count++;
                    }
                }
                else
                {
                    ValuableDiscoverCustom valCust = col.GetComponentInParent<ValuableDiscoverCustom>();
                    if (valCust != null)
                    {
                        PhysGrabObject pObj = Traverse.Create(valCust).Field<PhysGrabObject>("physGrabObject").Value;
                        if (pObj != null)
                        {
                            ValuableDiscover.instance.New(pObj, ValuableDiscoverGraphic.State.Discover, valCust);
                            count++;
                        }
                    }
                }
            }

            if (count > 0) 
                SemiFunc.UIItemInfoText(null, $"Area scanned. (Found {count} items)");
            else
                SemiFunc.UIItemInfoText(null, "Area scanned. No new items found.");
        }

        // --- BUILT-IN TRACKER ENHANCEMENT ---
        [HarmonyPatch(typeof(ItemTracker), "ValuableTarget")]
        public static class TrackerRange_Patch
        {
            static bool Prefix(ItemTracker __instance)
            {
                object typeObj = Traverse.Create(__instance).Field("trackerType").GetValue();
                if (typeObj == null || typeObj.ToString() != "Valuable")
                {
                    return true;
                }

                Traverse t = Traverse.Create(__instance);
                Transform nozzle = t.Field("nozzleTransform").GetValue<Transform>();
                if (nozzle == null) return true;

                Vector3 pos = nozzle.position;
                Collider[] cols = Physics.OverlapSphere(pos, 500f, SemiFunc.LayerMaskGetPhysGrabObject());
                PhysGrabObject bestO = null; ValuableObject bestV = null; ValuableDiscoverCustom bestC = null; float minD = float.MaxValue;

                foreach (Collider col in cols)
                {
                    ValuableObject v = col.GetComponentInParent<ValuableObject>(); ValuableDiscoverCustom c = null; PhysGrabObject p = null;
                    if (v != null) { 
                        if (!Traverse.Create(v).Field<bool>("discovered").Value) 
                            p = Traverse.Create(v).Field<PhysGrabObject>("physGrabObject").Value; 
                    }
                    else { 
                        c = col.GetComponentInParent<ValuableDiscoverCustom>(); 
                        if (c != null && !Traverse.Create(c).Field<bool>("discovered").Value) 
                            p = Traverse.Create(c).Field<PhysGrabObject>("physGrabObject").Value; 
                    }
                    
                    if (p != null) { 
                        float d = Vector3.Distance(pos, p.midPoint); 
                        PhysGrabObjectImpactDetector id = Traverse.Create(p).Field<PhysGrabObjectImpactDetector>("impactDetector").Value; 
                        if (d < minD && !p.grabbed && !(id != null && id.inCart)) { minD = d; bestO = p; bestV = v; bestC = c; } 
                    }
                }

                if (bestO != null) { 
                    t.Field("currentTarget").SetValue(bestO.transform); 
                    t.Field("currentTargetPhysGrabObject").SetValue(bestO); 
                    t.Field("currentTargetValuable").SetValue(bestV); 
                    t.Field("currentTargetValuableCustom").SetValue(bestC); 
                    t.Field("hasTarget").SetValue(true); 
                    PhotonView pv = Traverse.Create(bestO).Field<PhotonView>("photonView").Value; 
                    if (pv != null) AccessTools.Method(typeof(ItemTracker), "SetTarget").Invoke(__instance, new object[] { pv.ViewID }); 
                } else t.Field("hasTarget").SetValue(false);

                return false;
            }
        }
    }
}
