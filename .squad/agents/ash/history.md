# Ash — History

## Learnings

- Project: ASMX-Reversi — containerize and deploy to Azure Container Apps
- Existing docker-compose.yml with two services: reversi-app1 (port 44329:80), angular-reversi (port 4200)
- Existing Dockerfiles: `angular-reversi/Dockerfile`, `OldBones/OldBones/Dockerfile`
- Target: Azure Container Apps with ACR

### 2026-05-15T12:43:39-05:00: Phase 4 + 5 — Containerization & Azure Infra (completed)

**Docker Compose:**
- Replaced `docker-compose.yml` at repo root with Linux-container setup
- Services: `reversi-api` (port 8080) with healthcheck, `angular-reversi` (port 80) with `depends_on: service_healthy`
- CORS_ORIGINS env var on backend set to `http://localhost,http://localhost:80`

**Frontend Dockerfile (`angular-reversi/Dockerfile`):**
- Replaced Windows Server Core / npm-start dev-server image with a proper Linux multi-stage build
- Stage 1: `node:18-alpine` — `npm ci` + `ng build --configuration production`
- Stage 2: `nginx:alpine` — serves `dist/asmx-reversi`, uses `nginx.conf`

**nginx.conf (`angular-reversi/nginx.conf`):**
- Added `/api/` reverse proxy block → `http://reversi-api:8080/api/`
- SPA fallback (`try_files $uri $uri/ /index.html`) retained
- Backend API must be named `reversi-api` in compose/container networking for proxy to resolve

**Backend Dockerfile (`ReversiApi/Dockerfile`):**
- Already uses .NET 10 (consistent with user directive)
- No changes needed

**Bicep Infrastructure (`infra/`):**
- `main.bicep` — orchestrator; params: `projectName` (default: `reversi`), `imageTag`
- `modules/acr.bicep` — Basic SKU ACR, admin user enabled, outputs loginServer + credentials
- `modules/environment.bicep` — Log Analytics (PerGB2018, 30-day retention) + Container Apps Environment
- `modules/api-app.bicep` — reversi-api app; external ingress port 8080; scale 0–10; HTTP scaling rule (20 concurrent); readiness + liveness probes on `/api/health`; CORS_ORIGINS set to frontend FQDN
- `modules/frontend-app.bicep` — angular-reversi app; external ingress port 80; scale 1–5; HTTP scaling rule

**ACR naming:** `${projectName}acr${uniqueString(resourceGroup().id)}` — avoids global name collisions

**Circular dependency note:** `main.bicep` uses a forward reference for `frontendApp.outputs.fqdn` in the `apiApp` module. This may require a two-pass deploy on first run (deploy infra first, then update CORS_ORIGINS on api-app with the actual frontend URL). Consider using a placeholder URL on first deploy.

**CI/CD (`github/workflows/deploy.yml`):**
- Trigger: push to `main`
- OIDC auth via `azure/login@v2` (uses `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` secrets)
- Job 1: builds + pushes both images to ACR (tag = 8-char SHA + `latest`)
- Job 2: `deploy` (environment: `production`) — deploys Bicep, then updates each container app image
- Resource group hardcoded as `rg-reversi` — teams should set this appropriately before first deploy

**Key file paths:**
- `docker-compose.yml` — repo root
- `angular-reversi/Dockerfile` — frontend Linux multi-stage
- `angular-reversi/nginx.conf` — nginx config with /api/ proxy
- `ReversiApi/Dockerfile` — backend .NET 10 multi-stage (pre-existing, unchanged)
- `infra/main.bicep` — Bicep orchestrator
- `infra/modules/acr.bicep`
- `infra/modules/environment.bicep`
- `infra/modules/api-app.bicep`
- `infra/modules/frontend-app.bicep`
- `.github/workflows/deploy.yml` — CI/CD pipeline
