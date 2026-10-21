[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.0 Beta

## Sterowanie i instrukcja instalacji

Sterowanie i interfejs walki inspirowane Sparking! ZERO dla Tenkaichi3Decomp. Obowiązują animacje, obrażenia, koszty Ki, zasięg, zdolności i okna akcji BT3. Gra ani jej zasoby nie są dołączone.

## Pobieranie i demonstracja

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Windows.zip) · [**Linux x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Linux-x64.zip) · [**1.0 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.0-beta)

[▶ Gameplay demo — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/SHA256SUMS.txt)

## Instalacja

### Windows x64

Rozpakuj ZIP Windows, uruchom `BT3-SparkingControls.exe`, kliknij **Browse**, wybierz `Tenkaichi3Decomp.exe`, ustaw język i wygląd, a następnie **Save and start**. Zawsze uruchamiaj grę przez pomocnik. EXE Windows zawiera zasoby pomocnika i do zwykłego użycia można udostępnić sam plik; odbiorca potrzebuje gry. ZIP zawiera też kod i licencję. Nie wymaga modloadera, rekompilacji gry ani podmiany jej plików.

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Rozpakuj cały ZIP Linux x64. Zachowaj razem zarządzany EXE, `libbt3sparking.so`, profile i zasoby. Zainstaluj Mono z Windows Forms/System.Web.Extensions, libgdiplus i czcionki danego języka. Używaj X11 z kompozytorem lub działającego XWayland. W katalogu uruchom `bash start.sh`, wybierz **natywną grę Linux** i Save and start. EXE działa przez Mono, bez Wine/Proton. Natywny Wayland nie jest obsługiwany. Bibliotekę skompilowano dla glibc 2.35; inny system może wymagać `bash build.sh`. Nie uruchamiaj osobno drugiej kopii gry.

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## Sterowanie

□ = Kwadrat; △ = Trójkąt; ○ = Kółko; LS = lewy drążek. Strzałki LS oznaczają drążek, nie krzyżak. Zwykłe menu zachowują oryginalne sterowanie.

| Przycisk / kombinacja | Działanie |
| --- | --- |
| `□` | Ataki Rush |
| `△` | Pociski Ki |
| `X` | Natywny ruch i doskoki |
| `R2` | Ładowanie Ki |
| `L1` | Wznoszenie |
| `L2` | Opadanie |
| `R1` | Blok poza własnymi obsługiwanymi kombinacjami |
| `R2 + □` | Superatak 1 |
| `R2 + △` | Superatak 2 |
| `R2 + ○` | Atak ostateczny zgodnie z warunkami gry |
| `R2 + D-pad ↑ / ↓` | Zdolność 1 |
| `R2 + D-pad ← / →` | Zdolność 2 |
| `○` | Natywna postawa kontry |
| `R1 (combo)` | Natywny teleport do przodu w obsługiwanym combo |
| `R1 + LS (combo)` | Natywny teleport kierunkowy w tym combo |
| `R1 + □` | Zwykły chwyt |
| `R1 + LS↓ + □` | Alternatywny chwyt na ziemi, tylko drążek |
| `LS↑ + △` | Chwyt na ziemi oryginalnym poleceniem |
| `R3` | Emergency Blaster Wave zgodnie z warunkami gry |
| `L3` | Wyszukiwanie / namierzanie celu |
| `Select / View` | Zmiana kamery |
| `D-pad ↑` | Przytrzymaj: transformacje/fuzje |
| `D-pad ←` | Przytrzymaj: zmiana członka drużyny |
| `D-pad ↓ / → (no R2)` | Zablokowane w walce, aby zapobiec pomyłkom |

Specjały można wybierać przy już trzymanym R2. Aby dalej ładować atak, trzymaj przypisany przycisk ataku; samo puszczenie R2 go nie zwalnia. Przy komunikacie boost użyj □ + wskazanego kierunku. Nakładka zastępuje tylko Trójkąt i nie zasłania dłoni. Skróty zdolności nie odtwarzają dokładnie sterowania ZERO.

○ wywołuje kontrę BT3 z priorytetem nad trzymanym atakiem w obsługiwanych combo, nie pełne Perception/Super Perception ZERO. Teleporty R1 wymagają natywnego Ki i okien akcji; nie gwarantują pozycji za przeciwnikiem ani pozostania w miejscu. R1 + □ ma priorytet i generuje X–zwolnienie–X, zależnie od zasięgu i dostępności. Oryginalne podwójne X pozostaje. Chwyt na ziemi nie używa krzyżaka ↓; LS↑ + ○ go nie uruchamia. R3 zużywa zdrowie na warunkach BT3, zamiast punktów umiejętności Revenge Counter z ZERO. Fizyczne oryginalne skróty R3 transformacji/fuzji są przechwytywane; karty generują wymagane wejścia.

## Transformacje, fuzja i zmiana postaci

Przytrzymaj krzyżak ↑ i naciśnij □/△/X/○ z karty; puść ↑, by zamknąć. L1/R1 przełączają tylko istniejące transformacje i fuzje. Puste kategorie są pomijane; sama fuzja otwiera się od razu. Przytrzymaj krzyżak ←, aby wybrać kolegę z drużyny. Partner, forma, wskaźniki, odnowienie i stan walki nadal muszą spełniać warunki BT3. Stałe pozycje kart, puste miejsca i zdrowie rezerwowych. Online lokalne panele są po lewej także dla Gracza 2, bez zmiany logicznej strony wejścia.

## HUD i ustawienia

Przytrzymaj R2, aby zobaczyć ataki, polecenia i koszty. **Centered** równoważy środek i brzegi; **Expanded** rozsuwa panele; **Classic (4:3)** używa oryginalnej strefy 4:3. Natywne 4:3 bez zmian; ultrawide korzysta z bezpiecznych stref. Listę ataków można ukryć; zewnętrzna ramka domyślnie wyłączona w nowych ustawieniach. Preferowane są dostępne grafiki HD, w przeciwnym razie oryginalne; bez paczki tekstur. Ciemny launcher, zaokrąglone przyciski, kula z trzema gwiazdami, podgląd, 16 języków i arabskie RTL. Nazwy pochodzą z gry. **F6**: ataki; **F7**: ramka; **F8**: pozycja. Kliknij skrót, by zmienić, wyłączyć lub przywrócić domyślny; duplikaty są odrzucane. Wymagają aktywnego okna gry i nie powtarzają się przy trzymaniu. Blokada chroni przed zmianami w walce. Ścieżka, język, opcje, skróty i zmiany w grze są zachowywane między uruchomieniami/aktualizacjami. Po przeniesieniu gry wybierz nową ścieżkę.

## Zgodność, online i testy

Pomocnik tłumaczy lokalny pakiet kontrolera 18 bajtów przed próbkowaniem sieci; drugi gracz otrzymuje natywne wejścia. Nie zmienia wejścia zdalnego ani funkcji decyzji bojowych. Symbole, 61 pełnych funkcji i odwołania pozwalają sprawdzić zgodne zmiany adresów; zmiany strukturalne lub brak symboli mogą wymagać aktualizacji. Sprawdzono Linux v0.1.37 i v0.1.38.1, bez gwarancji wszystkich wersji. Proste mapowania nie dodają sleep. Chwyt/R3 używają faz dwóch próbek; druga aktywacja około 33 ms później przy 60 Hz. Karty zachowują natywny czas/potwierdzenia; zasięg, okna i rollback nadal obowiązują. Zaliczono: 480 183 testy pakietów/ABI Windows; 160 061 pakietów i 9 emulowanego loadera dla KAŻDEJ wersji Linux; 2250 launchera, 93 skrótów, 693 układu na interfejs i 12 stanów podglądu. UI Linux testowano w .NET na Windows, nie Mono/X11. Natywny Linux, fizyczne kontrolery i prawdziwe mecze/rollback online niezweryfikowane. Brak uniwersalnej gwarancji bez desynchronizacji.

## Kod i licencja

Oba ZIP-y zawierają kod i licencję AGPL-3.0. Czcionki zachowują swoje licencje. Bez gry, jej tekstur lub ustawień osobistych. Nieoficjalny projekt fanowski; znaki należą do ich właścicieli.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
