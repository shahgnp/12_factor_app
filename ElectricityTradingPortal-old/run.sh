#!/usr/bin/env bash
# Runs the app with the .NET 10 SDK/runtime that is installed in ~/.dotnet.
# (The system-wide /usr/share/dotnet SDK lacks the ASP.NET Core 10 runtime.)
set -e

if [ -d "$HOME/.dotnet" ]; then
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$HOME/.dotnet:$PATH"
fi

cd "$(dirname "$0")"
exec dotnet run "$@"