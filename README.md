# FlexCms.InvestPro

Shariah-compliant Islamic investment management module for [FlexCMS](https://github.com/rayhanul17/flex_cms_v1).

Per-partner contracts (Mudarabah / Musharakah / Labor-only / Mixed), four ledgers (capital / labor / expense / revenue), all-partner approval workflow, immutable closure snapshots with tamper-detection checksum, reopen + reclose with adjustment payouts, raw-SQL reports (closure / lifetime / zakat / planned vs actual) with PDF + Excel + CSV export.

## Prerequisites

- **.NET 10 SDK**
- **PostgreSQL 14+** (or MySQL 8 / SQL Server 2019+ — provider picked in host setup wizard)
- **EF Core CLI:** `dotnet tool install --global dotnet-ef --version 9.0.15`
- **Parent repo cloned** — see Layout below. This module's `.csproj` has a `<ProjectReference>` to `FlexCms.Framework`, so a stand-alone clone won't build.

## Required folder layout

```text
flex_cms_v1/                              ← parent repo
├── src/FlexCms.Framework/                ← csproj reference target
├── src/FlexCms.Host/
└── modules/
    └── FlexCms.InvestPro/                ← THIS repo cloned here
```

Module folder name **must** match `ModuleId` (`FlexCms.InvestPro`) — admin-uploaded updates land in `modules/FlexCms.InvestPro/`, so a mismatched dev folder would create a sibling and the load order would decide which DLL wins. The host logs a warning at startup if folder ≠ ModuleId.

## Dev workflow (run the module from source)

```bash
# 1. Clone both repos side-by-side
git clone https://github.com/rayhanul17/flex_cms_v1.git
cd flex_cms_v1/modules
git clone https://github.com/rayhanul17/FlexCms.InvestPro.git
cd ..

# 2. Build everything (framework + module + host)
dotnet build src/FlexCms.Host/FlexCms.Host.csproj

# 3. Run the host
cd src/FlexCms.Host
dotnet run --launch-profile http
# → http://localhost:5096
```

First run brings up the setup wizard at `/Setup`. After completing setup the host restarts itself; the module is then discovered, migrations applied, permissions seeded (26 entries under the `flexcms.investpro.` prefix), menu item added.

### Adding more EF migrations

From the module folder (`modules/FlexCms.InvestPro/`):

```bash
dotnet ef migrations add YourMigrationName
```

The next `dotnet run` on the host applies the new migration automatically (idempotent — `Database.MigrateAsync()` on activation).

## Distribution workflow (build a zip + admin upload)

For deploying to a server where dev source isn't cloned:

### Quick path — use the build script

```bash
# Linux / macOS / WSL / Git Bash
./build-package.sh

# Windows PowerShell
.\build-package.ps1
```

Both scripts:

1. `dotnet build -c Release`
2. Stage the `bin/Release/net10.0` output
3. Remove framework-side DLLs (host has its own — keeping them risks type-identity bugs)
4. Zip the result to `./dist/FlexCms.InvestPro-{version}.zip`

### Manual path (if you don't want the script)

```bash
# 1. Build release
dotnet build FlexCms.InvestPro.csproj -c Release

# 2. Stage output (exclude framework DLLs)
mkdir -p /tmp/pkg/FlexCms.InvestPro
cp -r bin/Release/net10.0/* /tmp/pkg/FlexCms.InvestPro/
rm /tmp/pkg/FlexCms.InvestPro/FlexCms.Framework.{dll,pdb} 2>/dev/null

# 3. Zip
cd /tmp/pkg
zip -r FlexCms.InvestPro.zip FlexCms.InvestPro/
```

### What's inside the zip

```text
FlexCms.InvestPro.zip
├── module.json                              ← manifest the host reads first
├── FlexCms.InvestPro.dll                    ← module code + compiled Razor views
├── FlexCms.InvestPro.pdb
├── FlexCms.InvestPro.deps.json
├── FlexCms.InvestPro.runtimeconfig.json
├── FlexCms.InvestPro.staticwebassets.*.json
├── Resources/i18n/{en,bn}.json              ← translations
└── LatoFont/                                ← bundled font for PDF reports
```

The host already supplies the `FlexCms.Framework.dll`, ASP.NET Core, EF Core, and NuGet providers — don't bundle those.

### Install the zip on a host

1. Log into admin as SuperAdmin → `/admin/modules` → **Upload Module**
2. Pick the zip → Upload
3. Click **Restart** (or restart the process manually)
4. After restart: module appears in the list as Active, migrations applied, permissions seeded, menu item added
5. Optional: `/admin/roles` → assign InvestPro permissions to any role (SuperAdmin bypasses)

## What this module ships

- **6 admin pages** under `/investpro/admin/`: Investments, Approvals, Reports, Partners, Expense Categories, Approval Config
- **19 EF entities** with prefix `investpro_`
- **26 permissions** with namespace `flexcms.investpro.`
- **i18n stubs** (en + bn) ready for additional strings
- **One top-level menu item** ("InvestPro" → `/investpro/admin/investments`); sub-nav is rendered by `_InvestProSubNav.cshtml`

## Architecture notes

- **Each entity has its own service** (PartnerService, InvestmentService, InvestmentSnapshotService, ...). Orchestrators (CloseService, ReopenService) compose entity services within a single scoped DbContext so a close+snapshot or reopen+demote is atomic.
- **All math lives in `CalculationHelper.cs`** — pure static functions for net P/L, partner profit/loss share, settlement, zakat-eligible base, adjustment delta, SHA256 checksum. Step-by-step so closure audits are linear.
- **Snapshot rows are immutable** — once written, never updated. Reopens demote v1 to Superseded and write v2 with adjustment payouts (`Outgoing` if delta > 0, `Incoming` if delta < 0).
- **Partner-vote identity is bound to the calling user** via `Partner.UserId` (a `Guid?` column added in `Phase7b_PartnerUserId`). SuperAdmin can override for offline-consent flows. Set the link from `/investpro/admin/partners/{id}/edit` → "App Account Link" card.
- **DbContext is injected via DI** (`InvestProModule.RegisterServices` registers it scoped). All services share one context per request — standard EF pooling applies.

## Permissions reference

Short keys here are used internally; controllers must use the fully-qualified `flexcms.investpro.*` form (the `*Key` consts are `internal` to refuse accidental short-string use from outside the module).

| Key | Group |
| --- | --- |
| `partner.view` / `.create` / `.edit` / `.delete` | Partners |
| `category.view` / `.create` / `.edit` / `.delete` | Expense Categories |
| `approval-config.view` / `.edit` | Config |
| `investment.view` / `.create` / `.edit` / `.delete` / `.activate` | Investments |
| `ledger.view` / `.write` | Ledger |
| `approval.view` / `.decide` | Approvals |
| `close.request` / `.decide` | Close workflow |
| `reopen.request` / `.decide` | Reopen workflow |
| `snapshot.view` | Snapshots |
| `payout.manage` | Payouts |
| `report.view` | Reports |

## License

Internal use within FlexCMS deployments. See parent repo for license.
