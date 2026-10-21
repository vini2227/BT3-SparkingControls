# BT3 Sparking Controls 1.0 Beta — Features and Differences

## What this add-on is
- A standalone, Sparking! ZERO-inspired control layout and battle HUD for Tenkaichi3Decomp.
- Battle controls change; ordinary menu navigation keeps the original layout.
- No modloader, game-file replacement or game recompilation is required. The add-on must launch the game.
- This remains Budokai Tenkaichi 3: its animations, damage, Ki costs, range, character abilities and action/cancel windows still apply.

## Core controls
- R2: charge Ki; L1: ascend; L2: descend; R1: guard outside your own attack combos.
- Square: Rush attacks; Triangle: Ki attacks; X: native movement/dash actions.
- R2 + Square: Super 1; R2 + Triangle: Super 2; R2 + Circle: Ultimate.
- R2 + D-pad Up/Down: Skill 1; R2 + D-pad Left/Right: Skill 2. These are this add-on's skill shortcuts, not a claim of an exact ZERO layout.
- Special selection works while R2 is already held. Directions are normalized for the selected special; they do not select a different legacy special route.
- Once a chargeable special has started, hold its assigned face button to keep charging; releasing R2 alone does not release that button.
- Square + the requested direction forwards the original boost input when the game asks for it.
- Select changes the camera. L3 maps to BT3's original search/lock-on action.
- D-pad Down/Right without R2 are blocked during battle to prevent unintended native shortcuts.

## Counter stance, vanishes and grabs
- Circle requests BT3's native counter stance, inspired by ZERO's Perception button. It does not add ZERO's complete Perception/Super Perception mechanics.
- During your own supported Rush/Ki combo actions, Circle takes priority over held attack buttons and requests the native combo counter stance.
- R1 alone during those combos requests the native forward vanish. It can move forward slightly; it is not a stationary teleport or a guaranteed teleport behind the opponent.
- R1 + left-stick direction during those combos requests the corresponding native directional vanish. Native Ki requirements and cancel windows still apply.
- R1 + Square requests a regular grab, with priority over the vanish/counter routes. It translates to the original X–release–X sequence; it cannot force a grab in an unavailable combo window or out of range.
- Ground grab: R1 + left stick Down + Square. The original Triangle + left stick Up route also remains. D-pad Down does not select this alternate ground-grab shortcut.
- Circle + left stick Up no longer accidentally requests the ground grab.
- Original double-tap X grabs remain intentionally available. This is a difference from ZERO Standard controls.
- R3 requests BT3's Emergency Blaster Wave through its native input sequence when the game offers the move. It uses BT3's health cost and eligibility rules; it is not ZERO's Skill Count-based Revenge Counter.
- Original physical R3 transformation/fusion shortcuts are consumed during battle; the panels generate the required native commands internally.

## Transformations, fusion and teammate switching
- Hold D-pad Up to open the transformation/fusion cards; release it to close the panel.
- Use Square, Triangle, X or Circle to choose the matching card.
- L1/R1 switch between forms and fusion when both categories exist. Empty categories are skipped; fusion-only characters open directly on fusion.
- Hold D-pad Left for teammate switching; choose the matching face-button card.
- Switching cards include reserve health bars. Empty slots stay visible as empty cards.
- All three panels share a compact cross layout with consistent card positions and proportional portrait cropping.
- Fusion and switching use original BT3 inputs, cooldowns and acknowledgment. A visible card does not bypass partner, gauge, form or battle-state requirements.
- Online local panels are placed on the left HUD side even when the local player is logically Player 2.

## Battle HUD and launcher
- Hold R2 to show the active character's five skills/specials with their names, assigned buttons and native costs.
- Costs are displayed as Skill Count values or Ki-bar values, without redundant unit labels; the Ultimate includes a Sparking/MAX marker.
- Compact single-line rows, aligned cost numbers, original-game font metrics, game-style translucent rows/frame and no redundant title/player label.
- Game button artwork and available HD replacements are preferred; original assets are the fallback. No game artwork or HD texture pack is bundled.
- Charge prompts cover only the old Triangle glyph with the appropriate new face-button symbol, preserving the hand and keeping the existing menus above the overlay.
- Portraits preserve their aspect ratio; panel sizing adapts to the viewport. Fine frame seams were removed.
- Dark amber/orange launcher with rounded controls, a framed three-star Dragon Ball and controller icon, and a live settings preview. Standard Windows caption/borders are preserved.
- Sixteen interface languages, including Arabic right-to-left layout. Names and moves still use the game assets/language.
- Centered, Expanded and Classic (4:3) panel placement; native 4:3 positioning is preserved. Ultrawide layouts use safe regions rather than the far screen edges.
- Show/hide the attack panel; optional outer attack frame is off by default. Translucent attack rows match the character-card style.
- Default shortcuts: F8 cycles placement, F7 toggles the outer frame, F6 toggles attack visibility. Rebind, clear or restore each shortcut. Duplicate bindings are rejected.
- Optional battle-shortcut lock; shortcuts require game focus and do not repeat while held.
- Game path, language, placement, visibility, frame, bindings and shortcut lock persist across launches and helper updates. In-game HUD changes are saved too.
- Teammate portrait crop corrections keep faces/eyes visible, including Vegeta forms. Online local cards, moves and prompts use the left HUD while preserving the correct logical input side.

## Online, timing and platform limits
- The current integration translates the local 18-byte controller packet before network sampling. The peer receives ordinary BT3 inputs; combat decision functions and remote input are not patched.
- This design aims to support an unmodified peer, but the latest changes have not been validated end-to-end in a real online match. No universal no-desync guarantee is claimed.
- Simple remaps add no sleep or extra action queue. Grab and emergency shortcuts require multiple native input samples; panel selections also follow native timing. Zero added latency for every shortcut is not promised.
- Windows follows the game's online/offline child-process relaunch. Linux reinstalls the preload on the game's same-PID exec restart and reopens the child-memory reader when necessary; that Linux behavior still needs a real runtime test.
- Windows package: current standalone EXE. Linux package: updated native x86-64 ELF integration plus a Mono launcher, without Wine/Proton.
- Linux requires Mono/libgdiplus and X11 or working XWayland/compositing. Fully native Wayland overlays are not implemented. The Linux port is experimental and has not been gameplay-tested on Linux.
- Compatible game address changes can be resolved automatically from symbols and reviewed signatures. Windows PE/COFF and Linux ELF resolvers verify 61 complete functions and their symbolic/data references; incompatible structural changes still require an update.
- Linux compatibility was checked against official v0.1.37 and v0.1.38.1 x64 binaries. This does not claim support for every older or future build.
- Grab and emergency sequences hold each press/release phase for two input samples to improve capture. The second press is about 33 ms later at 60 Hz; rollback, range and native action windows still apply. Transform, fusion and switching retain their native timing/acknowledgment.
- Package validation includes 480,183 Windows packet checks; 160,061 Linux packet checks and 9 emulated loader checks on each of two game versions; 2,250 launcher, 93 shortcut and 693 responsive checks per frontend, plus preview-state checks.
- Linux managed UI checks ran under Windows .NET. Native Mono/X11, real Linux LD_PRELOAD and live online/rollback behavior remain unverified.
