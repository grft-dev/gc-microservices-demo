# gc-microservices-demo `cartservice` (gc-port)

Graftcode port of the C# cart slice from Online Boutique. The exposed path keeps the cart business behavior and removes gRPC host/bootstrap ceremony.

## Exposed public surface

- Module: `cartservice.dll`
- Type: `cartservice.services.CartService`
- Methods:
  - `AddItem(string userId, string productId, int quantity)` -> `string` (`productId|quantity`)
  - `GetCart(string userId)` -> `string[]` (`productId|quantity`)
  - `EmptyCart(string userId)` -> `string` (empty on success)

Allowlist file: `src/CartService.GraftHost/graft-method-allowlist.json`

## Business parity and format

- Existing behavior is preserved: `AddItem` increments quantity for an already existing product in the same cart.
- A missing cart returns an empty list (`GetCart` returns `[]`).
- `EmptyCart` clears cart state for the user id.
- Product ids are escaped in output rows (`\|` and `\\`) so row parsing remains deterministic.

## Out of scope in this port

- Non-.NET services from the repository (Go/Java/Node/Python) stay outside this GRAFT slice.
- Legacy gRPC transport and cloud-specific cart backends (Spanner/AlloyDB) are intentionally removed from the exposed demo path.

## Legacy cleanup included

- Removed from active path: ASP.NET/gRPC host bootstrap (`Program`, `Startup`, health gRPC service), protobuf contract generation, in-repo `cartservice` unit tests, and legacy service Dockerfiles.
- The slice now runs as class libraries loaded by `gg`.

## Start stack (Redis + readiness seed + GG host)

```powershell
docker compose -f C:\Coding\Marketing\ports\gc-microservices-demo\src\cartservice\docker-compose.graft.yml up -d --build
```

Ports:

- GG WebSocket/API: `ws://localhost:8110/ws`
- Graft Vision: `http://localhost:8111`
- Redis: `localhost:6380`

Stop stack:

```powershell
docker compose -f C:\Coding\Marketing\ports\gc-microservices-demo\src\cartservice\docker-compose.graft.yml down -v
```

## Build host only

```powershell
dotnet build C:\Coding\Marketing\ports\gc-microservices-demo\src\cartservice\src\CartService.GraftHost\CartService.GraftHost.csproj -c Release
```

## Client configuration

Set runtime config for generated NuGet client:

```powershell
$env:GRAFTCODE_CONFIG_NUGET_CARTSERVICE = "name=graft.nuget.cartservice; runtime=netcore; modules=cartservice.dll; host=ws://localhost:8110/ws; stateless=true"
```

Current verified generated package:

- Package ID: `graft.nuget.cartservice_0odcwq`
- Version: `1.0.0`
- Feed: `https://grft.dev/73111fb7-29fd-4b3e-b9c5-362333e27f15__free`

## Unstable suffix without `GG_PROJECT_KEY`

This demo host does **not** use `GG_PROJECT_KEY`.

- Every generation can produce a new package suffix (for example `_abc123`).
- `gg` logs show the current `dotnet add package ... --source ...` command.
- Update the external verifier package reference to the latest generated package id/feed.
- If the package id stays the same, NuGet may not refresh `1.0.0`; clear local cache and restore again.

## External verifier

Path: `C:\Coding\Marketing\gc-microservices-demo-graft-e2e`

Run:

```powershell
dotnet run --project C:\Coding\Marketing\gc-microservices-demo-graft-e2e\gc-microservices-demo-graft-e2e.csproj -c Release -- `
  --host "ws://localhost:8110/ws" `
  --stateless true `
  --report "C:\Coding\Marketing\gc-microservices-demo-graft-e2e\reports\gc-microservices-demo-graft-e2e.json"
```
