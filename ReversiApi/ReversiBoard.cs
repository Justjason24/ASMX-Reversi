namespace ReversiApi;

/// <summary>
/// Core Reversi game engine. Ported from the legacy OldBones ASMX service.
/// Key changes from legacy:
///   - 8 directional Look methods collapsed into single parameterized LookDirection
///   - Eligible-moves search corrected: now scans from current-player pieces through
///     opponent pieces to empty cells (legacy scanned the wrong direction)
///   - CheckGameOver fixed: now also ends game when neither player has valid moves
///   - int board dimensions (was double via Math.Sqrt)
///   - Value tuples instead of Tuple&lt;int,int&gt;
///   - No Console.WriteLine debug output
///   - Exposes BoardData and GetValidMoves/CloneWithMove for AI
/// </summary>
public class ReversiBoard
{
    private const int BoardSize = 8;

    private static readonly (int dRow, int dCol)[] Directions =
    [
        (0, -1),  // left
        (0,  1),  // right
        (-1, 0),  // up
        ( 1, 0),  // down
        (-1, -1), // top-left
        (-1,  1), // top-right
        ( 1, -1), // down-left
        ( 1,  1), // down-right
    ];

    public string CurrentPlayerColor { get; set; } = "b";
    public int MoveRow { get; set; } = -1;
    public int MoveCol { get; set; } = -1;
    public string BoardString { get; set; } = string.Empty;
    public bool GameOver { get; set; }

    private char[,] _board = new char[BoardSize, BoardSize];

    /// <summary>Exposes board data read-only for AI evaluation.</summary>
    public char[,] BoardData => _board;

    // ──────────────────────────────────────────────────────────────────────────
    // Public API (mirrors legacy surface for service compatibility)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses BoardString into the internal 2-D board array.
    /// BoardString is a comma-delimited series of 'b','w','e',' ' chars, with
    /// an optional trailing comma (as produced by StringifyBoard).
    /// </summary>
    public void FillBoardArray()
    {
        var cells = BoardString.TrimEnd(',').Split(',');
        int size = (int)Math.Round(Math.Sqrt(cells.Length));
        _board = new char[size, size];
        int idx = 0;
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                _board[i, j] = cells[idx++][0];
    }

    /// <summary>
    /// Applies the move at (MoveRow, MoveCol) for the current player and flips
    /// all bracketed opponent pieces in every direction.
    /// </summary>
    public void UpdateBoardPieces()
    {
        _board[MoveRow, MoveCol] = CurrentPlayerColor[0];

        var toFlip = new List<(int Row, int Col)>();
        foreach (var (dRow, dCol) in Directions)
            toFlip.AddRange(LookDirection(dRow, dCol));

        foreach (var (row, col) in toFlip.Distinct())
            _board[row, col] = CurrentPlayerColor[0];
    }

    /// <summary>
    /// Clears previous 'e' eligible-move markers and recalculates valid moves
    /// for the current player, marking them 'e' on the board.
    /// </summary>
    public void MarkEligibleMoves()
    {
        int size = Size();
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if (_board[i, j] == 'e') _board[i, j] = ' ';

        foreach (var (row, col) in GetValidMoves())
            _board[row, col] = 'e';
    }

    /// <summary>Serialises the board back to a comma-delimited BoardString.</summary>
    public void StringifyBoard()
    {
        int size = Size();
        var sb = new System.Text.StringBuilder(size * size * 2);
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                sb.Append(_board[i, j]).Append(',');
        BoardString = sb.ToString();
    }

    /// <summary>
    /// Checks whether the game is over.
    /// Returns true (and sets GameOver=true) if:
    ///   - the board is completely full, OR
    ///   - neither the current player nor the opponent has any valid moves.
    /// This fixes the legacy bug where only a full board was considered game-over.
    /// </summary>
    public bool CheckGameOver()
    {
        int size = Size();
        bool hasBlank = false;
        for (int i = 0; i < size && !hasBlank; i++)
            for (int j = 0; j < size && !hasBlank; j++)
                if (_board[i, j] == ' ' || _board[i, j] == 'e')
                    hasBlank = true;

        if (!hasBlank)
        {
            GameOver = true;
            return true;
        }

        if (GetValidMoves().Count > 0)
        {
            GameOver = false;
            return false;
        }

        // Current player has no moves – check opponent
        string saved = CurrentPlayerColor;
        CurrentPlayerColor = GetOppositePlayerColor().ToString();
        bool opponentHasMoves = GetValidMoves().Count > 0;
        CurrentPlayerColor = saved;

        GameOver = !opponentHasMoves;
        return GameOver;
    }

    /// <summary>
    /// Returns all valid move positions for the current player.
    /// Algorithm: from each existing current-player piece, walk outward through
    /// opponent pieces in each of 8 directions; the first empty cell found
    /// (if any opponents were traversed) is a valid placement.
    /// This is the corrected algorithm – the legacy code scanned the wrong
    /// direction and produced both false positives and missed valid moves.
    /// </summary>
    public List<(int Row, int Col)> GetValidMoves()
    {
        int size = Size();
        char currentColor = CurrentPlayerColor[0];
        char oppositeColor = GetOppositePlayerColor();
        var result = new HashSet<(int, int)>();

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (_board[i, j] != currentColor) continue;

                foreach (var (dRow, dCol) in Directions)
                {
                    int r = i + dRow;
                    int c = j + dCol;

                    // First step must be an opponent piece
                    if (r < 0 || r >= size || c < 0 || c >= size) continue;
                    if (_board[r, c] != oppositeColor) continue;

                    // Walk through opponent pieces
                    r += dRow;
                    c += dCol;
                    while (r >= 0 && r < size && c >= 0 && c < size)
                    {
                        char cell = _board[r, c];
                        if (cell == oppositeColor)
                        {
                            r += dRow;
                            c += dCol;
                            continue;
                        }
                        // Empty or eligible marker = valid placement
                        if (cell == ' ' || cell == 'e')
                            result.Add((r, c));
                        break; // own piece or empty – stop either way
                    }
                }
            }
        }

        return [.. result];
    }

    /// <summary>
    /// Creates a deep copy of this board with the given move applied (pieces
    /// flipped) and the player switched. Used by the AI for move simulation.
    /// Does NOT call MarkEligibleMoves – AI uses GetValidMoves() directly.
    /// </summary>
    public ReversiBoard CloneWithMove(int row, int col)
    {
        int size = Size();
        var clone = new ReversiBoard
        {
            CurrentPlayerColor = CurrentPlayerColor,
            MoveRow = row,
            MoveCol = col,
        };
        clone._board = new char[size, size];
        Array.Copy(_board, clone._board, _board.Length);
        clone.UpdateBoardPieces();
        clone.SetOppositePlayerColor();
        return clone;
    }

    public char GetOppositePlayerColor() =>
        CurrentPlayerColor == "w" ? 'b' : 'w';

    public void SetOppositePlayerColor() =>
        CurrentPlayerColor = GetOppositePlayerColor().ToString();

    // ──────────────────────────────────────────────────────────────────────────
    // Factory
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Creates a fresh game board with the standard starting position.</summary>
    public static ReversiBoard NewGame()
    {
        var board = new ReversiBoard { CurrentPlayerColor = "b" };
        board._board = new char[BoardSize, BoardSize];
        for (int i = 0; i < BoardSize; i++)
            for (int j = 0; j < BoardSize; j++)
                board._board[i, j] = ' ';

        board._board[3, 3] = 'w';
        board._board[3, 4] = 'b';
        board._board[4, 3] = 'b';
        board._board[4, 4] = 'w';
        return board;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Score helpers
    // ──────────────────────────────────────────────────────────────────────────

    public int CountPieces(char color)
    {
        int size = Size();
        int count = 0;
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if (_board[i, j] == color) count++;
        return count;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private int Size() => (int)Math.Sqrt(_board.Length);

    /// <summary>
    /// From the current move position (MoveRow, MoveCol), walks in direction
    /// (dRow, dCol) collecting opponent pieces. Returns the list to flip if a
    /// current-player piece closes the bracket; returns empty otherwise.
    /// </summary>
    private List<(int Row, int Col)> LookDirection(int dRow, int dCol)
    {
        int size = Size();
        char currentColor = CurrentPlayerColor[0];
        char oppositeColor = GetOppositePlayerColor();

        int r = MoveRow + dRow;
        int c = MoveCol + dCol;

        if (r < 0 || r >= size || c < 0 || c >= size) return [];
        if (_board[r, c] == ' ' || _board[r, c] == 'e') return [];

        var candidates = new List<(int Row, int Col)>();
        while (r >= 0 && r < size && c >= 0 && c < size)
        {
            char cell = _board[r, c];
            if (cell == oppositeColor)
                candidates.Add((r, c));
            else if (cell == currentColor)
                return candidates; // bracket closed – flip everything collected
            else
                return []; // gap (empty or eligible marker) – no flip

            r += dRow;
            c += dCol;
        }
        return []; // hit boundary without closing bracket
    }
}
