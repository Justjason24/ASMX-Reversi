import { Component } from '@angular/core';
import { DataserviceService, GameState, NewGameRequest, MoveRequest } from '../services/dataservice.service';

@Component({
  selector: 'app-reversi',
  templateUrl: './reversi.component.html',
  styleUrls: ['./reversi.component.css']
})
export class ReversiComponent {

  constructor(private dataService: DataserviceService) {}

  // Setup state
  gameStarted = false;
  gameMode: 'pvp' | 'ai' = 'pvp';
  difficulty: 'easy' | 'medium' | 'hard' = 'medium';

  // Game state
  gameId = '';
  boardString = '';
  activePlayerColor = 'b';
  gameOver = false;
  blackScore = 2;
  whiteScore = 2;
  aiThinking = false;
  showMoveHistory = false;

  tableData: string[][] = this.emptyBoard();
  moveHistory: { player: string; row: number; col: number }[] = [];

  private emptyBoard(): string[][] {
    return Array.from({ length: 8 }, () => Array(8).fill(' '));
  }

  private buildDisplayBoard(state: GameState): string[][] {
    const display = state.board.map(row => [...row]);
    for (const move of state.eligibleMoves) {
      display[move.row][move.col] = 'e';
    }
    return display;
  }

  private applyState(state: GameState): void {
    this.gameId = state.gameId;
    this.boardString = state.boardString;
    this.activePlayerColor = state.currentPlayer;
    this.gameOver = state.gameOver;
    this.blackScore = state.blackScore;
    this.whiteScore = state.whiteScore;
    this.tableData = this.buildDisplayBoard(state);
  }

  get hasEligibleMoves(): boolean {
    return this.tableData.some(row => row.includes('e'));
  }

  startNewGame(): void {
    const request: NewGameRequest = { mode: this.gameMode };
    if (this.gameMode === 'ai') {
      request.difficulty = this.difficulty;
    }
    this.dataService.newGame(request).subscribe(state => {
      this.applyState(state);
      this.moveHistory = [];
      this.aiThinking = false;
      this.gameStarted = true;
    });
  }

  resetGame(): void {
    this.gameStarted = false;
    this.gameOver = false;
    this.aiThinking = false;
    this.tableData = this.emptyBoard();
    this.moveHistory = [];
  }

  onCellClick(playerColor: string, rowNumber: number, columnNumber: number): void {
    if (this.aiThinking || this.gameOver) return;

    this.moveHistory.push({ player: playerColor, row: rowNumber, col: columnNumber });

    const moveRequest: MoveRequest = {
      gameId: this.gameId,
      boardString: this.boardString,
      currentPlayer: playerColor,
      moveRow: rowNumber,
      moveCol: columnNumber
    };

    this.dataService.postMove(moveRequest).subscribe(state => {
      this.applyState(state);
      if (!state.gameOver && this.gameMode === 'ai' && state.currentPlayer !== 'b') {
        this.triggerAiMove(state);
      }
    });
  }

  private triggerAiMove(state: GameState): void {
    this.aiThinking = true;
    setTimeout(() => {
      this.dataService.aiMove({
        gameId: state.gameId,
        boardString: state.boardString,
        currentPlayer: state.currentPlayer,
        difficulty: this.difficulty
      }).subscribe(aiState => {
        this.applyState(aiState);
        this.aiThinking = false;
        // If after AI move the human still can't play (AI goes again), recurse
        if (!aiState.gameOver && this.gameMode === 'ai' && aiState.currentPlayer !== 'b') {
          this.triggerAiMove(aiState);
        }
      });
    }, 600);
  }
}

