#!/usr/bin/env bash
# Runs the FTD.Web storefront + admin panel for demo purposes.
#
# Why a script instead of inline env vars: a backgrounded `dotnet run` does not
# inherit exports from a subshell that exits immediately, so the provider
# setting silently never reached the process. `--no-launch-profile` stops
# dotnet re-reading Properties/launchSettings.json and overriding the URL.
set -euo pipefail

cd "$(dirname "$0")"

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"

# Workstation GC — the demo box has under 1 GB of RAM and server GC reserves
# per-core heaps it cannot afford.
export DOTNET_gcServer=0
export TMPDIR=/home/user/tmpbig

export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://0.0.0.0:5000"

exec dotnet run --project FTD.Web/FTD.Web.csproj --no-build --no-launch-profile
