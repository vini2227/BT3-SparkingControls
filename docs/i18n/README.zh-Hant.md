[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.0.3 Beta

## 操作與安裝指南

為 Tenkaichi3Decomp 提供受 Sparking! ZERO 啟發的操作配置與戰鬥 HUD。動畫、傷害、氣力消耗、距離、角色能力和動作窗口仍遵循 BT3。安裝包不包含遊戲或其資源。

## 1.0.3 Beta 版本更新

- 按住 R2 + L1 快速上升，或 R2 + L2 快速下降。單獨按 L1/L2 仍為普通移動；舊的雙擊快捷操作已停用。快速第二次按鍵可能需要等待遊戲原有的七影格雙擊視窗結束。
- 持續按住 R1 時放開 ○，輔助程式會請求退出反擊架勢並恢復防禦，仍遵守 BT3 動畫原有的退出時機。支援不同的按住時間，無需再次按 R1。
- 四種選單以 100 毫秒淡入、80 毫秒淡出，不增加輸入等待。Linux 會立即顯示已變更或播放動畫的畫面。

[CHANGELOG](../../CHANGELOG.md)

## 下載與示範

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-1.0.3-Beta-Windows.zip) · [**Linux x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-1.0.3-Beta-Linux-x64.zip) · [**1.0.3 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.0.3-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.3-beta/SHA256SUMS.txt)

## 安裝

### Windows x64

解壓縮 Windows ZIP，執行 `BT3-SparkingControls.exe`，按 **Browse** 選取 `Tenkaichi3Decomp.exe`，設定語言和介面後按 **Save and start**。請一律透過輔助程式啟動遊戲。Windows EXE 已內建輔助資源，日常使用可單獨分享，但接收者仍需自行準備遊戲。ZIP 同時包含原始碼與授權。無需模組載入器、重新編譯遊戲或替換遊戲檔案。

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

完整解壓縮 Linux x64 ZIP，將受控 EXE、`libbt3sparking.so`、設定檔與資源保存在同一資料夾。安裝支援 Windows Forms/System.Web.Extensions 的 Mono、libgdiplus 及對應字型，使用支援合成的 X11 或正常運作的 XWayland。在該目錄執行 `bash start.sh`，選取 **Linux 原生遊戲執行檔**，再按 Save and start。EXE 使用 Mono，不需 Wine/Proton。尚不支援原生 Wayland。函式庫以 glibc 2.35 為目標交叉編譯；其他系統可能需要 `bash build.sh`。不要另行啟動第二個遊戲實例。

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## 操作

□ = 方塊；△ = 三角；○ = 圓圈；LS = 左類比搖桿。LS 箭頭表示搖桿方向，不是方向鍵。一般選單保留遊戲原始操作。

| 按鍵／組合 | 動作 |
| --- | --- |
| `□` | Rush 連擊 |
| `△` | 氣彈 |
| `X` | 原生移動與衝刺 |
| `R2` | 集氣 |
| `L1` | 上升 |
| `L2` | 下降 |
| `R2 + L1` | 向上垂直 Z Dash |
| `R2 + L2` | 向下垂直 Z Dash |
| `R1` | 自身支援連段之外的防禦 |
| `R2 + □` | 超級攻擊 1 |
| `R2 + △` | 超級攻擊 2 |
| `R2 + ○` | 究極攻擊，仍須滿足原生條件 |
| `R2 + D-pad ↑ / ↓` | 技能 1 |
| `R2 + D-pad ← / →` | 技能 2 |
| `○` | 原生反擊架勢 |
| `R1 (combo)` | 支援連段中的原生向前瞬移 |
| `R1 + LS (combo)` | 這些連段中的原生方向瞬移 |
| `R1 + □` | 一般抓投 |
| `R1 + LS↓ + □` | 替代倒地抓投，僅限搖桿 |
| `LS↑ + △` | 原始指令倒地抓投 |
| `R3` | Emergency Blaster Wave，須符合原生條件 |
| `L3` | 搜尋／鎖定目標 |
| `Select / View` | 切換鏡頭 |
| `D-pad ↑` | 按住：變身／融合 |
| `D-pad ←` | 按住：切換隊員 |
| `D-pad ↓ / → (no R2)` | 戰鬥中停用以防誤觸 |

已按住 R2 時也能選擇特殊攻擊。要繼續蓄力，請按住對應攻擊鍵；只放開 R2 不會放開攻擊鍵。出現提示時，以 □ + 指定方向輸入原生 boost。覆蓋圖示只遮住三角，不遮手部圖案。技能快捷鍵屬於本輔助程式，並非 ZERO 操作的完全複製。

○ 請求 BT3 原生反擊架勢，在支援連段中優先於持續按住的攻擊，並非完整的 ZERO Perception/Super Perception 系統。R1 瞬移仍須滿足原生氣力及取消窗口，不保證移到敵人背後或停在原地。R1 + □ 優先產生 X—放開—X，不能繞過距離及動作窗口；保留原始雙按 X 抓投。倒地抓投不接受方向鍵 ↓；LS↑ + ○ 不再觸發。R3 使用 BT3 的生命消耗與條件，而非消耗技能點的 ZERO Revenge Counter。原始實體 R3 變身／融合操作會被接管，由卡片內部產生所需指令。

## 變身、融合與隊員切換

按住方向鍵 ↑，按卡片上的 □/△/X/○ 選取；放開 ↑ 關閉。只有同時存在變身和融合時，L1/R1 才切換分類。跳過空分類；僅有融合的角色直接開啟融合。按住方向鍵 ← 切換隊員。夥伴、型態、量表、冷卻和戰鬥狀態要求仍由 BT3 決定。三個選單的卡片位置一致，保留空格並顯示後備生命條。連線時本地面板在左側，即使邏輯上是玩家 2，也不改變輸入對應的玩家。

## HUD 與設定

按住 R2 查看招式、按鍵和消耗。**Centered** 平衡中央與邊緣；**Expanded** 拉開面板距離；**Classic (4:3)** 使用原始 4:3 區域。原生 4:3 位置不變，超寬螢幕採用安全區域。攻擊列表可隱藏，新設定預設關閉外框。優先使用既有高畫質資源，缺少時回退至原始資源，不附紋理包。啟動器有深色主題、圓角按鈕、三星龍珠圖示、預覽及 16 種語言與阿拉伯語 RTL；角色／招式名稱來自遊戲。**F6**：攻擊顯示；**F7**：外框；**F8**：位置。點選快捷鍵按鈕更改、停用或還原預設，不允許重複綁定。僅遊戲取得焦點時生效，長按不重複觸發。可鎖定戰鬥中的快捷鍵。路徑、語言、外觀、快捷鍵和遊戲內變更會跨啟動／更新保存；移動遊戲後須重選路徑。

## 相容性、連線與測試

輔助程式在網路取樣前轉換本地 18 位元組控制器封包，對手收到 BT3 原生輸入，不修改遠端輸入或戰鬥決策函式。透過符號、61 個完整函式及參照驗證相容位址變動；結構改變或缺少符號仍可能需要更新。Linux v0.1.37 與 v0.1.38.1 已通過相容檢查，不保證支援所有舊版或新版。簡單映射不額外加入 sleep；抓投／R3 每階段兩次取樣，60 Hz 時第二次按下約晚 33 毫秒。卡片遵循原生時序與確認，距離、窗口及 rollback 仍影響結果。通過 Windows 1,337,289 項封包／ABI、每個 Linux 版本 445,763 項封包及 9 項模擬載入器、每前端 2,250 項啟動器、93 項快捷鍵、693 項版面檢查及 12 個預覽狀態。Linux 介面於 Windows .NET 而非 Mono/X11 測試。實際 Linux 執行、實體控制器及真實連線／rollback 尚未驗證，不保證完全無失步。

## 原始碼與授權

兩個 ZIP 都包含原始碼及 AGPL-3.0 授權，字型保留各自授權。不含遊戲、遊戲紋理包或個人設定。本專案為非官方粉絲作品，商標歸各自權利人。

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
