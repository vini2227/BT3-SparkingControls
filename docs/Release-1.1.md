# 1.1 Beta — Changes

Windows release collecting the improvements developed after 1.0.6 Beta. Linux remains at 1.0.6; the Windows changes below are not included in that package.

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
