# Known Issues - Omni-Extraction Toolkit

This list tracks currently identified bugs, logical quirks, and technical limitations.

### 🐛 Current Issues
* **Internal Unit Overflow:** While we have implemented a $1B cap, extremely rapid damage/buffing ticks (e.g., slamming an item 100 times per second) might theoretically cause the `dollarValueCurrent` to briefly exceed the cap before the next RPC sync.
* **UI Overlap:** On ultra-wide monitors or custom resolutions, the 0-100% slider might require manual adjustment to avoid overlapping with native game messages (e.g., "Round Starting").
* **Host Requirement Clarity:** Features like *Shared Upgrades* and *Loot Damage Multipliers* do not provide a "Warning: Not Host" message yet; they simply remain inactive when playing in someone else's lobby.
* **Loot Damage Multipliers** - Out of sync of all player for some reason. Need to investigate

### ⚙️ Technical Limitations
* **Physical Tracker Range:** The 500m range boost is strictly for the *Valuable* version of the physical Item Tracker tool. Other variants (e.g., Enemy Tracker) remain at vanilla settings.
* **Scan Visualization:** The manual scan (`F`) uses local-only brackets. If another player picks up the item you scanned, the brackets will disappear correctly, but they won't see your ping.

### 📝 To-Do / Future Fixes
* [ ] Add Interaction Speed multiplier to Cheats menu.
* [ ] Implement "Host Only" visual warning for multipliers.