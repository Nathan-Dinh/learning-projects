# Running the PDF Mesh

A minimal ASP.NET service mesh (ControlPlane + three services each fronted by a Sidecar) with a Vue frontend for uploading PDFs.

## Prerequisites

- .NET SDK 9
- Node.js 20.19+ or 22.12+
- `jq` (only needed for `project_startup.sh`)

## Ports

| Component        | Port |
| ---------------- | ---- |
| ControlPlane     | 5144 |
| PDFHandler       | 5001 |
| PDF_Processing   | 5002 |
| Storage          | 5003 |
| Sidecar (Handler)| 6001 |
| Sidecar (Proc.)  | 6002 |
| Sidecar (Storage)| 6003 |
| Vue dev server   | 5173 |

## Start the mesh

Each service runs in its own terminal. From the repo root:

```bash
./scripts/shell_StartControlPlane.sh
./scripts/shell_StartSidecar_PDFHandler.sh
./scripts/shell_StartSidecar_PDFProcessing.sh
./scripts/shell_StartSidecar_Storage.sh
./scripts/shell_StartPDFHandler.sh
./scripts/shell_StartPDFProcessing.sh
./scripts/shell_StartStorage.sh
```

Order matters only in that the ControlPlane should be up before the sidecars start polling it for config. Each script does `dotnet clean && dotnet publish && dotnet run` for its project.

## Start the Vue frontend

```bash
cd ServiceMesh/src/vue-web-pdf
npm install     # first run only
npm run dev
```

Open http://localhost:5173. The dev server proxies `/api` to PDFHandler on `:5001`, so the browser makes same-origin calls.

## Use it

1. Pick a PDF.
2. Click **Upload**. The request goes Vue → PDFHandler → its sidecar → PDF_Processing's sidecar → PDF_Processing → Storage (via its own sidecar hop).
3. The extracted content renders page-by-page. Toggle to **JSON view** or **Copy JSON** as needed.

## Reset local state

SQLite DBs (`*.db`) are created on startup via EF migrations. If a database is out of sync with the migrations (e.g. `table already exists` errors), delete the relevant `.db` file and restart the service.

```bash
rm ServiceMesh/src/csharp_ControlPlane/controlplane.db
rm ServiceMesh/src/csharp_PDFHandler/handler.db
rm ServiceMesh/src/csharp_PDF_Processing/processing.db
rm ServiceMesh/src/csharp_Storage/storage.db
```

## Troubleshooting

- **`address already in use`** — a previous process still holds the port. `lsof -iTCP:PORT -sTCP:LISTEN` to find it, then kill.
- **`No route for X`** — the sidecar's config cache doesn't know that route yet. Make sure the ControlPlane is up and wait for the next refresh (`Sidecar:RefreshSeconds`, default 30s).
- **`X is not allowed to call Y`** — the AuthPolicy in the ControlPlane DB disallows it. Check the seeded policies in the ControlPlane migration.
- **`Connection refused (localhost:600X)`** — the matching sidecar isn't running. Start it.
