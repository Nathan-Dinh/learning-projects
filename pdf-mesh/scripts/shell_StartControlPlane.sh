#!/usr/bin/env bash
cd "$(dirname "$0")/.."
dotnet clean ServiceMesh/src/csharp_ControlPlane
dotnet publish ServiceMesh/src/csharp_ControlPlane
dotnet run --project ServiceMesh/src/csharp_ControlPlane --urls "http://localhost:5144"
