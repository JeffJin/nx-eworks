import { Injectable } from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {HttpClient, HttpEventType, HttpHeaders, HttpParams, HttpRequest, HttpResponse} from '@angular/common/http';
import {ImageDto} from '../models/dtos';
import {environment} from '../../environments/environment';
import {mockData} from './mock-data';

@Injectable()
export class ImageService {

  constructor(private httpClient: HttpClient) { }

  getImages(category: string = '', pageIndex: number = 0, pageSize: number = 12,
            orderBy = 'CreatedOn', isDescending: boolean = false): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.images);
        observer.complete();
      });
    }
    const headers = new HttpHeaders();
    headers.append('Content-Type', 'multipart/form-data');
    headers.append('Accept', 'application/json');
    const params = new HttpParams()
      .append('category', category)
      .append('pageIndex', pageIndex.toString())
      .append('pageSize', pageSize.toString())
      .append('orderBy', orderBy)
      .append('isDescending', isDescending.toString());
    return this.httpClient.get(`${environment.apiBaseUrl}/admin/images`, {headers, params});

  }

  getImage(id: string): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.images.find(item => item.id === id ));
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/images/${id}`);
  }

  updateImage(id: string, imageDto: ImageDto): Observable<any> {
    return this.httpClient.put(`${environment.apiBaseUrl}/images/${id}`, imageDto);
  }

  deleteImage(id: string): Observable<any>  {
    return this.httpClient.delete(`${environment.apiBaseUrl}/images/${id}`);
  }

}
