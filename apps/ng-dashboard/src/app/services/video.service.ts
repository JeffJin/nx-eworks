import {Injectable} from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {HttpClient, HttpEventType, HttpHeaders, HttpParams, HttpRequest, HttpResponse} from '@angular/common/http';
import {VideoDto} from '../models/dtos';
import {environment} from '../../environments/environment';
import {mockData} from './mock-data';

@Injectable()
export class VideoService {
  constructor(private httpClient: HttpClient) {
  }

  getVideo(id: string): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.videos.find(item => item.id === id));
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/videos/${id}`);
  }

  getVideos(category: string = '', pageIndex: number = 0, pageSize: number = 12,
            orderBy = 'CreatedOn', isDescending: boolean = false): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.videos);
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
    return this.httpClient.get(`${environment.apiBaseUrl}/admin/videos`, {headers, params});
  }

  getThumbnails(id: string): Observable<any> {
    return this.httpClient.get(`${environment.apiBaseUrl}/videos/thumbnails/${id}`);
  }

  updateVideo(id: string, videoDto: VideoDto): Observable<any> {
    videoDto.serialize().update();
    return this.httpClient.put(`${environment.apiBaseUrl}/videos/${id}`, videoDto);
  }

  deleteVideo(id: string): Observable<any> {
    return this.httpClient.delete(`${environment.apiBaseUrl}/videos/${id}`);
  }

}
