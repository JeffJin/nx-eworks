import { Injectable } from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {HttpClient} from '@angular/common/http';
import {environment} from '@environments/environment';
import {mockData} from './mock-data';
import {map, shareReplay, tap} from 'rxjs/operators';
import {CategoryDto} from '@app/models/dtos';

@Injectable()
export class CommonService {
  private categoryCache$: Observable<Array<CategoryDto>>;

  constructor(private httpClient: HttpClient) { }

  get categories$(): Observable<Array<CategoryDto>> {
    if (!this.categoryCache$) {
      this.categoryCache$ = this.getCategories().pipe(
        shareReplay(1),
      );
    }
    return this.categoryCache$;
  }

  private getCategories(): Observable<Array<CategoryDto>> {
    if (environment.noBackend) {
      return new Observable(observer => {
        observer.next(mockData.categories);
        observer.complete();
      });
    }
    // TODO get them from local storage if timestamp is not expired
    return this.httpClient.get<CategoryDto[]>(`${environment.apiBaseUrl}/common/categories`)
      .pipe(
        shareReplay(1),
      );
  }

}
