#!/usr/bin/env bash
cd "$(dirname "$0")/.."
dotnet clean ServiceMesh/src/csharp_Storage
dotnet publish ServiceMesh/src/csharp_Storage
dotnet run --project ServiceMesh/src/csharp_Storage --urls "http://localhost:5003" --Service:Name="Storage" --Service:SidecarUrl="http://localhost:6003"
