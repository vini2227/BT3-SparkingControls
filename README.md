[English](README.md) · [Français](docs/i18n/README.fr.md) · [Italiano](docs/i18n/README.it.md) · [Deutsch](docs/i18n/README.de.md)

[Español (España)](docs/i18n/README.es.md) · [العربية](docs/i18n/README.ar.md) · [Bahasa Indonesia](docs/i18n/README.id.md) · [Español (Latinoamérica)](docs/i18n/README.es-419.md)

[Português (Brasil)](docs/i18n/README.pt.md) · [Polski](docs/i18n/README.pl.md) · [Русский](docs/i18n/README.ru.md) · [简体中文](docs/i18n/README.zh-Hans.md)

[繁體中文](docs/i18n/README.zh-Hant.md) · [日本語](docs/i18n/README.ja.md) · [한국어](docs/i18n/README.ko.md) · [ไทย](docs/i18n/README.th.md)

---

<img src="desenvolvimento/assets/dragon-ball-three-stars.png" alt="BT3 Sparking Controls" width="86">

# BT3 Sparking Controls

**1.0.3 Beta — Controls and Setup Guide**

## 1.0.3 Beta — Changes

- Hold R2 + L1 for upward vertical Z Dash; R2 + L2 for downward vertical
  Z Dash. L1/L2 alone still ascend/descend normally. Their original double-tap
  fast-movement route is disabled. A rapid second ordinary press may wait for
  BT3's seven-frame double-tap window to expire before normal movement resumes.
- Release Circle while continuing to hold R1 to request exit from the counter
  stance and resume guard without another R1 press. Short and long holds work;
  the helper waits for the game's actual exit rather than a fixed release pulse.
  It still respects BT3's native animation exit window, without forced cancels.
- Attack, transformation, fusion and switching menus now fade in over 100 ms
  and out over 80 ms. Quick reopening reverses the fade smoothly. This is a
  visual effect only; it adds no input wait. Loss of focus clears immediately.
- Linux HUD presentation updates changed/animated frames immediately.
- Current tests and all 16 language guides were updated. Settings are preserved.

[Full changelog](CHANGELOG.md)

## Downloads and demonstration

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-1.0.3-Beta-Windows.zip) · [**Linux x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-1.0.3-Beta-Linux-x64.zip) · [**1.0.3 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.0.3-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/SHA256SUMS.txt)

## About

A Sparking! ZERO-inspired control layout and battle HUD for Tenkaichi3Decomp. BT3's original animations, damage, Ki costs, range, character abilities and combat timing windows still apply. This does not reproduce every ZERO mechanic. The game and its assets are not included.

## Windows installation

1. Extract BT3-SparkingControls-1.0.3-Beta-Windows.zip.
2. Run BT3-SparkingControls.exe.
3. Click Browse and select Tenkaichi3Decomp.exe in your game installation.
4. Choose your language, panel placement and HUD preferences.
5. Click Save and start. Always launch the game through the helper to use it.

The Windows EXE includes its helper resources. For normal use, you may share just BT3-SparkingControls.exe; the recipient still needs their own game. The full ZIP also provides source code, documentation and the AGPL-3.0 license. No modloader, game recompilation or replacement of game files is required.

Settings are saved in: %LOCALAPPDATA%\BT3-SparkingControls Updating the helper does not reset saved preferences. If you move the game, select its new executable location in the launcher.

## Linux installation — native Linux game

1. Extract BT3-SparkingControls-1.0.3-Beta-Linux-x64.zip.
2. Keep the entire extracted folder together, including the managed EXE,
   libbt3sparking.so, profiles, compatibility files and assets.
3. Install Mono with Windows Forms and System.Web.Extensions support,
   libgdiplus and fonts for your selected language using your distribution's
   package manager. Use an X11 session or working XWayland with compositing.
4. Open a terminal in the extracted folder and run:
   bash start.sh
5. Click Browse and choose the native Linux Tenkaichi3Decomp executable,
   not the Windows .exe.
6. Choose your settings and click Save and start.

This package is for Linux x86-64. Its managed .exe runs through Mono; Wine or Proton is not required. Native Wayland overlays are not implemented. The native library was cross-compiled with a glibc 2.35 target. Rebuilding may be necessary on a different system; source and build.sh are included.

Settings are saved in: $XDG_DATA_HOME/BT3-SparkingControls/linux Default: ~/.local/share/BT3-SparkingControls/linux

To check the game/library without starting gameplay: bash start.sh --check /path/to/Tenkaichi3Decomp

Official Linux v0.1.37 and v0.1.38.1 executables passed compatibility checks. Real Linux Mono/X11 rendering, LD_PRELOAD execution and gameplay have not been tested here. Read README.md and VALIDATION.json in the Linux package.

## Basic controls

- Square: Rush attacks.
- Triangle: Ki blasts.
- X: Native movement and dash actions. Original double-tap X grabs remain.
- R2: Charge Ki.
- L1: Ascend.
- L2: Descend.
- R2 + L1: Vertical Z Dash upward (hold).
- R2 + L2: Vertical Z Dash downward (hold).
- R1: Guard outside your own supported attack combos.
- L3: BT3's original search/lock-on action.
- Select / View: Change camera.

These labels use PlayStation-style button names. Ordinary menu navigation keeps the original game layout.

## Special attacks and skills

- R2 + Square: Super Attack 1.
- R2 + Triangle: Super Attack 2.
- R2 + Circle: Ultimate Attack, subject to native requirements.
- R2 + D-pad Up or Down: Skill 1.
- R2 + D-pad Left or Right: Skill 2.

Special selection works while R2 is already held. These skill shortcuts are this helper's layout, not a claim of exact ZERO controls.

For a chargeable special, keep holding its assigned face button to continue charging. Releasing R2 alone does not release that attack button.

When the game requests a boost input, use Square together with the requested direction. The overlay replaces the old Triangle glyph while preserving the hand graphic.

## Counters, vanishes and grabs

Circle: Requests BT3's native counter stance, inspired by ZERO's Perception. During supported Rush/Ki combos, it takes priority over held attack buttons. It does not add ZERO's full Perception or Super Perception systems. Release Circle while retaining R1 to request the end of the counter stance and return to guard, without another R1 press. This waits for BT3's actual stance exit and respects the native animation window, for short or long holds.

R1 during your own supported combos: Requests BT3's native forward vanish. It may move forward slightly. It is not a stationary teleport and does not guarantee appearing behind the opponent.

R1 + left-stick direction during those combos: Requests the corresponding native directional vanish. Native Ki requirements and cancel windows apply.

R1 + Square: Requests a regular grab, with priority over vanish/counter routes. The helper generates the original X-release-X sequence. Native range and action windows still apply; it cannot force an unavailable grab.

R1 + left stick Down + Square: Alternate ground-grab shortcut. Use the left analog stick, not D-pad Down. The original Triangle + left stick Up command also remains available. Circle + left stick Up no longer requests the ground grab.

Original double-tap X grabs remain intentionally available. D-pad Down and Right without R2 are blocked during battle to prevent accidental native commands.

R3: Requests BT3's Emergency Blaster Wave through its native input sequence when eligible. It uses BT3's health cost and requirements, not ZERO's Skill Count-based Revenge Counter system. Original physical R3 transformation/fusion shortcuts are consumed during battle; the card menus generate the required native inputs internally.

## Transformations, fusion and teammate switching

Hold D-pad Up: Open transformation/fusion cards. Press Square, Triangle, X or Circle to select the matching card. Release D-pad Up to close the panel.

L1 / R1: Switch between transformations and fusion when both are available. Empty categories are skipped. Fusion-only characters open directly on fusion.

Hold D-pad Left: Open teammate-switching cards. Press the face button shown on the desired teammate card.

Fusion, transformation and switching retain BT3's native partner, form, gauge, cooldown and battle-state requirements. A visible card does not bypass those requirements.

All three menus use consistent card positions. Switching cards show reserve health bars; missing characters remain as empty card placeholders. Online local panels appear on the left HUD side even when you join as Player 2. This visual placement does not change the logical input side.

## HUD and customization

Hold R2: Display the active character's attack list, commands and costs. Available HD game button artwork is preferred; original assets are the fallback. No game artwork or HD texture pack is bundled.

Panel placement options: - Centered: A balance between the center and edges. - Expanded: More space between the panels. - Classic (4:3): Original positioning within a 4:3 region.

Native 4:3 positioning is preserved. Ultrawide layouts use safe regions rather than putting controls at the extreme screen edges.

All four battle menus fade in over 100 ms and out over 80 ms. Closing retains only the last visual snapshot until it becomes transparent. Reopening reverses smoothly. No command waits for the fade. Losing game focus clears immediately.

Show attack panel: Enable or hide the attack list. Attack panel outer frame: Optional; off by default for fresh settings. Existing saved choices are preserved during updates.

## Default keyboard shortcuts

- F6: Show or hide the attack panel.
- F7: Toggle the attack panel outer frame.
- F8: Cycle panel placement.

Click a shortcut button in the launcher to assign another key or combination, clear the binding or restore its default. Duplicate bindings are rejected. Shortcuts require game focus and do not repeat while the key is held. Enable Lock shortcuts during battle to prevent in-game changes.

Game path, language, placement, attack visibility, frame, shortcut bindings and shortcut lock are saved. HUD changes made during gameplay are also saved and restored the next time you open the app.

The launcher includes 16 interface languages and Arabic right-to-left layout. Move and character names still come from your game's assets and language.

## Compatibility, online and timing

The helper translates the local 18-byte controller packet before network sampling. The peer receives native BT3 inputs. Remote controller input and combat decision functions are not patched by this integration.

Compatible game address changes can be resolved automatically from symbols and reviewed signatures. Windows PE/COFF and Linux ELF resolvers check 61 complete functions and their symbolic/data references. Structural changes or missing symbols may still require an updated, reviewed helper. Compatibility is not guaranteed for every older or future game version.

Simple remaps do not add sleeps. Multi-step shortcuts require several native input samples. Grab and emergency sequences use two-sample press/release phases; the second press is about 33 ms later at 60 Hz. Range, native timing windows and rollback still affect whether the game accepts a command. Panel selections also follow native timing and acknowledgment.

Automated validation passed: 1,337,289 Windows packet/ABI/native-command checks; 445,763 Linux packet checks and 9 emulated loader checks for EACH of two game versions; 2,250 launcher, 93 shortcut and 693 responsive checks per frontend, plus 180 fade checks per frontend and 12 preview states. Counter regressions execute the actual compiled counter handler with varied Circle hold durations, animation timing and delayed acknowledgment. Linux managed UI tests ran under Windows .NET. Linux presentation now updates changed/animated frames immediately.

These tests do not establish real-world online behavior. Live online matches, actual ping/rollback, physical-controller timing and native Linux gameplay remain unverified for this beta. Desync-free operation is not guaranteed.

## Source and license

Both ZIPs include source code and the AGPL-3.0 license. Windows build instructions: source/desenvolvimento/BUILD.md. Linux build instructions: README.md and build.sh. No game executable, personal settings or game texture pack is included.
