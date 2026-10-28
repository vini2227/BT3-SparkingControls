[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.1

## 1.1 — Windows / Linux

Windows 1.1: 원본 Decomp의 스토리 전투 감지를 수정하고 HUD 그리기 영역을 패널 크기로 줄였습니다. 시작 및 호환성 검사를 개선하고 화면 가장자리 배치를 추가했으며 설정은 유지됩니다. Linux 1.1도 제공됩니다. HUD 영역 축소, X11 이미지 재사용, 스토리 전투 감지 및 화면 가장자리 배치를 포함합니다. Windows 변형 버전은 지원하지 않으며 실제 Linux/X11 플레이는 검증되지 않았습니다. 시작부터 텍스처와 ×8을 켠 Mission 100의 성능 저하는 조사 중이며 해결이 확인되지 않았습니다. 자세한 테스트와 로그를 제공한 Aero에게 감사드립니다.

[1.1 — Changes / English](../Release-1.1.md)


## 조작 및 설치 안내

Tenkaichi3Decomp용 Sparking! ZERO 스타일 조작 배치와 전투 HUD입니다. 애니메이션, 피해량, 기 소모, 사거리, 캐릭터 능력과 동작 허용 시간은 BT3 규칙을 따릅니다. 게임과 게임 리소스는 포함되지 않습니다.

## 1.0.6 Beta 변경 사항

- Windows와 Linux: 네 종류의 메뉴를 각 플레이어 화면 안쪽 왼편에 배치하고 4:3, 16:9, 16:10, 울트라와이드에서 안정적인 크기를 유지합니다. Windows: 적응형 호환성 검사와 원본 Decomp, Char Package, Tag Team 자동 감지, 사용자 캐릭터 데이터 및 최대 4개의 로컬 컨트롤러 지원. Linux 1.0.6은 새 분할 화면 배치를 포함하지만 기존 네이티브 라이브러리와 분석기를 유지하며 v0.1.37/v0.1.38.1을 검사했습니다. Windows 변형 버전 지원은 포함하지 않습니다. Tag Team 4화면에서는 추가 boost 아이콘 대신 원래 안내를 표시합니다. Char Package 자체가 사용자 캐릭터의 온라인 사용을 차단합니다. 자동 테스트는 실제 대전을 대신하지 않으며 네이티브 Linux와 온라인 롤백은 검증되지 않았습니다. 모든 향후 버전이나 동기화 오류 방지를 보장하지 않습니다.

[CHANGELOG](../../CHANGELOG.md)

## 다운로드 및 시연

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1/BT3-SparkingControls-1.1-Windows.zip) · [**Linux x64 ZIP (1.1)**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1/BT3-SparkingControls-1.1-Linux-x64.zip) · [**1.1**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.1)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1/SHA256SUMS.txt)

## 설치

### Windows x64

Windows ZIP을 압축 해제하고 `BT3-SparkingControls.exe`를 실행합니다. **Browse**로 `Tenkaichi3Decomp.exe`를 선택한 뒤 언어와 표시를 설정하고 **Save and start**를 누르세요. 게임은 항상 보조 앱을 통해 시작하세요. Windows EXE는 보조 리소스를 포함하므로 일반 사용자는 EXE만 공유해도 되지만 게임은 별도로 필요합니다. ZIP에는 소스와 라이선스도 있습니다. 모드 로더, 게임 재컴파일, 게임 파일 교체는 필요하지 않습니다.

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Linux x64 ZIP 전체를 해제하고 관리형 EXE, `libbt3sparking.so`, 프로필과 리소스를 함께 보관하세요. Windows Forms/System.Web.Extensions 지원 Mono, libgdiplus와 필요한 글꼴을 설치하고 합성을 지원하는 X11 또는 정상 작동하는 XWayland를 사용하세요. 해당 폴더에서 `bash start.sh`를 실행하고 **Linux 네이티브 게임 실행 파일**을 선택한 뒤 Save and start를 누릅니다. EXE는 Mono로 실행되며 Wine/Proton은 필요 없습니다. 네이티브 Wayland는 미지원입니다. 라이브러리는 glibc 2.35 대상으로 크로스 컴파일되었으므로 다른 환경은 `bash build.sh`가 필요할 수 있습니다. 게임을 별도로 중복 실행하지 마세요.

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## 조작

□ = 네모, △ = 세모, ○ = 동그라미, LS = 왼쪽 스틱. LS 화살표는 방향 패드가 아닌 스틱 방향입니다. 일반 메뉴 탐색은 원래 조작을 유지합니다.

| 버튼 / 조합 | 동작 |
| --- | --- |
| `□` | 러시 공격 |
| `△` | 기탄 |
| `X` | 기존 이동 및 대시 |
| `R2` | 기 모으기 |
| `L1` | 상승 |
| `L2` | 하강 |
| `R2 + L1` | 위쪽 수직 Z Dash |
| `R2 + L2` | 아래쪽 수직 Z Dash |
| `R1` | 지원되는 자신의 콤보 이외의 가드 |
| `R2 + □` | 슈퍼 공격 1 |
| `R2 + △` | 슈퍼 공격 2 |
| `R2 + ○` | 원래 조건에 따른 궁극기 |
| `R2 + D-pad ↑ / ↓` | 스킬 1 |
| `R2 + D-pad ← / →` | 스킬 2 |
| `○` | 기존 반격 자세 |
| `R1 (combo)` | 지원되는 콤보 중 기존 전방 순간이동 |
| `R1 + LS (combo)` | 해당 콤보 중 방향 순간이동 |
| `R1 + □` | 일반 잡기 |
| `R1 + LS↓ + □` | 대체 다운 잡기, 스틱만 사용 |
| `LS↑ + △` | 원래 입력의 다운 잡기 |
| `R3` | 원래 조건 충족 시 Emergency Blaster Wave |
| `L3` | 적 탐색 / 록온 |
| `Select / View` | 카메라 변경 |
| `D-pad ↑` | 길게: 변신/합체 |
| `D-pad ←` | 길게: 팀원 교대 |
| `D-pad ↓ / → (no R2)` | 실수 방지를 위해 전투 중 차단 |

R2를 이미 누른 상태에서도 특수기를 선택할 수 있습니다. 충전을 계속하려면 지정된 공격 버튼을 유지하세요. R2만 놓아도 공격 버튼은 해제되지 않습니다. 부스트 안내가 나오면 □ + 지정 방향을 사용합니다. 덧씌운 아이콘은 세모만 가리고 손 그림은 보존합니다. 스킬 단축 조작은 ZERO의 정확한 복제가 아닙니다.

○는 지원 콤보에서 유지 중인 공격보다 우선하여 BT3 반격 자세를 요청하며 ZERO의 전체 Perception/Super Perception을 추가하지 않습니다. R1 순간이동은 원래 기 요구량과 캔슬 가능 시간에 따르며 상대 뒤 이동이나 제자리 이동을 보장하지 않습니다. R1 + □는 우선적으로 X–해제–X를 생성하지만 사거리와 허용 시간이 필요합니다. 기존 X 두 번 잡기는 유지됩니다. 다운 잡기는 방향 패드 ↓를 사용하지 않으며 LS↑ + ○로도 발동하지 않습니다. R3는 BT3 체력 소모와 조건을 사용하며 ZERO의 스킬 카운트 기반 Revenge Counter와 다릅니다. 기존 물리 R3 변신/합체 입력은 차단되고 카드가 필요한 입력을 내부 생성합니다.

## 변신, 합체 및 교대

방향 패드 ↑를 누른 채 카드에 표시된 □/△/X/○를 선택하고 ↑를 놓으면 닫습니다. 변신과 합체가 둘 다 있을 때만 L1/R1로 분류를 전환합니다. 빈 분류는 건너뛰며 합체만 있으면 바로 합체를 엽니다. 방향 패드 ←를 눌러 팀원을 선택합니다. 파트너, 형태, 게이지, 재사용 대기시간과 전투 상태 조건은 BT3를 따릅니다. 카드 위치를 통일하고 빈 칸과 대기 팀원 체력을 표시합니다. 온라인에서는 플레이어 2라도 로컬 패널이 왼쪽에 나오지만 논리적 입력 측은 바뀌지 않습니다.

## HUD 및 설정

R2를 눌러 기술, 조작과 비용을 확인합니다. **Centered**는 중앙과 가장자리의 균형, **Expanded**는 더 넓은 간격, **Classic (4:3)**은 원래 4:3 영역입니다. 네이티브 4:3 위치를 유지하고 울트라와이드에는 안전 영역을 사용합니다. 공격 목록을 숨길 수 있으며 외부 테두리는 새 설정에서 기본 꺼짐입니다. HD 리소스가 있으면 우선 사용하고 없으면 원본을 사용하며 텍스처 팩은 포함하지 않습니다. 어두운 런처, 둥근 버튼, 삼성구 아이콘, 미리보기와 아랍어 RTL 포함 16개 언어를 지원합니다. 기술/캐릭터 이름은 게임에서 가져옵니다. **F6** 공격 표시, **F7** 테두리, **F8** 위치. 단축키 버튼을 눌러 변경, 해제 또는 기본값 복원할 수 있으며 중복은 거부합니다. 게임 포커스가 필요하고 누른 채 반복하지 않습니다. 전투 중 변경 잠금도 가능합니다. 경로, 언어, 표시, 단축키와 게임 내 변경은 재실행/업데이트 후에도 저장됩니다. 게임을 옮기면 경로를 다시 선택하세요.

## 호환성, 온라인 및 검증

Windows와 Linux: 네 종류의 메뉴를 각 플레이어 화면 안쪽 왼편에 배치하고 4:3, 16:9, 16:10, 울트라와이드에서 안정적인 크기를 유지합니다. Windows: 적응형 호환성 검사와 원본 Decomp, Char Package, Tag Team 자동 감지, 사용자 캐릭터 데이터 및 최대 4개의 로컬 컨트롤러 지원. Linux 1.0.6은 새 분할 화면 배치를 포함하지만 기존 네이티브 라이브러리와 분석기를 유지하며 v0.1.37/v0.1.38.1을 검사했습니다. Windows 변형 버전 지원은 포함하지 않습니다. Tag Team 4화면에서는 추가 boost 아이콘 대신 원래 안내를 표시합니다. Char Package 자체가 사용자 캐릭터의 온라인 사용을 차단합니다. 자동 테스트는 실제 대전을 대신하지 않으며 네이티브 Linux와 온라인 롤백은 검증되지 않았습니다. 모든 향후 버전이나 동기화 오류 방지를 보장하지 않습니다.

## 소스 및 라이선스

두 ZIP 모두 소스와 AGPL-3.0 라이선스를 포함합니다. 글꼴은 해당 라이선스를 유지합니다. 게임, 게임 텍스처 및 개인 설정은 포함하지 않습니다. 비공식 팬 프로젝트이며 상표는 각 권리자 소유입니다.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
