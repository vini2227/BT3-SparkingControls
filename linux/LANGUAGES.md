# Launcher languages and panel placement

The launcher uses a dark theme with rounded buttons. English remains the default. The language and panel placement are saved. Panel placement offers **Centered** (central 3:2 safe area), **Expanded** (previous 16:9 safe area) and **Classic (4:3)** (original 4:3 safe area) on widescreen/ultrawide. All three preserve the same native 4:3 positioning. The placement preference is stored in panel-layout.txt in the parent settings folder, shared across helper versions; legacy per-version settings remain readable.

The following 16 interface languages match the official Steam language list verified on 2026-10-20:
https://store.steampowered.com/app/1790600/DRAGON_BALL_Sparking_ZERO/

- English (`en`)
- Français (`fr`)
- Italiano (`it`)
- Deutsch (`de`)
- Español (España) (`es`)
- العربية (`ar`)
- Bahasa Indonesia (`id`)
- Español (Latinoamérica) (`es-419`)
- Português (Brasil) (`pt`)
- Polski (`pl`)
- Русский (`ru`)
- 简体中文 (`zh-Hans`)
- 繁體中文 (`zh-Hant`)
- 日本語 (`ja`)
- 한국어 (`ko`)
- ไทย (`th`)

The Arabic launcher uses right-to-left layout; executable paths remain left-to-right. Game move/character names still come from the user's BT3 game files. Technical messages from the operating system or native library can retain their original language. Linux needs fonts for the chosen script; native Mono/X11 rendering still requires validation on Linux. The managed Linux launcher test here ran under Windows and is not a real Linux GUI test.
