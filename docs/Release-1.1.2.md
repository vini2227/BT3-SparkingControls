# 1.1.2 — Changes

## Z Warriors compatibility (Windows)
- The helper now accepts the Decomp with the **Z Warriors** update installed as well as the original Decomp. Both are recognized automatically from their native interfaces; no executable whitelist is used and the original input payload, controls and HUD are unchanged.
- Data symbols whose COFF grouping changed with link order are matched by unique name and address; for code, only the matching import thunk is accepted.
- Relocated input dispatch tables are validated by their entries and literal contents instead of their distance from a neighbouring symbol.
- Reviewed alternatives were added for the recompiled `Port_PadRead` (same 18-byte packet), `BtlAct_QueueTransform` (Z Warriors form-skin call already present in the game) and `Port_NetActive` (`setne` accepted only after the separate negative-state exit is confirmed). Changed instructions, redirected references and stripped symbols are still rejected before launch.

## HUD reads
- When the attack panel is hidden, attack data is no longer read; prompts, transformations, fusions and switching still work.
- Move names are read from the active character's real text, including custom names: reading stops after five move records, malformed or unavailable text is retried at most once per second, and the cache is limited to 64 entries. Reopening the attack panel reloads the current names.
- The build uses its own cache identifier; saved preferences are kept.

## Validation and scope
- Structural compatibility suite: **passed on the Z Warriors executable and on the original Decomp** (65 checks each, 0 failures). 279 dispatch-table checks, including rejected mutations.
- 1,372,977 emulated packet/ABI/native-command checks against the Z Warriors executable; 2,217 name-cache/parser checks; 1,013 local HUD / hidden-panel / custom-name checks; 180 fade checks.
- The user played Z Warriors through this helper and the controls worked. No live online match or same-PC FPS comparison was performed; these changes remove confirmed redundant reads but do not prove that any slowdown is fixed.
- Future Z Warriors or Decomp updates may need a new review. Char Package and Tag Team payloads are unchanged.
- **Linux 1.1.2** ports the HUD read improvements above (hidden attack panel, bounded move-name cache). Z Warriors is a Windows-only package, so there is nothing to support on Linux; the ELF resolver, native library and reviewed versions (v0.1.37 / v0.1.38.1) are unchanged. Checked under Windows .NET (2,217 cache, 192 composition, 180 fade, 2,282 launcher checks); real Linux Mono/X11 execution was not available.
- The packaged EXE is the exact locally tested build; its informational label is `1.1.2-zwarriors-review`.
