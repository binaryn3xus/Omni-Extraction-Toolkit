# Omni-Extraction Toolkit - The Ultimate R.E.P.O. Mod

The **Omni-Extraction Toolkit** is an all-in-one suite designed to modernize the R.E.P.O. interface and gameplay. It combines tactical scanning, dynamic HUD tracking, and team-based synchronization into a single, high-performance package.

It is modular and highly customizable—use the **REPOConfig** menu in-game to tailor it to your playstyle.

---

## ✨ Main Features

### Tactical Gold Scanner
Stop hunting for that last tiny plate. Press **`F`** at any time to ping every valuable and cosmetic box in range.

* **Gold Brackets:** Instantly highlights loot with the game's native discovery effect.
* **Team Friendly:** These pings are local to your screen only, so you won't clutter your friends' HUDs.
* **Tracker Overhaul:** If you are using the in-game Valuable Tracker tool, its range is boosted to 500m, allowing you to track loot through walls across the entire map.

### Dynamic HUD & Map Tracking
Keep your eyes on the job, not the truck monitor.

* **Real-time Stats:** Track your current **Haul**, the **Round Goal**, and the **Map Total** (potential value of all loot on the level) at a glance.
* **Held Item Readout:** When you grab a valuable, its name and price pop up at the bottom of the screen. (Hidden for weapons and tools to keep your view clear).
* **Clean Transitions:** The HUD automatically hides during loading screens, round intros, or whenever you open a menu.
* **Currency Mode:** A toggle in the settings lets you choose between raw "units" or "Actual Currency" (what ends up in your bank account).

### Shared Lobby Upgrades
Progress as a team. When anyone in the lobby buys an upgrade (Health, Stamina, Speed, etc.), **every player receives it instantly.**

* Includes individual stat toggles so you can decide exactly what to share.

---

## 😈 Gameplay Tweaks (Cheats)

* **Reverse Damage (Profit Buffing):** Set the "Loot Damage Multiplier" to a negative number (e.g., -2.0). Now, slamming items against walls actually **increases their value**. Turn a cheap $100 plate into a massive jackpot just by beating it up. (Value is safely capped to prevent integer overflows).
* **Infinite Resources:** Toggleable infinite battery/ammo for all tools and weapons, and infinite player stamina for endless sprinting.

---

## 🛠️ Customization

Everything is handled through the **REPOConfig** menu in-game.

* **HUD Fine-Tuning:** Use 0-100% sliders to move the HUD anywhere on your screen.
    * **Horizontal:** 0 (Left) to 100 (Right). **Default: 100**
    * **Vertical:** 0 (Bottom) to 100 (Top). **Default: 0**
* **Scanner Control:** Adjust your scan radius (1m–100m) and cooldown times.
* **Modular Toggles:** Enable or disable every feature of the toolkit independently.

---

## 📥 Installation

1. Ensure you have **BepInEx** and **REPOConfig** installed.
2. Drop `OmniExtractionToolkit.dll` into your `REPO/BepInEx/plugins/` folder.
3. Load up and start extracting.
