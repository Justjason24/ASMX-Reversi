import { Component } from '@angular/core';
import { DataserviceService } from '../services/dataservice.service';



@Component({
  selector: 'app-reversi',
  templateUrl: './reversi.component.html',
  styleUrls: ['./reversi.component.css']
})
export class ReversiComponent {

  constructor(public dataService: DataserviceService) {}

  activePlayerColor: string = 'w';
  errorMessage: string = '';

  tableData: any[][] = [
    [' ', 'e', ' ', ' '],
    ['e', 'b', 'w', ' '],
    [' ', 'w', 'b', 'e'],
    [' ', ' ', 'e', ' ']
  ];

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

  onCellClick(playerColor: any, rowNumber: any, columnNumber: any) {

    this.dataService.postMoveData(playerColor, rowNumber, columnNumber, this.tableData.toString() ).subscribe(
      (data) => {
        console.log('I got this data back! ', data); // This can be deleted soon

        // var parsedText = this.reallyBadXMLParser(data, "<ReversiNextMoveResult>", "</ReversiNextMoveResult>")
        var parsedText = this.getTextBetweenStrings(data, "<ReversiNextMoveResult>", "</ReversiNextMoveResult>");
        console.log("This is the parsed text", parsedText);
        if(parsedText?.startsWith("ERROR")){
          this.errorMessage = parsedText;
        }

        else {

          // Clicking the top right of the board returns a row = 0 and column = 3 in a 4x4 grid
          //this.tableData[rowNumber][columnNumber] = this.activePlayerColor;

          var boardArray = this.reallyBadBoardUpdater(parsedText);
          // this.updateBoard(boardArray);

          this.tableData = this.updateBoard(boardArray);
          
          this.activePlayerColor == 'w' ? this.activePlayerColor = 'b' : this.activePlayerColor = 'w'

          // console.log(this.tableData);
          // console.log('Stringified version to send to Web Service', this.tableData.toString());

          // console.log('Current Player Color', playerColor)
          // console.log('Row Data:', rowNumber);
          // console.log('Row Index:', columnNumber);
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
  
}
