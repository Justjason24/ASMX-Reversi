# Kane — History

## Learnings

- Project: ASMX-Reversi — modernizing legacy ASMX backend
- Legacy backend: `OldBones/` — .NET Framework, `WebService1.asmx`, `ReversiBoard.cs`
- Legacy Dockerfile exists at `OldBones/OldBones/Dockerfile`
- docker-compose.yml maps reversi-app1 on port 44329:80 and angular-reversi on 4200
- Goal: replace ASMX with modern .NET minimal API + add AI opponent

### 2026-05-15T12:43:39-05:00: Phase 1+2+3 — Backend Modernization Complete

**New project:** `ReversiApi/` — .NET 10 minimal API (net10.0)
**SDK available on machine:** 10.0.204

**Key files created:**
- `ReversiApi/ReversiBoard.cs` — ported game engine (modern C#)
- `ReversiApi/ReversiAi.cs` — AI: IReversiAi interface + MinimaxAiPlayer
- `ReversiApi/Models.cs` — request/response DTOs (records)
- `ReversiApi/Program.cs` — minimal API: /api/health, /api/game/new, /api/game/move, /api/game/ai-move
- `ReversiApi/Dockerfile` — multi-stage Linux build on mcr.microsoft.com/dotnet/sdk:10.0

**Critical bug found and fixed in the legacy eligible-moves algorithm:**
The original `MarkEligibleMoves()` scanned from OPPONENT pieces through CURRENT PLAYER pieces to
find empty cells. This is backwards — it finds cells where a placement would be flanked by the
current player's OWN pieces, not by opponents. The correct algorithm scans from CURRENT PLAYER
pieces through OPPONENT pieces to find empty cells. The new `GetValidMoves()` implements this.
Verified: initial board for black correctly returns (2,3),(3,2),(4,5),(5,4) — the standard moves.

**CheckGameOver bug fixed:**
Legacy only checked if board is full. Now also ends game when neither player has valid moves.

**Flip logic (UpdateBoardPieces/LookDirection) was correct in the original** — no fix needed there.
The 8 directional Look methods collapsed into a single parameterized `LookDirection(dRow,dCol)`.

**AI architecture:**
- Easy: depth 1, random move selection
- Medium: depth 3, positional weight heuristic (corners +100, X-squares -25, edges +10)
- Hard: depth 5, full evaluation (positional + piece count + mobility)
- Alpha-beta pruning applied at all levels
- `CloneWithMove(row,col)` applies move, flips pieces, switches player — used for AI simulation

**API contract (stateless — board state travels with each request):**
- POST /api/game/new → GameStateResponse (gameId, boardString, board[][], currentPlayer, gameOver, scores, eligibleMoves)
- POST /api/game/move → same GameStateResponse
- POST /api/game/ai-move → same, AI selects and applies move
- GET /api/health → {status, version}
- CORS configured via CORS_ORIGINS env var (comma-separated origins)

**Board string format preserved:** comma-delimited chars (b/w/e/ ) with trailing comma, 8×8=64 tokens.
Board 2D array added as `board[][]` in response for new frontend code.

**Build verified:** `dotnet build ReversiApi/` — succeeded, 0 warnings, 0 errors
**Runtime verified:** all 4 endpoints smoke-tested, eligible moves + flips + AI all correct

---

## Team Sprint Summary (2026-05-15T18:19:23Z)

**Backend complete.** Dallas provided architecture roadmap, Ash containerized the API, Lambert wired frontend to JSON endpoints.

**Remaining team status:**
- **Ash:** Deployed Bicep infrastructure and GitHub Actions CI/CD; docker-compose tested locally
- **Lambert:** Frontend rewired to JSON, AI mode UX complete, environment config (dev/prod) working
- **Ready for deployment:** Push to `main` triggers GitHub Actions → builds images → pushes to ACR → deploys to Container Apps
- **Blocking:** GitHub secrets must be configured (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID)
