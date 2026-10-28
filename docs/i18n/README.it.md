[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.1 Beta (Windows)

## 1.1 Beta — Windows

Windows 1.1 Beta: rilevamento delle battaglie Storia corretto nel Decomp originale, HUD disegnato solo nell’area dei pannelli, miglioramenti ad avvio e compatibilità, posizione ai bordi dello schermo e impostazioni conservate. Linux resta alla 1.0.6. Il rallentamento in Mission 100 con texture e ×8 all’avvio è ancora sotto indagine; la soluzione non è confermata. Grazie ad Aero per i test dettagliati e i log.

[1.1 — Changes / English](../Release-1.1.md)


## Comandi e guida all’installazione

Comandi e HUD ispirati a Sparking! ZERO per Tenkaichi3Decomp. Animazioni, danni, costi Ki, portata, abilità e finestre d’azione rimangono quelli di BT3. Gioco e risorse del gioco non sono inclusi.

## Novità della versione 1.0.6 Beta

- Windows e Linux: i quattro menu restano a sinistra nella visuale di ciascun giocatore, con dimensioni stabili in 4:3, 16:9, 16:10 e ultrawide. Windows: controlli di compatibilità adattivi e riconoscimento automatico del Decomp originale, Char Package e Tag Team, dati dei personaggi personalizzati e fino a quattro controller locali. Linux 1.0.6 include il nuovo layout ma mantiene libreria e risolutore precedenti, verificati con v0.1.37/v0.1.38.1; non supporta le varianti Windows. Con quattro visuali Tag Team restano gli indicatori boost nativi, senza sovrapposizione aggiuntiva. Char Package disabilita autonomamente i personaggi personalizzati online. I test automatici non sostituiscono partite reali; Linux nativo e rollback online non sono verificati. Nessuna garanzia universale per versioni future o desync.

[CHANGELOG](../../CHANGELOG.md)

## Download e dimostrazione

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-1.1-Beta-Windows.zip) · [**Linux x64 ZIP (1.0.6)**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-1.0.6-Beta-Linux-x64.zip) · [**1.1 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.1-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/SHA256SUMS.txt)

## Installazione

### Windows x64

Estrai lo ZIP Windows, avvia `BT3-SparkingControls.exe`, premi **Browse**, seleziona `Tenkaichi3Decomp.exe`, scegli lingua e opzioni e premi **Save and start**. Avvia sempre il gioco tramite l’assistente. L’EXE Windows integra le risorse e può essere condiviso da solo per l’uso normale; il destinatario deve avere il gioco. Lo ZIP include anche sorgenti e licenza. Non servono modloader, ricompilazione del gioco o sostituzione dei suoi file.

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Estrai tutto lo ZIP Linux x64 e tieni insieme EXE gestito, `libbt3sparking.so`, profili e risorse. Installa Mono con Windows Forms/System.Web.Extensions, libgdiplus e i font necessari. Usa X11 con compositing o XWayland funzionante. Nella cartella esegui `bash start.sh`, seleziona il gioco **nativo Linux** e premi Save and start. L’EXE usa Mono, senza Wine/Proton. Wayland nativo non è implementato. La libreria è compilata per glibc 2.35; altri sistemi potrebbero richiedere `bash build.sh`. Non avviare separatamente una seconda copia del gioco.

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## Comandi

□ = Quadrato; △ = Triangolo; ○ = Cerchio; LS = levetta sinistra. Le frecce di LS indicano la levetta, non la croce direzionale. I normali menu mantengono i comandi originali.

| Tasto / combinazione | Azione |
| --- | --- |
| `□` | Attacchi Rush |
| `△` | Colpi Ki |
| `X` | Movimento e scatti nativi |
| `R2` | Caricare il Ki |
| `L1` | Salire |
| `L2` | Scendere |
| `R2 + L1` | Z Dash verticale verso l’alto |
| `R2 + L2` | Z Dash verticale verso il basso |
| `R1` | Guardia fuori dalle proprie combo supportate |
| `R2 + □` | Super attacco 1 |
| `R2 + △` | Super attacco 2 |
| `R2 + ○` | Attacco supremo secondo i requisiti nativi |
| `R2 + D-pad ↑ / ↓` | Abilità 1 |
| `R2 + D-pad ← / →` | Abilità 2 |
| `○` | Posizione di contrattacco nativa |
| `R1 (combo)` | Teletrasporto frontale nativo nelle combo supportate |
| `R1 + LS (combo)` | Teletrasporto direzionale nativo in queste combo |
| `R1 + □` | Presa normale |
| `R1 + LS↓ + □` | Presa a terra alternativa, solo levetta |
| `LS↑ + △` | Presa a terra con il comando originale |
| `R3` | Emergency Blaster Wave quando consentito dal gioco |
| `L3` | Cercare / agganciare il bersaglio |
| `Select / View` | Cambiare telecamera |
| `D-pad ↑` | Tenere: trasformazioni/fusioni |
| `D-pad ←` | Tenere: cambio compagno |
| `D-pad ↓ / → (no R2)` | Bloccati in battaglia contro input involontari |

Puoi selezionare gli speciali con R2 già premuto. Per continuare a caricare, tieni premuto il pulsante d’attacco assegnato; rilasciare solo R2 non lo rilascia. Quando richiesto, usa □ + direzione indicata per il boost originale. La sovrapposizione copre solo Triangolo e lascia visibile la mano. Le scorciatoie delle abilità non riproducono esattamente ZERO.

○ richiede la posizione di contrattacco di BT3 con priorità sugli attacchi tenuti durante combo supportate; non aggiunge tutto il sistema Perception/Super Perception di ZERO. I teletrasporti R1 mantengono costi Ki e finestre nativi, senza garantire di finire dietro al nemico o restare fermi. R1 + □ ha priorità e genera X–rilascio–X, entro portata e finestre valide. Il doppio X originale rimane. La presa a terra non accetta croce ↓; LS↑ + ○ non la attiva. R3 usa condizioni e costo in salute di BT3, non il Revenge Counter di ZERO basato sui punti abilità. I vecchi comandi fisici R3 di trasformazione/fusione vengono consumati; le schede generano gli input richiesti.

## Trasformazioni, fusione e cambio

Tieni croce ↑ e premi □/△/X/○ della scheda; rilascia ↑ per chiudere. L1/R1 cambiano categoria solo se esistono sia trasformazioni che fusioni. Categorie vuote ignorate; solo fusione si apre direttamente. Tieni croce ← per cambiare compagno. Restano i requisiti di partner, forma, indicatori, ricarica e stato del combattimento. Schede in posizioni coerenti, spazi vuoti e barre salute delle riserve. Online i pannelli locali sono a sinistra anche come Giocatore 2, senza cambiare il lato logico degli input.

## HUD e impostazioni

Tieni R2 per attacchi, comandi e costi. **Centered** bilancia centro e bordi; **Expanded** distanzia i pannelli; **Classic (4:3)** usa l’area originale 4:3. Il 4:3 nativo resta invariato; ultrawide usa aree sicure. Lista attacchi nascondibile e cornice esterna opzionale, disattivata per nuove impostazioni. Risorse HD preferite se disponibili, altrimenti originali; nessun pacchetto texture incluso. Launcher scuro con pulsanti arrotondati, sfera a tre stelle, anteprima e 16 lingue con arabo RTL; nomi presi dal gioco. **F6**: attacchi; **F7**: cornice; **F8**: posizione. Clicca una scorciatoia per modificarla, disabilitarla o ripristinarla; duplicati rifiutati. Richiedono focus sul gioco e non si ripetono se tenute. Il blocco impedisce cambi durante la lotta. Percorso, lingua, opzioni, scorciatoie e cambi in partita persistono tra avvii e aggiornamenti. Se sposti il gioco, seleziona il nuovo percorso.

## Compatibilità, online e test

Windows e Linux: i quattro menu restano a sinistra nella visuale di ciascun giocatore, con dimensioni stabili in 4:3, 16:9, 16:10 e ultrawide. Windows: controlli di compatibilità adattivi e riconoscimento automatico del Decomp originale, Char Package e Tag Team, dati dei personaggi personalizzati e fino a quattro controller locali. Linux 1.0.6 include il nuovo layout ma mantiene libreria e risolutore precedenti, verificati con v0.1.37/v0.1.38.1; non supporta le varianti Windows. Con quattro visuali Tag Team restano gli indicatori boost nativi, senza sovrapposizione aggiuntiva. Char Package disabilita autonomamente i personaggi personalizzati online. I test automatici non sostituiscono partite reali; Linux nativo e rollback online non sono verificati. Nessuna garanzia universale per versioni future o desync.

## Sorgenti e licenza

Entrambi gli ZIP includono sorgenti e licenza AGPL-3.0. I font mantengono le proprie licenze. Nessun gioco, texture del gioco o impostazione personale inclusa. Progetto fan non ufficiale; marchi dei rispettivi titolari.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
