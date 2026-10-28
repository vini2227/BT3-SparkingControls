[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.1 Beta

## 1.1 Beta — Windows / Linux

Windows 1.1 Beta: korrigierte Kampferkennung im Story-Modus des Original-Decomp, zugeschnittene HUD-Zeichenflächen, verbesserter Start und Kompatibilitätsprüfung, Platzierung am Bildschirmrand und erhaltene Einstellungen. Linux 1.1 ist ebenfalls verfügbar: zugeschnittenes HUD, wiederverwendetes X11-Bild, Story-Kampferkennung und Bildschirmrand-Layout. Windows-Varianten werden nicht unterstützt; echte Linux/X11-Partien sind nicht verifiziert. Die Verlangsamung in Mission 100 mit Texturen und ×8 beim Start wird weiter untersucht; eine Behebung ist nicht bestätigt. Vielen Dank an Aero für ausführliche Tests und Protokolle.

[1.1 — Changes / English](../Release-1.1.md)


## Steuerung und Einrichtung

Eine von Sparking! ZERO inspirierte Steuerung mit Kampf-HUD für Tenkaichi3Decomp. Animationen, Schaden, Ki-Kosten, Reichweite, Fähigkeiten und Aktionsfenster stammen weiterhin aus BT3. Spiel und Spielressourcen sind nicht enthalten.

## Neu in Version 1.0.6 Beta

- Windows und Linux: Alle vier Menüs stehen links im jeweiligen Spielerbereich, mit stabiler Größe für 4:3, 16:9, 16:10 und Ultrawide. Windows: adaptive Kompatibilitätsprüfung und automatische Erkennung von Original-Decomp, Char Package und Tag Team; Daten eigener Charaktere und bis zu vier lokale Controller. Linux 1.0.6 enthält das neue Layout, behält jedoch die bisherige native Bibliothek und Auflösung für die geprüften Versionen v0.1.37/v0.1.38.1; Windows-Varianten werden nicht unterstützt. Bei vier Tag-Team-Ansichten bleiben native Boost-Hinweise ohne zusätzliche Überlagerung sichtbar. Char Package deaktiviert eigene Charaktere selbst im Onlinemodus. Automatisierte Tests ersetzen keine echten Partien; natives Linux und Online-Rollback sind nicht verifiziert. Keine Garantie für jede zukünftige Version oder gegen jeden Desync.

[CHANGELOG](../../CHANGELOG.md)

## Downloads und Demonstration

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-1.1-Beta-Windows.zip) · [**Linux x64 ZIP (1.1)**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-1.1-Beta-Linux-x64.zip) · [**1.1 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.1-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/SHA256SUMS.txt)

## Installation

### Windows x64

Windows-ZIP entpacken, `BT3-SparkingControls.exe` starten, über **Browse** die `Tenkaichi3Decomp.exe` auswählen, Sprache und Darstellung einstellen und **Save and start** anklicken. Das Spiel immer über den Helfer starten. Die Windows-EXE enthält ihre Ressourcen und kann zur normalen Nutzung einzeln weitergegeben werden; der Empfänger benötigt das Spiel. Das ZIP enthält zusätzlich Quellcode und Lizenz. Kein Modloader, Austausch von Spieldateien oder Neukompilieren des Spiels nötig.

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Das gesamte Linux-x64-ZIP entpacken. Verwaltete EXE, `libbt3sparking.so`, Profile und Ressourcen zusammenlassen. Mono mit Windows Forms/System.Web.Extensions, libgdiplus und passende Schriftarten installieren. X11 mit Compositor oder funktionierendes XWayland verwenden. Im Ordner `bash start.sh` ausführen, das **native Linux-Spiel** auswählen und Save and start anklicken. Die .exe läuft mit Mono, ohne Wine/Proton. Natives Wayland wird nicht unterstützt. Die Bibliothek wurde für glibc 2.35 kompiliert; andere Systeme erfordern eventuell `bash build.sh`. Keine zweite Spielinstanz separat starten.

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## Steuerung

□ = Quadrat, △ = Dreieck, ○ = Kreis, LS = linker Analogstick. LS-Pfeile bezeichnen den Stick, nicht das Steuerkreuz. Die normale Menünavigation bleibt unverändert.

| Taste / Kombination | Aktion |
| --- | --- |
| `□` | Rush-Angriffe |
| `△` | Ki-Schüsse |
| `X` | Native Bewegung und Dash |
| `R2` | Ki aufladen |
| `L1` | Aufsteigen |
| `L2` | Absteigen |
| `R2 + L1` | Vertikaler Z Dash nach oben |
| `R2 + L2` | Vertikaler Z Dash nach unten |
| `R1` | Blocken außerhalb eigener unterstützter Kombos |
| `R2 + □` | Superangriff 1 |
| `R2 + △` | Superangriff 2 |
| `R2 + ○` | Ultimativer Angriff unter nativen Voraussetzungen |
| `R2 + D-pad ↑ / ↓` | Fähigkeit 1 |
| `R2 + D-pad ← / →` | Fähigkeit 2 |
| `○` | Native Konterhaltung |
| `R1 (combo)` | Nativer Vorwärtsteleport in unterstützten Kombos |
| `R1 + LS (combo)` | Nativer Richtungsteleport in diesen Kombos |
| `R1 + □` | Normaler Griff |
| `R1 + LS↓ + □` | Alternativer Bodengriff, nur Analogstick |
| `LS↑ + △` | Bodengriff mit Originaleingabe |
| `R3` | Emergency Blaster Wave bei nativer Verfügbarkeit |
| `L3` | Ziel suchen / anvisieren |
| `Select / View` | Kamera wechseln |
| `D-pad ↑` | Halten: Verwandlungen/Fusionen |
| `D-pad ←` | Halten: Teamwechsel |
| `D-pad ↓ / → (no R2)` | Im Kampf gegen unbeabsichtigte Eingaben gesperrt |

Spezialangriffe lassen sich bei bereits gehaltenem R2 wählen. Zum weiteren Aufladen die zugewiesene Angriffstaste halten; R2 allein loszulassen beendet dies nicht. Bei einer Boost-Aufforderung □ + angezeigte Richtung verwenden. Das Overlay ersetzt nur das Dreieck und lässt die Hand sichtbar. Die Fähigkeitskürzel sind keine exakte Kopie der ZERO-Steuerung.

○ fordert BT3s Konterhaltung mit Vorrang vor gehaltenen Angriffen in unterstützten Kombos an, nicht ZERO s vollständiges Perception/Super-Perception-System. R1-Teleports behalten native Ki-Anforderungen und Zeitfenster; sie garantieren keine Position hinter dem Gegner und keinen Stillstand. R1 + □ hat Vorrang und erzeugt X–Loslassen–X, abhängig von Reichweite und Verfügbarkeit. Doppel-X bleibt erhalten. Steuerkreuz ↓ wählt keinen Bodengriff; LS↑ + ○ löst ihn nicht aus. R3 verwendet BT3s Lebenskosten und Bedingungen statt ZERO s Skill-Count-Revenge-Counter. Originale physische R3-Verwandlungs-/Fusionseingaben werden abgefangen; die Karten erzeugen die nötigen Eingaben intern.

## Verwandlung, Fusion und Wechsel

Steuerkreuz ↑ halten und die auf der Karte angezeigte Taste □/△/X/○ drücken; Loslassen schließt. L1/R1 wechseln nur zwischen vorhandenen Verwandlungen und Fusionen. Leere Kategorien werden übersprungen; reine Fusionscharaktere öffnen direkt Fusionen. Steuerkreuz ← halten, um Teammitglieder zu wählen. Partner, Form, Anzeigen, Abklingzeiten und Kampfzustand bleiben native Voraussetzungen. Einheitliche Kartenpositionen, leere Platzhalter und Reserve-Lebensbalken. Online stehen lokale Menüs links, auch als Spieler 2; die logische Eingabeseite bleibt korrekt.

## HUD und Einstellungen

R2 halten: Angriffe, Eingaben und Kosten. **Centered** liegt zwischen Mitte und Rand, **Expanded** verteilt die Panels weiter, **Classic (4:3)** nutzt den ursprünglichen 4:3-Bereich. Natives 4:3 bleibt erhalten; Ultrawide nutzt sichere Bereiche. Angriffsliste ausblendbar, Außenrahmen bei neuen Einstellungen standardmäßig aus. Verfügbare HD-Ressourcen werden bevorzugt, sonst Originale; kein Texturpaket enthalten. Dunkler Launcher mit runden Tasten, Drei-Sterne-Kugel, Vorschau und 16 Sprachen inklusive Arabisch RTL. Spielnamen stammen aus dem Spiel. **F6**: Angriffe, **F7**: Rahmen, **F8**: Position. Kürzel anklicken, ändern, deaktivieren oder zurücksetzen; doppelte Belegung abgelehnt. Nur bei Spielfokus, ohne Wiederholung bei Halten. Optional im Kampf sperren. Pfad, Sprache, Darstellung, Kürzel und Änderungen im Spiel bleiben nach Neustarts/Updates gespeichert. Bei verschobenem Spiel den neuen Pfad wählen.

## Kompatibilität, Online und Tests

Windows und Linux: Alle vier Menüs stehen links im jeweiligen Spielerbereich, mit stabiler Größe für 4:3, 16:9, 16:10 und Ultrawide. Windows: adaptive Kompatibilitätsprüfung und automatische Erkennung von Original-Decomp, Char Package und Tag Team; Daten eigener Charaktere und bis zu vier lokale Controller. Linux 1.0.6 enthält das neue Layout, behält jedoch die bisherige native Bibliothek und Auflösung für die geprüften Versionen v0.1.37/v0.1.38.1; Windows-Varianten werden nicht unterstützt. Bei vier Tag-Team-Ansichten bleiben native Boost-Hinweise ohne zusätzliche Überlagerung sichtbar. Char Package deaktiviert eigene Charaktere selbst im Onlinemodus. Automatisierte Tests ersetzen keine echten Partien; natives Linux und Online-Rollback sind nicht verifiziert. Keine Garantie für jede zukünftige Version oder gegen jeden Desync.

## Quellcode und Lizenz

Beide ZIPs enthalten Quellcode und AGPL-3.0-Lizenz. Schriftlizenzen bleiben erhalten. Kein Spiel, Spieltexturpaket oder persönliche Einstellungen enthalten. Inoffizielles Fanprojekt; Marken gehören ihren jeweiligen Inhabern.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
