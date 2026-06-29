# Parker — History

## Learnings

- Project: ASMX-Reversi — testing the Reversi game
- Frontend tests: `angular-reversi/` uses Jasmine + Karma (`ng test`)
- Backend tests: `ReversiApi.Tests/` using xUnit + Microsoft.AspNetCore.Mvc.Testing (59 tests, all passing)
- Existing test file: `angular-reversi/src/app/app.component.spec.ts`

### 2026-05-15 — Backend Test Suite Created
- Created `ReversiApi.Tests/` with xUnit targeting net10.0
- **ReversiBoardTests.cs** (30 tests): initial setup, valid/invalid moves, flipping (single/multi/chain), player switching, game-over (full board, no-moves-for-either-player), corners, edges, diagonal captures, serialization round-trip, MarkEligibleMoves, CloneWithMove immutability
- **ReversiAiTests.cs** (12 tests): all 3 difficulties return valid moves, time budget (<2s for hard), corner preference (medium/hard), edge cases (no moves, single move, near-endgame, few moves)
- **ApiIntegrationTests.cs** (17 tests): health endpoint, new game (pvp/ai), valid move, invalid move behavior, AI move all difficulties, full game flow to completion
- Added `public partial class Program { }` to `ReversiApi/Program.cs` for WebApplicationFactory access
- **Finding**: API does NOT validate moves server-side — invalid moves (e.g., placing on empty non-eligible cell) return 200 OK. Pieces get placed without any captures. This is a quality concern for Kane.
- **Finding**: Board serialization uses trailing comma — FillBoardArray handles it with TrimEnd(',')
- Test command: `dotnet test ReversiApi.Tests/`

## Team Updates (from Scribe)

- 2026-05-15: Parker findings merged into decisions.md as action item for Kane
- Move validation bug documented and ready for developer action
- No archival needed (decisions.md < 20KB)
