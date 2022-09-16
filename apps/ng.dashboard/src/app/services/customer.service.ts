import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpRequest} from '@angular/common/http';
import {CustomerDto, DeviceDto} from '../models/dtos';
import {Observable} from 'rxjs/Observable';
import {environment} from '../../environments/environment';
import {mockData} from './mock-data';

@Injectable()
export class CustomerService {


  constructor(private httpClient: HttpClient) { }

  getCustomers(pageIndex: number = 0, pageSize: number = 12): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.customers);
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/organizations`);
  }

  searchCustomers(keywords): Observable<any> {
    return this.httpClient.get(`${environment.apiBaseUrl}/organizations/search?keywords=${keywords}`);
  }

  getCustomer(id: string): Observable<any> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.customers.find(item => item.id === id ));
        observer.complete();
      });
    }
    return this.httpClient.get(`${environment.apiBaseUrl}/organizations/${id}`);
  }

  addCustomer(dto: CustomerDto): Observable<any> {
    return this.httpClient.post(`${environment.apiBaseUrl}/organizations/`, dto);
  }

  updateCustomer(id: string, dto: CustomerDto): Observable<any> {
    return this.httpClient.put(`${environment.apiBaseUrl}/organizations/${id}`, dto);
  }

  deleteCustomer(id: string): Observable<any> {
    return this.httpClient.delete(`${environment.apiBaseUrl}/organizations/${id}`);
  }
}
