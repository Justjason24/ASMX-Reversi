# Lambert — History

## Learnings

- Project: ASMX-Reversi — Angular 16 + Material frontend for Reversi
- Key path: `angular-reversi/src/app/` — components in `reversi/`, services in `services/`
- Uses Angular Material for UI components
- Existing Dockerfile at `angular-reversi/Dockerfile`, nginx.conf for serving
- Phase 6 complete (2026-05-15): SOAP/XML replaced with REST/JSON via HttpClient
- Environment files: `angular-reversi/src/environments/environment.ts` (dev: http://localhost:8080/api) and `environment.prod.ts` (prod: /api via nginx)
- `angular.json` requires `fileReplacements` entry in production config to swap environment files
- Service interfaces: GameState, NewGameRequest, MoveRequest, AiMoveRequest in `dataservice.service.ts`
- Board display: server returns `board: string[][]` (b/w/space) + `eligibleMoves: {row,col}[]` separately; component merges them with 'e' for rendering
- AI flow: player move → POST /api/game/move → 600ms setTimeout → POST /api/game/ai-move; `aiThinking` flag disables board clicks
- New Material modules added to app.module.ts: MatButtonToggleModule, MatSelectModule, MatFormFieldModule, MatButtonModule
- Build command (verified): `cd angular-reversi && npm install && npx ng build`

---

## Team Sprint Summary (2026-05-15T18:19:23Z)

**Phase 6 complete.** Frontend fully integrated with new backend API.

**Remaining team status:**
- **Dallas:** Provided modernization plan and architecture direction; sprint leadership complete
- **Kane:** Backend .NET 10 API with minimax AI fully implemented and tested
- **Ash:** Infrastructure, docker-compose, GitHub Actions CI/CD all ready
- **Ready:** All 4 endpoints (new, move, ai-move, health) verified; game flow tested with PvP and AI modes; bundle size acceptable (688 KB with Material components)
- **Deployment:** Ready for Azure Container Apps. GitHub Actions pipeline will trigger on `git push main` once secrets configured.
- **Game UX:** Human plays Black (first), AI plays White; 3 difficulty levels (Easy/Medium/Hard); 600ms AI thinking delay for visual polish
