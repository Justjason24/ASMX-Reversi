# Dallas — History

## Learnings

- Project: ASMX-Reversi — Angular 16 + ASMX/.NET Reversi game
- Stack: Angular 16 w/ Material, legacy .NET Framework ASMX backend (OldBones/), Docker
- Goals: Modernize to modern .NET, containerize for Azure Container Apps, add AI opponent
- Key paths: `angular-reversi/` (frontend), `OldBones/` (legacy backend), `docker-compose.yml`
- 2026-05-15: Completed full codebase analysis and wrote modernization plan to `.squad/decisions/inbox/dallas-modernization-plan.md`
- Backend: ReversiBoard.cs is ~810 lines with 8 directional Look* methods for flipping + 8 for eligible moves. Core data model is comma-delimited board string with chars b/w/e/space.
- Frontend: DataserviceService builds raw SOAP XML envelopes and parses responses with indexOf string matching — no XML parser used.
- Architecture decision: Minimax with alpha-beta pruning for AI opponent; 3 difficulty levels (depth 1/3/5-6).
- Architecture decision: Switch from Windows containers to Linux containers (nginx for frontend, aspnet:8.0 for backend).
- Architecture decision: Backend API uses JSON REST endpoints, not SOAP. Clean break from XML protocol.
- Bug found: CheckGameOver() only checks if board is full — does not handle case where neither player has valid moves.
- Dead code: Person.cs, Helper.cs referenced in .csproj but missing from disk. HelloWorld/AddA/Debug endpoints are test artifacts.
- The existing nginx.conf in angular-reversi/ is correct for SPA serving but currently unused — the Dockerfile runs npm start instead.
- Phase execution order: Backend port → API contract → AI + Containerization (parallel) → Frontend update → Azure deploy.

---

## Team Sprint Summary (2026-05-15T18:19:23Z)

**All phases complete** — end-to-end modernization delivered.

**Kane (Backend):** Ported ReversiBoard.cs to .NET 10, fixed eligible-moves and CheckGameOver bugs, implemented minimax AI (3 difficulty levels). Created 4-endpoint REST API with stateless design and CORS support.

**Ash (DevOps):** Updated docker-compose to Linux, rewrote both Dockerfiles (multi-stage), added nginx reverse proxy, wrote complete Bicep infrastructure (ACR + Container Apps + Log Analytics), configured GitHub Actions CI/CD with OIDC.

**Lambert (Frontend):** Rewired from SOAP/XML to JSON REST, implemented game mode selection UI (PvP/AI), added AI opponent integration with difficulty selector, configured environment-specific API endpoints (dev/prod).

**Outcome:** 100% ready for cloud deployment. GitHub Actions pipeline requires secrets (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID) to execute. No rework needed.
