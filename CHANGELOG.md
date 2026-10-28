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

---

# 1.0.6 Beta — Changes

This release contains all changes since public 1.0.3 Beta. Windows and Linux packages are available; platform-specific differences are listed below.

## Automatic Windows variant detection

- Select the game's EXE as usual. The launcher automatically distinguishes the original Decomp, the supplied Char Package 0.1.38.1 Windows build and the supplied Tag Team Beta Windows x64 build. No variant selector is needed.
- Check native interfaces and symbolic references before applying a profile. Filenames and hashes alone do not grant compatibility. Timestamp and section-address changes are accepted by the tested profiles; incompatible native interface changes are rejected.
- Preserve the original input payload byte-for-byte and keep the original compatibility path. Saved preferences, controller mappings and regular single/two-view HUD behavior remain intact.
- Char Package: use the variant's native transformation/fusion queries. Read active character/replacement names, form labels, portrait paths and fallback base portraits from the game's package tables, respecting its assigned IDs and enabled packages. Its custom characters remain disabled online by the game itself.
- Tag Team: use the variant's controller-to-fighter/view mapping and private state banks for up to four local controllers. Attack, transformation, fusion and team-switch panels stay in their own quarter at 4:3, 16:9, 16:10 and ultrawide ratios. Game restrictions on changing/fusing an extra fighter still apply.

## Validation and current limits

- Original Decomp: 1,337,289 packet/ABI/native-command checks passed.
- Char Package: 1,337,289 packet/ABI/native-command checks passed.
- Tag Team: 1,361,661 packet/ABI/native-command checks passed, including four-controller state isolation. An additional 696-check focused run verified four-controller output with executable code protected read/execute and private state left writable.
- Existing visual regressions: 1,056 card-layout, 425 split attack, 693 responsive and 180 fade checks passed. New variant HUD/metadata tests: 1,299 checks passed.
- Native packet tests use emulated query results and exercise selected actual native command functions. They do not replace a live game test. Live local/online play, physical controllers and rollback end-to-end were not tested.
- In Tag Team's four-view mode, the extra Square/Circle replacement over native boost hints is not drawn: the original hints remain. The game exposes only two shared prompt-state rows to the existing reader; guessing their owner would risk covering another player's HUD. The remapped controls and all four menu types remain active.
- Support is for the supplied Windows variants with the reviewed interfaces, not arbitrary forks or every future revision. The Linux resolver is unchanged.

---

## Adaptive Windows compatibility

- Recognize the updated installed game without changing its files or adding its hash to an allowlist.
- Keep the reviewed battle/HUD signatures and symbolic address checks. When the input collection body changes, validate a bounded Windows input boundary contract instead: entry ABI and register preservation, exact 18-byte active-low packet writer, return path, and the native online sampling caller.
- Resolve repeated compiler-local COFF names in their function emission group. Unrelated variables with the same name no longer cause a false incompatibility or become interchangeable targets.
- Preserve the primary compatibility failure in diagnostic logs if the legacy profile also fails.
- Keep the existing historical profiles, controls, preferences, fades and all four splitscreen menus. The native input payload is unchanged.

This is bounded compatibility detection, not support for arbitrary future game changes. Different packet layouts, entry ABI, battle interfaces or online sampling code still require review. The static contract does not prove arbitrary code behavior or online synchronization.

Validation: current installed Windows executable accepted; changed timestamps, relocated sections and input-reader alignment accepted; changed packet output/return, native interfaces, sampling caller, unrelated duplicate-symbol targets and stripped symbols rejected. 1,337,289 packet/ABI/native-command checks passed at three allocation addresses against the current game's native functions. Live online gameplay and older executable runtime tests were not performed. The Linux resolver is unchanged.

---

## Local splitscreen menus — Windows and Linux

- Detect local split mode from the battle screen setting, independently of visible menus. Online remains a single local view.
- Position both attack menus at the bottom-left of their own player viewport. Exclude decorative letterbox/pillarbox regions before dividing the rendered picture.
- Keep size and position stable when the other player opens or closes their menu. Both players use the same typography scale.
- Use compact scaling with safe bounds at 4:3, 16:9, 16:10, 21:9 and 32:9. Existing Centered/Expanded/Classic choices apply within each viewport.
- Transformations, fusion and teammate switching also use each player viewport with a left anchor. Their card grid, header spacing and size remain identical across the three menu types, including empty slots.
- Preserve current single-screen positioning, attack visibility, frame option, settings and fade animation.
- Read-only HUD change; native input payload and Linux shared library are byte-for-byte unchanged.

Validation: 1,056 card-menu checks, 425 attack splitscreen cases, 693 responsive checks and 180 fade checks per frontend under Windows .NET. Seven render resolutions, three placements, both frame styles, solo/both visible menus and letterboxing. Windows extracted executable compatibility check passed. Native Linux runtime and live split gameplay remain unverified.

Linux 1.0.6 includes the split-screen HUD improvements. Its native library and compatibility resolver remain unchanged from 1.0.3: reviewed official Linux v0.1.37 and v0.1.38.1 only. Automatic Char Package / Tag Team detection and the adaptive Windows input-boundary checks are Windows-only. The common release number does not imply identical platform support.

---

# BT3 Sparking Controls 1.0.3 Beta — October 22, 2026

This release includes all improvements since the public 1.0 Beta.

- **Vertical Z Dash:** hold **R2 + L1** to rise quickly or **R2 + L2** to descend quickly. Starting while L1/L2 is already held is supported. L1/L2 alone retain normal ascent/descent. Their old double-tap fast-movement shortcut is disabled; a rapid second normal press may wait for BT3's seven-frame double-tap window to expire.
- **Counter stance to guard:** release **Circle while keeping R1 held** to request the end of the counter stance and resume guard without pressing R1 again. Short and long Circle holds are supported. The helper waits until BT3 actually exits the stance, fixing a release pulse that could finish too early. BT3's native animation exit window still applies; the helper does not force an animation cancel.
- **Quick menu fades:** attack, transformation, fusion and teammate-switching panels fade in over **100 ms** and out over **80 ms**. Quick reopening reverses smoothly. This is visual only and adds no input wait. Losing game focus clears the overlay immediately.
- **Linux HUD presentation:** changed/animated frames are presented immediately instead of being limited by the maintenance refresh timer.
- **Documentation:** updated setup/control TXT, feature list, source/build records and all 16 language guides. Windows and Linux packages include current sources.

## Validation

- 1,337,289 Windows packet/ABI/native-command checks at three allocations.
- 445,763 packet checks and 9 emulated loader checks for EACH of Linux v0.1.37 and v0.1.38.1.
- 180 managed fade checks per frontend; 693 responsive checks, optional-frame rendering and 9 Windows layered/persisted-layout cases.
- Existing launcher/shortcut coverage remains applicable: 2,250 launcher checks across 16 languages, 93 shortcut checks and 12 preview states.
- Counter tests execute the actual compiled native counter handler and cover varied hold durations, animation completion and delayed input acknowledgment on host/guest and both offline ports.

These are automated/CPU-emulated and managed rendering checks. Actual Linux Mono/X11/LD_PRELOAD gameplay and live online rollback remain unverified. Native costs, range, action windows and animation rules still apply; no universal no-desync guarantee is made.

Settings persist across updates. No game executable, game artwork pack or personal settings are bundled. The existing gameplay demonstration was recorded for 1.0 Beta; it does not demonstrate the new fixes.
