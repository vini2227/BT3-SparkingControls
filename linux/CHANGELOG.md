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
