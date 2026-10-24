[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.0.6 Beta

## Controles y guía de instalación

Controles y HUD inspirados en Sparking! ZERO para Tenkaichi3Decomp. Se mantienen las animaciones, daño, costes de Ki, alcance, habilidades y ventanas de acción de BT3. El juego y sus recursos no están incluidos.

## Novedades de la versión 1.0.6 Beta

- Windows y Linux: los cuatro menús quedan a la izquierda dentro de la vista de cada jugador, con tamaño estable en 4:3, 16:9, 16:10 y ultrapanorámico. Windows: comprobaciones adaptativas y detección automática del Decomp original, Char Package y Tag Team; datos de personajes personalizados y hasta cuatro mandos locales. Linux 1.0.6 incluye la nueva disposición, pero conserva la biblioteca y el resolutor anteriores, probados con v0.1.37/v0.1.38.1; no incluye las variantes Windows. Con cuatro vistas Tag Team permanecen los avisos boost nativos, sin superposición adicional. Char Package desactiva los personajes personalizados online. Las pruebas automáticas no sustituyen partidas reales; Linux nativo y rollback online siguen sin verificar. No se garantiza toda versión futura ni la ausencia total de desync.

[CHANGELOG](../../CHANGELOG.md)

## Descargas y demostración

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-1.0.6-Beta-Windows.zip) · [**Linux x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-1.0.6-Beta-Linux-x64.zip) · [**1.0.6 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.0.6-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/SHA256SUMS.txt)

## Instalación

### Windows x64

Extrae el ZIP de Windows, ejecuta `BT3-SparkingControls.exe`, pulsa **Browse**, selecciona `Tenkaichi3Decomp.exe`, elige idioma y ajustes y pulsa **Save and start**. Inicia siempre el juego desde el auxiliar. El EXE integra sus recursos y puede compartirse solo para uso normal; el destinatario necesita el juego. El ZIP incluye código y licencia. No hace falta modloader, recompilar el juego ni sustituir sus archivos.

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Extrae todo el ZIP Linux x64 y conserva juntos el EXE administrado, `libbt3sparking.so`, perfiles y recursos. Instala Mono con Windows Forms/System.Web.Extensions, libgdiplus y fuentes para tu idioma. Usa X11 con composición o XWayland funcional. Ejecuta `bash start.sh` en esa carpeta, selecciona el juego **nativo de Linux** y pulsa Save and start. El .exe usa Mono, sin Wine/Proton. Wayland nativo no está implementado. La biblioteca se compiló para glibc 2.35; otro sistema puede requerir `bash build.sh`. No abras otra copia del juego por separado.

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## Controles

□ = Cuadrado; △ = Triángulo; ○ = Círculo; LS = stick izquierdo. LS con flechas se refiere al stick, no a la cruceta. La navegación de los menús normales conserva los controles originales.

| Botón / combinación | Acción |
| --- | --- |
| `□` | Ataques Rush |
| `△` | Ráfagas de Ki |
| `X` | Movimiento y desplazamientos nativos |
| `R2` | Cargar Ki |
| `L1` | Ascender |
| `L2` | Descender |
| `R2 + L1` | Z Dash vertical hacia arriba |
| `R2 + L2` | Z Dash vertical hacia abajo |
| `R1` | Guardia fuera de tus combos compatibles |
| `R2 + □` | Superataque 1 |
| `R2 + △` | Superataque 2 |
| `R2 + ○` | Ataque definitivo, sujeto a requisitos nativos |
| `R2 + D-pad ↑ / ↓` | Habilidad 1 |
| `R2 + D-pad ← / →` | Habilidad 2 |
| `○` | Postura de contraataque nativa |
| `R1 (combo)` | Teletransporte frontal nativo durante combos compatibles |
| `R1 + LS (combo)` | Teletransporte direccional nativo durante esos combos |
| `R1 + □` | Agarre normal |
| `R1 + LS↓ + □` | Agarre en el suelo alternativo, solo stick |
| `LS↑ + △` | Agarre en el suelo con el comando original |
| `R3` | Emergency Blaster Wave, sujeto a requisitos nativos |
| `L3` | Buscar / fijar objetivo |
| `Select / View` | Cambiar cámara |
| `D-pad ↑` | Mantener: transformaciones/fusiones |
| `D-pad ←` | Mantener: cambio de compañero |
| `D-pad ↓ / → (no R2)` | Bloqueados en combate para evitar acciones accidentales |

Puedes seleccionar ataques mientras ya mantienes R2. Para seguir cargando un especial, mantén su botón de ataque; soltar solo R2 no lo suelta. Cuando se indique, □ + dirección solicitada envía el boost original. La superposición tapa solo el Triángulo y mantiene visible la mano. Los atajos de habilidades no son una reproducción exacta de ZERO.

○ solicita la postura nativa de BT3 y tiene prioridad sobre los ataques mantenidos durante combos compatibles; no añade todo Perception/Super Perception de ZERO. Los teletransportes de R1 conservan costes y ventanas nativos: no garantizan aparecer detrás ni permanecer inmóvil. R1 + □ tiene prioridad y genera X–soltar–X, según alcance y ventana válida. Se conserva el doble X original. El agarre en el suelo no acepta cruceta ↓; LS↑ + ○ no lo activa. R3 usa condiciones y coste de vida de BT3, no el Revenge Counter de ZERO basado en puntos de habilidad. Los antiguos atajos físicos R3 de transformación/fusión se consumen; las tarjetas generan sus entradas.

## Transformaciones, fusión y cambio

Mantén cruceta ↑ y pulsa □/△/X/○ según la tarjeta; suelta ↑ para cerrar. L1/R1 cambian entre formas y fusiones solo si existen ambas. Se omiten categorías vacías; los personajes con solo fusión entran directamente en ella. Mantén cruceta ← para cambiar compañero. Se mantienen los requisitos de compañero, forma, barras, recarga y estado del combate. Los tres menús usan posiciones fijas, huecos vacíos y barras de vida de reserva. Online, los paneles locales aparecen a la izquierda incluso como Jugador 2, sin cambiar el lado lógico de las entradas.

## HUD y ajustes

Mantén R2 para ver ataques, comandos y costes. **Centered** equilibra centro y bordes; **Expanded** separa los paneles; **Classic (4:3)** usa el área 4:3 original. Se respeta el 4:3 nativo y las pantallas ultrapanorámicas usan áreas seguras. Puedes ocultar ataques y activar el marco externo, desactivado por defecto con ajustes nuevos. Se prefieren recursos HD disponibles y, si faltan, los originales; no se incluye un paquete de texturas. Lanzador oscuro, botones redondeados, esfera de tres estrellas, vista previa y 16 idiomas con árabe RTL; los nombres proceden del juego. **F6**: ataques; **F7**: marco; **F8**: posición. Pulsa el botón del atajo para cambiarlo, desactivarlo o restaurarlo; no se admiten duplicados. Requieren foco del juego y no se repiten al mantenerlos. El bloqueo impide cambios durante la lucha. Ruta, idioma, apariencia, atajos y cambios dentro del juego se guardan entre aperturas y actualizaciones. Si mueves el juego, selecciona la nueva ruta.

## Compatibilidad, online y pruebas

Windows y Linux: los cuatro menús quedan a la izquierda dentro de la vista de cada jugador, con tamaño estable en 4:3, 16:9, 16:10 y ultrapanorámico. Windows: comprobaciones adaptativas y detección automática del Decomp original, Char Package y Tag Team; datos de personajes personalizados y hasta cuatro mandos locales. Linux 1.0.6 incluye la nueva disposición, pero conserva la biblioteca y el resolutor anteriores, probados con v0.1.37/v0.1.38.1; no incluye las variantes Windows. Con cuatro vistas Tag Team permanecen los avisos boost nativos, sin superposición adicional. Char Package desactiva los personajes personalizados online. Las pruebas automáticas no sustituyen partidas reales; Linux nativo y rollback online siguen sin verificar. No se garantiza toda versión futura ni la ausencia total de desync.

## Código y licencia

Ambos ZIP incluyen código y licencia AGPL-3.0. Las fuentes conservan sus licencias. No se incluye el juego, texturas ni ajustes personales. Proyecto de fans no oficial; las marcas pertenecen a sus titulares.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
