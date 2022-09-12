import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { combineLatest } from 'rxjs';
import {HttpClient, HttpHeaders, HttpRequest} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {PlaylistDto} from '../models/dtos';
import {mockData} from './mock-data';

@Injectable()
export class PlaylistService {

  constructor(private httpClient: HttpClient) { }

  searchPlaylist(keywords: any): Observable<any> {
    return this.httpClient.get(`${environment.apiBaseUrl}/playlists/search?keywords=${keywords}`);
  }

  addPlaylist(dto: any): Observable<any> {
    return this.httpClient.post(`${environment.apiBaseUrl}/playlists/`, dto);
  }

  loadPlaylists(): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.playlists);
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/playlists/`);

  }

  deletePlaylist(id): Observable<any>{
    return this.httpClient.delete(`${environment.apiBaseUrl}/playlists/${id}`);
  }

  getPlaylist(id: string, includeMedia: boolean = false, mediaType: string = 'video'): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.playlists.find(item => item.id === id ));
        observer.complete();
      });
    }

    const result = this.httpClient.get(`${environment.apiBaseUrl}/playlists/${id}`);
    if(includeMedia){
      return Observable.create((obs) => {
        result.subscribe((playlist: PlaylistDto) => {
          const tasks: Array<Observable<any>> = [];
          const playlistItems = [];
          playlist.subPlaylists.map(sp => {
            sp.playlistItems.map(pi => {
              playlistItems.push(pi);
              tasks.push(this.httpClient.get(`${environment.apiBaseUrl}/${mediaType}/${pi.mediaAssetId}`));
            });
          });
          if(tasks.length === 0){
            obs.next(playlist);
            obs.complete();
          }
          else{
            combineLatest(tasks).subscribe((...results) => {
              playlistItems.map( pi => {
                pi.media = results[0].find(m => m.id === pi.mediaAssetId);
              });

              obs.next(playlist);
              obs.complete();
            });
          }
        });
      });
    }
    else{
      return result;
    }
  }

  updatePlaylist(dto: PlaylistDto): Observable<any> {
    return this.httpClient.put(`${environment.apiBaseUrl}/playlists/update`, dto);
  }

  publishPlaylist(id: string) {
    return this.httpClient.post(`${environment.apiBaseUrl}/playlists/publish/${id}`, null);
  }
}
