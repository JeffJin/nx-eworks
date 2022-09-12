import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpRequest} from '@angular/common/http';
import {DeviceDto} from '../models/dtos';
import {Observable} from 'rxjs/Observable';
import {environment} from '../../environments/environment';
import {mockData} from './mock-data';

@Injectable()
export class DeviceService {


  constructor(private httpClient: HttpClient) { }

  getDevices(pageIndex: number = 0, pageSize: number = 12): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.devices);
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/devices`);
  }

  getDevice(serialNumber: string): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.devices.find(item => item.serialNumber === serialNumber ));
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/devices/${serialNumber}`);
  }

  registerDevice(dto: DeviceDto): Observable<any> {
    return this.httpClient.post(`${environment.apiBaseUrl}/devices/`, dto);
  }

  updateDevice(id: string, dto: DeviceDto): Observable<any> {
    return this.httpClient.put(`${environment.apiBaseUrl}/devices/${id}`, dto);
  }

  deleteDevice(id: string) {
    return this.httpClient.delete(`${environment.apiBaseUrl}/devices/${id}`);
  }


  getDeviceStatus(serialNumber: string) {
    return this.httpClient.delete(`${environment.apiBaseUrl}/devices/status/${serialNumber}`);
  }
}
