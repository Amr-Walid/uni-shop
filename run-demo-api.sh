#!/usr/bin/env bash
# Starts the API for a local demo, backed by the in-memory database.
#
# Why a script: exporting these vars inline before a backgrounded `dotnet run`
# is unreliable — the subshell can exit before the child inherits them, and
# `dotnet run` re-reads Properties/launchSettings.json unless told not to.
# Keeping it in one file makes the demo reproducible.
set -euo pipefail

export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$PATH"

# Constrained sandbox: server GC reserves per-core heaps this box cannot spare.
export DOTNET_gcServer=0

# /tmp is a small tmpfs here; the SDK needs room for its intermediate files.
export TMPDIR="${TMPDIR:-/home/user/tmpbig}"

export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://0.0.0.0:5100}"

# Database:Provider=InMemory lives in appsettings.Development.json. It does not
# persist: every restart returns to the seeded catalogue, which is what makes
# the demo repeatable.

cd "$(dirname "$0")/FTD.Api"

exec dotnet run --no-build --no-launch-profile
