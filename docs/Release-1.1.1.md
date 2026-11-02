# 1.1.1 — Changes

## Windows compatibility
- Reviewed the updated original Decomp interfaces. Read-only input dispatch tables are validated by their entries instead of their distance from an unrelated symbol, avoiding a false incompatibility after table relocation.
- Retained reviewed alternatives for changed native functions, strict instruction/reference checks, and launch-time address resolution. Unsupported interfaces still require review; compatibility with every future update is not guaranteed.
- The installed November 2 Decomp passed the structural compatibility suite. No game files were changed. This is not a live online match test.

## Recovery controls — original Windows Decomp
- During BT3's native launched/falling state, hold the **left analog stick Down** to request native recovery, or **Up** to request native airborne recovery.
- These shortcuts do not use the D-pad. Explicit button combinations take priority; requests stop when the stick returns to neutral or the native state changes.
- Normal grounded recovery remains available through BT3's native options. There is no Circle-only restriction.
- This is inspired by Sparking! ZERO's gestures, but uses BT3's eligibility, timing windows and animations. It does **not** reproduce ZERO's Dragon Dash recovery exactly or change recovery physics, costs or combat rules.

## Validation and scope
- 1,372,977 emulated packet/ABI/native-command checks passed, including alternate input-sampling phases and local online-host/guest packet paths. These checks do not establish actual network rollback or guarantee no desync.
- The recovery addition targets the original Windows Decomp. Char Package and Tag Team payloads are unchanged. BT4 remains a separate project/build.
- **Linux remains at 1.1** with its previously documented compatibility limits; the Windows recovery and compatibility changes above have not been ported to Linux.
- This release packages the exact latest locally installed Windows EXE. Its informational build label still contains `1.1.1-sz-recovery-test`; that identifies the tested build, not a different download.
- Saved preferences are retained. Normal diagnostic collection stays disabled; use diagnostics only when collecting a report.

## Known issue and Lua assessment
The texture-pack / ×8 slowdown reported by Aero is still under investigation, including team battles. This release does not claim to fix it. No additional performance improvement is claimed here.

The new Decomp Lua API can draw HUDs inside the game but does not expose control remapping. A Lua HUD plus native control component is a possible future approach; **no Lua migration is included in 1.1.1**.

Special thanks to **Aero** for extensive testing, reproduction details and diagnostic logs.
