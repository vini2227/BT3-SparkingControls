#!/usr/bin/env bash
set -euo pipefail
package_dir=$(CDPATH= cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)
if [[ $(uname -s) != Linux || $(uname -m) != x86_64 ]]; then echo 'Linux x86-64 is required.' >&2; exit 1; fi
if ! command -v mono >/dev/null 2>&1; then echo 'Mono runtime is required. Read README.md.' >&2; exit 1; fi
if [[ ${1:-} != --check && -z ${DISPLAY:-} ]]; then echo 'X11/XWayland display is required. Native Wayland overlay is not implemented.' >&2; exit 1; fi
exec mono "$package_dir/BT3-SparkingControls-Linux.exe" "$@"
