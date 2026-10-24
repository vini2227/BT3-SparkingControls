[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.0.6 Beta

## Panduan kontrol dan pemasangan

Tata letak kontrol dan HUD pertarungan terinspirasi Sparking! ZERO untuk Tenkaichi3Decomp. Animasi, kerusakan, biaya Ki, jangkauan, kemampuan dan jendela aksi BT3 tetap berlaku. Gim dan asetnya tidak disertakan.

## Perubahan versi 1.0.6 Beta

- Windows dan Linux: keempat menu berada di kiri dalam area tiap pemain, dengan ukuran stabil untuk 4:3, 16:9, 16:10 dan ultrawide. Windows: pemeriksaan kompatibilitas adaptif dan deteksi otomatis Decomp asli, Char Package serta Tag Team; data karakter kustom dan hingga empat kontroler lokal. Linux 1.0.6 membawa tata letak baru tetapi memakai pustaka dan resolver lama yang diuji pada v0.1.37/v0.1.38.1; varian Windows tidak didukung. Dalam empat tampilan Tag Team, petunjuk boost bawaan tetap terlihat tanpa lapisan tambahan. Char Package sendiri menonaktifkan karakter kustom secara online. Pengujian otomatis bukan pengganti permainan nyata; Linux native dan rollback online belum diverifikasi. Tidak ada jaminan untuk setiap versi mendatang atau bebas desync.

[CHANGELOG](../../CHANGELOG.md)

## Unduhan dan demonstrasi

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-1.0.6-Beta-Windows.zip) · [**Linux x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-1.0.6-Beta-Linux-x64.zip) · [**1.0.6 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.0.6-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0.6-beta/SHA256SUMS.txt)

## Pemasangan

### Windows x64

Ekstrak ZIP Windows, jalankan `BT3-SparkingControls.exe`, klik **Browse**, pilih `Tenkaichi3Decomp.exe`, atur bahasa/tampilan, lalu **Save and start**. Selalu mulai gim melalui aplikasi pendamping. EXE Windows memuat aset pendamping sehingga dapat dibagikan sendiri untuk penggunaan biasa; penerima tetap membutuhkan gim. ZIP juga memuat kode sumber dan lisensi. Tidak perlu modloader, kompilasi ulang atau penggantian berkas gim.

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Ekstrak seluruh ZIP Linux x64; simpan EXE terkelola, `libbt3sparking.so`, profil dan aset bersama-sama. Pasang Mono dengan Windows Forms/System.Web.Extensions, libgdiplus dan fon bahasa yang dipilih. Gunakan X11 dengan kompositor atau XWayland yang berfungsi. Jalankan `bash start.sh` dari folder, pilih gim **Linux native**, lalu Save and start. EXE memakai Mono tanpa Wine/Proton. Wayland native belum didukung. Pustaka dikompilasi dengan target glibc 2.35; sistem berbeda mungkin perlu `bash build.sh`. Jangan mulai salinan gim lain secara terpisah.

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## Kontrol

□ = Kotak; △ = Segitiga; ○ = Lingkaran; LS = stik kiri. Panah LS berarti stik, bukan D-pad. Navigasi menu biasa tetap memakai kontrol asli.

| Tombol / kombinasi | Aksi |
| --- | --- |
| `□` | Serangan Rush |
| `△` | Tembakan Ki |
| `X` | Gerakan dan dash native |
| `R2` | Mengisi Ki |
| `L1` | Naik |
| `L2` | Turun |
| `R2 + L1` | Z Dash vertikal ke atas |
| `R2 + L2` | Z Dash vertikal ke bawah |
| `R1` | Bertahan di luar kombo sendiri yang didukung |
| `R2 + □` | Serangan super 1 |
| `R2 + △` | Serangan super 2 |
| `R2 + ○` | Serangan ultimate sesuai persyaratan native |
| `R2 + D-pad ↑ / ↓` | Kemampuan 1 |
| `R2 + D-pad ← / →` | Kemampuan 2 |
| `○` | Sikap serangan balik native |
| `R1 (combo)` | Teleport maju native dalam kombo yang didukung |
| `R1 + LS (combo)` | Teleport berarah native dalam kombo tersebut |
| `R1 + □` | Cengkeraman biasa |
| `R1 + LS↓ + □` | Cengkeraman tanah alternatif, hanya stik |
| `LS↑ + △` | Cengkeraman tanah dengan input asli |
| `R3` | Emergency Blaster Wave jika syarat native terpenuhi |
| `L3` | Mencari / mengunci sasaran |
| `Select / View` | Mengganti kamera |
| `D-pad ↑` | Tahan: transformasi/fusi |
| `D-pad ←` | Tahan: mengganti anggota tim |
| `D-pad ↓ / → (no R2)` | Diblokir dalam pertarungan untuk mencegah input tak sengaja |

Serangan spesial bisa dipilih saat R2 sudah ditahan. Untuk terus mengisi serangan, tahan tombol serangan yang ditetapkan; melepas R2 saja tidak melepaskannya. Saat diminta, gunakan □ + arah yang ditunjukkan untuk boost asli. Overlay hanya menutupi Segitiga dan membiarkan gambar tangan terlihat. Pintasan kemampuan bukan salinan persis kontrol ZERO.

○ meminta sikap serangan balik BT3 dengan prioritas di atas serangan yang ditahan saat kombo didukung; bukan seluruh sistem Perception/Super Perception ZERO. Teleport R1 mengikuti biaya Ki/jendela native, tanpa jaminan muncul di belakang musuh atau tetap diam. R1 + □ diprioritaskan dan menghasilkan X–lepas–X, tergantung jangkauan dan jendela aksi. Ketuk ganda X asli tetap ada. Cengkeraman tanah tidak menerima D-pad ↓; LS↑ + ○ tidak memicunya. R3 memakai biaya kesehatan dan syarat BT3, bukan Revenge Counter berbasis Skill Count ZERO. Pintasan fisik R3 transformasi/fusi asli dikonsumsi; kartu menghasilkan input yang diperlukan.

## Transformasi, fusi dan pergantian

Tahan D-pad ↑ lalu tekan □/△/X/○ sesuai kartu; lepas ↑ untuk menutup. L1/R1 berganti antara bentuk dan fusi hanya bila keduanya tersedia. Kategori kosong dilewati; karakter dengan fusi saja langsung membuka fusi. Tahan D-pad ← untuk mengganti anggota tim. Syarat pasangan, bentuk, pengukur, cooldown dan keadaan pertarungan BT3 tetap berlaku. Posisi kartu konsisten, slot kosong dan bilah kesehatan cadangan. Daring, panel lokal ada di kiri bahkan sebagai Pemain 2, tanpa mengubah sisi logis input.

## HUD dan pengaturan

Tahan R2 untuk daftar serangan, perintah dan biaya. **Centered** menyeimbangkan tengah dan tepi; **Expanded** memperlebar jarak panel; **Classic (4:3)** memakai area asli 4:3. Posisi 4:3 native dipertahankan; ultrawide memakai area aman. Daftar dapat disembunyikan; bingkai luar mati secara default untuk pengaturan baru. Aset HD tersedia diutamakan, lalu aset asli; tanpa paket tekstur bawaan. Peluncur gelap dengan tombol bulat, bola tiga bintang, pratinjau dan 16 bahasa termasuk Arab RTL; nama tetap dari gim. **F6**: serangan; **F7**: bingkai; **F8**: posisi. Klik pintasan untuk mengganti, menonaktifkan atau mengembalikan default; duplikat ditolak. Membutuhkan fokus gim dan tidak berulang saat ditahan. Kunci pintasan mencegah perubahan saat bertarung. Jalur, bahasa, tampilan, pintasan dan perubahan dalam gim tersimpan antar peluncuran/pembaruan. Jika gim dipindah, pilih jalur baru.

## Kompatibilitas, daring dan pengujian

Windows dan Linux: keempat menu berada di kiri dalam area tiap pemain, dengan ukuran stabil untuk 4:3, 16:9, 16:10 dan ultrawide. Windows: pemeriksaan kompatibilitas adaptif dan deteksi otomatis Decomp asli, Char Package serta Tag Team; data karakter kustom dan hingga empat kontroler lokal. Linux 1.0.6 membawa tata letak baru tetapi memakai pustaka dan resolver lama yang diuji pada v0.1.37/v0.1.38.1; varian Windows tidak didukung. Dalam empat tampilan Tag Team, petunjuk boost bawaan tetap terlihat tanpa lapisan tambahan. Char Package sendiri menonaktifkan karakter kustom secara online. Pengujian otomatis bukan pengganti permainan nyata; Linux native dan rollback online belum diverifikasi. Tidak ada jaminan untuk setiap versi mendatang atau bebas desync.

## Sumber dan lisensi

Kedua ZIP memuat sumber dan lisensi AGPL-3.0. Fon mempertahankan lisensinya. Gim, tekstur gim dan pengaturan pribadi tidak disertakan. Proyek penggemar tidak resmi; merek milik pemegangnya.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
