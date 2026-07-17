# Yuviron Backend

Backend for Yuviron: ASP.NET Core API, application layer, domain model, EF Core persistence, background consumers, media worker, analytics, payments, notifications, and deployment automation.

## Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Repository Structure](#repository-structure)
- [Requirements](#requirements)
- [Local Development](#local-development)
- [Configuration](#configuration)
- [Database and Migrations](#database-and-migrations)
- [Tests and Build Checks](#tests-and-build-checks)
- [API Runtime](#api-runtime)
- [Media Worker](#media-worker)
- [Smart Links](#smart-links)
- [Deployment Flow](#deployment-flow)
- [Production Deployment Checklist](#production-deployment-checklist)
- [Rollback](#rollback)
- [Operational Checks](#operational-checks)
- [Security Notes](#security-notes)

## Architecture

The solution follows a layered backend structure:

- `Yuviron.Api` - ASP.NET Core entry point, controllers, middleware, Swagger, health checks, SignalR hub, HTTP pipeline.
- `Yuviron.Application` - use cases, commands, queries, validators, DTOs, abstractions.
- `Yuviron.Domain` - entities, enums, domain events, primitives, domain exceptions.
- `Yuviron.Infrastructure` - EF Core, MySQL persistence, migrations, Redis, RabbitMQ/MassTransit, ClickHouse analytics, external services, background jobs, email, Stripe integration.
- `Yuviron.MediaWorker` - background worker for media processing and RabbitMQ consumers.
- `Yuviron.Tests` - unit/integration-style tests using xUnit and EF Core InMemory.

## Tech Stack

- .NET 9
- ASP.NET Core
- EF Core 9 with MySQL
- Redis
- RabbitMQ
- ClickHouse
- MassTransit
- OpenTelemetry and Aspire Dashboard
- Serilog and Seq
- Stripe
- xUnit, FluentAssertions, Moq
- Docker Compose for local infrastructure
- GitHub Actions for dev/prod deployment

## Repository Structure

```text
src/
  Yuviron.Api/
  Yuviron.Application/
  Yuviron.Domain/
  Yuviron.Infrastructure/
  Yuviron.MediaWorker/
Yuviron.Tests/
scripts/
  generate_appsettings.py
  README.md
.github/workflows/
  deploy.yml
docker-compose.yml
Yuviron.slnx
```

## Requirements

Local machine:

- .NET SDK 9.x
- Docker Desktop or Docker Engine
- Git
- Optional: `dotnet-ef`

Install EF tool if needed:

```powershell
dotnet tool install --global dotnet-ef
```

Check installed versions:

```powershell
dotnet --version
docker version
git --version
```

## Local Development

### 1. Clone repository

```powershell
git clone https://github.com/JByte-organization/yuviron-backend.git
cd yuviron-backend
```

### 2. Start local infrastructure

```powershell
docker compose up -d
```

This starts:

- MySQL on `localhost:3306`
- Redis on `localhost:6379`
- ClickHouse HTTP on `localhost:8123`
- ClickHouse native on `localhost:9000`
- RabbitMQ on `localhost:5672`
- RabbitMQ UI on `localhost:15672`
- Seq on `localhost:8081`
- Aspire Dashboard on `localhost:18888`
- OTLP endpoint on `localhost:4317`

### 3. Restore dependencies

```powershell
dotnet restore Yuviron.slnx
```

### 4. Run API

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src\Yuviron.Api\Yuviron.Api.csproj
```

In `Development`, the API applies EF Core migrations on startup and seeds essential data.

### 5. Run MediaWorker

Open another terminal:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src\Yuviron.MediaWorker\Yuviron.MediaWorker.csproj
```

### 6. Stop local infrastructure

```powershell
docker compose down
```

To also remove local volumes and erase local databases:

```powershell
docker compose down -v
```

## Configuration

Configuration is loaded from standard ASP.NET Core sources:

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Production.json`
- environment variables
- generated appsettings files during CI/CD

Do not commit real secrets into source control. Use GitHub Secrets, environment variables, or a secret manager for real deployments.

Main configuration sections:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Port=3306;Database=yuviron_db;Uid=yuviron_user;Pwd=supersecret123;",
    "Redis": "localhost:6379"
  },
  "RabbitMQ": {
    "Host": "127.0.0.1",
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "ClickHouse": {
    "Host": "localhost",
    "HttpPort": "8123",
    "Database": "yuviron_analytics",
    "User": "yuviron_user",
    "Password": "supersecret123"
  },
  "Frontend": {
    "BaseUrl": "https://dev.yuviron.com"
  },
  "CorsSettings": {
    "AllowedOrigins": ["http://localhost:3000"]
  },
  "JwtSettings": {
    "Secret": "replace-with-secret",
    "Issuer": "YuvironApi",
    "Audience": "YuvironClient",
    "ExpiryMinutes": 60
  }
}
```

Important sections and values:

| Section / key | Purpose |
|---|---|
| `ConnectionStrings:Default` | MySQL connection string |
| `ConnectionStrings:Redis` | Redis connection string |
| `RabbitMQ:*` | RabbitMQ host, user, password, virtual host |
| `ClickHouse:*` | ClickHouse analytics connection settings |
| `Frontend:BaseUrl` | Base frontend URL used in emails and smart links |
| `CorsSettings:AllowedOrigins` | Allowed browser origins |
| `JwtSettings:*` | JWT signing and validation settings |
| `Email:*` | SMTP settings |
| `Stripe:*` | Stripe API and webhook secrets |
| `JamendoApi:ClientId` | Jamendo API client id |
| `StreamSecurity:SecretKey` | Stream URL signing secret |
| `FILE_STORAGE_ROOT` | File storage root path |
| `Otlp:Endpoint` | OpenTelemetry endpoint |
| `Seq:ServerUrl` | Seq log sink URL |

Environment variable format for nested config:

```powershell
$env:ConnectionStrings__Default = "Server=localhost;Port=3306;Database=yuviron_db;Uid=yuviron_user;Pwd=supersecret123;"
$env:Frontend__BaseUrl = "http://localhost:3000"
$env:JwtSettings__Secret = "local-development-secret"
```

## Database and Migrations

The project uses EF Core migrations in:

```text
src/Yuviron.Infrastructure/Persistence/Migrations
```

### Local migration behavior

When `ASPNETCORE_ENVIRONMENT=Development`, the API calls `Database.MigrateAsync()` during startup. That means local migrations are applied automatically when the API starts.

### Production migration behavior

In production, migrations are not applied by the API startup path. They are applied by the GitHub Actions deployment workflow before services are rebuilt and started.

Deployment step:

```text
Run DB migrations -> python3 ./scripts/cli.py stack migrate "${DEPLOY_ENV}"
```

### Create a migration

```powershell
dotnet ef migrations add MigrationName `
  --project src\Yuviron.Infrastructure\Yuviron.Infrastructure.csproj `
  --startup-project src\Yuviron.Api\Yuviron.Api.csproj `
  --context AppDbContext `
  --output-dir Persistence\Migrations
```

### Generate idempotent SQL script

```powershell
dotnet ef migrations script `
  --project src\Yuviron.Infrastructure\Yuviron.Infrastructure.csproj `
  --startup-project src\Yuviron.Api\Yuviron.Api.csproj `
  --context AppDbContext `
  --idempotent `
  --no-build
```

### Apply migrations manually in local development

Usually this is not needed because the API applies migrations automatically in Development. If manual update is needed:

```powershell
dotnet ef database update `
  --project src\Yuviron.Infrastructure\Yuviron.Infrastructure.csproj `
  --startup-project src\Yuviron.Api\Yuviron.Api.csproj `
  --context AppDbContext
```

## Tests and Build Checks

Run all tests:

```powershell
dotnet test Yuviron.slnx
```

Build release configuration:

```powershell
dotnet build Yuviron.slnx -c Release --no-restore
```

Recommended pre-push checks:

```powershell
dotnet restore Yuviron.slnx
dotnet test Yuviron.slnx
dotnet build Yuviron.slnx -c Release --no-restore
dotnet ef migrations script --project src\Yuviron.Infrastructure\Yuviron.Infrastructure.csproj --startup-project src\Yuviron.Api\Yuviron.Api.csproj --context AppDbContext --idempotent --no-build
```

## API Runtime

Important endpoints:

| Endpoint | Purpose |
|---|---|
| `/health/live` | Liveness check |
| `/health/ready` | Readiness check with dependencies |
| `/swagger` | Swagger UI when enabled |
| `/hubs/app` | SignalR hub |
| `/sl/{code}` | Legacy smart link redirect |
| `/album/{publicId}` | Public album smart link redirect |
| `/artist/{publicId}` | Public artist smart link redirect |
| `/track/{publicId}` | Public track smart link redirect |
| `/playlist/{publicId}` | Public playlist smart link redirect |
| `/user/{publicId}` | Public user profile smart link redirect |

Swagger documents:

- `/swagger/client/swagger.json`
- `/swagger/admin/swagger.json`
- `/swagger/artist/swagger.json`

Swagger is enabled in Development. It can also be enabled by setting:

```json
{
  "Swagger": {
    "Enabled": true
  }
}
```

## Media Worker

`Yuviron.MediaWorker` listens to RabbitMQ and runs background media processing, including audio transcoding workflows.

Run locally:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src\Yuviron.MediaWorker\Yuviron.MediaWorker.csproj
```

The worker requires RabbitMQ and shared configuration for storage, Jamendo, media settings, and infrastructure services.

## Smart Links

Smart links support shareable public URLs for:

- albums
- artists
- tracks
- playlists
- user profiles

The public URL shape is:

```text
{Frontend:BaseUrl}/{entity-segment}/{publicId}?si={smartLinkCode}
```

Examples:

```text
https://yuviron.com/artist/{publicId}?si={code}
https://yuviron.com/user/{publicId}?si={code}
```

The backend resolves these URLs through public redirect endpoints, records clicks when the `si` code matches an existing smart link, and redirects to the frontend route.

## Deployment Flow

Deployment is managed by:

```text
.github/workflows/deploy.yml
```

The workflow runs on pushes to:

| Branch | Environment | Purpose |
|---|---|---|
| `dev` | `dev` | Development deployment |
| `main` | `prod` | Production deployment |

There is no separate `prod` branch in the workflow. Production deployment is triggered by merge/push to `main`.

### CI/CD stages

The deploy job runs on a self-hosted runner with labels:

```text
self-hosted, yuviron
```

Main stages:

1. Capture currently deployed backend revision.
2. Sync backend source to the triggered commit.
3. Generate `appsettings.{Environment}.json` files from GitHub Secrets.
4. Clean stale migrator containers.
5. Report Docker disk usage.
6. Clean Docker build cache and dangling resources.
7. Run preflight checks.
8. Run EF Core database migrations.
9. Build and start backend, media worker, and nginx.
10. Run Trivy vulnerability scan.
11. Run smoke tests.
12. Roll back automatically if deployment fails after previous revision was captured.
13. Show service status and logs.

### Generated appsettings

The workflow calls:

```bash
python3 scripts/generate_appsettings.py
```

Generated files:

```text
src/Yuviron.Api/appsettings.{ASPNET_ENV}.json
src/Yuviron.MediaWorker/appsettings.{ASPNET_ENV}.json
```

Required GitHub Secrets used by the generator:

| Secret | Purpose |
|---|---|
| `JWT_SECRET` | JWT signing secret |
| `EMAIL_USERNAME` | SMTP username |
| `EMAIL_PASSWORD` | SMTP password |
| `SEED_ADMIN_EMAIL` | Initial admin email |
| `SEED_ADMIN_PASSWORD` | Initial admin password |
| `SEED_MANAGER_EMAIL` | Dev manager seed email |
| `SEED_MANAGER_PASSWORD` | Dev manager seed password |
| `SEED_USER_EMAIL` | Dev user seed email |
| `SEED_USER_PASSWORD` | Dev user seed password |
| `SEED_PREMIUM_EMAIL` | Dev premium user seed email |
| `SEED_PREMIUM_PASSWORD` | Dev premium user seed password |
| `JAMENDO_CLIENT_ID` | Jamendo client id |
| `STREAM_SECRET` | Stream signing secret |
| `STRIPE_SECRET_KEY` | Stripe secret key |
| `STRIPE_WEBHOOK_SECRET` | Stripe webhook secret |

## Production Deployment Checklist

Before merging into `main`:

- `dotnet test Yuviron.slnx` passes.
- `dotnet build Yuviron.slnx -c Release --no-restore` passes.
- EF Core idempotent migration script is generated successfully.
- New migrations are reviewed for existing production data.
- No real secrets are committed.
- Required GitHub Secrets exist in the correct GitHub environment.
- Frontend URLs and CORS origins are correct for production.
- Stripe webhook secret matches the production webhook endpoint.
- MySQL, Redis, RabbitMQ, ClickHouse, storage, nginx, and runner are healthy.

Recommended Git flow:

```powershell
git status
git add .
git commit -m "feat: describe your backend change"
git push origin your-branch
```

Then:

1. Open PR from `your-branch` to `dev`.
2. Merge into `dev`.
3. Wait for dev deployment and smoke tests.
4. Manually verify dev behavior.
5. Open PR from `dev` to `main`.
6. Merge into `main`.
7. Watch production GitHub Actions deployment.
8. Verify `/health/ready` and critical user flows.

## Rollback

The GitHub Actions workflow captures the previous deployed commit before deployment. If deployment fails, it attempts to:

1. Reset backend source to the previous commit.
2. Restore previous generated appsettings snapshots.
3. Clean Docker resources safely.
4. Rebuild/restart backend and media worker.
5. Run smoke tests.

Manual rollback on the server follows the same principle:

```bash
cd /opt/yuviron-server/src/yuviron-backend
git fetch origin <previous-sha>
git reset --hard <previous-sha>

cd /opt/yuviron-server
python3 ./scripts/cli.py stack restart backend media-worker --rebuild --env prod
python3 ./scripts/cli.py stack smoke prod
```

Use manual rollback only when the automated rollback fails or the deployment must be reverted after a successful release.

## Operational Checks

Local:

```powershell
docker compose ps
dotnet test Yuviron.slnx
```

Runtime health:

```text
GET /health/live
GET /health/ready
```

RabbitMQ UI:

```text
http://localhost:15672
```

Seq:

```text
http://localhost:8081
```

Aspire Dashboard:

```text
http://localhost:18888
```

Production checks are handled by:

```bash
python3 ./scripts/cli.py stack smoke prod
docker logs yuviron-prod-backend --tail 160
docker logs yuviron-prod-media-worker --tail 160
docker logs yuviron-prod-nginx --tail 160
```

## Security Notes

- Do not commit real SMTP, Stripe, JWT, database, Redis, RabbitMQ, or stream secrets.
- If a secret was committed, rotate it. Removing it from a later commit is not enough.
- Production deployment blocks fixable HIGH/CRITICAL container vulnerabilities through Trivy.
- Keep CORS origins strict.
- Keep `JwtSettings:Secret` long, random, and environment-specific.
- Keep `StreamSecurity:SecretKey` environment-specific.
- Store production secrets in GitHub Secrets or a dedicated secret manager.
- Review data migrations carefully before merging to `main`.

## Current Verification Baseline

At the time this README was updated, the backend passed:

```text
dotnet test Yuviron.slnx
Passed: 96, Failed: 0

dotnet build Yuviron.slnx -c Release --no-restore
Build succeeded: 0 warnings, 0 errors

dotnet ef migrations script --project src\Yuviron.Infrastructure\Yuviron.Infrastructure.csproj --startup-project src\Yuviron.Api\Yuviron.Api.csproj --context AppDbContext --idempotent --no-build
SQL script generated successfully
```
