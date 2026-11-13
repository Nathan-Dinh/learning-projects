#!/usr/bin/env bash
cd "$(dirname "$0")/.."
dotnet clean ServiceMesh/src/csharp_PDF_Processing
dotnet publish ServiceMesh/src/csharp_PDF_Processing
dotnet run --project ServiceMesh/src/csharp_PDF_Processing --urls "http://localhost:5002" --Service:Name="PDF_Processing" --Service:SidecarUrl="http://localhost:6002"
