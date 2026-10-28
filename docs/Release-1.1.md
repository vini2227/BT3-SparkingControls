# 1.1 — Changes

Windows and Linux 1.1 packages are available. The Windows changes below are followed by a separate Linux section describing platform differences.

## Linux 1.1 — Changes

- Ported Story / Mission 100 fight detection for the reviewed original Linux Decomp, including native R3 story confirmation handling.
- Cropped CPU composition and menu snapshots; reused the X11 image and transfer buffer instead of allocating/destroying them per frame. Idle HUD is unmapped; unchanged frames are not uploaded again. Game viewport coordinates and fade timings are retained.
- Added Screen edges placement, persisted with the other three layouts and included in the F8 cycle and translated launcher.
- Cached HD boost-icon ink bounds. Diagnostic launcher logging is off by default; use `bash start.sh --diagnostics` only when collecting a report. Settings stay in the existing XDG directory.
- Rebuilt the native ELF library and Mono frontend. The ELF compatibility resolver still targets reviewed official v0.1.37/v0.1.38.1; automatic Windows variant detection and Windows-specific debugger/heap/arena changes are not included. No universal future-version or updater support is claimed.

Validation: 446,473 emulated packet/ABI/native-command checks and 9 loader checks for EACH reviewed Linux executable; 192 cropped pixel/persistence checks; 180 fade, 693 responsive, 1,408 card, 565 split-screen, 93 shortcut and 2,282 launcher checks. UI tests ran on Windows .NET, not Mono. Real Linux LD_PRELOAD/X11 presentation, native Wayland, physical controllers and online rollback remain unverified. This is not a confirmed fix for the reported Windows Mission 100 ×8/texture-pack slowdown.

Special thanks to **Aero** for extensive testing, reproduction details and diagnostic logs that guided the performance work.

## Controls and HUD
- Corrected fight detection in the original Decomp's Story / Mission 100 paths when the usual running flag is absent. Remapping still requires native fight state and live battle subsystems; menus, CPU input and non-fight states retain their protections. Char Package / Tag Team native payloads remain at their reviewed 1.0.6 versions.
- Tightened native prompt handling to avoid inappropriate stick/R3 hints during story gameplay.
- Added **Screen edges**, a fourth panel placement option for players who want menus at the outer edges of a solo ultrawide picture. Existing Centered, Expanded and Classic (4:3) layouts remain available.
- Improved restoration and live reload of saved panel preferences; language, game path, placement, visibility, frame and shortcuts remain persistent across helper updates.

## Rendering and startup
- Reuse a cropped native HUD surface, skip unchanged presentations and hide the window when there is no HUD content.
- Crop CPU composition and menu snapshots too, instead of clearing two full-screen bitmaps. At 5120×1440 the old two CPU canvases alone occupied about 56 MiB; the new canvas sizes follow actual panel bounds. A one-pixel surface measures menu geometry. Fade timing and panel anchors are preserved.
- Cache boost-icon ink bounds instead of repeatedly scanning the same HD artwork.
- Keep helper allocation outside the game's preferred memory arena and retain normal Windows heap behavior during tracked launches.
- Normal launches skip periodic diagnostic collection, extra diagnostic reads and verbose renderer recording. Optional diagnostics remain available; no diagnostic test scripts are enabled by default.

## Compatibility
- Resolve compatible executable address changes and validated online-state initialization changes at launch/relaunch, outside the controller loop. Reuse parsed image data during validation.
- Preserve normal tracking of game restarts and the local native-input approach. The offline-only debugger-detach experiment is not the default launch mode and must not be used online.
- The updated local game passed structural compatibility checks. This does not guarantee every future build or every updater/restart path; incompatible interfaces still require review.

## Validation and known issue
Local checks passed for cropped pixel composition (197), fade behavior (180), card menus (1,408), responsive layout (693), split-screen attacks (565), recovery (9), shortcuts (93) and saved settings. Earlier native input/ABI, arena and compatibility reports are retained as prior checks, not represented as newly repeated live matches.

**The reported Mission 100 slowdown is still under investigation.** With a texture pack and ×8 resolution enabled from startup, Aero's PC recorded long native frame-submission stalls. These changes reduce confirmed helper work and allocations, but the affected-PC slowdown has not been confirmed fixed. Automated checks do not establish live rollback, universal updater compatibility or sustained GPU performance. Graphics quality is not reduced automatically.

## Thanks
Special thanks to **Aero** for the extensive testing, detailed reproduction steps and diagnostic logs that helped identify and investigate these issues.
