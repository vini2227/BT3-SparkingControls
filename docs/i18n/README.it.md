[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.0.3 Beta

## Comandi e guida all’installazione

Comandi e HUD ispirati a Sparking! ZERO per Tenkaichi3Decomp. Animazioni, danni, costi Ki, portata, abilità e finestre d’azione rimangono quelli di BT3. Gioco e risorse del gioco non sono inclusi.

## Novità della versione 1.0.3 Beta

- Tieni premuto R2 + L1 per salire rapidamente o R2 + L2 per scendere. L1/L2 da soli mantengono il movimento normale; il vecchio doppio tocco è disabilitato. Una seconda pressione rapida può attendere la finestra nativa di sette fotogrammi.
- Rilasciando ○ mentre tieni R1, l’ausilio richiede l’uscita dalla postura di contrattacco e riprende la guardia, rispettando la finestra di uscita dell’animazione di BT3. Funziona con durate diverse senza premere nuovamente R1.
- I quattro menu hanno una dissolvenza in entrata di 100 ms e in uscita di 80 ms, senza attese aggiuntive per i comandi. Su Linux, i fotogrammi modificati o animati vengono presentati immediatamente.

[CHANGELOG](../../CHANGELOG.md)

## Download e dimostrazione

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-1.0.3-Beta-Windows.zip) · [**Linux x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-1.0.3-Beta-Linux-x64.zip) · [**1.0.3 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.0.3-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/SHA256SUMS.txt)

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

Traduce il pacchetto locale di 18 byte prima del campionamento di rete; l’altro giocatore riceve input nativi. Non modifica input remoti o decisioni di combattimento. Simboli, 61 funzioni complete e riferimenti verificano modifiche compatibili agli indirizzi; cambi strutturali o simboli mancanti possono richiedere aggiornamenti. Verificati Linux v0.1.37 e v0.1.38.1, senza garanzia per tutte le versioni. Nessuno sleep nei remap semplici. Presa e R3 usano fasi di due campioni; seconda pressione circa 33 ms dopo a 60 Hz. Le schede seguono tempi/conferme nativi; portata, finestre e rollback restano rilevanti. Superati: 1.337.289 controlli pacchetti/ABI Windows; 445.763 pacchetti e 9 loader emulati per CIASCUNA versione Linux; 2.250 launcher, 93 scorciatoie, 693 layout per frontend e 12 stati anteprima. UI Linux testata in .NET su Windows, non Mono/X11. Linux reale, controller fisici e partite/rollback online non ancora verificati. Nessuna garanzia universale contro desync.

## Sorgenti e licenza

Entrambi gli ZIP includono sorgenti e licenza AGPL-3.0. I font mantengono le proprie licenze. Nessun gioco, texture del gioco o impostazione personale inclusa. Progetto fan non ufficiale; marchi dei rispettivi titolari.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
