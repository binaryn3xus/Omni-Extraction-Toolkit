using HarmonyLib;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using OmniExtractionToolkit;

namespace OmniExtractionToolkit.Features
{
    // --- HUD COMPONENT ---
    public class EnhancedHUD : MonoBehaviour
    {
        private TextMeshProUGUI mapText;
        private TextMeshProUGUI heldItemText;
        private GameObject cooldownBar;
        private RectTransform cooldownFill;
        private TextMeshProUGUI cooldownLabel;
        private float lastX, lastY;

        private void Start() { StartCoroutine(CreateUI()); }

        private IEnumerator CreateUI()
        {
            yield return new WaitForSeconds(1.0f);
            TMP_FontAsset font = null;
            if (ItemInfoUI.instance != null)
            {
                var tmp = ItemInfoUI.instance.GetComponent<TextMeshProUGUI>();
                if (tmp != null) font = tmp.font;
            }
            RectTransform canvasRect = HUDCanvas.instance.GetComponent<RectTransform>();

            // 1. Map Tracker
            GameObject go = new GameObject("EnhancedMapTracker");
            go.transform.SetParent(canvasRect, false);
            mapText = go.AddComponent<TextMeshProUGUI>();
            if (font != null) mapText.font = font;
            mapText.fontSize = 20;

            // 2. Held Item Text
            GameObject heldGo = new GameObject("EnhancedHeldItem");
            heldGo.transform.SetParent(canvasRect, false);
            heldItemText = heldGo.AddComponent<TextMeshProUGUI>();
            if (font != null) heldItemText.font = font;
            heldItemText.fontSize = 22;
            heldItemText.alignment = TextAlignmentOptions.Bottom;
            RectTransform heldRt = heldGo.GetComponent<RectTransform>();
            heldRt.anchorMin = heldRt.anchorMax = new Vector2(0.5f, 0);
            heldRt.pivot = new Vector2(0.5f, 0);
            heldRt.anchoredPosition = new Vector2(0, 50);
            heldRt.sizeDelta = new Vector2(600, 50);
            
            // 3. Cooldown Bar Container
            cooldownBar = new GameObject("EnhancedCooldownBar");
            cooldownBar.transform.SetParent(canvasRect, false);
            cooldownBar.AddComponent<RectTransform>();
            Image bg = cooldownBar.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(cooldownBar.transform, false);
            cooldownFill = fill.AddComponent<RectTransform>();
            cooldownFill.anchorMin = Vector2.zero;
            cooldownFill.anchorMax = new Vector2(0, 1);
            cooldownFill.pivot = new Vector2(0, 0.5f);
            cooldownFill.anchoredPosition = Vector2.zero;
            cooldownFill.sizeDelta = Vector2.zero;
            Image img = fill.AddComponent<Image>();
            img.color = new Color(1f, 0.84f, 0f, 1f); // Gold

            // Label
            GameObject lblGo = new GameObject("Label");
            lblGo.transform.SetParent(cooldownBar.transform, false);
            cooldownLabel = lblGo.AddComponent<TextMeshProUGUI>();
            if (font != null) cooldownLabel.font = font;
            cooldownLabel.fontSize = 11;
            cooldownLabel.text = "RECHARGING SCANNER";

            UpdatePosition();
            cooldownBar.SetActive(false);
        }

        private void UpdatePosition()
        {
            if (mapText == null || cooldownBar == null) return;
            float xPct = OmniExtractionToolkitPlugin.HUDX.Value / 100f;
            float yPct = OmniExtractionToolkitPlugin.HUDY.Value / 100f;

            RectTransform mapRt = mapText.GetComponent<RectTransform>();
            mapRt.anchorMin = mapRt.anchorMax = mapRt.pivot = new Vector2(xPct, yPct);
            float xPadding = Mathf.Lerp(20, -20, xPct);
            float yPadding = Mathf.Lerp(20, -20, yPct);
            mapRt.anchoredPosition = new Vector2(xPadding, yPadding);

            if (xPct < 0.33f) mapText.alignment = TextAlignmentOptions.Left;
            else if (xPct > 0.66f) mapText.alignment = TextAlignmentOptions.Right;
            else mapText.alignment = TextAlignmentOptions.Center;

            RectTransform cdRt = cooldownBar.GetComponent<RectTransform>();
            cdRt.anchorMin = cdRt.anchorMax = cdRt.pivot = new Vector2(xPct, yPct);
            cdRt.sizeDelta = new Vector2(135, 8);
            float verticalOffset = (yPct > 0.5f) ? -110f : 110f;
            cdRt.anchoredPosition = new Vector2(xPadding, yPadding + verticalOffset);

            RectTransform lblRt = cooldownLabel.GetComponent<RectTransform>();
            lblRt.anchorMin = lblRt.anchorMax = new Vector2(xPct, yPct > 0.5f ? 0 : 1);
            lblRt.pivot = new Vector2(xPct, 0);
            lblRt.anchoredPosition = new Vector2(0, yPct > 0.5f ? -24 : 4);
            lblRt.sizeDelta = new Vector2(200, 18);
            if (xPct < 0.33f) cooldownLabel.alignment = TextAlignmentOptions.Left;
            else if (xPct > 0.66f) cooldownLabel.alignment = TextAlignmentOptions.Right;
            else cooldownLabel.alignment = TextAlignmentOptions.Center;

            lastX = OmniExtractionToolkitPlugin.HUDX.Value;
            lastY = OmniExtractionToolkitPlugin.HUDY.Value;
        }

        private void Update()
        {
            bool isGameMain = GameDirector.instance != null && GameDirector.instance.currentState == GameDirector.gameState.Main;
            bool isMenuClosed = true;
            if (MenuManager.instance != null) isMenuClosed = Traverse.Create(MenuManager.instance).Field<int>("currentMenuState").Value == 1;
            bool show = isGameMain && isMenuClosed && !SemiFunc.MenuLevel();

            if (lastX != OmniExtractionToolkitPlugin.HUDX.Value || lastY != OmniExtractionToolkitPlugin.HUDY.Value) UpdatePosition();

            // --- 1. Map Stats ---
            if (mapText != null) {
                mapText.gameObject.SetActive(show);
                if (show && RoundDirector.instance != null) {
                    Traverse t = Traverse.Create(RoundDirector.instance);
                    int current = t.Field("currentHaul").GetValue<int>();
                    int total = t.Field("haulGoalMax").GetValue<int>();
                    int goal = t.Field("haulGoal").GetValue<int>();
                    
                    // REVERTED: Matching raw units to vanilla HUD ($1,327 etc)
                    mapText.text = $"<color=white>Haul: <color=yellow>${current}</color> / Goal: ${goal}\nMap Total: <color=green>${total}</color></color>";
                } else {
                    mapText.text = ""; 
                }
            }

            // --- 2. Held Item Value (Valuables Only) ---
            if (heldItemText != null)
            {
                PhysGrabObject grabbedObj = null;
                if (PhysGrabber.instance != null)
                    grabbedObj = Traverse.Create(PhysGrabber.instance).Field<PhysGrabObject>("grabbedPhysGrabObject").Value;

                bool isValuable = false;
                float val = 0;
                string name = "";

                if (grabbedObj != null)
                {
                    ValuableObject v = grabbedObj.GetComponent<ValuableObject>();
                    bool isEquippable = grabbedObj.GetComponent<ItemEquippable>() != null;
                    bool isShop = SemiFunc.RunIsShop();

                    if (v != null && (!isEquippable || isShop))
                    {
                        isValuable = true;
                        name = grabbedObj.gameObject.name.Replace("(Clone)", "").Trim();
                        val = Traverse.Create(v).Field<float>("dollarValueCurrent").Value;
                    }
                }

                bool showHeld = show && OmniExtractionToolkitPlugin.ShowHeldItem.Value && isValuable;
                heldItemText.gameObject.SetActive(showHeld);
                if (showHeld)
                {
                    heldItemText.text = $"{name} <color=#00FF00>(${val:F0})</color>";
                } else {
                    heldItemText.text = "";
                }
            }

            // --- 3. Cooldown Bar ---
            if (cooldownBar != null) {
                float time = Time.time - ScannerPatches.lastScanTime;
                float cd = OmniExtractionToolkitPlugin.ScanCooldown.Value;
                if (show && time < cd) {
                    cooldownBar.SetActive(true);
                    cooldownFill.anchorMax = new Vector2(time / cd, 1);
                } else {
                    cooldownBar.SetActive(false);
                }
            }
        }
    }

    // --- HUD PATCHES ---
    [HarmonyPatch(typeof(HUDCanvas), "Awake")]
    public static class HUDCanvas_Patch
    {
        static void Postfix(HUDCanvas __instance)
        {
            if (__instance.gameObject.GetComponent<EnhancedHUD>() == null)
                __instance.gameObject.AddComponent<EnhancedHUD>();
        }
    }
}
