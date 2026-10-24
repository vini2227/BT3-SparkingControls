# Build BT3 Sparking Controls 1.0.6 Beta — Windows

Python 3 and .NET Framework 4 C# compiler are required. Install Pillow to rebuild the icon. From the source root run `python desenvolvimento/tools/build-single-exe-windows.py`. The reviewed payload, profiles and compatibility metadata are included, so rebuilding the launcher does not require the game or Zig. An optional output EXE path can be passed as the first argument.

For native development install pefile, capstone, keystone-engine and unicorn. Set BT3_GAME to the reviewed game executable and ZIG to Zig 0.13.0 zig.exe before running desenvolvimento/tools/build-native-windows.py. Review game and control changes before regenerating compatibility metadata with build-compatibility-windows.py --reviewed. Some diagnostic scripts retain development paths; adapt them to your local game/cache. Never regenerate signatures solely to suppress a compatibility refusal.

Compatibility accepts verified address changes; structural differences can require a new reviewed profile. Linux now has its own ELF resolver and current controls/UI port. See the separate Linux package for native source/build instructions.

## Variant profiles

The `variants/` directory is embedded by the Windows single-EXE builder and must be included with source distributions. Rebuilding the launcher requires no game executable or native compiler. The original native payload remains unchanged.

To regenerate variant metadata after reviewing the game interfaces, install pefile/capstone/keystone-engine, set ZIG to Zig 0.13.0, and run `python desenvolvimento/tools/build-variants-windows.py --reviewed --characters PATH_TO_CHAR_PACKAGE_EXE --tagteam PATH_TO_TAG_TEAM_EXE`. Tag Team's native source is in variants/tagteam/src. The generator builds in analysis/variant-native without replacing the original native payload/profile. PDB/debug timestamps can change the payload hash between builds even when executable code is identical. Run compatibility, packet and HUD tests before distributing a regenerated build.
