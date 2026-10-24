# BT3 Sparking Controls 1.0.6 Beta — Linux x64

A Sparking! ZERO-inspired control layout and HUD for the native Linux version of Tenkaichi3Decomp. This is a beta port. Read FEATURES-EN.md for controls, differences and limitations.

## Changes in 1.0.6 Beta

All four menu types are left-aligned within each local player viewport, with stable scale across supported aspect ratios. This Linux package includes the split-screen update; the adaptive Windows compatibility checks and automatic Char Package / Tag Team detection are not included. The native library and Linux resolver are unchanged.

## Retained features

Vertical Z Dash on R2 + L1/L2; legacy double taps disabled. Releasing Circle while holding R1 waits for native stance exit and resumes guard without another R1 press. All battle menus use 100 ms fade-in / 80 ms fade-out, with immediate dirty-frame presentation on Linux. Native BT3 animation windows still apply. Read CHANGELOG.md and the Controls and Setup Guide.

## Start

Extract the entire ZIP into one folder. Keep the executable, .so library, profiles, compatibility files and assets together. Run `bash start.sh`, choose the native Linux game executable, then Save and start. The managed `.exe` runs with Mono, without Wine or Proton. The add-on must launch the game; do not separately launch another copy.

Requires Linux x86-64, Mono with Windows Forms/System.Web.Extensions, libgdiplus, fonts for the selected language and a composited X11 display or working XWayland. Native Wayland overlays are not implemented. The library was cross-compiled with a glibc 2.35 target. On a different system, rebuilding from source may be necessary.

## Compatibility and settings

Official Linux v0.1.37 and v0.1.38.1 executables passed the compatibility checks. The symbol resolver can accept compatible address changes after verifying 61 complete functions and references. Structural changes or stripped symbols can require a new reviewed profile. Compatibility is not guaranteed for every release.

Settings persist in `$XDG_DATA_HOME/BT3-SparkingControls/linux` (default `~/.local/share/BT3-SparkingControls/linux`). Runtime cache is in the sibling `linux-runtime` folder. Prior `linux-online-v0.1.37` settings are migrated when applicable. Updating the extracted application does not reset saved settings.

F8: placement. F7: attack frame. F6: attack visibility. Rebind/disable/reset shortcuts in the launcher. In-game changes persist. Enable the shortcut lock to prevent battle-time changes.

## Build and diagnostics

Sources and AGPL-3.0 license are included. With a C compiler (clang recommended), Mono development tools/mcs and Python 3 installed, run `bash build.sh`. Rebuilding the native library refreshes its profile hashes. Reviewed compatibility metadata is already included; do not regenerate it merely to bypass a compatibility error.

`bash start.sh --check /path/to/Tenkaichi3Decomp` validates the game/library without launching gameplay. Maintainer metadata regeneration: `python3 tools/build-compatibility-linux.py --game /path/to/reviewed/game --output compatibility-linux.json` (requires tests/requirements.txt). Test scripts accept the game/resolved-profile options documented by `--help`/their argument parser; CPU emulation is not an online test.

## Validation limits

445,763 CPU-emulated packet/ABI/native-command checks and 9 emulated loader checks passed for EACH of v0.1.37 and v0.1.38.1. Managed UI checks: 2,250 launcher checks across 16 languages, 93 shortcut checks, 693 responsive checks and 12 preview states. Incompatible modified ELF code and Windows executables were refused.

The UI fixtures ran under Windows .NET, not Mono. Real Linux LD_PRELOAD, X11 fullscreen/compositing, physical-controller timing, gameplay and online rollback have NOT been tested here. No universal no-desync guarantee is made. No game executable, game artwork or HD texture pack is bundled.
