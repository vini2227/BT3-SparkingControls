[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.1

## 1.1 — Windows / Linux

Windows 1.1：オリジナルDecompのストーリー戦闘検出を修正し、HUD描画領域をパネル範囲に縮小。起動と互換性チェックを改善し、画面端の配置を追加しました。設定は保持されます。Linux 1.1も公開しました。HUDの描画範囲縮小、X11画像の再利用、ストーリー戦闘検出、画面端配置を追加。Windows派生版の対応は含まれず、実際のLinux/X11プレイは未検証です。起動時からテクスチャと×8を有効にしたMission 100の処理落ちは調査中で、解消は未確認です。詳細なテストとログを提供してくれたAeroに感謝します。

[1.1 — Changes / English](../Release-1.1.md)


## 操作・セットアップガイド

Tenkaichi3Decomp 向けの、Sparking! ZERO に着想を得た操作配置と戦闘 HUD です。アニメーション、ダメージ、気力消費、射程、能力、行動可能なタイミングは BT3 の仕様に従います。ゲームとその素材は含まれません。

## 1.0.6 Beta の変更点

- Windows・Linux共通：4種類のメニューを各プレイヤーの表示領域内の左側に配置し、4:3、16:9、16:10、ウルトラワイドで安定したサイズを維持します。Windowsでは互換性検証を改善し、通常版Decomp、Char Package、Tag Teamを自動判別。カスタムキャラクター情報と最大4台のローカルコントローラーに対応します。Linux 1.0.6には新しい分割画面配置を含みますが、ネイティブライブラリと解析処理は従来のままで、検証対象はv0.1.37/v0.1.38.1です。Windowsの派生版対応は含みません。Tag Teamの4画面では追加のboostアイコンを重ねず、元の表示を残します。Char Package自体がカスタムキャラクターのオンライン使用を無効にしています。自動テストは実際の対戦を保証せず、ネイティブLinuxとオンラインのロールバックは未検証です。すべての将来版への対応や同期ずれ防止は保証できません。

[CHANGELOG](../../CHANGELOG.md)

## ダウンロードと実演

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1/BT3-SparkingControls-1.1-Windows.zip) · [**Linux x64 ZIP (1.1)**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1/BT3-SparkingControls-1.1-Linux-x64.zip) · [**1.1**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.1)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1/SHA256SUMS.txt)

## インストール

### Windows x64

Windows ZIP を展開して `BT3-SparkingControls.exe` を実行し、**Browse** で `Tenkaichi3Decomp.exe` を選択します。言語と表示を設定し **Save and start** を押してください。ゲームは必ず補助アプリから起動します。Windows EXE は必要な補助素材を内蔵しており、通常利用なら EXE 単体を共有できますが、相手にもゲーム本体が必要です。ZIP にはソースとライセンスも付属します。Mod ローダー、ゲームの再コンパイル、ゲームファイルの差し替えは不要です。

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Linux x64 ZIP をすべて展開し、マネージド EXE、`libbt3sparking.so`、プロファイル、素材を同じフォルダーに保ちます。Windows Forms/System.Web.Extensions 対応の Mono、libgdiplus、必要なフォントを導入し、合成対応 X11 または動作する XWayland を使用します。そのフォルダーで `bash start.sh` を実行し、**Linux ネイティブ版**のゲームを選んで Save and start を押します。EXE は Mono で動作し、Wine/Proton は不要です。ネイティブ Wayland は未対応です。ライブラリは glibc 2.35 向けにクロスコンパイルされており、環境によって `bash build.sh` が必要です。別のゲームを重複起動しないでください。

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## 操作

□＝四角、△＝三角、○＝丸、LS＝左スティック。LS の矢印は十字キーではなくスティック方向です。通常のメニュー操作は元のままです。

| ボタン／組み合わせ | 動作 |
| --- | --- |
| `□` | ラッシュ攻撃 |
| `△` | 気弾 |
| `X` | 元の移動・ダッシュ |
| `R2` | 気力をためる |
| `L1` | 上昇 |
| `L2` | 下降 |
| `R2 + L1` | 上方向の垂直 Z Dash |
| `R2 + L2` | 下方向の垂直 Z Dash |
| `R1` | 対応コンボ中以外のガード |
| `R2 + □` | 必殺技 1 |
| `R2 + △` | 必殺技 2 |
| `R2 + ○` | 究極技、元の発動条件に従う |
| `R2 + D-pad ↑ / ↓` | スキル 1 |
| `R2 + D-pad ← / →` | スキル 2 |
| `○` | 元のカウンター構え |
| `R1 (combo)` | 対応コンボ中の前方瞬間移動 |
| `R1 + LS (combo)` | 同コンボ中の方向指定瞬間移動 |
| `R1 + □` | 通常のつかみ |
| `R1 + LS↓ + □` | 代替ダウンつかみ、スティックのみ |
| `LS↑ + △` | 元の入力によるダウンつかみ |
| `R3` | Emergency Blaster Wave、元の条件に従う |
| `L3` | 敵を探す／ロックオン |
| `Select / View` | カメラ切り替え |
| `D-pad ↑` | 長押し：変身／合体 |
| `D-pad ←` | 長押し：仲間の交代 |
| `D-pad ↓ / → (no R2)` | 誤操作防止のため戦闘中は無効 |

R2 を押したままでも必殺技を選べます。ためを続けるには割り当てられた攻撃ボタンを押し続けます。R2 だけを離しても攻撃ボタンは離されません。ブースト指示では □＋指定方向を入力します。表示は元の三角だけを置き換え、手の絵を隠しません。スキルのショートカットは ZERO の完全再現ではありません。

○ は対応コンボ中に押し続けた攻撃より優先して BT3 のカウンター構えを要求します。ZERO の Perception/Super Perception 全体を追加するものではありません。R1 の瞬間移動は元の気力条件とキャンセル可能時間に従い、敵の背後やその場での移動を保証しません。R1＋□ は優先して X→離す→X を生成しますが、射程や受付時間は必要です。元の X 2 回入力も残ります。ダウンつかみに十字キー↓は使えず、LS↑＋○でも発動しません。R3 は BT3 の体力消費と条件を使い、ZERO のスキルカウント式 Revenge Counter とは異なります。元の物理 R3 変身／合体入力は抑制され、カードが必要な入力を内部生成します。

## 変身・合体・交代

十字キー↑を押し、カードに対応する □/△/X/○ を選びます。↑を離すと閉じます。変身と合体の両方がある場合のみ L1/R1 でタブ切り替え。空の分類は飛ばし、合体のみなら直接開きます。十字キー←を押して仲間を選択。相方、形態、ゲージ、クールダウン、戦闘状態など BT3 の条件は維持されます。各メニューのカード位置は共通で、空き枠と控えの体力バーも表示。オンラインのローカル HUD はプレイヤー 2 でも左側ですが、論理的な入力側は変えません。

## HUD と設定

R2 を押すと技・入力・コストを表示します。**Centered** は中央と端のバランス、**Expanded** は間隔を広げる配置、**Classic (4:3)** は元の 4:3 領域です。ネイティブ 4:3 は維持し、ウルトラワイドでは安全領域を使います。技一覧は非表示可能。外枠は新規設定では初期オフです。HD 素材があれば優先し、なければ元の素材を使用。素材パックは付属しません。ダークテーマ、丸いボタン、三星球アイコン、プレビュー、アラビア語 RTL を含む 16 言語に対応し、技／キャラクター名はゲームから取得します。**F6**：技一覧、**F7**：外枠、**F8**：配置。ショートカットをクリックして変更・無効化・初期化でき、重複割り当ては拒否されます。ゲームにフォーカスがある場合のみ動作し、長押しで連続切り替えはしません。戦闘中の変更をロック可能。パス、言語、外観、キーとゲーム内変更は再起動／更新後も保存。ゲーム移動後はパスを選び直します。

## 互換性・オンライン・検証

Windows・Linux共通：4種類のメニューを各プレイヤーの表示領域内の左側に配置し、4:3、16:9、16:10、ウルトラワイドで安定したサイズを維持します。Windowsでは互換性検証を改善し、通常版Decomp、Char Package、Tag Teamを自動判別。カスタムキャラクター情報と最大4台のローカルコントローラーに対応します。Linux 1.0.6には新しい分割画面配置を含みますが、ネイティブライブラリと解析処理は従来のままで、検証対象はv0.1.37/v0.1.38.1です。Windowsの派生版対応は含みません。Tag Teamの4画面では追加のboostアイコンを重ねず、元の表示を残します。Char Package自体がカスタムキャラクターのオンライン使用を無効にしています。自動テストは実際の対戦を保証せず、ネイティブLinuxとオンラインのロールバックは未検証です。すべての将来版への対応や同期ずれ防止は保証できません。

## ソースとライセンス

両 ZIP にソースと AGPL-3.0 ライセンスを同梱。フォントはそれぞれのライセンスに従います。ゲーム、ゲーム素材パック、個人設定は含みません。非公式ファンプロジェクトで、商標は各権利者に帰属します。

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
