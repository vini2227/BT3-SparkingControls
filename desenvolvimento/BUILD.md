# Build BT3 Sparking Controls 1.0 Beta — Windows

Python 3 and .NET Framework 4 C# compiler are required. Install Pillow to rebuild the icon. From the source root run `python desenvolvimento/tools/build-single-exe-windows.py`. The reviewed payload, profiles and compatibility metadata are included, so rebuilding the launcher does not require the game or Zig. An optional output EXE path can be passed as the first argument.

For native development install pefile, capstone, keystone-engine and unicorn. Set BT3_GAME to the reviewed game executable and ZIG to Zig 0.13.0 zig.exe before running desenvolvimento/tools/build-native-windows.py. Review game and control changes before regenerating compatibility metadata with build-compatibility-windows.py --reviewed. Some diagnostic scripts retain development paths; adapt them to your local game/cache. Never regenerate signatures solely to suppress a compatibility refusal.

Compatibility accepts verified address changes; structural differences can require a new reviewed profile. Linux now has its own ELF resolver and current controls/UI port. See the separate Linux package for native source/build instructions.
