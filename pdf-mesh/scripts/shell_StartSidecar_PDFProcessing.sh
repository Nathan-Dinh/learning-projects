#!/usr/bin/env bash
cd "$(dirname "$0")/.."
dotnet clean ServiceMesh/src/csharp_Sidecar
dotnet publish ServiceMesh/src/csharp_Sidecar
dotnet run --project ServiceMesh/src/csharp_Sidecar --urls "http://localhost:6002" --Sidecar:ServiceName="PDF_Processing" --Sidecar:LocalServiceUrl="http://localhost:5002" --Sidecar:ControlPlaneUrl="http://localhost:5144"
