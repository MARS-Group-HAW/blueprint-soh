#!/usr/bin/env bash

# Build the application and all of its dependencies
dotnet publish -c Release -o SOHFerryTransfer/win -r win-x64 --self-contained

dotnet publish -c Release -o SOHFerryTransfer/linux -r linux-x64 --self-contained

dotnet publish -c Release -o SOHFerryTransfer/mac -r osx-x64 --self-contained