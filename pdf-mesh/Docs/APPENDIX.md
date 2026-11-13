# Appendix

## PDF to JSON

### Service A - Upload API

- Receives PDF files from clients
- Validates file type and size
- Passes the PDF to Service B through the mesh

### Service B - PDF Processing Service

- Receives the raw PDF
- Extracts text, tables, structure from the PDF
- Converts the extracted data into JSON format
- Sends the JSON to Service C

### Service C - Storage Service

- Receives the JSON output
- Stores it in a database
- Exposes an endpoint to retrieve stored JSON by ID

### Libraries

- **UglyToad.PdfPig** - PDF text extraction in .NET
- **System.Text.Json** - building the JSON output

## Service Mesh

### What is it?

- A dedicated infrastructure layer that handles communication between services
- Services don't manage their own networking - the mesh does it for them
- Each service gets a **sidecar proxy** that sits next to it

### The problem the mesh is trying to solve

- Managing networking per service becomes unmanageable at scale
  - Every service needs its own retry logic, timeout handling, auth, encryption, and logging
  - That code gets duplicated across every service - one bug means patching all of them
  - Different teams may implement it differently, leading to inconsistent behavior
  - Updating a networking policy (e.g., new TLS version) means redeploying every service
  - **How the mesh fixes this:**
    - Networking logic lives in the sidecar, not in application code - write it once, every service gets it
    - One config change in the control plane updates the policy across all sidecars at once - no redeployments
    - Every sidecar behaves the same way, so networking is consistent regardless of which team built the service
    - Services only contain business logic - they don't know or care about retries, encryption, or routing
- Zero-trust security (every call must be encrypted and authorized)

### How it works

- Every service has a sidecar proxy running alongside it
- The control plane is a central server that pushes config (routing rules, policies) to all sidecars
- Services never talk to each other directly - all traffic flows through sidecars

#### How the sidecar intercepts traffic

- The sidecar runs on the same machine (or container) as the service
- Network rules redirect all incoming/outgoing traffic through the sidecar automatically
- The service thinks it's talking directly to another service, but the sidecar is intercepting it
- Think of it like a mail room - you hand your letter to the mail room, not directly to the other person

#### Step by step: Service A calls Service B

1. Service A sends a normal HTTP request to Service B (e.g., `http://service-b/api/data`)
2. Sidecar A intercepts the outgoing request before it leaves
3. Sidecar A looks up routing rules from the control plane - where is Service B?
4. Sidecar A encrypts the request (mTLS) and forwards it to Sidecar B
5. Sidecar B receives it, decrypts it, checks auth policies - is A allowed to talk to B?
6. If allowed, Sidecar B forwards the plain request to Service B
7. Service B processes it and sends a response back
8. The response travels back the same path: Service B → Sidecar B → Sidecar A → Service A

Neither service knows any of this happened. They just see normal HTTP requests and responses.

#### How the control plane works

- It's a central server that stores all the rules (routing, auth policies, retry config)
- When you change a rule, the control plane pushes the update to all sidecars
- Sidecars check in with the control plane regularly to stay up to date
- Example: you set "all requests get 3 retries" in the control plane → every sidecar starts retrying automatically

### What the sidecar handles

- **mTLS** - auto encryption between services, no code changes
- **Retries & timeouts** - configurable per route
- **Load balancing** - distributes traffic across instances
- **Observability** - traces, metrics, logs without touching app code
- **Traffic control** - canary deploys, A/B routing, fault injection
- **Auth policies** - controls which services can talk to each other

### Two main parts

- **Data plane** - the sidecar proxies that intercept and forward traffic
- **Control plane** - central config that tells sidecars the routing rules and policies

## Real-world tools

- **Istio** - most popular, runs on Kubernetes
- **Linkerd** - lighter, easier to learn
- **Consul Connect** - HashiCorp's option

# .NET Info

### Project structure

```
ServiceMesh/
├── ServiceMesh.sln
├── src/
│   ├── csharp_PDFHandler/        Service A - Upload API (:5001)
│   ├── csharp_PDF_Processing/    Service B - PDF Processing (:5002)
│   ├── csharp_Storage/           Service C - Storage (:5003)
│   ├── csharp_Sidecar/           Sidecar proxy (3 instances: :6001, :6002, :6003)
│   ├── csharp_ControlPlane/      Control Plane (:5144)
│   └── vue-web-pdf/              Vue Frontend (:5173)
├── tests/
│   └── Tests/                    xUnit unit tests
└── docker-compose.yml            8 containers on shared 'mesh' network
```

### Kestrel vs ASP.NET Core

- **ASP.NET Core** - the framework (controllers, routing, middleware, dependency injection)
- **Kestrel** - the web server underneath (listens on a port, handles raw HTTP connections)
- ASP.NET Core handles the logic, Kestrel handles the connection
- Kestrel is the default server but can be put behind Nginx or IIS in production

### Sidecar runs two things

- **Kestrel** - listens for incoming requests and forwards them
- **BackgroundService** - polls the control plane for config updates on an interval
- Both run in the same process
- If the control plane is unreachable, the sidecar keeps using the last config it received

```csharp
public class ConfigPollingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Call control plane, update local config
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```
