using ReversiApi;

var builder = WebApplication.CreateBuilder(args);

// ─── CORS ─────────────────────────────────────────────────────────────────────
// Origins are configured via CORS_ORIGINS env var (comma-separated).
// Falls back to localhost development origins.
var corsOrigins = (builder.Configuration["CORS_ORIGINS"]
    ?? "http://localhost:4200,http://localhost,http://localhost:80")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddSingleton<IReversiAi, MinimaxAiPlayer>();

var app = builder.Build();
app.UseCors();

// ─── Health ───────────────────────────────────────────────────────────────────
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", version = "1.0.0" }));

// ─── New Game ─────────────────────────────────────────────────────────────────
app.MapPost("/api/game/new", (NewGameRequest request) =>
{
    var board = ReversiBoard.NewGame();
    board.MarkEligibleMoves();
    board.StringifyBoard();
    return Results.Ok(ToGameState(board, Guid.NewGuid().ToString()));
});

// ─── Player Move ──────────────────────────────────────────────────────────────
app.MapPost("/api/game/move", (MoveRequest request) =>
{
    var board = new ReversiBoard
    {
        BoardString = request.BoardString,
        CurrentPlayerColor = request.CurrentPlayer,
        MoveRow = request.MoveRow,
        MoveCol = request.MoveCol,
    };

    board.FillBoardArray();

    if (board.MoveRow >= 0 && board.MoveCol >= 0)
    {
        var validMoves = board.GetValidMoves();
        bool isValid = validMoves.Any(m => m.Row == board.MoveRow && m.Col == board.MoveCol);
        if (!isValid)
            return Results.BadRequest(new { error = "Invalid move" });

        board.UpdateBoardPieces();
    }

    board.SetOppositePlayerColor();
    board.MarkEligibleMoves();
    board.StringifyBoard();
    board.CheckGameOver();

    return Results.Ok(ToGameState(board, request.GameId));
});

// ─── AI Move ──────────────────────────────────────────────────────────────────
app.MapPost("/api/game/ai-move", (AiMoveRequest request, IReversiAi ai) =>
{
    var board = new ReversiBoard
    {
        BoardString = request.BoardString,
        CurrentPlayerColor = request.CurrentPlayer,
    };
    board.FillBoardArray();

    var aiMove = ai.GetBestMove(board, request.Difficulty);

    if (aiMove.Row >= 0)
    {
        board.MoveRow = aiMove.Row;
        board.MoveCol = aiMove.Col;
        board.UpdateBoardPieces();
    }

    board.SetOppositePlayerColor();
    board.MarkEligibleMoves();
    board.StringifyBoard();
    board.CheckGameOver();

    return Results.Ok(ToGameState(board, request.GameId));
});

app.Run();

// ─── Helper: build GameStateResponse from a board ────────────────────────────
static GameStateResponse ToGameState(ReversiBoard board, string gameId)
{
    var data = board.BoardData;
    int size = (int)Math.Sqrt(data.Length);

    var board2d = new string[size][];
    var eligibleMoves = new List<MovePosition>();

    for (int i = 0; i < size; i++)
    {
        board2d[i] = new string[size];
        for (int j = 0; j < size; j++)
        {
            board2d[i][j] = data[i, j].ToString();
            if (data[i, j] == 'e')
                eligibleMoves.Add(new MovePosition(i, j));
        }
    }

    return new GameStateResponse(
        GameId: gameId,
        BoardString: board.BoardString,
        Board: board2d,
        CurrentPlayer: board.CurrentPlayerColor,
        GameOver: board.GameOver,
        BlackScore: board.CountPieces('b'),
        WhiteScore: board.CountPieces('w'),
        EligibleMoves: [.. eligibleMoves]);
}

// Make Program class accessible for integration tests (WebApplicationFactory)
public partial class Program { }

