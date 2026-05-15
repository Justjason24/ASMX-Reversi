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
