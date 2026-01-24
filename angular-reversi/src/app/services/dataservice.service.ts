import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http'


@Injectable({
  providedIn: 'root'
})
export class DataserviceService {

  constructor(private http: HttpClient) { }

  debugMethod() {
      const headers = new HttpHeaders({ 'Content-Type': 'text/xml', 'Accept': 'text/xml'});
      let xmlData = `<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                      <soap:Body>
                        <Debug xmlns="http://tempuri.org/" />
                      </soap:Body>
                    </soap:Envelope>`;

    return this.http.post(`http://localhost:44329/WebService1.asmx`, xmlData, {headers: headers, responseType: 'text'});
  }

  postMoveData(currentPlayerColor: string, moveRow: any, moveCol: any, currentBoard: any) {
    const headers = new HttpHeaders({ 'Content-Type': 'text/xml', 'Accept': 'text/xml'});

    let xmlData = `<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <ReversiNextMove xmlns="http://tempuri.org/">
    <board>
      <CurrentPlayerColor>${currentPlayerColor}</CurrentPlayerColor>
      <MoveRow>${moveRow}</MoveRow>
      <MoveCol>${moveCol}</MoveCol>
      <BoardString>${currentBoard}</BoardString>
    </board>
    </ReversiNextMove>
  </soap:Body>
</soap:Envelope>`;

  console.log("Making a real request to the web service" + "\n" + xmlData);
  
  return this.http.post(`http://localhost:44329/WebService1.asmx`, xmlData, {headers: headers, responseType: 'text'});
  }


}
