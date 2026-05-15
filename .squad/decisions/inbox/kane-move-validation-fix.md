# Decision: Server-side move validation on `/api/game/move`

**Author:** Kane (Backend Dev)  
**Date:** 2025-05-15  

## Context
The `/api/game/move` endpoint accepted any (row, col) coordinates without verifying they represent a legal Reversi move. Invalid moves returned 200 OK and corrupted the board state.

## Decision
Added server-side validation before applying a move. The endpoint now calls `board.GetValidMoves()` and checks whether the requested position is in the list. If not, it returns **400 Bad Request** with `{ "error": "Invalid move" }`.

## Updated test
The existing test `Move_InvalidMove_StillReturns200ButNoCapture` documented the old buggy behavior. Renamed it to `Move_InvalidMove_Returns400BadRequest` and updated the assertion to expect 400.

## Result
All 59 tests pass. Invalid moves are now rejected before they can corrupt game state.
