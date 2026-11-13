#!/usr/bin/env bash
cd "$(dirname "$0")/.."
dotnet clean ServiceMesh/src/csharp_Sidecar
dotnet publish ServiceMesh/src/csharp_Sidecar
dotnet run --project ServiceMesh/src/csharp_Sidecar --urls "http://localhost:6003" --Sidecar:ServiceName="Storage" --Sidecar:LocalServiceUrl="http://localhost:5003" --Sidecar:ControlPlaneUrl="http://localhost:5144"
