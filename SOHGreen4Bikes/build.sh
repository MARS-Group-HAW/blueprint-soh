#!/usr/bin/env bash

# Build the application and all of its dependencies
dotnet publish -c Release -o SOHGreen4Bikes/win -r win-x64 --self-contained

dotnet publish -c Release -o SOHGreen4Bikes/linux -r linux-x64 --self-contained

dotnet publish -c Release -o SOHGreen4Bikes/mac -r osx-x64 --self-contained