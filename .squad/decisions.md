# Squad Decisions

## Active Decisions

### API Move Validation Required (2026-05-15)

**Status**: Open for Kane (Developer)  
**Source**: Parker test findings  
**Priority**: Medium

The `/api/game/move` endpoint does not validate that submitted moves are legal. Clients can place pieces anywhere, corrupting game state. While the Angular frontend enforces rules, the API must validate: check that `(MoveRow, MoveCol)` is in `board.GetValidMoves()` before applying. Return 400 if invalid.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
