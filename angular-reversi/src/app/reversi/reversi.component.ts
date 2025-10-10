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

        var parsedText = this.reallyBadXMLParser(data, "<ReversiNextMoveResult>", "</ReversiNextMoveResult>")
        if(parsedText?.startsWith("ERROR")){
          this.errorMessage = parsedText;
        }

        else {

          // Clicking the top right of the board returns a row = 0 and column = 3 in a 4x4 grid
          this.tableData[rowNumber][columnNumber] = this.activePlayerColor;
          
          this.activePlayerColor == 'w' ? this.activePlayerColor = 'b' : this.activePlayerColor = 'w'

          console.log(this.tableData);
          console.log('Stringified version to send to Web Service', this.tableData.toString());

          console.log('Current Player Color', playerColor)
          console.log('Row Data:', rowNumber);
          console.log('Row Index:', columnNumber);
        }

      }
    );


  }



  reallyBadXMLParser(str: string, start: string, end: string) {
    const regex = new RegExp(`${start}(.*?)${end}`);
    const match = str.match(regex);
    return match ? match[1] : null;
  }
  
}
