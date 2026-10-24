# 1.0.6 Beta — Changes

- **Both platforms: local split-screen HUD.** Attacks, transformations, fusion and teammate-switch menus sit on the left inside each player’s viewport. Their scale and position stay stable when another menu opens. Decorative bars are excluded; 4:3, 16:9, 16:10 and ultrawide layouts are handled.
- **Windows: adaptive compatibility checks.** Compatible address/timestamp changes and reviewed input-collection changes can be accepted automatically. Native packet, battle and network interfaces are still checked; arbitrary future versions are not guaranteed.
- **Windows: automatic variant selection.** Select the game EXE normally. The helper recognizes the original Decomp, the supplied Char Package 0.1.38.1 and Tag Team Beta x64 builds without a manual variant selector.
- **Windows: variant-aware HUD.** Char Package names, forms and portraits use active package data. Tag Team uses up to four local controller state banks and keeps menus inside each player’s quarter. Original input payload and existing settings are preserved.
- **Linux 1.0.6:** updated split-screen HUD, with the existing native library and resolver. Reviewed official Linux v0.1.37/v0.1.38.1 remain the tested targets; Windows variant support is not included.

**Known limits:** Tag Team’s four-view mode leaves native boost hints visible instead of drawing the extra Square/Circle replacement. Char Package itself disables custom characters online. Automated checks do not establish live gameplay or rollback compatibility; native Linux Mono/X11 gameplay remains unverified.

[Full changelog](../CHANGELOG.md)
