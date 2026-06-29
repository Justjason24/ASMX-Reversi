namespace ReversiApi;

public interface IReversiAi
{
    /// <summary>
    /// Returns the best move for the current player on the given board.
    /// The board's CurrentPlayerColor identifies whose turn it is.
    /// </summary>
    (int Row, int Col) GetBestMove(ReversiBoard board, string difficulty);
}

/// <summary>
/// Minimax AI with alpha-beta pruning.
///
/// Difficulty levels:
///   easy   – depth 1, picks randomly among all valid moves
///   medium – depth 3, positional weight heuristic (corners +100, edges +10, X-squares -25)
///   hard   – depth 5, full evaluation: positional weights + piece count + mobility
/// </summary>
public sealed class MinimaxAiPlayer : IReversiAi
{
    // Positional weights for an 8×8 board.
    // Corners are extremely valuable; X-squares (diagonal to corner) are dangerous.
    private static readonly int[,] PositionalWeights = new int[8, 8]
    {
        { 100, -25,  10,  5,  5,  10, -25, 100 },
        { -25, -45,   1,  1,  1,   1, -45, -25 },
        {  10,   1,   3,  2,  2,   3,   1,  10 },
        {   5,   1,   2,  1,  1,   2,   1,   5 },
        {   5,   1,   2,  1,  1,   2,   1,   5 },
        {  10,   1,   3,  2,  2,   3,   1,  10 },
        { -25, -45,   1,  1,  1,   1, -45, -25 },
        { 100, -25,  10,  5,  5,  10, -25, 100 },
    };

    private static readonly Random Rng = new();

    public (int Row, int Col) GetBestMove(ReversiBoard board, string difficulty)
    {
        var validMoves = board.GetValidMoves();
        if (validMoves.Count == 0) return (-1, -1);

        if (difficulty == "easy")
            return validMoves[Rng.Next(validMoves.Count)];

        int depth = difficulty == "hard" ? 5 : 3;
        char aiColor = board.CurrentPlayerColor[0];

        var (_, bestMove) = Minimax(board, depth, int.MinValue, int.MaxValue, true, aiColor);
        return bestMove == (-1, -1) ? validMoves[0] : bestMove;
    }

    private (int Score, (int Row, int Col) Move) Minimax(
        ReversiBoard board, int depth, int alpha, int beta,
        bool maximizing, char aiColor)
    {
        var validMoves = board.GetValidMoves();

        if (depth == 0 || board.GameOver || validMoves.Count == 0)
            return (Evaluate(board, aiColor), (-1, -1));

        (int Row, int Col) bestMove = validMoves[0];

        if (maximizing)
        {
            int maxScore = int.MinValue;
            foreach (var move in validMoves)
            {
                var next = board.CloneWithMove(move.Row, move.Col);
                var (score, _) = Minimax(next, depth - 1, alpha, beta, false, aiColor);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, score);
                if (beta <= alpha) break; // α-β cutoff
            }
            return (maxScore, bestMove);
        }
        else
        {
            int minScore = int.MaxValue;
            foreach (var move in validMoves)
            {
                var next = board.CloneWithMove(move.Row, move.Col);
                var (score, _) = Minimax(next, depth - 1, alpha, beta, true, aiColor);
                if (score < minScore)
                {
                    minScore = score;
                    bestMove = move;
                }
                beta = Math.Min(beta, score);
                if (beta <= alpha) break; // α-β cutoff
            }
            return (minScore, bestMove);
        }
    }

    /// <summary>
    /// Evaluates the board from aiColor's perspective. Higher = better for AI.
    /// Components: positional weights, piece count, and (hard) mobility.
    /// </summary>
    private static int Evaluate(ReversiBoard board, char aiColor)
    {
        char opponentColor = aiColor == 'b' ? 'w' : 'b';
        var data = board.BoardData;
        int size = (int)Math.Sqrt(data.Length);

        int posScore = 0;
        int aiPieces = 0;
        int opponentPieces = 0;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                char cell = data[i, j];
                if (cell == aiColor)
                {
                    aiPieces++;
                    posScore += PositionalWeights[i, j];
                }
                else if (cell == opponentColor)
                {
                    opponentPieces++;
                    posScore -= PositionalWeights[i, j];
                }
            }
        }

        int pieceScore = aiPieces - opponentPieces;

        // Mobility: count valid moves for current player (whoever's turn it is)
        int mobility = board.GetValidMoves().Count;
        int mobilityScore = board.CurrentPlayerColor[0] == aiColor
            ? mobility * 5
            : -mobility * 5;

        return posScore + pieceScore * 2 + mobilityScore;
    }
}
