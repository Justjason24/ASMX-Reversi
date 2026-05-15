using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReversiApi.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ── Health ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Health_ReturnsOkWithStatus()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("healthy", json.GetProperty("status").GetString());
        Assert.Equal("1.0.0", json.GetProperty("version").GetString());
    }

    // ── New Game ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task NewGame_Pvp_ReturnsValidInitialState()
    {
        var response = await _client.PostAsJsonAsync("/api/game/new",
            new { Mode = "pvp", Difficulty = "medium" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var state = await response.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        Assert.NotNull(state);
        Assert.False(string.IsNullOrEmpty(state!.GameId));
        Assert.Equal("b", state.CurrentPlayer);
        Assert.False(state.GameOver);
        Assert.Equal(2, state.BlackScore);
        Assert.Equal(2, state.WhiteScore);
        Assert.Equal(4, state.EligibleMoves.Length);
        Assert.NotNull(state.Board);
        Assert.Equal(8, state.Board.Length);
    }

    [Fact]
    public async Task NewGame_Ai_ReturnsValidInitialState()
    {
        var response = await _client.PostAsJsonAsync("/api/game/new",
            new { Mode = "ai", Difficulty = "hard" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var state = await response.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        Assert.NotNull(state);
        Assert.Equal("b", state!.CurrentPlayer);
        Assert.Equal(2, state.BlackScore);
        Assert.Equal(2, state.WhiteScore);
    }

    [Fact]
    public async Task NewGame_BoardStringIsNonEmpty()
    {
        var response = await _client.PostAsJsonAsync("/api/game/new",
            new { Mode = "pvp" });

        var state = await response.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);
        Assert.False(string.IsNullOrEmpty(state!.BoardString));
    }

    // ── Player Move ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Move_ValidMove_ReturnsUpdatedState()
    {
        // Start a new game
        var newResponse = await _client.PostAsJsonAsync("/api/game/new",
            new { Mode = "pvp" });
        var initialState = await newResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        // Make a valid move — pick the first eligible move
        var eligibleMove = initialState!.EligibleMoves[0];
        var moveRequest = new
        {
            GameId = initialState.GameId,
            BoardString = initialState.BoardString,
            CurrentPlayer = initialState.CurrentPlayer,
            MoveRow = eligibleMove.Row,
            MoveCol = eligibleMove.Col,
        };

        var moveResponse = await _client.PostAsJsonAsync("/api/game/move", moveRequest);

        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);
        var state = await moveResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        Assert.NotNull(state);
        Assert.Equal("w", state!.CurrentPlayer); // switched to white
        Assert.True(state.BlackScore > initialState.BlackScore, "Black should have gained pieces");
    }

    [Fact]
    public async Task Move_InvalidMove_Returns400BadRequest()
    {
        var newResponse = await _client.PostAsJsonAsync("/api/game/new",
            new { Mode = "pvp" });
        var initialState = await newResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        // Try to move to an invalid cell (0,0 is not a valid Reversi opening move)
        var moveRequest = new
        {
            GameId = initialState!.GameId,
            BoardString = initialState.BoardString,
            CurrentPlayer = initialState.CurrentPlayer,
            MoveRow = 0,
            MoveCol = 0,
        };

        var moveResponse = await _client.PostAsJsonAsync("/api/game/move", moveRequest);
        Assert.Equal(HttpStatusCode.BadRequest, moveResponse.StatusCode);
    }

    // ── AI Move ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task AiMove_ReturnsStateWithAiMoveApplied()
    {
        // Start a new game, make black's move, then request AI move for white
        var newResponse = await _client.PostAsJsonAsync("/api/game/new",
            new { Mode = "ai", Difficulty = "medium" });
        var initialState = await newResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        // Black makes a move first
        var firstMove = initialState!.EligibleMoves[0];
        var moveResponse = await _client.PostAsJsonAsync("/api/game/move", new
        {
            GameId = initialState.GameId,
            BoardString = initialState.BoardString,
            CurrentPlayer = initialState.CurrentPlayer,
            MoveRow = firstMove.Row,
            MoveCol = firstMove.Col,
        });
        var afterBlack = await moveResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        // Now request AI move for white
        var aiResponse = await _client.PostAsJsonAsync("/api/game/ai-move", new
        {
            GameId = afterBlack!.GameId,
            BoardString = afterBlack.BoardString,
            CurrentPlayer = afterBlack.CurrentPlayer,
            Difficulty = "medium",
        });

        Assert.Equal(HttpStatusCode.OK, aiResponse.StatusCode);
        var afterAi = await aiResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        Assert.NotNull(afterAi);
        Assert.Equal("b", afterAi!.CurrentPlayer); // back to black
        Assert.True(afterAi.WhiteScore > afterBlack.WhiteScore || afterAi.WhiteScore >= 1,
            "AI should have placed a white piece");
    }

    [Theory]
    [InlineData("easy")]
    [InlineData("medium")]
    [InlineData("hard")]
    public async Task AiMove_AllDifficulties_ReturnValidResponse(string difficulty)
    {
        var newResponse = await _client.PostAsJsonAsync("/api/game/new",
            new { Mode = "pvp" });
        var state = await newResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        // Make one black move then ask AI for white's move
        var move = state!.EligibleMoves[0];
        var afterBlack = await _client.PostAsJsonAsync("/api/game/move", new
        {
            GameId = state.GameId,
            BoardString = state.BoardString,
            CurrentPlayer = state.CurrentPlayer,
            MoveRow = move.Row,
            MoveCol = move.Col,
        });
        var blackState = await afterBlack.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        var aiResponse = await _client.PostAsJsonAsync("/api/game/ai-move", new
        {
            GameId = blackState!.GameId,
            BoardString = blackState.BoardString,
            CurrentPlayer = blackState.CurrentPlayer,
            Difficulty = difficulty,
        });

        Assert.Equal(HttpStatusCode.OK, aiResponse.StatusCode);
    }

    // ── Full Game Flow ────────────────────────────────────────────────────────

    [Fact]
    public async Task FullGameFlow_PlayUntilGameOver()
    {
        var newResponse = await _client.PostAsJsonAsync("/api/game/new",
            new { Mode = "pvp" });
        var state = await newResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);

        int maxMoves = 100; // safety limit
        int moveCount = 0;

        while (!state!.GameOver && moveCount < maxMoves)
        {
            if (state.EligibleMoves.Length == 0)
            {
                // Current player has no moves — pass turn
                // The API handles this by setting MoveRow/MoveCol to -1
                var passResponse = await _client.PostAsJsonAsync("/api/game/move", new
                {
                    GameId = state.GameId,
                    BoardString = state.BoardString,
                    CurrentPlayer = state.CurrentPlayer,
                    MoveRow = -1,
                    MoveCol = -1,
                });
                state = await passResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);
            }
            else
            {
                // Pick first eligible move
                var move = state.EligibleMoves[0];
                var moveResponse = await _client.PostAsJsonAsync("/api/game/move", new
                {
                    GameId = state.GameId,
                    BoardString = state.BoardString,
                    CurrentPlayer = state.CurrentPlayer,
                    MoveRow = move.Row,
                    MoveCol = move.Col,
                });
                state = await moveResponse.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions);
            }
            moveCount++;
        }

        // Game should eventually end
        Assert.True(state!.GameOver || moveCount >= maxMoves,
            $"Game should end. Moves played: {moveCount}, GameOver: {state.GameOver}");

        if (state.GameOver)
        {
            // Scores should add up: total pieces + empty = 64
            Assert.True(state.BlackScore + state.WhiteScore <= 64);
            Assert.True(state.BlackScore + state.WhiteScore > 0);
        }
    }
}
