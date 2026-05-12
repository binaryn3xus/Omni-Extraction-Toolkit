# Known Issues - Omni-Extraction Toolkit

This list tracks currently identified bugs, logical quirks, and technical limitations.

### 🐛 Current Issues

__None Known Right Now__

### ⚙️ Technical Limitations
* **Physical Transformations:** Changes to scale (Cart Shrink) and mass (Weight Reduction) are applied by the Host to ensure network synchronization. If you are a client and the host doesn't have the mod, items will not shrink.
* **Physical Tracker Range:** The 500m range boost is strictly for the *Valuable* version of the physical Item Tracker tool. Other variants (e.g., Enemy Tracker) remain at vanilla settings.
* **Scan Visualization:** The manual scan (`F`) uses local-only brackets. If another player picks up the item you scanned, the brackets will disappear correctly, but they won't see your ping.

### 📝 To-Do / Future Fixes

__None Right Now__