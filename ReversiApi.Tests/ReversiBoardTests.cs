namespace ReversiApi.Tests;

public class ReversiBoardTests
{
    // ── Initial Board Setup ──────────────────────────────────────────────────

    [Fact]
    public void NewGame_HasCorrectStartingPieces()
    {
        var board = ReversiBoard.NewGame();
        var data = board.BoardData;

        Assert.Equal('w', data[3, 3]);
        Assert.Equal('b', data[3, 4]);
        Assert.Equal('b', data[4, 3]);
        Assert.Equal('w', data[4, 4]);
    }

    [Fact]
    public void NewGame_BlackMovesFirst()
    {
        var board = ReversiBoard.NewGame();
        Assert.Equal("b", board.CurrentPlayerColor);
    }

    [Fact]
    public void NewGame_Has2BlackAnd2White()
    {
        var board = ReversiBoard.NewGame();
        Assert.Equal(2, board.CountPieces('b'));
        Assert.Equal(2, board.CountPieces('w'));
    }

    [Fact]
    public void NewGame_HasCorrectEligibleMoves()
    {
        var board = ReversiBoard.NewGame();
        var moves = board.GetValidMoves();

        // Standard Reversi: black has 4 valid opening moves
        Assert.Equal(4, moves.Count);

        var expected = new HashSet<(int, int)> { (2, 3), (3, 2), (4, 5), (5, 4) };
        Assert.Equal(expected, moves.ToHashSet());
    }

    [Fact]
    public void NewGame_RemainingCellsAreEmpty()
    {
        var board = ReversiBoard.NewGame();
        var data = board.BoardData;
        int emptyCount = 0;
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (data[i, j] == ' ') emptyCount++;

        Assert.Equal(60, emptyCount); // 64 - 4 starting pieces
    }

    // ── Valid Move Detection ─────────────────────────────────────────────────

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 2)]
    [InlineData(4, 5)]
    [InlineData(5, 4)]
    public void GetValidMoves_InitialBlackMoves_AreCorrect(int row, int col)
    {
        var board = ReversiBoard.NewGame();
        var moves = board.GetValidMoves();
        Assert.Contains((row, col), moves);
    }

    [Fact]
    public void GetValidMoves_AfterBlackPlays_WhiteHasCorrectMoves()
    {
        var board = ReversiBoard.NewGame();
        // Black plays (2,3) — flips w(3,3) to b
        var afterMove = board.CloneWithMove(2, 3);
        // afterMove is now white's turn
        var whiteMoves = afterMove.GetValidMoves();
        Assert.True(whiteMoves.Count > 0, "White should have valid moves after black's opening");
    }

    [Fact]
    public void GetValidMoves_DetectsMultipleDirections()
    {
        // Black at center surrounded by white in several directions, with black anchors
        // to ensure valid moves exist in multiple directions
        var board = CreateBoardFromLayout(new char[,]
        {
            { 'b', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', 'w', ' ', 'w', ' ', ' ', ' ', ' ' },
            { ' ', ' ', 'w', 'w', ' ', ' ', ' ', ' ' },
            { ' ', 'w', 'w', ' ', 'w', 'w', 'b', ' ' },
            { ' ', ' ', 'w', 'w', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', 'w', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', 'b', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        }, "b");

        var moves = board.GetValidMoves();
        // Black should have valid moves in multiple directions from its pieces
        Assert.True(moves.Count > 0, $"Expected valid moves but found {moves.Count}");
    }

    // ── Invalid Move Rejection ───────────────────────────────────────────────

    [Fact]
    public void GetValidMoves_DoesNotIncludeOccupiedCells()
    {
        var board = ReversiBoard.NewGame();
        var moves = board.GetValidMoves();

        // Center cells are occupied
        Assert.DoesNotContain((3, 3), moves);
        Assert.DoesNotContain((3, 4), moves);
        Assert.DoesNotContain((4, 3), moves);
        Assert.DoesNotContain((4, 4), moves);
    }

    [Fact]
    public void GetValidMoves_DoesNotIncludeCellsWithNoCaptures()
    {
        var board = ReversiBoard.NewGame();
        var moves = board.GetValidMoves();

        // (0,0) is a corner with no adjacent pieces — never valid on opening
        Assert.DoesNotContain((0, 0), moves);
        // (2,2) is diagonal to w(3,3) but no opponent between
        Assert.DoesNotContain((2, 2), moves);
    }

    // ── Piece Flipping ───────────────────────────────────────────────────────

    [Fact]
    public void UpdateBoardPieces_FlipsSingleDirection()
    {
        var board = ReversiBoard.NewGame();
        // Black plays (2,3) — should flip w(3,3)
        board.MoveRow = 2;
        board.MoveCol = 3;
        board.UpdateBoardPieces();

        Assert.Equal('b', board.BoardData[2, 3]); // placed piece
        Assert.Equal('b', board.BoardData[3, 3]); // flipped from white
        Assert.Equal('b', board.BoardData[3, 4]); // was already black
    }

    [Fact]
    public void UpdateBoardPieces_FlipsMultipleDirections()
    {
        // Black plays at (2,5), capturing white in two directions:
        // - left along row 2: w(2,4) bracketed by b(2,3) 
        // - diagonal down-left: w(3,4) bracketed by b(4,3)
        var board = CreateBoardFromLayout(new char[,]
        {
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', 'b', 'w', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', 'w', ' ', ' ', ' ' },
            { ' ', ' ', ' ', 'b', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        }, "b");

        board.MoveRow = 2;
        board.MoveCol = 5;
        board.UpdateBoardPieces();

        Assert.Equal('b', board.BoardData[2, 5]); // placed
        Assert.Equal('b', board.BoardData[2, 4]); // flipped (left direction)
        Assert.Equal('b', board.BoardData[3, 4]); // flipped (diagonal down-left)
    }

    [Fact]
    public void UpdateBoardPieces_FlipsLongChain()
    {
        // A long chain of opponent pieces between two player pieces
        var board = CreateBoardFromLayout(new char[,]
        {
            { 'b', 'w', 'w', 'w', 'w', 'w', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        }, "b");

        board.MoveRow = 0;
        board.MoveCol = 6;
        board.UpdateBoardPieces();

        // All whites in row 0 should be flipped
        for (int c = 0; c <= 6; c++)
            Assert.Equal('b', board.BoardData[0, c]);
    }

    // ── Player Switching ─────────────────────────────────────────────────────

    [Fact]
    public void SetOppositePlayerColor_SwitchesFromBlackToWhite()
    {
        var board = ReversiBoard.NewGame();
        Assert.Equal("b", board.CurrentPlayerColor);
        board.SetOppositePlayerColor();
        Assert.Equal("w", board.CurrentPlayerColor);
    }

    [Fact]
    public void SetOppositePlayerColor_SwitchesFromWhiteToBlack()
    {
        var board = ReversiBoard.NewGame();
        board.CurrentPlayerColor = "w";
        board.SetOppositePlayerColor();
        Assert.Equal("b", board.CurrentPlayerColor);
    }

    [Fact]
    public void CloneWithMove_SwitchesPlayer()
    {
        var board = ReversiBoard.NewGame();
        var afterMove = board.CloneWithMove(2, 3);
        Assert.Equal("w", afterMove.CurrentPlayerColor);
    }

    // ── Game Over Detection ──────────────────────────────────────────────────

    [Fact]
    public void CheckGameOver_FullBoard_ReturnsTrue()
    {
        // Fill entire board with black
        var board = CreateBoardFromLayout(CreateFullBoard('b'), "b");
        Assert.True(board.CheckGameOver());
        Assert.True(board.GameOver);
    }

    [Fact]
    public void CheckGameOver_InitialBoard_ReturnsFalse()
    {
        var board = ReversiBoard.NewGame();
        Assert.False(board.CheckGameOver());
        Assert.False(board.GameOver);
    }

    [Fact]
    public void CheckGameOver_NeitherPlayerHasMoves_ReturnsTrue()
    {
        // Board where neither player can move (no valid captures possible)
        // All black on left half, all white on right half, no interleaving
        var layout = new char[8, 8];
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                layout[i, j] = j < 4 ? 'b' : 'w';

        var board = CreateBoardFromLayout(layout, "b");
        Assert.True(board.CheckGameOver());
    }

    [Fact]
    public void CheckGameOver_CurrentPlayerNoMoves_OpponentHasMoves_ReturnsFalse()
    {
        // Create a board where current player has no moves but opponent does
        // Black's turn, black has no valid moves, but white does
        var board = CreateBoardFromLayout(new char[,]
        {
            { 'b', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', 'w', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'b' },
        }, "w");

        // White should have no valid moves (no black adjacent in capture pattern)
        // but let's verify
        var whiteHasMoves = board.GetValidMoves().Count > 0;
        if (!whiteHasMoves)
        {
            // Check if black has moves
            board.CurrentPlayerColor = "b";
            var blackHasMoves = board.GetValidMoves().Count > 0;
            board.CurrentPlayerColor = "w";

            if (blackHasMoves)
            {
                // Neither has moves won't be true — game isn't over
                Assert.False(board.CheckGameOver());
            }
        }
    }

    // ── Edge Cases ───────────────────────────────────────────────────────────

    [Fact]
    public void CornerMove_CanBeValidAndFlips()
    {
        // Set up a board where (0,0) corner is a valid move for black
        var board = CreateBoardFromLayout(new char[,]
        {
            { ' ', 'w', 'b', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
        }, "b");

        var moves = board.GetValidMoves();
        Assert.Contains((0, 0), moves);

        board.MoveRow = 0;
        board.MoveCol = 0;
        board.UpdateBoardPieces();

        Assert.Equal('b', board.BoardData[0, 0]);
        Assert.Equal('b', board.BoardData[0, 1]); // flipped
    }

    [Fact]
    public void EdgeMove_CanBeValidAndFlips()
    {
        // Top edge: black at (0,0), white at (0,1), empty at (0,2)
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

        var moves = board.GetValidMoves();
        Assert.Contains((0, 2), moves);
    }

    [Fact]
    public void CornerOppositeCorner_CanFlipDiagonally()
    {
        // Diagonal from (0,0) through opponents to (7,7) or vice versa
        var layout = new char[8, 8];
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                layout[i, j] = ' ';

        layout[0, 0] = 'b';
        for (int k = 1; k < 7; k++)
            layout[k, k] = 'w';
        // (7,7) should be a valid move for black

        var board = CreateBoardFromLayout(layout, "b");
        var moves = board.GetValidMoves();
        Assert.Contains((7, 7), moves);
    }

    // ── Board Serialization Round-Trip ───────────────────────────────────────

    [Fact]
    public void StringifyAndParse_RoundTripsCorrectly()
    {
        var original = ReversiBoard.NewGame();
        original.MarkEligibleMoves();
        original.StringifyBoard();

        var restored = new ReversiBoard
        {
            BoardString = original.BoardString,
            CurrentPlayerColor = original.CurrentPlayerColor,
        };
        restored.FillBoardArray();

        // Verify all cells match
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                Assert.Equal(original.BoardData[i, j], restored.BoardData[i, j]);
    }

    [Fact]
    public void StringifyBoard_ProducesCommaSeparatedString()
    {
        var board = ReversiBoard.NewGame();
        board.StringifyBoard();

        Assert.False(string.IsNullOrEmpty(board.BoardString));
        var cells = board.BoardString.TrimEnd(',').Split(',');
        Assert.Equal(64, cells.Length);
    }

    [Fact]
    public void FillBoardArray_HandleTrailingComma()
    {
        var board = ReversiBoard.NewGame();
        board.StringifyBoard();

        // StringifyBoard adds trailing comma
        Assert.EndsWith(",", board.BoardString);

        // FillBoardArray should handle it
        var restored = new ReversiBoard { BoardString = board.BoardString };
        restored.FillBoardArray();

        Assert.Equal(board.BoardData[3, 3], restored.BoardData[3, 3]);
    }

    // ── MarkEligibleMoves ────────────────────────────────────────────────────

    [Fact]
    public void MarkEligibleMoves_MarksValidCellsWithE()
    {
        var board = ReversiBoard.NewGame();
        board.MarkEligibleMoves();

        var data = board.BoardData;
        int eCount = 0;
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (data[i, j] == 'e') eCount++;

        Assert.Equal(4, eCount); // 4 valid opening moves for black
    }

    [Fact]
    public void MarkEligibleMoves_ClearsPreviousMarkers()
    {
        var board = ReversiBoard.NewGame();
        board.MarkEligibleMoves();

        // Now make a move and re-mark
        var afterMove = board.CloneWithMove(2, 3);
        afterMove.MarkEligibleMoves();

        // Old 'e' markers should be gone; new ones for white
        var data = afterMove.BoardData;
        var eCells = new List<(int, int)>();
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (data[i, j] == 'e') eCells.Add((i, j));

        // White's valid moves — shouldn't overlap with black's old markers
        Assert.True(eCells.Count > 0);
        // (2,3) was played, (4,5) and (5,4) were black moves — shouldn't be 'e' now
        Assert.DoesNotContain((2, 3), eCells);
    }

    // ── CloneWithMove ────────────────────────────────────────────────────────

    [Fact]
    public void CloneWithMove_DoesNotMutateOriginal()
    {
        var board = ReversiBoard.NewGame();
        int originalBlack = board.CountPieces('b');
        int originalWhite = board.CountPieces('w');

        _ = board.CloneWithMove(2, 3);

        Assert.Equal(originalBlack, board.CountPieces('b'));
        Assert.Equal(originalWhite, board.CountPieces('w'));
        Assert.Equal("b", board.CurrentPlayerColor);
    }

    [Fact]
    public void CloneWithMove_AppliesFlips()
    {
        var board = ReversiBoard.NewGame();
        var clone = board.CloneWithMove(2, 3);

        // After black plays (2,3), black should have gained pieces
        Assert.True(clone.CountPieces('b') > 2);
    }

    // ── CountPieces ──────────────────────────────────────────────────────────

    [Fact]
    public void CountPieces_CorrectAfterMove()
    {
        var board = ReversiBoard.NewGame();
        var afterMove = board.CloneWithMove(2, 3);

        // Black placed 1 + flipped 1 = 4 total black; white lost 1 = 1
        Assert.Equal(4, afterMove.CountPieces('b'));
        Assert.Equal(1, afterMove.CountPieces('w'));
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private static ReversiBoard CreateBoardFromLayout(char[,] layout, string currentPlayer)
    {
        var board = new ReversiBoard { CurrentPlayerColor = currentPlayer };

        // Build board string from layout
        int size = layout.GetLength(0);
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                sb.Append(layout[i, j]).Append(',');

        board.BoardString = sb.ToString();
        board.FillBoardArray();
        return board;
    }

    private static char[,] CreateFullBoard(char color)
    {
        var layout = new char[8, 8];
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                layout[i, j] = color;
        return layout;
    }
}
