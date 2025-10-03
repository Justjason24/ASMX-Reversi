import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http'


@Injectable({
  providedIn: 'root'
})
export class DataserviceService {

  constructor(private http: HttpClient) { }

  postDummyData() {
      const headers = new HttpHeaders({ 'Content-Type': 'text/xml', 'Accept': 'text/xml'});

      let xmlData = `<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <AddA xmlns="http://tempuri.org/">
      <name>Jason</name>
    </AddA>
  </soap:Body>
</soap:Envelope>`;

    console.log("I'm about to make a post request to OLD BONES! ");

    return this.http.post(`https://localhost:44329/WebService1.asmx`, xmlData, {headers: headers, responseType: 'text'});
  }

  postMoveData(currentPlayerColor: string, moveRow: any, moveCol: any, currentBoard: any) {
    let xmlData = ``;
    
  }
}
