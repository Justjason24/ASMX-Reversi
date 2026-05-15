namespace ReversiApi;

// ─── Request Models ───────────────────────────────────────────────────────────

public record NewGameRequest(
    string Mode = "pvp",
    string Difficulty = "medium");

public record MoveRequest(
    string GameId,
    string BoardString,
    string CurrentPlayer,
    int MoveRow,
    int MoveCol);

public record AiMoveRequest(
    string GameId,
    string BoardString,
    string CurrentPlayer,
    string Difficulty = "medium");

// ─── Response Models ──────────────────────────────────────────────────────────

public record MovePosition(int Row, int Col);

/// <summary>
/// Canonical game state returned by all game endpoints.
/// boardString maintains the legacy comma-delimited format for client compatibility.
/// board is a typed 8×8 array for new frontend code.
/// </summary>
public record GameStateResponse(
    string GameId,
    string BoardString,
    string[][] Board,
    string CurrentPlayer,
    bool GameOver,
    int BlackScore,
    int WhiteScore,
    MovePosition[] EligibleMoves);
