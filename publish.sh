#!/bin/bash
set -e

# Clean and build
dotnet clean
dotnet build -c Release

# Pack the nuget package
dotnet pack -c Release -o ./nupkgs

# Prompt for API key (silent input)
read -s -p "Enter NuGet API key: " NUGET_API_KEY
echo

# Publish to nuget.org
dotnet nuget push ./nupkgs/dev.elias.beancounter.*.nupkg \
  -k "$NUGET_API_KEY" \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate