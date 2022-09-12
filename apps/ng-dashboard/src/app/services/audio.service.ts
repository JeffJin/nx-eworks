import { Injectable } from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {HttpClient, HttpEventType, HttpHeaders, HttpRequest, HttpResponse} from '@angular/common/http';
import {AudioDto} from '../models/dtos';
import {environment} from '../../environments/environment';
import {mockData} from './mock-data';

@Injectable()
export class AudioService {


  constructor(private httpClient: HttpClient) { }

  getAudios(pageIndex: number = 0, pageSize: number = 12, isPrivate: boolean = false): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.audios);
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/admin/audios`);
  }

  getAudio(id: string): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.audios.find(item => item.id === id ));
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/audios/${id}`);
  }

  updateAudio(id: string, audioDto: AudioDto): Observable<any> {
    audioDto.serialize().update();
    return this.httpClient.put(`${environment.apiBaseUrl}/audios/${id}`, audioDto);
  }

  deleteAudio(id: string): Observable<any> {
    return this.httpClient.delete(`${environment.apiBaseUrl}/audios/${id}`);
  }
}
