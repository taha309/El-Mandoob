# El Mandoob — Third-Party Asset Audit

This file tracks asset/licensing work required before a public or commercial release.

## Verified code/package licenses

### Delivery Boy upstream code
- Source: https://github.com/phamson02/DeliveryBoy-UnityGame
- Repository license: MIT
- Action: retain the upstream MIT notice and attribution.

### RTL Text Mesh Pro
- Package: `com.nosuchstudio.rtltmpro`
- Pinned version: `v3.4.5`
- Source: https://github.com/hk1ll3r/RTLTMPro
- License: MIT
- Purpose: Arabic shaping / right-to-left support.

### Unity packages
Packages from Unity's package registry remain subject to their applicable Unity package/software terms. They are not relicensed by this repository.

## Imported upstream content that still needs source-by-source verification

The upstream project includes folders/assets whose individual redistribution rights should be confirmed before release. Do not assume that the upstream repository's MIT code license automatically covers every imported asset.

### High-priority audit list
- `Assets/City Pack/`
- `Assets/Free Pixel Army/`
- `Assets/Joystick Pack/`
- `Assets/Tiny RPG Forest/`
- `Assets/menu/Font/`
- `Assets/music/`
- menu/background/character images under `Assets/menu/`
- sprites and vehicle/environment art not created specifically for El Mandoob

## Release policy

Before release, every retained third-party asset must have one of the following recorded here:

1. a license that allows the intended redistribution/commercial use, with attribution requirements satisfied; or
2. written permission from the rights holder; or
3. replacement with an original or clearly licensed asset.

Assets with unclear provenance should be replaced rather than guessed safe.

## Arabic font

The prototype currently creates a dynamic TextMeshPro font from an Arabic-capable system font at runtime. This is useful for desktop development but is not the final shipping solution.

Before mobile/public release:
- add a bundled Arabic-capable font with a suitable redistribution license;
- generate a TextMeshPro font asset containing the required Arabic glyphs;
- switch the runtime presentation layer to prefer that bundled asset;
- record the font's license and attribution here.

## El Mandoob original content

The Egyptian Arabic dialogue, customer/business data, economy/progression code, story-order system, garage UI, runtime HUD and related El Mandoob-specific code added on `dev/el-mandoob` are new derivative-project work. They do not remove the obligation to preserve licenses/notices for upstream material that remains in the project.
