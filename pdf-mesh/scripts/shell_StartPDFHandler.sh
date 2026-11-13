#!/usr/bin/env bash
cd "$(dirname "$0")/.."
dotnet clean ServiceMesh/src/csharp_PDFHandler
dotnet publish ServiceMesh/src/csharp_PDFHandler
dotnet run --project ServiceMesh/src/csharp_PDFHandler --urls "http://localhost:5001" --Service:Name="PDF_Handler" --Service:SidecarUrl="http://localhost:6001"
