import { Component, OnInit } from '@angular/core';
import { DataserviceService } from '../services/dataservice.service';



@Component({
  selector: 'app-reversi',
  templateUrl: './reversi.component.html',
  styleUrls: ['./reversi.component.css']
})
export class ReversiComponent implements OnInit {

  constructor(public dataService: DataserviceService) {}

  activePlayerColor: string = 'b';
  errorMessage: string = '';
  eligibleMoves = true;
  boardFull = false;

  whiteScore: number = 2;
  blackScore: number = 2;

  tableData: any[][] = [
    [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',],
    [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',],
    [' ', ' ', ' ', ' ', 'e', ' ', ' ', ' ',],
    [' ', ' ', ' ', 'b', 'w', 'e', ' ', ' ',],
    [' ', ' ', 'e', 'w', 'b', ' ', ' ', ' ',],
    [' ', ' ', ' ', 'e', ' ', ' ', ' ', ' ',],
    [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',],
    [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',]
  ];

  moveHistory: any[] = [];
  chunkedMoveHistory: string[][] = [];

  ngOnInit() {

      console.log("test area");
  }


  onCellClick(playerColor: any, rowNumber: any, columnNumber: any) {

    this.moveHistory.push(playerColor, rowNumber, columnNumber);
    this.chunkedMoveHistory = [];
    for(let i = 0; i < this.moveHistory.length; i += 3) {
      this.chunkedMoveHistory.push(this.moveHistory.slice(i, i + 3));
    }



    this.dataService.postMoveData(playerColor, rowNumber, columnNumber, this.tableData.toString() ).subscribe(
      (data) => {

        var boardString = this.getTextBetweenStrings(data, "<BoardString>", "</BoardString>");

        var boardArray = this.createBoardArrayFromString(boardString);
        this.tableData = this.updateBoard(boardArray); 
  
        this.activePlayerColor = this.getTextBetweenStrings(data, "<CurrentPlayerColor>", "</CurrentPlayerColor>");

        if(boardArray.every(x => x === 'w' || x === 'b')) {
          this.boardFull = true;
        }

        if(!boardArray.includes('e')) {
          this.eligibleMoves = false;
        } 

        if(boardArray.includes('e')) {
          this.eligibleMoves = true;
        }

        this.calculateScore();
        
      }
    );
  }


  createBoardArrayFromString(gameDataString: string) {
      var boardArray = gameDataString.split(",");
      boardArray.pop(); // remove the last element
      return boardArray;
  }


  getTextBetweenStrings(fullString: string, startString: string, endString: string) {
    const startIndex = fullString.indexOf(startString);

    const contentStartIndex = startIndex + startString.length;
    const endIndex = fullString.indexOf(endString, contentStartIndex);


    return fullString.substring(contentStartIndex, endIndex);
  }

  updateBoard(arr: string[]) {

      var numColumns = Math.sqrt(arr.length);
      const numRows = Math.ceil(arr.length / numColumns);
      var result: any[][] = [];

      for (let i = 0; i < numRows; i++) {
        const startIndex = i * numColumns;
        const endIndex = startIndex + numColumns;
        // Slice the original array to get a chunk for the current row
        result.push(arr.slice(startIndex, endIndex));
      }

      return result;

  }

  calculateScore() {
    this.whiteScore = this.tableData.flat().filter(x => x === 'w').length;
    this.blackScore = this.tableData.flat().filter(x => x === 'b').length;
  }

  debugAPICall() {
    this.dataService.debugMethod().subscribe((data) => {
    var boardString = this.getTextBetweenStrings(data, "<BoardString>", "</BoardString>");

    var boardArray = this.createBoardArrayFromString(boardString);
    this.tableData = this.updateBoard(boardArray); 
  
    this.activePlayerColor = this.getTextBetweenStrings(data, "<CurrentPlayerColor>", "</CurrentPlayerColor>");

    if(boardArray.every(x => x === 'w' || x === 'b')) {
      this.boardFull = true;
    }

        if(!boardArray.includes('e')) {
          this.eligibleMoves = false;
        } 

        if(boardArray.includes('e')) {
          this.eligibleMoves = true;
        }

        this.calculateScore();
    });
  }
  
}
