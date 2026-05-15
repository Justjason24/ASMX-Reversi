import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface GameState {
  boardString: string;
  board: string[][];
  currentPlayer: string;
  gameOver: boolean;
  blackScore: number;
  whiteScore: number;
  eligibleMoves: { row: number; col: number }[];
  gameId: string;
}

export interface NewGameRequest {
  mode: 'pvp' | 'ai';
  difficulty?: 'easy' | 'medium' | 'hard';
}

export interface MoveRequest {
  gameId: string;
  boardString: string;
  currentPlayer: string;
  moveRow: number;
  moveCol: number;
}

export interface AiMoveRequest {
  gameId: string;
  boardString: string;
  currentPlayer: string;
  difficulty: string;
}

@Injectable({
  providedIn: 'root'
})
export class DataserviceService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  newGame(request: NewGameRequest): Observable<GameState> {
    return this.http.post<GameState>(`${this.apiUrl}/game/new`, request);
  }

  postMove(request: MoveRequest): Observable<GameState> {
    return this.http.post<GameState>(`${this.apiUrl}/game/move`, request);
  }

  aiMove(request: AiMoveRequest): Observable<GameState> {
    return this.http.post<GameState>(`${this.apiUrl}/game/ai-move`, request);
  }
}
