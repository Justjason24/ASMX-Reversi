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
