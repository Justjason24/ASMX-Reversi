# ASMX-Reversi Decisions Log

**Last Updated:** 2026-05-15T12:43:39-05:00

---

## 2026-05-15: User directive — Target .NET 10 for Backend

**By:** User (via Copilot)  
**Category:** Architecture Decision

Target .NET 10 for the backend modernization, not .NET 8. Captured for team memory and captured in Kane's backend implementation.

---

## 2026-05-15: Ash Infrastructure Decisions

**By:** Ash (Cloud/DevOps)  
**Date:** 2026-05-15T12:43:39-05:00  
**Status:** Decided

### Decision 1: Backend External Ingress with CORS (not nginx proxy for API)

**Context:** Two options for frontend→backend communication in Azure Container Apps:
1. Backend internal ingress, frontend nginx proxies `/api/` to internal backend URL
2. Backend external ingress, CORS configured on backend to allow frontend origin

**Decision:** External ingress on backend with CORS. The docker-compose setup uses nginx proxy (option 1) for local dev convenience (avoids CORS entirely in local), but in Azure the backend is deployed with external ingress and CORS_ORIGINS is set to the frontend Container App FQDN.

**Rationale:** Simpler Azure networking (no internal DNS resolution needed), easier debugging (API URL is directly accessible), consistent with the plan's recommendation. The nginx `/api/` proxy in docker-compose remains useful for local development.

### Decision 2: ACR Admin Credentials (not Managed Identity) for Initial Setup

**Status:** Decided — revisit before production hardening

**Context:** Container Apps can pull from ACR via:
1. Admin credentials (username/password stored as secrets)
2. Managed Identity assigned to the Container Apps Environment

**Decision:** Admin credentials for initial setup. Bicep uses `acr.listCredentials()` outputs.

**Rationale:** Simpler Bicep — Managed Identity approach requires additional role assignments (AcrPull role). For a project at this stage, admin credentials are acceptable. Should be migrated to Managed Identity before a production workload.

**Action for future:** Replace admin credential secrets with a system-assigned managed identity on the Container Apps Environment with `AcrPull` role on the ACR.

### Decision 3: Two-Stage Deploy for First Run (CORS circular dependency)

**Status:** Known limitation

**Context:** `main.bicep` references `frontendApp.outputs.fqdn` to set `CORS_ORIGINS` on the backend. On the very first deploy, both apps are created simultaneously and the frontend FQDN isn't known before the backend is provisioned.

**Decision:** Accept this limitation. On first deploy, `CORS_ORIGINS` will be set to the frontend URL *after* it is known (Bicep evaluates the ARM deployment graph and resolves outputs). ARM handles this correctly for new deployments — the frontend FQDN is determined during the same deployment, so ARM's dependency resolution should handle it.

**Note:** If this causes issues in practice, the fallback is a two-pass deploy: first deploy without `frontendUrl` parameter (use a placeholder), then re-deploy with the actual FQDN.

### Decision 4: Scale-to-Zero for Backend, Min 1 for Frontend

**Status:** Decided

**Context:** Cost optimization for a non-production workload.

**Decision:**
- Backend (`reversi-api`): minReplicas=0 (scale to zero when idle — saves cost)
- Frontend (`angular-reversi`): minReplicas=1 (always one instance — avoids cold-start latency for first page load)

**Rationale:** Cold start on the backend is acceptable (API call will wait); cold start on the frontend (nginx static file serving) affects perceived page load and is less acceptable.

### Decision 5: GitHub Actions OIDC — Required Secrets

**Status:** Documented for team setup

The deploy workflow requires these GitHub repository/environment secrets to be configured before the pipeline can run:

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | App registration client ID (federated credential configured for this repo) |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Target Azure subscription ID |

The GitHub environment `production` should be created in the repository settings with these secrets. OIDC federated credentials must be configured on the Azure app registration with subject `repo:seanbot2000/ASMX-Reversi:environment:production`.

---

## 2026-05-15: Kane Backend Modernization — Decisions

**Author:** Kane (Backend Dev)  
**Date:** 2026-05-15T12:43:39-05:00  
**Status:** Done — Phases 1, 2, 3 complete

### What Was Built

`ReversiApi/` — new .NET 10 minimal API replacing the legacy ASMX service.

Files:
- `ReversiApi/ReversiBoard.cs` — ported + corrected game engine
- `ReversiApi/ReversiAi.cs` — minimax AI with alpha-beta pruning (IReversiAi / MinimaxAiPlayer)
- `ReversiApi/Models.cs` — request/response records
- `ReversiApi/Program.cs` — 4 endpoints, CORS middleware, DI wiring
- `ReversiApi/Dockerfile` — multi-stage Linux build targeting .NET 10

### Decision 1: Target .NET 10 (not .NET 8)

Per user directive (copilot-directive-20260515-net10.md). SDK 10.0.204 is present on the build machine.
Docker images: `mcr.microsoft.com/dotnet/sdk:10.0` / `mcr.microsoft.com/dotnet/aspnet:10.0`.

### Decision 2: Stateless API design

Board state travels with every request (boardString + currentPlayer). No server-side session storage.
The `gameId` field is passed through for client tracking but the server does not store it.
Rationale: simpler, horizontally scalable, no distributed cache needed.

### Decision 3: Eligible-moves algorithm rewritten (NOT a simple port)

The legacy `MarkEligibleMoves()` algorithm was fundamentally incorrect — it scanned from
opponent pieces through current-player pieces to empty cells. This is the **wrong direction**;
it found positions where placing would be flanked by the current player's own pieces rather
than opponent pieces, producing false positives and missing real valid moves.

**New algorithm** in `GetValidMoves()`: scans from each CURRENT PLAYER piece through OPPONENT
pieces in all 8 directions; the first empty cell after ≥1 opponent piece is a valid placement.

This was verified against the standard Reversi starting position: black's valid moves are
exactly (2,3), (3,2), (4,5), (5,4).

The flip logic (LookDirection / UpdateBoardPieces) was correct in the original and preserved.

### Decision 4: CheckGameOver bug fixed

Legacy `CheckGameOver()` only returned true when the board was completely full. Standard Reversi
ends when neither player has any valid moves (which can happen with empty cells remaining).
New implementation: game over if board is full OR if neither player has valid moves.

### Decision 5: CORS via environment variable

CORS origins configured via `CORS_ORIGINS` env var (comma-separated). Falls back to localhost
development origins. This lets docker-compose / Azure Container Apps configure origins without
rebuilding the image.

### Decision 6: AI difficulty mapping

- Easy: random selection from valid moves (depth 1)
- Medium: minimax depth 3, positional weight heuristic
- Hard: minimax depth 5, positional weights + piece count + mobility

### Decision 7: boardString format preserved exactly

The comma-delimited format with trailing comma is maintained so the existing Angular frontend
can continue to function during the JSON migration transition. A typed `board[][]` field is also
returned for new frontend code.

### API Contract Summary

**POST /api/game/new**
```json
Request:  { "mode": "pvp|ai", "difficulty": "easy|medium|hard" }
Response: GameStateResponse (see below)
```

**POST /api/game/move**
```json
Request:  { "gameId": "uuid", "boardString": "...", "currentPlayer": "b|w", "moveRow": 2, "moveCol": 3 }
Response: GameStateResponse
```

**POST /api/game/ai-move**
```json
Request:  { "gameId": "uuid", "boardString": "...", "currentPlayer": "w", "difficulty": "medium" }
Response: GameStateResponse (AI move already applied, state is for the next player)
```

**GET /api/health**
```json
Response: { "status": "healthy", "version": "1.0.0" }
```

**GameStateResponse shape**
```json
{
  "gameId": "uuid",
  "boardString": " , , ,...,e,...,",
  "board": [["b","w",...], ...],
  "currentPlayer": "b",
  "gameOver": false,
  "blackScore": 2,
  "whiteScore": 2,
  "eligibleMoves": [{ "row": 2, "col": 3 }, ...]
}
```

Port default: 8080. CORS must include `http://localhost:4200` for Angular dev.

---

## 2026-05-15: Lambert Frontend Decisions — Phase 6 SOAP → REST/JSON Migration

**Author:** Lambert (Frontend Dev)  
**Date:** 2026-05-15T12:43:39-05:00

**Decision:** Rewired Angular frontend from SOAP/XML to JSON REST API as specified in the modernization plan Phase 6.

### Key Choices

**1. Board display merging**  
The new backend returns `board: string[][]` (cells: `b`, `w`, ` `) and `eligibleMoves: {row, col}[]` as separate fields. The component merges them by overlaying `'e'` onto the display grid — this keeps the template unchanged (still checks `cell === 'e'`).

**2. AI turn detection**  
After a player's move response, if `state.currentPlayer !== 'b'` (i.e., it's White's turn) in AI mode, the AI is triggered. This assumes human = Black, AI = White — consistent with standard Reversi convention where Black moves first.

**3. AI thinking delay**  
A 600ms `setTimeout` before calling `/api/game/ai-move` provides a brief visual pause so the board update is visible before the AI responds. This is UX polish, not a technical requirement.

**4. Pass move convention**  
Retained the `-1, -1` row/col convention for "no eligible moves" pass. The backend must handle this gracefully.

**5. Environment config**  
Dev uses `http://localhost:8080/api` (direct to .NET API), prod uses `/api` (nginx reverse proxy). Added `fileReplacements` to `angular.json` production config.

**6. Bundle budget warnings**  
Adding MatButtonToggleModule, MatSelectModule, MatFormFieldModule pushed the initial bundle from ~500kb to ~688kb (budget warning). This is acceptable — the budget thresholds are conservative defaults and the build succeeds. Can be addressed by lazy-loading modules if needed.

---

## 2026-05-15: Dallas Modernization Plan

**Author:** Dallas (Lead)  
**Date:** 2026-05-15  
**Status:** Master Plan (Reference)

High-level modernization strategy: ASMX → .NET 10 minimal API, Angular 16 → JSON REST, Windows containers → Linux, add AI opponent via minimax, deploy to Azure Container Apps.

See `.squad/decisions/inbox/dallas-modernization-plan.md` for full details and Phase dependencies.
