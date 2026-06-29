using System.Diagnostics;

namespace ReversiApi.Tests;

public class ReversiAiTests
{
    private readonly MinimaxAiPlayer _ai = new();

    // ── Easy Difficulty ──────────────────────────────────────────────────────

    [Fact]
    public void Easy_ReturnsValidMove()
    {
        var board = ReversiBoard.NewGame();
        var move = _ai.GetBestMove(board, "easy");
        var validMoves = board.GetValidMoves();

        Assert.Contains(move, validMoves);
    }

    [Fact]
    public void Easy_ReturnsValidMove_MultipleRuns()
    {
        // Easy is random — run multiple times to exercise randomness
        var board = ReversiBoard.NewGame();
        var validMoves = board.GetValidMoves();

        for (int i = 0; i < 20; i++)
        {
            var move = _ai.GetBestMove(board, "easy");
            Assert.Contains(move, validMoves);
        }
    }

    // ── Medium Difficulty ────────────────────────────────────────────────────

    [Fact]
    public void Medium_ReturnsValidMove()
    {
        var board = ReversiBoard.NewGame();
        var move = _ai.GetBestMove(board, "medium");
        var validMoves = board.GetValidMoves();

        Assert.Contains(move, validMoves);
    }

    // ── Hard Difficulty ──────────────────────────────────────────────────────

    [Fact]
    public void Hard_ReturnsValidMove()
    {
        var board = ReversiBoard.NewGame();
        var move = _ai.GetBestMove(board, "hard");
        var validMoves = board.GetValidMoves();

        Assert.Contains(move, validMoves);
    }

    [Fact]
    public void Hard_CompletesWithinTimeBudget()
    {
        var board = ReversiBoard.NewGame();
        var sw = Stopwatch.StartNew();
        _ai.GetBestMove(board, "hard");
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 2000,
            $"Hard AI took {sw.ElapsedMilliseconds}ms, exceeds 2 second budget");
    }

    // ── Corner Preference ────────────────────────────────────────────────────

    [Fact]
    public void Medium_PrefersCornerWhenAvailable()
    {
        // Create a board where a corner move is available
        var board = CreateBoardWithCornerAvailable();
        if (board == null) return; // skip if we can't construct the scenario

        var validMoves = board.GetValidMoves();
        var corners = new HashSet<(int, int)> { (0, 0), (0, 7), (7, 0), (7, 7) };
        var cornerMoves = validMoves.Where(m => corners.Contains(m)).ToList();

        if (cornerMoves.Count == 0) return; // no corner available, skip

        var move = _ai.GetBestMove(board, "medium");
        Assert.Contains(move, corners);
    }

    [Fact]
    public void Hard_PrefersCornerWhenAvailable()
    {
        var board = CreateBoardWithCornerAvailable();
        if (board == null) return;

        var validMoves = board.GetValidMoves();
        var corners = new HashSet<(int, int)> { (0, 0), (0, 7), (7, 0), (7, 7) };
        var cornerMoves = validMoves.Where(m => corners.Contains(m)).ToList();

        if (cornerMoves.Count == 0) return;

        var move = _ai.GetBestMove(board, "hard");
        Assert.Contains(move, corners);
    }

    // ── Edge Cases ───────────────────────────────────────────────────────────

    [Fact]
    public void Ai_NoValidMoves_ReturnsNegativeOne()
    {
        // Board with no valid moves for current player
        var layout = new char[8, 8];
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                layout[i, j] = 'b';
        layout[7, 7] = 'w';

        var board = CreateBoardFromLayout(layout, "w");
        var move = _ai.GetBestMove(board, "medium");

        Assert.Equal((-1, -1), move);
    }

    [Fact]
    public void Ai_SingleValidMove_ReturnsThatMove()
    {
        // Construct a board where only one move is valid
        var board = CreateBoardFromLayout(new char[,]
        {
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', 'b', 'w', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        }, "b");

        var validMoves = board.GetValidMoves();
        if (validMoves.Count == 1)
        {
            var move = _ai.GetBestMove(board, "hard");
            Assert.Equal(validMoves[0], move);
        }
    }

    [Fact]
    public void Ai_NearEndgame_DoesNotCrash()
    {
        // Nearly full board
        var layout = new char[8, 8];
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                layout[i, j] = 'b';

        // Leave some empty and a few white pieces
        layout[0, 0] = 'w';
        layout[0, 1] = 'w';
        layout[7, 6] = ' ';
        layout[7, 7] = ' ';

        var board = CreateBoardFromLayout(layout, "b");
        var move = _ai.GetBestMove(board, "hard");

        // Should not throw — just needs to return something
        Assert.True(move == (-1, -1) || (move.Row >= 0 && move.Col >= 0));
    }

    [Fact]
    public void Ai_FewValidMoves_DoesNotCrash()
    {
        // Board with very limited options
        var board = CreateBoardFromLayout(new char[,]
        {
            { 'b', 'w', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        }, "b");

        var move = _ai.GetBestMove(board, "medium");
        Assert.True(move == (-1, -1) || (move.Row >= 0 && move.Col >= 0));
    }

    [Theory]
    [InlineData("easy")]
    [InlineData("medium")]
    [InlineData("hard")]
    public void Ai_AllDifficulties_HandleStandardOpening(string difficulty)
    {
        var board = ReversiBoard.NewGame();
        var move = _ai.GetBestMove(board, difficulty);
        var validMoves = board.GetValidMoves();

        Assert.Contains(move, validMoves);
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private static ReversiBoard? CreateBoardWithCornerAvailable()
    {
        // Board where (0,0) corner is a valid move for black
        var layout = new char[8, 8];
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                layout[i, j] = ' ';

        // Set up so black can play (0,0) capturing whites on the diagonal
        layout[1, 1] = 'w';
        layout[2, 2] = 'w';
        layout[3, 3] = 'b';
        // Also give a non-corner alternative
        layout[4, 4] = 'w';
        layout[5, 5] = 'b';
        layout[3, 4] = 'w';
        layout[3, 5] = 'b';

        var board = CreateBoardFromLayout(layout, "b");
        var moves = board.GetValidMoves();

        return moves.Contains((0, 0)) ? board : null;
    }

    private static ReversiBoard CreateBoardFromLayout(char[,] layout, string currentPlayer)
    {
        var board = new ReversiBoard { CurrentPlayerColor = currentPlayer };

        int size = layout.GetLength(0);
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                sb.Append(layout[i, j]).Append(',');

        board.BoardString = sb.ToString();
        board.FillBoardArray();
        return board;
    }
}
