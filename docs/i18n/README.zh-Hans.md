[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.1 Beta

## 1.1 Beta — Windows / Linux

Windows 1.1 Beta：修正原版 Decomp 剧情战斗识别，按面板范围裁剪 HUD 绘制区域，改进启动和兼容性检查，新增屏幕边缘布局并保留设置。Linux 1.1 也已提供：裁剪 HUD、复用 X11 图像、剧情战斗识别及屏幕边缘布局。不包含 Windows 分支支持；实际 Linux/X11 游戏尚未验证。从启动时就启用纹理包和 ×8 的 Mission 100 卡顿仍在调查中，尚未确认修复。特别感谢 Aero 提供详细测试和日志。

[1.1 — Changes / English](../Release-1.1.md)


## 操作与安装指南

为 Tenkaichi3Decomp 提供受 Sparking! ZERO 启发的操作布局和战斗 HUD。动画、伤害、气力消耗、距离、角色能力和动作窗口仍遵循 BT3。安装包不包含游戏或其资源。

## 1.0.6 Beta 版本更新

- Windows 和 Linux：四类菜单均位于各玩家视口左侧，在 4:3、16:9、16:10 和超宽屏下保持稳定尺寸。Windows：自适应兼容性检查，自动识别原版 Decomp、Char Package 和 Tag Team；读取自定义角色数据，支持最多四个本地控制器。Linux 1.0.6 包含新的分屏布局，但保留原生库和解析器，已检查的目标为 v0.1.37/v0.1.38.1；不包含 Windows 分支支持。Tag Team 四视图模式保留原生 boost 提示，不添加额外覆盖图标。Char Package 本身禁止自定义角色联机。自动化测试不等同于实际对战；原生 Linux 和在线回滚尚未验证。不能保证支持所有未来版本或完全避免不同步。

[CHANGELOG](../../CHANGELOG.md)

## 下载与演示

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-1.1-Beta-Windows.zip) · [**Linux x64 ZIP (1.1)**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-1.1-Beta-Linux-x64.zip) · [**1.1 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.1-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/SHA256SUMS.txt)

## 安装

### Windows x64

解压 Windows ZIP，运行 `BT3-SparkingControls.exe`，点击 **Browse** 选择 `Tenkaichi3Decomp.exe`，设置语言和界面后点击 **Save and start**。请始终通过辅助程序启动游戏。Windows EXE 已内置辅助资源，日常使用可单独分享，但接收者仍需自行准备游戏。ZIP 同时包含源码和许可证。无需模组加载器、重新编译游戏或替换游戏文件。

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

完整解压 Linux x64 ZIP，将托管 EXE、`libbt3sparking.so`、配置和资源保存在同一文件夹。安装支持 Windows Forms/System.Web.Extensions 的 Mono、libgdiplus 及相应字体，使用支持合成的 X11 或可正常运行的 XWayland。在该目录运行 `bash start.sh`，选择 **Linux 原生游戏可执行文件**，再点击 Save and start。托管 EXE 使用 Mono，无需 Wine/Proton。尚不支持原生 Wayland。动态库以 glibc 2.35 为目标交叉编译；其他系统可能需要 `bash build.sh`。不要另行启动第二个游戏实例。

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## 操作

□ = 方块；△ = 三角；○ = 圆圈；LS = 左摇杆。LS 箭头表示摇杆方向，不是十字键。普通菜单导航保留游戏原始操作。

| 按键／组合 | 动作 |
| --- | --- |
| `□` | Rush 连击 |
| `△` | 气弹 |
| `X` | 原生移动与冲刺 |
| `R2` | 蓄气 |
| `L1` | 上升 |
| `L2` | 下降 |
| `R2 + L1` | 向上垂直 Z Dash |
| `R2 + L2` | 向下垂直 Z Dash |
| `R1` | 自身受支持连段之外的防御 |
| `R2 + □` | 超级攻击 1 |
| `R2 + △` | 超级攻击 2 |
| `R2 + ○` | 究极攻击，仍需满足原生条件 |
| `R2 + D-pad ↑ / ↓` | 技能 1 |
| `R2 + D-pad ← / →` | 技能 2 |
| `○` | 原生反击架势 |
| `R1 (combo)` | 受支持连段中的原生向前瞬移 |
| `R1 + LS (combo)` | 这些连段中的原生方向瞬移 |
| `R1 + □` | 普通抓投 |
| `R1 + LS↓ + □` | 替代倒地抓投，仅限摇杆 |
| `LS↑ + △` | 原始指令倒地抓投 |
| `R3` | Emergency Blaster Wave，须满足原生条件 |
| `L3` | 搜索／锁定目标 |
| `Select / View` | 切换镜头 |
| `D-pad ↑` | 按住：变身／融合 |
| `D-pad ←` | 按住：切换队员 |
| `D-pad ↓ / → (no R2)` | 战斗中禁用以防误触 |

已按住 R2 时也能选择特殊攻击。要继续蓄力，请保持对应攻击键；只松开 R2 不会松开攻击键。出现提示时，用 □ + 指定方向输入原生 boost。覆盖图标只遮住三角，不挡手部图案。技能快捷键属于本辅助程序，不声称与 ZERO 完全相同。

○ 请求 BT3 原生反击架势，在受支持连段中优先于持续按住的攻击，不是完整的 ZERO Perception/Super Perception 系统。R1 瞬移仍需满足原生气力与取消窗口，不保证移到敌人身后或原地停留。R1 + □ 优先生成 X—松开—X，不能绕过距离和动作窗口；原始双击 X 抓投保留。倒地抓投不接受十字键 ↓；LS↑ + ○ 不再触发倒地抓投。R3 使用 BT3 的生命消耗和触发条件，不是消耗技能点的 ZERO Revenge Counter。原始物理 R3 变身／融合操作会被接管，由卡片内部生成所需指令。

## 变身、融合与队员切换

按住十字键 ↑，按卡片所示 □/△/X/○ 选择；松开 ↑ 关闭。只有同时存在变身和融合时，L1/R1 才切换分类。跳过空分类；仅有融合的角色直接进入融合。按住十字键 ← 选择队员。伙伴、形态、量表、冷却和战斗状态要求仍由 BT3 决定。三个菜单卡片位置一致，保留空位并显示后备生命条。联机本地面板始终在左侧，即使逻辑上是玩家 2，也不改变输入对应的玩家。

## HUD 与设置

按住 R2 查看招式、按键和消耗。**Centered** 平衡中央与边缘；**Expanded** 拉开面板距离；**Classic (4:3)** 使用原始 4:3 区域。原生 4:3 位置不变，超宽屏采用安全区域。可隐藏攻击列表，外框在全新设置中默认关闭。优先使用已有高清资源，缺失时使用原始资源，不附带纹理包。启动器为深色主题、圆角按钮、三星龙珠图标和实时预览，支持 16 种语言与阿拉伯语 RTL；招式和角色名来自游戏。**F6** 切换攻击显示、**F7** 切换外框、**F8** 切换位置。点击快捷键按钮更改、禁用或恢复默认，拒绝重复绑定。仅游戏获得焦点时生效，长按不重复触发。可锁定战斗中快捷键。路径、语言、外观、快捷键和游戏内更改会跨启动／更新保存；移动游戏后需重选路径。

## 兼容性、联机与测试

Windows 和 Linux：四类菜单均位于各玩家视口左侧，在 4:3、16:9、16:10 和超宽屏下保持稳定尺寸。Windows：自适应兼容性检查，自动识别原版 Decomp、Char Package 和 Tag Team；读取自定义角色数据，支持最多四个本地控制器。Linux 1.0.6 包含新的分屏布局，但保留原生库和解析器，已检查的目标为 v0.1.37/v0.1.38.1；不包含 Windows 分支支持。Tag Team 四视图模式保留原生 boost 提示，不添加额外覆盖图标。Char Package 本身禁止自定义角色联机。自动化测试不等同于实际对战；原生 Linux 和在线回滚尚未验证。不能保证支持所有未来版本或完全避免不同步。

## 源码与许可

两个 ZIP 都包含源码和 AGPL-3.0 许可证，字体保留各自许可。不包含游戏、游戏纹理包或个人设置。本项目为非官方粉丝作品，商标归各自权利人。

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
