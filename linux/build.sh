#!/usr/bin/env bash
set -euo pipefail
package_dir=$(CDPATH= cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)
cd -- "$package_dir"
compiler=${CC:-clang}
command -v "$compiler" >/dev/null || { echo 'Install clang (or set CC).' >&2; exit 1; }
command -v mcs >/dev/null || { echo 'Install mono-devel / mono-mcs.' >&2; exit 1; }
extra=()
compiler_banner=$("$compiler" --version)
if [[ ${compiler_banner,,} != *clang* ]]; then extra+=(-maccumulate-outgoing-args); fi
"$compiler" -shared -fPIC -O2 -ffreestanding -fno-builtin -fno-stack-protector -Wall -Wextra -Werror "${extra[@]}" -o libbt3sparking.so src/native_linux.c src/online_filter.c
mcs -target:exe -platform:x64 -optimize+ -out:BT3-SparkingControls-Linux.exe -r:System.Windows.Forms -r:System.Drawing -r:System.Web.Extensions src/AssemblyInfo.cs src/SparkingControls.cs src/GameButtonIcons.cs src/UiLanguage.cs src/Transformations.cs src/LinuxDesktop.cs src/LinuxCompatibility.cs
python3 tools/update-profile.py
echo 'Built native ELF library and Mono frontend. Run bash start.sh.'
