import { Injectable } from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {HttpClient, HttpEventType, HttpHeaders, HttpRequest, HttpResponse} from '@angular/common/http';
import {environment} from '../../environments/environment';

@Injectable()
export class FileService {

  constructor(private httpClient: HttpClient) { }

  uploadFile(formData: FormData): Observable<any> {
    const headers = new HttpHeaders();
    const options = {
      reportProgress: true,
      headers
    };
    const req = new HttpRequest('POST', `${environment.apiBaseUrl}/files/upload`, formData, options);
    return this.httpClient.request(req);
  }

}
