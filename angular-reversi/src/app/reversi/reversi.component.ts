import { Component, OnInit } from '@angular/core';
import { DataserviceService } from '../services/dataservice.service';



@Component({
  selector: 'app-reversi',
  templateUrl: './reversi.component.html',
  styleUrls: ['./reversi.component.css']
})
export class ReversiComponent implements OnInit {

  constructor(public dataService: DataserviceService) {}

  activePlayerColor: string = 'w';
  errorMessage: string = '';

  tableData: any[][] = [
    [' ', 'e', ' ', ' '],
    ['e', 'b', 'w', ' '],
    [' ', 'w', 'b', 'e'],
    [' ', ' ', 'e', ' ']
  ];

  moveHistory: any[] = [];
  chunkedMoveHistory: string[][] = [];

  ngOnInit() {
      // this.dataService.postDummyData().subscribe(x => {
      //   console.log(x);
      // });
      console.log("test area");
  }


  onCellClick(playerColor: any, rowNumber: any, columnNumber: any) {

    this.moveHistory.push(playerColor, rowNumber, columnNumber);
    this.chunkedMoveHistory = [];
    for(let i = 0; i < this.moveHistory.length; i += 3) {
      this.chunkedMoveHistory.push(this.moveHistory.slice(i, i + 3));
    }

    console.log(this.activePlayerColor);
    this.dataService.postMoveData(playerColor, rowNumber, columnNumber, this.tableData.toString() ).subscribe(
      (data) => {
        console.log('I got this data back! ', data); // This can be deleted soon

        var parsedText = this.getTextBetweenStrings(data, "<BoardString>", "</BoardString>");
        console.log("This is the parsed text", parsedText);
        if(parsedText?.startsWith("ERROR")){
          this.errorMessage = parsedText;
        }

        else {

          
          var boardArray = this.reallyBadBoardUpdater(parsedText);
          this.tableData = this.updateBoard(boardArray); 
    
          this.activePlayerColor = this.getTextBetweenStrings(data, "<CurrentPlayerColor>", "</CurrentPlayerColor>");
        }
      }
    );
  }


  reallyBadBoardUpdater(gameDataString: string) {
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

  // onCellDummyClick(playerColor: any, rowNumber: any, columnNumber: any) {

  //   this.dataService.postDummyData().subscribe(
  //     (data) => {
  //       console.log('I got this data back! ', data);
  //     }
  //   );

  //   console.log('Current Player Color', playerColor)
  //   console.log('Row Data:', rowNumber);
  //   console.log('Row Index:', columnNumber);
  // }
  
}
