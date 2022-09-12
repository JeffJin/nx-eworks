import { Injectable } from '@angular/core';
import {environment} from '../../environments/environment';
import {CustomerDto, GroupDto} from '../models/dtos';
import {HttpClient, HttpHeaders, HttpRequest} from '@angular/common/http';
import {Observable} from 'rxjs';
import {mockData} from "./mock-data";

@Injectable()
export class GroupService {

  constructor(private httpClient: HttpClient) { }

  addGroup(dto: any): Observable<any> {
    return this.httpClient.post(`${environment.apiBaseUrl}/groups/`, dto);
  }

  loadGroups(): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.groups);
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/groups/`);
  }

  loadGroupsWithDevices(num: number): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.groups);
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/groups/devices/${num}`);
  }

  searchGroups(keywords): Observable<any> {
    return this.httpClient.get(`${environment.apiBaseUrl}/groups/search?keywords=${keywords}`);
  }

  getGroup(id: string): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.groups.find(item => item.id === id ));
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/groups/${id}`);
  }

  updateGroup(id: string, dto: GroupDto): Observable<any> {
    return this.httpClient.put(`${environment.apiBaseUrl}/groups/${id}`, dto);
  }

  deleteGroup(id: string): Observable<any> {
    return this.httpClient.delete(`${environment.apiBaseUrl}/groups/${id}`);
  }
}
