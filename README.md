# BT3 Sparking Controls

**1.0 Beta** — A Sparking! ZERO-inspired controller layout and battle HUD for Tenkaichi3Decomp, with Windows and experimental native Linux support.

BT3's original combat rules, animations, costs, range and action windows still apply.

## Download and install

Download the Windows or Linux x64 ZIP from this repository's **Releases** page.

- **Windows:** extract the ZIP, run `BT3-SparkingControls.exe`, select your game executable and click **Save and start**.
- **Linux x64:** keep the whole extracted folder together. Install Mono/Windows Forms, libgdiplus and use X11 or working XWayland. Run `bash start.sh`, select the native Linux game executable and click **Save and start**. Wine/Proton is not required.

The game and its assets are not included. Launch the game through the helper.

## Controls and features

See [Controls and Setup Guide](docs/Controls-and-Setup-Guide.txt) for installation, every command and usage instructions. See [Features and Differences](FEATURES-EN.md) for a detailed comparison with Sparking! ZERO.

- R2 + Square/Triangle/Circle: Super 1 / Super 2 / Ultimate.
- R2 + D-pad: character skills.
- R1 + Square: native grab sequence; left stick Down adds the ground-grab shortcut.
- Circle: native counter stance. R1 during supported combos: native vanish, with analog direction support.
- Hold D-pad Up for transformations/fusion; hold D-pad Left for teammate switching.
- Optional attack list/frame, consistent cards, reserve health bars and viewport-aware placement.
- F6/F7/F8: attack visibility / outer frame / placement. Rebind, disable, reset or lock shortcuts.
- Sixteen interface languages. Settings and in-game HUD choices persist across launches.

## Compatibility and beta status

Compatible address changes are resolved using reviewed signatures and symbols. Structural changes can still require an update. Linux compatibility checks passed for official v0.1.37 and v0.1.38.1 x64 binaries.

Automated packet, ABI, loader, launcher, shortcut and layout checks passed. Linux managed UI checks ran under Windows .NET; native Mono/X11, Linux gameplay and live online rollback remain unverified. No universal no-desync guarantee is made.

## Source and building

- Windows: [Build instructions](desenvolvimento/BUILD.md), C# frontend and native C filter in `desenvolvimento/src`.
- Linux: [Linux README](linux/README.md), native C/Mono source and `build.sh` in `linux`.
- Compiled helper payloads and reviewed metadata are included; game executables are not.

## License

[GNU AGPL-3.0](LICENSE-AGPL-3.0.txt). Bundled fonts retain their included licenses. Dragon Ball and game-related names belong to their respective owners. This is an unofficial fan project.
