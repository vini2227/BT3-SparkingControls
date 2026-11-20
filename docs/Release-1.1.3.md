# 1.1.3 — Changes

## Game rebuilds no longer need a helper update (Windows)
- **Z Warriors v1.05 Quick Fix** rebuilt the game executable. Three functions the helper only checks — `BtlInput_BuildRecord`, `HudPrompt_UpdateCommand` and `BtlInput_MergeStick` — changed size (different register choice and block order, a relocated switch table, the same |x−127| computed in bytes). Their logic is the same, but 1.1.2 refused to start (`Changed native interface: BtlInput_BuildRecord length`).
- A changed function that the helper **never calls** is now accepted as a rebuild when it keeps: the same sequence of direct calls in the same order; every other reviewed reference; switch tables with the same number of cases, all inside the function; reviewed constants with identical content; and a size within −25%/+33%. At most a quarter of the checked functions may be rebuilds in one executable.
- Functions the helper calls, the input-reader hook (`Port_PadRead` packet contract) and the online-state contract stay strict. Changed call targets, changed constants and stripped symbols are still rejected before launch; a refusal now names the failed rebuild check.
- Controls, HUD, payload and online code are unchanged. Linux stays at 1.1.2 (Z Warriors is Windows-only).

## Validation and scope
- Structural compatibility suite passed on **Z Warriors v1.05** (6430a1a2…, 71 checks, three functions accepted as rebuilds), the previous Z Warriors build (4aa8923c…, 68 checks) and the original Decomp (de4f626a…, 65 checks), including the moved-sections and mutation-rejection fixtures.
- Not yet played with Z Warriors v1.05; no live online match was performed. A future update that changes game logic (not just compilation) still requires a review.
